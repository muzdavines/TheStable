using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Generating
{

    public class PR_GetScaleConvertedShape : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Convert To New Scale" : "Convert Field To New Scale"; }
        public override string GetNodeTooltipDescription { get { return "Generating cells set scaled to the size of current field planner."; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.CellsManipulation; } }
        public override Color GetNodeColor() { return new Color(0.45f, 0.9f, 0.15f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(198, 138); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideOnConnected, 1)] public PGGPlannerPort Source;
        [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGPlannerPort Converted;
        [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGVector3Port RootOffset;
        [Tooltip("If you're converting this planner then toggling this will do important calculations without need to use additional nodes. It will align field with target field -> convert cell -> apply root offset.")]
        public bool ConvertingSelf = false;

        private bool sameScaleDetected = false;
        public override void OnStartReadingNode()
        {
            Source.TriggerReadPort(true);
            sameScaleDetected = false;

            var portChecker = Source.GetInputCheckerSafe;
            var portPlan = GetPlannerFromPort(Source, false);
            if (portPlan != null) portChecker = portPlan.LatestChecker;

            if (portChecker == null) return;
            if (portChecker.ChildPositionsCount == 0) return;

            var myPlan = CurrentExecutingPlanner;
            if (myPlan == null) return;
            if (myPlan.LatestChecker == portChecker) return;

            if ( portPlan.LatestChecker.RootScale == myPlan.LatestChecker.RootScale)
            {
                sameScaleDetected = true;
                CheckerField3D oChecker = portPlan.LatestChecker.Copy();
                Converted.ProvideShape(oChecker);
                return;
            }

            CheckerField3D convertedChecker = portChecker.GenerateCheckerConvertedToNewScale(myPlan.LatestChecker.RootScale, ConvertingSelf ? myPlan.LatestChecker : null);
            RootOffset.Value = portChecker.GetScaleConversionRootOffset(myPlan.LatestChecker.RootScale);
            Converted.ProvideShape(convertedChecker);
        }

#if UNITY_EDITOR
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if ( sameScaleDetected)
            {
                UnityEditor.EditorGUILayout.HelpBox("Same Scale Fields!", UnityEditor.MessageType.None);
            }
        }
#endif

    }
}