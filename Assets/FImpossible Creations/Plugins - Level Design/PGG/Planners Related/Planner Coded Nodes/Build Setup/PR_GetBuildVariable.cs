using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.BuildSetup
{

    public class PR_GetBuildVariable : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Get Build Variable"; }
        public override string GetNodeTooltipDescription { get { return "Get variable defined in 'Build Planner' window in 'Build Variables' foldout"; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.4f, 0.4f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(180, 82); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }
        public override EPlannerNodeVisibility NodeVisibility { get { return EPlannerNodeVisibility.JustPlanner; } }

        [HideInInspector] public int VariableID = 0;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGUniversalPort Value;
        //[HideInInspector][Port(EPortPinType.Output, EPortPinDisplay.JustPort)] public FloatPort Random;

        public override void DONT_USE_IT_YET_OnReadPort(IFGraphPort port)
        {
            if (VariableID < 0) return;
            if (ParentPlanner == null) return;
            if (ParentPlanner.ParentBuildPlanner == null) return;
            if (ParentPlanner.ParentBuildPlanner.BuildVariables == null) return;
            if (VariableID >= ParentPlanner.ParentBuildPlanner.BuildVariables.Count) return;

            FieldVariable getVar = ParentPlanner.ParentBuildPlanner.BuildVariables[VariableID];
            Value.Variable.SetValue(getVar);
            base.DONT_USE_IT_YET_OnReadPort(port);
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (sp == null) sp = baseSerializedObject.FindProperty("VariableID");
            SerializedProperty s = sp.Copy();

            if (ParentPlanner != null)
            {
                VariableID = EditorGUILayout.IntPopup(VariableID, ParentPlanner.GetBuildVariablesNameList(), ParentPlanner.GetBuildVariablesIDList(), GUILayout.Width(NodeSize.x-83));
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