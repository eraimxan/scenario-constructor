using UnityEditor;

namespace ScenarioKit.Editor
{
    [CustomEditor(typeof(ScenarioDefinition))]
    sealed class ScenarioDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            ScenarioEditorUtility.DrawProblems(ScenarioValidator.Validate((ScenarioDefinition)target),
                "Сценарий настроен корректно.");
            DrawDefaultInspector();
        }
    }
}
