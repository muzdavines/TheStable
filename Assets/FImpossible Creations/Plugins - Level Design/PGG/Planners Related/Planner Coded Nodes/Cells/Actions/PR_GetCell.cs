using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells.Actions
{

    public class PR_GetCell : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Get Cell"; }
        public override string GetNodeTooltipDescription { get { return "Getting cell of choosed field using world space position"; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 240 : 188, 124); } }
        public override bool IsFoldable { get { return true; } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override EPlannerNodeType NodeType
        {
            get
            {
                return EPlannerNodeType.ReadData;
            }
        }

        [Port(EPortPinType.Input, 1)] public PGGPlannerPort Owner;
        [Port(EPortPinType.Input, 1)] public PGGVector3Port WorldPos;
        [Port(EPortPinType.Output)] public PGGCellPort Cell;

        public override void OnStartReadingNode()
        {
            Cell.Clear();
            WorldPos.TriggerReadPort(true);
            var pos = WorldPos.GetPortValue;
            if (pos == null) return;

            Owner.TriggerReadPort(true);

            var planner = GetPlannerFromPort(Owner);
            if (planner == null) return;

            CheckerField3D checker = planner.LatestChecker;
            FieldCell cell = checker.GetCellInWorldPos((Vector3)pos);

            if (FGenerators.CheckIfIsNull(cell)) return;
            Cell.ProvideFullCellData(cell, checker, planner.LatestResult);
        }

    }
}