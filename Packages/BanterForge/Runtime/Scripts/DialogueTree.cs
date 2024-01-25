using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace GDPanda.BanterForge.Tree
{
    [CreateAssetMenu(menuName = "Dialogue System/New Dialogue Tree", fileName = "New Dialogue Tree")]
    public class DialogueTree : ScriptableObject
    {
        public Node rootNode;
        public List<Node> nodes = new List<Node>();

        [NonSerialized]
        public Node currentNode;

        public Action<Node> CurrentNodeChanged;
        
        public Node Progress()
        {
            if (!rootNode.hasBeenRun)
                currentNode = rootNode;

            if (currentNode == null)
            {
                return null;
            }
            
            var nextNode = currentNode.Progress();
            if (nextNode == currentNode) 
                return currentNode;
            
            currentNode = nextNode;
            CurrentNodeChanged?.Invoke(nextNode);

            return currentNode;
        }

#if UNITY_EDITOR
        public Node CreateNode(System.Type type)
        {
            var node = ScriptableObject.CreateInstance(type) as Node;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();
            
            Undo.RecordObject(this, "Dialogue Tree (CreateNode)");
            nodes.Add(node);
            
            AssetDatabase.AddObjectToAsset(node, this);
            Undo.RegisterCreatedObjectUndo(node, "Dialogue Tree (CreateNode)");
            AssetDatabase.SaveAssets();
            return node;
        }

        public void DeleteNode(Node node)
        {
            Undo.RecordObject(this, "Dialogue Tree (DeleteNode)");
            nodes.Remove(node);
            
            // AssetDatabase.RemoveObjectFromAsset(node);
            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
        }


        public void AddChild(Node parent, Node child)
        {
            Undo.RecordObject(parent, "Dialogue Tree (AddChild)");
            
            RootNode root = parent as RootNode;
            if (root)
            {
                root.child = child;
            }
            
            SingleOutputNode singleOutput = parent as SingleOutputNode;
            if (singleOutput)
            {
                singleOutput.child = child;
            }
            
            MultipleOutputNode multipleOutput = parent as MultipleOutputNode;
            if (multipleOutput)
            {
                multipleOutput.children.Add(child);
            }
            
            EditorUtility.SetDirty(parent);
        }

        public void RemoveChild(Node parent, Node child)
        {
            Undo.RecordObject(parent, "Dialogue Tree (RemoveChild)");

            
            RootNode root = parent as RootNode;
            if (root)
            {
                root.child = null;
            }
            
            SingleOutputNode singleOutput = parent as SingleOutputNode;
            if (singleOutput)
            {
                singleOutput.child = null;
            }
            
            MultipleOutputNode multipleOutput = parent as MultipleOutputNode;
            if (multipleOutput)
            {
                multipleOutput.children.Remove(child);
            }
            
            EditorUtility.SetDirty(parent);
        }
#endif

        public List<Node> GetChildren(Node parent)
        {
            List<Node> children = new List<Node>();
            
            SingleOutputNode singleOutput = parent as SingleOutputNode;
            if (singleOutput && singleOutput.child != null)
            {
                children.Add(singleOutput.child);
            }
            
            RootNode root = parent as RootNode;
            if (root && root.child != null)
            {
                children.Add(root.child);
            }
            
            MultipleOutputNode multipleOutput = parent as MultipleOutputNode;
            if (multipleOutput)
            {
                return multipleOutput.children;
            }

            return children;
        }

        public void ResetNodes()
        {
            foreach (var node in nodes)
            {
                node.ResetNode();
            }
        }
    }
}