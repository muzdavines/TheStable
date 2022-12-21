using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells
{

    public class PR_GetCellByIndex : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Get Cell By Index" : "Get Cell By Index"; }
        public override string GetNodeTooltipDescription { get { return "Getting cell using index"; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(188, _EditorFoldout ? 122 : 102); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override bool IsFoldable { get { return true; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }

        [Port(EPortPinType.Input)] public IntPort Index;
        [Port(EPortPinType.Output)] public PGGCellPort Cell;
        [HideInInspector][Port(EPortPinType.Input)] public PGGPlannerPort CellOf;

        public override void OnStartReadingNode()
        {
            Index.TriggerReadPort(true);
            CellOf.TriggerReadPort(true);
            Cell.Clear();

            var planner = GetPlannerFromPort(CellOf, false);
            if (FGenerators.IsNull(planner)) return;

            int i = Index.GetInputValue;
            if ( i >= 0 && i < planner.LatestChecker.AllCells.Count)
            {
                Cell.ProvideFullCellData(planner.LatestChecker.GetCell(i), planner.LatestChecker, planner.LatestResult);
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
                if (sp == null) sp = baseSerializedObject.FindProperty("CellOf");
                EditorGUILayout.PropertyField(sp);
                baseSerializedObject.ApplyModifiedProperties();
            }
        }

#endif

    }
}