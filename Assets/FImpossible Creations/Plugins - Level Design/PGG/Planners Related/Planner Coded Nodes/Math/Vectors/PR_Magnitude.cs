using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Vectors
{

    public class PR_Magnitude : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Magnitude" : "Magnitude (distance)"; }
        public override string GetNodeTooltipDescription { get { return "Returning distance between two positions"; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.45f, 0.8f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(140, 98); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }

        [HideInInspector] [Port(EPortPinType.Input, true)] public PGGVector3Port A;
        [HideInInspector] [Port(EPortPinType.Input, true)] public PGGVector3Port B;
        [HideInInspector] [Port(EPortPinType.Output, true)] public FloatPort Magnitude;

        public override void DONT_USE_IT_YET_OnReadPort(IFGraphPort port)
        {
            A.TriggerReadPort();
            B.TriggerReadPort();
            Magnitude.Value = (A.GetInputValue - B.GetInputValue).magnitude;
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (sp == null) sp = baseSerializedObject.FindProperty("A");
            SerializedProperty s = sp.Copy();

            EditorGUILayout.PropertyField(s);
            s.Next(false);
            EditorGUILayout.PropertyField(s, GUILayout.Width(NodeSize.x - 80));
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
            GUILayout.Label("A Value: " + A.GetPortValueSafe);
            GUILayout.Label("B Value: " + B.GetPortValueSafe);
            GUILayout.Label("Latest Result: " + Magnitude.GetPortValueSafe);
        }

#endif

    }
}