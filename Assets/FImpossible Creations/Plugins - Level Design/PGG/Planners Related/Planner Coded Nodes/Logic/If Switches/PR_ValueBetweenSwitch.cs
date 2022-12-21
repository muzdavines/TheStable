using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Logic
{

    public class PR_ValueBetweenSwitch : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? ( "If Between : False/True") : "If Value Between switch => return True or False"; }
        public override Color GetNodeColor() { return new Color(0.3f, 0.8f, 0.55f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(200, 121); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Logic; } }

        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "A", 1)] public FloatPort AValue;
        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.Default, "B", 1)] public FloatPort BValue;
        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.Default, "C", 1)] public FloatPort CValue;
        [HideInInspector] [Port(EPortPinType.Output, true)] public BoolPort Output;


        bool GetResult()
        {
            float a = AValue.GetInputValue;
            return a > BValue.GetInputValue && a < CValue.GetInputValue;
        }

        public override void OnStartReadingNode()
        {
            AValue.TriggerReadPort();
            BValue.TriggerReadPort();
            CValue.TriggerReadPort();
            Output.Value = GetResult();
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        //SerializedProperty spLog = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            baseSerializedObject.Update();
            if (sp == null) sp = baseSerializedObject.FindProperty("AValue");
            SerializedProperty s = sp.Copy();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(s, GUIContent.none, GUILayout.Width(32));
            EditorGUILayout.LabelField(" >  " + System.Math.Round(BValue.GetInputValue,2) + "  AND   <  " + System.Math.Round(CValue.GetInputValue, 2), GUILayout.Width(NodeSize.x-60));
            GUILayout.EndHorizontal();
            s.Next(false);
            EditorGUILayout.PropertyField(s, GUILayout.Width(NodeSize.x - 84));
            s.Next(false);
            EditorGUILayout.PropertyField(s, GUILayout.Width(NodeSize.x - 84));
            s.Next(false);
            GUILayout.Space(-19);
            EditorGUILayout.PropertyField(s);
            baseSerializedObject.ApplyModifiedProperties();
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            EditorGUILayout.LabelField("In A: " + AValue.GetPortValueSafe);
            EditorGUILayout.LabelField("In B: " + BValue.GetPortValueSafe);
            EditorGUILayout.LabelField("In C: " + CValue.GetPortValueSafe);

            GUILayout.Space(4);
            EditorGUILayout.LabelField("Actual Out: " + Output.Value);

        }

#endif

    }
}