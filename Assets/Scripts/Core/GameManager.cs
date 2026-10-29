using System;
using PurrfectCatch.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PurrfectCatch.Core
{
    /// <summary>
    /// Головний менеджер: монети (Purrcoins), пасивний дохід від котиків,
    /// покупка меблів, перехід між сценами.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        public const string MainMenuScene = "MainMenu";
        public const string CafeScene = "Cafe";
        public const string KitchenScene = "Kitchen";

        /// <summary>Баланс змінився (новий баланс).</summary>
        public event Action<long> CoinsChanged;

        /// <summary>Куплено нові меблі.</summary>
        public event Action<FurnitureData> FurnitureBought;

        private GameDatabase database;
        private float incomeTimer;

        public GameDatabase Database
        {
            get
            {
                if (database == null)
                {
                    database = Resources.Load<GameDatabase>("GameDatabase");
                    if (database == null)
                    {
                        Debug.LogWarning("[GameManager] Немає Resources/GameDatabase. " +
                                         "Запусти меню Purrfect Catch → 1. Create Starter Data.");
                        database = ScriptableObject.CreateInstance<GameDatabase>();
                    }
                }
                return database;
            }
        }

        private SaveData Save => SaveManager.Instance.Data;

        public long Coins => Save.coins;

        /// <summary>Пасивний дохід нараховується всюди, крім головного меню.</summary>
        public bool PassiveIncomeActive => SceneManager.GetActiveScene().name != MainMenuScene;

        /// <summary>Сума бонусів усіх відкритих котиків (ГДД 3.1).</summary>
        public int CoinsPerSecond
        {
            get
            {
                int total = 0;
                foreach (var cat in Database.cats)
                    if (cat != null && IsCatUnlocked(cat))
                        total += cat.coinsPerSecond;
                return total;
            }
        }

        public bool IsCatUnlocked(CatData cat)
        {
            return cat != null && (cat.requiredFurniture == null || OwnsFurniture(cat.requiredFurniture));
        }

        public bool OwnsFurniture(FurnitureData furniture)
        {
            return furniture != null && Save.ownedFurniture.Contains(furniture.id);
        }

        public void AddCoins(long amount)
        {
            if (amount <= 0) return;
            Save.coins += amount;
            CoinsChanged?.Invoke(Save.coins);
        }

        public bool TrySpend(long amount)
        {
            if (amount < 0 || Save.coins < amount) return false;
            Save.coins -= amount;
            CoinsChanged?.Invoke(Save.coins);
            return true;
        }

        public bool TryBuyFurniture(FurnitureData furniture)
        {
            if (furniture == null || OwnsFurniture(furniture)) return false;
            if (Save.coins < furniture.price) return false;

            // Спершу позначаємо покупку, потім списуємо гроші,
            // щоб підписники CoinsChanged уже бачили меблі купленими.
            Save.ownedFurniture.Add(furniture.id);
            TrySpend(furniture.price);
            SaveManager.Instance.Save();
            FurnitureBought?.Invoke(furniture);
            return true;
        }

        /// <summary>Зараховує виручку зміни і оновлює рекорди.</summary>
        public void RegisterShiftResult(ShiftResult result)
        {
            AddCoins(result.Total);
            Save.shiftsPlayed++;
            if (result.bestStreak > Save.bestStreak) Save.bestStreak = result.bestStreak;
            if (result.Total > Save.bestShiftEarnings) Save.bestShiftEarnings = result.Total;
            SaveManager.Instance.Save();
        }

        private void Update()
        {
            if (!PassiveIncomeActive) return;

            // Time.deltaTime = 0 під час паузи, тож на паузі дохід не йде.
            incomeTimer += Time.deltaTime;
            if (incomeTimer >= 1f)
            {
                int seconds = (int)incomeTimer;
                incomeTimer -= seconds;
                AddCoins((long)CoinsPerSecond * seconds);
            }
        }

        public void LoadScene(string sceneName)
        {
            Time.timeScale = 1f;
            SaveManager.Instance.Save();
            SceneManager.LoadScene(sceneName);
        }

        public void QuitGame()
        {
            SaveManager.Instance.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
