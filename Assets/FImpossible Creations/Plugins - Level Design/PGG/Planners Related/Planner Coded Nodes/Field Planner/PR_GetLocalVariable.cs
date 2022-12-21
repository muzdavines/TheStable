using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field
{

    public class PR_GetLocalVariable : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Get Local Variable"; }
        public override Color GetNodeColor() { return new Color(1.0f, 0.4f, 0.4f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(170, 82); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }

        [HideInInspector] public int VariableID = 0;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGUniversalPort Value;


        public override void OnStartReadingNode()
        {
            IPlanNodesContainer container = FieldPlanner.GetNodesContainer(this);
            if (container == null) { return; }

            PR_SetLocalVariable variable = container.GraphLocalVariables.GetLocalVar(VariableID);
            if (variable == null) { return; }

            if (variable.OverrideVariable == null)
            {
                variable.OnStartReadingNode();
                variable.Input.TriggerReadPort(true);
                Value.Variable.SetValue(variable.Value.Variable);
            }
            else
            {
                Value.Variable.SetValue(variable.OverrideVariable);
            }
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (sp == null) sp = baseSerializedObject.FindProperty("VariableID");
            SerializedProperty s = sp.Copy();

            IPlanNodesContainer container = FieldPlanner.GetNodesContainer(this);

            if (container != null)
            {
                if (container.GraphLocalVariables == null) UnityEngine.Debug.Log("nul");
                VariableID = EditorGUILayout.IntPopup(VariableID, container.GraphLocalVariables.GetLocalVarsNameList(), container.GraphLocalVariables.GetLocalVarIDList(), GUILayout.Width(NodeSize.x - 83));
            }
            else
            {
                EditorGUILayout.LabelField("error");
            }

            //EditorGUILayout.PropertyField(s, GUIContent.none, GUILayout.Width(NodeSize.x - 84));

            s.Next(false);
            GUILayout.Space(-21);
            GUILayout.BeginHorizontal();
            GUILayout.Space(-27);
            EditorGUILayout.PropertyField(s, GUIContent.none);
            GUILayout.EndHorizontal();
        }
#endif

    }
}