using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells.Actions
{

    public class PR_RoundAccordingly : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? " Get Cell Aligned Position" : "Get Cell Aligned Position"; }
        public override string GetNodeTooltipDescription { get { return "Rounding position of one field to be aligned with cell positions of other field"; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(198, 102); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.CellsManipulation; } }

        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue,  1)] public PGGCellPort Cell;
        //[Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, 1)] public PGGPlannerPort RoundWith;
        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, 1)] public PGGCellPort RoundWith;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGVector3Port Rounded;


        public override void OnStartReadingNode()
        {
            Cell.TriggerReadPort();
            RoundWith.TriggerReadPort();

            var myCell = Cell.GetInputCellValue;
            if (myCell == null) return;

            var myChkr = Cell.GetInputCheckerValue;
            if (myChkr == null) return;

            //var oPlanner = GetPlannerFromPort(RoundWith);
            //if (oPlanner == null) return;

            //var oChkr = oPlanner.LatestResult.Checker;
            var oChkr = RoundWith.GetInputCheckerValue;
            if (oChkr == null) return;

            Rounded.Value = myChkr.RoundPositionAccordingly(oChkr, myChkr.GetWorldPos( myCell) );
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (sp == null) sp = baseSerializedObject.FindProperty("Rounded");

            GUILayout.Space(-21);
            GUILayout.BeginHorizontal();
            GUILayout.Space(19);
            EditorGUILayout.PropertyField(sp, GUIContent.none);
            GUILayout.EndHorizontal();

        }

#endif

    }
}