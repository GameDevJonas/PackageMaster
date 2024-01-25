using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GDPanda.BanterForge.Tree;
using UnityEngine;

namespace GDPanda.BanterForge.Tree
{
    public class ChoiceNode : MultipleOutputNode
    {
        private int playerDecisionIndex = -1;

        public override void ResetNode()
        {
            base.ResetNode();
            playerDecisionIndex = -1;
        }

        //TODO: Await player decision!
        protected override Node OnRun()
        {
            if (children.Count <= 0)
            {
                return null;
            }
            
            if (playerDecisionIndex < 0)
            {
                hasBeenRun = false;
                return this;
            }

            Debug.Log($"<color=#1B9AAA><b>Choice node progressing with choice {playerDecisionIndex}</b></color>");
            
            var child = children.FirstOrDefault(x => playerDecisionIndex == x.connectedIndex);
            return child;
        }

        public void OnPlayerHasMadeDecision(int index)
        {
            playerDecisionIndex = -1;

            Node matchingChildIndex = null;
            if (children.Any(childNode => index == childNode.connectedIndex))
            {
                Debug.Log($"<color=#1B9AAA><i>Choice node has gotten info on player choice with index {index}</i></color>");
                playerDecisionIndex = index;
                return;
            }
            
            Debug.Log($"<color=red>No child assigned at index:{index} on choice node; {nodeName}</color>");
        }
        
        protected override void OnCleanup()
        {
            
        }
    }
}