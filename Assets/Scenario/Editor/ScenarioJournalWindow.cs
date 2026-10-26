using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ScenarioKit.Editor
{
    /// <summary>
    /// Re-analyzes a finished session without running the scenario: pick a saved journal
    /// (or the session currently in Play Mode) and any analyzer.
    /// </summary>
    sealed class ScenarioJournalWindow : EditorWindow
    {
        const string CurrentSession = "Текущее прохождение (Play Mode)";

        List<string> files = new List<string>();
        List<ResultAnalyzer> analyzers = new List<ResultAnalyzer>();
        int sourceIndex;
        int analyzerIndex;
        IReadOnlyList<LogRecord> records;
        readonly List<string> loadProblems = new List<string>();
        AnalysisReport report;
        Vector2 scroll;
        bool showRecords = true;

        [MenuItem("Window/Scenario/Журнал и анализ")]
        static void Open() => GetWindow<ScenarioJournalWindow>("Журнал сценария");

        void OnEnable() => Refresh();
        void OnFocus() => Refresh();

        void Refresh()
        {
            files = JournalFile.List().ToList();
            analyzers = ScenarioEditorUtility.FindAssets<ResultAnalyzer>().ToList();
        }

        void OnGUI()
        {
            var sources = new List<string>();
            if (Application.isPlaying)
                sources.Add(CurrentSession);
            sources.AddRange(files.Select(Path.GetFileNameWithoutExtension));

            using (new EditorGUILayout.HorizontalScope())
            {
                if (sources.Count == 0)
                    EditorGUILayout.LabelField("Журнал", "Нет сохранённых журналов");
                else
                    sourceIndex = EditorGUILayout.Popup("Журнал", Mathf.Clamp(sourceIndex, 0, sources.Count - 1), sources.ToArray());
                if (GUILayout.Button("Папка", GUILayout.Width(60)))
                {
                    Directory.CreateDirectory(JournalFile.Folder);
                    EditorUtility.RevealInFinder(JournalFile.Folder);
                }
            }

            if (analyzers.Count == 0)
                EditorGUILayout.HelpBox("В проекте нет ассетов анализаторов (Create → Scenario → Analyzers).", MessageType.Warning);
            else
                analyzerIndex = EditorGUILayout.Popup("Анализатор", Mathf.Clamp(analyzerIndex, 0, analyzers.Count - 1),
                    analyzers.Select(a => a.DisplayName).ToArray());

            using (new EditorGUI.DisabledScope(sources.Count == 0 || analyzers.Count == 0))
                if (GUILayout.Button("Загрузить и оценить", GUILayout.Height(24)))
                    Analyze(sources[sourceIndex]);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var problem in loadProblems)
                EditorGUILayout.HelpBox(problem, MessageType.Warning);
            if (report != null)
                EditorGUILayout.HelpBox(report.ToText(), report.Warnings.Count > 0 ? MessageType.Warning : MessageType.Info);
            if (records != null)
            {
                showRecords = EditorGUILayout.Foldout(showRecords, $"Записи журнала ({records.Count})", true);
                if (showRecords)
                    foreach (var record in records)
                        EditorGUILayout.SelectableLabel(record.ToString(), EditorStyles.miniLabel, GUILayout.Height(14));
            }
            EditorGUILayout.EndScrollView();
        }

        void Analyze(string source)
        {
            loadProblems.Clear();
            if (source == CurrentSession)
            {
                var runner = FindAnyObjectByType<ScenarioRunner>();
                records = runner != null && runner.Session != null ? runner.Session.Journal.Records : null;
                if (records == null)
                    loadProblems.Add("В сцене нет запущенного прохождения.");
            }
            else
            {
                var path = files.First(f => Path.GetFileNameWithoutExtension(f) == source);
                records = JournalFile.Load(path, loadProblems);
            }

            report = records != null ? analyzers[analyzerIndex].Analyze(records) : null;
        }
    }
}
