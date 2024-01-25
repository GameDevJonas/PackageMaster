#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

namespace GDPanda.BanterForge.Tree.Editor
{
    /// <summary>
    /// https://www.youtube.com/watch?v=nKpM98I7PeM
    /// </summary>
    public class DialogueTreeEditor : EditorWindow
    {
        public DialogueTree activeTree;
        
        private DialogueTreeView _treeView;
        private InspectorView _inspectorView;
        private VisualElement _errorElement;
        private UnityEngine.UIElements.Button _button;
        private NodeView _selectedNode;
            
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;


        [MenuItem("Dialogue System/Editor ...")]
        public static void OpenWindow()
        {
            DialogueTreeEditor wnd = GetWindow<DialogueTreeEditor>();
            wnd.titleContent = new GUIContent("Empty Tree");
        }
        
        public static void OpenWindowFromTree(DialogueTree tree)
        {
            DialogueTreeEditor wnd = GetWindow<DialogueTreeEditor>();
            wnd.titleContent = new GUIContent($"{tree.name} EDITOR");

            wnd.activeTree = tree;
            wnd.InitializeWindow();
        }
        
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            DialogueTree tree = EditorUtility.InstanceIDToObject(instanceID) as DialogueTree;
            if (tree != null)
            {
                DialogueTreeEditor.OpenWindowFromTree(tree);
                return true;
            }
            return false;
        }
        
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            m_VisualTreeAsset.CloneTree(root);

            var styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>(
                    "Assets/Editor/UI Builder/DialogueTreeEditor.uss");
            root.styleSheets.Add(styleSheet);

            InitializeWindow();
        }

        public void InitializeWindow()
        {
            VisualElement root = rootVisualElement;

            _treeView = root.Q<DialogueTreeView>();
            _inspectorView = root.Q<InspectorView>();
            _errorElement = FindVisualElementChildByName(_treeView, "error-visual");
            _button = root.Q<UnityEngine.UIElements.Button>();

            _button.clicked -= OnToolbarButtonClick;
            _button.clicked += OnToolbarButtonClick;
            
            // _errorElement = _treeView.Children().Where(x => x.name == "error-visual").ToList()[0];

            _treeView.OnNodeSelected = OnNodeSelectionChanged;
            var success = _treeView.PopulateView(activeTree);
            ShowError(!success);
            if(!success)
                return;

            var treeLabel = FindVisualElementChildByName(root.Q<SplitView>(), "treeview-label") as Label;
            if(treeLabel != null)
                treeLabel.text = $"Tree View: {activeTree.name}";
        }
        
        private void OnToolbarButtonClick()
        {
            var nextActiveNode = FindObjectOfType<DialogueTreeRunner>().ProgressDebugLogTree();
            _treeView.OnActiveNodeChanged(nextActiveNode);
        }

        public void ShowError(bool show)
        {
            _errorElement.visible = show;
            foreach (var element in _errorElement.Children())
            {
                element.visible = show;
            }
        }

        private void OnNodeSelectionChanged(NodeView nodeView)
        {
            if (_selectedNode != null)
            {
                _selectedNode.OnNodeNameChanged();
            }
            
            if(nodeView == null)
                return;
            
            // _treeView.PopulateView(activeTree);
            _selectedNode = nodeView;
            _selectedNode.OnNodeNameChanged();
            _inspectorView.UpdateSelection(nodeView);
            Repaint();
        }

        public static VisualElement FindVisualElementChildByName(VisualElement parent, string name)
        {
            var children = parent.Children();
            foreach (var visualElement in children)
            {
                if (visualElement.name == name)
                    return visualElement;
            }

            foreach (var child in children)
            {
                var foundOnChild = FindVisualElementChildByName(child, name);
                if (foundOnChild != null)
                    return foundOnChild;
            }
            
            return null;
        }
    }
}
#endif
