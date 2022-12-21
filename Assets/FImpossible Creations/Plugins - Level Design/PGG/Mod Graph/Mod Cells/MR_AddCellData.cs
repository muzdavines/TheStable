using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using FIMSpace.Generating.Planner.Nodes;
using FIMSpace.Generating.Rules;

namespace FIMSpace.Generating.Planning.ModNodes.Operations
{

    public class MR_AddCellData : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "   Add Cell Data" : "Add Cell Data"; }
        public override string GetNodeTooltipDescription { get { return "Adding Cell Data string to provided cell which can be used by other spawners rules"; } }
        public override Color GetNodeColor() { return new Color(0.7f, 0.55f, 0.25f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(184, _EditorFoldout ? 100 : 82); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }

        [Port(EPortPinType.Input, 1)] public PGGStringPort DataString;
        [HideInInspector] [Port(EPortPinType.Input, 1)] public PGGModCellPort Cell;
        //[HideInInspector] [Port(EPortPinType.Input, 1)] public PGGVector3Port CommandDirection;

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Cell.TriggerReadPort(true);
            var cell = Cell.GetInputCellValue;
            if (FGenerators.IsNull(cell)) cell = MG_Cell;

            if (cell == null) return;

            DataString.TriggerReadPort(true);
            string str = DataString.GetInputValue;

            if (!string.IsNullOrEmpty(str))
            {
                //CommandDirection.TriggerReadPort(true);
                //if (CommandDirection.Connections.Count > 0)
                //{
                //    cell.AddCustomData(str);

                //}
                //else
                {
                    cell.AddCustomData(str);
                }
            }
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