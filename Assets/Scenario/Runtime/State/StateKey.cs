using UnityEngine;

namespace ScenarioKit
{
    /// <summary>Names a boolean piece of world state ("power is on") that steps can require.</summary>
    [CreateAssetMenu(menuName = "Scenario/State Key", fileName = "State_", order = 21)]
    public sealed class StateKey : ScriptableObject
    {
        [Tooltip("Человекочитаемое название для сообщений. Пусто — имя ассета.")]
        [SerializeField] string displayName;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    }
}
