using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Shape
{

    public class PR_RemoveAllFieldCells : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Remove All Field Cells"; }
        public override string GetNodeTooltipDescription { get { return "Remove All Field Cells."; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override bool IsFoldable { get { return false; } }

        public override Vector2 NodeSize { get { return new Vector2(190, 84); } }

        [Port(EPortPinType.Input, 1)] public PGGPlannerPort Planner;
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.CellsManipulation; } }

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Planner.TriggerReadPort(true);

            FieldPlanner plan = GetPlannerFromPort(Planner, false);
            CheckerField3D myChe = Planner.GetInputCheckerSafe;
            if (plan) myChe = plan.LatestResult.Checker;
            if (myChe == null) { return; }

            myChe.ClearAllCells();
            if (plan) plan.LatestResult.Checker = myChe;
        }

    }
}