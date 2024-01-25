using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GDPanda.BanterForge.Tree
{
    public class DialogueTreeRunner : MonoBehaviour
    {
        [SerializeField]
        private DialogueTree tree;

        private bool _lookingForChoice;

        /*private void Start()
        {
            tree = ScriptableObject.CreateInstance<DialogueTree>();
        }*/

        public void ResetTree()
        {
            tree.ResetNodes();
            tree.currentNode = null;
        }
        
        public Node ProgressTree()
        {
            var node = CheckForNode(false);
            return node;
        }

        public Node ProgressDebugLogTree()
        {
            return CheckForNode(true);
        }

        private Node CheckForNode(bool fromEditorDebug)
        {
            var node = tree.Progress();
            if (!node)
            {
                Debug.Log($"<color=#FFC43D><b>Arrived at null node, resetting tree</b></color>");
                _lookingForChoice = false;
                tree.ResetNodes();
                return null;
            }
            
            var dialogueNode = node as DialogueNode;
            if (dialogueNode != null)
            {
                Debug.Log($"<i>Node is dialogue node; {dialogueNode.nodeName}</i>");
                Debug.Log(
                    $"<color=#EF476F>Dialogue: \"{dialogueNode.dialogueContent.character?.Name}: {dialogueNode.dialogueContent.dialogueLine}\"</color>");
                _lookingForChoice = false;
                return dialogueNode;
            }

            var choiceNode = node as ChoiceNode;
            if (choiceNode != null)
            {
                if (_lookingForChoice)
                {
                    if (fromEditorDebug)
                    {
                        OnPlayerMadeDecision(Random.Range(0, 5));
                        return ProgressTree();
                    }

                    return choiceNode;
                }
                else
                {
                    _lookingForChoice = true;
                    Debug.Log($"<b>Node is choice node, awaiting response; {choiceNode.nodeName}</b>");
                }

                return choiceNode;
            }

            Debug.Log($"<color=red>Node is not dialogue or choice?; {node.nodeName}</color>");
            return node.Progress();
        }

        public void OnPlayerMadeDecision(int decisionIndex)
        {
            if (!_lookingForChoice)
            {
                Debug.Log("<color=red>Tried calling OnPlayerMadeDecision without declaring that runner is looking for decision</color>");
                return;
            }

            var choiceNode = tree.currentNode as ChoiceNode;
            if (choiceNode == null)
            {
                Debug.Log("<color=red>Tried calling OnPlayerMadeDecision when trees active node is not choice node</color>");
                return;
            }

            choiceNode.OnPlayerHasMadeDecision(decisionIndex);
            
            _lookingForChoice = false;
        }
    }
}