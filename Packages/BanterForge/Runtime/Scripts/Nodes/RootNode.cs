using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GDPanda.BanterForge.Tree
{
    public class RootNode : Node
    {
        protected override Node OnRun()
        {
            return child;
        }

        protected override void OnCleanup()
        {
            
        }
    }
}