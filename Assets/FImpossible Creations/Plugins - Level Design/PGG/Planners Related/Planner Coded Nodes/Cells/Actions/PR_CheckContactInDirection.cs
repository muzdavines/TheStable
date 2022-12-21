using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells.Actions
{

    public class PR_CheckContactInDirection : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Contact In Direction" : "Check Cell Contact In Direction"; }
        public override string GetNodeTooltipDescription { get { return "Checking if there is collision with other field in choosed direction"; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.CellsManipulation; } }
        public override Color GetNodeColor() { return new Color(0.2f, 0.9f, 0.3f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(254, 141); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }

        [Port(EPortPinType.Input, "Start Cell")] public PGGCellPort Cell;
        [Port(EPortPinType.Input)] public PGGVector3Port Direction;
        [Port(EPortPinType.Input)] public PGGPlannerPort CheckContactWith;
        [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGCellPort Contact;

        private FieldPlanner latestCaller = null;

        public override void Prepare(PlanGenerationPrint print)
        {
            latestCaller = null;
            base.Prepare(print);
        }

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Contact.Clear();

            // Call for contact mask only once for the planner
            if (latestCaller != newResult.ParentFieldPlanner)
            {
                CheckContactWith.TriggerReadPort(true);
                latestCaller = newResult.ParentFieldPlanner;
            }

            System.Collections.Generic.List<FieldPlanner> contactMask = GetPlannersFromPort(CheckContactWith);


            if (contactMask == null) { return; }
            if (contactMask.Count == 0) { return; }

            Cell.TriggerReadPort(true);
            FieldCell myCell = Cell.GetInputCellValue;
            if (FGenerators.CheckIfIsNull(myCell)) { return; }

            Direction.TriggerReadPort(true);
            Vector3Int dir = Direction.GetInputValue.V3toV3Int();

            if (dir == Vector3Int.zero) { return; }

            float nrst = float.MaxValue;
            Checker.CheckerField3D myChecker = Cell.GetInputCheckerValue;

            if (myChecker == null) { return; }

            Checker.CheckerField3D.DebugHelper = false;

            if (Cell.GetInputResultValue != null)
            {
                if (Cell.GetInputResultValue.ParentFieldPlanner)
                {
                    var cellPlanner = Cell.GetInputResultValue.ParentFieldPlanner;

                    if (contactMask.Count == 1 && contactMask[0] == cellPlanner && CheckContactWith.PortState() == EPortPinState.Connected)
                    {
                        var selfContact = GetPlannerFromPort(CheckContactWith);
                        if (selfContact != cellPlanner)
                            contactMask.Remove(Cell.GetInputResultValue.ParentFieldPlanner);
                    }
                    else
                        contactMask.Remove(Cell.GetInputResultValue.ParentFieldPlanner);
                }
            }

            for (int i = 0; i < contactMask.Count; i++)
            {
                Checker.CheckerField3D othChecker = contactMask[i].LatestResult.Checker;

                if (myChecker.CheckCollisionInDirection(myCell, dir, othChecker))
                {
                    FieldCell oCell = myChecker._CheckCollisionInDirection_OtherCell;

                    float dist = (othChecker.GetWorldPos(oCell) - myChecker.GetWorldPos(myCell)).sqrMagnitude;

                    if (dist < nrst)
                    {
                        nrst = dist;
                        Contact.ProvideFullCellData(myChecker._CheckCollisionInDirection_OtherCell, othChecker, contactMask[i].LatestResult);
                    }
                }
            }



            #region Debugging Gizmos
#if UNITY_EDITOR
            if (Debugging)
            {
                DebuggingInfo = "Outside direction iteration";

                Vector3 wPos = myChecker.GetWorldPos(myCell);
                Vector3 owPos = Vector3.zero;
                Vector3 scl = myChecker.RootScale;

                bool detected = false;
                if (FGenerators.Exists(Contact.Cell))
                    if (FGenerators.Exists(Contact.Checker))
                    {
                        detected = true;
                        owPos = Contact.Checker.GetWorldPos(Contact.Cell);
                    }

                DebuggingGizmoEvent = () =>
                {
                    Gizmos.color = detected ? Color.green : Color.yellow;
                    Gizmos.DrawCube(wPos, scl * 0.5f);

                    if (detected)
                    {
                        Gizmos.DrawCube(wPos, scl * 0.5f);
                        Gizmos.DrawCube(owPos, scl * 0.5f);
                        Gizmos.DrawLine(wPos, owPos);
                    }
                };
            }
#endif
            #endregion

        }

#if UNITY_EDITOR

        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            GUILayout.Label("Cell: " + Cell.GetPortValueSafe);
        }

#endif

    }
}