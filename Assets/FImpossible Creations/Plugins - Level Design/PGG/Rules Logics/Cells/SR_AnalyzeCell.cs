using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Cells
{
    public class SR_AnalyzeCell : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Analyze Cell"; }
        public override string Tooltip() { return "Checking state of choosed cell to allow or disallow spawn"; }


        [PGG_SingleLineTwoProperties("Dir", 0, 30, 10, -150)]
        [Tooltip("Leave zero if you want check just target cell")]
        public Vector3Int CellOffset = Vector3Int.zero;
        [Tooltip("Offset cell with spawn rotation (it's local space instead of world space cell offset)")]
        [HideInInspector] public bool Dir = false;

        public enum ESR_Space { Empty, Occupied, OutOfGrid, Any, CenterCell, OccupiedWithNode, InGrid }
        public ESR_Space CheckedCellMustBe = ESR_Space.Empty;

        [HideInInspector] public FieldModification occupiedBy;
        [HideInInspector] public GameObject occupiedByPrefab;

        [PGG_SingleLineSwitch("CheckMode", 68, "Select if you want to use Tags, SpawnStigma or CellData", 140)]
        [HideInInspector] public string occupiedByTagged;
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        [HideInInspector] public float allowFromCenter = 0f;
        [HideInInspector] public List<FieldModification> occupiedByMore;
        [HideInInspector] public NeightbourPlacement placement;
        [HideInInspector] public CheckCellsSelectorSetup checkSetup = new CheckCellsSelectorSetup(true, false);

        public EProcedureType Type { get { return EProcedureType.Coded; } }



        #region Editor stuff
#if UNITY_EDITOR
        public override void NodeHeader()
        {
            base.NodeHeader();
            checkSetup.UseRotor = true;
            checkSetup.UseCondition = false;
            DrawMultiCellSelector(checkSetup, OwnerSpawner);
            EditorGUIUtility.labelWidth = 140;
        }

        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            if (CheckedCellMustBe == ESR_Space.Occupied)
            {
                GUILayout.Space(3);
                EditorGUILayout.PropertyField(so.FindProperty("occupiedBy"));
                EditorGUILayout.PropertyField(so.FindProperty("occupiedByPrefab"));
                EditorGUILayout.PropertyField(so.FindProperty("occupiedByTagged"));
            }

            if (CheckedCellMustBe == ESR_Space.CenterCell)
            {
                GUILayout.Space(3);
                EditorGUILayout.PropertyField(so.FindProperty("allowFromCenter"));
            }

            EditorGUIUtility.labelWidth = 0;
        }
