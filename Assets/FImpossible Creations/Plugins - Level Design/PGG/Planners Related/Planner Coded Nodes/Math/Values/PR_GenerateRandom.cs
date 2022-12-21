using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Values
{

    public class PR_GenerateRandom : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Generate Random"; }
        public override string GetNodeTooltipDescription { get { return "Get random value out of choosed values"; } }

        public override Color GetNodeColor() { return new Color(0.3f, 0.5f, 0.75f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(182, 101); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        //public override bool IsFoldable { get { return true; } }


        [Port(EPortPinType.Input, 1)] public FloatPort From;
        [HideInInspector][Port(EPortPinType.Input, 1)] public FloatPort To;
       [HideInInspector] [Port(EPortPinType.Output, true)] public FloatPort Random;

        public override void OnStartReadingNode()
        {
            From.TriggerReadPort(true);
            To.TriggerReadPort(true);
            Random.Value = FGenerators.GetRandom(From.GetInputValue, To.GetInputValue);
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            baseSerializedObject.Update();
            if (sp == null) sp = baseSerializedObject.FindProperty("To");

            SerializedProperty spc = sp.Copy();
            EditorGUILayout.PropertyField(spc, GUILayout.Width(NodeSize.x - 80));
            spc.Next(false);

            GUILayout.Space(-21);
            GUILayout.BeginHorizontal();
            GUILayout.Space(-26);
            EditorGUILayout.PropertyField(spc, GUIContent.none);
            GUILayout.EndHorizontal();

            baseSerializedObject.ApplyModifiedProperties();
        }

#endif

    }
}