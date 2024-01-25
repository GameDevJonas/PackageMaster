using System.Collections.Generic;
using UnityEngine;

namespace GDPanda.BanterForge.Tree
{
    public abstract class MultipleOutputNode : Node
    {
        [HideInInspector]
        public List<Node> children = new List<Node>();
    }
}