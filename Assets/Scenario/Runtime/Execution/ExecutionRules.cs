using UnityEngine;

namespace ScenarioKit
{
    public enum ViolationOutcome
    {
        /// <summary>The step stays active and the trainee tries again.</summary>
        RetryStep,
        /// <summary>The step is closed as Failed.</summary>
        FailStep
    }

    /// <summary>
    /// The mode-specific part of execution. The session handles activation and logging;
    /// rules decide what a violation means. A new mode = a new subclass, the session does not change.
    /// </summary>
    public abstract class ExecutionRules : ScriptableObject
    {
        public abstract ViolationOutcome OnViolation(StepRun step);

        /// <summary>Checked after every violation and when a group starts. True ends the group, unresolved steps become Skipped.</summary>
        public virtual bool ShouldCloseGroup(GroupRun group) => false;

        /// <summary>Optional hint for the HUD, e.g. "нарушений 1 из 2".</summary>
        public virtual string DescribeGroup(GroupRun group) => null;
    }
}
