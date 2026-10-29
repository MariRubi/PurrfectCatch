using PurrfectCatch.Audio;
using PurrfectCatch.Core;
using PurrfectCatch.Kitchen;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PurrfectCatch.Menus
{
    /// <summary>
    /// Пауза на кухні (Esc): RESUME, SETTINGS, TO CAFE (дострокове завершення зміни), MAIN MENU.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private ShiftManager shift;
        [SerializeField] private GameObject panel;
        [SerializeField] private SettingsMenu settings;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button toCafeButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button closeButton;

        public bool IsPaused => panel.activeSelf;

        private void Awake()
        {
            resumeButton.onClick.AddListener(Resume);
            closeButton.onClick.AddListener(Resume);
            settingsButton.onClick.AddListener(settings.Open);
            toCafeButton.onClick.AddListener(() => LeaveShift(GameManager.CafeScene));
            mainMenuButton.onClick.AddListener(() => LeaveShift(GameManager.MainMenuScene));
            panel.SetActive(false);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;

            if (settings.IsOpen) settings.Close();
            else if (IsPaused) Resume();
            else if (!shift.HasEnded) Pause();
        }

        public void Pause()
        {
            AudioManager.PlaySfx(Sfx.Click);
            panel.SetActive(true);
            Time.timeScale = 0f; // зупиняє таймер, рух предметів і спавн
        }

        public void Resume()
        {
            AudioManager.PlaySfx(Sfx.Click);
            if (settings.IsOpen) settings.gameObject.SetActive(false);
            panel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void LeaveShift(string sceneName)
        {
            AudioManager.PlaySfx(Sfx.Click);
            Time.timeScale = 1f;
            shift.EndShift(ShiftEndReason.LeftEarly); // зароблене за зміну не згорає
            GameManager.Instance.LoadScene(sceneName);
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f; // про всяк випадок, щоб гра не лишилась «замороженою»
        }
    }
}
