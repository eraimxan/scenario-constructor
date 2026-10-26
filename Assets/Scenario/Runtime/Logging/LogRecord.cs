using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ScenarioKit
{
    [Serializable]
    public struct LogField
    {
        public string key;
        public string value;
    }

    /// <summary>
    /// The journal contract. Every record has the same five fields; anything else goes to
    /// <see cref="data"/> as key/value pairs. Readers ignore types and keys they do not know,
    /// so new record types and fields never break existing analyzers.
    /// </summary>
    [Serializable]
    public sealed class LogRecord
    {
        /// <summary>Seconds since the session started.</summary>
        public float time;
        /// <summary>See <see cref="RecordTypes"/>; custom types are allowed.</summary>
        public string type;
        public string stepId;
        public string result;
        public string message;
        public List<LogField> data = new List<LogField>();

        public LogRecord() { }

        public LogRecord(float time, string type, string stepId = null, string result = null, string message = null)
        {
            this.time = time;
            this.type = type;
            this.stepId = stepId;
            this.result = result;
            this.message = message;
        }

        public LogRecord With(string key, object value)
        {
            data.Add(new LogField { key = key, value = Convert.ToString(value, CultureInfo.InvariantCulture) });
            return this;
        }

        public string Get(string key, string fallback = null)
        {
            if (data != null)
                foreach (var field in data)
                    if (field.key == key)
                        return field.value;
            return fallback;
        }

        public bool TryGetInt(string key, out int value) =>
            int.TryParse(Get(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

        public override string ToString()
        {
            var text = new StringBuilder();
            text.Append(time.ToString("0.00", CultureInfo.InvariantCulture)).Append("s  ").Append(type);
            if (!string.IsNullOrEmpty(stepId)) text.Append("  [").Append(stepId).Append(']');
            if (!string.IsNullOrEmpty(result)) text.Append("  ").Append(result);
            if (!string.IsNullOrEmpty(message)) text.Append("  «").Append(message).Append('»');
            if (data != null && data.Count > 0)
            {
                text.Append("  {");
                for (int i = 0; i < data.Count; i++)
                    text.Append(i > 0 ? ", " : "").Append(data[i].key).Append('=').Append(data[i].value);
                text.Append('}');
            }
            return text.ToString();
        }
    }
}
