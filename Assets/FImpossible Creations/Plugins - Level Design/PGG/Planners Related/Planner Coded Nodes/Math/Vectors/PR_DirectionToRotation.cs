using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Vectors
{

    public class PR_DirectionToRotation : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? " Dir to Rot" : "Convert Direction To Rotation (Look Direction)"; }
        public override string GetNodeTooltipDescription { get { return "Converting direction vector to euler angles.\nUnfolded : Up axis for look direction."; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.45f, 0.8f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 188 : 148, _EditorFoldout ? 102 : 82); } }
        public override bool IsFoldable { get { return true; } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }

        [HideInInspector] [Port(EPortPinType.Input, true)] public PGGVector3Port Direction;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGVector3Port Angles;
        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.HideName)] public PGGVector3Port UpVector;

        public override void OnCreated()
        {
            base.OnCreated();
            UpVector.Value = Vector3.up;
        }

        public override void OnStartReadingNode()
        {
            Direction.TriggerReadPort(true);
            UpVector.TriggerReadPort(true);
            if (Direction.GetInputValue == Vector3.zero) return;
            Angles.Value = Quaternion.LookRotation(Direction.GetInputValue, UpVector.GetInputValue).eulerAngles;
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            baseSerializedObject.Update();

            if (sp == null) sp = baseSerializedObject.FindProperty("Direction");
            SerializedProperty s = sp.Copy();

            EditorGUILayout.PropertyField(s, GUIContent.none);
            s.Next(false);
            GUILayout.Space(-21);
            GUILayout.BeginHorizontal();
            GUILayout.Space(39);
            EditorGUILayout.PropertyField(s, GUIContent.none);
            GUILayout.EndHorizontal();

            UpVector.AllowDragWire = false;

            if (_EditorFoldout)
            {
                UpVector.AllowDragWire = true;
                s.Next(false);
                EditorGUILayout.PropertyField(s);
            }

            baseSerializedObject.ApplyModifiedProperties();
        }
        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            GUILayout.Label("Direction: " + Direction.GetPortValueSafe);
            GUILayout.Label("Out Angles: " + Angles.GetPortValueSafe);
        }

#endif

    }
}