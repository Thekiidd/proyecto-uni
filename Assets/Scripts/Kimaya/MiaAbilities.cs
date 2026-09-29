using System.Collections;
using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Habilidades especiales de Mia (Schnauzer):
    ///   - Olfato: presiona Q para revelar caminos ocultos (HiddenPath)
    ///   - Espacios estrechos: si entra en SmallTunnel, encoge su collider
    /// </summary>
    [RequireComponent(typeof(PlayerController2D))]
    public class MiaAbilities : MonoBehaviour
    {
        [Header("Olfato")]
        public KeyCode smellKey        = KeyCode.Q;
        public float   smellRadius     = 6f;
        public float   smellDuration   = 3f;
        public float   smellCooldown   = 5f;
        public LayerMask hiddenLayer;

        [Header("Espacio estrecho")]
        public Vector2 normalColliderSize  = new Vector2(0.6f, 0.9f);
        public Vector2 squeezeColliderSize = new Vector2(0.4f, 0.55f);
        public float   squeezeSpeedMult   = 0.65f;

        [Header("Partículas & Visual")]
        public ParticleSystem smellParticles;
        public Color smellGlowColor = new Color(0.5f, 1f, 0.5f, 0.8f);

        private PlayerController2D _pc;
        private CapsuleCollider2D _col;
        private bool _smellReady = true;
        private float _cooldownTimer = 0f;
        private bool _inTunnel = false;

        private void Awake()
        {
            _pc  = GetComponent<PlayerController2D>();
            _col = GetComponent<CapsuleCollider2D>();
            if (_col == null) _col = GetComponentInChildren<CapsuleCollider2D>();
        }

        private void Update()
        {
            if (!_pc.isActive) return;

            // Cooldown del olfato
            if (!_smellReady)
            {
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer <= 0f) _smellReady = true;
            }

            // Activar olfato
            if (Input.GetKeyDown(smellKey) && _smellReady)
                StartCoroutine(ActivateSmell());

            // Ajustar velocidad en túneles
            if (_inTunnel)
                _pc.moveSpeed = 3f * squeezeSpeedMult;
        }

        private IEnumerator ActivateSmell()
        {
            _smellReady    = false;
            _cooldownTimer = smellCooldown;

            if (smellParticles != null) smellParticles.Play();

            // Revelar todos los HiddenPath en radio
            var hiddenPaths = Physics2D.OverlapCircleAll(
                transform.position, smellRadius, hiddenLayer);
            foreach (var h in hiddenPaths)
            {
                var hp = h.GetComponent<HiddenPath>();
                hp?.Reveal(smellDuration);
            }

            yield return new WaitForSeconds(smellDuration);

            if (smellParticles != null) smellParticles.Stop();
        }

        // Llamado por SmallTunnel trigger
        public void EnterTunnel()
        {
            _inTunnel = true;
            if (_col != null) _col.size = squeezeColliderSize;
        }

        public void ExitTunnel()
        {
            _inTunnel = false;
            if (_col != null) _col.size = normalColliderSize;
            _pc.moveSpeed = 5f; // restaurar velocidad normal
        }

        public bool SmellReady => _smellReady;
        public float SmellCooldownNormalized => _smellReady ? 1f : (1f - _cooldownTimer / smellCooldown);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 1f, 0.4f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, smellRadius);
        }
    }
}
