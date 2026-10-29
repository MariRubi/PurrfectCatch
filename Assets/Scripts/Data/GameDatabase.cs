using System.Collections.Generic;
using UnityEngine;

namespace PurrfectCatch.Data
{
    /// <summary>
    /// Список усіх предметів, котиків і меблів гри.
    /// Має лежати саме в Assets/Resources/GameDatabase.asset: GameManager завантажує його звідти.
    /// </summary>
    [CreateAssetMenu(menuName = "Purrfect Catch/Game Database", fileName = "GameDatabase")]
    public class GameDatabase : ScriptableObject
    {
        public List<ItemData> items = new List<ItemData>();
        public List<CatData> cats = new List<CatData>();
        public List<FurnitureData> furniture = new List<FurnitureData>();
    }
}
