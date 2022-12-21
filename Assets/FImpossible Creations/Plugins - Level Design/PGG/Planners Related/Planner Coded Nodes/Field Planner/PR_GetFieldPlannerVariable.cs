using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field
{

    public class PR_GetFieldPlannerVariable : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Get Field Variable" : "Get Field Planner Variable"; }
        public override Color GetNodeColor() { return new Color(1.0f, 0.4f, 0.4f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(194, 82); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }
        public override EPlannerNodeVisibility NodeVisibility { get { return EPlannerNodeVisibility.JustPlanner; } }

        [Port(EPortPinType.Input, EPortNameDisplay.HideName, EPortValueDisplay.HideValue)] public PGGPlannerPort Planner;
        [HideInInspector] public int VariableID = 0;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGUniversalPort Value;
        //[HideInInspector][Port(EPortPinType.Output, EPortPinDisplay.JustPort)] public FloatPort Random;
        FieldPlanner latestPlanner = null;
        [SerializeField] [HideInInspector] private bool displayNumber = false;

        public override void OnStartReadingNode()
        {
            latestPlanner = GetPlannerFromPort(Planner);
        }

        public FieldPlanner TargetPlanner()
        {
            if (latestPlanner == null) latestPlanner = ParentPlanner;
            return latestPlanner;
        }

        public override void DONT_USE_IT_YET_OnReadPort(IFGraphPort port)
        {
            if (VariableID < 0) return;
            FieldPlanner plan = TargetPlanner();
            if (plan == null) return;
            if (plan.Variables == null) return;
            if (VariableID >= plan.Variables.Count) return;


            FieldVariable getVar = plan.Variables[VariableID];
            Value.Variable.SetValue(getVar);
            base.DONT_USE_IT_YET_OnReadPort(port);
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        SerializedProperty sp2 = null;

        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            //base.Editor_OnNodeBodyGUI(setup);
            FieldPlanner plan = TargetPlanner();

            if (sp == null) sp = baseSerializedObject.FindProperty("VariableID");
            SerializedProperty s = sp.Copy();

            if (sp2 == null) sp2 = baseSerializedObject.FindProperty("Planner");

            //UnityEditor.EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(sp2, GUILayout.Width(20));

            if (plan != null)
            {
                //if (plan.IndexOnPreset != ParentPlanner.IndexOnPreset)
                if (GUILayout.Button(new GUIContent(".", "Click on dot to display variable id with number instead of selector - it can be useful when you get variables of other Field Planner"), EditorStyles.boldLabel, GUILayout.Width(5))) displayNumber = !displayNumber;

                if (plan.IndexOnPreset != ParentPlanner.IndexOnPreset || displayNumber)
                {
                    float lbW = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 20;
                    VariableID = EditorGUILayout.IntField("ID:", VariableID, GUILayout.Width(NodeSize.x - 113));
                    EditorGUIUtility.labelWidth = lbW;
                }
                else
                    VariableID = EditorGUILayout.IntPopup(VariableID, plan.GetVariablesNameList(), plan.GetVariablesIDList(), GUILayout.Width(NodeSize.x - 113));
            }
            else
            {
                EditorGUILayout.LabelField("error");
            }

            //EditorGUILayout.PropertyField(s, GUIContent.none, GUILayout.Width(NodeSize.x - 84));
            EditorGUILayout.EndHorizontal();

            s.Next(false);
            GUILayout.Space(-21);
            GUILayout.BeginHorizontal();
            GUILayout.Space(-27);
            EditorGUILayout.PropertyField(s, GUIContent.none);
            GUILayout.EndHorizontal();
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            GUILayout.Label("Input: " + Value.Variable.GetValue() );
        }

#endif

    }
}