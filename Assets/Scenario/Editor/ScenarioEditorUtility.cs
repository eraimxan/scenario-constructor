using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ScenarioKit.Editor
{
    static class ScenarioEditorUtility
    {
        public static IEnumerable<T> FindAssets<T>() where T : Object =>
            AssetDatabase.FindAssets("t:" + typeof(T).Name)
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(asset => asset != null);

        public static ScenarioDefinition FindScenarioOf(ModeProfile mode) =>
            FindAssets<ScenarioDefinition>().FirstOrDefault(s => s.Modes.Contains(mode));

        public static void DrawProblems(IEnumerable<string> problems, string okText)
        {
            var list = problems.ToList();
            if (list.Count == 0)
                EditorGUILayout.HelpBox(okText, MessageType.Info);
            else
                EditorGUILayout.HelpBox(string.Join("\n", list.Select(p => "• " + p)), MessageType.Warning);
        }

        /// <summary>Human-readable activation order of a mode, as the session will run it.</summary>
        public static string DescribePlan(ModeProfile mode)
        {
            var text = new StringBuilder();
            for (int g = 0; g < mode.Groups.Count; g++)
            {
                var group = mode.Groups[g];
                var start = g == 0 ? "сразу"
                    : group.start == GroupStart.WithPreviousGroup ? "вместе с предыдущей" : "после предыдущей";
                text.AppendLine($"Группа {g + 1} «{group.name}» — начинается {start}");
                foreach (var stage in group.stages)
                {
                    var names = stage.steps.Where(s => s != null).Select(s => s.Title).ToList();
                    if (names.Count == 0)
                        continue;
                    text.AppendLine(stage.activation == StageActivation.Sequential
                        ? "   по очереди: " + string.Join(" → ", names)
                        : "   одновременно: " + string.Join(", ", names));
                }
            }
            return text.ToString();
        }
    }
}
