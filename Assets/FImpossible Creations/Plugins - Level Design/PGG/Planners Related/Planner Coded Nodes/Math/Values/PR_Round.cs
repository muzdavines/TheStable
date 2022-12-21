using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Values
{
    public enum ERoundingMode { RoundTo1, RoundToPlannerSize, RoundToCustom }
    public enum ERoundingType { Round, Ceil, Floor }

    public class PR_Round : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Round Value"; }
        public override string GetNodeTooltipDescription { get { return "Rounding provided value"; } }
        public override bool IsFoldable { get { return true; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.5f, 0.75f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(170, _EditorFoldout ? (RoundingMode == ERoundingMode.RoundToCustom ? 140 : 120) : 82); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }

        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "To Round", 1, typeof(int))] public FloatPort InValue;
        [HideInInspector] [Port(EPortPinType.Output, true)] public FloatPort OutNorm;

        [HideInInspector] public ERoundingType RoundingType = ERoundingType.Round;
        [HideInInspector] public ERoundingMode RoundingMode = ERoundingMode.RoundTo1;
        [HideInInspector] public float RoundTo = 1f;

        public override void OnStartReadingNode()
        {
            InValue.TriggerReadPort();
            InValue.GetPortValueCall();

            float outVal = InValue.GetInputValue;

            if (RoundingMode == Values.ERoundingMode.RoundTo1)
            {
                outVal = RoundWith(outVal, RoundingType);
            }
            else if (RoundingMode == Values.ERoundingMode.RoundToCustom)
            {
                outVal = RoundValueTo(outVal, RoundTo);
            }
            else if (RoundingMode == Values.ERoundingMode.RoundToPlannerSize)
            {
                if (FieldPlanner.CurrentGraphExecutingPlanner == null)
                {
                    //UnityEngine.Debug.Log("null current executing planner!");
                    outVal = RoundWith(outVal, RoundingType);
                }
                else
                    outVal = RoundValueTo(outVal, FieldPlanner.CurrentGraphExecutingPlanner.GetScaleF);
            }

            OutNorm.Value = outVal;
        }

        private float RoundValueTo(float toRound, float to)
        {
            return RoundWith(toRound / to, RoundingType) * to;
        }

        public static float RoundWith(float toRound, ERoundingType type)
        {
            if (type == ERoundingType.Round) return Mathf.Round(toRound);
            else if (type == ERoundingType.Ceil) return Mathf.Ceil(toRound);
            else if (type == ERoundingType.Floor) return Mathf.Floor(toRound);
            return toRound;
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            baseSerializedObject.Update();
            base.Editor_OnNodeBodyGUI(setup);

            if (sp == null) sp = baseSerializedObject.FindProperty("InValue");
            SerializedProperty s = sp.Copy();

            //EditorGUILayout.PropertyField(s, GUIContent.none);
            s.Next(false);
            GUILayout.Space(-21);
            GUILayout.BeginHorizontal();
            GUILayout.Space(19);
            EditorGUILayout.PropertyField(s, GUIContent.none);
            GUILayout.EndHorizontal();

            if (_EditorFoldout)
            {
                s.Next(false); EditorGUILayout.PropertyField(s, GUIContent.none);
                s.Next(false); EditorGUILayout.PropertyField(s, GUIContent.none);

                if (RoundingMode == Values.ERoundingMode.RoundToCustom)
                {
                    s.Next(false);
                    EditorGUIUtility.labelWidth = 70;
                    EditorGUILayout.PropertyField(s);
                    EditorGUIUtility.labelWidth = 0;
                }
            }

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}