using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Access
{

    public class PR_GetGlobalIndexOfInstance : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Get Index Of Instance" : "Get Index of Field Planner Instance"; }
        public override string GetNodeTooltipDescription { get { return "Getting field planner instance index."; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(210, 127); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }



        public enum EIndexType
        {
            GlobalIndex, PlannerInstanceIndex
        }

        public EIndexType IndexType = EIndexType.GlobalIndex;
        [Port(EPortPinType.Input)] public PGGPlannerPort Planner;
        [Port(EPortPinType.Output, EPortValueDisplay.NotEditable)] public IntPort Index;


        public override void OnStartReadingNode()
        {
            Planner.TriggerReadPort(true);
            var planner = GetPlannerFromPort(Planner, false);
            if (planner == null) return;

            if ( IndexType == EIndexType.PlannerInstanceIndex)
            {
                if (planner.IsDuplicate == false) Index.Value = 0;
                else Index.Value = planner.IndexOfDuplicate + 1;
            }
            else if (IndexType == EIndexType.GlobalIndex)
            {
                if (planner.ParentBuildPlanner == null) return;
                Index.Value = planner.ParentBuildPlanner.CountIndexOfPlannerInstance(planner);
            }

        }


    }
}