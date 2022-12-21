using FIMSpace.Generating.Rules.Helpers;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Placement.Alpha
{
    public class SR_FreeSpace : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Check Free Space"; }
        public override string Tooltip() { return "Allowing/disallowing to spawn if space around cell is in desired state"; }

        public EProcedureType Type { get { return EProcedureType.Procedure; } }

        [Range(0.15f, 1f)] public float FreeSpaceNeeded = 0.3f;
        [Range(0f, .9f)] public float OnlyAround = 0.5f;
        public float LimitToCellMargins = 1f;
        //public float CheckSurroundingCells = 0f;

        [Space(4)]
        [Range(0f, 1.5f)] public float MoveToFreePos = 1f;
        public bool RandomFreePos = false;

        [Space(4)]

        [PGG_SingleLineSwitch("CheckMode", 68, "Select if you want to use Tags, SpawnStigma or CellData", 40)]
        public string IgnoreTagged = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        [PGG_SingleLineSwitch("CheckMode", 68, "Select if you want to use Tags, SpawnStigma or CellData", 40)]
        public string CheckOnTagCoords = "";
        [HideInInspector] public ESR_Details CheckModeCoords = ESR_Details.Tag;

        public Vector3 CoordsDirOffset = Vector3.zero;

        [Space(4)]
        public bool debug = false;


        private static List<CollisionOffsetData> obstacles = new List<CollisionOffsetData>();
        private static List<Vector3> freePoses = new List<Vector3>();

        private Vector3? targetPos = null;

        public override void ResetRule(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset)
        {
            base.ResetRule(grid, preset);
            targetPos = null;
        }

#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            EditorGUILayout.HelpBox("To detect offset prefabs must have colliders!", MessageType.None);
            base.NodeBody(so);
        }
