using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Planning.ModNodes.Cells
{

    public class MR_GetCellPosition : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Get Cell Position" : "Get Grid Cell Position"; }
        public override string GetNodeTooltipDescription { get { return "Get Grid Cell Position"; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(210, _EditorFoldout ? 102 : 84); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        
        [Port(EPortPinType.Output, EPortValueDisplay.HideValue, "Row / Height / Column")] public PGGVector3Port RowColumnPosition;
        [HideInInspector] [Port(EPortPinType.Input, 1)] public PGGModCellPort Cell;

        public override void OnStartReadingNode()
        {
            Cell.TriggerReadPort(true);

            var cell = Cell.GetInputCellValue;

            if (FGenerators.IsNull(cell)) return;

            RowColumnPosition.Value = cell.Pos.V3IntToV3();
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (_EditorFoldout)
            {
                if (sp == null) sp = baseSerializedObject.FindProperty("Cell");
                EditorGUILayout.PropertyField(sp);
            }

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}