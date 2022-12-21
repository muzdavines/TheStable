using FIMSpace.Generating.Planner.Nodes;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.Utilities
{
    public class PR_Rewire : PGGPlanner_ExecutionNode
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Rewire"; }
        public override string GetNodeTooltipDescription { get { return "Use it to forward execution connections in more clean way inside the graph"; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.3f, 0.3f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(136, 54); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Cosmetics; } }
    }
}