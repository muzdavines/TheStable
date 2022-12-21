using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Values
{

    public class PR_V3Get : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? ("Get " + Component.ToString()) : "Vector 3 : Get X,Y or Z"; }
        public override string GetNodeTooltipDescription { get { return "Small node for quick get Vector3.X,Y or Z value"; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.5f, 0.75f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(138, _EditorFoldout ? 100 : 80); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override bool IsFoldable { get { return true; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }

        public enum EOutType
        {
            X, Y, Z
        }

        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.HideName, EPortValueDisplay.HideValue, "Get", 1, typeof(int))] public PGGVector3Port InVal;
        [HideInInspector] public EOutType Component = EOutType.X;
        [HideInInspector] [Port(EPortPinType.Output, true)] public FloatPort Out;
        [HideInInspector] [Port(EPortPinType.Output, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "Single Val")] public PGGVector3Port V3Val;

        public override void OnStartReadingNode()
        {
            InVal.TriggerReadPort(true);
            Vector3 toSplit = InVal.GetInputValue;
            Out.Value = toSplit.x;

            switch (Component)
            {
                case EOutType.X: V3Val.Value = new Vector3(toSplit.x, 0, 0); break;
                case EOutType.Y: V3Val.Value = new Vector3(0, toSplit.y, 0); Out.Value = toSplit.y; break;
                case EOutType.Z: V3Val.Value = new Vector3(0, 0, toSplit.z); Out.Value = toSplit.z; break;
            }
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (sp == null) sp = baseSerializedObject.FindProperty("InVal");
            SerializedProperty s = sp.Copy();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(s, GUILayout.Width(22));
            s.Next(false);
            EditorGUILayout.PropertyField(s, GUIContent.none, GUILayout.Width(32));
            s.Next(false);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(-19);
            EditorGUILayout.PropertyField(s, GUIContent.none);

            V3Val.AllowDragWire = false;

            if (_EditorFoldout)
            {
                V3Val.AllowDragWire = true;
                s.Next(false);
                EditorGUILayout.PropertyField(s);
            }

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}