using FIMSpace.Generating.Planner.Nodes;
using FIMSpace.Generating.Planning.PlannerNodes;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Planning
{
    public interface IPlanNodesContainer 
    {
        List<PGGPlanner_NodeBase> Procedures { get; }
        List<PGGPlanner_NodeBase> PostProcedures { get; }
        List<FieldVariable> Variables { get; }
        ScriptableObject ScrObj { get; }
        FieldPlanner.LocalVariables GraphLocalVariables { get; }
    }
}
