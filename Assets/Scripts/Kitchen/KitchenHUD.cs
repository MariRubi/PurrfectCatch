using System.Collections;
using PurrfectCatch.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PurrfectCatch.Kitchen
{
    /// <summary>
    /// HUD кухні (ГДД 8.1): сердечка, таймер, комбо, заробіток за зміну + вікно підсумків.
    /// </summary>
    public class KitchenHUD : MonoBehaviour
    {
        [SerializeField] private ShiftManager shift;

        [Header("Верхня панель")]
        [SerializeField] private Image[] hearts;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text comboText;
        [SerializeField] private TMP_Text earningsText;

        [Header("Підсумки зміни")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private Button resultToCafeButton;

        private int lastMultiplier = 1;
        private Coroutine comboPunch;

        private void Awake()
        {
            resultPanel.SetActive(false);
            resultToCafeButton.onClick.AddListener(() => GameManager.Instance.LoadScene(GameManager.CafeScene));
        }

        private void OnEnable()
        {
            shift.LivesChanged += OnLivesChanged;
            shift.TimeChanged += OnTimeChanged;
            shift.ComboChanged += OnComboChanged;
            shift.EarningsChanged += OnEarningsChanged;
            shift.ShiftEnded += OnShiftEnded;
        }

        private void OnDisable()
        {
            shift.LivesChanged -= OnLivesChanged;
            shift.TimeChanged -= OnTimeChanged;
            shift.ComboChanged -= OnComboChanged;
            shift.EarningsChanged -= OnEarningsChanged;
            shift.ShiftEnded -= OnShiftEnded;
        }

        private void OnLivesChanged(int lives)
        {
            for (int i = 0; i < hearts.Length; i++)
                if (hearts[i] != null) hearts[i].enabled = i < lives;
        }

        private void OnTimeChanged(float secondsLeft)
        {
            int s = Mathf.CeilToInt(secondsLeft);
            timerText.text = $"{s / 60:00}:{s % 60:00}";
        }

        private void OnComboChanged(int multiplier, int streak)
        {
            comboText.gameObject.SetActive(multiplier > 1);
            comboText.text = $"x{multiplier} combo";

            if (multiplier > lastMultiplier && isActiveAndEnabled)
            {
                if (comboPunch != null) StopCoroutine(comboPunch);
                comboPunch = StartCoroutine(Punch(comboText.transform));
            }
            lastMultiplier = multiplier;
        }

        private void OnEarningsChanged(long earned)
        {
            earningsText.text = earned.ToString();
        }

        private void OnShiftEnded(ShiftResult r)
        {
            // Якщо гравець вийшов через паузу, одразу вантажиться інша сцена, тож вікно не потрібне.
            if (r.reason == ShiftEndReason.LeftEarly) return;

            string title = r.reason == ShiftEndReason.TimeUp ? "Shift complete!" : "Too many mistakes!";
            string tips = r.tips > 0 ? $"\nPerfect shift tips: +{r.tips}" : "";
            resultText.text =
                $"<size=130%><b>{title}</b></size>\n\n" +
                $"Earned: {r.earned}{tips}\n" +
                $"<b>Total: {r.Total}</b>\n\n" +
                $"Correct: {r.correct}   Mistakes: {r.mistakes}\n" +
                $"Best streak: {r.bestStreak}";
            resultPanel.SetActive(true);
        }

        private static IEnumerator Punch(Transform target)
        {
            const float duration = 0.25f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                target.localScale = Vector3.one * (1f + Mathf.Sin(t / duration * Mathf.PI) * 0.35f);
                yield return null;
            }
            target.localScale = Vector3.one;
        }
    }
}
