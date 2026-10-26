using System.Collections.Generic;
using System.Linq;

namespace ScenarioKit
{
    /// <summary>
    /// Finds configuration mistakes a scenario author can make in the inspector.
    /// Used by the editor (warnings in the inspector) and by the runner before a session starts.
    /// </summary>
    public static class ScenarioValidator
    {
        public static IEnumerable<string> Validate(ScenarioDefinition scenario)
        {
            var ids = new HashSet<string>();
            foreach (var step in scenario.Steps)
            {
                if (step == null)
                {
                    yield return "В списке шагов есть пустая ссылка.";
                    continue;
                }
                if (!ids.Add(step.Id))
                    yield return $"Идентификатор шага «{step.Id}» не уникален.";
                foreach (var problem in step.Validate())
                    yield return $"Шаг «{step.Title}»: {problem}";
            }

            if (scenario.Modes.Count == 0)
                yield return "Нет ни одного режима прохождения.";
            foreach (var mode in scenario.Modes)
            {
                if (mode == null)
                {
                    yield return "В списке режимов есть пустая ссылка.";
                    continue;
                }
                foreach (var problem in Validate(mode, scenario))
                    yield return $"Режим «{mode.DisplayName}»: {problem}";
            }
        }

        public static IEnumerable<string> Validate(ModeProfile mode, ScenarioDefinition scenario = null)
        {
            if (mode.Rules == null)
                yield return "Не выбраны правила исполнения (Rules).";
            if (mode.DefaultAnalyzer == null)
                yield return "Не выбран анализатор по умолчанию.";
            if (mode.Groups.Count == 0)
                yield return "Нет ни одной группы.";

            foreach (var group in mode.Groups)
            {
                if (group.stages.Count == 0)
                    yield return $"Группа «{group.name}» не содержит этапов.";
                for (int s = 0; s < group.stages.Count; s++)
                {
                    var stage = group.stages[s];
                    if (stage.steps.Count == 0)
                        yield return $"Группа «{group.name}», этап {s + 1}: нет шагов.";
                    if (stage.steps.Contains(null))
                        yield return $"Группа «{group.name}», этап {s + 1}: пустая ссылка на шаг.";
                }
            }

            foreach (var duplicate in mode.AllSteps.GroupBy(s => s).Where(x => x.Count() > 1))
                yield return $"Шаг «{duplicate.Key.Title}» встречается в плане {duplicate.Count()} раза; используется первое вхождение.";

            if (scenario == null)
                yield break;

            var planned = new HashSet<StepDefinition>(mode.AllSteps);
            foreach (var step in scenario.Steps)
                if (step != null && !planned.Contains(step))
                    yield return $"Шаг сценария «{step.Title}» не включён в план режима.";
            foreach (var step in planned)
                if (!scenario.Steps.Contains(step))
                    yield return $"Шаг «{step.Title}» есть в плане, но не в списке шагов сценария.";
        }
    }
}
