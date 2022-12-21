using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace FIMSpace.Generating.Rules.QuickSolutions
{
    public class SR_WallPlacer : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Wall Placer"; }
        public override string Tooltip() { return "Spawning wall tiles with pre-defined rules and aligning rotation"; }
        public EProcedureType Type { get { return EProcedureType.Coded; } }
        public enum EWallModule
        {
            [Tooltip("WallBase - Use this module mode to place basic walls around the grid area")]
            WallBase,
            [Tooltip("Curve90 - Use this module mode to place modules of curved walls replacing two corner walls")]
            Curve90,
            [Tooltip("Curve45 - Use this module mode to place modules of walls creating diagonal connection")]
            Curve45,
            [Tooltip("CornerFill - Use this module mode to place patch model in wall corners (not removing basic wall modules)")]
            CornerFill
        }
        public enum ECornerMode45 { Lined, In, Out }
        public enum ECornerMode { In, Out }


        public EWallModule Module = EWallModule.WallBase;
        public ECornerMode CornerMode = ECornerMode.In;
        public ECornerMode45 CornerMode45 = ECornerMode45.Lined;

        private ESR_Space SpawnOn = ESR_Space.OutOfGrid;
        private string OccupiedTags = "";

        [PGG_SingleLineTwoProperties("UseYLevel", 0, 80, 10, -70)]
        [Space(5)] public bool SpawnOnEachSide = false;
        [Tooltip("(when setted to -1 then disabled) If you making like two/three or more floor levels building, you can get initial shape for walls out of Y=0 grid level cells")]
        [HideInInspector] public int UseYLevel = -1;
        [PGG_SingleLineTwoProperties("SetGhosts", 0, 90)]
        [Space(5)] public bool AutoRemoveOverlaps = true;
        [HideInInspector]
        [Tooltip("Not removing spawns completely, just not generating model on scene, but other rules will see 'ghosted' spawns, which can be helpful for spawning props on walls with 'Get Coordinates' node")]
        public bool SetGhosts = false;
        [Tooltip("Other way algorithm is removing overlaps")] public bool Version2 = false;
        [Range(0, 2)] public int Padding = 0;
        public float YawOffset = 0f;

        [PGG_SingleLineSwitch("OffsetMode", 58, "Select if you want to offset postion with cell size or world units", 140)]
        public Vector3 DirectOffset = Vector3.zero;
        [HideInInspector] public ESR_Measuring OffsetMode = ESR_Measuring.Units;

        [HideInInspector][Tooltip("Ignoring placement of wall placer done with different Field Modificators")] public bool IgnoreOtherMods = true;

        [Tooltip("You can try toggling it if some modules are not spawned in desired places for unknown reasons")]
        public bool DontCheckAdditionals = false;

        private static readonly string BASE = "WBASE";
        private static readonly string SEGM = "WSEGM";
        private static readonly string SEGMR = "WREM";

#if UNITY_EDITOR
        public override void NodeHeader()
        {
            base.NodeHeader();

            if (GUIIgnore.Count != 5)
            {
                GUIIgnore.Clear(); GUIIgnore.Add(""); GUIIgnore.Add(""); GUIIgnore.Add(""); GUIIgnore.Add(""); GUIIgnore.Add(""); GUIIgnore.Add("");
            }

            if (Module == EWallModule.WallBase || Module == EWallModule.CornerFill)
            {
                GUIIgnore[0] = "AutoRemoveOverlaps";
                GUIIgnore[1] = "CornerMode45";

                if (Module != EWallModule.CornerFill)
                {
                    GUIIgnore[2] = "CornerMode";
                    GUIIgnore[3] = "Padding";
                    GUIIgnore[4] = "DontCheckAdditionals";
                }
                else
                {
                    GUIIgnore[2] = "";
                    GUIIgnore[3] = "";
                    GUIIgnore[4] = "";

                }

                GUIIgnore[5] = "Version2";
            }
            else // Curve 90 or Curve 45
            {
                /*if (Module != EWallModule.Curve90) */ GUIIgnore[0] = "SpawnOnEachSide";
                //else GUIIgnore[0] = "";

                GUIIgnore[5] = "Version2";

                if (Module != EWallModule.Curve45)
                {
                    GUIIgnore[1] = "CornerMode45";
                    GUIIgnore[2] = "Padding";
                    if (Module == EWallModule.Curve90) if (CornerMode == ECornerMode.In) GUIIgnore[5] = "";
                }
                else
                {
                    GUIIgnore[1] = "CornerMode";
                    GUIIgnore[2] = "";
                }

                if (Module != EWallModule.Curve90) GUIIgnore[3] = "DontCheckAdditionals";
                else GUIIgnore[3] = "";

                GUIIgnore[4] = "";
            }
        }
#endif

        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);

            _presetForDebug = preset;
            Vector3 refGridPos = cell.WorldPos(preset);
            //if ( spawn.PreviewMesh ) { refGridPos += Quaternion.Euler(0, -YawOffset, 0) * spawn.PreviewMesh.bounds.center; }

            extraToCheckRemoveFrom.Clear();

            FieldCell sourceCel = cell;
            if (UseYLevel != -1)
            {
                cell = grid.GetCell(new Vector3Int(cell.Pos.x, UseYLevel, cell.Pos.z));
            }

            if (Module == EWallModule.WallBase)
            {
                var cells = SpawnRules.GetTargetNeightboursPLUS(cell, grid, SpawnOn, OccupiedTags, ESR_Details.Tag);

                for (int i = 0; i < cells.Count; i++)
                {
                    var c = cells[i];
                    Vector3 toCell = cell.Pos - c.Pos;
                    toCell.Normalize();

                    if (SpawnOnEachSide == false)
                    {
                        CopySpawnToTempData(ref spawn, toCell, preset);
                        break;
                    }
                    else
                    {
                        CopySpawnToTempData(ref spawn, toCell, preset);
                    }
                }

            }
            else if (Module == EWallModule.Curve90)
            {
                if (CornerMode == ECornerMode.Out)
                {
                    if (GetCustomStigmaOutOfCell(cell, SEGM, mod)) return;

                    for (int r = 0; r < 4; r++)
                    {
                        var cell1 = SpawnRules.GetAngledNeightbour(cell, grid, SpawnRules.Get90Offset(r));
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell1, OccupiedTags, ESR_Details.Tag) == false) { continue; }

                        int ro = r + 1; if (ro > 3) ro = 0;
                        var cell2 = SpawnRules.GetAngledNeightbour(cell1, grid, SpawnRules.Get90Offset(ro));
                        var cell2c = SpawnRules.GetAngledNeightbour(cell, grid, SpawnRules.Get90Offset(ro));
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell2, OccupiedTags, ESR_Details.Tag)) { continue; }

                        Vector3 toCell = cell.Pos - cell1.Pos;

                        if (ShouldContinue(cell1)) { Debug.DrawRay(Vector3.Lerp(cell.WorldPos(preset), cell1.WorldPos(preset), 0.4f), Vector3.up * 4f, Color.red, 1.01f); Debug.Log("ignorit"); continue; }
                        if (ShouldContinue(cell2))
                        {
                            //Debug.DrawRay(Vector3.Lerp(cell.WorldPos(preset), cell2.WorldPos(preset), 0.4f), Vector3.up * 4f, Color.yellow, 1.01f);
                            //Debug.DrawLine(Vector3.Lerp(cell.WorldPos(preset), cell2.WorldPos(preset), 0.4f), cell.WorldPos(preset), Color.yellow, 1.01f);
                            //Debug.Log("ignorit2");
                            if (!DontCheckAdditionals) continue;
                        }


                        // If 45 wall is here dont spawn curve
                        bool ignoreIt = false;
                        var nn = grid.GetPLUSSquare(cell1, false);
                        for (int nc = 0; nc < nn.Length; nc++)
                        {
                            var cl = nn[nc];
                            if (FGenerators.CheckIfExist_NOTNULL(cl))
                                if (GetCustomStigmaOutOfCell(cl, SEGM, mod)) { ignoreIt = true; break; }
                        }

                        if (ignoreIt)
                        {
                            //Debug.DrawLine(cell.WorldPos(preset), Vector3.up * 100f, Color.magenta, 1.01f);
                            if (!DontCheckAdditionals) continue;
                        }
                        else
                        {
                            //Debug.DrawLine(cell.WorldPos(preset) + (cell1.Pos - cell.Pos).V3IntToV3() * 0.75f, Vector3.up * 100f, Color.green, 1.01f);
                        }


                        if (AutoRemoveOverlaps)
                        {

                            #region Commented but can be helpful later

                            //UnityEngine.Debug.DrawLine(cell.WorldPos(preset), cell.WorldPos(preset) - toCell * 2f, Color.green, 1.01f);
                            //UnityEngine.Debug.DrawLine(cell.WorldPos(preset), cell.WorldPos(preset) + Vector3.up, Color.green, 1.01f);

                            //UnityEngine.Debug.DrawLine(cell2.WorldPos(preset) + Vector3.up * 0.1f, cell2.WorldPos(preset) + (cell.Pos - cell2.Pos).V3IntToV3() * 2f + Vector3.up * 0.1f, Color.yellow, 1.01f);
                            //UnityEngine.Debug.DrawLine(cell2.WorldPos(preset) + Vector3.up * 0.1f, cell2.WorldPos(preset) + Vector3.up, Color.yellow, 1.01f);

                            //UnityEngine.Debug.DrawLine(cell1.WorldPos(preset) + Vector3.up * 0.2f, cell1.WorldPos(preset) + (cell.Pos - cell1.Pos).V3IntToV3() * 2f + Vector3.up * 0.2f, Color.magenta, 1.01f);
                            //UnityEngine.Debug.DrawLine(cell1.WorldPos(preset) + Vector3.up * 0.2f, cell1.WorldPos(preset) + Vector3.up, Color.magenta, 1.01f);

                            #endregion

                            Vector3 measurePos = GetMeasurePosOffset(refGridPos, toCell, preset);

                            Quaternion rr = Quaternion.LookRotation(cell.Pos - cell1.Pos);
                            //var toRemove = SpawnRules.GetSpawnRotated(cell, rr);
                            var toRemove = SpawnRules.GetNearest(cell, measurePos, preset);

                            if (FGenerators.CheckIfExist_NOTNULL(toRemove))
                                if (toRemove.OwnerMod == spawn.OwnerMod)
                                {
                                    if (toRemove.GetCustomStigma(Module.ToString()) == false)
                                    {
                                        //UnityEngine.Debug.DrawRay(toRemove.GetWorldPositionWithFullOffset(preset) + Vector3.up, Vector3.up, Color.red, 1.01f);
                                        //UnityEngine.Debug.DrawRay(cell.WorldPos(preset), Vector3.up, Color.yellow, 1.01f);
                                        //UnityEngine.Debug.DrawLine(cell.WorldPos(preset), measurePos, Color.white, 1.01f);
                                        //UnityEngine.Debug.DrawRay(measurePos, Vector3.up, Color.white, 1.01f);
                                        ScheduleToRemove(toRemove);
                                    }
                                    //cell.RemoveSpawnFromCell(toRemove);
                                }

                            rr = Quaternion.LookRotation(cell1.Pos - cell2.Pos);
                            toRemove = SpawnRules.GetNearest(cell2, measurePos, preset);
                            //toRemove = SpawnRules.GetSpawnRotated(cell2, rr);

                            if (FGenerators.CheckIfExist_NOTNULL(toRemove))
                            {

                                if (toRemove.OwnerMod == spawn.OwnerMod)
                                {
                                    if (toRemove.GetCustomStigma(Module.ToString()) == false)
                                    {
                                        ScheduleToRemove(toRemove);
                                        AddCellToCheckRemoveFrom(cell2);
                                    }
                                    //cell2.RemoveSpawnFromCell(toRemove);
                                }
                            }
                        }

                        toCell.Normalize();
                        CopySpawnToTempData(ref spawn, toCell, preset);
                        if (!SpawnOnEachSide) break;
                    }
                }
                else
                {

                    for (int r = 0; r < 4; r++)
                    {
                        var cell1 = SpawnRules.GetAngledNeightbour(cell, grid, SpawnRules.Get90Offset(r));
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell1, OccupiedTags, ESR_Details.Tag) == false) continue;

                        int ro = r + 1; if (ro > 3) ro = 0;
                        var cell2 = SpawnRules.GetAngledNeightbour(cell, grid, SpawnRules.Get90Offset(ro));
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell2, OccupiedTags, ESR_Details.Tag) == false) continue;

                        Vector3 toCell = cell.Pos - cell1.Pos;

                        if (ShouldContinue(cell1)) continue;
                        if (ShouldContinue(cell2)) continue;


                        // If 45 wall is here dont spawn curve
                        bool ignoreIt = false;
                        var nn = grid.GetPLUSSquare(cell1, false);
                        for (int nc = 0; nc < nn.Length; nc++)
                        {
                            var cl = nn[nc];
                            if (FGenerators.CheckIfExist_NOTNULL(cl))
                                if (GetCustomStigmaOutOfCell(cl, SEGM, mod)) { ignoreIt = true; break; }
                        }

                        if (ignoreIt) continue;


                        if (AutoRemoveOverlaps)
                        {
                            if (Version2 == false)
                            {
                                Quaternion rr = Quaternion.LookRotation(toCell);
                                var toRemove = SpawnRules.GetSpawnWithExactMod(cell, mod);
                                //var toRemove = SpawnRules.GetSpawnRotated(cell, rr);
                                if (FGenerators.CheckIfExist_NOTNULL(toRemove))
                                //if (toRemove.OwnerMod == spawn.OwnerMod)
                                {
                                    //if (toRemove.GetCustomStigma(Module.ToString()) == false)
                                    ScheduleToRemove(toRemove);
                                    //cell.RemoveSpawnFromCell(toRemove);
                                }

                                rr = Quaternion.LookRotation(cell.Pos - cell2.Pos);
                                toRemove = SpawnRules.GetSpawnWithExactMod(cell, mod); //SpawnRules.GetSpawnRotated(cell, rr);
                                if (FGenerators.CheckIfExist_NOTNULL(toRemove))
                                //if (toRemove.OwnerMod == spawn.OwnerMod)
                                {
                                    //if (toRemove.GetCustomStigma(Module.ToString()) == false)
                                    //cell.RemoveSpawnFromCell(toRemove);
                                    ScheduleToRemove(toRemove);
                                }
                            }
                            else
                            {
                                string stigma = Module.ToString();
                                Quaternion rr = Quaternion.LookRotation(cell.Pos - cell1.Pos);
                                var toRemove = SpawnRules.GetSpawnRotated(cell, rr, spawn.OwnerMod, stigma, 5);

                                if (FGenerators.CheckIfExist_NOTNULL(toRemove))
                                {
                                    ScheduleToRemove(toRemove);
                                    //cell.RemoveSpawnFromCell(toRemove);
                                }

                                rr = Quaternion.LookRotation(cell.Pos - cell2.Pos);
                                toRemove = SpawnRules.GetSpawnRotated(cell, rr, spawn.OwnerMod, stigma, 5);
                                if (FGenerators.CheckIfExist_NOTNULL(toRemove))
                                {
                                    ScheduleToRemove(toRemove);
                                    AddCellToCheckRemoveFrom(cell2);
                                    //cell.RemoveSpawnFromCell(toRemove);
                                }
                            }
                        }

                        toCell.Normalize();
                        CopySpawnToTempData(ref spawn, toCell, preset);
                        if (!SpawnOnEachSide) break;
                    }
                }
            }
            else if (Module == EWallModule.Curve45)
            {

                if (CornerMode45 == ECornerMode45.In)
                {
                    for (int r = 0; r < 4; r++)
                    {
                        // Checking if target Forward and up cell is out of grid as first condition
                        var cell1 = SpawnRules.GetAngledNeightbour90(cell, grid, r);
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell1, OccupiedTags, ESR_Details.Tag) == false) continue; // not out of grid then ignore this rotation iteration

                        var cell2 = SpawnRules.GetAngledNeightbour90(cell, grid, r + 1);
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell2, OccupiedTags, ESR_Details.Tag) == false) continue; // not out of grid then ignore this rotation iteration


                        #region Commented but can be helpful later
                        //if (cell.FlatPosInt == new Vector2Int(0, 7))
                        //{
                        //    if (r == 3)
                        //    {
                        //        Debug.DrawRay(cell.WorldPos(preset.CellSize), Vector3.up * 3f, Color.white, 0.1f);

                        //        Debug.DrawLine(cell1.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.yellow, 0.1f);
                        //        Debug.DrawRay(cell1.WorldPos(preset.CellSize), Vector3.up, Color.yellow, 0.1f);

                        //        Debug.DrawLine(cell2.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.cyan, 0.1f);
                        //        Debug.DrawRay(cell2.WorldPos(preset.CellSize), Vector3.up, Color.cyan, 0.1f);

                        //    }
                        //}
                        #endregion


                        Vector3 toCell = cell.Pos - cell1.Pos;

                        // Check if further diagonal cells are empty --------------
                        var cell3 = SpawnRules.GetAngledNeightbour90(cell1, grid, r - 1);
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell3, OccupiedTags, ESR_Details.Tag) == false) continue;

                        var cell4 = SpawnRules.GetAngledNeightbour90(cell2, grid, r - 2);
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell4, OccupiedTags, ESR_Details.Tag) == false) continue;

                        #region Commented but can be helpful later
                        //if (cell.FlatPosInt == new Vector2Int(0, 7))
                        //{
                        //    if (r == 3)
                        //    {
                        //        Debug.DrawRay(cell.WorldPos(preset.CellSize), Vector3.up * 3f, Color.white, 0.1f);

                        //        Debug.DrawLine(cell3.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.yellow, 0.1f);
                        //        Debug.DrawRay(cell3.WorldPos(preset.CellSize), Vector3.up, Color.yellow, 0.1f);

                        //        Debug.DrawLine(cell4.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.green, 0.1f);
                        //        Debug.DrawRay(cell4.WorldPos(preset.CellSize), Vector3.up, Color.green, 0.1f);

                        //    }
                        //}
                        #endregion

                        // Check if connection required cells are not empty
                        var cell5 = SpawnRules.GetAngledNeightbour90(cell, grid, r - 1);
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell5, OccupiedTags, ESR_Details.Tag)) continue;

                        var cell6 = SpawnRules.GetAngledNeightbour90(cell, grid, r - 2);
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell6, OccupiedTags, ESR_Details.Tag)) continue;

                        #region Commented but can be helpful later
                        //if (cell.FlatPosInt == new Vector2Int(0, 7))
                        //{
                        //    if (r == 3)
                        //    {
                        //        Debug.DrawRay(cell.WorldPos(preset.CellSize), Vector3.up * 3f, Color.white, 0.1f);

                        //        Debug.DrawLine(cell5.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.yellow, 0.1f);
                        //        Debug.DrawRay(cell5.WorldPos(preset.CellSize), Vector3.up, Color.yellow, 0.1f);

                        //        Debug.DrawLine(cell6.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.green, 0.1f);
                        //        Debug.DrawRay(cell6.WorldPos(preset.CellSize), Vector3.up, Color.green, 0.1f);

                        //    }
                        //}
                        #endregion


                        if (Padding > 0)
                        {
                            if (Padding > 1)
                            {
                                // Checking more further cells if are out of grid
                                var cell7 = SpawnRules.GetAngledNeightbour90(cell1, grid, r - 1);
                                var cell8 = SpawnRules.GetAngledNeightbour90(cell2, grid, r + 2);

                                var cell9 = SpawnRules.GetAngledNeightbour90(cell5, grid, r - 1);
                                var cell10 = SpawnRules.GetAngledNeightbour90(cell6, grid, r + 2);

                                var cell9f = SpawnRules.GetAngledNeightbour90(cell9, grid, r - 1);
                                var cell10f = SpawnRules.GetAngledNeightbour90(cell10, grid, r + 2);

                                #region Commented but can be helpful later
                                //if (cell.FlatPosInt == new Vector2Int(0, 7))
                                //{
                                //    if (r == 3)
                                //    {
                                //        Debug.DrawRay(cell.WorldPos(preset.CellSize), Vector3.up * 3f, Color.white, 0.1f);

                                //        Debug.DrawLine(cell9.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.yellow, 0.1f);
                                //        Debug.DrawRay(cell9.WorldPos(preset.CellSize), Vector3.up, Color.yellow, 0.1f);

                                //        Debug.DrawLine(cell10.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.green, 0.1f);
                                //        Debug.DrawRay(cell10.WorldPos(preset.CellSize), Vector3.up, Color.green, 0.1f);

                                //    }
                                //}
                                #endregion

                                if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell7, OccupiedTags, ESR_Details.Tag) == false) continue;
                                if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell8, OccupiedTags, ESR_Details.Tag) == false) continue;
                                if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell9, OccupiedTags, ESR_Details.Tag)) continue;
                                if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell10, OccupiedTags, ESR_Details.Tag)) continue;
                                if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell9f, OccupiedTags, ESR_Details.Tag)) continue;
                                if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell10f, OccupiedTags, ESR_Details.Tag)) continue;
                            }

                            bool ignoreIt = false;
                            var nn = grid.GetPLUSSquare(cell, false);
                            for (int nc = 0; nc < nn.Length; nc++)
                            {
                                var cl = nn[nc];
                                if (FGenerators.CheckIfExist_NOTNULL(cl))
                                    if (GetCustomStigmaOutOfCell(cl, SEGM, mod)) { ignoreIt = true; break; }
                            }

                            if (ignoreIt) continue;
                        }


                        if (AutoRemoveOverlaps)
                        {
                            var toRemove = SpawnRules.GetSpawnRotated(cell, Quaternion.LookRotation(cell.Pos - cell1.Pos));
                            //var toRemove = SpawnRules.GetSpawnWithExactMod(cell, mod);
                            //var fCell = SpawnRules.GetAngledNeightbour90(cell1, grid, r);

                            if (FGenerators.CheckIfExist_NOTNULL(toRemove))
                            //if (toRemove.OwnerMod == spawn.OwnerMod)
                            {
                                ScheduleToRemove(toRemove);
                                //cell.RemoveSpawnFromCell(toRemove);
                            }


                            toRemove = SpawnRules.GetSpawnRotated(cell, Quaternion.LookRotation(cell.Pos - cell2.Pos));
                            //toRemove = SpawnRules.GetSpawnWithExactMod(cell, mod);
                            //var fCell = SpawnRules.GetAngledNeightbour90(cell1, grid, r);

                            if (FGenerators.CheckIfExist_NOTNULL(toRemove))
                                //if (toRemove.OwnerMod == spawn.OwnerMod)
                                ScheduleToRemove(toRemove);
                            //cell.RemoveSpawnFromCell(toRemove);

                            //RemoveSEGMStigmedOutOfCell(cell, spawn);

                            #region Commented but can be helpful later
                            //Debug.DrawRay(cell.WorldPos(), Vector3.up * 3f, Color.white, 0.1f);
                            //Debug.DrawLine(cell.WorldPos(), cell2.WorldPos(), Color.white, 0.1f);
                            #endregion

                        }

                        toCell.Normalize();

                        CopySpawnToTempData(ref spawn, toCell, preset);
                        if ( !SpawnOnEachSide) break;
                    }
                }
                else if (CornerMode45 == ECornerMode45.Out)
                {

                    for (int r = 0; r < 4; r++)
                    {
                        // Cell1 is up/down/left/right cell which should be OUT of grid area -------------------
                        var cell1 = SpawnRules.GetAngledNeightbour90(cell, grid, r);

                        // Cell2 is diagonal cell which should be IN grid area
                        var cell2 = SpawnRules.GetAngledNeightbour90(cell1, grid, r + 1);

                        #region Commented but can be helpful later
                        //if (cell.FlatPosInt == new Vector2Int(4, 2))
                        //{
                        //    if (r == 3)
                        //    {
                        //        Debug.DrawRay(cell.WorldPos(preset.CellSize), Vector3.up * 3f, Color.white, 0.1f);
                        //        Debug.DrawLine(cell1.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.yellow, 0.1f);

                        //        Debug.DrawRay(cell1.WorldPos(preset.CellSize), Vector3.up, Color.yellow, 0.1f);
                        //        Debug.DrawLine(cell2.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.red, 0.1f);
                        //        Debug.DrawRay(cell2.WorldPos(preset.CellSize), Vector3.up, Color.red, 0.1f);
                        //    }
                        //}
                        #endregion

                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell1, OccupiedTags, ESR_Details.Tag) == false) continue; // If inside grid check other rotation
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell2, OccupiedTags, ESR_Details.Tag)) continue; // If out of grid then check next rotation


                        // Target cell is known -----------------------------
                        Vector3 toCell = cell.Pos - cell1.Pos;


                        // Check if further cells are in grid area for walls connection --------------
                        var cell3 = SpawnRules.GetAngledNeightbour90(cell, grid, r - 1);
                        var cell4 = SpawnRules.GetAngledNeightbour90(cell, grid, r - 3);
                        var cell5 = SpawnRules.GetAngledNeightbour90(cell2, grid, r); // cell 2 off

                        #region Commented but can be helpful later
                        //if (cell.FlatPosInt == new Vector2Int(4,2))
                        //{
                        //    if (r == 3)
                        //    {
                        //        Vector3 off = Vector3.up * 0.5f;
                        //        Debug.DrawLine(cell3.WorldPos(preset.CellSize) + off, cell.WorldPos(preset.CellSize) + off, Color.cyan, 0.1f);
                        //        Debug.DrawRay(cell3.WorldPos(preset.CellSize) + off, Vector3.up, Color.cyan, 0.1f);
                        //        Debug.DrawLine(cell4.WorldPos(preset.CellSize) + off, cell.WorldPos(preset.CellSize) + off, Color.magenta, 0.1f);
                        //        Debug.DrawRay(cell4.WorldPos(preset.CellSize) + off, Vector3.up, Color.magenta, 0.1f);
                        //        Debug.DrawLine(cell5.WorldPos(preset.CellSize) + off, cell2.WorldPos(preset.CellSize) + off, Color.black, 0.1f);
                        //        Debug.DrawRay(cell5.WorldPos(preset.CellSize) + off, Vector3.up, Color.black, 0.1f);
                        //    }
                        //}
                        #endregion


                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell3, OccupiedTags, ESR_Details.Tag)) continue; // If not in grid then ignore
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell4, OccupiedTags, ESR_Details.Tag)) continue; // If not in grid then ignore
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell5, OccupiedTags, ESR_Details.Tag)) continue; // If not in grid then ignore
                                                                                                                          //if (GetCustomStigmaOutOfCell(cell4, SEGM)) continue;

                        // Check if further direction cells are out of grid ----------------------
                        var cell6 = SpawnRules.GetAngledNeightbour90(cell1, grid, r);
                        var cell7 = SpawnRules.GetAngledNeightbour90(cell1, grid, r - 1);

                        #region Commented but can be helpful later
                        //if (cell.FlatPosInt == new Vector2Int(4, 2))
                        //{
                        //    if (r == 3)
                        //    {
                        //        Debug.DrawRay(cell6.WorldPos(preset.CellSize), Vector3.up * 3f, Color.white, 0.1f);
                        //        Debug.DrawLine(cell6.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.yellow, 0.1f);

                        //        Debug.DrawRay(cell7.WorldPos(preset.CellSize), Vector3.up, Color.yellow, 0.1f);
                        //        Debug.DrawLine(cell7.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.red, 0.1f);
                        //    }
                        //}
                        #endregion

                        if (Padding > 0)
                        {
                            if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell6, OccupiedTags, ESR_Details.Tag) == false) continue; // If in grid then ignore
                            if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell7, OccupiedTags, ESR_Details.Tag) == false) continue; // If in grid then ignore
                        }
                        else
                        {
                            bool any = false;
                            if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell6, OccupiedTags, ESR_Details.Tag)) any = true; // If in grid then ignore
                            if (!any) if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell7, OccupiedTags, ESR_Details.Tag)) any = true; // If in grid then ignore
                            if (!any) continue;

                            if (cell1.HaveCustomData(SEGM)) continue;

                            bool ignoreIt = false;
                            var nn = grid.GetPLUSSquare(cell, false);
                            for (int nc = 0; nc < nn.Length; nc++)
                            {
                                var cl = nn[nc];
                                if (FGenerators.CheckIfExist_NOTNULL(cl))
                                    if (GetCustomStigmaOutOfCell(cl, SEGM, mod)) { ignoreIt = true; break; }
                            }
                            if (ignoreIt) continue;
                        }

                        if (Padding > 0)
                        {
                            // Checking if further cells are out of grid too
                            var cell8 = SpawnRules.GetAngledNeightbour90(cell6, grid, r);
                            var cell9 = SpawnRules.GetAngledNeightbour90(cell7, grid, r - 1);
                            if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell8, OccupiedTags, ESR_Details.Tag) == false) continue; // If  in grid then ignore
                            if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell9, OccupiedTags, ESR_Details.Tag) == false) continue; // If  in grid then ignore

                            if (Padding > 1)
                            {
                                // On sides must be in grid
                                //var cell8s = SpawnRules.GetAngledNeightbour90(cell8, grid, r + 1);
                                //var cell9s = SpawnRules.GetAngledNeightbour90(cell9, grid, r - 2);
                                //if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell8s, OccupiedTags)) continue; // If out of grid then ignore
                                //if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell9s, OccupiedTags)) continue; // If out of grid then ignore

                                // Checking if further cells are out of grid too
                                var cell10 = SpawnRules.GetAngledNeightbour90(cell8, grid, r);
                                var cell11 = SpawnRules.GetAngledNeightbour90(cell9, grid, r - 1);

                                #region Commented but can be helpful later
                                //if (cell.FlatPosInt == new Vector2Int(0, 1))
                                //{
                                //    if (r == 3)
                                //    {
                                //        Debug.DrawRay(cell8.WorldPos(preset.CellSize), Vector3.up * 3f, Color.white, 0.1f);
                                //        Debug.DrawLine(cell8.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.white, 0.1f);

                                //        Debug.DrawRay(cell9.WorldPos(preset.CellSize), Vector3.up, Color.cyan, 0.1f);
                                //        Debug.DrawLine(cell9.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.cyan, 0.1f);

                                //        Debug.DrawRay(cell10.WorldPos(preset.CellSize), Vector3.up * 3f, Color.yellow, 0.1f);
                                //        Debug.DrawLine(cell10.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.yellow, 0.1f);

                                //        Debug.DrawRay(cell11.WorldPos(preset.CellSize), Vector3.up, Color.red, 0.1f);
                                //        Debug.DrawLine(cell11.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.red, 0.1f);
                                //    }
                                //}
                                #endregion

                                if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell10, OccupiedTags, ESR_Details.Tag) == false) continue; // If in grid then ignore
                                if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell11, OccupiedTags, ESR_Details.Tag) == false) continue; // If in grid then ignore
                            }
                        }

                        if (AutoRemoveOverlaps)
                        {
                            var toRemove = SpawnRules.GetSpawnRotated(cell, Quaternion.LookRotation(cell.Pos - cell1.Pos));

                            // If furthercell is out of grid so no hole remains
                            //var fCell = SpawnRules.GetAngledNeightbour90(cell1, grid, r);
                            //if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, fCell, OccupiedTags))
                            {
                                if (FGenerators.CheckIfExist_NOTNULL(toRemove))
                                    if (toRemove.OwnerMod == spawn.OwnerMod)
                                    {
                                        //cell.RemoveSpawnFromCell(toRemove);
                                        ScheduleToRemove(toRemove);
                                    }
                            }

                            #region Commented but can be helpful later
                            //Debug.DrawRay(cell.WorldPos(), Vector3.up * 3f, Color.white, 0.1f);
                            //Debug.DrawLine(cell.WorldPos(), cell2.WorldPos(), Color.white, 0.1f);
                            #endregion


                            //fCell = SpawnRules.GetAngledNeightbour90(cell1, grid, r - 1);
                            //if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, fCell, OccupiedTags))
                            {
                                toRemove = SpawnRules.GetSpawnRotated(cell2, Quaternion.LookRotation(cell2.Pos - cell1.Pos));
                                if (FGenerators.CheckIfExist_NOTNULL(toRemove))
                                    if (toRemove.GetCustomStigma(BASE))
                                    {
                                        ScheduleToRemove(toRemove);
                                        AddCellToCheckRemoveFrom(cell2);
                                        //cell2.RemoveSpawnFromCell(toRemove);
                                    }
                            }

                            #region Commented but can be helpful later
                            //Debug.DrawRay(cell.WorldPos(), Vector3.up * 3f, Color.white, 0.1f);
                            //Debug.DrawLine(cell.WorldPos(), cell2.WorldPos(), Color.white, 0.1f);
                            #endregion

                            #region Commented but can be helpful later
                            //Debug.DrawRay(cell.WorldPos(), Vector3.up * 3f, Color.red, 0.1f);
                            //Debug.DrawRay(cell1.WorldPos(), Vector3.up * 3f, Color.cyan, 0.1f);
                            //Debug.DrawLine(fCell.WorldPos(), cell1.WorldPos(), Color.red, 0.1f);
                            #endregion

                            //RemoveSEGMStigmedOutOfCell(cell, spawn);
                        }

                        toCell.Normalize();
                        CopySpawnToTempData(ref spawn, toCell, preset);
                        cell1.AddCustomData(SEGM);

                        if (!SpawnOnEachSide) break;
                    }

                }
                else if (CornerMode45 == ECornerMode45.Lined)
                {
                    for (int r = 0; r < 4; r++)
                    {
                        var cell1 = SpawnRules.GetAngledNeightbour90(cell, grid, r);
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell1, OccupiedTags, ESR_Details.Tag) == false) continue;

                        var cell2 = SpawnRules.GetAngledNeightbour90(cell, grid, r + 1);
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell2, OccupiedTags, ESR_Details.Tag) == false) continue;

                        // Counter cell1-> if there is space
                        var cell1c = SpawnRules.GetAngledNeightbour90(cell, grid, r + 2);
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell1c, OccupiedTags, ESR_Details.Tag)) continue;

                        if (Padding > 0)
                        {
                            // Further cell must be in grid space
                            var cell2f = SpawnRules.GetAngledNeightbour90(cell2, grid, r + 2);

                            #region Commented but can be helpful later
                            //if (cell.FlatPosInt == new Vector2Int(8, 4))
                            //{
                            //    if (r == 0)
                            //    {
                            //        Debug.DrawRay(cell.WorldPos(preset.CellSize), Vector3.up * 3f, Color.white, 0.1f);
                            //        Debug.DrawLine(cell1.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.yellow, 0.1f);

                            //        Debug.DrawRay(cell1.WorldPos(preset.CellSize), Vector3.up, Color.yellow, 0.1f);
                            //        Debug.DrawLine(cell2.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.red, 0.1f);
                            //        Debug.DrawRay(cell2.WorldPos(preset.CellSize), Vector3.up, Color.red, 0.1f);

                            //        Debug.DrawLine(cell1c.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.cyan, 0.1f);
                            //        Debug.DrawRay(cell1c.WorldPos(preset.CellSize), Vector3.up, Color.cyan, 0.1f);

                            //        Debug.DrawLine(cell2f.WorldPos(preset.CellSize), cell.WorldPos(preset.CellSize), Color.cyan, 0.1f);
                            //        Debug.DrawRay(cell2f.WorldPos(preset.CellSize), Vector3.up, Color.cyan, 0.1f);
                            //    }
                            //}
                            #endregion

                            if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell2f, OccupiedTags, ESR_Details.Tag)) continue;

                            // Checking if neightbour is corner which is not single wall module
                            var cell5 = SpawnRules.GetAngledNeightbour90(cell, grid, r - 1);
                            //Debug.DrawLine(cell.WorldPos(preset.CellSize) + Vector3.up * 0.04f, cell5.WorldPos(preset.CellSize), Color.yellow, 0.1f);
                            if (GetCustomStigmaOutOfCell(cell5, BASE, mod)) continue;
                            if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell5, OccupiedTags, ESR_Details.Tag)) continue;

                            if (Padding > 1)
                            {
                                var cell6 = SpawnRules.GetAngledNeightbour90(cell, grid, r - 2);
                                //Debug.DrawLine(cell.WorldPos(preset.CellSize), cell6.WorldPos(preset.CellSize), Color.red, 0.1f);
                                if (GetCustomStigmaOutOfCell(cell6, BASE, mod)) continue;
                            }
                        }


                        if (AutoRemoveOverlaps)
                        {
                            var toRemove = SpawnRules.GetSpawnRotated(cell, Quaternion.LookRotation(cell.Pos - cell1.Pos));

                            if (FGenerators.CheckIfExist_NOTNULL(toRemove))
                                if (toRemove.OwnerMod == spawn.OwnerMod)
                                    ScheduleToRemove(toRemove);
                            //cell.RemoveSpawnFromCell(toRemove);

                            toRemove = SpawnRules.GetSpawnRotated(cell, Quaternion.LookRotation(cell.Pos - cell2.Pos));
                            if (FGenerators.CheckIfExist_NOTNULL(toRemove))
                                if (toRemove.OwnerMod == spawn.OwnerMod)
                                    ScheduleToRemove(toRemove);

                            toRemove = SpawnRules.GetNearest(cell, refGridPos, _presetForDebug, _presetForDebug.CellSize,
                                (s) =>
                                {
                                    if (s.GetCustomStigma(EWallModule.Curve90.ToString())) return true; else return false;
                                });

                            if (FGenerators.CheckIfExist_NOTNULL(toRemove))
                                if (toRemove.OwnerMod == spawn.OwnerMod)
                                    ScheduleToRemove(toRemove);

                            //cell.RemoveSpawnFromCell(toRemove);
                            //RemoveSEGMStigmedOutOfCell(cell, spawn);
                        }

                        Vector3 toCell = cell.Pos - cell1.Pos;

                        toCell.Normalize();

                        CopySpawnToTempData(ref spawn, toCell, preset);
                        break;
                    }
                }
            }
            else if (Module == EWallModule.CornerFill)
            {

                if (CornerMode == ECornerMode.In) // Corner fill in
                {
                    for (int r = 0; r < 4; r++)
                    {
                        var cell1 = SpawnRules.GetAngledNeightbour(cell, grid, SpawnRules.Get90Offset(r));
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell1, OccupiedTags, ESR_Details.Tag) == false) continue;

                        int ro = r + 1; if (ro > 3) ro = 0;
                        var cell2 = SpawnRules.GetAngledNeightbour(cell, grid, SpawnRules.Get90Offset(ro));
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell2, OccupiedTags, ESR_Details.Tag) == false) continue;

                        Vector3 toCell = cell.Pos - cell1.Pos;
                        if (!DontCheckAdditionals) if (HaveRotatedSpawnLike(cell, toCell)) continue;

                        if (GetCustomStigmaOutOfCell(cell, EWallModule.Curve45.ToString(), mod)) continue;

                        toCell.Normalize();

                        if (SpawnOnEachSide == false)
                        { CopySpawnToTempData(ref spawn, toCell, preset); break; }
                        else
                        {
                            CopySpawnToTempData(ref spawn, toCell, preset);
                        }
                    }
                }
                else // Corner fill out
                {
                    for (int r = 0; r < 4; r++)
                    {
                        var cell1 = SpawnRules.GetAngledNeightbour90(cell, grid, r);
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell1, OccupiedTags, ESR_Details.Tag) == false) continue;

                        var cell2 = SpawnRules.GetAngledNeightbour90(cell1, grid, r + 1);
                        if (SpawnRules.CheckNeightbourCellAllow(SpawnOn, cell2, OccupiedTags, ESR_Details.Tag)) continue;

                        Vector3 toCell = cell.Pos - cell1.Pos;

                        if (!DontCheckAdditionals)
                        {
                            if (HaveRotatedSpawnLike(cell, toCell)) continue;
                            //UnityEngine.Debug.DrawRay(cell.WorldPos(_presetForDebug), Vector3.up * 2f, Color.cyan, 1.01f);
                            if (GetCustomStigmaOutOfCell(cell, EWallModule.Curve90.ToString(), mod))
                            {
                                continue;
                            }
                        }


                        if (Padding < 2)
                        {
                            if (GetCustomStigmaOutOfCell(cell, EWallModule.Curve45.ToString(), mod))
                            {
                                if (Padding == 0) continue;
                                else
                                if (GetCustomStigmaOutOfCell(cell2, EWallModule.Curve45.ToString(), mod)) continue;
                            }
                        }
                        else if (Padding == 2)
                        {
                            var cellC = SpawnRules.GetAngledNeightbour45(cell, grid, r);
                            var cellC2 = SpawnRules.GetAngledNeightbour45(cell, grid, r - 4);

                            var cellC4 = SpawnRules.GetAngledNeightbour45(cell, grid, r + 2);
                            var cellC6 = SpawnRules.GetAngledNeightbour45(cell, grid, r - 2);

                            if (GetCustomStigmaOutOfCell(cellC, EWallModule.Curve45.ToString(), mod) && GetCustomStigmaOutOfCell(cellC2, EWallModule.Curve45.ToString(), mod)) continue;
                            if (GetCustomStigmaOutOfCell(cellC4, EWallModule.Curve45.ToString(), mod) && GetCustomStigmaOutOfCell(cellC6, EWallModule.Curve45.ToString(), mod)) continue;
                        }

                        toCell.Normalize();

                        if (SpawnOnEachSide == false)
                        {
                            CopySpawnToTempData(ref spawn, toCell, preset); break;
                        }
                        else
                        {
                            CopySpawnToTempData(ref spawn, toCell, preset);
                        }
                    }
                }

            } // End corner fill

            CellAllow = false;

            if (tempSpawns != null)
            {
                if (tempSpawns.Count > 0) CellAllow = true;
            }

            if (CellAllow)
            {
                spawn.TempRotationOffset += new Vector3(0, YawOffset, 0);
                spawn.TempPositionOffset = Quaternion.Euler(spawn.TempRotationOffset) * GetUnitOffset(DirectOffset, OffsetMode, preset);
            }
        }

        FieldSetup _presetForDebug = null;
        void ScheduleToRemove(SpawnData spawn)
        {
            spawn.AddCustomStigma(SEGMR);
            //UnityEngine.Debug.DrawRay(spawn.GetWorldPositionWithFullOffset(_presetForDebug) + Vector3.up * 1.5f, Vector3.up, Color.magenta, 1.01f);
        }


        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            var preSpawn = spawn;
            spawn.Enabled = false;

            if (tempSpawns != null) if (tempSpawns.Count > 0)
                {
                    spawn = tempSpawns[0];
                    spawn.LocalScaleMul = preSpawn.LocalScaleMul;
                    spawn.Enabled = true;

                    for (int i = 1; i < tempSpawns.Count; i++)
                    {
                        tempSpawns[i].LocalScaleMul = preSpawn.LocalScaleMul;
                        cell.AddSpawnToCell(tempSpawns[i]);
                    }
                }

            RemoveSEGMStigmedOutOfCell(cell, mod, preset);

            for (int e = 0; e < extraToCheckRemoveFrom.Count; e++)
                RemoveSEGMStigmedOutOfCell(extraToCheckRemoveFrom[e], mod, preset);
        }

        private static List<FieldCell> extraToCheckRemoveFrom = new List<FieldCell>();
        void AddCellToCheckRemoveFrom(FieldCell cell)
        {
            if (!extraToCheckRemoveFrom.Contains(cell)) extraToCheckRemoveFrom.Add(cell);
        }

        /// <summary> Removing stigmed segments out of same cell </summary>
        private void RemoveSEGMStigmedOutOfCell(FieldCell cell, FieldModification modRequired, FieldSetup forDebug)
        {
            // Removing stigmed segments out of same cell
            var stigmed = GetSpawnsWithStigmaOutOfCell(cell, SEGMR);
            // UnityEngine.Debug.Log("segmr = " + SEGMR + "  spawns=" + cell.GetJustCellSpawnCount() + "  stigmed="+stigmed.Count);

            for (int st = 0; st < stigmed.Count; st++)
            {
                if (FGenerators.CheckIfExist_NOTNULL(stigmed[st]))
                    if (stigmed[st].OwnerMod == modRequired)
                    {
                        if (stigmed[st].DontSpawnMainPrefab) continue;

                        //if (stigmed.Count > 1) UnityEngine.Debug.DrawRay(stigmed[st].GetWorldPositionWithFullOffset(forDebug) + Vector3.up, Vector3.up, Color.red, 1.01f);

                        if (!SetGhosts)
                            cell.RemoveSpawnFromCell(stigmed[st]);
                        else
                            stigmed[st].DontSpawnMainPrefab = true;
                    }
            }
        }

        protected void CopySpawnToTempData(ref SpawnData source, Vector3 normal, FieldSetup preset)
        {
            var tgtSpawn = source.Copy();
            AssignSpawnCoords(tgtSpawn, normal, preset);
            AddTempData(tgtSpawn, source);
            if (tempSpawns.Count == 1) AssignSpawnCoords(source, normal, preset);
        }

        bool ShouldContinue(FieldCell cell)
        {
            var spawns = cell.CollectSpawns();
            for (int i = 0; i < spawns.Count; i++)
            { if (spawns[i].DontSpawnMainPrefab) continue; if (CheckSEGMStigma(spawns[i])) return true; }
            //for (int i = 0; i < spawns.Count; i++) if (spawns[i].GetCustomStigma(SEGM)) return true;
            return false;
        }

        bool CheckSEGMStigma(SpawnData spawn)
        {
            if (IgnoreOtherMods)
            {
                // Different mode then not checking
                if (spawn.OwnerMod != OwnerSpawner.Parent) return false;
            }

            if (spawn.GetCustomStigma(SEGM)) return true;
            return false;
        }


        bool HaveRotatedSpawnLike(FieldCell cell, Vector3 dir)
        {
            var spawns = cell.CollectSpawns();
            for (int i = 0; i < spawns.Count; i++)
            { if (spawns[i].DontSpawnMainPrefab) continue; if (spawns[i].OwnerMod == OwnerSpawner.Parent) if (CheckSEGMStigma(spawns[i])) if (SpawnRules.IsSpawnRotated(spawns[i], Quaternion.LookRotation(dir))) return true; }
            return false;
        }

        public void AssignSpawnCoords(SpawnData spawn, Vector3 normal, FieldSetup preset)
        {
            spawn.RotationOffset = Quaternion.LookRotation(normal).eulerAngles + Vector3.up * YawOffset;
            spawn.DirectionalOffset = GetUnitOffset(DirectOffset, OffsetMode, preset);
            spawn.AddCustomStigma(Module.ToString());
            if (Module != EWallModule.WallBase) spawn.AddCustomStigma(SEGM);
            else spawn.AddCustomStigma(BASE);
        }

        Vector3 GetMeasurePosOffset(Vector3 gridCellPos, Vector3 normal, FieldSetup preset)
        {
            Vector3 offR = Quaternion.LookRotation(normal).eulerAngles + Vector3.up * YawOffset;
            Vector3 dirOff = GetUnitOffset(DirectOffset, OffsetMode, preset);
            return gridCellPos + Quaternion.Euler(offR) * dirOff;
        }

    }
}