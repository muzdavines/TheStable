using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.Utilities
{

    public class PR_ToString : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "To String" : "Convert Value to String"; }
        public override string GetNodeTooltipDescription { get { return "Convert any port value to the string"; } }
        public override Color GetNodeColor() { return new Color(0.4f, 0.4f, 0.4f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(158, 84); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Debug; } }

        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.HideOnConnected, 1)] public PGGUniversalPort Value;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGStringPort Text;

        public override void OnStartReadingNode()
        {
            Value.TriggerReadPort();
            Text.StringVal = Value.GetPortValueSafe.ToString();
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (sp == null) sp = baseSerializedObject.FindProperty("Value");
            EditorGUIUtility.labelWidth = 1;
            EditorGUILayout.PropertyField(sp, GUILayout.Width(NodeSize.x - 80));
            EditorGUIUtility.labelWidth = 0;

            SerializedProperty spc = sp.Copy(); spc.Next(false);
            GUILayout.Space(-19);
            EditorGUILayout.PropertyField(spc);
        }
#endif

    }
}