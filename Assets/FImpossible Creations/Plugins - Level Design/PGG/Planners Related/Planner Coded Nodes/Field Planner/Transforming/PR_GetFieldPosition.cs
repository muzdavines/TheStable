using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Transforming
{

    public class PR_GetFieldPosition : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Get Field Position" : "Get Field Position"; }
        public override string GetNodeTooltipDescription { get { return "Returning choosed field origin position"; } }
        public override Color GetNodeColor() { return new Color(0.2f, 0.72f, 0.9f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(188, _EditorFoldout ? 102 : 84); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public enum EPositionOut
        {
            Origin, Center
        }

        [HideInInspector] public EPositionOut Output = EPositionOut.Origin;
        [HideInInspector] [Port(EPortPinType.Output, EPortNameDisplay.HideName, EPortValueDisplay.HideValue)] public PGGVector3Port Position;
        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.Default, 1)] public PGGPlannerPort Planner;

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }
        public override void OnStartReadingNode()
        {
            FieldPlanner planner = GetPlannerFromPort(Planner);

            if (planner == null) return;
            if (planner.LatestResult == null) return;
            if (planner.LatestResult.Checker == null) return;

            if (Output == EPositionOut.Center)
            {
                Position.Value = planner.LatestResult.Checker.GetFullBoundsWorldSpace().center;
            }
            else
            {
                Position.Value = planner.LatestResult.Checker.RootPosition;
            }
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            UnityEditor.EditorGUI.BeginChangeCheck();
            Output = (EPositionOut)UnityEditor.EditorGUILayout.EnumPopup(Output, GUILayout.Width(NodeSize.x - 80));

            Planner.AllowDragWire = true;

            GUILayout.Space(-20);

            baseSerializedObject.Update();
            if (sp == null) sp = baseSerializedObject.FindProperty("Position");
            SerializedProperty spc = sp.Copy();
            UnityEditor.EditorGUILayout.PropertyField(spc); spc.Next(false);

            if (_EditorFoldout)
            {
                UnityEditor.EditorGUILayout.PropertyField(spc);
                if (UnityEditor.EditorGUI.EndChangeCheck()) _editorForceChanged = true;
                baseSerializedObject.ApplyModifiedProperties();
            }
            else
            {
                Planner.AllowDragWire = false;
            }
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            UnityEditor.EditorGUILayout.LabelField("Debugging:", UnityEditor.EditorStyles.helpBox);
            GUILayout.Label("Position Value: " + Position.Value);
        }
#endif

    }
}