using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.BuildSetup
{

    public class PR_GetBuildAreaBounds : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Build area Bounds" : "Get Build area Bounds"; }
        public override string GetNodeTooltipDescription { get { return "Getting bounds built out of all currently placed field planners, Use 'GetFieldBounds' node to read bounds data."; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(180, 80); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        [Tooltip("Use 'GetFieldBounds' node to read bounds data")]
        [Port(EPortPinType.Output)] public PGGUniversalPort FullBounds;

        public override void OnStartReadingNode()
        {
            FieldPlanner cplan = FieldPlanner.CurrentGraphExecutingPlanner;
            if (cplan == null) return;

            BuildPlannerPreset build = cplan.ParentBuildPlanner;
            if (build == null) return;

            var planners = build.CollectAllAvailablePlanners(true, true);
            if (planners.Count == 0) return;

            Bounds b = planners[0].LatestChecker.GetFullBoundsWorldSpace();

            for (int i = 1; i < planners.Count; i++)
            {
                b.Encapsulate(planners[i].LatestChecker.GetFullBoundsWorldSpace());
            }

            FullBounds.Variable.SetTemporaryReference(true, b);
        }

    }
}