using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Values
{

    public class PR_Value : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? " Value" : "Any value type output"; }
        public override string GetNodeTooltipDescription { get { return "Choose type set value and output it"; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.5f, 0.75f, 0.9f); }
        public override Vector2 NodeSize { get { float w = 130f; if (InputType == EType.Vector3) w = 210; else if (InputType == EType.Bool) if (_EditorFoldout == false) w = 114;  return new Vector2( w, _EditorFoldout ? 102 : 82); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override bool IsFoldable { get { return true; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }


        public EType InputType = EType.Number;

        [HideInInspector] [Port(EPortPinType.Output, EPortNameDisplay.HideName, EPortValueDisplay.AlwaysEditable)] public IntPort IntInput;
        [HideInInspector] [Port(EPortPinType.Output, EPortNameDisplay.HideName, EPortValueDisplay.AlwaysEditable)] public BoolPort BoolInput;
        [HideInInspector] [Port(EPortPinType.Output, EPortNameDisplay.HideName, EPortValueDisplay.AlwaysEditable)] public FloatPort FloatInput;
        [HideInInspector] [Port(EPortPinType.Output, EPortNameDisplay.HideName, EPortValueDisplay.AlwaysEditable)] public PGGVector3Port Vector3Input;
        [HideInInspector] [Port(EPortPinType.Output, EPortNameDisplay.HideName, EPortValueDisplay.AlwaysEditable)] public PGGStringPort StringPort;

        public override void OnCreated()
        {
            _EditorFoldout = true;
            base.OnCreated();
        }


#if UNITY_EDITOR
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            if ( _EditorFoldout) InputType = (EType)UnityEditor.EditorGUILayout.EnumPopup(InputType, GUILayout.Width(NodeSize.x - 60));

            //GUILayout.Space(-20);
            NodePortBase port = null;

            IntInput.AllowDragWire = false;
            BoolInput.AllowDragWire = false;
            FloatInput.AllowDragWire = false;
            Vector3Input.AllowDragWire = false;
            StringPort.AllowDragWire = false;

            if (InputType == EType.Cell)
            {
                UnityEngine.Debug.Log("Cell port is not supported with 'Value' node");
                InputType = EType.String;
            }

            switch (InputType)
            {
                case EType.Int: port = IntInput; EditorGUILayout.PropertyField(baseSerializedObject.FindProperty("IntInput")); break;
                case EType.Bool: port = BoolInput; EditorGUILayout.PropertyField(baseSerializedObject.FindProperty("BoolInput")); break;
                case EType.Number: port = FloatInput; EditorGUILayout.PropertyField(baseSerializedObject.FindProperty("FloatInput")); break;
                case EType.Vector3: port = Vector3Input; EditorGUILayout.PropertyField(baseSerializedObject.FindProperty("Vector3Input")); break;
                case EType.String: port = StringPort; EditorGUILayout.PropertyField(baseSerializedObject.FindProperty("StringPort")); break;
            }

            if (port != null) port.AllowDragWire = true;
        }
#endif

    }
}