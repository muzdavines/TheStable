using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells.Actions
{

    public class PR_SetCellParameter : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? (ParameterToSet== ESetCellParameter.InternalDirection ? "Set Cell Parameter" : "Add Cell Parameter") : "Set\\Add Cell Parameter"; }
        public override string GetNodeTooltipDescription { get { return "Setting cell available parameter like internal direction or CellData (string - it can be used like tag)"; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2((ParameterToSet == ESetCellParameter.InternalDirection && Value.IsNotConnected) ? 230 : 178, 99); } }
        public override bool IsFoldable { get { return false; } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }

        [HideInInspector][Port(EPortPinType.Input, 1)] public PGGCellPort Cell;
        public enum ESetCellParameter { CellData, InternalDirection, ObjectData }
        [HideInInspector] public ESetCellParameter ParameterToSet = ESetCellParameter.CellData;

        [HideInInspector][Port(EPortPinType.Input)] public PGGStringPort String;
        [HideInInspector][Port(EPortPinType.Input)] public PGGVector3Port Value;
        [HideInInspector] public Object Object;


        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Cell.TriggerReadPort();

            var cell = Cell.GetInputCellValue;

            if (FGenerators.IsNull(cell)) return;

            if (ParameterToSet == ESetCellParameter.CellData)
            {
                //UnityEngine.Debug.Log("add data to cell " + cell.Pos + " hash " + cell.GetHashCode() + " checker hash " + Cell.GetInputCheckerValue.GetHashCode() + "  grid hash " + Cell.GetInputCheckerValue.Grid.GetHashCode() );
                cell.AddCustomData(String.GetInputValue);
            }
            else if (ParameterToSet == ESetCellParameter.InternalDirection)
            {
                cell.HelperVector = (Value.GetInputValue).V3toV3Int();
            }
            else if (ParameterToSet == ESetCellParameter.ObjectData)
            {
                cell.AddCustomObject(Object);
            }

        }


#if UNITY_EDITOR

        private SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            baseSerializedObject.Update();
            if (sp == null) sp = baseSerializedObject.FindProperty("Cell");
            SerializedProperty spc = sp.Copy();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(spc, GUILayout.Width(50)); spc.Next(false);
            EditorGUILayout.PropertyField(spc, GUIContent.none); spc.Next(false);
            EditorGUILayout.EndHorizontal();

            if (ParameterToSet == ESetCellParameter.CellData)
            {
                EditorGUILayout.PropertyField(spc); spc.Next(false);
            }
            else if (ParameterToSet == ESetCellParameter.InternalDirection)
            {
                spc.Next(false); EditorGUILayout.PropertyField(spc);
            }
            else if (ParameterToSet == ESetCellParameter.ObjectData)
            {
                float prelb = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 49;
                spc.Next(false); spc.Next(false); EditorGUILayout.PropertyField(spc);
                EditorGUIUtility.labelWidth = prelb;
            }

            baseSerializedObject.ApplyModifiedProperties();
        }

#endif

    }
}