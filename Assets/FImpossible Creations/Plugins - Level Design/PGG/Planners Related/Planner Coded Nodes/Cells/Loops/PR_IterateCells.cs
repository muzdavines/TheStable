using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells.Loops
{

    public class PR_IterateCells : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "   Iterate Cells" : "Iterate Cells"; }
        public override string GetNodeTooltipDescription { get { return "Running loop iteration through every cell of provided field"; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.CellsManipulation; } }
        public override Color GetNodeColor() { return new Color(0.2f, 0.9f, 0.3f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(188, _EditorFoldout ? 151 : 91); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }
        public override int OutputConnectorsCount { get { return 2; } }
        public override int AllowedOutputConnectionIndex { get { return 0; } }
        public override int HotOutputConnectionIndex { get { return 1; } }
        public override bool IsFoldable { get { return true; } }

        public override string GetOutputHelperText(int outputId = 0)
        {
            if (outputId == 0) return "Finish";
            return "Iteration";
        }


        [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGCellPort IterationCell;
        [HideInInspector][Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideOnConnected, 1)] public PGGPlannerPort CellsOf;
        [HideInInspector][Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.NotEditable, 1)] public BoolPort BreakLoop;
        [HideInInspector] public bool RandomizeCellsOrder = false;

        private List<FieldCell> _randomizeHelper = new List<FieldCell>();

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            CheckerField3D checker = null;
            PlannerResult lastReslt = newResult;

            CellsOf.TriggerReadPort(true);
            BreakLoop.Value = false;

            IterationCell.Clear();

            if (CellsOf.HasShape)
            {
                checker = CellsOf.shape;
            }
            else
            {
                var planner = GetPlannerFromPort(CellsOf);
                //UnityEngine.Debug.Log("Planner connection = " + Planner.BaseConnection.PortReference.GetPortValue + " vs " + Planner.GetPortValue);
                //UnityEngine.Debug.Log("planner = " + planner.IndexOnPreset + ", " + planner.IndexOfDuplicate);

                if (planner != null)
                {
                    lastReslt = planner.LatestResult;
                    if (planner.LatestResult != null)
                    {
                        checker = planner.LatestChecker;
                    }
                }
                else
                {
                    checker = CellsOf.GetInputCheckerSafe;
                }
            }


            if (checker != null)
            {
                System.Collections.Generic.List<FieldCell> cellsToIterate = checker.CopyGridCellsList();

                if (RandomizeCellsOrder)
                {
                    _randomizeHelper.Clear();
                    for (int i = 0; i < cellsToIterate.Count; i++) _randomizeHelper.Add(cellsToIterate[i]);

                    cellsToIterate.Clear();

                    while (_randomizeHelper.Count > 0)
                    {
                        int ind = FGenerators.GetRandom(0, _randomizeHelper.Count);
                        cellsToIterate.Add(_randomizeHelper[ind]);
                        _randomizeHelper.RemoveAt(ind);
                    }
                }

                for (int c = 0; c < cellsToIterate.Count; c++)
                {
                    if (c != 0)
                        if (BreakLoop.IsConnected)
                        {
                            BreakLoop.TriggerReadPort(true);
                            if (BreakLoop.GetInputValue) break;
                        }

                    FieldCell cell = cellsToIterate[c];
                    IterationCell.ProvideFullCellData(cell, checker, lastReslt);
                    CallOtherExecutionWithConnector(1, print);
                    //PGGTriggerPort.Execute()
                    //IterationTrigger.Execute(planner, print);

                    #region Debugging Gizmos
#if UNITY_EDITOR
                    if (Debugging)
                    {
                        DebuggingInfo = "Outside direction iteration";

                        Vector3 wPos = checker.GetWorldPos(cell);
                        Vector3 scl = checker.RootScale;
                        Matrix4x4 mx = checker.Matrix;

                        DebuggingGizmoEvent = () =>
                        {
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(wPos, scl * 0.5f);
                        };
                    }
#endif
                    #endregion

                }
            }

        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (!_EditorFoldout) return;

            baseSerializedObject.Update();
            if (sp == null) sp = baseSerializedObject.FindProperty("CellsOf");
            SerializedProperty spc = sp.Copy();
            EditorGUILayout.PropertyField(spc); spc.Next(false);
            EditorGUILayout.PropertyField(spc); spc.Next(false);
            EditorGUILayout.PropertyField(spc);
            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}