#if UNITY_EDITOR
using UnityEditor;
using FIMSpace.FEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

namespace FIMSpace.Generating.Rules.Modelling
{
    public class SR_ReplaceFilterMesh : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Replace MeshFilter Mesh"; }
        public override string Tooltip() { return "If target spawn has attached MeshFilter, this node will replace it's mesh with provided one or few for random choose."; }
        public EProcedureType Type { get { return EProcedureType.Event; } }

        public List<Mesh> RandomMeshes = new List<Mesh>();

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (RandomMeshes.Count == 0) return;

            Mesh randMesh = RandomMeshes[FGenerators.GetRandom(0, RandomMeshes.Count)];

            spawn.OnGeneratedEvents.Add(o =>
            {
                MeshFilter filt = o.GetComponent<MeshFilter>();
                if ( filt) filt.sharedMesh = randMesh;
            });
        }

    }
}