using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GDPanda.BanterForge.Tree.Editor
{
    [CustomEditor(typeof(DialogueNode))]
    public class DialogueNodeInspector : UnityEditor.Editor
    {
        private bool _expandedInfoBox = false;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);
            // GUILayout.BeginHorizontal();
            _expandedInfoBox = EditorGUILayout.BeginFoldoutHeaderGroup(_expandedInfoBox, "Info");
            if (_expandedInfoBox)
            {
                GUILayout.BeginVertical();
                EditorGUILayout.HelpBox("Test \n" +
                                        "wah", MessageType.Info);
                GUILayout.EndVertical();

            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            // GUILayout.EndHorizontal();
            
        }
    }
}