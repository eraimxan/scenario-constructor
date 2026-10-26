using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScenarioKit
{
    public enum StageActivation
    {
        [Tooltip("Все шаги этапа активируются сразу и выполняются в любом порядке.")]
        Simultaneous,
        [Tooltip("Шаги этапа активируются по одному, в порядке списка.")]
        Sequential
    }

    /// <summary>A set of steps activated together (or one by one). The next stage starts when every step here is resolved.</summary>
    [Serializable]
    public sealed class Stage
    {
        [Tooltip("Необязательная пометка для автора сценария.")]
        public string note;
        public StageActivation activation = StageActivation.Simultaneous;
        public List<StepDefinition> steps = new List<StepDefinition>();
    }

    public enum GroupStart
    {
        [Tooltip("Группа начинается, когда предыдущая завершена.")]
        AfterPreviousGroup,
        [Tooltip("Группа начинается одновременно с предыдущей (группы перекрываются).")]
        WithPreviousGroup
    }

    /// <summary>
    /// A unit of execution rules: violation thresholds and skipping are counted per group.
    /// Training normally has one group, the exam several.
    /// </summary>
    [Serializable]
    public sealed class StepGroup
    {
        public string name = "Группа";
        public GroupStart start = GroupStart.AfterPreviousGroup;
        public List<Stage> stages = new List<Stage>();
    }
}
