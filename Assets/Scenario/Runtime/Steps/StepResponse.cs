namespace ScenarioKit
{
    public enum StepResponseKind
    {
        /// <summary>The action has nothing to do with the step.</summary>
        Ignored,
        /// <summary>The action moved the step forward but did not finish it.</summary>
        Progress,
        Completed,
        Violation
    }

    /// <summary>What a step logic says about one action.</summary>
    public readonly struct StepResponse
    {
        public readonly StepResponseKind Kind;
        public readonly string Message;

        StepResponse(StepResponseKind kind, string message)
        {
            Kind = kind;
            Message = message;
        }

        public static StepResponse Ignored => new StepResponse(StepResponseKind.Ignored, null);
        public static StepResponse Progress(string message = null) => new StepResponse(StepResponseKind.Progress, message);
        public static StepResponse Completed => new StepResponse(StepResponseKind.Completed, null);
        public static StepResponse Violation(string reason) => new StepResponse(StepResponseKind.Violation, reason);
    }
}
