using FIMSpace.Graph;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field
{

    public class PR_SetLocalVariable : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Set Local Variable"; }
        public override Color GetNodeColor() { return new Color(1.0f, 0.4f, 0.4f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(190, 82); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }

        [HideInInspector] [Port(EPortPinType.Input, true, 1)] public PGGUniversalPort Input;
        [HideInInspector] public string VName = "Variable Name";
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGUniversalPort Value;
        [NonSerialized] public FieldVariable OverrideVariable = null;
        public override void OnStartReadingNode()
        {
            Input.TriggerReadPort(true);
            Value.Variable.SetValue(Input.GetPortValueSafe);
            //if (Input.Connections.Count > 0)
            //{
            //    NodePortBase oPort = Input.Connections[0].PortReference as NodePortBase;
            //    if (oPort == null) return;

            //    oPort.TriggerReadPort(true);
            //    Value.Variable.SetValue(oPort.GetPortValueSafe);
            //}
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            baseSerializedObject.Update();

            if (sp == null) sp = baseSerializedObject.FindProperty("Input");
            SerializedProperty s = sp.Copy();


            EditorGUILayout.PropertyField(s, GUIContent.none);
            s.Next(false);

            GUILayout.Space(-21);
            GUILayout.BeginHorizontal();
            GUILayout.Space(26);
            EditorGUILayout.PropertyField(s, GUIContent.none, GUILayout.Width(NodeSize.x - 110));
            GUILayout.EndHorizontal();


            s.Next(false);
            GUILayout.Space(-21);
            GUILayout.BeginHorizontal();
            GUILayout.Space(-27);
            EditorGUILayout.PropertyField(s, GUIContent.none);
            GUILayout.EndHorizontal();

            //GUILayout.Space(-21);
            //EditorGUILayout.PropertyField(s, GUIContent.none);
            baseSerializedObject.ApplyModifiedProperties();
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            GUILayout.Label("Input: " + Input.GetPortValueSafe);
            GUILayout.Label("Latest Val: " + Value.Variable.GetValue());
        }

#endif

    }
}