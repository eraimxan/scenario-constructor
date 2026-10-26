using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScenarioKit
{
    public enum OutOfTurnPolicy
    {
        [Tooltip("Действие шага до его очереди фиксируется как нарушение этого шага.")]
        Violation,
        [Tooltip("Шаг полностью неактивен до своей очереди и не принимает действия.")]
        IgnoreUntilActive
    }

    /// <summary>
    /// One way to run the scenario: activation plan over the shared step assets,
    /// execution rules and the default analyzer. Several profiles reuse the same steps.
    /// </summary>
    [CreateAssetMenu(menuName = "Scenario/Mode Profile", fileName = "Mode_", order = 2)]
    public sealed class ModeProfile : ScriptableObject
    {
        [SerializeField] string displayName;
        [SerializeField, TextArea(2, 4)] string description;
        [Tooltip("Что происходит при нарушении и когда группа завершается досрочно.")]
        [SerializeField] ExecutionRules rules;
        [SerializeField] OutOfTurnPolicy outOfTurn = OutOfTurnPolicy.Violation;
        [Tooltip("Анализатор, который оценивает результат сразу после прохождения.")]
        [SerializeField] ResultAnalyzer defaultAnalyzer;
        [Tooltip("Группы → этапы → шаги. Порядок меняется перетаскиванием.")]
        [SerializeField] List<StepGroup> groups = new List<StepGroup>();

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public ExecutionRules Rules => rules;
        public OutOfTurnPolicy OutOfTurn => outOfTurn;
        public ResultAnalyzer DefaultAnalyzer => defaultAnalyzer;
        public IReadOnlyList<StepGroup> Groups => groups;

        public IEnumerable<StepDefinition> AllSteps =>
            groups.SelectMany(g => g.stages).SelectMany(s => s.steps).Where(s => s != null);

        internal void Configure(string newName, ExecutionRules newRules, ResultAnalyzer analyzer,
            OutOfTurnPolicy policy, IEnumerable<StepGroup> newGroups)
        {
            displayName = newName;
            rules = newRules;
            defaultAnalyzer = analyzer;
            outOfTurn = policy;
            groups = new List<StepGroup>(newGroups);
        }
    }
}
