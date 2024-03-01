using Arch.Unity.Conversion;
using UnityEditor;
using UnityEngine;

namespace Arch.Unity.Editor
{
    [CustomEditor(typeof(EntityConverter))]
    [CanEditMultipleObjects]
    public class EntityConverterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (Application.isPlaying)
            {
                if (((EntityConverter)target).IsEntityAlive())
                {
                    EditorGUILayout.HelpBox("This GameObject is synchronized with Entity.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("The Entity has been destroyed.", MessageType.Warning);
                }
            }

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                var modeProperty = serializedObject.FindProperty("conversionMode");
                EditorGUILayout.PropertyField(modeProperty);
                if (modeProperty.enumValueIndex == 1)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("convertHybridComponents"));
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
