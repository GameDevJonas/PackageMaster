#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GDPanda.BanterForge.Tree.Editor
{
    public class MyPort : Port
    {
        private class DefaultEdgeConnectorListener : IEdgeConnectorListener
        {
            private GraphViewChange m_GraphViewChange;
            private List<Edge> m_EdgesToCreate;
            private List<GraphElement> m_EdgesToDelete;
            
            public DefaultEdgeConnectorListener()
            {
                this.m_EdgesToCreate = new List<Edge>();
                this.m_EdgesToDelete = new List<GraphElement>();
                this.m_GraphViewChange.edgesToCreate = this.m_EdgesToCreate;
            }
       
            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
            }
       
            public void OnDrop(UnityEditor.Experimental.GraphView.GraphView graphView, Edge edge)
            {
                this.m_EdgesToCreate.Clear();
                this.m_EdgesToCreate.Add(edge);
                this.m_EdgesToDelete.Clear();
                if (edge.input.capacity == Port.Capacity.Single)
                {
                    foreach (Edge connection in edge.input.connections)
                    {
                        if (connection != edge)
                            this.m_EdgesToDelete.Add((GraphElement) connection);
                    }
                }
                if (edge.output.capacity == Port.Capacity.Single)
                {
                    foreach (Edge connection in edge.output.connections)
                    {
                        if (connection != edge)
                            this.m_EdgesToDelete.Add((GraphElement) connection);
                    }
                }
                if (this.m_EdgesToDelete.Count > 0)
                    graphView.DeleteElements((IEnumerable<GraphElement>) this.m_EdgesToDelete);
                List<Edge> edgesToCreate = this.m_EdgesToCreate;
                if (graphView.graphViewChanged != null)
                    edgesToCreate = graphView.graphViewChanged(this.m_GraphViewChange).edgesToCreate;
                foreach (Edge edge1 in edgesToCreate)
                {
                    graphView.AddElement((GraphElement) edge1);
                    edge.input.Connect(edge1);
                    edge.output.Connect(edge1);
                }
            }
        }
       
        public Action<MyPort> OnConnect;
        public Action<MyPort> OnDisconnect;
        
        public NodeView nodeView;
       
        protected MyPort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type, NodeView view) : base(portOrientation, portDirection, portCapacity, type)
        {
            
        }

        
        public override void Connect(Edge edge)
        {
            base.Connect(edge);
           
            OnConnect?.Invoke(edge.output as MyPort);
        }
 
        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);
            OnDisconnect?.Invoke(edge.output as MyPort);
        }
 
        public override void DisconnectAll()
        {
            base.DisconnectAll();
            OnDisconnect?.Invoke(this);
        }
       
        public new static MyPort Create<TEdge>(
            Orientation orientation,
            Direction direction,
            Port.Capacity capacity,
            System.Type type,
            NodeView view)
            where TEdge : Edge, new()
        {
            var listener = new MyPort.DefaultEdgeConnectorListener();
            var ele = new MyPort(orientation, direction, capacity, type, view)
            {
                m_EdgeConnector = (EdgeConnector) new EdgeConnector<TEdge>((IEdgeConnectorListener) listener),
                nodeView = view
            };
            ele.AddManipulator((IManipulator) ele.m_EdgeConnector);
            return ele;
        }
    }
}
#endif
