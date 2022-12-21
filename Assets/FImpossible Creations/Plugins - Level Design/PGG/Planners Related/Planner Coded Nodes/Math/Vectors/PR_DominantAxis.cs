using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Vectors
{

    public class PR_DominantAxis : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Dominant Axis"; }
        public override string GetNodeTooltipDescription { get { return "Choosing biggest value axis, like when X = 0.6 Y = 0.2 Z = 0.3 then it will choose Vector3(1,0,0)"; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.45f, 0.8f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(148, 82); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }

        [HideInInspector] [Port(EPortPinType.Input, true)] public PGGVector3Port InVal;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGVector3Port Dominant;

        public override void DONT_USE_IT_YET_OnReadPort(IFGraphPort port)
        {
            InVal.TriggerReadPort();
            Dominant.Value = FVectorMethods.ChooseDominantAxis(InVal.GetInputValue);
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (sp == null) sp = baseSerializedObject.FindProperty("InVal");
            SerializedProperty s = sp.Copy();


            EditorGUILayout.PropertyField(s, GUIContent.none);
            s.Next(false);
            GUILayout.Space(-21);
            GUILayout.BeginHorizontal();
            GUILayout.Space(39);
            EditorGUILayout.PropertyField(s, GUIContent.none);
            GUILayout.EndHorizontal();
        }
        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            GUILayout.Label("In Value: " + InVal.GetPortValueSafe);
            GUILayout.Label("Latest Result: " + Dominant.GetPortValueSafe);
        }

#endif

    }
}