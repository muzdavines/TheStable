using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells
{

    public class PR_GetRadomCellIn : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Get Random Cell In" : "Get Random Cell In List"; }
        public override string GetNodeTooltipDescription { get { return "Getting random cell out of the provided list of cells, planner or out of the shape"; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(178, _EditorFoldout ? 118 : 98); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override bool IsFoldable { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }

        [Port(EPortPinType.Input, EPortValueDisplay.NotEditable, 1)] public PGGPlannerPort GetCellFrom;
        [Port(EPortPinType.Output)] public PGGCellPort ChoosedCell;

        int lastIterCall = -100;
        public override void PreGeneratePrepare()
        {
            lastIterCall = -100;
            base.PreGeneratePrepare();
        }

        public override void OnStartReadingNode()
        {
            if (CurrentExecutingPlanner == null) return;

            if (lastIterCall == CurrentExecutingPlanner.GetNodeHelperIterationIndex()) return; // Was called in this iter
            lastIterCall = CurrentExecutingPlanner.GetNodeHelperIterationIndex();

            GetCellFrom.TriggerReadPort(true);
            ChoosedCell.Clear();

            System.Collections.Generic.List<FieldPlanner> planners = null;
            Checker.CheckerField3D checker = null;

            if (GetCellFrom.IsConnected)
            {
                if (GetCellFrom.JustCheckerContainer)
                {
                    checker = GetCellFrom.GetInputCheckerSafe;
                }
                else if (GetCellFrom.HasShape)
                {
                    checker = GetCellFrom.shape;
                }
                else
                {
                    planners = GetPlannersFromPort(GetCellFrom, false, true, true);
                }
            }
            else
            {
                return;
            }

            if (planners != null)
            {
                if (planners.Count == 1)
                {
                    checker = planners[0].LatestChecker;
                    ChoosedCell.ProvideFullCellData(checker.GetCell(FGenerators.GetRandom(0, checker.ChildPositionsCount)), checker, planners[0].LatestResult);
                }
                else
                {
                    var choosedPlanner = planners[FGenerators.GetRandom(0, planners.Count)];
                    checker = choosedPlanner.LatestChecker;
                    ChoosedCell.ProvideFullCellData(checker.GetCell(FGenerators.GetRandom(0, checker.ChildPositionsCount)), checker, choosedPlanner.LatestResult);
                }
            }
            else
            {
                if (checker != null)
                    ChoosedCell.ProvideFullCellData(checker.GetCell(FGenerators.GetRandom(0, checker.ChildPositionsCount)), checker, CurrentExecutingPlanner.LatestResult);
            }

        }
    }
}