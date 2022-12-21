using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Shape
{
    public class PR_ShapeSubtract : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Subtract Shape Cells"; }
        public override string GetNodeTooltipDescription { get { return "Removing cells of one shape and outputting resulting shape"; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(230, 124); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        [Port(EPortPinType.Input, 1)] public PGGPlannerPort From;
        [Port(EPortPinType.Input, 1)] public PGGPlannerPort ToRemove;
        [Port(EPortPinType.Output)] public PGGPlannerPort Result;

        public override void OnStartReadingNode()
        {
            From.TriggerReadPort(true);
            ToRemove.TriggerReadPort(true);

            var checker = From.GetInputCheckerSafe;
            var plan = GetPlannerFromPort(From, false);
            if (plan != null) checker = plan.LatestChecker;

            if (checker == null) return;
            if (checker.ChildPositionsCount == 0) return;

            var oChecker = ToRemove.GetInputCheckerSafe;
            var oplan = GetPlannerFromPort(ToRemove, false);

            if (oplan == null) return;
            if (oChecker == null) return;
            if (oChecker.ChildPositionsCount == 0) return;

            CheckerField3D nChecker = checker.Copy();
            nChecker.RemoveCellsCollidingWith(oChecker);

            Result.ProvideShape(nChecker);
        }
    }
}