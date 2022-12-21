using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Values
{

    public class PR_Split : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Split"; }
        public override string GetNodeTooltipDescription { get { return "Split vector 3 onto X, Y and Z ports"; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.5f, 0.75f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(136, 142); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }

        public enum EOutType
        {
            Vector3, Vector2
        }

        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "In Value", 1, typeof(int))] public PGGVector3Port InValA;
        //[HideInInspector][Port(EPortPinType.Input, EPortPinDisplay.Default, "Y", 1, typeof(int))] public FloatPort InValB;
        //[HideInInspector][Port(EPortPinType.Input, EPortPinDisplay.Default, "Z", 1, typeof(int))] public FloatPort InValC;
        [HideInInspector] [Port(EPortPinType.Output, EPortNameDisplay.Default, EPortValueDisplay.NotEditable, "X")] public FloatPort OutA;
        [HideInInspector] [Port(EPortPinType.Output, EPortNameDisplay.Default, EPortValueDisplay.NotEditable, "Y")] public FloatPort OutB;
        [HideInInspector] [Port(EPortPinType.Output, EPortNameDisplay.Default, EPortValueDisplay.NotEditable, "Z")] public FloatPort OutC;

        public override void OnStartReadingNode()
        {
            InValA.TriggerReadPort(true);

            var inVal = InValA.GetInputValue;
            OutA.Value = inVal.x;
            OutB.Value = inVal.y;
            OutC.Value = inVal.z;

            //if ( inVal.GetType() == typeof(Vector3) )
            //{
            //    Vector3 toSplit = (Vector3)inVal;
            //    OutA.Value = toSplit.x;
            //    OutB.Value = toSplit.y;
            //    OutC.Value = toSplit.z;
            //}
            //else if (inVal.GetType() == typeof(Vector3Int))
            //{
            //    Vector3Int toSplit = (Vector3Int)inVal;
            //    OutA.Value = toSplit.x;
            //    OutB.Value = toSplit.y;
            //    OutC.Value = toSplit.z;
            //}
            //else if (inVal.GetType() == typeof(Vector2Int))
            //{
            //    Vector2Int toSplit = (Vector2Int)inVal;
            //    OutA.Value = toSplit.x;
            //    OutB.Value = toSplit.y;
            //    OutC.Value = 0;
            //}
            //else if (inVal.GetType() == typeof(Vector2))
            //{
            //    Vector2 toSplit = (Vector2)inVal;
            //    OutA.Value = toSplit.x;
            //    OutB.Value = toSplit.y;
            //    OutC.Value = 0;
            //}
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