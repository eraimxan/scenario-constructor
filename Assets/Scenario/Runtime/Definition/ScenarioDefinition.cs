using System.Collections.Generic;
using UnityEngine;

namespace ScenarioKit
{
    /// <summary>The scenario: the library of steps and the modes that can run them.</summary>
    [CreateAssetMenu(menuName = "Scenario/Scenario", fileName = "Scenario", order = 1)]
    public sealed class ScenarioDefinition : ScriptableObject
    {
        [SerializeField] string title;
        [SerializeField, TextArea(2, 5)] string description;
        [Tooltip("Все шаги сценария. Режимы ссылаются на эти же ассеты, не копируя их.")]
        [SerializeField] List<StepDefinition> steps = new List<StepDefinition>();
        [Tooltip("Режимы прохождения, доступные в меню.")]
        [SerializeField] List<ModeProfile> modes = new List<ModeProfile>();

        public string Title => string.IsNullOrWhiteSpace(title) ? name : title;
        public string Description => description;
        public IReadOnlyList<StepDefinition> Steps => steps;
        public IReadOnlyList<ModeProfile> Modes => modes;

        internal void Configure(string newTitle, IEnumerable<StepDefinition> newSteps, IEnumerable<ModeProfile> newModes)
        {
            title = newTitle;
            steps = new List<StepDefinition>(newSteps);
            modes = new List<ModeProfile>(newModes);
        }
    }
}
