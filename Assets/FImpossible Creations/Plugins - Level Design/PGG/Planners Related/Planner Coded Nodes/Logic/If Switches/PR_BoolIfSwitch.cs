using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Logic
{

    public class PR_BoolIfSwitch : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Output A or B" : "If true\\false => Output A\\B value" ; }
        public override Color GetNodeColor() { return new Color(0.3f, 0.8f, 0.55f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(200, 120); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Logic; } }

        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "A", 1, typeof(int))] public PGGUniversalPort AValue;
        [HideInInspector][Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "B", 1, typeof(int))] public PGGUniversalPort BValue;
        [HideInInspector][Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "False: A  True: B")] public BoolPort OutputBValue;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGUniversalPort Output;


        public override void OnStartReadingNode()
        {
            AValue.TriggerReadPort();
            BValue.TriggerReadPort();
            OutputBValue.TriggerReadPort();

            AValue.GetPortValueCall();
            BValue.GetPortValueCall();
            OutputBValue.GetPortValueCall();

            if (OutputBValue.GetInputValue == true)
            {
                Output.Variable.SetValue(BValue.GetPortValueSafe);
            }
            else
            {
                Output.Variable.SetValue(AValue.GetPortValueSafe);
            }

        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            baseSerializedObject.Update();
            if (sp == null) sp = baseSerializedObject.FindProperty("AValue");
            SerializedProperty s = sp.Copy();

            s.Next(false);
            EditorGUILayout.PropertyField(s, GUIContent.none);
            s.Next(false);
            EditorGUILayout.PropertyField(s, GUIContent.none);
            s.Next(false);
            GUILayout.Space(-21);
            GUILayout.BeginHorizontal();
            GUILayout.Space(19);
            EditorGUILayout.PropertyField(s, GUIContent.none);
            GUILayout.EndHorizontal();
            baseSerializedObject.ApplyModifiedProperties();
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            EditorGUILayout.LabelField("A Value: " + AValue.GetPortValueSafe);
            EditorGUILayout.LabelField("B Value: " + BValue.GetPortValueSafe);
            EditorGUILayout.LabelField("Switch Value: " + OutputBValue.GetPortValueSafe);
            EditorGUILayout.LabelField("Out Value: " + Output.Variable.GetValue());
        }

#endif

    }
}