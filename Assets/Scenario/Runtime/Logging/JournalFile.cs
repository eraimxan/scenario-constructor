using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ScenarioKit
{
    /// <summary>Reads journals saved by <see cref="JsonLinesLogger"/>.</summary>
    public static class JournalFile
    {
        public const string Extension = ".jsonl";

        public static string Folder => Path.Combine(Application.persistentDataPath, "ScenarioLogs");

        /// <summary>Saved journals, newest first.</summary>
        public static IEnumerable<string> List() =>
            Directory.Exists(Folder)
                ? Directory.GetFiles(Folder, "*" + Extension).OrderByDescending(File.GetLastWriteTimeUtc)
                : Enumerable.Empty<string>();

        /// <summary>Unreadable lines are skipped and reported in <paramref name="problems"/>.</summary>
        public static List<LogRecord> Load(string path, List<string> problems)
        {
            var records = new List<LogRecord>();
            int lineNumber = 0;
            foreach (var line in File.ReadLines(path))
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                try
                {
                    var record = JsonUtility.FromJson<LogRecord>(line);
                    if (record != null && !string.IsNullOrEmpty(record.type))
                        records.Add(record);
                    else
                        problems?.Add($"Строка {lineNumber}: запись без типа пропущена.");
                }
                catch (ArgumentException e)
                {
                    problems?.Add($"Строка {lineNumber}: не удалось прочитать ({e.Message}).");
                }
            }
            return records;
        }
    }
}
