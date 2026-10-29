using UnityEngine;

namespace PurrfectCatch.Data
{
    /// <summary>
    /// Продукт або сміття, що з'являється на кухонній дошці (ГДД 3.3).
    /// Створення: ПКМ у Project → Create → Purrfect Catch → Item.
    /// </summary>
    [CreateAssetMenu(menuName = "Purrfect Catch/Item", fileName = "Item_")]
    public class ItemData : ScriptableObject
    {
        public string displayName;
        public Sprite sprite;

        [Tooltip("true: їжа, її треба на тарілку (D / →). false: сміття, у відро (A / ←).")]
        public bool isEdible = true;

        [Tooltip("Базова ціна. За правильне сортування дається ціна * поточне комбо.")]
        [Min(0)] public int baseValue = 2;

        [Tooltip("Як часто з'являється відносно інших предметів.")]
        [Min(0f)] public float spawnWeight = 1f;

        [Tooltip("Колір квадратика-заглушки, поки немає спрайта.")]
        public Color placeholderColor = Color.white;
    }
}
