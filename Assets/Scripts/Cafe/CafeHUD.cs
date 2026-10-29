using PurrfectCatch.Audio;
using PurrfectCatch.Core;
using PurrfectCatch.Data;
using PurrfectCatch.Menus;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PurrfectCatch.Cafe
{
    /// <summary>
    /// HUD зали кафе (ГДД 8.1): баланс, пасивний дохід, кнопки «магазин», «колекція», «почати зміну».
    /// </summary>
    public class CafeHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text incomeText;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button guestBookButton;
        [SerializeField] private Button startShiftButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;

        [SerializeField] private ShopPanel shop;
        [SerializeField] private GuestBookPanel guestBook;
        [SerializeField] private SettingsMenu settings;

        private void Awake()
        {
            shopButton.onClick.AddListener(shop.Open);
            guestBookButton.onClick.AddListener(guestBook.Open);
            settingsButton.onClick.AddListener(settings.Open);
            startShiftButton.onClick.AddListener(() =>
            {
                AudioManager.PlaySfx(Sfx.Click);
                GameManager.Instance.LoadScene(GameManager.KitchenScene);
            });
            mainMenuButton.onClick.AddListener(() =>
            {
                AudioManager.PlaySfx(Sfx.Click);
                GameManager.Instance.LoadScene(GameManager.MainMenuScene);
            });
        }

        private void OnEnable()
        {
            GameManager.Instance.CoinsChanged += OnCoinsChanged;
            GameManager.Instance.FurnitureBought += OnFurnitureBought;
            OnCoinsChanged(GameManager.Instance.Coins);
            UpdateIncome();
        }

        private void OnDisable()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.CoinsChanged -= OnCoinsChanged;
            GameManager.Instance.FurnitureBought -= OnFurnitureBought;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;

            if (settings.IsOpen) settings.Close();
            else if (shop.IsOpen) shop.Close();
            else if (guestBook.IsOpen) guestBook.Close();
        }

        private void OnCoinsChanged(long coins) => coinsText.text = coins.ToString();

        private void OnFurnitureBought(FurnitureData _) => UpdateIncome();

        private void UpdateIncome() => incomeText.text = $"+{GameManager.Instance.CoinsPerSecond} coins/sec";
    }
}
