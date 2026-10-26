using UnityEditor;

namespace ScenarioKit.Editor
{
    [CustomEditor(typeof(ModeProfile))]
    sealed class ModeProfileEditor : UnityEditor.Editor
    {
        bool showPlan = true;

        public override void OnInspectorGUI()
        {
            var mode = (ModeProfile)target;
            var scenario = ScenarioEditorUtility.FindScenarioOf(mode);

            ScenarioEditorUtility.DrawProblems(ScenarioValidator.Validate(mode, scenario),
                scenario != null ? $"Режим настроен корректно (сценарий «{scenario.Title}»)." : "Режим настроен корректно.");
            if (scenario == null)
                EditorGUILayout.HelpBox("Режим не добавлен ни в один сценарий — его не будет в меню.", MessageType.None);

            DrawDefaultInspector();

            EditorGUILayout.Space();
            showPlan = EditorGUILayout.Foldout(showPlan, "Схема активации", true);
            if (showPlan)
                EditorGUILayout.HelpBox(ScenarioEditorUtility.DescribePlan(mode), MessageType.None);
        }
    }
}
