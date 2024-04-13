using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Unity.Collections;
using Arch.Unity.Conversion;

namespace Arch.Unity.Editor
{
    [CustomEditor(typeof(EntitySelectionProxy))]
    public sealed class EntityEditor : UnityEditor.Editor
    {
        static readonly Dictionary<string, bool> isExpandedDictionary = new();

        void OnEnable()
        {
            EditorApplication.update += Repaint;
        }

        void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }

        protected override void OnHeaderGUI() { }
        public override void OnInspectorGUI()
        {
            var proxy = (EntitySelectionProxy)target;
            DrawHeader(proxy);
            DrawLine(Styles.LineColor);

            foreach (var component in proxy.world.GetAllComponents(proxy.entityReference))
            {
                if (component is GameObjectReference or EntityConverter or EntityName) continue;

                var componentType = component.GetType();
                if (!isExpandedDictionary.TryGetValue(componentType.FullName, out var isExpanded))
                {
                    isExpandedDictionary.Add(componentType.FullName, true);
                    isExpanded = true;
                }

                var headerText = ObjectNames.NicifyVariableName(componentType.Name);
                var headerIconImage = component is UnityEngine.Component co
                    ? EditorGUIUtility.ObjectContent(co, componentType).image
                    : Styles.AssemblyIcon.image;
                var label = new GUIContent(headerText, headerIconImage);

                isExpanded = EditorGUILayout.Foldout(isExpanded, label, true, EditorStyles.foldoutHeader);
                if (isExpanded)
                {
                    using (new EditorGUI.DisabledScope(true))
                    using (new EditorGUI.IndentLevelScope())
                    {
                        if (component is UnityEngine.Component c)
                        {
                            EditorGUILayout.ObjectField("Value", c, componentType, true);
                        }
                        else
                        {
                            DrawMembers(component, componentType.FullName, 0);
                        }
                    }
                }
                isExpandedDictionary[componentType.FullName] = isExpanded;

                DrawLine(Styles.ThinLineColor);
            }
        }

        static void DrawHeader(EntitySelectionProxy selectionProxy)
        {
            EditorGUI.indentLevel--;
            var tmp = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 20f;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Styles.GameObjectIcon, GUILayout.Height(27f), GUILayout.Width(33f));

                using (new EditorGUI.DisabledScope(true))
                using (new EditorGUILayout.VerticalScope())
                {
                    var entityReference = selectionProxy.entityReference;
                    var hasGameObject = selectionProxy.world.TryGet(entityReference, out GameObjectReference gameObjectReference);
                    var hasName = selectionProxy.world.TryGet(entityReference, out EntityName entityName);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.TextField(hasName ? entityName.ToString() : $"Entity({entityReference.Entity.Id}:{entityReference.Version})");

                        EditorGUI.indentLevel++;
                        EditorGUIUtility.labelWidth = 15f;
                        EditorGUILayout.IntField("Id", entityReference.Entity.Id, GUILayout.Width(80f));
                        EditorGUIUtility.labelWidth = 45f;
                        EditorGUILayout.IntField("Version", entityReference.Version, GUILayout.Width(90f));
                        EditorGUI.indentLevel--;
                        EditorGUIUtility.labelWidth = 20f;
                    }
                    
