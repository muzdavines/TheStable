using System.Collections.Generic;
using UnityEngine;
using FIMSpace.Generating.Rules;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace FIMSpace.Generating.NotUsed
{
    //public class SR_StampTile_PlaneCheckAround : SR_StampTile_PlaneCheck, ISpawnProcedureType
    //{
    //    public override string TitleName() { return "Stamp Plane Fit Around"; }
    //    public ESR_Space CheckFromNrightbours = ESR_Space.Occupied;
    //    public string TargetNeightboursTags = "";

    //    protected List<StamperTileTag.TileFitInfo> parentTiles = new List<StamperTileTag.TileFitInfo>();

    //    protected override bool CheckPrefabTile(FieldCell cell, FGenGraph<FieldCell, FGenVertex> grid, GameObject prefab, ref StamperTileTag.TileFitInfo mostCorrectTile)
    //    {
    //        var toCheck = SpawnRules.GetTargetNeightboursAround(cell, grid, CheckFromNrightbours, TargetNeightboursTags);

    //        bool was = false;
    //        for (int i = 0; i < toCheck.Count; i++)
    //        {
    //            var nCell = toCheck[i];
    //            bool done = base.CheckPrefabTile(nCell, grid, prefab, ref mostCorrectTile);
    //            if (was == false) if (done) was = true;
    //        }

    //        return was;
    //    }
    //}
}