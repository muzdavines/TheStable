using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules
{
    public enum ESR_Space { Empty, Occupied, OutOfGrid, InGrid }

    public enum ESR_NeightbourCondition
    {
        [Tooltip("All selected cells must met node rule")]
        AllNeeded,
        [Tooltip("Just one cell need to met node rule then condition is completed")]
        AtLeastOne
    }


    public enum ESR_DirectionMode { NoOffset, WorldDirection, CellRotateDirection }
    public enum ESR_OffsetSpace { WorldDirection, CellRotateDirection }

    public enum ESR_AngleRemovalMode { Any, OnTheLeft, OnTheRight, InFront, Behind, Below, Above }

    public enum ESR_DistanceRule { Lower, Greater, Equal }

    public enum ESR_Measuring { Units, Cells }
    public enum ESR_Transforming { Position, Rotation, Scale }
    public enum ESR_RotationOrScale { Rotation, Scale }

    public enum ESR_Origin
    {
        [Tooltip("Measure distance using prefabs origin positions")]
        SpawnPosition,
        [Tooltip("Measure distance using first-mesh model bounds center position")]
        BoundsCenter,
        [Tooltip("Measure distance using first found renderer bounds center position")]
        RendererCenter
    }

    public enum ESR_Details { Tag, SpawnStigma, CellData, Name }


    public static class SpawnRules
    {
        static readonly Quaternion Q0 = Quaternion.Euler(0, 0, 0);
        static readonly Quaternion Q90 = Quaternion.Euler(0, 90, 0);
        static readonly Quaternion Q180 = Quaternion.Euler(0, 180, 0);
        static readonly Quaternion Q270 = Quaternion.Euler(0, 270, 0);
        static readonly Quaternion QX90 = Quaternion.Euler(90, 0, 0);
        static readonly Quaternion QX270 = Quaternion.Euler(270, 0, 0);
        public static Quaternion GetRemovalAngle(ESR_AngleRemovalMode mode)
        {
            switch (mode)
            {
                case ESR_AngleRemovalMode.OnTheLeft: return Q270;
                case ESR_AngleRemovalMode.OnTheRight: return Q90;
                case ESR_AngleRemovalMode.InFront: return Q0;
                case ESR_AngleRemovalMode.Behind: return Q180;
                case ESR_AngleRemovalMode.Below: return QX90;
                case ESR_AngleRemovalMode.Above: return QX270;
            }

            return Q0;
        }

        internal static Vector3 GetAxis(ESR_AngleRemovalMode angleMode)
        {
            if (angleMode == ESR_AngleRemovalMode.Above || angleMode == ESR_AngleRemovalMode.Below)
                return Vector3.right;
            else
                return Vector3.up;
        }


        public static bool IsNull(this FieldCell data)
        { return FGenerators.CheckIfIsNull(data); }

        public static bool NotNull(this FieldCell data)
        { return FGenerators.CheckIfExist_NOTNULL(data); }

        public static bool IsNull(this SpawnData data)
        { return FGenerators.CheckIfIsNull(data); }

        public static bool NotNull(this SpawnData data)
        { return FGenerators.CheckIfExist_NOTNULL(data); }

        private static bool CheckCellAllowOR(FieldCell cell, string occupyTags, ESR_Details checkMode, bool useStateORTag = false)
        {
            if (useStateORTag == false)
                return false;
            else
            {
                if (string.IsNullOrEmpty(occupyTags)) return false;
                else
                    return CheckNeightbourCellAllow(ESR_Space.Occupied, cell, occupyTags, checkMode);
            }
        }

        public static bool CheckNeightbourCellAllow(this ESR_Space check, FieldCell cell, string occupyTags, ESR_Details checkMode, bool useStateORTag = false)
        {
            if (cell == null)
            {
                if (check == ESR_Space.OutOfGrid)
                    return true;
                else
                {
                    // Not occupied at all (no cell) - so false
                    return false;

                    //if (checkMode == ESR_Details.CellData)
                    //{
                    //    if (occupyTags.Length > 0) if (occupyTags[0] == '!') return true;
                    //}
                    //return CheckCellAllowOR(cell, occupyTags, checkMode, useStateORTag);
                }
            }
            else
            {
                if (check == ESR_Space.InGrid)
                {
                    if (cell.InTargetGridArea)
                    {
                        return true;
                    }
                    else
                    {
                        return CheckCellAllowOR(cell, occupyTags, checkMode, useStateORTag);
                    }
                }
            }

            if (check == ESR_Space.Occupied)
            {
                if (checkMode == ESR_Details.CellData)
                {
                    if (FGenerators.CheckIfExist_NOTNULL(SpawnRuleBase.CellSpawnsHaveSpecifics(cell, occupyTags, ESR_Details.CellData)))
                        //if (SpawnRuleBase.CellHaveData(cell, occupyTags))
                        return true;
                    else
                        return false;
                }

                if (string.IsNullOrEmpty(occupyTags))
                {
                    if (cell.CollectSpawns().Count > 0) return true;
                }
                else
                {
                    if (cell.CollectSpawns().Count <= 0) return false;
                    else
                    {
                        if (FGenerators.CheckIfExist_NOTNULL(SpawnRuleBase.CellSpawnsHaveSpecifics(cell, occupyTags, checkMode)))
                            return true;
                    }
                }
            }
            else if (check == ESR_Space.Empty)
            {
                if (cell.CollectSpawns().Count < 1) return true;
                return CheckCellAllowOR(cell, occupyTags, checkMode, useStateORTag);

            }
            else if (check == ESR_Space.OutOfGrid)
            {
                if (cell.InTargetGridArea == false) return true;
                return CheckCellAllowOR(cell, occupyTags, checkMode, useStateORTag);
            }
            else if (check == ESR_Space.InGrid)
            {
                if (cell.InTargetGridArea) return true;
                return CheckCellAllowOR(cell, occupyTags, checkMode, useStateORTag);
            }

            return false;
        }

        public static bool CheckNeightbourCellAllowAngled(this ESR_Space check, FieldCell cell, string occupyTags, Vector3 checkDirection, float ignoreAboveAngle, ESR_Details checkMode, bool useStateORTag = false)
        {
            if (cell == null)
            {
                if (check == ESR_Space.OutOfGrid)
                    return true;
                else
                    return CheckCellAllowOR(cell, occupyTags, checkMode, useStateORTag);
            }
            else
            {
                if (check == ESR_Space.InGrid) if (cell.InTargetGridArea)
                        return true;
                    else
                        return CheckCellAllowOR(cell, occupyTags, checkMode, useStateORTag);

            }

            if (check == ESR_Space.Occupied)
            {

                if (string.IsNullOrEmpty(occupyTags))
                {
                    if (cell.CollectSpawns().Count > 0)
                    {
                        if (ignoreAboveAngle > 0f)
                        {
                            var spawns = cell.CollectSpawns();
                            for (int i = 0; i < spawns.Count; i++)
                            {
                                var spawn = spawns[i];
                                float angle = Vector3.Angle(checkDirection, spawn.GetFullOffset().normalized);

                                //float angle = Quaternion.Angle(checkDirection, Quaternion.Euler(spawn.LocalRotationOffset + spawn.RotationOffset));
                                if (angle <= ignoreAboveAngle) return true;
                            }
                        }
                    }
                    else
                        return true;
                }
                else
                {
                    if (cell.CollectSpawns().Count <= 0)
                        return false;
                    else
                    {
                        if (ignoreAboveAngle > 0f)
                        {
                            var spawns = SpawnRuleBase.GetAllSpecificSpawns(cell, occupyTags, checkMode);
                            for (int i = 0; i < spawns.Count; i++)
                            {
                                var spawn = spawns[i];
                                float angle = Vector3.Angle(checkDirection, spawn.GetFullOffset().normalized);
                                //UnityEngine.Debug.Log("Angle " + angle + " " + cell.FlatPosInt);
                                //float angle = Quaternion.Angle(checkDirection, Quaternion.Euler(spawn.LocalRotationOffset + spawn.RotationOffset));
                                if (angle <= ignoreAboveAngle) return true;
                            }
                        }
                        else

                        if (FGenerators.CheckIfExist_NOTNULL(SpawnRuleBase.CellSpawnsHaveSpecifics(cell, occupyTags, checkMode)))
                        {
                            return true;
                        }
                    }
                }
            }
            else if (check == ESR_Space.Empty)
            {
                if (cell.CollectSpawns().Count < 1) return true;
                return CheckCellAllowOR(cell, occupyTags, checkMode, useStateORTag);
            }
            else if (check == ESR_Space.OutOfGrid)
            {
                if (cell.InTargetGridArea == false) return true;
                return CheckCellAllowOR(cell, occupyTags, checkMode, useStateORTag);
            }
            else if (check == ESR_Space.InGrid)
            {
                if (cell.InTargetGridArea) return true;
                return CheckCellAllowOR(cell, occupyTags, checkMode, useStateORTag);
            }

            return false;
        }

        /// <summary> Get all neightbours around cell with option to generate ghost cells </summary>
        public static List<FieldCell> GetTargetNeightboursAround(FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, ESR_Space targetState, string tags, ESR_Details checkMode, bool useStateORTag = false)
        {
            List<FieldCell> cells = new List<FieldCell>();

            for (int x = -1; x <= 1; x++)
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && z == 0) continue; // Ignore self

                    FieldCell nCell = grid.GetCell(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z, false);
                    if (SpawnRules.CheckNeightbourCellAllow(targetState, nCell, tags, checkMode, useStateORTag) == false) continue;

                    if (nCell != null)
                        cells.Add(nCell);
                    else
                        if (targetState == ESR_Space.OutOfGrid)
                        cells.Add(new FieldCell() { Pos = new Vector3Int(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z), InTargetGridArea = false });
                }

            return cells;
        }

        /// <summary> Get all neightbours around cell and generate ghost cells instead of nulls </summary>
        public static List<FieldCell> Get3x3Square(FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            List<FieldCell> cells = new List<FieldCell>();

            for (int x = -1; x <= 1; x++)
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && z == 0) continue; // Ignore self

                    FieldCell nCell = grid.GetCell(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z, false);

                    if (nCell != null)
                        cells.Add(nCell);
                    else
                        cells.Add(new FieldCell() { Pos = new Vector3Int(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z), InTargetGridArea = false });
                }

            return cells;
        }

        public static List<FieldCell> GetTargetNeightboursDiagonal(FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            List<FieldCell> cells = new List<FieldCell>();

            for (int x = -1; x <= 1; x++)
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && z == 0) continue; // Ignore self
                    if (x == 0 || z == 0) continue;

                    FieldCell nCell = grid.GetCell(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z, false);

                    if (nCell != null)
                        cells.Add(nCell);
                    else
                        cells.Add(new FieldCell() { Pos = new Vector3Int(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z) });
                }

            return cells;
        }


        public static List<FieldCell> GetTargetNeightboursDiagonal(FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, ESR_Space targetState, string tags, ESR_Details checkMode, bool useStateORTag = false)
        {
            List<FieldCell> cells = new List<FieldCell>();

            for (int x = -1; x <= 1; x++)
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && z == 0) continue; // Ignore self
                    if (x == 0 || z == 0) continue;

                    FieldCell nCell = grid.GetCell(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z, false);
                    if (SpawnRules.CheckNeightbourCellAllow(targetState, nCell, tags, checkMode, useStateORTag) == false) continue;

                    if (nCell != null)
                        cells.Add(nCell);
                    else
                        if (targetState == ESR_Space.OutOfGrid)
                        cells.Add(new FieldCell() { Pos = new Vector3Int(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z) });
                }

            return cells;
        }

        internal static SpawnData GetSpawnRotated(FieldCell cell, Quaternion targetRot, float tolerance = 1f)
        {
            var spawns = cell.CollectSpawns();

            for (int i = 0; i < spawns.Count; i++)
            {
                var s = spawns[i];
                if (Quaternion.Angle(Quaternion.Euler(s.RotationOffset), targetRot) < tolerance)
                    return s;
            }

            return null;
        }

        internal static SpawnData GetNearest(FieldCell cell, Vector3 wpos, FieldSetup forNearestCheck, float maxDistance = 1000f, System.Func<SpawnData, bool> iterationCondition = null)
        {
            var spawns = cell.CollectSpawns();

            float nearest = float.MaxValue;
            float maxDistSqrt = maxDistance * maxDistance;
            SpawnData nesrSp = null;
            //if (spawn.PreviewMesh) 
            //    wpos = cell.WorldPos(forNearestCheck) + spawn.GetMeshFilterOrColliderBounds().center;

            for (int i = 0; i < spawns.Count; i++)
            {
                var s = spawns[i];
                if (s.DontSpawnMainPrefab) continue;
                //if (Quaternion.Angle(Quaternion.Euler(s.RotationOffset), targetRot) < tolerance)
                {
                    Vector3 refPos;
                    float dist;

                    //if (s.PreviewMesh) 
                    //    refPos = cell.WorldPos(forNearestCheck) + s.GetMeshFilterOrColliderBounds().center;
                    //else
                    refPos = (s.GetWorldPositionWithFullOffset(forNearestCheck));

                    dist = (refPos - wpos).sqrMagnitude;
                    if (dist > maxDistSqrt) continue;

                    if (iterationCondition != null)
                    {
                        if (iterationCondition.Invoke(s) == false) continue;
                    }

                    if (dist < nearest)
                    {
                        nearest = dist;
                        nesrSp = s;
                    }
                }
            }

            return nesrSp;
        }


        internal static SpawnData GetSpawnRotated(FieldCell cell, Quaternion targetRot, FieldModification mustHaveMod, string cantHaveStigma, float tolerance = 1f)
        {
            var spawns = cell.CollectSpawns();
            for (int i = 0; i < spawns.Count; i++)
            {
                var s = spawns[i];
                bool mod = false;

                if (mustHaveMod != null) mod = s.OwnerMod == mustHaveMod;
                if (mod) if (!string.IsNullOrEmpty(cantHaveStigma)) if (s.GetCustomStigma(cantHaveStigma)) mod = false;

                if (mod)
                    if (Quaternion.Angle(Quaternion.Euler(s.RotationOffset), targetRot) < tolerance)
                        return s;
            }

            return null;
        }

        internal static SpawnData GetSpawnWithStigma(FieldCell cell, string stigma, FieldModification onlyMod = null)
        {
            var spawns = cell.CollectSpawns();

            if (onlyMod == null)
            {
                for (int i = 0; i < spawns.Count; i++) if (spawns[i].GetCustomStigma(stigma)) return spawns[i];
            }
            else
            {
                for (int i = 0; i < spawns.Count; i++) if (spawns[i].OwnerMod == onlyMod) if (spawns[i].GetCustomStigma(stigma)) return spawns[i];
            }

            return null;
        }

        internal static SpawnData GetSpawnWithExactMod(FieldCell cell, FieldModification mod = null)
        {
            var spawns = cell.CollectSpawns();

            if (mod == null)
            {
                return null;
            }
            else
            {
                for (int i = 0; i < spawns.Count; i++) if (spawns[i].OwnerMod == mod) return spawns[i];
            }

            return null;
        }

        internal static bool IsSpawnRotated(SpawnData spawn, Quaternion targetRotation)
        {
            if (Quaternion.Angle(Quaternion.Euler(spawn.RotationOffset), targetRotation) < 1f) return true;
            return false;
        }

        public static List<FieldCell> GetTargetNeightboursPLUS(FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            List<FieldCell> cells = new List<FieldCell>();

            for (int x = -1; x <= 1; x++)
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && z == 0) continue; // Ignore self
                    if (x != 0 && z != 0) continue; // Ignore diagonals

                    FieldCell nCell = grid.GetCell(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z, false);

                    if (FGenerators.CheckIfExist_NOTNULL(nCell))
                    {
                        cells.Add(nCell);
                    }
                    else // null cell but it's desired state
                        cells.Add(new FieldCell() { Pos = new Vector3Int(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z) });
                }

            return cells;
        }

        public static List<FieldCell> GetTargetNeightboursPLUS(FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, ESR_Space targetState, string tags, ESR_Details checkMode, bool useStateORTag = false)
        {
            List<FieldCell> cells = new List<FieldCell>();

            for (int x = -1; x <= 1; x++)
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && z == 0) continue; // Ignore self
                    if (x != 0 && z != 0) continue; // Ignore diagonals

                    FieldCell nCell = grid.GetCell(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z, false);
                    if (SpawnRules.CheckNeightbourCellAllow(targetState, nCell, tags, checkMode, useStateORTag) == false) continue;

                    if (FGenerators.CheckIfExist_NOTNULL(nCell))
                    {
                        cells.Add(nCell);
                    }
                    else // null cell but it's desired state
                        if (targetState == ESR_Space.OutOfGrid)
                        cells.Add(new FieldCell() { Pos = new Vector3Int(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z) });
                }

            return cells;
        }


        public static readonly Quaternion Step0 = Quaternion.Euler(0f, 0f, 0f);
        public static readonly Quaternion Step45 = Quaternion.Euler(0f, 45f, 0f);
        public static readonly Quaternion Step90 = Quaternion.Euler(0f, 90f, 0f);
        public static readonly Quaternion Step135 = Quaternion.Euler(0f, 135f, 0f);
        public static readonly Quaternion Step180 = Quaternion.Euler(0f, 180f, 0f);
        public static readonly Quaternion Step225 = Quaternion.Euler(0f, 225, 0f);
        public static readonly Quaternion Step270 = Quaternion.Euler(0f, 270f, 0f);
        public static readonly Quaternion Step315 = Quaternion.Euler(0f, 315f, 0f);

        public enum CellOffset
        {
            Angle0, Angle90, Angle180, Angle270, Angle45, Angle135, Angle225, Angle315
        }

        public static Vector3Int GetCellOffset(CellOffset off)
        {
            switch (off)
            {
                case CellOffset.Angle0: return new Vector3Int(0, 0, 1);
                case CellOffset.Angle90: return new Vector3Int(1, 0, 0);
                case CellOffset.Angle180: return new Vector3Int(0, 0, -1);
                case CellOffset.Angle270: return new Vector3Int(-1, 0, 0);
                case CellOffset.Angle45: return new Vector3Int(1, 0, 1);
                case CellOffset.Angle135: return new Vector3Int(-1, 0, 1);
                case CellOffset.Angle225: return new Vector3Int(-1, 0, -1);
                case CellOffset.Angle315: return new Vector3Int(1, 0, -1);
                default: return Vector3Int.zero;
            }
        }


        //public static Vector3 GetCellOffsetV3(CellOffset off)
        //{
        //    return V2toV3(GetCellOffset(off));
        //}

        public static Quaternion GetCellOffsetQ(CellOffset off)
        {
            switch (off)
            {
                case CellOffset.Angle0: return Step0;
                case CellOffset.Angle45: return Step45;
                case CellOffset.Angle90: return Step90;
                case CellOffset.Angle135: return Step135;
                case CellOffset.Angle180: return Step180;
                case CellOffset.Angle225: return Step225;
                case CellOffset.Angle270: return Step270;
                case CellOffset.Angle315: return Step315;
                default: return Step0;
            }
        }

        public static CellOffset GetCellOffset45Step(int id)
        {
            switch (id)
            {
                case 0: return CellOffset.Angle0;
                case 1: return CellOffset.Angle45;
                case 2: return CellOffset.Angle90;
                case 3: return CellOffset.Angle135;
                case 4: return CellOffset.Angle180;
                case 5: return CellOffset.Angle225;
                case 6: return CellOffset.Angle270;
                case 7: return CellOffset.Angle315;
                default: return CellOffset.Angle0;
            }
        }

        public static Vector3 V2toV3(Vector2 flat)
        {
            return new Vector3(flat.x, 0, flat.y);
        }

        public static Vector3Int V2toV3Int(Vector2 flat)
        {
            return new Vector3Int(Mathf.RoundToInt(flat.x), 0, Mathf.RoundToInt(flat.y));
        }

        public static Vector2Int V2toV2Int(Vector2 flat)
        {
            return new Vector2Int(Mathf.RoundToInt(flat.x), Mathf.RoundToInt(flat.y));
        }

        public static CellOffset Get90Offset(int step)
        {
            if (step == 0) return CellOffset.Angle0;
            else if (step == 1) return CellOffset.Angle90;
            else if (step == 2) return CellOffset.Angle180;
            else return CellOffset.Angle270;
        }

        public static FieldCell GetAngledNeightbour(FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, CellOffset angle)
        {
            FieldCell c = grid.GetCell(cell.Pos + GetCellOffset(angle), false);

            if (FGenerators.CheckIfIsNull(c))

                c = new FieldCell() { Pos = cell.Pos + (GetCellOffset(angle)), InTargetGridArea = false };
            return c;
        }

        public static FieldCell GetAngledNeightbour45(FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, int angleId)
        {
            if (angleId > 7) angleId -= 8;
            if (angleId < 0) angleId += 8;
            FieldCell c = grid.GetCell(cell.Pos + GetCellOffset(GetCellOffset45Step(angleId)), false);

            if (FGenerators.CheckIfIsNull(c))
                c = new FieldCell() { Pos = cell.Pos + (GetCellOffset(GetCellOffset45Step(angleId))), InTargetGridArea = false };
            return c;
        }

        public static FieldCell GetAngledNeightbour90(FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, int angleId)
        {
            if (angleId > 3) angleId -= 4;
            if (angleId < 0) angleId += 4;
            FieldCell c = grid.GetCell(cell.Pos + GetCellOffset(SpawnRules.Get90Offset(angleId)), false);

            if (FGenerators.CheckIfIsNull(c))
                c = new FieldCell() { Pos = cell.Pos + (GetCellOffset(SpawnRules.Get90Offset(angleId))), InTargetGridArea = false };
            return c;
        }

    }
}