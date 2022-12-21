using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Vectors
{

    public class PR_Append : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Append"; }
        public override string GetNodeTooltipDescription { get { return "Completing X,Y and Z value into new Vector3"; } }

        public override Color GetNodeColor() { return new Color(0.3f, 0.5f, 0.75f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(130, 137); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }

        public enum EOutType
        {
            Vector3, Vector2
        }

        [Port(EPortPinType.Input, EPortNameDisplay.Default, "X", 1, typeof(int))] public FloatPort InValA;
        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, "Y", 1, typeof(int))] public FloatPort InValB;
        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, "Z", 1, typeof(int))] public FloatPort InValC;
        
        [HideInInspector] [Port(EPortPinType.Output, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "Output")] 
        [Tooltip("Outputting Vector3 connected from 3 input ports above")]
        public PGGVector3Port OutV3;


        public override void OnStartReadingNode()
        {
            InValA.TriggerReadPort(true);
            InValB.TriggerReadPort(true);
            InValC.TriggerReadPort(true);

            Vector3 appended = new Vector3();
            appended.x = InValA.GetInputValue;
            appended.y = InValB.GetInputValue;
            appended.z = InValC.GetInputValue;
            OutV3.Value = appended;
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (sp == null) sp = baseSerializedObject.FindProperty("InValA");
            SerializedProperty s = sp.Copy();

            s.Next(false);
            EditorGUILayout.PropertyField(s, GUIContent.none);
            s.Next(false);
            EditorGUILayout.PropertyField(s, GUIContent.none);
            s.Next(false);
            //GUILayout.Space(-21);
            //GUILayout.BeginHorizontal();
            //GUILayout.Space(19);
            EditorGUILayout.PropertyField(s, GUIContent.none);
            //GUILayout.EndHorizontal();
        }
#endif

    }
}