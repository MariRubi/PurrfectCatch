using System.Collections.Generic;
using PurrfectCatch.Audio;
using PurrfectCatch.Core;
using PurrfectCatch.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

namespace PurrfectCatch.Kitchen
{
    /// <summary>
    /// Серце активної фази: робоча зміна на кухні (ГДД 3.1, 3.3, 7.1).
    /// Предмети їдуть зверху вниз по дошці; гравець сортує НИЖНІЙ (найближчий) предмет:
    /// D / → / клік по тарілці: на тарілку (їжа), A / ← / клік по відру: у відро (сміття).
    /// Якщо предмет доїхав до низу несортованим, це теж помилка.
    /// </summary>
    public class ShiftManager : MonoBehaviour
    {
        [Header("Правила зміни (ГДД 3.1)")]
        [SerializeField] private float shiftDuration = 60f;
        [SerializeField] private int maxLives = 3;
        [Tooltip("Скільки правильних дій поспіль потрібно для наступного рівня комбо.")]
        [SerializeField] private int correctActionsPerComboLevel = 5;
        [SerializeField] private int[] comboMultipliers = { 1, 2, 4, 8 };
        [Tooltip("Чайові за зміну без жодної помилки, у % від виручки.")]
        [SerializeField, Range(0, 100)] private int perfectShiftTipPercent = 25;

        [Header("Темп конвеєра (росте протягом зміни)")]
        [SerializeField] private float startFallSpeed = 1.6f;
        [SerializeField] private float endFallSpeed = 4f;
        [SerializeField] private float startSpawnInterval = 1.3f;
        [SerializeField] private float endSpawnInterval = 0.55f;
        [Tooltip("Як складність зростає від 0 (початок зміни) до 1 (кінець).")]
        [SerializeField] private AnimationCurve difficultyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float startDelay = 1f;
        [SerializeField] private float flyDuration = 0.25f;

        [Header("Посилання на об'єкти сцени")]
        [SerializeField] private KitchenItem itemPrefab;
        [SerializeField] private Transform spawnPoint;
        [Tooltip("Лінія внизу дошки: предмет, що її перетнув, вважається пропущеним.")]
        [SerializeField] private Transform missLine;
        [SerializeField] private SortZone trashZone;
        [SerializeField] private SortZone plateZone;
        [Tooltip("Заглушка для предметів без спрайта.")]
        [SerializeField] private Sprite placeholderSprite;

        // Події для HUD
        public event System.Action<int> LivesChanged;              // життя, що лишилися
        public event System.Action<float> TimeChanged;             // секунд до кінця
        public event System.Action<int, int> ComboChanged;         // множник, серія
        public event System.Action<long> EarningsChanged;          // зароблено за зміну
        public event System.Action<SortSide, bool> ItemSorted;     // куди і чи правильно
        public event System.Action<ShiftResult> ShiftEnded;

        public bool IsRunning { get; private set; }
        public bool HasEnded { get; private set; }
        public int Lives => lives;
        public float TimeLeft => Mathf.Max(0f, shiftDuration - elapsed);
        public int Multiplier => comboMultipliers.Length > 0 ? comboMultipliers[comboLevel] : 1;

        // Предмети на дошці від найнижчого (поточного) до найвищого
        private readonly List<KitchenItem> activeItems = new List<KitchenItem>();
        private readonly List<ItemData> catalog = new List<ItemData>();
        private ObjectPool<KitchenItem> pool;
        private Camera cam;

        private float elapsed;
        private float spawnTimer;
        private int lives;
        private int streak;
        private int bestStreak;
        private int comboLevel;
        private int correctCount;
        private int mistakeCount;
        private long earned;

        private void Awake()
        {
            pool = new ObjectPool<KitchenItem>(
                createFunc: () => Instantiate(itemPrefab, transform),
                actionOnGet: item => item.gameObject.SetActive(true),
                actionOnRelease: item => item.gameObject.SetActive(false),
                actionOnDestroy: item => Destroy(item.gameObject),
                collectionCheck: false,
                defaultCapacity: 10,
                maxSize: 40);
        }

        private void Start()
        {
            cam = Camera.main;

            foreach (var item in GameManager.Instance.Database.items)
                if (item != null && item.spawnWeight > 0f)
                    catalog.Add(item);

            if (catalog.Count == 0 || itemPrefab == null || spawnPoint == null || missLine == null)
            {
                Debug.LogError("[ShiftManager] Не налаштовано: потрібні предмети в GameDatabase, " +
                               "itemPrefab, spawnPoint і missLine.");
                enabled = false;
                return;
            }

            lives = maxLives;
            RaiseAllEvents();
            Invoke(nameof(BeginShift), startDelay);
        }

        private void BeginShift()
        {
            if (HasEnded) return;
            IsRunning = true;
            SpawnItem();
        }

