using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ScenarioKit
{
    /// <summary>Result of one analyzer over one journal.</summary>
    public sealed class AnalysisReport
    {
        public string AnalyzerName { get; }
        public float Score { get; set; }
        public float MaxScore { get; set; }
        public List<string> Lines { get; } = new List<string>();
        /// <summary>Missing or inconsistent data. The report is still usable.</summary>
        public List<string> Warnings { get; } = new List<string>();

        public AnalysisReport(string analyzerName) => AnalyzerName = analyzerName;

        public string Summary =>
            $"{Score.ToString("0.#", CultureInfo.InvariantCulture)} из {MaxScore.ToString("0.#", CultureInfo.InvariantCulture)}";

        public string ToText()
        {
            var text = new StringBuilder();
            text.AppendLine($"{AnalyzerName}: {Summary}");
            foreach (var line in Lines)
                text.AppendLine(line);
            foreach (var warning in Warnings)
                text.AppendLine($"⚠ {warning}");
            return text.ToString();
        }
    }
}
