#if UNITY_EDITOR
using System;
using System.Linq;
using Unity.Profiling.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GDPanda.BanterForge.Tree.Editor
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        private Label _nameLabel;

        public string Naem => node.nodeName;
        
        public Action<NodeView> OnNodeSelected;
        public Node node;
        public MyPort input;
        public MyPort[] output = new MyPort[5];
        
        private VisualElement _addedIndexLabel;
        
        public NodeView(Node node) : base("Assets/Editor/UI Builder/NodeView.uxml")
        {
            this.node = node;
            this.title = node.name;
            this.viewDataKey = this.node.guid;
            
            style.left = node.position.x;
            style.top = node.position.y;

            _nameLabel = DialogueTreeEditor.FindVisualElementChildByName(this, "title-label") as Label;
            var characterNameLabel = DialogueTreeEditor.FindVisualElementChildByName(this, "character-label") as Label;
            var diaNode = node as DialogueNode;
            if (diaNode != null)
            {
                var character = diaNode.dialogueContent.character;
                if (character)
                {
                    if (characterNameLabel != null)
                        characterNameLabel.style.display = DisplayStyle.Flex;
                    
                    var charName = diaNode.dialogueContent.character.Name;
                    var charColor = diaNode.dialogueContent.character.CharacterNodeColor;
                    
                    //make sure alpha at 70/0.2745098f
                    charColor.a = 0.2745098f;
                    
                    var outputElement = DialogueTreeEditor.FindVisualElementChildByName(this, "output") as VisualElement;
                    
                    if (characterNameLabel != null)
                        characterNameLabel.text = charName;
                    
                    if (outputElement != null)
                        outputElement.style.backgroundColor = charColor;
                }
            }
            else
            {
                if (characterNameLabel != null)
                    characterNameLabel.style.display = DisplayStyle.None;
            }
            
            if (!string.IsNullOrEmpty(node.nodeName))
            {
                _nameLabel.text = node.nodeName;
            }
            
            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
        }
        
        public void OnNodeNameChanged()
        {
            if(!node)
                return;
            
            _nameLabel.text = node.nodeName;
            
            style.left = node.position.x;
            style.top = node.position.y;
            
            var characterNameLabel = DialogueTreeEditor.FindVisualElementChildByName(this, "character-label") as Label;
            var diaNode = node as DialogueNode;
            if (diaNode != null)
            {
                var character = diaNode.dialogueContent.character;
                if (character)
                {
                    if (characterNameLabel != null)
                        characterNameLabel.style.display = DisplayStyle.Flex;
                
                    var charName = diaNode.dialogueContent.character.Name;
                    var charColor = diaNode.dialogueContent.character.CharacterNodeColor;
                
                    //make sure alpha at 70/0.2745098f
                    charColor.a = 0.2745098f;
                
                    var outputElement = DialogueTreeEditor.FindVisualElementChildByName(this, "output") as VisualElement;

                    if (characterNameLabel != null)
                        characterNameLabel.text = charName;

                    if (outputElement != null)
                        outputElement.style.backgroundColor = charColor;
                }
            }
            else
            {
                if (characterNameLabel != null)
                    characterNameLabel.style.display = DisplayStyle.None;
            }
            
            if (!string.IsNullOrEmpty(node.nodeName))
            {
                _nameLabel.text = node.nodeName;
            }

            // RefreshMyPorts();
            // this.MarkDirtyRepaint();
            // EditorUtility.SetDirty(node);
        }
        
        private void SetupClasses()
        {
            if (node is RootNode)
            {
                /*if(string.IsNullOrEmpty(node.nodeName))
                    node.nodeName = "Root";*/
                
                AddToClassList("root");
            }
            else if (node is DialogueNode)
            {
                /*if(string.IsNullOrEmpty(node.nodeName))
                    node.nodeName = "Dialogue";*/
                
                AddToClassList("dialogue");
            }
            else if (node is ChoiceNode)
            {
                /*if(string.IsNullOrEmpty(node.nodeName))
                    node.nodeName = "Choice";*/
                
                AddToClassList("choice");
            }
            /*else if (node is DecoratorNode)
            {
                AddToClassList("decorator");
            }*/
        }

        private void CreateInputPorts()
        {
            if (node is RootNode)
            {
                // input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            }
            else if (node is ActionNode)
            {
                input = InstantiateMyPort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool), this);
            }
            else if (node is MultipleOutputNode)
            {
                input = InstantiateMyPort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool), this);
            }
            else if (node is SingleOutputNode)
            {
                input = InstantiateMyPort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool), this);
            }
            
            if (input != null)
            {
                input.portName = "";
                
                input.OnConnect -= OnInputConnected;
                input.OnConnect += OnInputConnected;

                input.OnDisconnect -= OnInputDisconnected;
                input.OnDisconnect += OnInputDisconnected;
                
                // input.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(input);
            }
        }
        
        private void OnInputConnected(MyPort portConnected)
        {
            var thisName = Naem;
            var connectedNaem = portConnected.nodeView.Naem;
            // return;
            
            //Connected nodes output nodes
            var connectedViewPorts = portConnected.nodeView.output.ToList();
            var amountOfPorts = connectedViewPorts.Count(x => x != null);
            for (var index = 0; index < connectedViewPorts.Count; index++)
            {
                var port = connectedViewPorts[index];
                if (port?.node == null)
                    continue;

                var sameNode = port == portConnected;
                if (!sameNode)
                    continue;
                
                node.connectedIndex = index;
                EditorUtility.SetDirty(node);
                
                if(amountOfPorts <= 1)
                    return;

                if(_addedIndexLabel != null)
                    Remove(_addedIndexLabel);
                
                _addedIndexLabel = new Label("<b>"+node.connectedIndex);
                _addedIndexLabel.name = "index-label";
                /*_addedIndexLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                _addedIndexLabel.style.color = new StyleColor(new Color(1, 1, 1, 1));
                _addedIndexLabel.style.translate = new StyleTranslate(new Translate(50, -2));
                _addedIndexLabel.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 1));
                _addedIndexLabel.style.maxWidth = 15;*/
                
                Insert(0, _addedIndexLabel);
                return;
            }

            
        }

        private void OnInputDisconnected(MyPort _)
        {
            if (_addedIndexLabel != null)
            {
                Remove(_addedIndexLabel);
                _addedIndexLabel = null;
                // node.connectedIndex = -1;
                if(node != null)
                    EditorUtility.SetDirty(node);
            }
        }
        
        private void CreateOutputPorts()
        {
            if (node is RootNode)
            {
                output[0] = InstantiateMyPort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool), this);
            }
            else if (node is MultipleOutputNode)
            {
                for (int i = 0; i < 5; i++)
                {
                    output[i] = InstantiateMyPort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool), this);
                }
            }
            else if (node is SingleOutputNode)
            {
                output[0] = InstantiateMyPort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool), this);
            }
            
            if (output != null)
            {
                output[0].portName = "";
                // output.style.alignSelf = Align.FlexEnd;
                // output.style.flexDirection = FlexDirection.ColumnReverse;
                for (int i = 0; i < 5; i++)
                {
                    outputContainer.Add(output[i]);
                }
            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RecordObject(node, "Dialogue Tree (Set Position)");
            
            node.position.x = newPos.xMin;
            node.position.y = newPos.yMin;
            
            EditorUtility.SetDirty(node);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            OnNodeSelected?.Invoke(this);
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            OnNodeSelected?.Invoke(null);
        }
        
        private MyPort InstantiateMyPort(
            Orientation orientation,
            Direction direction,
            Port.Capacity capacity,
            System.Type type,
            NodeView view)
        {
            return MyPort.Create<Edge>(orientation, direction, capacity, type, view);
        }
    }
}
#endif
