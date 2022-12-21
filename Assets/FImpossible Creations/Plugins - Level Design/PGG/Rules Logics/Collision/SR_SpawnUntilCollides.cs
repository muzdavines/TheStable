#if UNITY_EDITOR
using UnityEditor;
#endif
using FIMSpace.Generating.Rules.Helpers;
using UnityEngine;

namespace FIMSpace.Generating.Rules.Collision
{
    public class SR_SpawnUntilCollides : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Spawn Until Collides"; }
        public override string Tooltip() { return "Checking collision in cell and spawning selected object, can jump through few cells and spawn multiple objects"; }
        public EProcedureType Type { get { return EProcedureType.Coded; } }


        [PGG_SingleLineSwitch("CheckMode", 68, "Select if you want to use Tags, SpawnStigma or CellData", 110)]
        public string StopOnTagged = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        public float CollisionBoxSize = 0.9f;
        public Vector3 StepTranslation = Vector3.up;
        public int MaxCheckCount = 4;

        [Space(4)]
        public bool JumpThroughCellsEveryStep = true;
        public bool StopWhenOutOfGrid = true;
        public Vector3 MinimumBoundsSizes = Vector3.zero;

        [Space(5)]
        public bool Debug = false;


#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            base.NodeBody(so);
            if (MaxCheckCount < 0) MaxCheckCount = 0;
        }
#endif
        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);
            CellAllow = true;
        }

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            _EditorDebug = Debug;

            // Preparing parameters for mocing on grid and checking collisions
            CollisionOffsetData thisOffset = new CollisionOffsetData(spawn);
            Vector3 startPos = cell.WorldPos(preset); startPos += thisOffset.boundsWithSpawnOff.center;
            Vector3 direction = spawn.GetRotationOffset() * StepTranslation;

            System.Collections.Generic.List<SpawnData> checkSpawns = cell.CollectSpawns(OwnerSpawner.ScaleAccess);

            Vector3 gridMax = preset.TransformCellPosition(grid.GetMax() + Vector3Int.one * 2);
            Vector3 gridMin = preset.TransformCellPosition(grid.GetMin() - Vector3Int.one * 2);

            for (int i = 0; i < MaxCheckCount; i++)
            {
                FieldCell checkCell;
                if (i == 0 || !JumpThroughCellsEveryStep)
                {
                    checkCell = cell;
                }
                else
                {
                    Vector3Int step = PGGUtils.V3toV3Int(direction.normalized);
                    checkCell = grid.GetCell(cell.Pos + step, false);
                }

                if (FGenerators.CheckIfIsNull(checkCell )) break;
                //if (StopWhenOutOfGrid) if (checkCell.InTargetGridArea == false) break;

                Vector3 checkPos;

                if (checkCell == cell)
                    checkPos = startPos + direction * i;
                else
                {
                    checkSpawns = checkCell.CollectSpawns(OwnerSpawner.ScaleAccess);
                    checkPos = checkCell.WorldPos(preset);
                    checkPos += thisOffset.boundsWithSpawnOff.center;
                }

                Bounds currentCollCheck = thisOffset.bounds;
                currentCollCheck.center = checkPos + thisOffset.bounds.center;
                currentCollCheck.size *= CollisionBoxSize;

                if (currentCollCheck.size.x < MinimumBoundsSizes.x) currentCollCheck.size = new Vector3(MinimumBoundsSizes.x, currentCollCheck.size.y, currentCollCheck.size.z);
                if (currentCollCheck.size.y < MinimumBoundsSizes.y) currentCollCheck.size = new Vector3(currentCollCheck.size.x, MinimumBoundsSizes.y, currentCollCheck.size.z);
                if (currentCollCheck.size.z < MinimumBoundsSizes.z) currentCollCheck.size = new Vector3(currentCollCheck.size.x, currentCollCheck.size.y, MinimumBoundsSizes.z);

                if (StopWhenOutOfGrid)
                {
                    Vector3 cPs = currentCollCheck.center;

                    if (direction.x < 0 && cPs.x < gridMin.x) continue;
                    else
                    if (direction.x > 0 && cPs.x > gridMax.x) continue;

                    if (direction.y < 0 && cPs.y < gridMin.y) continue;
                    else
                    if (direction.y > 0 && cPs.y > gridMax.y) continue;

                    if (direction.z < 0 && cPs.z < gridMin.z) continue;
                    else
                    if (direction.z > 0 && cPs.z > gridMax.z) continue;

                    //if (cPs.x > gridMax.x || cPs.x < gridMin.x) continue;
                    //if (cPs.y > gridMax.y || cPs.y < gridMin.y) continue;
                    //if (cPs.z > gridMax.z || cPs.z < gridMin.z) continue;
                }

                // Check box bounded collision on spawns
                for (int s = 0; s < checkSpawns.Count; s++)
                {
                    var sp = checkSpawns[s];
                    if (sp == spawn) continue;
                    if (sp.Enabled == false) continue;
                    if (sp.Spawner == OwnerSpawner) continue;

                    if (SpawnHaveSpecifics(sp, StopOnTagged, CheckMode) == false) continue; // Checking collision only on wanted tags

                    CollisionOffsetData checkCol = new CollisionOffsetData(sp);
                    Bounds oBounds = checkCol.boundsWithSpawnOff;
                    oBounds.center += checkCell.WorldPos(preset);

                    if (oBounds.Intersects(currentCollCheck))
                    {
                        // Colllision occured, hard stop
                        //FDebug.DrawBounds3D(oBounds, Color.red, 1f);
                        if (i == 0) { spawn.Enabled = false; CellAllow = false; }
                        return;
                    }
                }

                // Collision check did not break, let's add spawn
                //FDebug.DrawBounds3D(currentCollCheck, Color.black, 1f);
                if (i > 0)
                {
                    var newSpawn = spawn.Copy();
                    newSpawn.Offset += direction * i;
                    AddTempData(newSpawn, spawn);
                }
            }
        }


        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            if (tempSpawns != null) if (tempSpawns.Count > 0)
                {
                    var spawn = tempSpawns[0];
                    spawn.LocalScaleMul = thisSpawn.LocalScaleMul;
                    spawn.Enabled = true;

                    for (int i = 0; i < tempSpawns.Count; i++)
                    {
                        tempSpawns[i].LocalScaleMul = thisSpawn.LocalScaleMul;
                        cell.AddSpawnToCell(tempSpawns[i]);
                    }
                }
        }


