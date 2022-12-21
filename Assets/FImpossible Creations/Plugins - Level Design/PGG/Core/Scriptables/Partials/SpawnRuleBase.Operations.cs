using FIMSpace.Generating.Rules;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public abstract partial class SpawnRuleBase
    {
        public static SpawnData CellSpawnsHavePrefab(FieldCell cell, GameObject prefab)
        {
            var spawns = cell.CollectSpawns();
            for (int i = 0; i < spawns.Count; i++)
                if (spawns[i].Prefab == prefab) return spawns[i];

            return null;
        }


        public static SpawnData CellSpawnsHaveModificator(FieldCell cell, FieldModification mod)
        {
            var spawns = cell.CollectSpawns();
            for (int i = 0; i < spawns.Count; i++)
                if (spawns[i].OwnerMod == mod) return spawns[i];

            return null;
        }

        public static SpawnData CellSpawnsHaveModPack(FieldCell cell, ModificatorsPack pack)
        {
            var spawns = cell.CollectSpawns();
            for (int i = 0; i < spawns.Count; i++)
                if (spawns[i].OwnerMod != null)
                    if (spawns[i].OwnerMod.ParentPack == pack) return spawns[i];

            return null;
        }

        public static bool GetCustomStigmaOutOfCell(FieldCell cell, string stigma, FieldModification sameMod)
        {
            var spawns = cell.CollectSpawns();

            if (sameMod == null)
            {
                for (int i = 0; i < spawns.Count; i++) if (spawns[i].GetCustomStigma(stigma)) return true;
            }
            else
            {
                for (int i = 0; i < spawns.Count; i++) if (spawns[i].OwnerMod == sameMod) if (spawns[i].GetCustomStigma(stigma)) return true;
            }

            return false;
        }

        public static List<SpawnData> GetSpawnsWithStigmaOutOfCell(FieldCell cell, string stigma)
        {
            List<SpawnData> stigmedSpawns = new List<SpawnData>();

            var cellSpawns = cell.CollectSpawns();

            for (int i = 0; i < cellSpawns.Count; i++)
                if (cellSpawns[i].GetCustomStigma(stigma)) stigmedSpawns.Add(cellSpawns[i]);

            return stigmedSpawns;
        }

        public static SpawnData CellSpawnsHaveTag(FieldCell cell, string occupiedByTagged, SpawnData toIgnore = null, bool random = false)
        {
            //if (string.IsNullOrEmpty(occupiedByTagged)) return false;

            if (random == false)
            {
                var spawns = cell.CollectSpawns();
                for (int i = 0; i < spawns.Count; i++)
                {
                    if (toIgnore != null) if (spawns[i] == toIgnore) continue;

                    if (SpawnRuleBase.SpawnHaveTag(spawns[i], occupiedByTagged))
                        //if (cell.Spawns[i].OwnerMod.ModTag == occupiedByTagged || cell.Spawns[i].tag == occupiedByTagged || cell.Spawns[i].Spawner.SpawnerTag == occupiedByTagged)
                        return spawns[i];
                }
            }
            else
            {
                List<SpawnData> datas = new List<SpawnData>();

                var spawns = cell.CollectSpawns();
                for (int i = 0; i < spawns.Count; i++)
                {
                    if (toIgnore != null) if (spawns[i] == toIgnore) continue;
                    if (SpawnHaveTag(spawns[i], occupiedByTagged)) datas.Add(spawns[i]);
                }

                if (datas.Count > 0)
                {
                    if (datas.Count == 1) return datas[0];
                    else
                        return datas[FGenerators.GetRandom(0, datas.Count)];
                }
            }

            return null;
        }


        public static void FilterConditionsAllowCells(ref FieldCell[] cells, string occupiedBySpec, ESR_Details checkMode, ESR_Space space = ESR_Space.InGrid)
        {
            List<FieldCell> cll = new List<FieldCell>();

            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == null) continue;
                if (!CellConditionsAllows(cells[i], occupiedBySpec, checkMode, space)) cll.Add(cells[i]);
            }

            cells = cll.ToArray();
        }

        public static bool CellConditionsAllows(FieldCell cell, string occupiedBySpec, ESR_Details checkMode, ESR_Space space = ESR_Space.InGrid)
        {
            if (cell.NotNull())
                if (!string.IsNullOrEmpty(occupiedBySpec))
                    if (CellSpawnsHaveSpecifics(cell, occupiedBySpec, checkMode).NotNull())
                        return true;

            if (FGenerators.CheckIfIsNull(cell)) // NULL CELL!
            {
                if (space == ESR_Space.InGrid) return false;
                else if (space == ESR_Space.OutOfGrid) return true; // true
                else return false;
            }
            else // SOME CELL FOUND SO WE CAN CHECK IT
            {
                if (space == ESR_Space.OutOfGrid)
                {
                    if (cell.InTargetGridArea == false) return true; else return false;
                }
                else if (space == ESR_Space.InGrid)
                {
                    if (cell.InTargetGridArea) return true; else return false;
                }
                else
                {
                    if (space == ESR_Space.Empty)
                    {
                        if (cell.GetJustCellSpawnCount() == 0) return true; else return false;
                    }
                    if (space == ESR_Space.Occupied)
                    {
                        if (string.IsNullOrEmpty(occupiedBySpec))
                        {
                            return cell.GetJustCellSpawnCount() > 0;
                        }
                        else
                            return CellSpawnsHaveSpecifics(cell, occupiedBySpec, checkMode).NotNull();
                    }
                }
            }

            return false;
        }


        public static SpawnData CellSpawnsHaveSpecifics(FieldCell cell, string occupiedBySpec, ESR_Details checkMode, SpawnData toIgnore = null, bool random = false)
        {
            if (checkMode == ESR_Details.CellData)
            {
                if (string.IsNullOrEmpty(occupiedBySpec) || CellHaveData(cell, occupiedBySpec)) return new SpawnData() { OwnerCell = cell };
                return null;
            }

            if (random == false)
            {
                var spawns = cell.CollectSpawns();
                for (int i = 0; i < spawns.Count; i++)
                {
                    if (toIgnore != null) if (spawns[i] == toIgnore) continue;

                    if (SpawnRuleBase.SpawnHaveSpecifics(spawns[i], occupiedBySpec, checkMode))
                        //if (cell.Spawns[i].OwnerMod.ModTag == occupiedByTagged || cell.Spawns[i].tag == occupiedByTagged || cell.Spawns[i].Spawner.SpawnerTag == occupiedByTagged)
                        return spawns[i];
                }
            }
            else
            {
                List<SpawnData> datas = new List<SpawnData>();

                var spawns = cell.CollectSpawns();
                for (int i = 0; i < spawns.Count; i++)
                {
                    if (toIgnore != null) if (spawns[i] == toIgnore) continue;
                    if (SpawnHaveSpecifics(spawns[i], occupiedBySpec, checkMode)) datas.Add(spawns[i]);
                }

                if (datas.Count > 0)
                {
                    if (datas.Count == 1) return datas[0];
                    else
                        return datas[FGenerators.GetRandom(0, datas.Count)];
                }
            }

            return null;
        }


        public static Vector3Int GetOffset(Quaternion rot, Vector3 dir)
        {
            Vector3 off = rot * dir;
            return new Vector3Int(Mathf.RoundToInt(off.x), Mathf.RoundToInt(off.y), Mathf.RoundToInt(off.z));
        }

        public static SpawnData GetSpawnDataWithTag(FieldCell cell, string tag)
        {
            var spawns = cell.CollectSpawns();
            for (int i = 0; i < spawns.Count; i++)
                if (SpawnHaveTag(spawns[i], tag))
                    return spawns[i];

            return null;
        }

        public static SpawnData GetSpawnDataWithSpecifics(FieldCell cell, string tag, ESR_Details checkMode)
        {
            if (checkMode == ESR_Details.CellData)
            {
                if (FGenerators.CheckIfIsNull(cell))
                {
                    if (tag.Length > 0) if (tag[0] == '!') return new SpawnData() { OwnerCell = cell };
                    return null;
                }

                if (CellHaveData(cell, tag)) return new SpawnData() { OwnerCell = cell };
                return null;
            }

            if (FGenerators.CheckIfExist_NOTNULL(cell))
            {
                var spawns = cell.CollectSpawns();
                for (int i = 0; i < spawns.Count; i++)
                    if (SpawnHaveSpecifics(spawns[i], tag, checkMode))
                        return spawns[i];
            }

            return null;
        }


        public static float CompareOffsetDirectionalAngle(SpawnData spawn, SpawnData spawnData, Quaternion? rotate = null)
        {
            Vector3 pos = rotate == null ? spawn.GetFullOffset(true) : rotate.Value * spawn.GetFullOffset(true);
            //pos = Vector3.LerpUnclamped(pos, Vector3.zero, 0.1f);
            Vector3 targetPos = spawnData.GetFullOffset(true);
            return Vector3.Angle(pos.normalized, targetPos.normalized);
        }

        public static float CompareDirectionalAngle(SpawnData spawn, SpawnData spawnData, ESR_AngleRemovalMode angleMode, bool worldDirection = true/*, FieldCell debug = null*/)
        {
            Vector3 pos = spawn.GetFullOffset(true);
            Vector3 targetPos = spawnData.GetFullOffset(true);

            Quaternion angleRot;

            if (angleMode == ESR_AngleRemovalMode.Any)
            {
                angleRot = Quaternion.identity;
            }
            else
            {
                if (!worldDirection)
                {
                    angleRot = SpawnRules.GetRemovalAngle(angleMode) * spawn.GetRotationOffset();
                }
                else
                    angleRot = SpawnRules.GetRemovalAngle(angleMode);
            }

            //if (debug is null == false)
            //{
            //    UnityEngine.Debug.DrawRay(debug.WorldPos() + pos, (targetPos.normalized - pos.normalized), Color.yellow, 1f);
            //    UnityEngine.Debug.DrawRay(debug.WorldPos() + pos, angleRot * Vector3.forward, Color.red, 1f);
            //    UnityEngine.Debug.DrawRay(debug.WorldPos(), Vector3.up, Color.yellow, 1f);
            //}

            return Vector3.SignedAngle(targetPos.normalized - pos.normalized, angleRot * Vector3.forward, SpawnRules.GetAxis(angleMode));
        }

        public static bool SpawnHaveTag(SpawnData spawn, string tag)
        {
            if (string.IsNullOrEmpty(tag)) return false;

            // Checking all tags with semicolon splitter from source tag
            string[] splitted = tag.Split(',');

            if (!string.IsNullOrEmpty(spawn.OwnerMod.ModTag))
            {
                bool have = HaveTags(spawn.OwnerMod.ModTag, splitted);
                if (have) return true;
            }

            if (!string.IsNullOrEmpty(spawn.Spawner.SpawnerTag))
            {
                bool have = HaveTags(spawn.Spawner.SpawnerTag, splitted);
                if (have) return true;
            }

            if (spawn.Spawner.Parent != null)
                if (spawn.Spawner.Parent.ParentPack != null)
                    if (!string.IsNullOrEmpty(spawn.Spawner.Parent.ParentPack.TagForAllSpawners))
                    {
                        bool have = HaveTags(spawn.Spawner.Parent.ParentPack.TagForAllSpawners, splitted);
                        if (have) return true;
                    }

            return false;
        }

        public static bool SpawnHaveStigma(SpawnData spawn, string stigma)
        {
            if (string.IsNullOrEmpty(stigma)) return false;

            // Checking all tags with semicolon splitter from source tag
            string[] splitted = stigma.Split(',');

            for (int i = 0; i < splitted.Length; i++)
            {
                if (spawn.GetCustomStigma(splitted[i])) return true;
            }

            return false;
        }

        public static bool CellHaveData(FieldCell cell, string dataString)
        {
            if (string.IsNullOrEmpty(dataString)) return false;

            // Checking all tags with semicolon splitter from source tag
            string[] splitted = dataString.Split(',');

            for (int i = 0; i < splitted.Length; i++)
            {
                if (cell.HaveCustomData(splitted[i])) return true;
            }

            return false;
        }

        public static bool SpawnHaveSpecifics(SpawnData spawn, string specification, ESR_Details checkMode)
        {
            if (string.IsNullOrEmpty(specification)) return false;

            switch (checkMode)
            {
                case ESR_Details.Tag: return SpawnHaveTag(spawn, specification);

                case ESR_Details.SpawnStigma: return SpawnHaveStigma(spawn, specification);

                case ESR_Details.CellData: return CellHaveData(spawn.OwnerCell, specification);

                case ESR_Details.Name:
                    if (spawn.Spawner == null) return false;
                    return specification == spawn.Spawner.Name;
            }

            return false;
        }


        private static List<string> _tPosit = new List<string>();
        private static List<string> _tNeg = new List<string>();
        public static bool HaveTags(string toCheck, string[] sourceTags)
        {
            if (sourceTags.Length == 0) return false;
            if (string.IsNullOrEmpty(toCheck)) return false;


            string[] checkSplit = toCheck.Split(',');


            _tPosit.Clear();
            _tNeg.Clear();


            // Collecting '!' negate tags
            for (int s = 0; s < sourceTags.Length; s++)
            {
                if (string.IsNullOrEmpty(sourceTags[s])) continue;

                if (sourceTags[s][0] == '!') // Getting negations
                {
                    _tNeg.Add(sourceTags[s].Substring(1, sourceTags[s].Length - 1));
                }
                else
                {
                    _tPosit.Add(sourceTags[s]);
                }
            }

            // Checking all input tags
            for (int s = 0; s < checkSplit.Length; s++)
            {
                int negs = 0;
                //bool negsOk = true;

                //for (int n = 0; n < _tNeg.Count; n++)
                //{
                //    if (checkSplit[s] != _tNeg[n]) { negs++; } else negsOk = false;
                //}
                for (int n = 0; n < _tNeg.Count; n++)
                {
                    if (checkSplit[s] == _tNeg[n]) { negs++; }
                }

                if (_tNeg.Count > 0) { if (negs > 0) return false; else return true; }

                for (int p = 0; p < _tPosit.Count; p++) // When any tag is detected then having tag
                {
                    if (checkSplit[s] == _tPosit[p]) return true;
                }

                //if (negsOk) { if (negs != _tNeg.Count) return true; } else return false;
            }

            //for (int i = 0; i < sourceTags.Length; i++)
            //{
            //    for (int s = 0; s < checkSplit.Length; s++)
            //    {
            //        #region Check for '!' Tag
            //        if (string.IsNullOrEmpty(sourceTags[i]) == false && sourceTags[i][0] == '!')
            //        {
            //            if (sourceTags[i].Substring(1, sourceTags[i].Length - 1) == checkSplit[i])
            //                return false;
            //            else
            //                return true;
            //        }
            //        else
            //        #endregion
            //        {
            //            if (checkSplit[s] == sourceTags[i]) return true;
            //        }
            //    }
            //}

            return false;
        }



        /// <summary> Flat cell distance </summary>
        public static int ManhattanDistance(Vector3Int pos1, Vector3Int pos2)
        {
            return Mathf.Abs(pos2.x - pos1.x) + Mathf.Abs(pos2.y - pos1.y) + Mathf.Abs(pos2.z - pos1.z);
        }

        public static SpawnData GetConditionalSpawnData(FieldCell cell, string tag, GameObject prefab = null, FieldModification mod = null)
        {
            SpawnData targetSpawn = null;

            if (mod != null) targetSpawn = CellSpawnsHaveModificator(cell, mod);
            if (FGenerators.CheckIfIsNull(targetSpawn)) return GetConditionalData(cell, tag, prefab, mod);
            return null;
        }

        public static SpawnData GetConditionalData(FieldCell cell, string tag, GameObject prefab = null, FieldModification mod = null)
        {
            SpawnData targetSpawn = null;

            if (prefab != null) targetSpawn = CellSpawnsHavePrefab(cell, prefab);
            if (string.IsNullOrEmpty(tag) == false) targetSpawn = CellSpawnsHaveTag(cell, tag);

            return targetSpawn;
        }

        public static List<SpawnData> GetAllSpecificSpawns(FieldCell cell, string tag, ESR_Details checkMode, GameObject prefab = null, FieldModification mod = null)
        {
            List<SpawnData> targetSpawn = new List<SpawnData>();

            var spawns = cell.CollectSpawns();
            for (int i = 0; i < spawns.Count; i++)
            {
                if (SpawnHaveSpecifics(spawns[i], tag, checkMode)) targetSpawn.Add(spawns[i]);
            }

            return targetSpawn;
        }

        public static Color GetProcedureColor(EProcedureType rule, float alpha = 1f)
        {
            if (rule == EProcedureType.Event)
            {
                return new Color(0.45f, 0.45f, .9f, alpha);
            }
            else if (rule == EProcedureType.OnConditionsMet)
            {
                return new Color(0.9f, 0.9f, .3f, alpha);
            }
            else if (rule == EProcedureType.Coded)
            {
                return new Color(0.3f, 0.9f, .9f, alpha);
            }
            else if (rule == EProcedureType.Rule)
            {
                return new Color(0.7f, .85f, .7f, alpha);
            }

            //return new Color(0.5f, 1f, .5f, 1f);
            return new Color(1f, 1f, 1f, alpha);
        }

        public static Vector3 GetUnitOffset(Vector3 directOffset, ESR_Measuring offsetMode, FieldSetup preset)
        {
            if (offsetMode == ESR_Measuring.Units) return directOffset;
            return Vector3.Scale(directOffset, preset.GetCellUnitSize());
        }

        public static bool DrawingCategory(EProcedureType? selectedCategory, EProcedureType current)
        {
            if (selectedCategory == EProcedureType.Rule)
            {
                if (current != EProcedureType.Rule) return false;
            }
            else if (selectedCategory == EProcedureType.Event)
            {
                if (current != EProcedureType.Event) return false;
            }
            else if (selectedCategory == EProcedureType.OnConditionsMet)
            {
                if (current != EProcedureType.OnConditionsMet) return false;
            }
            else if (selectedCategory == EProcedureType.Coded)
            {
                if (current == EProcedureType.Rule ||
                    current == EProcedureType.Event ||
                    current == EProcedureType.OnConditionsMet)
                    return false;
            }

            return true;
        }


        /// <summary> Leave tags empty if you want prevent all spawns</summary>
        public static SpawnInstruction GeneratePreventSpawns(string tags = "")
        {
            SpawnInstruction preventSpawn;
            preventSpawn = new SpawnInstruction();
            preventSpawn.definition = new InstructionDefinition();

            if (string.IsNullOrEmpty(tags))
            {
                preventSpawn.definition.InstructionType = InstructionDefinition.EInstruction.PreventAllSpawn;
            }
            else
            {
                preventSpawn.definition.Tags = tags;
                preventSpawn.definition.InstructionType = InstructionDefinition.EInstruction.PreventSpawnSelective;
            }

            return preventSpawn;
        }


        public virtual SpawnRuleBase CustomCopy(FieldSpawner spawner)
        {
            return null;
        }


        #region Selector Support

        public static bool CellSelector_CustomSelection(CheckCellsSelectorSetup setup)
        {
            if (setup.ToCheck == null) return false;
            if (setup.ToCheck.Count == 0) return false;
            if (setup.ToCheck.Count == 1) if (setup.ToCheck[0] == Vector3Int.zero) return false;
            return true;
        }

        public static int CellSelector_SelectedCount(CheckCellsSelectorSetup setup)
        {
            if (setup.ToCheck == null) return 0;
            return setup.ToCheck.Count;
        }

        /// <summary>
        /// Returns true if all cells are needed, if at least one then returns false
        /// </summary>
        public static bool CellSelector_InitialCondition(CheckCellsSelectorSetup setup)
        {
            if (setup.UseCondition == false || (setup.UseCondition && setup.Condition == ESR_NeightbourCondition.AllNeeded)) return true;
            return false;
        }

        public static FieldCell CellSelector_GetSingleSelectedCell(CheckCellsSelectorSetup setup, FGenGraph<FieldCell, FGenPoint> grid, FieldCell origin, SpawnData spawn, int index = 0)
        {
            if (setup.ToCheck == null) return origin;
            if (setup.ToCheck.Count == 0) return origin;
            if (setup.ToCheck.Count == 1) if (setup.ToCheck[0] == Vector3Int.zero) return origin;
            if (index >= setup.ToCheck.Count) return origin;
            if (setup.ToCheck[index] == Vector3Int.zero) return origin;
            return CellSelector_GetCell(setup, grid, origin, spawn, index);
            //return grid.GetCell(origin.Pos + setup.ToCheck[index], false);
        }

        public static List<FieldCell> CellSelector_GetSelectedCells(CheckCellsSelectorSetup setup, FGenGraph<FieldCell, FGenPoint> grid, FieldCell origin, SpawnData spawn)
        {
            List<FieldCell> cells = new List<FieldCell>();

            for (int i = 0; i < setup.ToCheck.Count; i++)
            {
                cells.Add(CellSelector_GetCell(setup, grid, origin, spawn, i));
                //cells.Add(grid.GetCell(origin.Pos + setup.ToCheck[i], false));
            }

            return cells;
        }

        public static FieldCell CellSelector_GetCell(CheckCellsSelectorSetup setup, FGenGraph<FieldCell, FGenPoint> grid, FieldCell origin, SpawnData spawn, int index)
        {
            Vector3Int targetPos = origin.Pos;

            if (setup.UseRotor)
            {
                if (setup.Rotor == CheckCellsSelectorSetup.ERotor.RotorWithSpawnRotation)
                {
                    if (FGenerators.CheckIfExist_NOTNULL(spawn))
                    {
                        Vector3 rot = spawn.GetFullRotationOffset();

                        if (rot != Vector3.zero)
                        {
                            rot = (Quaternion.Euler(rot) * setup.ToCheck[index]);
                            targetPos += new Vector3Int(Mathf.RoundToInt(rot.x), Mathf.RoundToInt(rot.y), Mathf.RoundToInt(rot.z));
                        }
                        else
                            targetPos += setup.ToCheck[index];
                    }
                    else
                        targetPos += setup.ToCheck[index];
                }
                else
                    targetPos += setup.ToCheck[index];
            }
            else
            {
                targetPos += setup.ToCheck[index];
            }

            return grid.GetCell(targetPos, true);
        }



        public static bool CellSelector_CheckCondition(CheckCellsSelectorSetup setup, FGenGraph<FieldCell, FGenPoint> grid, FieldCell origin, SpawnData spawn, FieldSpawner spawner, Func<FieldCell, SpawnData, FieldSpawner, bool> conditionMethod)
        {
            if (CellSelector_CustomSelection(setup) == false) return conditionMethod(origin, spawn, spawner);

            bool condMet = CellSelector_InitialCondition(setup);

            for (int i = 0; i < CellSelector_SelectedCount(setup); i++)
            {
                FieldCell cell = CellSelector_GetCell(setup, grid, origin, spawn, i);

                bool isOk = conditionMethod(cell, spawn, spawner);

                if (setup.Condition == ESR_NeightbourCondition.AtLeastOne)
                {
                    if (isOk) { condMet = true; break; }  // At least one met
                }
                else
                {
                    if (!isOk) { condMet = false; break; } // All needed
                }
            }

            return condMet;
        }


        public static bool CellSelector_CheckCondition(CheckCellsSelectorSetup setup, FGenGraph<FieldCell, FGenPoint> grid, FieldCell origin, SpawnData spawn, FieldSetup field, Func<FieldCell, SpawnData, FGenGraph<FieldCell, FGenPoint>, FieldSetup, bool> conditionMethod)
        {
            if (CellSelector_CustomSelection(setup) == false) return conditionMethod(origin, spawn, grid, field);

            bool condMet = CellSelector_InitialCondition(setup);

            for (int i = 0; i < CellSelector_SelectedCount(setup); i++)
            {
                FieldCell cell = CellSelector_GetCell(setup, grid, origin, spawn, i);

                bool isOk = conditionMethod(cell, spawn, grid, field);

                if (setup.Condition == ESR_NeightbourCondition.AtLeastOne)
                {
                    if (isOk) { condMet = true; break; }  // At least one met
                }
                else
                {
                    if (!isOk) { condMet = false; break; } // All needed
                }
            }

            return condMet;
        }


        public static void CellSelector_Execute(CheckCellsSelectorSetup setup, FGenGraph<FieldCell, FGenPoint> grid, FieldCell origin, FieldCell secCell, SpawnData spawn, Action<FieldCell, SpawnData> actionMethod)
        {
            if (CellSelector_CustomSelection(setup) == false) { actionMethod(secCell, spawn); return; }

            for (int i = 0; i < CellSelector_SelectedCount(setup); i++)
            {
                FieldCell cell = CellSelector_GetCell(setup, grid, origin, spawn, i);
                actionMethod(cell, spawn);
            }
        }


        public static bool CellSelector_CheckCondition(CheckCellsSelectorSetup setup, FGenGraph<FieldCell, FGenPoint> grid, FieldCell origin, SpawnData spawn, Func<FieldCell, SpawnData, bool> conditionMethod)
        {
            if (CellSelector_CustomSelection(setup) == false) return conditionMethod(origin, spawn);

            bool condMet = CellSelector_InitialCondition(setup);

            for (int i = 0; i < CellSelector_SelectedCount(setup); i++)
            {
                FieldCell cell = CellSelector_GetCell(setup, grid, origin, spawn, i);

                bool isOk = conditionMethod(cell, spawn);

                if (setup.Condition == ESR_NeightbourCondition.AtLeastOne)
                {
                    if (isOk) { condMet = true; break; } // At least one met
                }
                else
                {
                    if (!isOk) { condMet = false; break; } // All needed not met
                }
            }

            return condMet;
        }


        public static bool CellSelector_CheckCondition(CheckCellsSelectorSetup setup, FGenGraph<FieldCell, FGenPoint> grid, FieldCell origin, SpawnData spawn, Func<FieldCell, bool> conditionMethod)
        {
            if (CellSelector_CustomSelection(setup) == false) return conditionMethod(origin);

            bool condMet = CellSelector_InitialCondition(setup);

            for (int i = 0; i < CellSelector_SelectedCount(setup); i++)
            {
                FieldCell cell = CellSelector_GetCell(setup, grid, origin, spawn, i);
                bool isOk = conditionMethod(cell);
                if (setup.Condition == ESR_NeightbourCondition.AtLeastOne)
                { if (isOk) { condMet = true; break; } }
                else if (!isOk) { condMet = false; break; } // All needed
            }

            return condMet;
        }


        #endregion

    }
}