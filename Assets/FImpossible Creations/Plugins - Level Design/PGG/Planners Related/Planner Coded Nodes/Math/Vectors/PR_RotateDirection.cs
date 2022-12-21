using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Vectors
{

    public class PR_RotateDirection : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Rotate Direction"; }
        public override string GetNodeTooltipDescription { get { return "Offsetting provided direction with provided angles in degrees"; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.45f, 0.8f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(176, 102); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }

        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue)] public PGGVector3Port Direction;
        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue)] public PGGVector3Port EulerAngles;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGVector3Port Out;

        public override void DONT_USE_IT_YET_OnReadPort(IFGraphPort port)
        {
            Direction.TriggerReadPort();
            EulerAngles.TriggerReadPort();
            Out.Value = Quaternion.Euler(EulerAngles.GetInputValue) * Direction.GetInputValue;
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (sp == null) sp = baseSerializedObject.FindProperty("Direction");
            SerializedProperty s = sp.Copy();
            EditorGUILayout.PropertyField(s);
            s.Next(false);
            EditorGUILayout.PropertyField(s);
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
            GUILayout.Label("Direction: " + Direction.GetPortValueSafe);
            GUILayout.Label("EulerAngles: " + EulerAngles.GetPortValueSafe);
            GUILayout.Label("Out: " + Out.Value);
        }

#endif

    }
}