                    EditorGUILayout.ObjectField("From", hasGameObject ? gameObjectReference.GameObject : null, typeof(GameObject), true);
                }
            }
            EditorGUIUtility.labelWidth = tmp;
            EditorGUI.indentLevel++;
        }

        static void DrawLine(Color color)
        {
            var rect = EditorGUILayout.GetControlRect(false, 1f);
            rect.xMin -= 20f;
            rect.xMax += 5f;
            EditorGUI.DrawRect(rect, color);
        }

        static void DrawMembers(object target, string rootPath, int depth)
        {
            if (depth > 10) return;
            if (target == null) return;

            foreach (var fieldInfo in target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var value = fieldInfo.GetValue(target);
                var label = ObjectNames.NicifyVariableName(fieldInfo.Name);
                switch (value)
                {
                    case bool fieldValue:
                        EditorGUILayout.Toggle(label, fieldValue);
                        break;
                    case byte fieldValue:
                        EditorGUILayout.IntField(label, fieldValue);
                        break;
                    case sbyte fieldValue:
                        EditorGUILayout.IntField(label, fieldValue);
                        break;
                    case short fieldValue:
                        EditorGUILayout.IntField(label, fieldValue);
                        break;
                    case ushort fieldValue:
                        EditorGUILayout.IntField(label, fieldValue);
                        break;
                    case int fieldValue:
                        EditorGUILayout.IntField(label, fieldValue);
                        break;
                    case uint fieldValue:
                        EditorGUILayout.LongField(label, fieldValue);
                        break;
                    case long fieldValue:
                        EditorGUILayout.LongField(label, fieldValue);
                        break;
                    case float fieldValue:
                        EditorGUILayout.FloatField(label, fieldValue);
                        break;
                    case double fieldValue:
                        EditorGUILayout.DoubleField(label, fieldValue);
                        break;
                    case char fieldValue:
                        EditorGUILayout.TextField(label, fieldValue.ToString());
                        break;
                    case string fieldValue:
                        EditorGUILayout.TextField(label, fieldValue);
                        break;
                    case Vector2 fieldValue:
                        EditorGUILayout.Vector2Field(label, fieldValue);
                        break;
                    case Vector2Int fieldValue:
                        EditorGUILayout.Vector2IntField(label, fieldValue);
                        break;
                    case Vector3 fieldValue:
                        EditorGUILayout.Vector3Field(label, fieldValue);
                        break;
                    case Vector3Int fieldValue:
                        EditorGUILayout.Vector3IntField(label, fieldValue);
                        break;
                    case Vector4 fieldValue:
                        EditorGUILayout.Vector4Field(label, fieldValue);
                        break;
                    case Color fieldValue:
                        EditorGUILayout.ColorField(label, fieldValue);
                        break;
                    case Rect fieldValue:
                        EditorGUILayout.RectField(label, fieldValue);
                        break;
                    case RectInt fieldValue:
                        EditorGUILayout.RectIntField(label, fieldValue);
                        break;
                    case Bounds fieldValue:
                        EditorGUILayout.BoundsField(label, fieldValue);
                        break;
                    case BoundsInt fieldValue:
                        EditorGUILayout.BoundsIntField(label, fieldValue);
                        break;
                    case AnimationCurve fieldValue:
                        EditorGUILayout.CurveField(label, fieldValue);
                        break;
                    case Gradient fieldValue:
                        EditorGUILayout.GradientField(label, fieldValue);
                        break;
                    case Enum fieldValue:
                        EditorGUILayout.EnumPopup(label, fieldValue);
                        break;
                    case UnityEngine.Object fieldValue:
                        EditorGUILayout.ObjectField(label, fieldValue, fieldValue.GetType(), true);
                        break;
                    case FixedString32Bytes fieldValue:
                        EditorGUILayout.TextField(label, fieldValue.ConvertToString());
                        break;
                    case FixedString64Bytes fieldValue:
                        EditorGUILayout.TextField(label, fieldValue.ConvertToString());
                        break;
                    case FixedString128Bytes fieldValue:
                        EditorGUILayout.TextField(label, fieldValue.ConvertToString());
                        break;
                    case FixedString512Bytes fieldValue:
                        EditorGUILayout.TextField(label, fieldValue.ConvertToString());
                        break;
                    case FixedString4096Bytes fieldValue:
                        EditorGUILayout.TextField(label, fieldValue.ConvertToString());
                        break;
                    default:
                        var path = $"{rootPath}/{fieldInfo.Name}";
                        if (!isExpandedDictionary.TryGetValue(path, out var isExpanded))
                        {
                            isExpandedDictionary.Add(path, true);
                            isExpanded = true;
                        }
                        isExpanded = EditorGUILayout.Foldout(isExpanded, label, true, EditorStyles.foldoutHeader);
                        if (isExpanded)
                        {
                            using (new EditorGUI.IndentLevelScope())
                            {
                                DrawMembers(value, path, depth + 1);
                            }
                        }
                        isExpandedDictionary[path] = isExpanded;
                        break;
                }
            }
        }
    }
}