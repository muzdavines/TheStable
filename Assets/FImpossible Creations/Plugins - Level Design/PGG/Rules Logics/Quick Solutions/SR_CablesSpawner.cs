using FIMSpace.Generating.Rules.Helpers;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace FIMSpace.Generating.Rules.QuickSolutions.Alpha
{
    public class SR_CablesSpawner : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Cables Spawner"; }
        public override string Tooltip() { return "Spawning multiple instances of choosed prefab in line until reaching desired spawn"; }
        public EProcedureType Type { get { return EProcedureType.Coded; } }

        [Tooltip("Measured in cells")] public int CheckDistance = 10;
        public Vector3 CollisionOrigin = Vector3.zero;

        [Space(5)]
        [PGG_SingleLineSwitch("CheckMode", 50, "Select if you want to use Tags, SpawnStigma or CellData", 122)]
        public string IgnoreCollisionWith = "";
        [HideInInspector] public ESR_Details IgnCollCheckMode = ESR_Details.Tag;

        [PGG_SingleLineSwitch("CheckMode", 56, "Select if you want to use Tags, SpawnStigma or CellData", 110)]
        public string AttachToTagged = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        public string PutDataInCells = "";

        [Space(5)]
        public ESR_OffsetSpace Direction = ESR_OffsetSpace.CellRotateDirection;
        public Vector3 SpawnDirection = Vector3.forward;

        public SpawnSettings OptionalBranching;

        [Space(6)]
        public bool Debug = false;

        #region Editor

#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            base.NodeBody(so);
            if (CheckDistance < 2) CheckDistance = 2;
        }
#endif

        #endregion

        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);
            OptionalBranching.Reset();
            RunCheck(ref spawn, cell, grid);
        }

        List<FieldCell> cellsUntilCollision = new List<FieldCell>();

        void RunCheck(ref SpawnData spawn, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            cellsUntilCollision = ProceedLineCollision(ref spawn, cell, grid);

            if (cellsUntilCollision.Count > 0)
            {
                var finalCell = cellsUntilCollision[cellsUntilCollision.Count - 1];
                if (FGenerators.CheckIfIsNull(finalCell )) return; // No Cell

                SpawnData sp;
                if (string.IsNullOrEmpty(AttachToTagged) == false)
                {
                    sp = GetSpawnDataWithSpecifics(finalCell, AttachToTagged, CheckMode);
                    if (FGenerators.CheckIfIsNull(sp)) return;// If final collision not on desired tag
                }
                else
                {
                    if (finalCell.GetSpawnsJustInsideCell().Count == 0) return;
                    sp = finalCell.GetSpawnsJustInsideCell()[0];
                }

                if (sp != null)
                {
                    int distance = Mathf.RoundToInt(FVectorMethods.DistanceTopDownManhattan(finalCell.Pos, cell.Pos));
                    if (distance >= 2) CellAllow = true;
                }
            }
        }

        List<FieldCell> ProceedLineCollision(ref SpawnData spawn, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            List<FieldCell> checkedCells = new List<FieldCell>();
            Vector3Int dir;
            Quaternion spawnRot;

            if (Direction == ESR_OffsetSpace.WorldDirection)
            {
                spawnRot = Quaternion.LookRotation(SpawnDirection);
                dir = PGGUtils.V3toV3Int(SpawnDirection.normalized);
            }
            else
            {
                spawnRot = spawn.GetRotationOffset();
                dir = PGGUtils.V3toV3Int(spawnRot * SpawnDirection.normalized);
            }

            Vector3 origin = spawn.GetFullOffset(true) + spawnRot * CollisionOrigin;

            for (int i = 1; i < CheckDistance; i++)
            {
                if ( OptionalBranching.randomBranching )
                {
                    dir = PGGUtils.V3toV3Int(spawnRot * (Vector3)OptionalBranching.GetDir());
                }

                var nCell = grid.GetCell(cell.Pos + dir * i, false);
                if (FGenerators.CheckIfIsNull(nCell ) || nCell.InTargetGridArea == false)
                {
                    return checkedCells;
                }

                var collided = CheckBoundLineCollision(nCell, origin - dir, dir, IgnoreCollisionWith);
                if (collided != null)
                {
                    checkedCells.Add(nCell);
                    return checkedCells;
                }

                checkedCells.Add(nCell);
            }

            return checkedCells;
        }


        public SpawnData CheckBoundLineCollision(FieldCell checkInCell, Vector3 origin, Vector3Int dir, string ignoreCollisionWith)
        {
            Ray ray = new Ray(origin - dir, dir);

            var cellSpawns = checkInCell.CollectSpawns(OwnerSpawner.ScaleAccess);
            for (int i = 0; i < cellSpawns.Count; i++)
            {
                var s = cellSpawns[i];
                if (string.IsNullOrEmpty(ignoreCollisionWith) == false) if (SpawnHaveSpecifics(s, ignoreCollisionWith, IgnCollCheckMode)) continue;

                Mesh collM = s.PreviewMesh;
                if (collM != null)
                {
                    CollisionOffsetData otherData = new CollisionOffsetData(s);
                    Bounds cBounds = otherData.boundsWithSpawnOff;

                    float dist;
                    if (cBounds.IntersectRay(ray, out dist))
                    {
                        return s;
                    }
                    else { }
                }
            }

            return null;
        }


        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            if (tempSpawns != null)
                if (tempSpawns.Count > 0)
                {
                    for (int i = 0; i < tempSpawns.Count; i++) tempSpawns[i].OwnerCell.AddSpawnToCell(tempSpawns[i]);
                }
        }


        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            for (int i = 0; i < cellsUntilCollision.Count; i++)
            {
                var c = cellsUntilCollision[i];
                if (c == cell) continue;

                if (string.IsNullOrEmpty(PutDataInCells) == false)
                    c.AddCustomData(PutDataInCells);

                SpawnData cableSpawn = spawn.Copy();
                cableSpawn.OwnerCell = c;
                AddTempData(cableSpawn, spawn);
            }

            if (cellsUntilCollision.Count > 0) if (string.IsNullOrEmpty(PutDataInCells) == false) cell.AddCustomData(PutDataInCells);

            _EditorDebug = Debug;
        }

