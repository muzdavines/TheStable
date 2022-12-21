using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Algebra
{

    public class PR_Divide : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Divide"; }
        public override Color GetNodeColor() { return new Color(0.3f, 0.5f, 0.75f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(120, 100); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }

        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "A", 1, typeof(int))] public PGGUniversalPort InValA;
        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "B", 1, typeof(int))] public PGGUniversalPort InValB;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGUniversalPort OutVal;


        public override void OnStartReadingNode()
        {
            InValA.TriggerReadPort();
            InValB.TriggerReadPort();

            InValA.Variable.SetValue(InValA.GetPortValue);
            InValB.Variable.SetValue(InValB.GetPortValue);

            OutVal.Variable.AlgebraOperation(InValA.Variable, InValB.Variable, FieldVariable.EAlgebraOperation.Divide);
        }

#if UNITY_EDITOR
            SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (sp == null) sp = baseSerializedObject.FindProperty("InValB");
            SerializedProperty s = sp.Copy();


            EditorGUILayout.PropertyField(s, GUIContent.none);
            s.Next(false);
            GUILayout.Space(-21);
            GUILayout.BeginHorizontal();
            GUILayout.Space(19);
            EditorGUILayout.PropertyField(s, GUIContent.none);
            GUILayout.EndHorizontal();
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            GUILayout.Label("A Value: " + InValA.Variable.GetValue());
            GUILayout.Label("B Value: " + InValB.Variable.GetValue());

            GUILayout.Label("Latest Result: " + OutVal.Variable.GetValue());
        }
#endif

    }
}