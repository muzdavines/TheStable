using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Access
{

    public class PR_GetPlannerDuplicate : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Instance of Planner" : "Get Instance of Planner"; }
        public override string GetNodeTooltipDescription { get { return "Getting instance of some planner if the planner is using instances."; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(220, 142); } }
        public override bool IsFoldable { get { return false; } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        [Port(EPortPinType.Input)] public PGGPlannerPort InstanceOf;
        [Port(EPortPinType.Input, 1)] public IntPort Index;
        [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGPlannerPort Planner;
        [Tooltip("If index = 0 should return the main instance, otherwise index = 0 will return first extra instance.")]
        public bool StartWithMainInstance = true;

        bool DrawInstInd { get { return Planner.PortState() == EPortPinState.Connected; } }
        public override void OnStartReadingNode()
        {
            InstanceOf.TriggerReadPort(true);
            Index.TriggerReadPort(true);

            int ind = Index.GetInputValue;
            FieldPlanner planner = GetPlannerFromPort(InstanceOf);

            if (planner == null) return;
            if (CurrentExecutingPlanner == null) return;
            if (CurrentExecutingPlanner.ParentBuildPlanner == null) return;

            if (planner.Instances == 1)
            {
                Planner.SetIDsOfPlanner(planner);
            }
            else
            {
                if (StartWithMainInstance)
                {
                    if (ind == 0)
                    {
                        Planner.SetIDsOfPlanner(planner);
                        return;
                    }

                    ind -= 1;
                }

                var pln = planner.GetDuplicatesPlannersList();
                if (pln == null) return;
                if (pln.Count == 0) return;

                Planner.SetIDsOfPlanner(pln[Mathf.Min(pln.Count - 1, ind)]);
            }
        }

    }
}