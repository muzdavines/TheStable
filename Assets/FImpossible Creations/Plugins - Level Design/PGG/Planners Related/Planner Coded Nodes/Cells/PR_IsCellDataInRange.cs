using FIMSpace.Graph;
using UnityEngine;
using FIMSpace.Generating.Checker;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells
{

    public class PR_IsCellDataInRange : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "   Check cell data in range" : "Check if there is cell with cell data in range"; }
        public override string GetNodeTooltipDescription { get { return "Checking cells around if there is cell with cell data inside"; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(228, _EditorFoldout ? 199 : 140); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override bool IsFoldable { get { return true; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }

        [Port(EPortPinType.Input, EPortValueDisplay.NotEditable, 1)] public PGGCellPort StartCell;
        [Port(EPortPinType.Input, 1)] public PGGStringPort DataToCheck;
        [Port(EPortPinType.Input, 1)] public IntPort MaxDistance;

        [Port(EPortPinType.Output, EPortValueDisplay.NotEditable)] public BoolPort Found;
        [HideInInspector][Port(EPortPinType.Output)] public PGGCellPort DetectedCell;
        [HideInInspector][Port(EPortPinType.Input, 1)] public PGGPlannerPort CheckIn;
        [HideInInspector][Tooltip("If you want to check cells in Y axis levels too")] public bool CheckY = false;

        FieldCell _preStartCell = null;

        public override void PreGeneratePrepare()
        {
            base.PreGeneratePrepare();
            _preStartCell = null;
        }

        public override void OnStartReadingNode()
        {
            StartCell.TriggerReadPort(true);
            var startCell = StartCell.GetInputCellValue;

            if (_preStartCell == startCell) return; // Was computed

            DetectedCell.Clear();
            Found.Value = false;

            PlannerResult result = null;

            if (FGenerators.IsNull(startCell)) return;

            Vector3 startCellWPos = startCell.Pos;

            if (StartCell.GetInputCheckerValue != null)
                startCellWPos = StartCell.GetInputCheckerValue.LocalToWorld(startCell.Pos);

            CheckIn.TriggerReadPort(true);
            Checker.CheckerField3D checker = null;

            if (CheckIn.IsConnected)
            {
                FieldPlanner plan = CheckIn.GetPlannerFromPort(false);

                if (plan)
                {
                    checker = plan.LatestChecker;
                    result = plan.LatestResult;
                }
                else
                {
                    checker = CheckIn.GetInputCheckerSafe;
                    if (CurrentExecutingPlanner) result = CurrentExecutingPlanner.LatestResult;
                }
            }
            else
            {
                FieldPlanner plan = CheckIn.GetPlannerFromPort(false);
                if (plan == null) { plan = CurrentExecutingPlanner; if (plan == null) return; }

                result = plan.LatestResult;
                checker = plan.LatestChecker;
            }

            if (checker == null) return;

            MaxDistance.TriggerReadPort(true);
            int dist = MaxDistance.GetInputValue;

            if (dist <= 0) return;

            Vector3Int startLocPos = checker.WorldToLocal(startCellWPos).V3toV3Int();

            DataToCheck.TriggerReadPort(true);
            string checkStr = DataToCheck.GetInputValue;

            if (string.IsNullOrEmpty(checkStr)) return;

            FieldCell nearestFound = null;
            float nearest = float.MaxValue;
            float maxDist = (float)dist;

            if (!CheckY)
            {

                for (int d = 0; d <= dist; d++)
                {

                    for (int x = -d; x <= d; x += 1)
                    {
                        for (int z = -d; z <= d; z += 1)
                        {
                            //if (x == 0 && z == 0) continue;

                            if (Mathf.Abs(x) != d && Mathf.Abs(z) != d) continue;
                            Vector3Int off = new Vector3Int(x, 0, z);

                            if (off.magnitude > maxDist) continue;

                            var foundCell = checker.GetCell(startLocPos + off);

                            if (FGenerators.NotNull(foundCell))
                            {
                                if (foundCell.HaveCustomData(checkStr))
                                {
                                    float cDist = Vector3.SqrMagnitude(startLocPos - foundCell.Pos);
                                    if (cDist < nearest)
                                    {
                                        nearest = cDist;
                                        nearestFound = foundCell;

                                    }
                                }
                            }

                        }
                    }

                    if (FGenerators.NotNull(nearestFound))
                    {
                        Found.Value = true;
                        break;
                    }

                }

            }
            else
            {

                // Y Level Check included

                for (int d = 0; d <= dist; d++)
                {

                    for (int x = -d; x <= d; x += 1)
                    {
                        for (int y = -d; y <= d; y += 1)
                        {
                            for (int z = -d; z <= d; z += 1)
                            {
                                //if (x == 0 && z == 0 && y == 0) continue;

                                if (Mathf.Abs(x) != d && Mathf.Abs(z) != d && Mathf.Abs(y) != d) continue;
                                Vector3Int off = new Vector3Int(x, y, z);

                                if (off.magnitude > maxDist) continue;

                                //if (startLocPos == new Vector3Int(4, 0, 0)) UnityEngine.Debug.Log("checking in " + off);

                                var foundCell = checker.GetCell(startLocPos + off);

                                //checker.DebugLogDrawCellInWorldSpace(startLocPos + off, Color.green);

                                if (FGenerators.NotNull(foundCell))
                                {
                                    //checker.DebugLogDrawCellInWorldSpace(startLocPos + off, Color.green);

                                    if (foundCell.HaveCustomData(checkStr))
                                    {
                                        float cDist = Vector3.SqrMagnitude(startLocPos - foundCell.Pos);

                                        if (cDist < nearest)
                                        {
                                            nearest = cDist;
                                            nearestFound = foundCell;
                                        }
                                    }
                                }

                            }
                        }

                    }

                    if (FGenerators.NotNull(nearestFound))
                    {
                        Found.Value = true;
                        break;
                    }

                }


            }

            DetectedCell.ProvideFullCellData(nearestFound, checker, result);

        }

        FieldCell CheckCellIn(CheckerField3D checker, string data, Vector3Int pos)
        {
            FieldCell cell = checker.GetCell(pos);
            if (FGenerators.IsNull(cell)) return null;
            if (cell.InTargetGridArea == false) return null;
            if (cell.HaveCustomData(data)) return cell;
            return null;
        }

#if UNITY_EDITOR

        private SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (_EditorFoldout)
            {
                baseSerializedObject.Update();
                if (sp == null) sp = baseSerializedObject.FindProperty("DetectedCell");
                SerializedProperty spc = sp.Copy();
                EditorGUILayout.PropertyField(spc); spc.Next(false);
                EditorGUILayout.PropertyField(spc); spc.Next(false);
                EditorGUILayout.PropertyField(spc);
                baseSerializedObject.ApplyModifiedProperties();
            }
        }

#endif

    }
}