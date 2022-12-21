using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells
{

    public class PR_GetCellParameters : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Cell Parameters" : "Get Cell Parameters"; }
        public override string GetNodeTooltipDescription { get { return "Accessing some parameters of provided cell"; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(CellParameter == ECellParameter.InternalCellDirection ? 230 : 190, 100); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override bool IsFoldable { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }

        public enum ECellParameter
        {
            InFieldLocalPos, WorldPosition, Owner, InternalCellDirection
        }

        [HideInInspector][Port(EPortPinType.Input)] public PGGCellPort Cell;
        [HideInInspector] public ECellParameter CellParameter = ECellParameter.WorldPosition;

        [HideInInspector][Port(EPortPinType.Output, EPortNameDisplay.Default, EPortValueDisplay.HideValue)] public PGGVector3Port Output;
        [HideInInspector][Port(EPortPinType.Output, EPortNameDisplay.Default, EPortValueDisplay.HideValue)] public PGGPlannerPort Owner;
        Vector3 read = Vector3.zero;

        public override void OnStartReadingNode()
        {
            Cell.TriggerReadPort(true);
            Output.Value = Vector3.zero;
            Owner.Clear();

            if ( CellParameter == ECellParameter.Owner)
            {
                PlannerResult res = Cell.GetInputResultValue;
                if (res == null) return;
                Owner.SetIDsOfPlanner(res.ParentFieldPlanner);
            }
            else
            {
                var cell = Cell.GetInputCellValue;
                if (cell == null) return;

                var checker = Cell.GetInputCheckerValue;

                Vector3 size = Vector3.one;
                if (checker != null) size = checker.RootScale;

                if (CellParameter == ECellParameter.InFieldLocalPos) Output.Value = cell.Pos;
                else if (CellParameter == ECellParameter.WorldPosition)
                {
                    if ( checker != null)
                    {
                        Output.Value = checker.GetWorldPos(cell);
                    }
                    else
                    {
                        Output.Value = cell.WorldPos(size);
                    }
                }
                else if (CellParameter == ECellParameter.InternalCellDirection) Output.Value = cell.HelperDirection;
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


            if (CellParameter == ECellParameter.Owner)
            {
                spc.Next(false); EditorGUILayout.PropertyField(spc);
            }
            else
            {
                EditorGUILayout.PropertyField(spc); spc.Next(false);
            }


            baseSerializedObject.ApplyModifiedProperties();
        }


        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);

            if (Cell.GetInputCellValue == null) EditorGUILayout.LabelField("NULL CELL!");
            else
            {
                EditorGUILayout.LabelField("Out Value: " + read);
            }
        }
#endif

    }
}