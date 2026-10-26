using System.IO;
using System.Text;
using UnityEngine;

namespace ScenarioKit
{
    /// <summary>
    /// Saves the journal as JSON Lines (one record per line) to
    /// <c>persistentDataPath/ScenarioLogs</c>. Saved files can be analyzed again later.
    /// </summary>
    [CreateAssetMenu(menuName = "Scenario/Loggers/JSON Lines File Logger", fileName = "Logger_JsonLines", order = 61)]
    public sealed class JsonLinesLogger : LoggerAsset
    {
        public override ILogWriter Open(string sessionName)
        {
            Directory.CreateDirectory(JournalFile.Folder);
            var path = Path.Combine(JournalFile.Folder, sessionName + JournalFile.Extension);
            Debug.Log($"[Scenario] Журнал сохраняется в {path}");
            return new Writer(path);
        }

        sealed class Writer : ILogWriter
        {
            readonly StreamWriter stream;

            public Writer(string path) => stream = new StreamWriter(path, false, new UTF8Encoding(false));

            public void Write(LogRecord record)
            {
                stream.WriteLine(JsonUtility.ToJson(record));
                stream.Flush();
            }

            public void Dispose() => stream.Dispose();
        }
    }
}
