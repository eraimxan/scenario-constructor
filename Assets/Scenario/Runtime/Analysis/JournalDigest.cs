using System;
using System.Collections.Generic;

namespace ScenarioKit
{
    /// <summary>What the journal says about one step.</summary>
    public sealed class StepFacts
    {
        public string Id;
        public string Title;
        public string Group;
        public int Violations;
        /// <summary>Null when the journal has no final result for the step.</summary>
        public StepStatus? Result;
        public float? ActivatedAt;
        public float? ResolvedAt;
    }

    /// <summary>
    /// Per-step facts extracted from journal records. Shared by all analyzers, so every analyzer
    /// works with any journal that follows the record contract. Unknown record types are ignored.
    /// </summary>
    public sealed class JournalDigest
    {
        readonly List<StepFacts> steps = new List<StepFacts>();
        readonly Dictionary<string, StepFacts> byId = new Dictionary<string, StepFacts>();

        public string Mode { get; private set; }
        public IReadOnlyList<StepFacts> Steps => steps;

        /// <summary>Problems are added to <paramref name="warnings"/>; building never fails.</summary>
        public static JournalDigest Build(IReadOnlyList<LogRecord> records, List<string> warnings)
        {
            var digest = new JournalDigest();
            bool started = false, registered = false;
            string endResult = null;

            foreach (var record in records)
            {
                if (record == null)
                    continue;
                switch (record.type)
                {
                    case RecordTypes.SessionStart:
                        started = true;
                        digest.Mode = record.Get("mode");
                        break;
                    case RecordTypes.SessionEnd:
                        endResult = record.result ?? "";
                        break;
                    case RecordTypes.StepRegistered when HasStep(record):
                        registered = true;
                        var facts = digest.Facts(record.stepId);
                        facts.Title = record.Get("title", facts.Title);
                        facts.Group = record.Get("group", facts.Group);
                        break;
                    case RecordTypes.StepActivated when HasStep(record):
                        var activated = digest.Facts(record.stepId);
                        activated.ActivatedAt ??= record.time;
                        break;
                    case RecordTypes.StepViolation when HasStep(record):
                        digest.Facts(record.stepId).Violations++;
                        break;
                    case RecordTypes.StepResult when HasStep(record):
                        var resolved = digest.Facts(record.stepId);
                        if (Enum.TryParse(record.result, out StepStatus status))
                        {
                            resolved.Result = status;
                            resolved.ResolvedAt = record.time;
                        }
                        else
                            warnings.Add($"Шаг «{record.stepId}»: неизвестный результат «{record.result}».");
                        break;
                }
            }

            if (!started)
                warnings.Add("В журнале нет записи о начале прохождения (session.start).");
            if (!registered && digest.steps.Count > 0)
                warnings.Add("В журнале нет списка шагов (step.registered): шаги восстановлены по событиям, максимум может быть занижен.");
            if (endResult == null)
                warnings.Add("Прохождение не завершено: в журнале нет записи session.end.");
            else if (endResult == "Aborted")
                warnings.Add("Прохождение было прервано.");
            if (digest.steps.Count == 0)
                warnings.Add("В журнале нет ни одного шага.");

            return digest;
        }

        static bool HasStep(LogRecord record) => !string.IsNullOrEmpty(record.stepId);

        StepFacts Facts(string id)
        {
            if (!byId.TryGetValue(id, out var facts))
            {
                facts = new StepFacts { Id = id, Title = id, Group = "" };
                byId.Add(id, facts);
                steps.Add(facts);
            }
            return facts;
        }
    }
}
