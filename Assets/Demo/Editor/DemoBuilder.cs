using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ScenarioKit.Demo.Editor
{
    /// <summary>
    /// Generates the demo scenario assets and scene. The committed scene was produced by this builder;
    /// it is kept to show how the demo is assembled and to restore it.
    /// </summary>
    static class DemoBuilder
    {
        public const string ScenePath = "Assets/Demo/Scenes/Demo.unity";

        [MenuItem("Tools/Scenario/Пересобрать демо-сцену")]
        static void RebuildFromMenu()
        {
            if (EditorUtility.DisplayDialog("Пересобрать демо?",
                    "Демо-сцена и ассеты в Assets/Demo/Scenario будут созданы заново, ручные изменения в них пропадут.",
                    "Пересобрать", "Отмена"))
                Build();
        }

        /// <summary>Entry point for -executeMethod ScenarioKit.Demo.Editor.DemoBuilder.Build.</summary>
        public static void Build()
        {
            EnsureTag("Box");
            // The scene goes first: opening a scene unloads unused assets and would invalidate fresh asset references.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var assets = DemoAssets.Create();
            var world = DemoWorld.Build(assets);

            var runnerObject = new GameObject("Scenario Runner");
            var runner = runnerObject.AddComponent<ScenarioRunner>();
            DemoWiring.Set(runner, "scenario", assets.Scenario);
            DemoWiring.SetList(runner, "loggers", assets.Loggers);

            DemoInterface.Build(assets, world, runner);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log($"[Scenario] Демо собрано: {ScenePath}");
        }

        static void EnsureTag(string tag)
        {
            var manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tags = manager.FindProperty("tags");
            for (int i = 0; i < tags.arraySize; i++)
                if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                    return;
            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            manager.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
