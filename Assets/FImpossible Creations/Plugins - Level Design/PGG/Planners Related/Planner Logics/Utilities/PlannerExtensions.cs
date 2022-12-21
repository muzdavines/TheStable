using FIMSpace.Generating.Planner.Nodes;
using FIMSpace.Generating.Planning.PlannerNodes;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Planning
{
    public static class PlannerExtensions
    {
        /// <summary>
        /// To PlannerRuleBase (RuleBase)
        /// </summary>
        public static PlannerRuleBase ToRB(this PGGPlanner_NodeBase node)
        {
            return node as PlannerRuleBase;
        }
    }
}
