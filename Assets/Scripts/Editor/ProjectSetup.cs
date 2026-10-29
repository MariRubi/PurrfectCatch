using System.Collections.Generic;
using System.IO;
using PurrfectCatch.Audio;
using PurrfectCatch.Cafe;
using PurrfectCatch.Core;
using PurrfectCatch.Data;
using PurrfectCatch.Kitchen;
using PurrfectCatch.Menus;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PurrfectCatch.EditorTools
{
    /// <summary>
    /// Меню «Purrfect Catch» у верхній панелі Unity.
    /// 1. Create Starter Data: створює ScriptableObject-и з ГДД (продукти, сміття, котики, меблі),
    ///    GameDatabase та префаб менеджерів. Наявні ассети НЕ перезаписує.
    /// 2. Build All Scenes: збирає сцени MainMenu, Cafe, Kitchen із кольоровими заглушками,
    ///    щоб гру можна було запустити ще до появи арту.
    /// </summary>
    public static class ProjectSetup
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string DataFolder = "Assets/ScriptableObjects";
        private const string PlaceholderFolder = "Assets/Sprites/Placeholders";
        private const string PrefabFolder = "Assets/Prefabs";
        private const string DatabasePath = "Assets/Resources/GameDatabase.asset";
        private const string ManagersPath = "Assets/Resources/Managers.prefab";

        private static readonly Color PanelBrown = Hex("8C5E4E");
        private static readonly Color ButtonBeige = Hex("E8D2B0");
        private static readonly Color RowCream = Hex("F7EAD8");
        private static readonly Color TextDark = Hex("4A2E25");
        private static readonly Color TextLight = Hex("FFF6EC");
        private static readonly Color Dim = new Color(0f, 0f, 0f, 0.55f);

        private static Sprite rounded;
        private static Sprite circle;

        // ───────────────────────────── 1. Дані ─────────────────────────────

        [MenuItem("Purrfect Catch/1. Create Starter Data", priority = 1)]
        public static void CreateStarterData()
        {
            EnsureFolder(DataFolder + "/Items");
            EnsureFolder(DataFolder + "/Cats");
            EnsureFolder(DataFolder + "/Furniture");
            EnsureFolder("Assets/Resources");

            // Продукти (на тарілку) і сміття (у відро), ГДД 3.3
            var items = new List<ItemData>
            {
                Item("Fish", "Риба", true, 2, 1.2f, Hex("F2A65A")),
                Item("TunaSteak", "Стейк тунця", true, 5, 0.8f, Hex("E06C75")),
                Item("Shrimp", "Креветка", true, 4, 0.9f, Hex("F7A8C4")),
                Item("PremiumPate", "Преміум-паштет", true, 8, 0.4f, Hex("B07CE0")),
                Item("FishBone", "Риб'ячий кістяк", false, 1, 0.8f, Hex("8E969D")),
                Item("OldBoot", "Порваний черевик", false, 1, 0.6f, Hex("5A4636")),
                Item("CanLid", "Консервна кришка", false, 1, 0.7f, Hex("B8C0C8")),
                Item("Bone", "Кісточка", false, 1, 0.6f, Hex("6E7B6A")),
            };

            // Меблі з магазину (ціни можна міняти в інспекторі)
            var basket = Furniture("WickerBasket", "Плетений кошик",
                "Затишний кошик для найсонніших гостей.", 300, Hex("D4A373"));
            var sofa = Furniture("RoyalSofa", "Королівський диванчик",
                "Оксамит, золоті ніжки, жодних компромісів.", 2500, Hex("9D4EDD"));
            var lighthouse = Furniture("LighthouseScratcher", "Дряпка-маяк",
                "Висока дряпка з видом на затоку.", 12000, Hex("E76F51"));

            // Котики, ГДД 6.2 (замітки для Книги гостей — чернетки, переписуй як хочеш)
            var cats = new List<CatData>
            {
                Cat("Barsik", "Барсик", "Рудий смугастик",
                    "Дворовий веселун, їсть абсолютно все, завжди радий першим зайняти будь-яку коробку.",
                    "Прийшов на запах риби першого ж дня. Відтоді вважає кафе своєю коробкою.",
                    1, null, Hex("F4A261")),
                Cat("Misyats", "Місяць", "Сонний сірий британець",
                    "Філософ, який спить по 20 годин на добу на найм'якшій подушці.",
                    "Побачив плетений кошик через вікно і вирішив, що це знак долі.",
                    5, basket, Hex("8D99AE")),
                Cat("Princesa", "Принцеса", "Вибаглива перська",
                    "Пухнаста дама вищого світу, визнає тільки добірні креветки та оксамитові дивани.",
                    "Оглянула диванчик, пирхнула і лишилась. Назавжди.",
                    15, sofa, Hex("F1E3D3")),
                Cat("CaptainVusach", "Капітан Вусач", "Мейн-кун",
                    "Старий кіт-моряк, який полюбляє слухати шурхіт хвиль біля вікна кафе.",
                    "Списався на берег, щойно побачив маяк. Каже, що звідти видно всю затоку.",
                    40, lighthouse, Hex("6D4C41")),
            };

            var db = LoadOrCreate<GameDatabase>(DatabasePath, out _);
            AddUnique(db.items, items);
            AddUnique(db.cats, cats);
            AddUnique(db.furniture, new[] { basket, sofa, lighthouse });
            EditorUtility.SetDirty(db);

            CreateManagersPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Purrfect Catch] Стартові дані готові: Assets/ScriptableObjects і Assets/Resources.");
        }

        private static void CreateManagersPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ManagersPath) != null) return;

            var go = new GameObject("Managers");
            go.AddComponent<SaveManager>();
            var audio = go.AddComponent<AudioManager>();
            go.AddComponent<GameManager>();

            // Одразу заповнюємо список звуків усіма типами, лишається тільки накидати кліпи.
            var so = new SerializedObject(audio);
            var list = so.FindProperty("soundEffects");
            var ids = System.Enum.GetValues(typeof(Sfx));
            list.arraySize = ids.Length;
            for (int i = 0; i < ids.Length; i++)
                list.GetArrayElementAtIndex(i).FindPropertyRelative("id").enumValueIndex = i;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(go, ManagersPath);
            Object.DestroyImmediate(go);
        }

        private static ItemData Item(string file, string name, bool edible, int value, float weight, Color color)
        {
            var a = LoadOrCreate<ItemData>($"{DataFolder}/Items/Item_{file}.asset", out bool created);
            if (created)
            {
                a.displayName = name;
                a.isEdible = edible;
                a.baseValue = value;
                a.spawnWeight = weight;
                a.placeholderColor = color;
                EditorUtility.SetDirty(a);
            }
            return a;
        }

        private static FurnitureData Furniture(string id, string name, string description, int price, Color color)
        {
            var a = LoadOrCreate<FurnitureData>($"{DataFolder}/Furniture/Furniture_{id}.asset", out bool created);
            if (created)
            {
                a.id = id;
                a.displayName = name;
                a.description = description;
                a.price = price;
                a.placeholderColor = color;
                EditorUtility.SetDirty(a);
            }
            return a;
        }

        private static CatData Cat(string id, string name, string breed, string personality, string note,
                                   int cps, FurnitureData required, Color color)
        {
            var a = LoadOrCreate<CatData>($"{DataFolder}/Cats/Cat_{id}.asset", out bool created);
            if (created)
            {
                a.id = id;
                a.displayName = name;
                a.breed = breed;
                a.personality = personality;
                a.guestBookNote = note;
                a.coinsPerSecond = cps;
                a.requiredFurniture = required;
                a.placeholderColor = color;
                EditorUtility.SetDirty(a);
            }
            return a;
        }

        // ───────────────────────────── 2. Сцени ─────────────────────────────

        [MenuItem("Purrfect Catch/2. Build All Scenes", priority = 2)]
        public static void BuildAllScenes()
        {
            if (!TmpReady())
            {
                EditorUtility.DisplayDialog("TextMeshPro",
                    "Спочатку імпортуй ресурси TextMeshPro:\n" +
                    "Window → TextMeshPro → Import TMP Essential Resources.\n\n" +
                    "Потім запусти цю команду ще раз.", "OK");
                return;
            }

            bool anyExists = File.Exists($"{ScenesFolder}/MainMenu.unity") ||
                             File.Exists($"{ScenesFolder}/Cafe.unity") ||
                             File.Exists($"{ScenesFolder}/Kitchen.unity");
            if (anyExists && !EditorUtility.DisplayDialog("Purrfect Catch",
                    "Сцени MainMenu / Cafe / Kitchen вже існують. Перезібрати їх з нуля?\n" +
                    "Зміни, зроблені в цих сценах вручну, пропадуть.", "Перезібрати", "Скасувати"))
                return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            CreateStarterData();
            LoadPlaceholderSprites();
            EnsureFolder(ScenesFolder);

            var db = AssetDatabase.LoadAssetAtPath<GameDatabase>(DatabasePath);
            string menu = BuildMainMenu();
            string cafe = BuildCafe(db);
            string kitchen = BuildKitchen();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(menu, true),
                new EditorBuildSettingsScene(cafe, true),
                new EditorBuildSettingsScene(kitchen, true),
            };

            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(menu);
            EditorUtility.DisplayDialog("Purrfect Catch",
                "Готово! Відкрито сцену MainMenu, тисни Play.\n\n" +
                "Кухня: A/← у відро, D/→ на тарілку (або клік мишкою), Esc для паузи.", "Мяу!");
        }

        private static bool TmpReady()
        {
            try
            {
                return TMP_Settings.defaultFontAsset != null;
            }
            catch
            {
                return false;
            }
        }

        // ── Головне меню ──
        private static string BuildMainMenu()
        {
            var scene = NewScene();
            CreateCamera(Hex("F4C9B8"));
            var canvas = CreateCanvas().transform;

            var background = Img(canvas, "Background (заміни на арт)", null, Hex("F4C9B8"));
            Stretch(background.rectTransform);

            var title = Text(canvas, "Title", "PURRFECT CATCH", 130, PanelBrown);
            title.fontStyle = FontStyles.Bold;
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -80), new Vector2(1500, 180));

            var box = Box(canvas, new Vector2(560, 440), null, TextAnchor.MiddleCenter);
            box.anchoredPosition = new Vector2(0, -80);
            var play = Btn(box, "PLAY", new Vector2(420, 100));
            var settingsButton = Btn(box, "SETTINGS", new Vector2(420, 100));
            var exit = Btn(box, "EXIT", new Vector2(420, 100));

            var settings = BuildSettings(canvas);

            var menu = canvas.gameObject.AddComponent<MainMenu>();
            Wire(menu, ("playButton", play), ("settingsButton", settingsButton),
                 ("exitButton", exit), ("settings", settings));

            new GameObject("Scene Music").AddComponent<SceneMusic>();
            return SaveScene(scene, "MainMenu");
        }

        // ── Кухня ──
        private static string BuildKitchen()
        {
            var scene = NewScene();
            CreateCamera(Hex("D9CDB4"));

            Sprite2D("Board", rounded, Hex("C08A5B"), new Vector3(0, -0.2f, 0), new Vector2(3.6f, 11f), -5);

            var trash = Sprite2D("Trash Bin", rounded, Hex("5E6B73"), new Vector3(-5.2f, -1.5f, 0), new Vector2(2.6f, 3.2f), 0);
            trash.gameObject.AddComponent<BoxCollider2D>();
            var trashZone = trash.gameObject.AddComponent<SortZone>();
            SetEnum(trashZone, "side", (int)SortSide.Trash);
            WorldLabel("[A]  TRASH", new Vector3(-5.2f, -3.8f, 0));

            var plate = Sprite2D("Plate", circle, Color.white, new Vector3(5.2f, -1.5f, 0), new Vector2(3.2f, 3.2f), 0);
            plate.gameObject.AddComponent<CircleCollider2D>();
            var plateZone = plate.gameObject.AddComponent<SortZone>();
            SetEnum(plateZone, "side", (int)SortSide.Plate);
            WorldLabel("PLATE  [D]", new Vector3(5.2f, -3.8f, 0));

            var spawn = new GameObject("Spawn Point").transform;
            spawn.position = new Vector3(0, 6.2f, 0);
            var miss = new GameObject("Miss Line").transform;
            miss.position = new Vector3(0, -4.4f, 0);

            var shift = new GameObject("Shift Manager").AddComponent<ShiftManager>();
            Wire(shift, ("itemPrefab", GetItemPrefab()), ("spawnPoint", spawn), ("missLine", miss),
                 ("trashZone", trashZone), ("plateZone", plateZone), ("placeholderSprite", rounded));

            // HUD
            var canvasComp = CreateCanvas();
            var canvas = canvasComp.transform;

            var heartsRow = Rect("Hearts", canvas);
            Place(heartsRow, new Vector2(0, 1), new Vector2(40, -30), new Vector2(260, 70));
            var hl = heartsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 12;
            hl.childControlWidth = hl.childControlHeight = false;
            hl.childForceExpandWidth = hl.childForceExpandHeight = false;
            var hearts = new Object[3];
            for (int i = 0; i < 3; i++)
            {
                var heart = Img(heartsRow, $"Heart {i + 1}", circle, Hex("E5484D"));
                heart.rectTransform.sizeDelta = new Vector2(64, 64);
                hearts[i] = heart;
            }

            var coinIcon = Img(canvas, "Coin Icon", circle, Hex("F2C14E"));
            Place(coinIcon.rectTransform, new Vector2(0, 1), new Vector2(46, -118), new Vector2(52, 52));
            var earnings = Text(canvas, "Earnings Text", "0", 50, TextDark, TextAlignmentOptions.Left);
            Place(earnings.rectTransform, new Vector2(0, 1), new Vector2(112, -112), new Vector2(320, 64));

            var timerBox = Img(canvas, "Timer Box", rounded, new Color(1f, 1f, 1f, 0.45f), true);
            Place(timerBox.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(300, 96));
            var timer = Text(timerBox.transform, "Timer Text", "01:00", 64, TextDark);
            Stretch(timer.rectTransform);

            var combo = Text(canvas, "Combo Text", "x2 combo", 64, Hex("E07A2E"), TextAlignmentOptions.Right);
            combo.fontStyle = FontStyles.Bold;
            Place(combo.rectTransform, new Vector2(1, 1), new Vector2(-40, -30), new Vector2(460, 90));
            combo.gameObject.SetActive(false);

            // Підсумки зміни
            var resultOverlay = Overlay(canvas, "Result Panel");
            var resultBox = Box(resultOverlay, new Vector2(780, 640), null, TextAnchor.MiddleCenter);
            var resultText = Text(resultBox, "Result Text", "", 40, TextLight);
            resultText.rectTransform.sizeDelta = new Vector2(700, 430);
            var resultButton = Btn(resultBox, "TO CAFE", new Vector2(400, 100));
            resultOverlay.gameObject.SetActive(false);

            // Пауза
            var pauseOverlay = Overlay(canvas, "Pause Panel");
            var pauseBox = Box(pauseOverlay, new Vector2(560, 600), null, TextAnchor.MiddleCenter);
            var resume = Btn(pauseBox, "RESUME", new Vector2(420, 96));
            var pauseSettings = Btn(pauseBox, "SETTINGS", new Vector2(420, 96));
            var toCafe = Btn(pauseBox, "TO CAFE", new Vector2(420, 96));
            var mainMenu = Btn(pauseBox, "MAIN MENU", new Vector2(420, 96));
            var pauseClose = CloseButton(pauseBox);
            pauseOverlay.gameObject.SetActive(false);

            var settings = BuildSettings(canvas);

            var hud = canvasComp.gameObject.AddComponent<KitchenHUD>();
            Wire(hud, ("shift", shift), ("timerText", timer), ("comboText", combo), ("earningsText", earnings),
                 ("resultPanel", resultOverlay.gameObject), ("resultText", resultText),
                 ("resultToCafeButton", resultButton));
            WireArray(hud, "hearts", hearts);

            var pause = canvasComp.gameObject.AddComponent<PauseMenu>();
            Wire(pause, ("shift", shift), ("panel", pauseOverlay.gameObject), ("settings", settings),
                 ("resumeButton", resume), ("settingsButton", pauseSettings), ("toCafeButton", toCafe),
                 ("mainMenuButton", mainMenu), ("closeButton", pauseClose));

            new GameObject("Scene Music").AddComponent<SceneMusic>();
            return SaveScene(scene, "Kitchen");
        }

        private static KitchenItem GetItemPrefab()
        {
            string path = PrefabFolder + "/KitchenItem.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing.GetComponent<KitchenItem>();

            EnsureFolder(PrefabFolder);
            var go = new GameObject("KitchenItem");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = rounded;
            sr.sortingOrder = 10;
            go.transform.localScale = Vector3.one * 1.3f;
            go.AddComponent<KitchenItem>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<KitchenItem>();
        }

        // ── Зала кафе ──
        private static string BuildCafe(GameDatabase db)
        {
            var scene = NewScene();
            CreateCamera(Hex("F3D9C6"));

            Sprite2D("Floor", rounded, Hex("C99B77"), new Vector3(0, -4f, 0), new Vector2(19f, 3.2f), -10);
            Sprite2D("Window", rounded, Hex("A8D8EA"), new Vector3(0, 1.4f, 0), new Vector2(6.5f, 3.8f), -9);

            var room = new GameObject("Cafe Room").AddComponent<CafeRoom>();

            // Меблі
            Vector3[] furniturePos = { new Vector3(-5.5f, -2.4f), new Vector3(0.5f, -2.2f), new Vector3(6f, -0.9f) };
            Vector2[] furnitureSize = { new Vector2(2.4f, 1.1f), new Vector2(3.6f, 1.6f), new Vector2(1.3f, 4.4f) };
            var furniturePositions = new Dictionary<FurnitureData, Vector3>();
            var furnitureSlots = new List<(Object data, Object visual)>();
            for (int i = 0; i < db.furniture.Count; i++)
            {
                var f = db.furniture[i];
                if (f == null) continue;
                var pos = i < furniturePos.Length ? furniturePos[i] : new Vector3(-7f + i * 2.5f, -2.4f);
                var size = i < furnitureSize.Length ? furnitureSize[i] : new Vector2(2f, 1.5f);
                var sr = Sprite2D($"Furniture - {f.displayName}", rounded, f.placeholderColor, pos, size, -2, room.transform);
                furniturePositions[f] = pos;
                furnitureSlots.Add((f, sr));
            }

            // Котики
            var catSlots = new List<(Object data, Object visual)>();
            float freeX = -2.5f;
            foreach (var cat in db.cats)
            {
                if (cat == null) continue;
                Vector3 pos;
                if (cat.requiredFurniture != null && furniturePositions.TryGetValue(cat.requiredFurniture, out var fp))
                    pos = fp + (cat.requiredFurniture.id == "LighthouseScratcher" ? new Vector3(0, 2.6f) : new Vector3(0, 0.9f));
                else
                {
                    pos = new Vector3(freeX, -2.9f);
                    freeX += 1.5f;
                }
                var sr = Sprite2D($"Cat - {cat.displayName}", circle, cat.placeholderColor, pos, new Vector2(1.2f, 1.2f), 2, room.transform);
                sr.gameObject.AddComponent<CircleCollider2D>();
                catSlots.Add((cat, sr));
            }

            var roomSo = new SerializedObject(room);
            FillSlots(roomSo.FindProperty("furnitureSlots"), "furniture", furnitureSlots);
            FillSlots(roomSo.FindProperty("catSlots"), "cat", catSlots);
            roomSo.ApplyModifiedPropertiesWithoutUndo();

            // HUD
            var canvasComp = CreateCanvas();
            var canvas = canvasComp.transform;

            var coinIcon = Img(canvas, "Coin Icon", circle, Hex("F2C14E"));
            Place(coinIcon.rectTransform, new Vector2(0, 1), new Vector2(40, -34), new Vector2(60, 60));
            var coins = Text(canvas, "Coins Text", "0", 56, TextDark, TextAlignmentOptions.Left);
            Place(coins.rectTransform, new Vector2(0, 1), new Vector2(115, -28), new Vector2(500, 72));
            var income = Text(canvas, "Income Text", "+1 coins/sec", 34, PanelBrown, TextAlignmentOptions.Left);
            Place(income.rectTransform, new Vector2(0, 1), new Vector2(42, -104), new Vector2(500, 50));

            var topRight = Rect("Top Buttons", canvas);
            Place(topRight, new Vector2(1, 1), new Vector2(-30, -30), new Vector2(560, 80));
            var trl = topRight.gameObject.AddComponent<HorizontalLayoutGroup>();
            trl.spacing = 16;
            trl.childAlignment = TextAnchor.MiddleRight;
            trl.childControlWidth = trl.childControlHeight = false;
            trl.childForceExpandWidth = trl.childForceExpandHeight = false;
            var settingsButton = Btn(topRight, "SETTINGS", new Vector2(250, 76), 34);
            var menuButton = Btn(topRight, "MENU", new Vector2(200, 76), 34);

            var bottom = Rect("Bottom Buttons", canvas);
            Place(bottom, new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(1300, 120));
            var bl = bottom.gameObject.AddComponent<HorizontalLayoutGroup>();
            bl.spacing = 30;
            bl.childAlignment = TextAnchor.MiddleCenter;
            bl.childControlWidth = bl.childControlHeight = false;
            bl.childForceExpandWidth = bl.childForceExpandHeight = false;
            var shopButton = Btn(bottom, "SHOP", new Vector2(340, 110));
            var bookButton = Btn(bottom, "GUEST BOOK", new Vector2(380, 110));
            var startButton = Btn(bottom, "START SHIFT", new Vector2(420, 110), 44, Hex("F2C14E"));

            var res = UiResources();

            // Магазин
            var shopOverlay = Overlay(canvas, "Shop Panel");
            var shopBox = Box(shopOverlay, new Vector2(1120, 860), "SHOP");
            var shopList = ScrollList(shopBox, new Vector2(1040, 680), res);
            var shopClose = CloseButton(shopBox);
            var shop = shopOverlay.gameObject.AddComponent<ShopPanel>();
            Wire(shop, ("listRoot", shopList), ("itemViewPrefab", BuildShopItemPrefab(canvas)), ("closeButton", shopClose));
            shopOverlay.gameObject.SetActive(false);

            // Книга гостей
            var bookOverlay = Overlay(canvas, "Guest Book Panel");
            var bookBox = Box(bookOverlay, new Vector2(1120, 860), "GUEST BOOK");
            var bookList = ScrollList(bookBox, new Vector2(1040, 680), res);
            var bookClose = CloseButton(bookBox);
            var book = bookOverlay.gameObject.AddComponent<GuestBookPanel>();
            Wire(book, ("listRoot", bookList), ("cardPrefab", BuildCatCardPrefab(canvas)), ("closeButton", bookClose));
            bookOverlay.gameObject.SetActive(false);

            var settings = BuildSettings(canvas);

            var hud = canvasComp.gameObject.AddComponent<CafeHUD>();
            Wire(hud, ("coinsText", coins), ("incomeText", income), ("shopButton", shopButton),
                 ("guestBookButton", bookButton), ("startShiftButton", startButton),
                 ("settingsButton", settingsButton), ("mainMenuButton", menuButton),
                 ("shop", shop), ("guestBook", book), ("settings", settings));

            new GameObject("Scene Music").AddComponent<SceneMusic>();
            return SaveScene(scene, "Cafe");
        }

        private static ShopItemView BuildShopItemPrefab(Transform temporaryParent)
        {
            EnsureFolder(PrefabFolder + "/UI");
            var row = Img(temporaryParent, "ShopItemView", rounded, RowCream, true);
            row.rectTransform.sizeDelta = new Vector2(980, 170);

            var icon = Img(row.transform, "Icon", rounded, Color.white, true);
            Place(icon.rectTransform, new Vector2(0, 0.5f), new Vector2(24, 0), new Vector2(120, 120));

            var nameText = Text(row.transform, "Name", "Name", 40, TextDark, TextAlignmentOptions.Left);
            nameText.fontStyle = FontStyles.Bold;
            Place(nameText.rectTransform, new Vector2(0, 0.5f), new Vector2(170, 34), new Vector2(520, 56));
            var description = Text(row.transform, "Description", "Description", 26, TextDark, TextAlignmentOptions.TopLeft);
            Place(description.rectTransform, new Vector2(0, 0.5f), new Vector2(170, -26), new Vector2(520, 70));

            var buy = Btn(row.transform, "Buy", new Vector2(250, 90), 34);
            Place((RectTransform)buy.transform, new Vector2(1, 0.5f), new Vector2(-24, 0), new Vector2(250, 90));
            var buyText = buy.GetComponentInChildren<TextMeshProUGUI>();

            var view = row.gameObject.AddComponent<ShopItemView>();
            Wire(view, ("icon", icon), ("nameText", nameText), ("descriptionText", description),
                 ("buyButton", buy), ("buyButtonText", buyText));

            var prefab = PrefabUtility.SaveAsPrefabAsset(row.gameObject, PrefabFolder + "/UI/ShopItemView.prefab");
            Object.DestroyImmediate(row.gameObject);
            return prefab.GetComponent<ShopItemView>();
        }

        private static CatCardView BuildCatCardPrefab(Transform temporaryParent)
        {
            EnsureFolder(PrefabFolder + "/UI");
            var row = Img(temporaryParent, "CatCardView", rounded, RowCream, true);
            row.rectTransform.sizeDelta = new Vector2(980, 200);

            var portrait = Img(row.transform, "Portrait", circle, Color.white);
            Place(portrait.rectTransform, new Vector2(0, 0.5f), new Vector2(24, 0), new Vector2(150, 150));

            var nameText = Text(row.transform, "Name", "Name", 40, TextDark, TextAlignmentOptions.Left);
            nameText.fontStyle = FontStyles.Bold;
            Place(nameText.rectTransform, new Vector2(0, 0.5f), new Vector2(200, 62), new Vector2(740, 56));
            var body = Text(row.transform, "Body", "Text", 26, TextDark, TextAlignmentOptions.TopLeft);
            Place(body.rectTransform, new Vector2(0, 0.5f), new Vector2(200, -30), new Vector2(740, 128));

            var view = row.gameObject.AddComponent<CatCardView>();
            Wire(view, ("portrait", portrait), ("nameText", nameText), ("bodyText", body));

            var prefab = PrefabUtility.SaveAsPrefabAsset(row.gameObject, PrefabFolder + "/UI/CatCardView.prefab");
            Object.DestroyImmediate(row.gameObject);
            return prefab.GetComponent<CatCardView>();
        }

        // ── Вікно налаштувань (спільне для всіх сцен) ──
        private static SettingsMenu BuildSettings(Transform canvas)
        {
            var res = UiResources();
            var overlay = Overlay(canvas, "Settings Panel");
            var box = Box(overlay, new Vector2(700, 680), "SETTINGS");

            var master = LabeledSlider(box, "Master Volume", res);
            var music = LabeledSlider(box, "Music", res);
            var sfx = LabeledSlider(box, "Sound Effects", res);

            var toggleLabel = Text(box, "Fullscreen Label", "Full-Screen Mode", 34, TextLight);
            toggleLabel.rectTransform.sizeDelta = new Vector2(500, 44);

            var toggleGo = DefaultControls.CreateToggle(res);
            toggleGo.name = "Fullscreen Toggle";
            toggleGo.transform.SetParent(box, false);
            var oldLabel = toggleGo.transform.Find("Label");
            if (oldLabel != null) Object.DestroyImmediate(oldLabel.gameObject);
            ((RectTransform)toggleGo.transform).sizeDelta = new Vector2(48, 48);
            var toggleBg = toggleGo.transform.Find("Background") as RectTransform;
            if (toggleBg != null)
            {
                Stretch(toggleBg);
                var check = toggleBg.Find("Checkmark") as RectTransform;
                if (check != null) Stretch(check);
            }

            var close = CloseButton(box);

            var settings = overlay.gameObject.AddComponent<SettingsMenu>();
            Wire(settings, ("masterSlider", master), ("musicSlider", music), ("sfxSlider", sfx),
                 ("fullscreenToggle", toggleGo.GetComponent<Toggle>()), ("closeButton", close));
            overlay.gameObject.SetActive(false);
            return settings;
        }

        private static Slider LabeledSlider(Transform box, string label, DefaultControls.Resources res)
        {
            var text = Text(box, label + " Label", label, 34, TextLight);
            text.rectTransform.sizeDelta = new Vector2(500, 44);

            var go = DefaultControls.CreateSlider(res);
            go.name = label + " Slider";
            go.transform.SetParent(box, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(480, 28);
            var slider = go.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.8f;
            return slider;
        }

        // ───────────────────────────── Допоміжне: сцена ─────────────────────────────

        private static Scene NewScene() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        private static string SaveScene(Scene scene, string name)
        {
            string path = $"{ScenesFolder}/{name}.unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        private static Camera CreateCamera(Color background)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = background;
            go.transform.position = new Vector3(0, 0, -10);
            go.AddComponent<AudioListener>();
            return cam;
        }

        private static SpriteRenderer Sprite2D(string name, Sprite sprite, Color color, Vector3 position,
                                               Vector2 size, int sortingOrder, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = sortingOrder;
            return sr;
        }

        private static void WorldLabel(string text, Vector3 position)
        {
            var go = new GameObject("Label " + text);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = 5;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = TextDark;
            tmp.rectTransform.sizeDelta = new Vector2(5, 1);
            tmp.sortingOrder = 5;
            go.transform.position = position;
        }

        // ───────────────────────────── Допоміжне: UI ─────────────────────────────

        private static Canvas CreateCanvas()
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.layer = LayerMask.NameToLayer("UI");
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            return canvas;
        }

        private static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Place(RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Image Img(Transform parent, string name, Sprite sprite, Color color, bool sliced = false)
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            if (sliced && sprite != null) img.type = Image.Type.Sliced;
            return img;
        }

        private static TextMeshProUGUI Text(Transform parent, string name, string text, float size, Color color,
                                            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var rt = Rect(name, parent);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button Btn(Transform parent, string label, Vector2 size, float fontSize = 44, Color? color = null)
        {
            var img = Img(parent, label + " Button", rounded, color ?? ButtonBeige, true);
            img.rectTransform.sizeDelta = size;
            var button = img.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.96f, 0.9f);
            colors.pressedColor = new Color(0.85f, 0.78f, 0.7f);
            colors.disabledColor = new Color(0.65f, 0.65f, 0.65f, 0.6f);
            button.colors = colors;

            var text = Text(img.transform, "Label", label, fontSize, TextDark);
            text.fontStyle = FontStyles.Bold;
            Stretch(text.rectTransform);
            return button;
        }

        /// <summary>Напівпрозорий фон на весь екран, що блокує кліки під вікном.</summary>
        private static RectTransform Overlay(Transform parent, string name)
        {
            var img = Img(parent, name, null, Dim);
            Stretch(img.rectTransform);
            return img.rectTransform;
        }

        private static RectTransform Box(Transform parent, Vector2 size, string title,
                                         TextAnchor alignment = TextAnchor.UpperCenter)
        {
            var img = Img(parent, "Window", rounded, PanelBrown, true);
            Place(img.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, size);

            var layout = img.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = alignment;
            layout.spacing = 22;
            layout.padding = new RectOffset(40, 40, 36, 40);
            layout.childControlWidth = layout.childControlHeight = false;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;

            if (title != null)
            {
                var t = Text(img.transform, "Title", title, 58, TextLight);
                t.fontStyle = FontStyles.Bold;
                t.rectTransform.sizeDelta = new Vector2(size.x - 160, 72);
            }
            return img.rectTransform;
        }

        private static Button CloseButton(RectTransform box)
        {
            var button = Btn(box, "X", new Vector2(64, 64), 36);
            button.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            Place((RectTransform)button.transform, new Vector2(1, 1), new Vector2(-18, -18), new Vector2(64, 64));
            return button;
        }

        private static RectTransform ScrollList(Transform parent, Vector2 size, DefaultControls.Resources res)
        {
            var go = DefaultControls.CreateScrollView(res);
            go.name = "Scroll View";
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.12f);

            var scroll = go.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;
            if (scroll.horizontalScrollbar != null)
            {
                Object.DestroyImmediate(scroll.horizontalScrollbar.gameObject);
                scroll.horizontalScrollbar = null;
            }

            var content = scroll.content;
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;

            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 16;
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return content;
        }

        private static DefaultControls.Resources UiResources()
        {
            return new DefaultControls.Resources
            {
                standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
                background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
                inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
                knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
                checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd"),
                dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd"),
                mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd"),
            };
        }

        // ───────────────────────────── Допоміжне: ассети ─────────────────────────────

        private static void LoadPlaceholderSprites()
        {
            rounded = CreatePlaceholderSprite("RoundedSquare", false);
            circle = CreatePlaceholderSprite("Circle", true);
        }

        /// <summary>Генерує білий PNG (коло або заокруглений квадрат), що фарбується через color.</summary>
        private static Sprite CreatePlaceholderSprite(string name, bool isCircle)
        {
            string path = $"{PlaceholderFolder}/{name}.png";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;

            EnsureFolder(PlaceholderFolder);
            const int size = 128;
            const float cornerRadius = 28f;
            var pixels = new Color32[size * size];
            float half = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f - half;
                    float py = y + 0.5f - half;
                    float alpha;
                    if (isCircle)
                    {
                        alpha = Mathf.Clamp01(half - Mathf.Sqrt(px * px + py * py));
                    }
                    else
                    {
                        float dx = Mathf.Max(Mathf.Abs(px) - (half - cornerRadius), 0f);
                        float dy = Mathf.Max(Mathf.Abs(py) - (half - cornerRadius), 0f);
                        alpha = Mathf.Clamp01(cornerRadius - Mathf.Sqrt(dx * dx + dy * dy));
                    }
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255));
                }
            }

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = size; // спрайт рівно 1×1 юніт
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            if (!isCircle) importer.spriteBorder = new Vector4(32, 32, 32, 32); // для 9-slice в UI
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static T LoadOrCreate<T>(string path, out bool created) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            created = asset == null;
            if (created)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        private static void AddUnique<T>(List<T> list, IEnumerable<T> values) where T : Object
        {
            foreach (var v in values)
                if (v != null && !list.Contains(v))
                    list.Add(v);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string folderName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static void Wire(Object target, params (string field, Object value)[] references)
        {
            var so = new SerializedObject(target);
            foreach (var (field, value) in references)
            {
                var property = so.FindProperty(field);
                if (property == null)
                {
                    Debug.LogError($"[ProjectSetup] У {target.GetType().Name} немає поля '{field}'");
                    continue;
                }
                property.objectReferenceValue = value;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireArray(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(field);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void FillSlots(SerializedProperty list, string dataField, List<(Object data, Object visual)> slots)
        {
            list.arraySize = slots.Count;
            for (int i = 0; i < slots.Count; i++)
            {
                var element = list.GetArrayElementAtIndex(i);
                element.FindPropertyRelative(dataField).objectReferenceValue = slots[i].data;
                element.FindPropertyRelative("visual").objectReferenceValue = slots[i].visual;
            }
        }

        private static void SetEnum(Object target, string field, int value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).enumValueIndex = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var color);
            return color;
        }
    }
}