#endif

        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            obstacles.Clear();
            freePoses.Clear();

            Vector3? checkOrigin = null;
            Vector3 checkOriginOff = Vector3.zero;

            var spawns = cell.CollectSpawns(OwnerSpawner.ScaleAccess);

            // Getting child collision bounds data
            for (int i = 0; i < spawns.Count; i++)
            {
                if (spawns[i] == null) continue;

                if (!string.IsNullOrEmpty(IgnoreTagged))
                    if (SpawnHaveSpecifics(spawns[i], IgnoreTagged, CheckMode))
                    {
                        //if (cell.FlatPos == new Vector2Int(3, 3)) UnityEngine.Debug.Log("ignoring " + spawns[i].Prefab.name);
                        continue;
                    }

                if (spawns[i].IsSpawnCollidable() == false) continue;


                CollisionOffsetData data = new CollisionOffsetData(spawns[i]);
                obstacles.Add(data);

                if (string.IsNullOrEmpty(CheckOnTagCoords) == false)
                {
                    if (SpawnHaveSpecifics(spawns[i], CheckOnTagCoords, CheckModeCoords))
                    {
                        checkOriginOff = Quaternion.Euler(spawns[i].RotationOffset) * CoordsDirOffset;
                        checkOrigin = obstacles[obstacles.Count - 1].positionOffset + checkOriginOff;
                    }
                }
                else
                {
                    checkOriginOff = spawn.TempPositionOffset;
                    checkOrigin = obstacles[obstacles.Count - 1].positionOffset + checkOriginOff;
                }
            }

            //if (cell.ParentCell != null)
            //{
            //    Vector3 cellOffset = new Vector3(cell.FlatPosInt.x - cell.ParentCell.FlatPosInt.x, 0f, cell.FlatPosInt.y - cell.ParentCell.FlatPosInt.y) * preset.CellSize;

            //    for (int i = 0; i < cell.ParentCell.Spawns.Count; i++)
            //    {
            //        if (cell.ParentCell.Spawns[i] == null) continue;
            //        if (cell.ParentCell.Spawns[i].IsSpawnCollidable() == false) continue;

            //        obstacles.Add(new CollisionOffsetData(cell.ParentCell.Spawns[i], cellOffset));

            //        if (string.IsNullOrEmpty(CheckOnTagCoords) == false)
            //            if (SpawnHaveTag(cell.ParentCell.Spawns[i], CheckOnTagCoords))
            //            {
            //                checkOriginOff = Quaternion.Euler(cell.ParentCell.Spawns[i].RotationOffset) * CoordsDirOffset;
            //                checkOrigin = obstacles[obstacles.Count - 1].positionOffset + checkOriginOff;
            //            }
            //    }
            //}


            // Preparing targe spawn bounds
            CollisionOffsetData thisData = new CollisionOffsetData(spawn);

            Bounds refBounds = new Bounds(Vector3.zero, Vector3.one * FreeSpaceNeeded / 1.5f);
            //Bounds refBounds = new Bounds(Vector3.up * (spawn.PreviewMesh.bounds.center.y - spawn.Prefab.transform.localPosition.y), Vector3.one * FreeSpaceNeeded / 1.5f);
            //Bounds refBounds = new Bounds(Vector3.up * FreeSpaceNeeded * 0.75f, Vector3.one * FreeSpaceNeeded / 2f);

            //refBounds.center += Vector3.up * FreeSpaceNeeded * 0.1f;
            refBounds.size = Vector3.Scale(refBounds.size, thisData.prbounds.size);

            if (checkOrigin != null) refBounds.center = new Vector3(checkOrigin.Value.x, refBounds.center.y, checkOrigin.Value.z);


            // Box checking for free space
            int tries = Mathf.RoundToInt(1f / FreeSpaceNeeded);
            float step = .25f / (float)tries;

            refBounds.size *= 1f / (step * 3f);
            refBounds.center += Vector3.up * refBounds.extents.y * 0.52f;
            bool anywhere = true;
            string on = "";

            for (int x = -tries; x <= tries; x++)
            {
                for (int z = -tries; z <= tries; z++)
                {
                    Bounds checkBounds = refBounds;
                    checkBounds.center = new Vector3((float)x * step, checkBounds.center.y, (float)z * step) * preset.CellSize + checkOriginOff * 0.25f;

                    if (Mathf.Abs(checkBounds.min.x) > LimitToCellMargins || Mathf.Abs(checkBounds.max.x) > LimitToCellMargins) continue;
                    if (Mathf.Abs(checkBounds.min.z) > LimitToCellMargins || Mathf.Abs(checkBounds.max.z) > LimitToCellMargins) continue;

                    //float checkDist = Vector3.Distance(checkOrigin == null ? thisData.positionOffset : checkOrigin.Value, new Vector3(checkBounds.center.x, 0, checkBounds.center.z) );
                    //float checkDist = Vector2.Distance(new Vector2(checkBounds.center.x, checkBounds.center.z), new Vector2(thisData.prbounds.center.x, thisData.prbounds.center.z));
                    float checkDist = Vector2.Distance(new Vector2(checkBounds.center.x, checkBounds.center.z), new Vector2(refBounds.center.x, refBounds.center.z));
                    //float checkDist = Vector2.Distance(new Vector2(checkBounds.center.x, checkBounds.center.z), new Vector2(thisData.prbounds.center.x, thisData.prbounds.center.z));

                    if (checkDist < (2f - OnlyAround * 2f) || OnlyAround <= 0f)
                    {
                        if (debug)
                        {
                            Bounds logBounds = checkBounds;
                            logBounds.center += new Vector3(cell.Pos.x, cell.Pos.y, cell.Pos.z) * preset.CellSize;
                            FDebug.DrawBounds3D(logBounds, Color.green * 0.7f);
                        }

                        bool allFree = true;
                        // Checking if in target placement there is no obstacle
                        for (int coll = 0; coll < obstacles.Count; coll++)
                        {
                            if (checkBounds.Intersects(obstacles[coll].prbounds))
                            {
                                allFree = false;
                                on = obstacles[coll].name;
                                //if (cell.FlatPos == new Vector2Int(5, 3))
                                //    if (thisData.name.Contains("drawe"))
                                //        UnityEngine.Debug.Log("On " + on);

                                if (debug)
                                {
                                    Bounds logBounds = obstacles[coll].prbounds;
                                    logBounds.size *= 1.05f;
                                    logBounds.center += new Vector3(cell.Pos.x, cell.Pos.y, cell.Pos.z) * preset.CellSize;
                                    FDebug.DrawBounds3D(logBounds, Color.red * 0.7f);
                                    //UnityEngine.Debug.Log("Intersects " + obstacles[coll].name);
                                    //if (cell.FlatPos == new Vector2Int(3, 2)) UnityEngine.Debug.Log("Intersects " + obstacles[coll].name);
                                }

                                break;
                            }
                            else
                            {
                                Bounds logBounds = obstacles[coll].prbounds;
                                logBounds.size *= 1.05f;
                                logBounds.center += new Vector3(cell.Pos.x, cell.Pos.y, cell.Pos.z) * preset.CellSize;
                                //PlanHelper.DebugBounds3D(logBounds, Color.cyan * 0.7f);

                            }
                        }

                        if (allFree)
                        {
                            Vector3 tgtOff = checkBounds.center;
                            tgtOff.y = thisData.positionOffset.y;
                            tgtOff = Vector3.LerpUnclamped(thisData.positionOffset, tgtOff, MoveToFreePos);

                            if (debug)
                            {
                                Bounds logBounds = checkBounds;
                                logBounds.size *= 1.05f;
                                logBounds.center += new Vector3(cell.Pos.x, cell.Pos.y, cell.Pos.z) * preset.CellSize;
                                FDebug.DrawBounds3D(logBounds, Color.yellow * 0.7f);
                            }

                            freePoses.Add(tgtOff);
                        }
                        else
                        {
                            anywhere = false;
                        }
                    }
                }
            }


            // If any free slot found
            if (freePoses.Count > 0)
            {
                CellAllow = true;
            }
            else
                anywhere = false;

            if (anywhere == false)
            {
                // If we want to move spawn towards free position
                if (MoveToFreePos > 0f)
                {
                    if (freePoses.Count > 0)
                    {
                        Vector3 tgtPos = thisData.positionOffset;

                        if (RandomFreePos == false)
                        {
                            float nearest = float.MaxValue;

                            for (int i = 0; i < freePoses.Count; i++)
                            {
                                float dist = Vector3.Distance(checkOrigin == null ? thisData.positionOffset : checkOrigin.Value, freePoses[i]);
                                if (dist < nearest)
                                {
                                    nearest = dist;
                                    tgtPos = freePoses[i];
                                }
                            }
                        }
                        else
                            tgtPos = freePoses[FGenerators.GetRandom(0, freePoses.Count)];

                        targetPos = tgtPos;
                    }
                }
            }
            else
            {
                targetPos = null;
            }

            //if (cell.FlatPos == new Vector2Int(5, 3))
            //{
            //    if (thisData.name.Contains("drawe"))
            //        UnityEngine.Debug.Log("anywhere =  " + anywhere + thisData.name + "  on " + on);
            //}

        }


        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (targetPos != null)
            {
                spawn.Offset = targetPos.Value;
                spawn.DirectionalOffset = Vector3.zero;
            }
        }


    }
}