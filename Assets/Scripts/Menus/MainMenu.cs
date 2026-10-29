using PurrfectCatch.Audio;
using PurrfectCatch.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PurrfectCatch.Menus
{
    /// <summary>Головне меню (ГДД 3.4): PLAY → зала кафе, SETTINGS, EXIT.</summary>
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private SettingsMenu settings;

        private void Awake()
        {
            playButton.onClick.AddListener(() =>
            {
                AudioManager.PlaySfx(Sfx.Click);
                GameManager.Instance.LoadScene(GameManager.CafeScene);
            });
            settingsButton.onClick.AddListener(settings.Open);
            exitButton.onClick.AddListener(() => GameManager.Instance.QuitGame());
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && settings.IsOpen)
                settings.Close();
        }
    }
}
