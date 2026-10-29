using System.Collections;
using UnityEngine;

namespace PurrfectCatch.Kitchen
{
    public enum SortSide
    {
        Trash, // ліворуч: відро (A / ←)
        Plate  // праворуч: тарілка (D / →)
    }

    /// <summary>
    /// Зона сортування: відро або тарілка. Потрібен Collider2D, щоб по ній можна було клікнути мишкою.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SortZone : MonoBehaviour
    {
        [SerializeField] private SortSide side;
        [Tooltip("Куди летить предмет. Порожньо = центр об'єкта.")]
        [SerializeField] private Transform dropPoint;

        private Vector3 baseScale;
        private Coroutine bounceRoutine;

        public SortSide Side => side;
        public Vector3 DropPosition => dropPoint != null ? dropPoint.position : transform.position;

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        /// <summary>Пружний «підскок», коли предмет влучає в зону.</summary>
        public void Bounce()
        {
            if (!isActiveAndEnabled) return;
            if (bounceRoutine != null) StopCoroutine(bounceRoutine);
            bounceRoutine = StartCoroutine(BounceRoutine());
        }

        private IEnumerator BounceRoutine()
        {
            const float duration = 0.2f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = 1f + Mathf.Sin(t / duration * Mathf.PI) * 0.12f;
                transform.localScale = baseScale * k;
                yield return null;
            }
            transform.localScale = baseScale;
            bounceRoutine = null;
        }
    }
}
