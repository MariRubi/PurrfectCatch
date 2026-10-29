using UnityEngine;

namespace PurrfectCatch.Audio
{
    /// <summary>Поклади в сцену і признач кліп, щоб у цій сцені грала своя музика.</summary>
    public class SceneMusic : MonoBehaviour
    {
        [SerializeField] private AudioClip music;

        private void Start()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic(music);
        }
    }
}
