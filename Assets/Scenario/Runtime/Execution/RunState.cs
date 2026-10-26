using System.Collections.Generic;

namespace ScenarioKit
{
    public enum StepStatus { Pending, Active, Completed, Failed, Skipped }

    public enum GroupState { Pending, Active, Done }

    /// <summary>Runtime state of one step in one session.</summary>
    public sealed class StepRun
    {
        public StepDefinition Definition { get; }
        public GroupRun Group { get; }
        public StepStatus Status { get; internal set; }
        public int Violations { get; internal set; }
        public int Attempt => Violations + 1;
        public bool IsResolved => Status == StepStatus.Completed || Status == StepStatus.Failed || Status == StepStatus.Skipped;
        public string ProgressHint => Logic.ProgressHint;

        internal IStepLogic Logic { get; }

        internal StepRun(StepDefinition definition, GroupRun group)
        {
            Definition = definition;
            Group = group;
            Logic = definition.CreateLogic();
        }
    }

    /// <summary>Runtime state of one group: its steps split into activation waves.</summary>
    public sealed class GroupRun
    {
        public StepGroup Definition { get; }
        public int Index { get; }
        public GroupState State { get; internal set; }
        public int Violations { get; internal set; }
        public IReadOnlyList<StepRun> Steps => steps;

        internal readonly List<StepRun> steps = new List<StepRun>();
        /// <summary>Steps activated together. A Sequential stage becomes one wave per step.</summary>
        internal readonly List<List<StepRun>> waves = new List<List<StepRun>>();
        internal int waveIndex;

        internal GroupRun(StepGroup definition, int index)
        {
            Definition = definition;
            Index = index;
        }
    }
}
