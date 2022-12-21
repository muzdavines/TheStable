using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Generating
{

    public class PR_GetInlineShape : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Get Inline Shape"; }
        public override string GetNodeTooltipDescription { get { return "Generating grid silhouette cells"; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.CellsManipulation; } }
        public override Color GetNodeColor() { return new Color(0.45f, 0.9f, 0.15f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(188, _EditorFoldout ? 146 : 126); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override bool IsFoldable { get { return true; } }

        [Tooltip("Field to get inline cells group from")]
        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideOnConnected, 1)] public PGGPlannerPort Source;

        [Tooltip("Outputting computed group of cells as Shape")]
        [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGPlannerPort Inline;
        [Tooltip("Including outwards directions inside cells metadata which can be helpful for certain grid analyze methods")]
        public bool AddOutDirections = true;
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

            //checker.DebugLogDrawCellsInWorldSpace(Color.magenta);
            var inlideChecker = checker.GetInlineChecker(false, true, AddOutDirections, false, CopyCellReferences);
            Inline.ProvideShape(inlideChecker);
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