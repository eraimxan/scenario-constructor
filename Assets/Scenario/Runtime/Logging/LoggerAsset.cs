using System;
using UnityEngine;

namespace ScenarioKit
{
    /// <summary>Receives journal records of one session as they are written.</summary>
    public interface ILogWriter : IDisposable
    {
        void Write(LogRecord record);
    }

    /// <summary>
    /// A selectable log output. The asset holds settings only; every session gets its own writer.
    /// A new output = a subclass returning its own <see cref="ILogWriter"/>.
    /// </summary>
    public abstract class LoggerAsset : ScriptableObject
    {
        /// <param name="sessionName">Unique, file-name safe session name.</param>
        public abstract ILogWriter Open(string sessionName);
    }
}
