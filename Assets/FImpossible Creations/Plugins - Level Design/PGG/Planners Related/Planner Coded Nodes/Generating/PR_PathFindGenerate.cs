using FIMSpace.Graph;
using UnityEngine;
using System.Collections.Generic;
using FIMSpace.Generating.Checker;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Generating
{

    public class PR_PathFindGenerate : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Path Find Generate"; }
        public override string GetNodeTooltipDescription { get { return "Path find (A*) algorithm towards target position with collision detection. Supporting search towards Vector3 Position!"; } }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(262, _EditorFoldout ? 268 : 160); } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.7f, .9f, 0.95f); }

        [Port(EPortPinType.Input)] public PGGPlannerPort SearchFrom;
        [Port(EPortPinType.Input)] public PGGPlannerPort SearchTowards;
        [Port(EPortPinType.Input)] public PGGPlannerPort CollideWith;

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.CellsManipulation; } }

        [Range(0f, 1f)]
        public float NonDiagonal = 1;

        [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGPlannerPort PathShape;

        [HideInInspector] public bool TryStartCentered = true;
        [HideInInspector] [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGVector3Port StartPathCellPos;
        [HideInInspector] [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGVector3Port EndPathCellPos;
        [HideInInspector] [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGVector3Port StartPathDir;
        //[HideInInspector] [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGVector3Port EndPathDir;
        [HideInInspector] [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGCellPort ContactCell;

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            ContactCell.Clear();
            SearchFrom.TriggerReadPort(true);
            SearchTowards.TriggerReadPort(true);
            CollideWith.TriggerReadPort(true);

            PathShape.Clear();
            PathShape.JustCheckerContainer = true;

            FieldPlanner a = GetPlannerFromPort(SearchFrom, false);
            FieldPlanner b = GetPlannerFromPort(SearchTowards, false);

            if (a == null) return;
            if (b == null) return;

            CheckerField3D bChec = null;

            if (SearchTowards.Connections.Count > 0)
            {
                for (int c = 0; c < SearchTowards.Connections.Count; c++)
                {
                    var conn = SearchTowards.Connections[c];
                    if (conn.PortReference.GetPortValue is Vector3)
                    {
                        Vector3 pos = (Vector3)conn.PortReference.GetPortValue;

                        // Temporary checker to search for it
                        bChec = new CheckerField3D();
                        bChec.RootPosition = pos;
                        bChec.AddLocal(Vector3.zero);
                        break;
                    }
                }
            }

            if (bChec == null) bChec = b.LatestChecker;

            List<FieldPlanner> coll = GetPlannersFromPort(CollideWith, false, false, true);
            List<CheckerField3D> masks = new List<CheckerField3D>();

            for (int c = 0; c < coll.Count; c++) masks.Add(coll[c].LatestChecker);

            masks.Remove(a.LatestChecker);
            masks.Remove(bChec);

            CheckerField3D.PathFindParams findParams = new CheckerField3D.PathFindParams();
            findParams.directions = CheckerField3D.GetDefaultDirections;
            findParams.NoLimits = true;

            var path = a.LatestChecker.GeneratePathFindTowards(bChec, masks, findParams);
            
            //path.DebugLogDrawCellInWorldSpace(path.GetCell(0), Color.red);
            //if (path.AllCells.Count > 0)
            {
                StartPathCellPos.Value = path.GetWorldPos(0);
                if (path.ChildPositionsCount > 1) StartPathDir.Value = FVectorMethods.ChooseDominantAxis((path.GetWorldPos(1) - path.GetWorldPos(0)).normalized).normalized;
                if (path.ChildPositionsCount > 1) EndPathCellPos.Value = path.GetWorldPos(path.ChildPositionsCount - 1);
               
                if (path.ChildPositionsCount > 1)
                {
                    if (FGenerators.NotNull(a.LatestChecker._pathFind_endCellOther))
                    {
                        ContactCell.ProvideFullCellData(a.LatestChecker._pathFind_endCellOther, bChec, b.LatestResult);
                    }
                    else
                    {
                    }

                    //EndPathDir.Value = FVectorMethods.ChooseDominantAxis((bChec.GetWorldPos(path._GeneratePathFindTowards_OtherTargetCell) - path.GetWorldPos(path.ChildPositionsCount - 1)).normalized).normalized;
                }

                PathShape.ProvideShape(path);
            }

            //path.DebugLogDrawCellsInWorldSpace(Color.red);

            if (Debugging) DebuggingInfo = "Generating path from " + a.name + " towards " + b.name;
        }

#if UNITY_EDITOR

        UnityEditor.SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            baseSerializedObject.Update();
            if (_EditorFoldout)
            {
                GUILayout.Space(4);
                if (sp == null) sp = baseSerializedObject.FindProperty("TryStartCentered");
                SerializedProperty spc = sp.Copy();
                EditorGUILayout.PropertyField(spc, true);
                spc.Next(false); EditorGUILayout.PropertyField(spc, true);
                spc.Next(false); EditorGUILayout.PropertyField(spc, true);
                spc.Next(false); EditorGUILayout.PropertyField(spc, true);
                spc.Next(false); EditorGUILayout.PropertyField(spc, true);
            }

            baseSerializedObject.ApplyModifiedProperties();
        }

        //public override void Editor_OnAdditionalInspectorGUI()
        //{
        //    EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
        //    GUILayout.Label("PathStart: " + PathStart.GetInputValue);
        //    GUILayout.Label("PathEnd: " + PathEnd.GetInputValue);
        //}
#endif
    }
}