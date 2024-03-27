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

            var optionsProperty = serializedObject.FindProperty("options");

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                var modeProperty = optionsProperty.FindPropertyRelative("conversionMode");
                EditorGUILayout.PropertyField(modeProperty);
                if (modeProperty.enumValueIndex == 1)
                {
                    EditorGUILayout.PropertyField(optionsProperty.FindPropertyRelative("convertHybridComponents"));
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
