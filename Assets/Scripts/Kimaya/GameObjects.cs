using System.Collections;
using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Objeto que Falin puede empujar. Requiere Rigidbody2D y Collider2D.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PushableObject : MonoBehaviour
    {
        [Header("Física de empuje")]
        public float pushForce    = 3.5f;
        public float friction     = 5f;
        public bool  canBeStacked = true;

        private Rigidbody2D _rb;
        private bool _beingPushed = false;
        private float _pushTimer = 0f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale   = 3f;
            _rb.freezeRotation = true;
            _rb.constraints    = RigidbodyConstraints2D.FreezeRotation;
        }

        public void Push(Vector2 direction)
        {
            _beingPushed = true;
            _pushTimer   = 0.1f;
            _rb.linearVelocity = new Vector2(direction.x * pushForce, _rb.linearVelocity.y);
        }

        private void FixedUpdate()
        {
            if (_pushTimer > 0f) { _pushTimer -= Time.fixedDeltaTime; return; }

            // Fricción cuando no se empuja
            if (Mathf.Abs(_rb.linearVelocity.x) > 0.05f)
                _rb.linearVelocity = new Vector2(
                    Mathf.MoveTowards(_rb.linearVelocity.x, 0f, friction * Time.fixedDeltaTime),
                    _rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// Túnel estrecho — solo Mia puede pasar.
    /// Falin es bloqueada por un collider invisible.
    /// </summary>
    public class SmallTunnel : MonoBehaviour
    {
        [Tooltip("Si está activo, Falin choca con este collider.")]
        public Collider2D falinBlocker;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var mia = other.GetComponent<MiaAbilities>();
            if (mia != null) mia.EnterTunnel();

            // Si es Falin, asegurarse de que el blocker esté activo
            var falinCtrl = other.GetComponent<FalinAbilities>();
            if (falinCtrl != null && falinBlocker != null)
                falinBlocker.enabled = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var mia = other.GetComponent<MiaAbilities>();
            if (mia != null) mia.ExitTunnel();
        }
    }

    /// <summary>
    /// Camino oculto que Mia revela con su olfato.
    /// Se vuelve visible/semitransparente al activarse.
    /// </summary>
    public class HiddenPath : MonoBehaviour
    {
        [Header("Visual")]
        public Color hiddenColor   = new Color(0.3f, 0.8f, 0.4f, 0f);   // invisible
        public Color revealedColor = new Color(0.4f, 1f, 0.5f, 0.75f);  // semitransparente

        [Header("Colisión")]
        public bool enableColliderWhenRevealed = true;

        private SpriteRenderer _sr;
        private Collider2D     _col;
        private Coroutine      _hideRoutine;

        private void Awake()
        {
            _sr  = GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = GetComponentInChildren<SpriteRenderer>();
            _col = GetComponent<Collider2D>();

            if (_sr != null) _sr.color = hiddenColor;
            if (_col != null && !enableColliderWhenRevealed) _col.enabled = false;
        }

        public void Reveal(float duration)
        {
            if (_hideRoutine != null) StopCoroutine(_hideRoutine);
            _hideRoutine = StartCoroutine(RevealRoutine(duration));
        }

        private IEnumerator RevealRoutine(float duration)
        {
            // Fade in
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                if (_sr != null)
                    _sr.color = Color.Lerp(hiddenColor, revealedColor, t / 0.5f);
                yield return null;
            }
            if (_col != null) _col.enabled = true;

            yield return new WaitForSeconds(duration - 1f);

            // Fade out
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                if (_sr != null)
                    _sr.color = Color.Lerp(revealedColor, hiddenColor, t);
                yield return null;
            }
            if (_col != null && !enableColliderWhenRevealed) _col.enabled = false;
        }
    }
}
