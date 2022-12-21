using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Vectors
{

    public class PR_AngleDirection : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Angle Direction"; }
        public override string GetNodeTooltipDescription { get { return "Returning 360 degrees normalized direction"; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.45f, 0.8f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(150, 82); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }

        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.HideName, EPortValueDisplay.NotEditable, 1)] public FloatPort InVal;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGVector3Port OutNorm;

        public override void OnStartReadingNode()
        {
            InVal.GetPortValueCall();
            InVal.TriggerReadPort();
            OutNorm.Value = Quaternion.Euler(0f, (float)InVal.GetPortValue, 0f) * Vector3.forward;
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (sp == null) sp = baseSerializedObject.FindProperty("InVal");
            SerializedProperty s = sp.Copy();


            EditorGUILayout.PropertyField(s, GUIContent.none, GUILayout.Width(70));
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
            EditorGUILayout.LabelField("In Value: " + InVal.GetPortValueSafe);
            EditorGUILayout.LabelField("Out Value: " + OutNorm.GetPortValueSafe);
        }
#endif

    }
}