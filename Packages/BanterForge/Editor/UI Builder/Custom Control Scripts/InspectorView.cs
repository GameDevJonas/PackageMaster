#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GDPanda.BanterForge.Tree.Editor
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits> { }

        private UnityEditor.Editor _editorInstance;
        
        public InspectorView()
        {
            
        }

        public void UpdateSelection(NodeView nodeView)
        {
            Clear();
            
            UnityEngine.Object.DestroyImmediate(_editorInstance);
            _editorInstance = UnityEditor.Editor.CreateEditor(nodeView.node);
            IMGUIContainer container = new IMGUIContainer(() => { _editorInstance?.OnInspectorGUI(); });
            Add(container);
        }
    }
}
#endif
