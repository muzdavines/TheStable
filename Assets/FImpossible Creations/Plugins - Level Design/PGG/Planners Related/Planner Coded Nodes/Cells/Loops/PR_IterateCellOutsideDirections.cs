using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells.Loops
{

    public class PR_IterateCellOutsideDirections : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Iterate Cell Outside Dir"; }
        public override string GetNodeTooltipDescription { get { return "Running loop cell iteration through every direction which is not covered by other cell"; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.CellsManipulation; } }
        public override Color GetNodeColor() { return new Color(0.2f, 0.9f, 0.3f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(198, 111); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }
        public override int OutputConnectorsCount { get { return 2; } }
        public override int AllowedOutputConnectionIndex { get { return 0; } }
        public override int HotOutputConnectionIndex { get { return 1; } }
        public override string GetOutputHelperText(int outputId = 0)
        {
            if (outputId == 0) return "Finish";
            return "Iteration";
        }

        [Port(EPortPinType.Input)] public PGGCellPort Cell;
        [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGVector3Port OutDirection;

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Cell.TriggerReadPort(true);
            FieldCell cell = Cell.GetInputCellValue;

            if (FGenerators.CheckIfIsNull(cell)) { UnityEngine.Debug.Log("null cell"); return; }

            CheckerField3D checker = Cell.GetInputCheckerValue;

            if (FGenerators.CheckIfIsNull(checker)) { /*UnityEngine.Debug.Log("null checker");*/ return; }

            PlannerResult plannerOwner = Cell.GetInputResultValue;

            if (FGenerators.CheckIfIsNull(plannerOwner)) { /*UnityEngine.Debug.Log("null plannerOwner");*/ return; }
            if (FGenerators.CheckIfIsNull(plannerOwner.ParentFieldPlanner)) { /*Debug.Log("null ParentFieldPlanner");*/ return; }

            // Check in x=1 (right) direction (local right)
            wasIter = false; // just for debugging
            Iterate(checker, cell, print, new Vector3Int(1, 0, 0));
            Iterate(checker, cell, print, new Vector3Int(0, 0, 1));
            Iterate(checker, cell, print, new Vector3Int(-1, 0, 0));
            Iterate(checker, cell, print, new Vector3Int(0, 0, -1));


            #region Debugging Gizmos
#if UNITY_EDITOR
            if (Debugging)
            {
                DebuggingInfo = "Outside direction iteration";

                Vector3 wPos = cell.Pos;
                Vector3 scl = checker.RootScale;
                Matrix4x4 mx = checker.Matrix;


                DebuggingGizmoEvent = () =>
                {
                    Gizmos.matrix = mx;
                    Gizmos.color = wasIter ? Color.green : Color.red;
                    Gizmos.DrawCube(wPos, scl * 0.5f);
                    Gizmos.DrawLine(wPos, wPos + Vector3.Scale(scl, new Vector3(1, 0, 0) * 1.6f));
                    Gizmos.DrawLine(wPos, wPos + Vector3.Scale(scl, new Vector3(0, 0, 1) * 1.6f));
                    Gizmos.DrawLine(wPos, wPos + Vector3.Scale(scl, new Vector3(-1, 0, 0) * 1.6f));
                    Gizmos.DrawLine(wPos, wPos + Vector3.Scale(scl, new Vector3(0, 0, -1) * 1.6f));
                    Gizmos.matrix = Matrix4x4.identity;
                };
            }
#endif
            #endregion

        }

        bool wasIter = false;

        void Iterate(CheckerField3D checker, FieldCell cell, PlanGenerationPrint print, Vector3Int dir)
        {
            FieldCell oCell = checker.GetCell(cell.Pos + dir);

            if (FGenerators.CheckIfIsNull(oCell)) // space empty -> can check
            {
                OutDirection.Value = dir;
                CallOtherExecutionWithConnector(1, print);
                wasIter = true;
                //PGGTriggerPort.Execute
                //IterationTrigger.Execute(owner, print);
            }
        }

    }
}