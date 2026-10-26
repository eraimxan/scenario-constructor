namespace ScenarioKit
{
    /// <summary>
    /// Per-session state of one step. Created by <see cref="StepDefinition.CreateLogic"/>,
    /// so the step asset itself stays immutable and can be shared by any number of modes.
    /// </summary>
    public interface IStepLogic
    {
        /// <summary>True if the action belongs to this step. Used to detect actions performed out of turn.</summary>
        bool IsRelevant(ActionKey key);

        StepResponse Handle(in ActionEvent action, IWorldState world);

        /// <summary>Forget partial progress before the step is activated or retried.</summary>
        void Reset();

        /// <summary>Short progress text for the UI, e.g. "1 / 2". May be null.</summary>
        string ProgressHint { get; }
    }
}
