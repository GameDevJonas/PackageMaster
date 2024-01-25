#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GDPanda.BanterForge.Tree.Editor
{
    public class DialogueTreeView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<DialogueTreeView, GraphView.UxmlTraits> { }

        public Action<NodeView> OnNodeSelected;
        private DialogueTree _tree;

        public NodeView activeNodeView;
        
        private Vector2 _lastMousePosition;
        
        public DialogueTreeView()
        {
            Insert(0, new GridBackground());
            
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            
            var styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/gdpanda.banterforge/Editor/UI Builder/DialogueTreeEditor.uss");
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += OnUndoRedo;
        }
        
        private void OnUndoRedo()
        {
            PopulateView(_tree);
            AssetDatabase.SaveAssets();
        }

        public void OnActiveNodeChanged(Node newActiveNode)
        {
            if (activeNodeView != null)
            {
                var activeBorder = activeNodeView.Q("active-border");
                activeBorder.style.display = DisplayStyle.None;
            }
            
            if(newActiveNode == null)
                return;

            var foundNodeView = FindNodeView(newActiveNode);
            
            if (foundNodeView != null)
            {
                activeNodeView = foundNodeView;
                
                var activeBorder = activeNodeView.Q("active-border");
                activeBorder.style.display = DisplayStyle.Flex;
            }
        }
        
        private NodeView FindNodeView(Node node)
        {
            if (node == null)
                return null;
            
            return GetNodeByGuid(node.guid) as NodeView;
        }

        public bool PopulateView(DialogueTree wndActiveTree)
        {
            if(!wndActiveTree)
                return false;
            
            _tree = wndActiveTree;

            _tree.CurrentNodeChanged -= OnActiveNodeChanged;
            _tree.CurrentNodeChanged += OnActiveNodeChanged;
            
            graphViewChanged -= OnGraphViewChanged; 
            
            DeleteElements(graphElements);

            if (_tree.rootNode == null)
            {
                _tree.rootNode = _tree.CreateNode(typeof(RootNode)) as RootNode;
                EditorUtility.SetDirty(_tree);
                AssetDatabase.SaveAssets();
            }
            
            //Creates node view
            _tree.nodes.ForEach(n =>
            {
                CreateNodeView(n/*, n.position*/);
            });

            //Create edge view
            _tree.nodes.ForEach(n =>
            {
                var children = _tree.GetChildren(n);
                for (int c = 0; c < children.Count; c++)
                {
                    NodeView parentView = FindNodeView(n);
                    NodeView childView = FindNodeView(children[c]);
                    
                    if(childView == null || parentView == null)
                        continue;
                    
                    var index = childView.node.connectedIndex;
                    // Debug.Log(childView.node.connectedIndex);
                    if(index < 0)
                        continue;
                    
                    Edge edge = parentView.output[index].ConnectTo(childView.input);
                    AddElement(edge);
                }
            });
            
            
            graphViewChanged += OnGraphViewChanged;
            return true;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node).ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.elementsToRemove != null)
            {
                foreach (var elementToRemove in change.elementsToRemove)
                {
                    Edge edge = elementToRemove as Edge;
                    if (edge != null)
                    {
                        NodeView parentView = edge.output.node as NodeView;
                        NodeView childView = edge.input.node as NodeView;
                        if(parentView == null || childView == null)
                            continue;
                        
                        _tree.RemoveChild(parentView.node, childView.node);
                    }

                    NodeView nodeView = elementToRemove as NodeView;
                    if (nodeView != null)
                    {
                        _tree.DeleteNode(nodeView.node);
                    }
                }
            }

            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;
                    if(parentView == null || childView == null)
                        continue;
                    
                    _tree.AddChild(parentView.node, childView.node);
                }
            }
            
            return change;
        }

        private void CreateNodeView(Node node)
        {
            var isRootNode = node is RootNode;
            if (!isRootNode && node.position == Vector2.zero)
            {
                node.position.x = _lastMousePosition.x;
                node.position.y = _lastMousePosition.y;
            }
            
            NodeView nodeView = new NodeView(node);
            
            if (node is RootNode)
            {
                if(string.IsNullOrEmpty(node.nodeName))
                    node.nodeName = "Root";
            }
            else if (node is DialogueNode)
            {
                if(string.IsNullOrEmpty(node.nodeName))
                    node.nodeName = "Dialogue";
            }
            else if (node is ChoiceNode)
            {
                if(string.IsNullOrEmpty(node.nodeName))
                    node.nodeName = "Choice";
            }
            
            /*if (node.position == Vector2.zero)
            {
                nodeView.transform.position = position;
            }*/
            
            nodeView.OnNodeSelected = OnNodeSelected;
            AddElement(nodeView);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            _lastMousePosition = viewTransform.matrix.inverse.MultiplyPoint(evt.localMousePosition);
            
            var actionTypes = TypeCache.GetTypesDerivedFrom<ActionNode>();
            foreach (var actionNodeType in actionTypes)
            {
                evt.menu.AppendAction($"[{actionNodeType.BaseType.Name}], {actionNodeType.Name}", (_) => CreateNode(actionNodeType/*, _lastMousePosition*/));
            }
            
            var decoratorTypes = TypeCache.GetTypesDerivedFrom<SingleOutputNode>();
            foreach (var decoratorType in decoratorTypes)
            {
                evt.menu.AppendAction($"[{decoratorType.BaseType.Name}], {decoratorType.Name}", (_) => CreateNode(decoratorType/*, _lastMousePosition*/));
            }
            
            var compositeTypes = TypeCache.GetTypesDerivedFrom<MultipleOutputNode>();
            foreach (var compositeType in compositeTypes)
            {
                evt.menu.AppendAction($"[{compositeType.BaseType.Name}], {compositeType.Name}", (_) => CreateNode(compositeType/*, _lastMousePosition*/));
            }
        }

        private void CreateNode(System.Type type)
        {
            var node = _tree.CreateNode(type);
            CreateNodeView(node);
        }
    }
}
#endif