#endif
        #endregion


        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (Enabled == false || Ignore) return;

            if (CellOffset != Vector3Int.zero)
            {
                if (Dir)
                {
                    Vector3 rot = spawn.GetFullRotationOffset();
                    rot = (Quaternion.Euler(rot) * (Vector3)CellOffset);
                    cell = grid.GetCell(cell.Pos + new Vector3Int(Mathf.RoundToInt(rot.x), Mathf.RoundToInt(rot.y), Mathf.RoundToInt(rot.z)));
                }
                else
                {
                    cell = grid.GetCell(cell.Pos + CellOffset);
                }
            }

            CellAllow = CellSelector_CheckCondition(checkSetup, grid, cell, spawn, preset, (FieldCell fc, SpawnData sp, FGenGraph<FieldCell, FGenPoint> grd, FieldSetup fld) => { return CheckAllow(fc, sp, grd, fld); });

            #region Backup

            //CellAllow = false;

            //if (cell == null)
            //{
            //    if (CheckedCellMustBe == ESR_Space.OutOfGrid) SetPlacementStats();
            //    return;
            //}

            //if (CheckedCellMustBe == ESR_Space.Any)
            //{
            //    SetPlacementStats();
            //    return;
            //}

            //if (CheckedCellMustBe == ESR_Space.Occupied)
            //{
            //    var spawns = cell.CollectSpawns(OwnerSpawner.ScaleAccess);

            //    if (occupiedBy == null && occupiedByPrefab == null && string.IsNullOrEmpty(occupiedByTagged))
            //    {
            //        if (spawns.Count > 0) SetPlacementStats();
            //    }
            //    else
            //    {
            //        for (int s = 0; s < spawns.Count; s++)
            //        {
            //            if (spawns[s].OwnerMod == null) continue;
            //            if (spawns[s] == spawn) continue;

            //            if (string.IsNullOrEmpty(occupiedByTagged) == false)
            //            {
            //                if (SpawnHaveSpecifics(spawns[s], occupiedByTagged, CheckMode))
            //                {
            //                    SetPlacementStats();
            //                    break;
            //                }
            //            }
            //            else
            //            {
            //                SpawnData tgtSpwn = GetConditionalSpawnData(cell, occupiedByTagged, occupiedByPrefab, occupiedBy);
            //                if (tgtSpwn != null)
            //                {
            //                    SetPlacementStats();
            //                    break;
            //                }
            //            }
            //        }
            //    }


            //    #region Backup
            //    //if (occupiedBy != null) if (CellSpawnsHaveModificator(cell, occupiedBy))
            //    //        SetPlacementStats();
            //    //if (CellAllow == false)
            //    //    if (occupiedByPrefab != null) if (CellSpawnsHavePrefab(cell, occupiedByPrefab))
            //    //            SetPlacementStats();
            //    //if (CellAllow == false)
            //    //    if (!string.IsNullOrEmpty(occupiedByTagged))
            //    //        if (CellSpawnsHaveTag(cell, occupiedByTagged))
            //    //SetPlacementStats();
            //    #endregion


            //    return;
            //}
            //else if (CheckedCellMustBe == ESR_Space.Empty)
            //{
            //    if (cell.CollectSpawns(OwnerSpawner.ScaleAccess).Count < 1) SetPlacementStats();
            //    return;
            //}
            //else if (CheckedCellMustBe == ESR_Space.OutOfGrid)
            //{
            //    if (cell.InTargetGridArea == false) SetPlacementStats();
            //    return;
            //}
            //else if (CheckedCellMustBe == ESR_Space.CenterCell)
            //{
            //    //Debug.DrawRay((Vector3)grid.GetCenter() * preset.CellSize, Vector3.up * 10f, Color.blue, 1.1f);
            //    float dits = Vector3.Distance((Vector3)cell.Pos * preset.CellSize, (Vector3)grid.GetCenter() * preset.CellSize);
            //    if (dits <= allowFromCenter) SetPlacementStats();
            //    return;
            //}
            //else if (CheckedCellMustBe == ESR_Space.OccupiedWithNode)
            //{
            //    if (cell.ParentCell.NotNull()) SetPlacementStats();
            //    return;
            //}
            #endregion

        }

        public bool CheckAllow(FieldCell cell, SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, FieldSetup field)
        {

            if (cell == null)
            {
                if (CheckedCellMustBe == ESR_Space.OutOfGrid) SetPlacementStats();
                return true;
            }

            if (CheckedCellMustBe == ESR_Space.Any || CheckedCellMustBe == ESR_Space.InGrid)
            {
                if (CheckedCellMustBe == ESR_Space.Any)
                {
                    SetPlacementStats();
                    return true;
                }
                else if ( cell.InTargetGridArea == true)
                {
                    SetPlacementStats();
                    return true;
                }
            }

            if (CheckedCellMustBe == ESR_Space.Occupied)
            {
                var spawns = cell.CollectSpawns(OwnerSpawner.ScaleAccess);

                if (occupiedBy == null && occupiedByPrefab == null && string.IsNullOrEmpty(occupiedByTagged))
                {
                    if (spawns.Count > 0) SetPlacementStats();
                }
                else
                {
                    for (int s = 0; s < spawns.Count; s++)
                    {
                        if (spawns[s].OwnerMod == null) continue;
                        if (spawns[s] == spawn) continue;

                        if (string.IsNullOrEmpty(occupiedByTagged) == false)
                        {
                            if (SpawnHaveSpecifics(spawns[s], occupiedByTagged, CheckMode))
                            {
                                SetPlacementStats();
                                break;
                            }
                        }
                        else
                        {
                            SpawnData tgtSpwn = GetConditionalSpawnData(cell, occupiedByTagged, occupiedByPrefab, occupiedBy);
                            if (tgtSpwn != null)
                            {
                                SetPlacementStats();
                                break;
                            }
                        }
                    }
                }

                return CellAllow;
            }
            else if (CheckedCellMustBe == ESR_Space.Empty)
            {
                if (cell.CollectSpawns(OwnerSpawner.ScaleAccess).Count < 1)
                {
                    SetPlacementStats();
                    return true;
                }
            }

            else if (CheckedCellMustBe == ESR_Space.OutOfGrid)
            {
                if (cell.InTargetGridArea == false)
                {
                    SetPlacementStats();
                    return true;
                }
            }
            else if (CheckedCellMustBe == ESR_Space.CenterCell)
            {
                //Debug.DrawRay((Vector3)grid.GetCenter() * preset.CellSize, Vector3.up * 10f, Color.blue, 1.1f);
                float dits = Vector3.Distance((Vector3)cell.Pos * field.CellSize, (Vector3)grid.GetCenter() * field.CellSize);
                if (dits <= allowFromCenter)
                {
                    SetPlacementStats();
                    return true;
                }
            }
            else if (CheckedCellMustBe == ESR_Space.OccupiedWithNode)
            {
                if (cell.ParentCell.NotNull())
                {
                    SetPlacementStats();
                    return true;
                }
            }

            return false;
        }

        public void SetPlacementStats()
        {
            CellAllow = true;
        }

    }
}