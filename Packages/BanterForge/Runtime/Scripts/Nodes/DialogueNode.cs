using System;
using System.Collections;
using System.Collections.Generic;
using GDPanda.BanterForge.Tree;
using UnityEngine;
using UnityEngine.Serialization;

namespace GDPanda.BanterForge.Tree
{
    public class DialogueNode : SingleOutputNode
    {
        [Header("Dialogue node settings")]
        public DialogueContent dialogueContent;
        
        protected override Node OnRun()
        {
            if (hasBeenRun)
                return child;
            
            return this;
        }

        protected override void OnCleanup()
        {
            hasBeenRun = false;
        }

        public override void ResetNode()
        {
            base.ResetNode();
            hasBeenRun = false;
        }
    }

    [Serializable]
    public class DialogueContent
    {
        [FormerlySerializedAs("speaker")] public Character character;
        
        [TextArea]
        public string dialogueLine;

        public bool autoNextLine;
    }
}