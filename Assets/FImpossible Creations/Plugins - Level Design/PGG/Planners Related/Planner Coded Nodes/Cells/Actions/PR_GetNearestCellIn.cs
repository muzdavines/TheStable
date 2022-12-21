using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells.Actions
{
    public class PR_GetNearestCellIn : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Get Nearest Cell In" : "Get Nearest Cell In Position"; }
        public override string GetNodeTooltipDescription { get { return "Trying to find nearest cell to provided world position"; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 263 : 238, 122); } }
        public override bool IsFoldable { get { return true; } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }

        [Port(EPortPinType.Input)] public PGGPlannerPort InPlanner;
        [Port(EPortPinType.Input)] public PGGVector3Port WorldPosition;
        [Port(EPortPinType.Output)] public PGGCellPort Cell;


        public override void OnStartReadingNode()
        {
            Cell.Clear();

            InPlanner.TriggerReadPort(true);
            var fieldA = GetPlannerFromPort(InPlanner, false);

            if (fieldA == null) return;

            CheckerField3D chA = fieldA.LatestChecker;
            if (chA == null || chA.ChildPositionsCount < 1) return;

            WorldPosition.TriggerReadPort(true);
            Vector3 pos = WorldPosition.GetInputValue;

            var cell = chA.GetNearestCellInWorldPos(pos);
            Cell.ProvideFullCellData(cell, chA, fieldA.LatestResult);
        }
    }
}