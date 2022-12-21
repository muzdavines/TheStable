using FIMSpace.Graph;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells.Actions
{
    public class PR_IsAnyCellInPosition : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "   Is Any Cell In Position" : "Is Any Cell In Position"; }
        public override string GetNodeTooltipDescription { get { return "Checking if there is any cell in provided position"; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 240 : 210, _EditorFoldout ? 122 : 102); } }
        public override bool IsFoldable { get { return true; } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }

        [Port(EPortPinType.Input)] public PGGVector3Port WorldPos;
        [Port(EPortPinType.Output, EPortValueDisplay.NotEditable)] public BoolPort CellDetected;
        [HideInInspector] [Port(EPortPinType.Output)] public PGGCellPort Cell;


        public override void OnStartReadingNode()
        {
            Cell.Clear();
            CellDetected.Value = false;

            WorldPos.TriggerReadPort(true);
            Vector3 pos = WorldPos.GetInputValue;

            var pl = CurrentExecutingPlanner;
            
            if ( pl != null)
                if ( pl.ParentBuildPlanner != null)
                {
                    var allPlanners = pl.ParentBuildPlanner.CollectAllAvailablePlanners();
                    
                    for (int p = 0; p < allPlanners.Count; p++)
                    {
                        var planner = allPlanners[p];
                        var cell = planner.LatestChecker.GetCellInWorldPos(pos);
                        if ( FGenerators.NotNull(cell))
                        {
                            Cell.ProvideFullCellData(cell, planner.LatestChecker, planner.LatestResult);
                            CellDetected.Value = true;
                            break;
                        }
                    }
                }
            
            //var cell = chA.GetNearestCellInWorldPos(pos);
            //Cell.ProvideFullCellData(cell, chA, fieldA.LatestResult);
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (_EditorFoldout)
            {
                if (sp == null) sp = baseSerializedObject.FindProperty("Cell");
                EditorGUILayout.PropertyField(sp);
            }
        }
#endif


    }
}