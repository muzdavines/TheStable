using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Values
{

    public class PR_GetValueByAxis : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Get Value by Axis" : "Get Value using Axis"; }
        public override string GetNodeTooltipDescription { get { return "Getting single dimension value using axis, if value is (1.5, 0.4, 0) and axis is (0, 1, 0) then it will return (0, 0.4, 0)"; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.5f, 0.75f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(190, _EditorFoldout ? 162 : 142); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override bool IsFoldable { get { return true; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }


        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "Value", 1, typeof(int))] public PGGVector3Port InVal;
        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "Axis", 1, typeof(int))] public PGGVector3Port InAxis;
        [Port(EPortPinType.Output, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "Selective Out")] public PGGVector3Port OutV3;
        [Port(EPortPinType.Output, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "Selected Value")] public FloatPort OutD;
        [HideInInspector] public bool SignAsAxis = true;

        public override void OnStartReadingNode()
        {
            InVal.TriggerReadPort(true);
            InAxis.TriggerReadPort(true);

            var inVal = InVal.GetInputValue;
            //var inVal = InVal.GetPortValue;
            var inAxis = InAxis.GetInputValue;

            if (inAxis == Vector3.zero)
            {
                OutV3.Value = inVal;
                return;
            }

            OutV3.Value = FVectorMethods.ChooseDominantAxis(inAxis);

            Vector3 absV = new Vector3(Mathf.Abs(OutV3.Value.x),
                Mathf.Abs(OutV3.Value.y),
                Mathf.Abs(OutV3.Value.z));

            float val = 0f;

            if (absV.x > absV.y)
            {
                if (absV.z > absV.x)
                {
                    val = inVal.z;
                    if (SignAsAxis) if (inAxis.z < 0f) if (val > 0f) val = -val;
                    OutV3.Value = new Vector3(0, 0, val);
                }
                else
                {
                    val = inVal.x;
                    if (SignAsAxis) if (inAxis.x < 0f) if (val > 0f) val = -val;
                    OutV3.Value = new Vector3(val, 0, 0);
                }
            }
            else
            {
                if (absV.z > absV.y)
                {
                    val = inVal.z;
                    if (SignAsAxis) if (inAxis.z < 0f) if (val > 0f) val = -val;
                    OutV3.Value = new Vector3(0, 0, val);
                }
                else
                {
                    val = inVal.y;
                    if (SignAsAxis) if (inAxis.y < 0f) if (val > 0f) val = -val;
                    OutV3.Value = new Vector3(0, val, 0);
                }
            }

            OutD.Value = val;
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (_EditorFoldout)
            {
                baseSerializedObject.Update();
                if (sp == null) sp = baseSerializedObject.FindProperty("SignAsAxis");
                EditorGUILayout.PropertyField(sp, true);
                baseSerializedObject.ApplyModifiedProperties();
            }
        }
#endif


#if UNITY_EDITOR

        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            GUILayout.Label("InVal Value: " + InVal.GetPortValueSafe);
            GUILayout.Label("InAxis Value: " + InAxis.GetPortValueSafe);

            GUILayout.Label("Selective: " + OutV3.Value);
            GUILayout.Label("Selective Val: " + OutD.Value);
        }

#endif

    }
}