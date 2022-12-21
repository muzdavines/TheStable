using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Generating
{

    public class PR_GetOutlineShape : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Get Outline Shape"; }
        public override string GetNodeTooltipDescription { get { return "Generating grid outline cells"; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.CellsManipulation; } }
        public override Color GetNodeColor() { return new Color(0.45f, 0.9f, 0.15f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(214, _EditorFoldout ? 165 : 145); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override bool IsFoldable { get { return true; } }

        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideOnConnected, 1)] public PGGPlannerPort Source;
        [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGPlannerPort Outline;
        [Port(EPortPinType.Input, 1, 1)] public IntPort OutlineThickness;
        [Space(4)]
        public CheckerField3D.ECheckerMeasureMode Style = CheckerField3D.ECheckerMeasureMode.Rectangular;

        [Tooltip("Experimental.")]
        [HideInInspector] public bool CopyCellReferences = false;

        public override void OnStartReadingNode()
        {
            Source.TriggerReadPort(true);

            var checker = Source.GetInputCheckerSafe;
            var plan = GetPlannerFromPort(Source, false);
            if (plan != null) checker = plan.LatestChecker;

            if (checker == null) return;
            if (checker.ChildPositionsCount == 0) return;

            OutlineThickness.TriggerReadPort(true);
            int thickn = Mathf.Max(OutlineThickness.GetInputValue, 1);

            var outlineChecker = checker.GetOutlineChecker(thickn, Style, false, CopyCellReferences);
            Outline.ProvideShape(outlineChecker);
        }


#if UNITY_EDITOR
        UnityEditor.SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            baseSerializedObject.Update();
            if (_EditorFoldout)
            {
                GUILayout.Space(1);
                if (sp == null) sp = baseSerializedObject.FindProperty("CopyCellReferences");
                UnityEditor.EditorGUILayout.PropertyField(sp, true);
            }
            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}