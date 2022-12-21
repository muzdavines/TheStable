using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Vectors
{

    public class PR_RoundPosition : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? " Round Position" : "Round Position"; }
        public override string GetNodeTooltipDescription { get { return "Rounding field origin position"; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.5f, 0.75f, 0.9f); }
        public override Vector2 NodeSize { get { float h = 82f; if (_EditorFoldout) { if (RoundingMode == Values.ERoundingMode.RoundToCustom) h = 141f; else h = 121f; } return new Vector2(172, h); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override bool IsFoldable { get { return true; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }

        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "Position", 1, typeof(int))] public PGGVector3Port PositionValue;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGVector3Port OutNorm;

        [HideInInspector] public Values.ERoundingType RoundingType = Values.ERoundingType.Round;
        [HideInInspector] public Values.ERoundingMode RoundingMode = Values.ERoundingMode.RoundToPlannerSize;
        [HideInInspector] public float RoundTo = 1f;


        public override void OnStartReadingNode()
        {
            PositionValue.TriggerReadPort();
            PositionValue.GetPortValueCall();

            Vector3 outVal = PositionValue.GetInputValue;

            if (RoundingMode == Values.ERoundingMode.RoundTo1)
            {
                outVal = RoundValueTo(outVal, 1);
            }
            else if (RoundingMode == Values.ERoundingMode.RoundToCustom)
            {
                outVal = RoundValueTo(outVal, RoundTo);
            }
            else if (RoundingMode == Values.ERoundingMode.RoundToPlannerSize)
            {
                if (FieldPlanner.CurrentGraphExecutingPlanner == null)
                {
                    outVal = RoundValueTo(outVal, 1);
                }
                else
                    outVal = RoundValueTo(outVal, FieldPlanner.CurrentGraphExecutingPlanner.GetScaleF);
            }

            OutNorm.Value = outVal;
        }

        private Vector3 RoundValueTo(Vector3 toRound, float to)
        {
            if (RoundingType == Values.ERoundingType.Round) return FVectorMethods.FlattenVector(toRound, to);
            else if (RoundingType == Values.ERoundingType.Ceil) return FVectorMethods.FlattenVectorCeil(toRound, to);
            else if (RoundingType == Values.ERoundingType.Floor) return FVectorMethods.FlattenVectorFlr(toRound, to);
            return toRound;
        }




#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            baseSerializedObject.Update();
            base.Editor_OnNodeBodyGUI(setup);

            if (sp == null) sp = baseSerializedObject.FindProperty("PositionValue");
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
                s.Next(false);
                EditorGUILayout.PropertyField(s, GUIContent.none);
                s.Next(false);
                EditorGUILayout.PropertyField(s, GUIContent.none);

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

        public override void Editor_OnAdditionalInspectorGUI()
        {
            if (sp == null) sp = baseSerializedObject.FindProperty("PositionValue");
            baseSerializedObject.Update();

            SerializedProperty s = sp.Copy();
            s.Next(false);
            s.Next(false);

            EditorGUILayout.PropertyField(s);
            s.Next(false); EditorGUILayout.PropertyField(s);

            if (RoundingMode == Values.ERoundingMode.RoundToCustom)
            {

                EditorGUIUtility.labelWidth = 70;
                s.Next(false); EditorGUILayout.PropertyField(s);
                EditorGUIUtility.labelWidth = 0;
            }

            baseSerializedObject.ApplyModifiedProperties();

            GUILayout.Space(5);

            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            EditorGUILayout.LabelField("Value: " + PositionValue.GetPortValueSafe);
            EditorGUILayout.LabelField("Out Value: " + OutNorm.GetPortValueSafe);
        }

#endif

    }
}