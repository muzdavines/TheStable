using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells.Actions
{

    public class PR_CellDebug : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Cell Debug"; }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(164, 80); } }
        public override bool IsFoldable { get { return false; } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }

        [Port(EPortPinType.Input, 1)] public PGGCellPort Cell;

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Cell.TriggerReadPort();
            var cell = Cell.GetInputCellValue;

            if (FGenerators.IsNull(cell)) return;

            if (cell.GetCustomDatasCount() > 0)
                UnityEngine.Debug.Log(cell.GetCustomDatasCount() + " checker hash "  + CurrentExecutingPlanner.LatestChecker.GetHashCode() +" grid hash " + CurrentExecutingPlanner.LatestChecker.Grid.GetHashCode() + " cell c = " + cell.GetHashCode() + " dats: " +  "  " + cell.Pos);
        }
    }
}