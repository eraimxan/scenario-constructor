using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScenarioKit
{
    /// <summary>
    /// Scores a finished session using only its journal, so any journal (current, saved to disk,
    /// written by another mode) can be analyzed again with any analyzer.
    /// </summary>
    public abstract class ResultAnalyzer : ScriptableObject
    {
        [SerializeField] string displayName;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        /// <summary>Never throws: problems end up in <see cref="AnalysisReport.Warnings"/>.</summary>
        public AnalysisReport Analyze(IReadOnlyList<LogRecord> records)
        {
            var report = new AnalysisReport(DisplayName);
            if (records == null || records.Count == 0)
            {
                report.Warnings.Add("Журнал пуст — анализировать нечего.");
                return report;
            }

            try
            {
                Evaluate(records, report);
            }
            catch (Exception e)
            {
                report.Warnings.Add($"Анализ прерван из-за ошибки: {e.Message}");
                Debug.LogException(e, this);
            }
            return report;
        }

        protected abstract void Evaluate(IReadOnlyList<LogRecord> records, AnalysisReport report);
    }
}
