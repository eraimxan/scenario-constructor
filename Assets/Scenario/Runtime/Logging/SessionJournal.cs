using System;
using System.Collections.Generic;

namespace ScenarioKit
{
    /// <summary>In-memory journal of one session. The single source of truth for loggers and analyzers.</summary>
    public sealed class SessionJournal
    {
        readonly List<LogRecord> records = new List<LogRecord>();

        public IReadOnlyList<LogRecord> Records => records;

        public event Action<LogRecord> Added;

        public void Add(LogRecord record)
        {
            records.Add(record);
            Added?.Invoke(record);
        }
    }
}
