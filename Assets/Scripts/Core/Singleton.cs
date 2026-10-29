using UnityEngine;

namespace PurrfectCatch.Core
{
    /// <summary>
    /// Базовий клас для менеджерів-одинаків (GameManager, AudioManager, SaveManager).
    /// Живе між сценами (DontDestroyOnLoad). Якщо з'явиться дублікат, він знищується.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = (T)this;
            if (transform.parent != null) transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            OnInitialize();
        }

        /// <summary>Викликається один раз, коли цей екземпляр став головним.</summary>
        protected virtual void OnInitialize() { }

        protected virtual void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
