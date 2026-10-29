using PurrfectCatch.Audio;
using PurrfectCatch.Core;
using PurrfectCatch.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PurrfectCatch.Cafe
{
    /// <summary>Один рядок магазину: іконка, назва, опис, кнопка купівлі.</summary>
    public class ShopItemView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Button buyButton;
        [SerializeField] private TMP_Text buyButtonText;

        private FurnitureData furniture;

        public void Bind(FurnitureData data)
        {
            furniture = data;
            nameText.text = data.displayName;
            descriptionText.text = data.description;

            if (data.sprite != null)
            {
                icon.sprite = data.sprite;
                icon.color = Color.white;
            }
            else
            {
                icon.color = data.placeholderColor;
            }

            buyButton.onClick.AddListener(Buy);
            Refresh();
        }

        public void Refresh()
        {
            if (furniture == null) return;
            var game = GameManager.Instance;

            if (game.OwnsFurniture(furniture))
            {
                buyButtonText.text = "Owned";
                buyButton.interactable = false;
            }
            else
            {
                buyButtonText.text = $"Buy: {furniture.price}";
                buyButton.interactable = game.Coins >= furniture.price;
            }
        }

        private void Buy()
        {
            if (GameManager.Instance.TryBuyFurniture(furniture))
                AudioManager.PlaySfx(Sfx.Coin);
        }
    }
}
