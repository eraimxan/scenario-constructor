using System.Collections.Generic;
using UnityEngine;

namespace ScenarioKit
{
    /// <summary>
    /// Training scoring: points depend on the attempt that completed the step
    /// (default 10 for the first, 5 for the second, 0 later). Max = first-attempt points × steps.
    /// </summary>
    [CreateAssetMenu(menuName = "Scenario/Analyzers/Training Score", fileName = "Analyzer_Training", order = 80)]
    public sealed class TrainingScoreAnalyzer : ResultAnalyzer
    {
        [Tooltip("Баллы за выполнение с 1-й, 2-й, ... попытки. Попытки сверх списка дают 0.")]
        [SerializeField] int[] pointsByAttempt = { 10, 5 };

        protected override void Evaluate(IReadOnlyList<LogRecord> records, AnalysisReport report)
        {
            var digest = JournalDigest.Build(records, report.Warnings);
            int maxPerStep = pointsByAttempt.Length > 0 ? pointsByAttempt[0] : 0;

            foreach (var step in digest.Steps)
            {
                int points = 0;
                string outcome;
                switch (step.Result)
                {
                    case StepStatus.Completed:
                        int attempt = step.Violations + 1;
                        points = attempt <= pointsByAttempt.Length ? pointsByAttempt[attempt - 1] : 0;
                        outcome = $"выполнен с {attempt}-й попытки";
                        break;
                    case null:
                        outcome = "нет результата";
                        report.Warnings.Add($"Шаг «{step.Title}» не имеет итогового результата — 0 баллов.");
                        break;
                    default:
                        outcome = $"не выполнен ({step.Result}), нарушений: {step.Violations}";
                        break;
                }

                report.Score += points;
                report.Lines.Add($"{step.Title}: {outcome} — {points}");
            }

            report.MaxScore = maxPerStep * digest.Steps.Count;
        }
    }
}
