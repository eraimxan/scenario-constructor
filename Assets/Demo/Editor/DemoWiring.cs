using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScenarioKit.Demo.Editor
{
    /// <summary>Fills private serialized fields exactly like dragging references in the inspector.</summary>
    static class DemoWiring
    {
        public static void Set(Object target, string property, Object value)
        {
            var serialized = new SerializedObject(target);
            Find(serialized, property).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void Set(Object target, string property, string value)
        {
            var serialized = new SerializedObject(target);
            Find(serialized, property).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetList<T>(Object target, string property, IReadOnlyList<T> values) where T : Object
        {
            var serialized = new SerializedObject(target);
            var list = Find(serialized, property);
            list.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static SerializedProperty Find(SerializedObject serialized, string property) =>
            serialized.FindProperty(property)
            ?? throw new System.ArgumentException($"{serialized.targetObject.GetType().Name} has no serialized field '{property}'.");
    }
}
