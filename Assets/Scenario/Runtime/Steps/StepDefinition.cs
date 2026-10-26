using System.Collections.Generic;
using UnityEngine;

namespace ScenarioKit
{
    /// <summary>
    /// A reusable scenario step: identity, instruction for the trainee and completion/violation logic.
    /// Steps know nothing about modes, groups, ordering or scoring.
    /// New step types derive from this class and implement <see cref="CreateLogic"/>.
    /// </summary>
    public abstract class StepDefinition : ScriptableObject
    {
        [Tooltip("Стабильный идентификатор для журнала. Пусто — имя ассета.")]
        [SerializeField] string id;
        [Tooltip("Короткое название шага.")]
        [SerializeField] string title;
        [Tooltip("Что должен сделать обучаемый.")]
        [SerializeField, TextArea(2, 4)] string instruction;

        public string Id => string.IsNullOrWhiteSpace(id) ? name : id;
        public string Title => string.IsNullOrWhiteSpace(title) ? Id : title;
        public string Instruction => instruction;

        public abstract IStepLogic CreateLogic();

        /// <summary>Configuration problems shown in the editor. Empty when the step is fine.</summary>
        public virtual IEnumerable<string> Validate()
        {
            yield break;
        }

        internal void Describe(string newId, string newTitle, string newInstruction)
        {
            id = newId;
            title = newTitle;
            instruction = newInstruction;
        }
    }
}
