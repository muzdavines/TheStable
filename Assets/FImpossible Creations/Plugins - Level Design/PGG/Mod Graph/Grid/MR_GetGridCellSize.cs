using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planning.ModNodes.Grid
{

    public class MR_GetGridCellSize : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Get Grid Cell Size"; }
        public override string GetNodeTooltipDescription { get { return "Getting grid's single cell size in units"; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(218, _EditorFoldout ? 102 : 84); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }


        [Port(EPortPinType.Output, EPortValueDisplay.NotEditable)] public PGGVector3Port CellSize;


        public override void OnStartReadingNode()
        {
            var setup = MG_Preset;
            if (setup == null) return;
            CellSize.Value = MG_Preset.GetCellUnitSize();
        }

    }
}