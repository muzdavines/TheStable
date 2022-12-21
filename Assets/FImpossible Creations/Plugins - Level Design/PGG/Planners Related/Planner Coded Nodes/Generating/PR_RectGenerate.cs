using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.Generating
{

    public class PR_RectGenerate : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Rect Generate"; }
        public override string GetNodeTooltipDescription { get { return "Simple rect-shape generator.\nYou can use 'Join Shape Cells' node to apply line shape to the field. Expanding rect in right->up direction."; } }

        public override bool IsFoldable { get { return false; } }
        public override Vector2 NodeSize { get { return new Vector2(212, 160); } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.7f, .9f, 0.95f); }

        [Port(EPortPinType.Input)] public IntPort Width;
        [Port(EPortPinType.Input)] public IntPort Height;
        [Port(EPortPinType.Input)] public IntPort Depth;
        [Port(EPortPinType.Input)] public BoolPort CenterOrigin;

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.CellsManipulation; } }

        [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGPlannerPort RectShape;

        public override void OnCreated()
        {
            base.OnCreated();
            Width.Value = 3;
            Depth.Value = 4;
            Height.Value = 1;
        }

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Width.TriggerReadPort(true);
            Height.TriggerReadPort(true);
            Depth.TriggerReadPort(true);
            CenterOrigin.TriggerReadPort(true);

            RectShape.JustCheckerContainer = true;

            CheckerField3D rectChecker = new CheckerField3D();
            CheckerField3D myChecker = ParentPlanner.LatestChecker;
            rectChecker.CopyParamsFrom(myChecker);
            rectChecker.SetSize(Width.GetInputValue, Mathf.Max(1, Height.GetInputValue), Depth.GetInputValue);
            if (CenterOrigin.GetInputValue) rectChecker.CenterizeOrigin();

            //UnityEngine.Debug.Log("cells = " + rectChecker.ChildPositionsCount);
            //rectChecker.DebugLogDrawCellsInWorldSpace(Color.red);
            RectShape.ProvideShape(rectChecker);
        }
    }
}