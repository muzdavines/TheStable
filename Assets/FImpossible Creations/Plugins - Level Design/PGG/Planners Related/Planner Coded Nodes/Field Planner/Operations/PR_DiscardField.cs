using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Operations
{

    public class PR_DiscardField : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Discard Field"; }
        public override string GetNodeTooltipDescription { get { return "Discarding field or field duplicate from further execution and from being generated"; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(168, 79); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return false; } }

        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideOnConnected, 1)] public PGGPlannerPort Planner;


        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            var planner = GetPlannerFromPort(Planner);
            if (planner != null)
            {
                planner.Discard(print);
            }
        }

#if UNITY_EDITOR
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            Planner.DisplayVariableName = false;
            base.Editor_OnNodeBodyGUI(setup);
        }
#endif

    }
}