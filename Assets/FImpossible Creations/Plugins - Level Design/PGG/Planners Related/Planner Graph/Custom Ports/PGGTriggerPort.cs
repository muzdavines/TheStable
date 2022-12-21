using FIMSpace.Generating.Planning;
using FIMSpace.Generating.Planning.PlannerNodes;
using System;
using UnityEngine;

namespace FIMSpace.Graph
{
    [System.Serializable]
    public class PGGTriggerPort : NodePortBase
    {
        public override System.Type GetPortValueType { get { return null; } }
        public override object DefaultValue { get { return null; } }
        public override void Refresh(FGraph_NodeBase parentNode)
        {
            IsSendingSignals = true;
            base.Refresh(parentNode);
        }

        public override bool AllowConnectionWithValueType(IFGraphPort other)
        {
            return true;
        }

        public override bool CanConnectWith(IFGraphPort toPort)
        {
            NodePortBase port = toPort as NodePortBase;
            if (port != null) if (port.ParentNode) if (port.ParentNode.DrawInputConnector == false) return false;
            return true;
        }

        public override Color GetColor()
        {
            return new Color(.6f, .6f, .6f, 1f);
        }

        internal void Execute( FieldPlanner planner, PlanGenerationPrint print)
        {
            Execute(Connections, planner, print);
        }

        internal static void Execute(System.Collections.Generic.List<PortConnection> Connections, FieldPlanner planner, PlanGenerationPrint print)
        {
            if (Connections.Count == 0) return;

            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i] == null) continue;
                PlannerRuleBase node = Connections[i].NodeReference as PlannerRuleBase;

                if (node == null) continue;
                if (node.DrawInputConnector == false) { UnityEngine.Debug.Log("Connect with non executable node!"); continue; }

                planner.CallExecution(node, print);
            }
        }
    }
}