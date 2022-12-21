using FIMSpace.Generating.Planner.Nodes;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes
{
    public enum EType
    {
        Int, Bool, Number, Vector3, String,
        Cell
    }

    public class PE_Start : PGGPlanner_ExecutionNode
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Procedures Start"; }
        public override string GetNodeTooltipDescription { get { return "Initial node for calling execution in node graph"; } }
        public override Color GetNodeColor() { return new Color(0.3f, .9f, 0.75f, 1f); }
        public override bool DrawInputConnector { get { return false; } }
        public override EPlannerNodeVisibility NodeVisibility { get { return EPlannerNodeVisibility.JustFunctions; } }
    }
}