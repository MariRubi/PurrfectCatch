using UnityEngine;

namespace PurrfectCatch.Data
{
    /// <summary>Котик-гість кафе (ГДД 6.2).</summary>
    [CreateAssetMenu(menuName = "Purrfect Catch/Cat", fileName = "Cat_")]
    public class CatData : ScriptableObject
    {
        public string id;
        public string displayName;
        public string breed;
        public Sprite sprite;

        [TextArea(2, 4)] public string personality;

        [Tooltip("Гумористична замітка для «Книги гостей» (ГДД 4.3).")]
        [TextArea(3, 6)] public string guestBookNote;

        [Tooltip("Пасивний дохід, монет за секунду.")]
        [Min(0)] public int coinsPerSecond = 1;

        [Tooltip("Які меблі треба купити, щоб котик прийшов. Порожньо = доступний зі старту.")]
        public FurnitureData requiredFurniture;

        public Color placeholderColor = Color.white;
    }
}
