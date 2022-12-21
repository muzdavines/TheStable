using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field
{

    public class PR_SetLocalVariableAlloc : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Set Local Variable Allocated"; }
        public override string GetNodeTooltipDescription { get { return "Changing dynamically local variable value"; } }

        public override Color GetNodeColor() { return new Color(1.0f, 0.4f, 0.4f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(240, 82); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }

        [HideInInspector] [Port(EPortPinType.Input, true, 1)] public PGGUniversalPort Input;
        [HideInInspector] public int VariableID = 0;

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            IPlanNodesContainer container = FieldPlanner.GetNodesContainer(this);
            if (container == null) { return; }
            
            PR_SetLocalVariable variable = container.GraphLocalVariables.GetLocalVar(VariableID);
            if (variable == null) { return; }

            Input.TriggerReadPort(true);
            Input.Variable.SetValue(Input.GetPortValue);

            if (variable.OverrideVariable == null) variable.OverrideVariable = new FieldVariable();
            variable.OverrideVariable.SetValue(Input.Variable);
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (sp == null) sp = baseSerializedObject.FindProperty("Input");
            SerializedProperty s = sp.Copy();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(s, GUILayout.Width(25));

            IPlanNodesContainer container = FieldPlanner.GetNodesContainer(this);

            if (container != null)
            {
                VariableID = EditorGUILayout.IntPopup(VariableID, container.GraphLocalVariables.GetLocalVarsNameList(), container.GraphLocalVariables.GetLocalVarIDList(), GUILayout.Width(NodeSize.x - 83));
            }
            else
            {
                EditorGUILayout.LabelField("error");
            }

            GUILayout.EndHorizontal();
        }

#endif

    }
}