using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using FIMSpace.Generating.Planner.Nodes;

namespace FIMSpace.Generating.Planning.ModNodes.Operations
{

    public class MR_AllowSpawn : PGGPlanner_ExecutionNode
    {
        public override string GetDisplayName(float maxWidth = 120) { return infoName; }
        public override string GetNodeTooltipDescription { get { return "If using 'Break Spawn' you can restore spawning with this node when triggered."; } }
        public override Color GetNodeColor() { return new Color(0.55f, 0.65f, 0.3f, 0.9f); }
        public override bool IsFoldable { get { return false; } }
        public override Vector2 NodeSize { get { return new Vector2(wdth, 54); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return false; } }

        string infoName = "Restore Allow Spawn";
        float wdth = 188;

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }


        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            if (MG_ModGraph == null) return;
            MG_ModGraph.CellAllow = true;
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
                    infoName = "Restore Allow Spawn";
                    wdth = 188;
                }

            base.Editor_OnNodeBodyGUI(setup);
        }
#endif

    }
}