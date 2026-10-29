using PurrfectCatch.Core;
using PurrfectCatch.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PurrfectCatch.Cafe
{
    /// <summary>Картка котика у «Книзі гостей».</summary>
    public class CatCardView : MonoBehaviour
    {
        [SerializeField] private Image portrait;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text bodyText;

        private CatData cat;

        public void Bind(CatData data)
        {
            cat = data;
            Refresh();
        }

        public void Refresh()
        {
            if (cat == null) return;
            bool unlocked = GameManager.Instance.IsCatUnlocked(cat);

            if (cat.sprite != null) portrait.sprite = cat.sprite;
            Color baseColor = cat.sprite != null ? Color.white : cat.placeholderColor;
            portrait.color = unlocked ? baseColor : new Color(0.15f, 0.1f, 0.1f, 1f); // силует

            if (unlocked)
            {
                nameText.text = $"{cat.displayName} <size=70%>({cat.breed})</size>";
                bodyText.text = $"{cat.personality}\n<i>{cat.guestBookNote}</i>\n<b>+{cat.coinsPerSecond} coins/sec</b>";
            }
            else
            {
                string hint = cat.requiredFurniture != null ? cat.requiredFurniture.displayName : "?";
                nameText.text = "???";
                bodyText.text = $"Buy \"{hint}\" to invite this guest.";
            }
        }
    }
}
