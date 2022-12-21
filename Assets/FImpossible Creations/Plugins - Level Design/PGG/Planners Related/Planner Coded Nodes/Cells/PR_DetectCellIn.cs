using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells
{

    public class PR_DetectCellIn : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Detect Cell in" : "Detect Cell in Position"; }
        public override string GetNodeTooltipDescription { get { return "Detecting other planner cell in provided position (if other planner field is not assigned then using all other available planner fields)"; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(198, _EditorFoldout ? 118 : 98); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override bool IsFoldable { get { return true; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }

        [Port(EPortPinType.Input, EPortValueDisplay.NotEditable, 1)] public PGGVector3Port FindIn;
        [Port(EPortPinType.Output)] public PGGCellPort DetectedCell;
        [HideInInspector][Port(EPortPinType.Input, EPortValueDisplay.HideValue)] public PGGPlannerPort DetectOnlyCellsOf;

        public override void OnStartReadingNode()
        {
            FindIn.TriggerReadPort(true);

            System.Collections.Generic.List<FieldPlanner> planners;

            if (DetectOnlyCellsOf.IsConnected)
            {
                planners = GetPlannersFromPort(DetectOnlyCellsOf, false, true, true);
            }
            else
            {
                planners = ParentPlanner.ParentBuildPlanner.CollectAllAvailablePlanners(true, true);
                planners.Remove(CurrentExecutingPlanner);
            }

            Vector3 position = FindIn.GetInputValue;

            for (int p = 0; p < planners.Count; p++)
            {
                var pl = planners[p];
                if (pl == null) continue;

                FieldCell cell = pl.LatestChecker.GetCellInWorldPos(position);
                if (FGenerators.IsNull(cell)) continue;

                DetectedCell.ProvideFullCellData(cell, pl.LatestChecker, pl.LatestResult);
                break;
            }
        }

#if UNITY_EDITOR

        private SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (_EditorFoldout)
            {
                baseSerializedObject.Update();
                if (sp == null) sp = baseSerializedObject.FindProperty("DetectOnlyCellsOf");
                EditorGUILayout.PropertyField(sp);
                baseSerializedObject.ApplyModifiedProperties();
            }
        }

#endif

    }
}