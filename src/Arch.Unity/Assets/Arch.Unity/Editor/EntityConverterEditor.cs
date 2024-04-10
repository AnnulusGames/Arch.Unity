using Arch.Unity.Conversion;
using UnityEditor;
using UnityEngine;

namespace Arch.Unity.Editor
{
    [CustomEditor(typeof(EntityConverter))]
    [CanEditMultipleObjects]
    public sealed class EntityConverterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var optionsProperty = serializedObject.FindProperty("options");

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                var modeProperty = optionsProperty.FindPropertyRelative("conversionMode");
                EditorGUILayout.PropertyField(modeProperty);
                if (modeProperty.enumValueIndex == 1)
                {
                    EditorGUILayout.PropertyField(optionsProperty.FindPropertyRelative("convertHybridComponents"));
                    EditorGUILayout.PropertyField(optionsProperty.FindPropertyRelative("useDisabledComponent"));
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
