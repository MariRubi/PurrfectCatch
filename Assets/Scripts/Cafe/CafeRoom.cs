using System.Collections;
using System.Collections.Generic;
using PurrfectCatch.Audio;
using PurrfectCatch.Core;
using PurrfectCatch.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace PurrfectCatch.Cafe
{
    /// <summary>
    /// Зала кафе (ГДД 7.2): показує куплені меблі та котиків, що прийшли.
    /// Клік по котику: муркотіння й підскок.
    /// </summary>
    public class CafeRoom : MonoBehaviour
    {
        [System.Serializable]
        public class FurnitureSlot
        {
            public FurnitureData furniture;
            public SpriteRenderer visual;
        }

        [System.Serializable]
        public class CatSlot
        {
            public CatData cat;
            public SpriteRenderer visual;
        }

        [SerializeField] private List<FurnitureSlot> furnitureSlots = new List<FurnitureSlot>();
        [SerializeField] private List<CatSlot> catSlots = new List<CatSlot>();

        private Camera cam;
        private readonly HashSet<Transform> hopping = new HashSet<Transform>();

        private void Start()
        {
            cam = Camera.main;
            ApplyArt();
            Refresh();
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null) GameManager.Instance.FurnitureBought += OnFurnitureBought;
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null) GameManager.Instance.FurnitureBought -= OnFurnitureBought;
        }

        private void OnFurnitureBought(FurnitureData furniture)
        {
            Refresh();
            // Новий котик підстрибує, щоб гравець помітив його
            foreach (var slot in catSlots)
                if (slot.cat != null && slot.cat.requiredFurniture == furniture && slot.visual != null)
                    TryHop(slot.visual.transform);
        }

        /// <summary>Якщо в ScriptableObject уже є спрайт, він підставляється замість заглушки.</summary>
        private void ApplyArt()
        {
            foreach (var slot in furnitureSlots)
                if (slot.visual != null && slot.furniture != null && slot.furniture.sprite != null)
                {
                    slot.visual.sprite = slot.furniture.sprite;
                    slot.visual.color = Color.white;
                }

            foreach (var slot in catSlots)
                if (slot.visual != null && slot.cat != null && slot.cat.sprite != null)
                {
                    slot.visual.sprite = slot.cat.sprite;
                    slot.visual.color = Color.white;
                }
        }

        private void Refresh()
        {
            var game = GameManager.Instance;
            foreach (var slot in furnitureSlots)
                if (slot.visual != null)
                    slot.visual.gameObject.SetActive(game.OwnsFurniture(slot.furniture));

            foreach (var slot in catSlots)
                if (slot.visual != null)
                    slot.visual.gameObject.SetActive(game.IsCatUnlocked(slot.cat));
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null || cam == null || !mouse.leftButton.wasPressedThisFrame) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Vector2 worldPoint = cam.ScreenToWorldPoint(mouse.position.ReadValue());
            var hit = Physics2D.OverlapPoint(worldPoint);
            if (hit == null) return;

            foreach (var slot in catSlots)
            {
                if (slot.visual != null && hit.gameObject == slot.visual.gameObject)
                {
                    AudioManager.PlaySfx(Sfx.Purr);
                    TryHop(slot.visual.transform);
                    break;
                }
            }
        }

        private void TryHop(Transform target)
        {
            if (hopping.Contains(target)) return;
            StartCoroutine(Hop(target));
        }

        private IEnumerator Hop(Transform target)
        {
            hopping.Add(target);
            Vector3 start = target.localPosition;
            const float duration = 0.3f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                target.localPosition = start + Vector3.up * (Mathf.Sin(t / duration * Mathf.PI) * 0.4f);
                yield return null;
            }
            target.localPosition = start;
            hopping.Remove(target);
        }
    }
}
