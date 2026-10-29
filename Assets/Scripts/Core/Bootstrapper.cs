using PurrfectCatch.Audio;
using UnityEngine;

namespace PurrfectCatch.Core
{
    /// <summary>
    /// Автоматично створює менеджери перед завантаженням будь-якої сцени,
    /// тож можна натискати Play у будь-якій сцені (Kitchen, Cafe...) і все працюватиме.
    /// Якщо існує префаб Resources/Managers, береться він (там налаштовані звуки).
    /// </summary>
    public static class Bootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateManagers()
        {
            if (GameManager.Instance != null) return;

            var prefab = Resources.Load<GameObject>("Managers");
            GameObject managers;
            if (prefab != null)
            {
                managers = Object.Instantiate(prefab);
            }
            else
            {
                managers = new GameObject();
                managers.AddComponent<SaveManager>();
                managers.AddComponent<AudioManager>();
                managers.AddComponent<GameManager>();
            }
            managers.name = "[Managers]";
        }
    }
}
