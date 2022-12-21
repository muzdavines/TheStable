using FIMSpace.Generating.Rules.Placement;
using UnityEngine;

namespace FIMSpace.Generating.Rules.Cells.Legacy
{
    public class SR_CellSpawnsCount : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Cell Spawns Count"; }
        public override string Tooltip() { return "Allowing or not allowing when spawns count in choosed cell is higher lower or equal\n[Lightweight]"; }


        public ESR_DistanceRule AllowWhenCount = ESR_DistanceRule.Lower;
        public int Than = 5;

        public EProcedureType Type { get {return EProcedureType.Rule; } }

        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            int count = cell.CollectSpawns().Count;

            if ( AllowWhenCount == ESR_DistanceRule.Equal)
            {
                if (count == Than) CellAllow = true;
            }
            else if ( AllowWhenCount == ESR_DistanceRule.Greater)
            {
                if (count > Than) CellAllow = true;
            }
            else if (AllowWhenCount == ESR_DistanceRule.Lower)
            {
                if (count < Than) CellAllow = true;
            }
        }
    }
}