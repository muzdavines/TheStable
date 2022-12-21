using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.BuildSetup.GeneratingStage
{

    public class PR_GetIterationIndex : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Iteration Index" : "Get Current Iteration Index"; }
        public override string GetNodeTooltipDescription { get { return "Get iteration index in the current step of generating"; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(196, 122); } }

        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public enum EIterationType
        {
            [Tooltip("Global planner iteration step including 'Planner Iteration' and all 'Instance Iteration'")]
            GlobalStepIteration,
            [Tooltip("Index of the current Field Planner in Build Planner list")]
            PlannerIteration,
            [Tooltip("Iteration of current Field Planner instances")]
            InstanceIteration
        }

        public EIterationType Type = EIterationType.GlobalStepIteration;
        [Tooltip("Index + 1 so not starting from zero but from one")] public bool PlusOne = false;
        [Port(EPortPinType.Output, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "Index")] public IntPort Iteration;

        public override void OnStartReadingNode()
        {
            FieldPlanner planner = CurrentExecutingPlanner;
            if (planner == null) planner = ParentPlanner;
            if (planner == null) return; // Can be read on the grid view before preparing pre run procedures

            int iter = 0;

            switch (Type)
            {
                case EIterationType.GlobalStepIteration:
                    iter = ParentPlanner.ParentBuildPlanner.GenerationIteration;  break;
                case EIterationType.PlannerIteration:
                    iter = planner.IndexOnPreset; break;
                case EIterationType.InstanceIteration:
                    iter = planner.InstanceIndex; break;
            }

            if (PlusOne) iter += 1;

            Iteration.Value = iter;
        }


    }
}