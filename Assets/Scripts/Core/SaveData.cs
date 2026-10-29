using System;
using System.Collections.Generic;

namespace PurrfectCatch.Core
{
    /// <summary>
    /// Усе, що зберігається між запусками гри (ГДД 9.3: JSON у Application.persistentDataPath).
    /// Налаштування звуку зберігаються окремо, у PlayerPrefs (див. AudioManager).
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public long coins;
        public List<string> ownedFurniture = new List<string>();

        // Статистика та рекорди
        public int shiftsPlayed;
        public int bestStreak;
        public long bestShiftEarnings;
    }

    public enum ShiftEndReason
    {
        TimeUp,      // таймер дійшов до нуля
        OutOfLives,  // втрачено всі 3 життя
        LeftEarly    // гравець вийшов через меню паузи
    }

    /// <summary>Підсумки однієї робочої зміни на кухні.</summary>
    [Serializable]
    public struct ShiftResult
    {
        public ShiftEndReason reason;
        public long earned;   // монети за подані страви (ціна * комбо)
        public long tips;     // чайові за бездоганну зміну
        public int correct;
        public int mistakes;
        public int bestStreak;

        public long Total => earned + tips;
    }
}
