using System.Collections;
using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Puerta o barrera que se abre/cierra suavemente cuando una PressurePlate o palanca la activa.
    /// </summary>
    public class InteractiveDoor : MonoBehaviour
    {
        [Header("Movimiento")]
        [Tooltip("Hacia dónde se desplaza la puerta al abrirse (relativo)")]
        public Vector3 openOffset = new Vector3(0, 3f, 0);

        [Tooltip("Velocidad de apertura/cierre")]
        public float speed = 3f;

        private Vector3 closedPos;
        private Vector3 targetPos;
        private Coroutine moveCoroutine;

        private void Awake()
        {
            closedPos = transform.position;
            targetPos = closedPos;
        }

        public void OpenDoor()
        {
            SetTarget(closedPos + openOffset);
        }

        public void CloseDoor()
        {
            SetTarget(closedPos);
        }

        private void SetTarget(Vector3 newTarget)
        {
            targetPos = newTarget;
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(MoveToTarget());
        }

        private IEnumerator MoveToTarget()
        {
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPos;
        }
    }
}
