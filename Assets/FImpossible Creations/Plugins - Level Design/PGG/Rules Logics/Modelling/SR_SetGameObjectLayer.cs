using UnityEngine;
using System;

namespace FIMSpace.Generating.Rules.Modelling
{
    public class SR_SetGameObjectLayer : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Set Game Object Layer"; }
        public override string Tooltip() { return "Assigning game object layer to the spawned prefab"; }
        public EProcedureType Type { get { return EProcedureType.Event; } }

        [FPD_Layers] public int TargetLayer;

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            Action<GameObject> setLayer =
            (o) =>
            {
                o.layer = TargetLayer;
            };

            spawn.OnGeneratedEvents.Add(setLayer);
        }

    }
}