#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FIMSpace.Generating.Rules.Modelling
{
    public class SR_ReplaceWithRandomPrefab : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Replace Spawn with Random Prefab"; }
        public override string Tooltip() { return "Replace spawned prefab with other one, can be used for more complex modificators and using this rule for manage prefabs in custom way"; }
        public EProcedureType Type { get { return EProcedureType.Event; } }

        public List<GameObject> RandomList = new List<GameObject>();

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (RandomList.Count == 0) return;

            GameObject targetObj = RandomList[FGenerators.GetRandom(0, RandomList.Count)];
            if (targetObj == null) return;

            Action<SpawnData> replaceSpawn =
            (o) =>
            {
                o.Prefab = targetObj;
            };

            spawn.OnPreGeneratedEvents.Add(replaceSpawn);
        }

    }
}