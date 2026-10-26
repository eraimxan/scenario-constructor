using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScenarioKit.Demo.Editor
{
    /// <summary>Builds the canvas: task panel, control panel (action sources), menu, result and journal overlays.</summary>
    static class DemoInterface
    {
        // Opaque: the side panels cover screen areas the camera does not clear (see camera rect in DemoWorld).
        static readonly Color PanelColor = new Color(0.1f, 0.11f, 0.13f, 1f);
        static readonly Color OverlayColor = new Color(0.12f, 0.13f, 0.16f, 1f);

        public static void Build(DemoAssets assets, DemoWorld world, ScenarioRunner runner)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            var canvas = canvasObject.transform;

            var hud = canvasObject.AddComponent<ScenarioHud>();
            DemoWiring.Set(hud, "runner", runner);
            DemoWiring.SetList(hud, "analyzers", assets.Analyzers);

            BuildTaskPanel(canvas, hud);
            BuildControlPanel(canvas, assets, world);
            var menu = BuildMenu(canvas, hud);
            BuildResult(canvas, hud, menu);
            BuildJournal(canvas, hud);
            menu.transform.SetAsLastSibling();
        }

        static Transform Row(Transform parent, string name, float height)
        {
            var bar = new GameObject(name, typeof(RectTransform));
            bar.transform.SetParent(parent, false);
            var row = bar.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 10;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandHeight = false;
            DemoUi.Size(bar, height, -1);
            return bar.transform;
        }

        static void BuildTaskPanel(Transform canvas, ScenarioHud hud)
        {
            var panel = DemoUi.Panel("Task Panel", canvas, PanelColor);
            DemoUi.Place(panel, canvas, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(430, 0));
            DemoUi.Column(panel, 14, 8);

            var bar = Row(panel.transform, "Top Bar", 40);
            DemoWiring.Set(hud, "openMenuButton", DemoUi.Button(bar, "Menu Button", "Меню режимов", 40));
            DemoWiring.Set(hud, "journalButton", DemoUi.Button(bar, "Journal Button", "Журнал", 40));

            DemoWiring.Set(hud, "headerText", DemoUi.Label(panel.transform, "Header", "Сценарий", 26, TextAnchor.UpperLeft, 36));
            DemoWiring.Set(hud, "tasksText", DemoUi.Label(panel.transform, "Tasks", "", 18, TextAnchor.UpperLeft, -1, 1));
            DemoUi.Label(panel.transform, "Messages Title", "<color=#999999>Сообщения</color>", 15, TextAnchor.LowerLeft, 22);
            DemoWiring.Set(hud, "messagesText", DemoUi.Label(panel.transform, "Messages", "", 17, TextAnchor.LowerLeft, 160));
            DemoUi.Label(panel.transform, "Controls Hint",
                "<color=#888888>WASD / стрелки — движение, мышь — кнопки пульта справа.</color>", 15, TextAnchor.LowerLeft, 40);
        }

        static void BuildControlPanel(Transform canvas, DemoAssets assets, DemoWorld world)
        {
            var panel = DemoUi.Panel("Control Panel", canvas, PanelColor);
            DemoUi.Place(panel, canvas, new Vector2(1, 0), new Vector2(1, 1), new Vector2(-230, 0), Vector2.zero);
            DemoUi.Column(panel, 14, 10);
            DemoUi.Label(panel.transform, "Title", "<b>Пульт</b>", 24, TextAnchor.MiddleCenter, 40);

            void SourceButton(string caption, string action)
            {
                var button = DemoUi.Button(panel.transform, caption, caption, 52);
                DemoWiring.Set(button.gameObject.AddComponent<ButtonActionSource>(), "onClick", assets.Actions[action]);
            }

            SourceButton("Проверка связи", "btn.check");

            var lever = DemoUi.Button(panel.transform, "Рубильник", "Рубильник", 52);
            UnityEventTools.AddPersistentListener(lever.onClick, world.PowerSwitch.Toggle);
            var powerLabel = DemoUi.Label(panel.transform, "Power State", "Питание: ВЫКЛ", 18, TextAnchor.MiddleCenter, 28);
            DemoWiring.Set(world.PowerSwitch, "label", powerLabel);

            SourceButton("Пуск", "btn.start");
            SourceButton("Отчёт", "btn.report");
            SourceButton("Подтвердить", "btn.confirm");
            DemoUi.Label(panel.transform, "Spacer", "", 10, TextAnchor.UpperLeft, -1, 1);
        }

        static GameObject BuildMenu(Transform canvas, ScenarioHud hud)
        {
            var menu = DemoUi.Panel("Menu Panel", canvas, OverlayColor);
            DemoUi.Place(menu, canvas, new Vector2(0.36f, 0.18f), new Vector2(0.72f, 0.82f));
            DemoUi.Column(menu, 24, 12);
            DemoWiring.Set(hud, "menuPanel", menu);
            DemoWiring.Set(hud, "menuTitle", DemoUi.Label(menu.transform, "Title", "Сценарий", 28, TextAnchor.MiddleCenter, 44));
            DemoUi.Label(menu.transform, "Subtitle", "Выберите режим прохождения:", 18, TextAnchor.MiddleCenter, 30);

            var modes = new GameObject("Modes", typeof(RectTransform));
            modes.transform.SetParent(menu.transform, false);
            DemoUi.Column(modes, 0, 10);
            DemoUi.Size(modes, -1, 1);
            DemoWiring.Set(hud, "modeButtonTemplate", DemoUi.Button(modes.transform, "Mode Button Template", "Режим", 56));

            DemoWiring.Set(hud, "closeMenuButton", DemoUi.Button(menu.transform, "Close Button", "Закрыть", 40));
            return menu;
        }

        static void BuildResult(Transform canvas, ScenarioHud hud, GameObject menu)
        {
            var result = DemoUi.Panel("Result Panel", canvas, OverlayColor);
            DemoUi.Place(result, canvas, new Vector2(0.25f, 0.06f), new Vector2(0.86f, 0.94f));
            DemoUi.Column(result, 20, 10);
            DemoWiring.Set(hud, "resultPanel", result);
            DemoUi.Label(result.transform, "Title", "<b>Результат прохождения</b>", 26, TextAnchor.MiddleLeft, 40);
            DemoUi.Label(result.transform, "Analyzer Title", "Анализатор (выберите другой — оценка пересчитается по журналу):", 16, TextAnchor.LowerLeft, 26);
            DemoWiring.Set(hud, "analyzerDropdown", DemoUi.Dropdown(result.transform, "Analyzer Dropdown"));
            DemoWiring.Set(hud, "reportText", DemoUi.ScrollText(result.transform, "Report", 17));

            var bar = Row(result.transform, "Buttons", 48);
            DemoWiring.Set(hud, "replayButton", DemoUi.Button(bar, "Replay Button", "Пройти снова", 48));
            var toMenu = DemoUi.Button(bar, "Menu Button", "Другой режим", 48);
            UnityEventTools.AddBoolPersistentListener(toMenu.onClick, menu.SetActive, true);
        }

        static void BuildJournal(Transform canvas, ScenarioHud hud)
        {
            var journal = DemoUi.Panel("Journal Panel", canvas, OverlayColor);
            DemoUi.Place(journal, canvas, new Vector2(0.24f, 0.04f), new Vector2(0.87f, 0.96f));
            DemoUi.Column(journal, 16, 8);
            DemoWiring.Set(hud, "journalPanel", journal);
            DemoUi.Label(journal.transform, "Title",
                "<b>Журнал событий</b>  <color=#888888>время · тип · [шаг] · результат · «сообщение» · {данные}</color>", 18, TextAnchor.MiddleLeft, 32);
            DemoWiring.Set(hud, "journalText", DemoUi.ScrollText(journal.transform, "Records", 14));
            var close = DemoUi.Button(journal.transform, "Close Button", "Закрыть журнал", 40);
            UnityEventTools.AddBoolPersistentListener(close.onClick, journal.SetActive, false);
        }
    }
}
