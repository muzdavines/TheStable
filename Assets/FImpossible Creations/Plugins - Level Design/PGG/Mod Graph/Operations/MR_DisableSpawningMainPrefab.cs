using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using FIMSpace.Generating.Planner.Nodes;

namespace FIMSpace.Generating.Planning.ModNodes.Operations
{

    public class MR_DisableSpawningMainPrefab : PGGPlanner_ExecutionNode
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Disable Main Spawn" : "Disable Spawning Main Prefab"; }
        public override string GetNodeTooltipDescription { get { return "Disabling spawning the main prefab of the spawner, can be handy when spawning multiple objects with node graph."; } }
        public override Color GetNodeColor() { return new Color(0.55f, 0.65f, 0.3f, 0.9f); }
        public override bool IsFoldable { get { return false; } }
        public override Vector2 NodeSize { get { return new Vector2(wdth, 54); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return false; } }

        float wdth = 188;

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }


        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            if (MG_Spawn == null) return;
            MG_Spawn.DontSpawnMainPrefab = true;
        }


    }
}