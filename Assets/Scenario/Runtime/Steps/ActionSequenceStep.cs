using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScenarioKit
{
    /// <summary>
    /// Completed when the listed actions happen in order (one action = a simple step).
    /// Optional state requirements are checked at every action of the sequence,
    /// optional forbidden actions count as violations while the step is active.
    /// </summary>
    [CreateAssetMenu(menuName = "Scenario/Steps/Action Sequence Step", fileName = "Step_", order = 0)]
    public sealed class ActionSequenceStep : StepDefinition
    {
        [Serializable]
        public sealed class StateRequirement
        {
            public StateKey state;
            [Tooltip("Какое значение состояния требуется.")]
            public bool expected = true;
            [Tooltip("Сообщение о нарушении. Пусто — стандартное.")]
            public string violationMessage;
        }

        [Serializable]
        public sealed class ForbiddenAction
        {
            public ActionKey action;
            [Tooltip("Сообщение о нарушении. Пусто — стандартное.")]
            public string violationMessage;
        }

        [Tooltip("Действия, которые нужно выполнить по порядку. Одно действие — простой шаг.")]
        [SerializeField] List<ActionKey> sequence = new List<ActionKey>();
        [Tooltip("Действие из последовательности, выполненное раньше своей очереди, считается нарушением. Если выключено — оно просто игнорируется.")]
        [SerializeField] bool strictOrder = true;
        [Tooltip("Состояния мира, которые должны выполняться в момент каждого действия последовательности.")]
        [SerializeField] List<StateRequirement> requirements = new List<StateRequirement>();
        [Tooltip("Действия, которые считаются нарушением, пока шаг активен.")]
        [SerializeField] List<ForbiddenAction> forbidden = new List<ForbiddenAction>();

        public override IStepLogic CreateLogic() => new Logic(this);

        public override IEnumerable<string> Validate()
        {
            if (sequence.Count == 0)
                yield return "Последовательность действий пуста — шаг невозможно выполнить.";
            if (sequence.Contains(null))
                yield return "В последовательности есть пустой элемент.";
            if (requirements.Exists(r => r.state == null))
                yield return "В условиях есть элемент без State Key.";
            if (forbidden.Exists(f => f.action == null))
                yield return "В запрещённых действиях есть пустой элемент.";
            if (forbidden.Exists(f => f.action != null && sequence.Contains(f.action)))
                yield return "Одно и то же действие одновременно требуется и запрещено.";
        }

        internal void Configure(IEnumerable<ActionKey> actions, bool strict = true)
        {
            sequence = new List<ActionKey>(actions);
            strictOrder = strict;
        }

        internal void AddRequirement(StateKey state, bool expected, string message = null) =>
            requirements.Add(new StateRequirement { state = state, expected = expected, violationMessage = message });

        internal void AddForbidden(ActionKey action, string message = null) =>
            forbidden.Add(new ForbiddenAction { action = action, violationMessage = message });

        sealed class Logic : IStepLogic
        {
            readonly ActionSequenceStep step;
            int done;

            public Logic(ActionSequenceStep step) => this.step = step;

            public string ProgressHint => step.sequence.Count > 1 ? $"{done} / {step.sequence.Count}" : null;

            public bool IsRelevant(ActionKey key) => key != null && step.sequence.Contains(key);

            public void Reset() => done = 0;

            public StepResponse Handle(in ActionEvent action, IWorldState world)
            {
                var key = action.Key;

                foreach (var f in step.forbidden)
                    if (f.action == key)
                        return Fail(Or(f.violationMessage, $"запрещённое действие «{key.DisplayName}»"));

                if (done < step.sequence.Count && step.sequence[done] == key)
                {
                    foreach (var r in step.requirements)
                        if (r.state != null && world.Get(r.state) != r.expected)
                            return Fail(Or(r.violationMessage,
                                $"условие не выполнено: «{r.state.DisplayName}» должно быть {(r.expected ? "включено" : "выключено")}"));

                    done++;
                    return done == step.sequence.Count ? StepResponse.Completed : StepResponse.Progress();
                }

                // Repeating an already performed part of the sequence is harmless.
                int index = step.sequence.IndexOf(key);
                if (index < 0 || index < done)
                    return StepResponse.Ignored;

                return step.strictOrder
                    ? Fail($"нарушен порядок: сначала «{step.sequence[done].DisplayName}»")
                    : StepResponse.Ignored;
            }

            StepResponse Fail(string reason)
            {
                done = 0;
                return StepResponse.Violation(reason);
            }

            static string Or(string custom, string fallback) => string.IsNullOrWhiteSpace(custom) ? fallback : custom;
        }
    }
}
