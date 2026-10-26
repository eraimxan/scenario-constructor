using UnityEngine;

namespace ScenarioKit
{
    /// <summary>Training: a violation never ends anything, the trainee repeats the same step.</summary>
    [CreateAssetMenu(menuName = "Scenario/Rules/Training Rules", fileName = "Rules_Training", order = 40)]
    public sealed class TrainingRules : ExecutionRules
    {
        public override ViolationOutcome OnViolation(StepRun step) => ViolationOutcome.RetryStep;

        public override string DescribeGroup(GroupRun group) => $"нарушений: {group.Violations}";
    }
}
