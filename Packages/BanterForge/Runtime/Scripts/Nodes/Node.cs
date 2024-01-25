using System;
using System.Collections;
using UnityEngine;

namespace GDPanda.BanterForge.Tree
{
    public abstract class Node : ScriptableObject
    {
        [HideInInspector]
        public string guid;

        [Header("Base node settings")]
        public string nodeName;
       
        [HideInInspector]
        public Vector2 position;
        
        [HideInInspector]
        public int connectedIndex;
        
        [HideInInspector]
        public bool hasBeenRun = false;

        [HideInInspector]
        public Node child;
        
        public Node Progress()
        {
            if (!hasBeenRun)
            {
                hasBeenRun = true;
                return OnRun();
            }
            
            if (child == null)
            {
                return null;
            }

            return null;
        }

        protected abstract Node OnRun();

        protected abstract void OnCleanup();

        public virtual void ResetNode()
        {
            hasBeenRun = false;
        }
    }
}