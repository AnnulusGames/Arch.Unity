using UnityEditor;
using UnityEngine;

namespace Arch.Unity.Editor
{
    internal static class Styles
    {
        public static Color LineColor => EditorGUIUtility.isProSkin ? new(0.05f, 0.05f, 0.05f) : new(0.6f, 0.6f, 0.6f);
        public static Color ThinLineColor => EditorGUIUtility.isProSkin ? new(0.2f, 0.2f, 0.2f) : new(0.7f, 0.7f, 0.7f);

        public static readonly GUIContent GameObjectIcon = EditorGUIUtility.IconContent("GameObject Icon");
        public static readonly GUIContent AssemblyIcon = EditorGUIUtility.IconContent("Assembly Icon");
        public static readonly GUIContent ModelImporterIcon = EditorGUIUtility.IconContent("ModelImporter Icon");
    }
}