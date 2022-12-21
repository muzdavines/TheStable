#if UNITY_EDITOR
using UnityEditor;
using FIMSpace.FEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

namespace FIMSpace.Generating.Rules.Modelling
{
    public class SR_RandomMesh : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Spawn Random Mesh Renderer"; }
        public override string Tooltip() { return "Generating object with Mesh Renderer with mesh choosed from provided list. (thanks to this node you don't need to create prefab with single mesh renderer!)"; }
        public EProcedureType Type { get { return EProcedureType.Event; } }

        private GameObject preparedScheme = null;

        [FPD_Layers] public int TargetLayer = 0;
        public bool Static = false;
        public Material DefaultMaterial;

        [Space(5)]
        public List<Mesh> RandomMeshes = new List<Mesh>();

        public override void PreGenerateResetRule(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset, FieldSpawner callFrom)
        {
            if (RandomMeshes.Count == 0) return;

            if (callFrom.TemporaryPrefabOverride != null)
            {
                return;
            }

            if (preparedScheme) { FGenerators.DestroyObject(preparedScheme); }

            preparedScheme = new GameObject(OwnerSpawner.Name + "-RandomMesh");
            preparedScheme.layer = TargetLayer;
            preparedScheme.isStatic = Static;
            preparedScheme.AddComponent<MeshFilter>();
            preparedScheme.AddComponent<MeshRenderer>().sharedMaterial = DefaultMaterial;
            preparedScheme.transform.position = new Vector3(10000, -10000, 10000);
            preparedScheme.hideFlags = HideFlags.HideAndDontSave;

            callFrom.SetTemporaryPrefabToSpawn(preparedScheme);
        }

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (RandomMeshes.Count == 0) return;

            Mesh randMesh = RandomMeshes[FGenerators.GetRandom(0, RandomMeshes.Count)];

            spawn.OnGeneratedEvents.Add(o =>
            {
                o.GetComponent<MeshFilter>().sharedMesh = randMesh;
            });
        }

    }
}