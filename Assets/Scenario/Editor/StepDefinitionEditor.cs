using System.Linq;
using UnityEditor;

namespace ScenarioKit.Editor
{
    /// <summary>Shows step problems and which modes reuse the step.</summary>
    [CustomEditor(typeof(StepDefinition), true)]
    sealed class StepDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var step = (StepDefinition)target;
            ScenarioEditorUtility.DrawProblems(step.Validate(), "Шаг настроен корректно.");

            var modes = ScenarioEditorUtility.FindAssets<ModeProfile>()
                .Where(m => m.AllSteps.Contains(step))
                .Select(m => m.DisplayName)
                .ToList();
            EditorGUILayout.HelpBox(modes.Count > 0
                    ? "Используется в режимах: " + string.Join(", ", modes)
                    : "Шаг не используется ни в одном режиме.",
                MessageType.None);

            DrawDefaultInspector();
        }
    }
}
