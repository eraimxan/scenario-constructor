using UnityEngine;

namespace ScenarioKit
{
    /// <summary>Exam: a violated step fails without retry; enough violations in a group skip the rest of it.</summary>
    [CreateAssetMenu(menuName = "Scenario/Rules/Exam Rules", fileName = "Rules_Exam", order = 41)]
    public sealed class ExamRules : ExecutionRules
    {
        [Tooltip("После стольких нарушений в группе оставшиеся шаги группы пропускаются.")]
        [SerializeField, Min(1)] int violationThreshold = 2;

        public int ViolationThreshold => violationThreshold;

        public override ViolationOutcome OnViolation(StepRun step) => ViolationOutcome.FailStep;

        public override bool ShouldCloseGroup(GroupRun group) => group.Violations >= violationThreshold;

        public override string DescribeGroup(GroupRun group) => $"нарушений: {group.Violations} из {violationThreshold}";

        internal void Configure(int threshold) => violationThreshold = threshold;
    }
}
