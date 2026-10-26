using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ScenarioKit.Demo.Editor
{
    /// <summary>uGUI helpers for the demo builder, based on the standard DefaultControls.</summary>
    static class DemoUi
    {
        static DefaultControls.Resources resources;

        static DefaultControls.Resources Resources => resources.standard != null ? resources : resources = new DefaultControls.Resources
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
            inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
            checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd"),
            dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd"),
            mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd")
        };

        public static RectTransform Place(GameObject go, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin = default, Vector2 offsetMax = default)
        {
            var rect = go.GetComponent<RectTransform>();
            if (rect == null)
                rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        public static GameObject Panel(string name, Transform parent, Color color)
        {
            var panel = DefaultControls.CreatePanel(Resources);
            panel.name = name;
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        public static VerticalLayoutGroup Column(GameObject go, int padding, int spacing)
        {
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(padding, padding, padding, padding);
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static Text Label(Transform parent, string name, string text, int size,
            TextAnchor alignment = TextAnchor.UpperLeft, float preferredHeight = -1, float flexibleHeight = -1)
        {
            var go = DefaultControls.CreateText(Resources);
            go.name = name;
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<Text>();
            label.text = text;
            label.fontSize = size;
            label.color = Color.white;
            label.alignment = alignment;
            label.supportRichText = true;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            Size(go, preferredHeight, flexibleHeight);
            return label;
        }

        public static Button Button(Transform parent, string name, string caption, float height = 44)
        {
            var go = DefaultControls.CreateButton(Resources);
            go.name = name;
            go.transform.SetParent(parent, false);
            var button = go.GetComponent<Button>();
            // Keyboard navigation would steal WASD from the player.
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            var text = go.GetComponentInChildren<Text>();
            text.text = caption;
            text.fontSize = 18;
            Size(go, height, -1);
            return button;
        }

        public static Dropdown Dropdown(Transform parent, string name)
        {
            var go = DefaultControls.CreateDropdown(Resources);
            go.name = name;
            go.transform.SetParent(parent, false);
            var dropdown = go.GetComponent<Dropdown>();
            dropdown.navigation = new Navigation { mode = Navigation.Mode.None };
            dropdown.captionText.fontSize = 18;
            dropdown.itemText.fontSize = 18;
            Size(go, 40, -1);
            return dropdown;
        }

        /// <summary>Scroll view whose content is a single wrapping text.</summary>
        public static Text ScrollText(Transform parent, string name, int size)
        {
            var scroll = DefaultControls.CreateScrollView(Resources);
            scroll.name = name;
            scroll.transform.SetParent(parent, false);
            scroll.GetComponent<Image>().color = new Color(0, 0, 0, 0.35f);
            var scrollRect = scroll.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.scrollSensitivity = 30;
            Object.DestroyImmediate(scrollRect.horizontalScrollbar.gameObject);
            scrollRect.horizontalScrollbar = null;
            Size(scroll, -1, 1);

            var content = scrollRect.content.gameObject;
            Column(content, 8, 0);
            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return Label(content.transform, "Text", "", size);
        }

        public static void Size(GameObject go, float preferredHeight, float flexibleHeight)
        {
            var element = go.GetComponent<LayoutElement>();
            if (element == null)
                element = go.AddComponent<LayoutElement>();
            element.preferredHeight = preferredHeight;
            element.flexibleHeight = flexibleHeight;
        }
    }
}
