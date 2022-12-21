using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.ModNodes.Cells
{

    public class MR_GetCellAt : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Get Cell At" : "Get Cell At"; }
        public override string GetNodeTooltipDescription { get { return "Get grid cell at provided position / offset from current executed cell."; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(184, _EditorFoldout ? 142 : 104); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public enum EGetCellMode
        {
            ExactPosition, OffsetFromCurrentCell
        }

        [Port(EPortPinType.Input, EPortValueDisplay.HideValue)] public PGGVector3Port Offset;
        [Port(EPortPinType.Output)] public PGGModCellPort ResultCell;
        [HideInInspector] public EGetCellMode GetAt = EGetCellMode.ExactPosition;
        [HideInInspector] [Port(EPortPinType.Input, 1)] public PGGModCellPort OriginCell;

        public override void OnStartReadingNode()
        {
            if (GetAt == EGetCellMode.OffsetFromCurrentCell)
            {
                OriginCell.TriggerReadPort(true);

                FieldCell origin = OriginCell.GetInputCellValue;
                if (FGenerators.IsNull(origin)) origin = MG_Cell;
                if (FGenerators.IsNull(origin)) return;

                Offset.TriggerReadPort(true);
                ResultCell.ProvideFullCellData(MG_Grid.GetCell(origin.Pos + Offset.GetInputValue.V3toV3Int(), false));
            }
            else
            {
                Offset.TriggerReadPort(true);
                ResultCell.ProvideFullCellData(MG_Grid.GetCell(Offset.GetInputValue.V3toV3Int(), false));
            }
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (GetAt == EGetCellMode.ExactPosition) Offset.OverwriteName = "Position"; else Offset.OverwriteName = "Offset";

            if (_EditorFoldout)
            {
                if (sp == null) sp = baseSerializedObject.FindProperty("GetAt");
                EditorGUIUtility.labelWidth = 50;
                EditorGUILayout.PropertyField(sp);
                EditorGUIUtility.labelWidth = 0;
                SerializedProperty spc = sp.Copy(); spc.Next(false);
                EditorGUILayout.PropertyField(spc);

                OriginCell.AllowDragWire = true;
            }
            else
            {
                OriginCell.AllowDragWire = false;
            }

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}