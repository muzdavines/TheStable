using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.BuildSetup
{

    public class PR_CollectFields : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Collect All Fields"; }
        public override string GetNodeTooltipDescription { get { return "Get all active fields from build, port will output multiple fields inside"; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(200, 121); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        [Port(EPortPinType.Input, EPortNameDisplay.Default, 1)] public PGGStringPort OnlyTagged;
        [Port(EPortPinType.Input, EPortNameDisplay.Default, 1)] public BoolPort GetDuplicates;
        //[Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideOnConnected, 1)] public PGGPlannerPort JoinWith;
        [Port(EPortPinType.Output, true)] public PGGPlannerPort MultiplePlanners;

        public override void OnStartReadingNode()
        {
            FieldPlanner cplan = FieldPlanner.CurrentGraphExecutingPlanner;
            if (cplan == null) return;

            BuildPlannerPreset build = cplan.ParentBuildPlanner;
            if (build == null) return;

            List<FieldPlanner> planners = new List<FieldPlanner>();
            string tag = OnlyTagged.GetInputValue;
            bool checkTags = !string.IsNullOrEmpty(tag);

            for (int p = 0; p < build.BasePlanners.Count; p++)
            {
                var pl = build.BasePlanners[p];

                if (pl.DisableWholePlanner) continue;
                if (checkTags) if (pl.tag != tag) continue;

                if (!pl.Discarded)
                {
                    planners.Add(pl);
                }

                var duplList = pl.GetDuplicatesPlannersList();
                if (duplList != null)
                    for (int d = 0; d < duplList.Count; d++)
                    {
                        if (duplList[d].Discarded) continue;
                        planners.Add(duplList[d]);
                    }
            }

            MultiplePlanners.AssignPlannersList(planners);
        }

    }
}