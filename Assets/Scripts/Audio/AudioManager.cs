using System.Collections.Generic;
using PurrfectCatch.Core;
using UnityEngine;

namespace PurrfectCatch.Audio
{
    /// <summary>Усі звукові ефекти гри (ГДД 8.3). Кліпи призначаються у префабі Resources/Managers.</summary>
    public enum Sfx
    {
        Click,    // натискання кнопки
        Plate,    // «плюх» у тарілку
        Trash,    // брязкіт відра
        Mistake,  // ображене нявчання
        ComboUp,  // підвищення комбо
        Coin,     // дзвін монет / каса
        Purr      // муркотіння
    }

    /// <summary>
    /// Музика та звуки + три повзунки гучності (master / music / sfx), що зберігаються в PlayerPrefs.
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        [System.Serializable]
        public class SfxEntry
        {
            public Sfx id;
            [Tooltip("Якщо кліпів кілька, щоразу грає випадковий")]
            public AudioClip[] clips;
        }

        [SerializeField] private List<SfxEntry> soundEffects = new List<SfxEntry>();
        [SerializeField, Range(0f, 1f)] private float defaultVolume = 0.8f;
        [SerializeField, Range(0f, 0.3f)] private float pitchVariation = 0.08f;

        private const string MasterKey = "volume_master";
        private const string MusicKey = "volume_music";
        private const string SfxKey = "volume_sfx";

        private AudioSource musicSource;
        private AudioSource sfxSource;

        public float MasterVolume { get; private set; }
        public float MusicVolume { get; private set; }
        public float SfxVolume { get; private set; }

        protected override void OnInitialize()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            MasterVolume = PlayerPrefs.GetFloat(MasterKey, defaultVolume);
            MusicVolume = PlayerPrefs.GetFloat(MusicKey, defaultVolume);
            SfxVolume = PlayerPrefs.GetFloat(SfxKey, defaultVolume);
            ApplyVolumes();
        }

        public void SetMasterVolume(float value)
        {
            MasterVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MasterKey, MasterVolume);
            ApplyVolumes();
        }

        public void SetMusicVolume(float value)
        {
            MusicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicKey, MusicVolume);
            ApplyVolumes();
        }

        public void SetSfxVolume(float value)
        {
            SfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxKey, SfxVolume);
            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            AudioListener.volume = MasterVolume;
            if (musicSource != null) musicSource.volume = MusicVolume;
            if (sfxSource != null) sfxSource.volume = SfxVolume;
        }

        public void Play(Sfx id)
        {
            var entry = soundEffects.Find(e => e.id == id);
            if (entry == null || entry.clips == null || entry.clips.Length == 0) return;

            var clip = entry.clips[UnityEngine.Random.Range(0, entry.clips.Length)];
            if (clip == null) return;

            sfxSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            sfxSource.PlayOneShot(clip);
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return;
            musicSource.clip = clip;
            musicSource.Play();
        }

        /// <summary>Короткий виклик з будь-якого місця: AudioManager.PlaySfx(Sfx.Coin);</summary>
        public static void PlaySfx(Sfx id)
        {
            if (Instance != null) Instance.Play(id);
        }
    }
}
