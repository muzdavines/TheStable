using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Vectors
{

    public class PR_WrapAngle : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Wrap Angle" : "Wrap Angle to 0-360 or 180"; }
        public override string GetNodeTooltipDescription { get { return "Changing degrees value to be 0-360 or -180,180"; } }

        public override Color GetNodeColor() { return new Color(0.3f, 0.5f, 0.75f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(200, 77); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }

        public enum EOutType
        {
            ZeroTo360, PlusMinus180, PlusMinus360
        }

        [HideInInspector] [Port(EPortPinType.Input, true)] public FloatPort InValA;
        [HideInInspector] public EOutType OutType = EOutType.ZeroTo360;
        [HideInInspector] [Port(EPortPinType.Output, true)] public FloatPort Output;


        public override void OnStartReadingNode()
        {
            InValA.TriggerReadPort(true);

            float angle = InValA.GetInputValue;
            angle = FEngineering.WrapAngle(angle);

            if ( OutType == EOutType.ZeroTo360)
            {
                if (angle < 0) angle += 360f;
            }
            else if (OutType == EOutType.PlusMinus180)
            {
                if (angle < -180f) angle += 360f;
                if (angle > 180f) angle -= 360f;
            }

            Output.Value = angle;
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (sp == null) sp = baseSerializedObject.FindProperty("InValA");
            SerializedProperty s = sp.Copy();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(s, GUIContent.none, GUILayout.Width(24));
            s.Next(false);
            EditorGUILayout.PropertyField(s, GUIContent.none, GUILayout.Width(NodeSize.x - 107));
            s.Next(false);
            GUILayout.EndHorizontal();
            GUILayout.Space(-21);
            GUILayout.BeginHorizontal();
            GUILayout.Space(19);
            EditorGUILayout.PropertyField(s, GUIContent.none);
            GUILayout.EndHorizontal();

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}