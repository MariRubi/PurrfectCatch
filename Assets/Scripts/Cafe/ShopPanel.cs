using System.Collections.Generic;
using PurrfectCatch.Audio;
using PurrfectCatch.Core;
using PurrfectCatch.Data;
using UnityEngine;
using UnityEngine.UI;

namespace PurrfectCatch.Cafe
{
    /// <summary>Магазин меблів. Рядки створюються автоматично з GameDatabase.furniture.</summary>
    public class ShopPanel : MonoBehaviour
    {
        [SerializeField] private Transform listRoot;
        [SerializeField] private ShopItemView itemViewPrefab;
        [SerializeField] private Button closeButton;

        private readonly List<ShopItemView> views = new List<ShopItemView>();

        public bool IsOpen => gameObject.activeSelf;

        private void Awake()
        {
            closeButton.onClick.AddListener(Close);
            foreach (var furniture in GameManager.Instance.Database.furniture)
            {
                if (furniture == null) continue;
                var view = Instantiate(itemViewPrefab, listRoot);
                view.Bind(furniture);
                views.Add(view);
            }
        }

        private void OnEnable()
        {
            GameManager.Instance.CoinsChanged += OnCoinsChanged;
            GameManager.Instance.FurnitureBought += OnFurnitureBought;
            RefreshAll();
        }

        private void OnDisable()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.CoinsChanged -= OnCoinsChanged;
            GameManager.Instance.FurnitureBought -= OnFurnitureBought;
        }

        private void OnCoinsChanged(long _) => RefreshAll();
        private void OnFurnitureBought(FurnitureData _) => RefreshAll();

        private void RefreshAll()
        {
            foreach (var view in views) view.Refresh();
        }

        public void Open()
        {
            AudioManager.PlaySfx(Sfx.Click);
            gameObject.SetActive(true);
        }

        public void Close()
        {
            AudioManager.PlaySfx(Sfx.Click);
            gameObject.SetActive(false);
        }
    }
}
