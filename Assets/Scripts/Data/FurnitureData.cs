using UnityEngine;

namespace PurrfectCatch.Data
{
    /// <summary>Предмет інтер'єру з магазину (лежанки, будиночки, дряпки).</summary>
    [CreateAssetMenu(menuName = "Purrfect Catch/Furniture", fileName = "Furniture_")]
    public class FurnitureData : ScriptableObject
    {
        [Tooltip("Унікальний ключ для збереження. Не змінюй після релізу, бо загубляться покупки.")]
        public string id;
        public string displayName;
        [TextArea(2, 4)] public string description;
        [Min(0)] public int price = 100;
        public Sprite sprite;
        public Color placeholderColor = Color.white;
    }
}