#if UNITY_EDITOR    
        public override void OnDrawDebugGizmos(FieldSetup preset, SpawnData spawn, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            base.OnDrawDebugGizmos(preset, spawn, cell, grid);

            if (spawn.isTemp)
                Gizmos.color = new Color(0.1f, .7f, 0.1f, 0.3f);
            else
                Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.6f);

            CollisionOffsetData thisOffset = new CollisionOffsetData(spawn);
            Vector3 pos = cell.WorldPos(preset); pos += thisOffset.boundsWithSpawnOff.center;

            if (thisOffset.boundsWithSpawnOff.size.x < MinimumBoundsSizes.x) thisOffset.boundsWithSpawnOff.size = new Vector3(MinimumBoundsSizes.x, thisOffset.boundsWithSpawnOff.size.y, thisOffset.boundsWithSpawnOff.size.z);
            if (thisOffset.boundsWithSpawnOff.size.y < MinimumBoundsSizes.y) thisOffset.boundsWithSpawnOff.size = new Vector3(thisOffset.boundsWithSpawnOff.size.x, MinimumBoundsSizes.y, thisOffset.boundsWithSpawnOff.size.z);
            if (thisOffset.boundsWithSpawnOff.size.z < MinimumBoundsSizes.z) thisOffset.boundsWithSpawnOff.size = new Vector3(thisOffset.boundsWithSpawnOff.size.x, thisOffset.boundsWithSpawnOff.size.y, MinimumBoundsSizes.z);

            if (spawn.isTemp == false)
            {

                Gizmos.DrawCube(pos, thisOffset.boundsWithSpawnOff.size * CollisionBoxSize);
                Gizmos.DrawRay(pos, spawn.GetRotationOffset() * StepTranslation);
            }

            Gizmos.DrawWireCube(pos, thisOffset.boundsWithSpawnOff.size * CollisionBoxSize);
            Gizmos.color = _DbPreCol;
        }
#endif

    }
}