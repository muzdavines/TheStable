using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Vectors
{

    public class PR_Normalize : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Normalize"; }
        public override string GetNodeTooltipDescription { get { return "Normalizing vector to have length = 1"; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.45f, 0.8f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(140, 82); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }

        [HideInInspector] [Port(EPortPinType.Input, true)] public PGGVector3Port InVal;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGVector3Port OutNorm;

        public override void DONT_USE_IT_YET_OnReadPort(IFGraphPort port)
        {
            InVal.TriggerReadPort();
            OutNorm.Value = InVal.GetInputValue.normalized;
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
            GUILayout.Label("Latest Result: " + OutNorm.GetPortValueSafe);
        }

#endif

    }
}