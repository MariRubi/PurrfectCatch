using PurrfectCatch.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace PurrfectCatch.Menus
{
    /// <summary>Вікно налаштувань (ГДД 8.4): три повзунки гучності та повноекранний режим.</summary>
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button closeButton;

        public bool IsOpen => gameObject.activeSelf;

        private void Awake()
        {
            masterSlider.onValueChanged.AddListener(v => { if (AudioManager.Instance) AudioManager.Instance.SetMasterVolume(v); });
            musicSlider.onValueChanged.AddListener(v => { if (AudioManager.Instance) AudioManager.Instance.SetMusicVolume(v); });
            sfxSlider.onValueChanged.AddListener(v => { if (AudioManager.Instance) AudioManager.Instance.SetSfxVolume(v); });
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            closeButton.onClick.AddListener(Close);
        }

        private void OnEnable()
        {
            var audio = AudioManager.Instance;
            if (audio != null)
            {
                masterSlider.SetValueWithoutNotify(audio.MasterVolume);
                musicSlider.SetValueWithoutNotify(audio.MusicVolume);
                sfxSlider.SetValueWithoutNotify(audio.SfxVolume);
            }
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
        }

        private static void SetFullscreen(bool fullscreen)
        {
            Screen.fullScreenMode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
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
