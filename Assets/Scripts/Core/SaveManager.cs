using System;
using System.IO;
using UnityEngine;

namespace PurrfectCatch.Core
{
    /// <summary>
    /// Збереження та завантаження прогресу у JSON-файл.
    /// Файл лежить у Application.persistentDataPath/save.json
    /// (на Windows: C:\Users\ІМ'Я\AppData\LocalLow\DefaultCompany\PurrfectCatch).
    /// </summary>
    public class SaveManager : Singleton<SaveManager>
    {
        private const string FileName = "save.json";

        private SaveData data;

        public SaveData Data
        {
            get
            {
                if (data == null) Load();
                return data;
            }
        }

        private string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        protected override void OnInitialize()
        {
            Load();
        }

        public void Load()
        {
            data = null;
            try
            {
                if (File.Exists(SavePath))
                    data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Не вдалося прочитати збереження: {e.Message}");
            }

            if (data == null) data = new SaveData();
        }

        public void Save()
        {
            if (data == null) return;
            try
            {
                // Спершу пишемо у тимчасовий файл, щоб не зіпсувати збереження, якщо гра впаде посеред запису.
                string tempPath = SavePath + ".tmp";
                File.WriteAllText(tempPath, JsonUtility.ToJson(data, true));
                if (File.Exists(SavePath)) File.Delete(SavePath);
                File.Move(tempPath, SavePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Не вдалося зберегти гру: {e.Message}");
            }
        }

        /// <summary>Стерти прогрес (зручно для тестування).</summary>
        public void ResetProgress()
        {
            data = new SaveData();
            Save();
        }

        private void OnApplicationQuit() => Save();

        private void OnApplicationPause(bool paused)
        {
            if (paused) Save();
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Purrfect Catch/Стерти збереження")]
        private static void DeleteSaveFromEditor()
        {
            string path = Path.Combine(Application.persistentDataPath, FileName);
            if (File.Exists(path)) File.Delete(path);
            if (Instance != null) Instance.ResetProgress();
            Debug.Log("[SaveManager] Збереження видалено.");
        }
#endif
    }
}
