using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScenarioKit
{
    /// <summary>
    /// Exam scoring: base score minus penalties (default Failed 10, Skipped 5), not below zero.
    /// A step without a result is treated as Skipped and reported as a warning.
    /// </summary>
    [CreateAssetMenu(menuName = "Scenario/Analyzers/Exam Penalty", fileName = "Analyzer_Exam", order = 81)]
    public sealed class ExamPenaltyAnalyzer : ResultAnalyzer
    {
        [SerializeField, Min(0)] int baseScore = 100;
        [SerializeField, Min(0)] int failedPenalty = 10;
        [SerializeField, Min(0)] int skippedPenalty = 5;

        protected override void Evaluate(IReadOnlyList<LogRecord> records, AnalysisReport report)
        {
            var digest = JournalDigest.Build(records, report.Warnings);
            int totalPenalty = 0;

            foreach (var group in digest.Steps.GroupBy(s => s.Group))
            {
                int completed = 0, failed = 0, skipped = 0;
                foreach (var step in group)
                {
                    switch (step.Result)
                    {
                        case StepStatus.Completed:
                            completed++;
                            break;
                        case StepStatus.Failed:
                            failed++;
                            break;
                        case StepStatus.Skipped:
                            skipped++;
                            break;
                        default:
                            skipped++;
                            report.Warnings.Add($"Шаг «{step.Title}» не имеет итогового результата — считается пропущенным.");
                            break;
                    }
                }

                int penalty = failed * failedPenalty + skipped * skippedPenalty;
                totalPenalty += penalty;
                var name = string.IsNullOrEmpty(group.Key) ? "Без группы" : group.Key;
                report.Lines.Add($"{name}: Completed {completed}, Failed {failed}, Skipped {skipped} — штраф {penalty}");
                foreach (var step in group)
                    report.Lines.Add($"    {step.Title}: {(step.Result?.ToString() ?? "нет результата")}");
            }

            report.Lines.Add($"Сумма штрафов: {totalPenalty}");
            report.Score = Mathf.Max(0, baseScore - totalPenalty);
            report.MaxScore = baseScore;
        }
    }
}