#if UNITY_EDITOR
        public override void OnDrawDebugGizmos(FieldSetup preset, SpawnData spawn, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            base.OnDrawDebugGizmos(preset, spawn, cell, grid);

            Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.6f);

            Quaternion spawnRot = spawn.GetRotationOffset();
            Vector3 pos = spawn.GetWorldPositionWithFullOffset() + spawnRot * CollisionOrigin;

            float size = 0.5f;
            Gizmos.DrawCube(pos, Vector3.one * size);
            Gizmos.DrawWireCube(pos, Vector3.one * size);

            if (Direction == ESR_OffsetSpace.WorldDirection)
            {
                Gizmos.DrawRay(pos, SpawnDirection);
                Gizmos.DrawRay(pos + SpawnDirection, -SpawnDirection * 0.2f + Vector3.up * 0.15f);
                Gizmos.DrawRay(pos + SpawnDirection, -SpawnDirection * 0.2f - Vector3.up * 0.15f);
            }
            else
            {
                Vector3 selfDir = spawnRot * SpawnDirection;
                Gizmos.DrawRay(pos, selfDir);
                Gizmos.DrawRay(pos + selfDir, -selfDir * 0.2f + Vector3.up * 0.15f);
                Gizmos.DrawRay(pos + selfDir, -selfDir * 0.2f - Vector3.up * 0.15f);
            }

            Gizmos.color = _DbPreCol;
        }
#endif


        [System.Serializable]
        public class SpawnSettings
        {
            public bool randomBranching = false;
            public List<Vector3Int> directions = new List<Vector3Int>() { new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0), new Vector3Int(0, 0, 1) };
            public MinMax branchPer = new MinMax(5, 6);

            private int iterations = 0;
            private int targetIterations = 0;
            private Vector3Int currentDir;
            public void Reset()
            {
                iterations = 0;
                targetIterations = branchPer.GetRandom();
                currentDir = directions[FGenerators.GetRandom(0, directions.Count)];
            }

            public Vector3Int GetDir()
            {
                iterations++;
                if (iterations >= targetIterations) Reset();
                return currentDir;
            }
        }

    }
}