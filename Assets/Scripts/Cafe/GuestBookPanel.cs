using System.Collections.Generic;
using PurrfectCatch.Audio;
using PurrfectCatch.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PurrfectCatch.Cafe
{
    /// <summary>«Книга гостей кафе» (ГДД 4.3): картки котиків, закриті показуються як «???».</summary>
    public class GuestBookPanel : MonoBehaviour
    {
        [SerializeField] private Transform listRoot;
        [SerializeField] private CatCardView cardPrefab;
        [SerializeField] private Button closeButton;

        private readonly List<CatCardView> cards = new List<CatCardView>();

        public bool IsOpen => gameObject.activeSelf;

        private void Awake()
        {
            closeButton.onClick.AddListener(Close);
            foreach (var cat in GameManager.Instance.Database.cats)
            {
                if (cat == null) continue;
                var card = Instantiate(cardPrefab, listRoot);
                card.Bind(cat);
                cards.Add(card);
            }
        }

        private void OnEnable()
        {
            foreach (var card in cards) card.Refresh();
        }

        public void Open()
        {
            AudioManager.PlaySfx(Sfx.Click);
            gameObject.SetActive(true);
        }

        public void Close()
        {
            AudioManager.PlaySfx(Sfx.Click);
            gameObject.SetActive(false);
        }
    }
}
