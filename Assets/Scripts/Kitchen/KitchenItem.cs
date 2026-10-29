using System.Collections;
using PurrfectCatch.Data;
using UnityEngine;

namespace PurrfectCatch.Kitchen
{
    /// <summary>
    /// Один предмет на конвеєрі. Береться з пулу (Object Pooling), їде вниз,
    /// після сортування летить у відро чи тарілку і повертається в пул.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class KitchenItem : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private Vector3 baseScale;
        private System.Action<KitchenItem> release;

        public ItemData Data { get; private set; }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseScale = transform.localScale;
        }

        public void Setup(ItemData data, Vector3 position, Sprite placeholder, System.Action<KitchenItem> releaseToPool)
        {
            Data = data;
            release = releaseToPool;

            transform.position = position;
            transform.rotation = Quaternion.identity;
            transform.localScale = baseScale;

            bool hasArt = data.sprite != null;
            spriteRenderer.sprite = hasArt ? data.sprite : placeholder;
            spriteRenderer.color = hasArt ? Color.white : data.placeholderColor;
        }

        public void MoveDown(float distance)
        {
            transform.position += Vector3.down * distance;
        }

        /// <summary>Політ дугою до цілі; після приземлення викликає onArrive і повертається в пул.</summary>
        public void FlyTo(Vector3 target, float duration, System.Action onArrive)
        {
            StopAllCoroutines();
            StartCoroutine(FlyRoutine(target, duration, onArrive));
        }

        /// <summary>Пропущений предмет: тьмяніє й падає, потім повертається в пул.</summary>
        public void Drop(float duration)
        {
            StopAllCoroutines();
            StartCoroutine(DropRoutine(duration));
        }

        private IEnumerator FlyRoutine(Vector3 target, float duration, System.Action onArrive)
        {
            Vector3 start = transform.position;
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / duration);
                float eased = 1f - (1f - t) * (1f - t);
                Vector3 p = Vector3.Lerp(start, target, eased);
                p.y += Mathf.Sin(t * Mathf.PI) * 1.2f; // дуга
                transform.position = p;
                transform.localScale = baseScale * Mathf.Lerp(1f, 0.6f, t);
                transform.Rotate(0f, 0f, -540f * Time.deltaTime);
                yield return null;
            }
            onArrive?.Invoke();
            release?.Invoke(this);
        }

        private IEnumerator DropRoutine(float duration)
        {
            Color startColor = spriteRenderer.color;
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / duration);
                transform.position += Vector3.down * (3f * Time.deltaTime);
                var c = startColor;
                c.a = 1f - t;
                spriteRenderer.color = c;
                yield return null;
            }
            release?.Invoke(this);
        }
    }
}
