using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planner.Nodes
{
    [System.Serializable]
    public abstract class PGGPlanner_ExecutionNode : PlannerRuleBase
    {
        public override Color GetNodeColor()
        {
            return new Color(0.70f, 0.75f, 0.75f, 0.85f);
        }

        public override Vector2 NodeSize { get { return new Vector2(180, 58); } }
    }
}