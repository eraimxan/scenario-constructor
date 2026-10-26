namespace ScenarioKit
{
    public enum MessageKind { Info, Success, Violation, Warning }

    /// <summary>Short text for the trainee (violation reasons, group changes).</summary>
    public readonly struct SessionMessage
    {
        public readonly MessageKind Kind;
        public readonly string Text;

        public SessionMessage(MessageKind kind, string text)
        {
            Kind = kind;
            Text = text;
        }
    }
}
