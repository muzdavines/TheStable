using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using FIMSpace.Generating.Rules;

namespace FIMSpace.Generating.Planning.ModNodes.Operations
{

    public class MR_ApplyPrefabToSpawn : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? ("Apply Prefab") : "Apply Prefab To Spawn Data"; }
        public override string GetNodeTooltipDescription { get { return "Setting different prefab to be spawned by the spawn data"; } }
        public override Color GetNodeColor() { return new Color(0.7f, 0.55f, 0.25f, 0.9f); }
        public override bool IsFoldable { get { return false; } }
        public override Vector2 NodeSize { get { return new Vector2(184, _EditorFoldout ? 144 : 104); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }

        [Port(EPortPinType.Input, 1)] public PGGSpawnPort TargetSpawn;
        [Port(EPortPinType.Input, 1)] public PGGUniversalPort Prefab;


        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            TargetSpawn.TriggerReadPort(true);
            var spawn = TargetSpawn.GetFirstConnectedSpawn;

            if (TargetSpawn.IsConnected == false) spawn = MG_Spawn;
            if (FGenerators.IsNull(spawn)) {  return; }

            GameObject prefab = null;
            Prefab.TriggerReadPort(true);

            object val = Prefab.GetPortValueSafe;
            if (val == null) {  return; }
            prefab = val as GameObject;

            if (prefab == null) { return; }

            spawn.Prefab = prefab;
            spawn.TryDetectMeshInPrefab();
        }
    }
}