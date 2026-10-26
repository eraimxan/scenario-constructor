using UnityEngine;

namespace ScenarioKit
{
    /// <summary>Writes records to the Unity console.</summary>
    [CreateAssetMenu(menuName = "Scenario/Loggers/Console Logger", fileName = "Logger_Console", order = 60)]
    public sealed class ConsoleLogger : LoggerAsset
    {
        [Tooltip("Писать также сырые действия (тип «action»).")]
        [SerializeField] bool includeActions = true;

        public override ILogWriter Open(string sessionName) => new Writer(sessionName, includeActions);

        sealed class Writer : ILogWriter
        {
            readonly string session;
            readonly bool includeActions;

            public Writer(string session, bool includeActions)
            {
                this.session = session;
                this.includeActions = includeActions;
            }

            public void Write(LogRecord record)
            {
                if (includeActions || record.type != RecordTypes.Action)
                    Debug.Log($"[Scenario {session}] {record}");
            }

            public void Dispose() { }
        }
    }
}
