using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Vectors
{

    public class PR_DotProduct : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? (" " + Mode.ToString() + " Product") : "Dot or Angle Vector3 Product"; }
        public override string GetNodeTooltipDescription { get { return "Choosing biggest value axis, like when X = 0.6 Y = 0.2 Z = 0.3 then it will choose Vector3(1,0,0)"; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.45f, 0.8f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(219, _EditorFoldout ? 162 : 142); } }
        public override bool IsFoldable { get { return true; } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }

        public enum EDotMode { Dot, Angle }


        [Port(EPortPinType.Input)] public PGGVector3Port From;
        [Port(EPortPinType.Input)] public PGGVector3Port To;
        public bool Normalize = true;
        [Port(EPortPinType.Output, EPortValueDisplay.NotEditable)] public FloatPort Result;
        [HideInInspector] public EDotMode Mode = EDotMode.Angle;

        public override void DONT_USE_IT_YET_OnReadPort(IFGraphPort port)
        {
            From.TriggerReadPort(true);
            To.TriggerReadPort(true);
            
            if (Normalize)
            {
                Result.Value = ComputeProduct(From.GetInputValue.normalized, To.GetInputValue.normalized);
            }
            else
            {
                Result.Value = ComputeProduct(From.GetInputValue, To.GetInputValue);
            }
        }

        public float ComputeProduct(Vector3 lhs, Vector3 rhs)
        {
            if (Mode == EDotMode.Dot) return Vector3.Dot(lhs, rhs);
            else return Vector3.Angle(lhs, rhs);
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (sp == null) sp = baseSerializedObject.FindProperty("Mode");

            if (_EditorFoldout)
            {
                baseSerializedObject.Update();
                EditorGUIUtility.labelWidth = 40;
                EditorGUILayout.PropertyField(sp);
                EditorGUIUtility.labelWidth = 0;
                baseSerializedObject.ApplyModifiedProperties();
            }

        }
        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            GUILayout.Label("From: " + From.GetPortValueSafe);
            GUILayout.Label("To: " + To.GetPortValueSafe);
            GUILayout.Label("Latest Result: " + Result.GetPortValueSafe);
        }

#endif

    }
}