        private void Update()
        {
            if (!IsRunning || Time.timeScale == 0f) return;

            HandleInput();
            if (!IsRunning) return; // зміна могла закінчитися від помилки

            elapsed += Time.deltaTime;
            TimeChanged?.Invoke(TimeLeft);
            if (elapsed >= shiftDuration)
            {
                EndShift(ShiftEndReason.TimeUp);
                return;
            }

            float difficulty = difficultyCurve.Evaluate(Mathf.Clamp01(elapsed / shiftDuration));
            float speed = Mathf.Lerp(startFallSpeed, endFallSpeed, difficulty);
            float interval = Mathf.Lerp(startSpawnInterval, endSpawnInterval, difficulty);

            spawnTimer += Time.deltaTime;
            if (spawnTimer >= interval)
            {
                spawnTimer = 0f;
                SpawnItem();
            }

            float distance = speed * Time.deltaTime;
            foreach (var item in activeItems) item.MoveDown(distance);

            if (activeItems.Count > 0 && activeItems[0].transform.position.y <= missLine.position.y)
            {
                var missed = activeItems[0];
                activeItems.RemoveAt(0);
                missed.Drop(0.3f);
                RegisterMistake();
            }
        }

        private void HandleInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
                {
                    Sort(SortSide.Plate);
                    return;
                }
                if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
                {
                    Sort(SortSide.Trash);
                    return;
                }
            }

            var mouse = Mouse.current;
            if (mouse != null && cam != null && mouse.leftButton.wasPressedThisFrame)
            {
                Vector2 worldPoint = cam.ScreenToWorldPoint(mouse.position.ReadValue());
                var hit = Physics2D.OverlapPoint(worldPoint);
                if (hit != null && hit.TryGetComponent(out SortZone zone))
                    Sort(zone.Side);
            }
        }

        private void Sort(SortSide side)
        {
            if (activeItems.Count == 0) return;

            var item = activeItems[0];
            activeItems.RemoveAt(0);

            bool isCorrect = item.Data.isEdible == (side == SortSide.Plate);
            var zone = side == SortSide.Plate ? plateZone : trashZone;
            Vector3 target = zone != null ? zone.DropPosition : item.transform.position;
            item.FlyTo(target, flyDuration, () => { if (zone != null) zone.Bounce(); });

            if (isCorrect)
            {
                AudioManager.PlaySfx(side == SortSide.Plate ? Sfx.Plate : Sfx.Trash);
                RegisterCorrect(item.Data);
            }
            else
            {
                RegisterMistake();
            }

            ItemSorted?.Invoke(side, isCorrect);
        }

        private void RegisterCorrect(ItemData data)
        {
            correctCount++;
            streak++;
            if (streak > bestStreak) bestStreak = streak;

            int newLevel = Mathf.Min(streak / Mathf.Max(1, correctActionsPerComboLevel), comboMultipliers.Length - 1);
            if (newLevel > comboLevel)
            {
                comboLevel = newLevel;
                AudioManager.PlaySfx(Sfx.ComboUp);
            }
            ComboChanged?.Invoke(Multiplier, streak);

            long gain = (long)data.baseValue * Multiplier;
            if (gain > 0)
            {
                earned += gain;
                EarningsChanged?.Invoke(earned);
            }
        }

        private void RegisterMistake()
        {
            mistakeCount++;
            streak = 0;
            comboLevel = 0;
            ComboChanged?.Invoke(Multiplier, streak);

            lives--;
            LivesChanged?.Invoke(lives);
            AudioManager.PlaySfx(Sfx.Mistake);

            if (lives <= 0) EndShift(ShiftEndReason.OutOfLives);
        }

        private void SpawnItem()
        {
            var item = pool.Get();
            item.Setup(PickRandomItem(), spawnPoint.position, placeholderSprite, ReleaseItem);
            activeItems.Add(item);
        }

        private void ReleaseItem(KitchenItem item) => pool.Release(item);

        private ItemData PickRandomItem()
        {
            float total = 0f;
            foreach (var item in catalog) total += item.spawnWeight;

            float roll = Random.Range(0f, total);
            foreach (var item in catalog)
            {
                roll -= item.spawnWeight;
                if (roll <= 0f) return item;
            }
            return catalog[catalog.Count - 1];
        }

        /// <summary>Завершує зміну та зараховує виручку. Викликається і з меню паузи.</summary>
        public void EndShift(ShiftEndReason reason)
        {
            if (HasEnded) return;
            HasEnded = true;
            IsRunning = false;
            CancelInvoke();

            foreach (var item in activeItems) pool.Release(item);
            activeItems.Clear();

            bool perfect = reason == ShiftEndReason.TimeUp && mistakeCount == 0;
            var result = new ShiftResult
            {
                reason = reason,
                earned = earned,
                tips = perfect ? earned * perfectShiftTipPercent / 100 : 0,
                correct = correctCount,
                mistakes = mistakeCount,
                bestStreak = bestStreak
            };

            GameManager.Instance.RegisterShiftResult(result);
            if (result.Total > 0) AudioManager.PlaySfx(Sfx.Coin);
            ShiftEnded?.Invoke(result);
        }

        private void RaiseAllEvents()
        {
            LivesChanged?.Invoke(lives);
            TimeChanged?.Invoke(TimeLeft);
            ComboChanged?.Invoke(Multiplier, streak);
            EarningsChanged?.Invoke(earned);
        }

        private void OnValidate()
        {
            if (comboMultipliers == null || comboMultipliers.Length == 0) comboMultipliers = new[] { 1 };
            maxLives = Mathf.Max(1, maxLives);
            shiftDuration = Mathf.Max(1f, shiftDuration);
        }
    }
}
