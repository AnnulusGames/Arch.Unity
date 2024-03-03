using System;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Arch.Core;

namespace Arch.Unity.Editor
{
    public class ArchHierarchyWindow : EditorWindow
    {
        [MenuItem("Window/Arch/Arch Hierarchy")]
        public static ArchHierarchyWindow Open()
        {
            var window = GetWindow<ArchHierarchyWindow>(false, "Arch Hierarchy", true);
            window.titleContent.image = EditorGUIUtility.IconContent("UnityEditor.HierarchyWindow").image;
            window.Show();
            return window;
        }

        int selectedWorldId;
        HierarchyTreeView treeView;
        TreeViewState treeViewState;

        void OnEnable()
        {
            treeViewState = new TreeViewState();
            treeView = new HierarchyTreeView(treeViewState);

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        void OnDisable()
        {
            treeView.Dispose();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            treeView.SetSelection(Array.Empty<int>());
            Repaint();
        }

        void OnGUI()
        {
            var worlds = World.Worlds.Where(x => x != null).ToDictionary(x => x.Id, x => x);
            var worldSize = worlds.Count;
            var keys = worlds.Keys.ToArray();

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (worldSize == 0)
                {
                    GUILayout.Button("No World", EditorStyles.toolbarPopup, GUILayout.Width(100f));
                }
                else
                {
                    var displayedOptions = worlds.Select(x => $"World {x.Value.Id}").ToArray();
                    var id = EditorGUILayout.IntPopup(selectedWorldId, displayedOptions, keys, EditorStyles.toolbarPopup, GUILayout.Width(100f));
                    if (id != selectedWorldId)
                    {
                        treeView.SetSelection(Array.Empty<int>());
                        selectedWorldId = id;
                    }
                }

                GUILayout.FlexibleSpace();
            }

            if (worlds.Count == 0) return;
            if (!worlds.ContainsKey(selectedWorldId))
            {
                selectedWorldId = keys.First();
            }

            treeView.SetWorld(worlds[selectedWorldId]);
            var treeViewRect = EditorGUILayout.GetControlRect(false, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            treeView.OnGUI(treeViewRect);
        }
    }
}