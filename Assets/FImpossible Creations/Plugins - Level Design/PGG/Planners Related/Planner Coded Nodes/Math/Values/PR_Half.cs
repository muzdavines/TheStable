using FIMSpace.Graph;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Values
{
    public class PR_Half : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Half Value"; }
        public override string GetNodeTooltipDescription { get { return "Dividing provided value by 2"; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.5f, 0.75f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(136, 77); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }

        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue)] public PGGUniversalPort In;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGUniversalPort Out;

        [NonSerialized] protected FieldVariable halfVar = null;

        void CheckForHalfVarRefresh()
        {
            if (halfVar == null)
            {
                halfVar = new FieldVariable("", 0.5f);
                halfVar.SetInternalV3Value(new Vector3(0.5f, 0.5f, 0.5f));
            }
        }

        public override void OnStartReadingNode()
        {
            CheckForHalfVarRefresh();

            In.TriggerReadPort();
            var inRead = In.GetPortValueSafe;
            In.Variable.SetValue(inRead);
            Out.Variable.SetValue(inRead);
            Out.Variable.AlgebraOperation(In.Variable, halfVar, FieldVariable.EAlgebraOperation.Multiply);
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (sp == null) sp = baseSerializedObject.FindProperty("In");
            SerializedProperty s = sp.Copy();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(s, GUILayout.Width(NodeSize.x - 80));
            GUILayout.EndHorizontal();

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
            EditorGUILayout.LabelField("In Value: " + In.GetPortValueSafe);
            EditorGUILayout.LabelField("Out Value: " + Out.GetPortValueSafe);
        }

#endif
    }
}