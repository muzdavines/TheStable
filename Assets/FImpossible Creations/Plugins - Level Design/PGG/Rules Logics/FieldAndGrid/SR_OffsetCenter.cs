using UnityEngine;

namespace FIMSpace.Generating.Rules.FieldAndGrid
{
    public class SR_OffsetCenter : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Correct to Center"; }
        public override string Tooltip() { return "Depending on cell's count something center of grid square is not even and position should be offsetted to fit perfectly with grid center"; }

        public EProcedureType Type { get { return EProcedureType.Event; } }
        public FieldModification OnlyOn;
        public bool Override = false;
        public Vector3 MultiplyAxis = Vector3.one;

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (OnlyOn != null) if (spawn.OwnerMod != OnlyOn) return;

            if (Override)
            {
                spawn.Offset = (Vector3.Scale(grid.GetCenterOffset(preset.GetCellUnitSize()), MultiplyAxis) );
            }
            else
            {
                spawn.Offset += (Vector3.Scale(grid.GetCenterOffset(preset.GetCellUnitSize()), MultiplyAxis));
            }
        }
    }
}