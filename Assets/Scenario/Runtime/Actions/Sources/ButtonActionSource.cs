using UnityEngine;
using UnityEngine.UI;

namespace ScenarioKit
{
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("Scenario/Sources/Button Action Source")]
    public sealed class ButtonActionSource : ActionSource
    {
        [Tooltip("Действие при нажатии кнопки.")]
        [SerializeField] ActionKey onClick;

        Button button;

        void Awake() => button = GetComponent<Button>();
        void OnEnable() => button.onClick.AddListener(HandleClick);
        void OnDisable() => button.onClick.RemoveListener(HandleClick);

        void HandleClick() => Emit(onClick);
    }
}
