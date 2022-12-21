using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Access
{

    public class PR_GetFieldGridBoundaries : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return created ? "Grid Boundary (Cells)" : "Get Field Grid Boundary (in cells)"; }
        public override string GetNodeTooltipDescription { get { return "Get boundary size of the field in cells count"; } }
        [HideInInspector] public bool created = false;

        public override void OnCreated()
        {
            created = true;
            base.OnCreated();
        }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(210, _EditorFoldout ? 140 : 96); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override bool IsFoldable { get { return true; } }

        [Port(EPortPinType.Input, EPortValueDisplay.HideOnConnected, 1)] public PGGPlannerPort Planner;
        [Port(EPortPinType.Output, EPortValueDisplay.NotEditable)] public PGGVector3Port Size;
        [HideInInspector] [Port(EPortPinType.Output, EPortValueDisplay.NotEditable)] public PGGVector3Port Min;
        [HideInInspector] [Port(EPortPinType.Output, EPortValueDisplay.NotEditable)] public PGGVector3Port Max;

        public override void OnStartReadingNode()
        {
            base.OnStartReadingNode();

            FieldPlanner planner = GetPlannerFromPort(Planner);
            Vector3Int size, min, max;

            if (planner)
            {
                size = planner.LatestChecker.Grid.GetMaxSizeInCells();
                min = planner.LatestChecker.Grid.GetMin();
                max = planner.LatestChecker.Grid.GetMax();

                Size.Value = size;
                Min.Value = min;
                Max.Value = max;
            }
        }



#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (_EditorFoldout)
            {
                if (sp == null) sp = baseSerializedObject.FindProperty("Min");

                SerializedProperty s = sp.Copy();
                EditorGUILayout.PropertyField(s); s.Next(false);
                EditorGUILayout.PropertyField(s);
            }
        }
#endif

    }
}
