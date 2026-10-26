using UnityEngine;

namespace ScenarioKit
{
    /// <summary>
    /// Publishes a boolean state of a scene object. Call <see cref="Set"/> from any UnityEvent
    /// (e.g. Toggle.onValueChanged or a custom switch) to keep it up to date.
    /// </summary>
    [AddComponentMenu("Scenario/State Flag")]
    public sealed class StateFlag : MonoBehaviour
    {
        [SerializeField] StateKey key;
        [Tooltip("Значение при запуске сцены.")]
        [SerializeField] bool initialValue;

        public bool Value => WorldState.Shared.Get(key);

        void Awake() => WorldState.Shared.Set(key, initialValue);

        public void Set(bool value) => WorldState.Shared.Set(key, value);
    }
}
