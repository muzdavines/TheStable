using UnityEngine;

namespace FIMSpace.Generating.Rules.Placement
{
    //public class SR_AlignTo : SpawnRuleBase, ISpawnProcedureType
    //{
    //    public override string TitleName() { return "Align To"; }
    //    public override string Tooltip() { return ""; }

    //    public EProcedureType Type => EProcedureType.Procedure;

    //    [Space(4)]
    //    public string GetCoordsFrom = "";
    //    public string AlignTo = "";
    //    public Vector3 CheckOn1 = Vector3.left;
    //    public Vector3 CheckOn2 = Vector3.right;
    //    public bool Direct = true;


    //    private Vector3? targetPos = null;

    //    public override void ResetRule(FGenGraph<FieldCell, FGenVertex> grid, FieldSetup preset)
    //    {
    //        base.ResetRule(grid, preset);
    //        targetPos = null;
    //    }

    //    public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenVertex> grid, Vector3? restrictDirection = null)
    //    {
    //        if (string.IsNullOrEmpty(AlignTo)) return;

    //        SpawnData tagSpawn = cell.CollectSpawns().Count > 0 ? cell.CollectSpawns()[0] : spawn;

    //        SpawnData td = GetSpawnDataWithTag(cell, GetCoordsFrom);
    //        if ((td is null) == false) tagSpawn = td;

    //        FieldCell c1, c2;
    //        Quaternion checkRot = Quaternion.Euler(tagSpawn.TempRotationOffset != Vector3.zero ? tagSpawn.TempRotationOffset : tagSpawn.RotationOffset);
    //        Vector3 checkDir = (checkRot * CheckOn1);
    //        c1 = grid.GetCell(cell.Pos.x + Mathf.RoundToInt(checkDir.x), cell.Pos.y, cell.Pos.z + Mathf.RoundToInt(checkDir.z), false);
    //        checkDir = (checkRot * CheckOn2);
    //        c2 = grid.GetCell(cell.Pos.x + Mathf.RoundToInt(checkDir.x), cell.Pos.y, cell.Pos.z + Mathf.RoundToInt(checkDir.z), false);

    //        SpawnData alignTo = GetSpawnDataWithTag(c1, AlignTo);
    //        SpawnData alignTo2 = GetSpawnDataWithTag(c2, AlignTo);
    //        if (alignTo is null) alignTo = alignTo2;
    //    }

    //    public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenVertex> grid)
    //    {
    //        if (targetPos != null)
    //        {
    //            spawn.Offset = targetPos.Value;
    //            spawn.DirectionalOffset = Vector3.zero;
    //        }
    //    }


    //}
}