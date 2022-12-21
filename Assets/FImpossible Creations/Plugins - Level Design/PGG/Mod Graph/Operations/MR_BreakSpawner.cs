using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using FIMSpace.Generating.Planner.Nodes;

namespace FIMSpace.Generating.Planning.ModNodes.Operations
{

    public class MR_BreakSpawner : PGGPlanner_ExecutionNode
    {
        public override string GetDisplayName(float maxWidth = 120) { return infoName; }
        public override string GetNodeTooltipDescription { get { return "Disallow spawning when this node executes"; } }
        public override Color GetNodeColor() { return new Color(0.7f, 0.55f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(wdth, 54); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return false; } }

        string infoName = "Break Spawner";
        float wdth = 188;

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }


        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            if (MG_ModGraph == null) return;
            MG_ModGraph.CellAllow = false;
        }

#if UNITY_EDITOR
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            if (MG_ModGraph)
                if (MG_ModGraph.CallDuring == Rules.QuickSolutions.SR_ModGraph.ECallGraphOn.OnInfluence)
                {
                    infoName = "Not Working OnInfluence!";
                    wdth = 218;
                }
                else
                {
                    infoName = "Break Spawner";
                    wdth = 188;
                }

            base.Editor_OnNodeBodyGUI(setup);
        }
#endif

    }
}