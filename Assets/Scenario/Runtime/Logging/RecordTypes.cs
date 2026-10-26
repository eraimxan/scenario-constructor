namespace ScenarioKit
{
    /// <summary>Record types written by the session. The list is open: loggers and analyzers must tolerate others.</summary>
    public static class RecordTypes
    {
        /// <summary>data: scenario, mode, rules, outOfTurn, startedAt.</summary>
        public const string SessionStart = "session.start";
        /// <summary>result: Finished | Aborted.</summary>
        public const string SessionEnd = "session.end";
        /// <summary>One per step of the plan, written at start. data: title, group, groupIndex.</summary>
        public const string StepRegistered = "step.registered";
        /// <summary>data: name, index.</summary>
        public const string GroupStart = "group.start";
        /// <summary>data: name, violations; message: why it ended early (if it did).</summary>
        public const string GroupEnd = "group.end";
        /// <summary>Raw input. data: action, source.</summary>
        public const string Action = "action";
        public const string StepActivated = "step.activated";
        /// <summary>The step accepted a part of its sequence. data: progress.</summary>
        public const string StepProgress = "step.progress";
        /// <summary>message: reason. data: violation (count for the step), outOfTurn.</summary>
        public const string StepViolation = "step.violation";
        /// <summary>The step was reset for another attempt. data: attempt.</summary>
        public const string StepRetry = "step.retry";
        /// <summary>Final status. result: Completed | Failed | Skipped. data: attempts, violations.</summary>
        public const string StepResult = "step.result";
    }
}
