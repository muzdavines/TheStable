
using UnityEngine;

namespace FIMSpace.Generating.Rules.FieldAndGrid
{
    public class SR_ShiftTowards : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Shift Towards"; }
        public override string Tooltip() { return ""; }

        public EProcedureType Type { get { return EProcedureType.Event; } }
        //public string GetRotationFromTagged = "";

        [Range(0f,1f)] public float ToCenter = 0f;
        [Range(0f,.3f)] public float AddRandom = 0f;
        [Range(0f, 1f)] public float FitInY = 0f;

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            Vector3 centerPos = grid.GetWorldCenter(preset.GetCellUnitSize(), true);
            Vector3 cellPos = preset.GetCellWorldPosition(cell);
            Vector3 dir = centerPos - cellPos;
            dir.y = Mathf.Lerp(spawn.Offset.y, dir.y, FitInY);
            
            spawn.Offset = Vector3.Lerp(spawn.Offset, dir, ToCenter + FGenerators.GetRandom(0f, AddRandom)); 
        }
    }
}