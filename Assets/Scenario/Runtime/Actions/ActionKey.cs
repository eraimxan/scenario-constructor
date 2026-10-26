using UnityEngine;

namespace ScenarioKit
{
    /// <summary>
    /// Identifies a user action ("pressed Start", "entered zone A").
    /// Sources raise keys and steps listen to them; both reference the same asset,
    /// so a scenario author connects them by drag and drop instead of typing ids.
    /// </summary>
    [CreateAssetMenu(menuName = "Scenario/Action Key", fileName = "Action_", order = 20)]
    public sealed class ActionKey : ScriptableObject
    {
        [Tooltip("Человекочитаемое название для сообщений и журнала. Пусто — имя ассета.")]
        [SerializeField] string displayName;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        internal void Configure(string newDisplayName) => displayName = newDisplayName;
    }
}
