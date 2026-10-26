using UnityEngine;

namespace ScenarioKit
{
    /// <summary>
    /// Bridge for any UnityEvent (Toggle, Slider, a custom component's event):
    /// add a listener in the inspector and pick <see cref="RaiseDefault"/> or <see cref="RaiseKey"/>.
    /// </summary>
    [AddComponentMenu("Scenario/Sources/UnityEvent Action Source")]
    public sealed class UnityEventActionSource : ActionSource
    {
        [Tooltip("Действие для метода RaiseDefault().")]
        [SerializeField] ActionKey defaultKey;

        public void RaiseDefault() => Emit(defaultKey);

        public void RaiseKey(ActionKey key) => Emit(key);
    }
}
