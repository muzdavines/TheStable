using FIMSpace.Generating.Planning;
using FIMSpace.Generating.Checker;
using FIMSpace.Generating.PathFind;
using System;
using System.Collections.Generic;
using UnityEngine;
using FIMSpace.Hidden;

namespace FIMSpace.Generating
{
    public partial class PlanHelper
    {
        public List<HelperRect> InteriorRects = new List<HelperRect>();
        public List<ConnectionRect> ConnectionRects = new List<ConnectionRect>();

        public BuildPlanPreset PlanPreset;
        public PlanHelper(BuildPlanPreset planPreset)
        {
            PlanPreset = planPreset;
        }

        public bool IsInLimitRange(HelperRect rect, bool justPosition = false)
        {
            if (limited == false) return true;

            if (justPosition)
            {
                if (rect.pos.x < XLimit.x) return false;
                if (rect.pos.x > XLimit.y) return false;
                if (rect.pos.y < ZLimit.x) return false;
                if (rect.pos.y > ZLimit.y) return false;
                return true;
            }

            return IsInLimitRange(rect.Bound);
        }

        public bool IsInLimitRange(Bounds rect)
        {
            if (limited == false) return true;

            if (rect.min.x < XLimit.x) return false;
            if (rect.min.z < ZLimit.x) return false;
            if (rect.max.x > XLimit.y) return false;
            if (rect.max.z > ZLimit.y) return false;

            if (rect.center.x < XLimit.x) return false;
            if (rect.center.x > XLimit.y) return false;
            if (rect.center.y < ZLimit.x) return false;
            if (rect.center.y > ZLimit.y) return false;

            if (rect.center.x - rect.size.x / 2 < XLimit.x) return false;
            if (rect.center.x + rect.size.x / 2 > XLimit.y) return false;
            if (rect.center.y - rect.size.y / 2 < ZLimit.x) return false;
            if (rect.center.y + rect.size.y / 2 > ZLimit.y) return false;

            return true;
        }

        /// <summary>
        /// Generating corridors basing on BuildPlanPreset parameters
        /// </summary>
        public void GenerateCorridors(int count, float wallsSeparation, int additionalLength = 0)
        {
            List<HelperRect> corridors = new List<PlanHelper.HelperRect>();

            #region Getting random start rect if there are used guides

            bool getted = false;
            if (InteriorRects.Count != 0)
            {
                List<HelperRect> rRects = new List<HelperRect>();
                for (int r = 0; r < InteriorRects.Count; r++) if (InteriorRects[r].TypeID == -1) rRects.Add(InteriorRects[r]);

                if (rRects.Count > 0)
                {
                    count++;
                    getted = true;
                    for (int i = 0; i < rRects.Count; i++) corridors.Add(rRects[i]);
                }

                //if (rRects.Count > 0)
                //{
                //    List<HelperRect> iRects = new List<HelperRect>();

                //    if (rRects.Count == 1)
                //        iRects.Add(rRects[0]);
                //    else
                //        for (int r = 0; r < rRects.Count; r++) { int choose = FGenerators.GetRandom(0, rRects.Count); iRects.Add(rRects[choose]); rRects.RemoveAt(choose); }

                //    for (int i = 0; i < iRects.Count; i++)
                //    {
                //        Bounds b = new Bounds(iRects[i].pos, iRects[i].size);
                //        b.size *= 0.7f;

                //        if (IsInLimitRange(b))
                //        {
                //            corridor = iRects[i];
                //            getted = true;
                //            break;
                //        }
                //        else
                //        {
                //        }
                //    }
                //}
            }

            if (!getted)
            {
                HelperRect corridor = new HelperRect();
                corridor.TypeID = -1;
                corridor.size = new Vector2(PlanPreset.RootChunkSetup.InternalSetup.BranchLength.GetRandom() + additionalLength, PlanPreset.RootChunkSetup.InternalSetup.BranchThickness.GetRandom());

                corridor.Connections = new List<ConnectionRect>();
                FindPlaceFor(corridor, wallsSeparation);
                corridors.Add(corridor);
            }


            #endregion

            int generated = 0;
            for (int i = 0; i < count * 32; i++) // max 32 tries
            {
                HelperRect corridor = new HelperRect();
                corridor.Connections = new List<ConnectionRect>();
                corridor.TypeID = -1;
                corridor.rotated = false;
                HelperRect alignTo = corridors[FGenerators.GetRandom(0, corridors.Count)];

                if (alignTo.size.x > alignTo.size.y) // Aligning to horizontal corridor
                {
                    corridor.size = new Vector2Int(PlanPreset.RootChunkSetup.InternalSetup.BranchThickness.GetRandom(), PlanPreset.RootChunkSetup.InternalSetup.BranchLength.GetRandom());
                    float dir = FGenerators.GetRandom(0f, 1f) > 0.5f ? -1f : 1f;

                    corridor.pos = GetAlignNextToPositionRandom(ref corridor, alignTo, dir == 1f ? EAlignDir.Right : EAlignDir.Left);
                    //FDebug.DrawBounds3D(corridor.Bound, Color.red);

                    if (IsInLimitRange(corridor))
                    {
                        //if (IsAnyColliding(corridor)) corridor.pos = GetAlignNextToPositionRandom(ref corridor, alignTo, dir == 1f ? EAlignDir.Left : EAlignDir.Right);

                        if (IsAnyColliding(corridor, wallsSeparation, null) == false)
                        {
                            generated++;
                            corridors.Add(corridor);
                            InteriorRects.Add(corridor);
                        }
                    }
                }
                else // Align to vertical corridor
                {
                    corridor.size = new Vector2Int(PlanPreset.RootChunkSetup.InternalSetup.BranchLength.GetRandom(), PlanPreset.RootChunkSetup.InternalSetup.BranchThickness.GetRandom());
                    float dir = FGenerators.GetRandom(0f, 1f) > 0.5f ? -1f : 1f;

                    corridor.pos = GetAlignNextToPositionRandom(ref corridor, alignTo, dir == 1f ? EAlignDir.Up : EAlignDir.Down);
                    //FDebug.DrawBounds3D(corridor.Bound, Color.blue);

                    if (IsInLimitRange(corridor))
                    {
                        //if (IsAnyColliding(corridor)) corridor.pos = GetAlignNextToPositionRandom(ref corridor, alignTo, dir == 1f ? EAlignDir.Down : EAlignDir.Up);

                        if (IsAnyColliding(corridor, wallsSeparation, null) == false)
                        {
                            generated++;
                            corridors.Add(corridor);
                            InteriorRects.Add(corridor);
                        }
                        else
                        {
                            //UnityEngine.Debug.Log("colliding");
                        }
                    }
                }

                if (generated == count) break;
            }
        }


        /// <summary>
        /// Generating corridors basing on BuildPlanPreset parameters
        /// </summary>
        public void GeneratePathFindedCorridor(Vector2Int start, Vector2Int end, Vector2Int startDir, Vector2Int endDir, int cellSize = 1, float changeDirectionCost = 0.1f)
        {
            List<Bounds> dirChangeBounds = new List<Bounds>();

            Vector3 gridOff = new Vector3(0.0f, 0f, 0.0f);
            Bounds cBounds = new Bounds((start).V2toV3Bound() - new Vector3(0.5f, 0f, 0.5f), (GetDirectionalSize(startDir, cellSize)).V2toV3());
            if (cBounds.size.x % 2 == 0) cBounds.center += new Vector3(0.5f, 0f, 0f);
            if (cBounds.size.z % 2 == 0) cBounds.center -= new Vector3(0.0f, 0f, 0.5f);

            //FDebug.DrawBounds3D(cBounds, Color.blue);

            int maxIters = Mathf.RoundToInt(Vector2Int.Distance(start, end) * 3);
            PathFindHelper[] steps = new PathFindHelper[4];
            Vector2Int position = start;
            Vector2Int currentDir = startDir.Negate();

            // Generating rectangular bounds for corridors going from one point to another
            for (int i = 0; i < maxIters; i++)
            {
                int nearestD = 0;
                float nearestDist = float.MaxValue;

                // Get step resulting in nearest position to target point
                for (int d = 0; d < 4; d++)
                {
                    steps[d] = new PathFindHelper();
                    steps[d].Dir = GetStepDirection(d);
                    steps[d].Distance = Vector2.Distance(position + steps[d].Dir, end);

                    if (steps[d].Dir != currentDir)
                    {
                        steps[d].Distance += changeDirectionCost;
                    }

                    if (steps[d].Distance < nearestDist)
                    {
                        nearestDist = steps[d].Distance;
                        nearestD = d;
                    }
                }

                PathFindHelper pfNearest = steps[nearestD];

                Bounds newB = new Bounds((position + pfNearest.Dir).V2toV3Bound() - gridOff, (GetDirectionalSize(pfNearest.Dir, cellSize)).V2toV3());
                if (newB.size.x % 2 == 0) newB.center += new Vector3(0.5f, 0f, 0f);
                if (newB.size.z % 2 == 0) newB.center -= new Vector3(0.0f, 0f, 0.5f);

                if (currentDir != pfNearest.Dir) // Direction change occured
                {
                    dirChangeBounds.Add(cBounds);
                    cBounds = new Bounds(newB.center, newB.size);
                }
                else
                {
                    cBounds.Encapsulate(newB);
                    //FDebug.DrawBounds3D(newB, Color.yellow);
                }

                position += pfNearest.Dir;
                currentDir = pfNearest.Dir;

                if (position == end)
                {
                    dirChangeBounds.Add(cBounds);
                    break;
                }
            }

            // Generating interior rects from bounds
            for (int i = 0; i < dirChangeBounds.Count; i++)
            {
                HelperRect pathRect = new HelperRect();

                pathRect.TypeID = -1;
                pathRect.Connections = new List<ConnectionRect>();
                pathRect.EncapsateInto(dirChangeBounds[i]);

                InteriorRects.Add(pathRect); // Adding to building plan (ID = -1 -> Corridor)
            }

        }

        public Vector2 GetDirectionalSize(Vector2Int dir, int cellsSize)
        {
            if (cellsSize <= 1) return Vector2.one;
            if (dir.x != 0) return new Vector2(1, cellsSize);
            else return new Vector2(cellsSize, 1);
        }

        struct PathFindHelper
        {
            public Vector2Int Dir;
            public float Distance;
        }

        Vector2Int GetStepDirection(int iter)
        {
            if (iter == 0) return new Vector2Int(1, 0);
            else if (iter == 1) return new Vector2Int(0, 1);
            else if (iter == 2) return new Vector2Int(-1, 0);
            else return new Vector2Int(0, -1);
        }

        public enum EGenerationResult { Unknown, Success, ShallowRetry, HardRetry, Failed }
        public EGenerationResult GenerateRooms(float wallsSeparation)
        {
            if (PlanPreset == null) return EGenerationResult.Failed;
            if (PlanPreset.Settings == null) return EGenerationResult.Failed;

            List<HelperRect> roomsToCheck = new List<HelperRect>();
            List<HelperRect> notConnected = new List<HelperRect>();

            for (int i = 0; i < PlanPreset.Settings.Count; i++)
            {
                for (int r = 0; r < PlanPreset.Settings[i].Duplicates; r++)
                {
                    roomsToCheck.Add(new HelperRect(PlanPreset.Settings[i], i, r));
                }
            }

            int safetyTries = 0;
            int shallowTries = 0;
            bool error = false;

            // Few tries to find all needed connections for rooms (handling not founding - very rare case when settings for rooms are kinda wrong)
            for (int safety = 0; safety < 8; safety++)
            {

                // Simple check two times (handling not founding - very rare case when settings for rooms are kinda wrong)
                for (int shallowCheck = 0; shallowCheck < 2; shallowCheck++)
                {
                    int toCheck = roomsToCheck.Count;
                    notConnected.Clear();

                    // Placement for rooms on corridors
                    for (int i = 0; i < toCheck; i++)
                    {
                        int inter = FGenerators.GetRandom(0, roomsToCheck.Count);

                        roomsToCheck[inter].ResetSeparationOffset();

                        var adding = FindPlaceFor(roomsToCheck[inter], wallsSeparation);

                        roomsToCheck.RemoveAt(inter);

                        if (adding.HelperBool == false) notConnected.Add(adding);
                    }

                    //if (notConnected.Count > 0) UnityEngine.Debug.Log("Not Connected " + notConnected.Count);
                    if (notConnected.Count == 0) { /*if (shallowCheck != 0) UnityEngine.Debug.Log("All ok at " + shallowCheck);*/ break; }

                    for (int i = 0; i < notConnected.Count; i++) roomsToCheck.Add(notConnected[i]);

                    shallowTries++;
                }


                #region Handling exception situations when could not found placement for all rooms


                if (notConnected.Count > 0) // Could not find connection so we generating another corridor and connect rooms with it
                {
                    // Choosing random corridor connection and removing it, replacing with corridor branch
                    HelperRect toReplace = new HelperRect();
                    toReplace.TypeID = -1;

                    for (int i = 0; i < InteriorRects.Count; i++)
                    {
                        var refRect = InteriorRects[i];
                        if (refRect.TypeID == -1)
                        {
                            if (refRect.Connections != null)
                                if (refRect.Connections.Count > 0)
                                    if (refRect.Connections[0].Connection1.TypeID != -1)
                                    {
                                        toReplace = refRect.Connections[0].Connection1;
                                    }
                                    else
                                    {
                                        toReplace = refRect.Connections[0].Connection2;
                                    }
                        }

                        if (toReplace.TypeID != -1) { break; }
                    }

                    if (toReplace.TypeID == -1) // If could not find connection to remove in corridors we generating new corridor connected to one of the rooms
                    {
                    }
                    else
                    {
                        RemoveRectWithIDAndAllConnectedChildren(toReplace.TypeID);
                        roomsToCheck.Add(toReplace);
                        GenerateCorridors(1, 2 + safety);
                        //wrongFound = true;
                    }
                }
                else
                {
                    if (safety > 0) UnityEngine.Debug.Log("Safety break for building plan generation! Nr:" + safety);
                    error = true;
                    break;
                }

                #endregion

                safetyTries++;
            }



            // Searching for target additional room connections
            for (int i = 0; i < InteriorRects.Count; i++) // Checking all room presets
            {
                var rect = InteriorRects[i];
                if (rect.TypeID == -1) continue;
                if (rect.CanConnectTo() == false) continue;

                int tgtConnections = PlanPreset.Settings[rect.TypeID].DoorConnectionsCount.GetRandom();

                if (tgtConnections > 0)
                    for (int c = 0; c < tgtConnections - 1; c++) // Trying to find connection c-times
                    {
                        for (int t = 0; t < InteriorRects.Count; t++) // Check target connection rect
                        {
                            var tgtRect = InteriorRects[t];
                            if (tgtRect.CanConnectTo() == false) continue;
                            if (rect.HaveConnectionWithCorridor() == true) if (tgtRect.TypeID == -1) continue;
                            if (i == t) continue;

                            var conn = GetConnectionRect(InteriorRects[i], tgtRect);

                            if (conn.Found)
                            {
                                if (rect.HaveConnectionWith(tgtRect) == false)
                                {
                                    rect.Connections.Add(conn);
                                    tgtRect.Connections.Add(conn);
                                    conn.Id = ConnectionRects.Count;
                                    ConnectionRects.Add(conn);
                                    //UnityEngine.Debug.Log("New connections " + rect.TypeID + " and " + tgtRect.TypeID);
                                    break;
                                }
                            }
                        }
                    }

            }



            #region Connecting with farthest doors algorithm test

            // Re-Aligning all room connections
            //if (freePoses.Count > 1) // Getting farthest to other connections slot in algorithm below!
            //{
            //    int poseNearestBiggsetSpace = 0;
            //    float nearestBiggestSpace = 0;

            //    for (int p = 0; p < freePoses.Count; p++) // Checking all poses and searching for one which nearest distance to other connection is bigger than others
            //    {
            //        float nearest = float.MaxValue;

            //        for (int i = 0; i < ConnectionRects.Count; i++) // Checking all connection rects to find if any of them is too near target position
            //        {
            //            float dist = Vector2.Distance(freePoses[p], ConnectionRects[i].pos);
            //            if (dist < nearest) nearest = dist;
            //        }

            //        if (nearest > nearestBiggestSpace) // Finding connection slot which is farthest from other connections
            //        {
            //            poseNearestBiggsetSpace = p;
            //            nearestBiggestSpace = nearest;
            //        }
            //    }

            //    tgt = freePoses[poseNearestBiggsetSpace];
            //}

            #endregion



            #region Detecting walls relation for each room and preparing restrictions

            for (int i = 0; i < InteriorRects.Count; i++)
            {
                var inter = InteriorRects[i];
                var cells = inter.GenerateGraphCells(true);

                //inter.nears = PlanPreset.GetDefinition(PlanPreset.NeightbourWallsCellsRestrictions, PlanPreset.Settings[inter.ID]);
                //inter.counters = PlanPreset.GetDefinition(PlanPreset.CounterWallsCellsRestrictions, PlanPreset.Settings[inter.ID]);
                //inter.outsides = PlanPreset.GetDefinition(PlanPreset.OutsideWallsCellsRestrictions, PlanPreset.Settings[inter.ID]);

                inter.nears = new SpawnRestrictionsGroup(PlanPreset.NeightbourWallsCellsRestrictions);
                inter.counters = new SpawnRestrictionsGroup(PlanPreset.CounterWallsCellsRestrictions);
                inter.outsides = new SpawnRestrictionsGroup(PlanPreset.OutsideWallsCellsRestrictions);

                InteriorRects[i] = inter;

                if (InteriorRects[i].nears.Restriction.IsRestricting() == false &&
                    inter.counters.Restriction.IsRestricting() == false &&
                    inter.outsides.Restriction.IsRestricting() == false)
                {
                    break;
                }

                //if (InteriorRects[i].nears.Type == SpawnRestrictions.ERestrictionType.None &&
                //    inter.counters.Type == SpawnRestrictions.ERestrictionType.None &&
                //    inter.outsides.Type == SpawnRestrictions.ERestrictionType.None)
                //    break;

                for (int c = 0; c < cells.Count; c++)
                {
                    var cell = cells[c];

                    // Check if cell is edge cell
                    if (cell.neightbours.IsSideEdge() == false) { continue; }

                    if (cell.neightbours.r == false) { CheckRestrictionalNeightbours(ref inter, cell, SpawnData.ESpawnMark.Right); }
                    if (cell.neightbours.l == false) { CheckRestrictionalNeightbours(ref inter, cell, SpawnData.ESpawnMark.Left); }
                    if (cell.neightbours.u == false) { CheckRestrictionalNeightbours(ref inter, cell, SpawnData.ESpawnMark.Forward); }
                    if (cell.neightbours.d == false) { CheckRestrictionalNeightbours(ref inter, cell, SpawnData.ESpawnMark.Back); }
                }

                InteriorRects[i] = inter;
            }

            #endregion


            #region Debug restrictions draw

            //float _drawSz = 2f;
            //Vector3 off = new Vector3(_drawSz / 2f, 0f, _drawSz / 2f);
            //for (int i = 0; i < InteriorRects.Count; i++)
            //{
            //    if (InteriorRects[i].nears.Restriction.CustomDefinition.InstructionType != SpawnRestrictions.ERestrictionType.None)
            //        for (int nc = 0; nc < InteriorRects[i].nears.Cells.Count; nc++)
            //        {
            //            var r = InteriorRects[i].nears.Cells[nc];
            //            Vector3 dir = SpawnData.GetPlacementDirection(r.placement);
            //            Vector3 strt = new Vector3(r.cellPosition.x, 0f, r.cellPosition.y) * _drawSz + off;
            //            Debug.DrawRay(strt, Vector3.up, Color.yellow * 0.6f, 1.1f);
            //            Debug.DrawRay(strt, dir * 0.8f, Color.yellow, 1.1f);
            //        }

            //    if (InteriorRects[i].counters.Type != SpawnRestrictions.ERestrictionType.None)
            //        for (int nc = 0; nc < InteriorRects[i].counters.Cells.Count; nc++)
            //        {
            //            var r = InteriorRects[i].counters.Cells[nc];
            //            Vector3 dir = SpawnData.GetPlacementDirection(r.placement);
            //            Vector3 strt = new Vector3(r.cellPosition.x, 0f, r.cellPosition.y) * _drawSz + off;
            //            Debug.DrawRay(strt, Vector3.up, Color.magenta * 0.6f, 1.1f);
            //            Debug.DrawRay(strt, dir * 0.8f, Color.magenta, 1.1f);
            //        }


            //if (InteriorRects[i].outsides.Type != SpawnRestrictions.ERestrictionType.None)
            //    for (int nc = 0; nc < InteriorRects[i].outsides.Cells.Count; nc++)
            //    {
            //        var r = InteriorRects[i].outsides.Cells[nc];
            //        Vector3 dir = SpawnData.GetPlacementDirection(r.placement);
            //        Vector3 strt = new Vector3(r.cellPosition.x, 0f, r.cellPosition.y) * _drawSz + off;
            //        Debug.DrawRay(strt, Vector3.up, Color.black * 0.6f, 1.1f);
            //        Debug.DrawRay(strt, dir * 0.8f, Color.black, 1.1f);
            //    }

            //}

            #endregion



            if (error) return EGenerationResult.Failed;

            if (safetyTries == 0 && shallowTries == 0)
                return EGenerationResult.Success;
            else
            {
                if (safetyTries > 0) return EGenerationResult.HardRetry;
                else
                    return EGenerationResult.ShallowRetry;
            }

        }

        public void CheckRestrictionalNeightbours(ref HelperRect inter, FieldCell cell, SpawnData.ESpawnMark direction)
        {
            if (FGenerators.CheckIfIsNull(cell))
            {
                return;
            }

            // Preparing check bounds
            Bounds cellB = cell.ToBounds(1f, inter.separationOffset);
            cellB.size *= 0.99f;

            Vector3 dir = SpawnData.GetPlacementDirection(direction);

            float centerOff = 0f;// PlanPreset.WallsSeparation / 2f;
            Bounds checkBR = cellB;
            checkBR.size *= (0.55f + centerOff / 4f);
            checkBR.center += dir * (0.5f);

            Bounds checkBRMid = cellB;
            checkBRMid.size *= 0.55f;
            checkBRMid.center += dir * (1.5f + centerOff);

            Bounds checkBRFar = cellB;
            checkBRFar.center += dir * (50);
            if (direction == SpawnData.ESpawnMark.Left || direction == SpawnData.ESpawnMark.Right)
                checkBRFar.size = new Vector3(checkBRFar.size.x * 100f, checkBRFar.size.y, checkBRFar.size.z * 0.5f);
            else
                checkBRFar.size = new Vector3(checkBRFar.size.x * 0.5f, checkBRFar.size.y, checkBRFar.size.z * 100f);


            bool detected = false;
            // Checking if colliding with other interior in very small distance on each side
            for (int itr = 0; itr < InteriorRects.Count; itr++)
            {
                var inR = InteriorRects[itr]; if (inR.IndividualID == inter.IndividualID) if ( inR.DuplicateID == inter.DuplicateID ) continue; // Ignoring self interior

                //Debug.DrawRay(checkBR.center * 1f + Vector3.up * 0.1f, Vector3.up * 0.5f, Color.cyan * 0.5f, 1.1f);
                //Debug.DrawRay(checkBR.center * 1f + Vector3.up * 0.1f, Vector3.forward, Color.cyan, 1.1f);

                //FDebug.DrawBounds2D(checkBR, new Color(1f, 1f, 1f, 0.4f));
                //FDebug.DrawBounds2D(new Bounds(inR.GridBound.center, inR.GridBound.size), new Color(1f, 1f, 1f, 0.4f));

                // Checking if current cell collides with some interior on it's right in small distance
                if (checkBR.Intersects(inR.GridBound))
                {
                    //nears.Add(new RestrictionCoords(inter.ID, cell, direction));
                    inter.nears.Cells.Add(new SpawnInstructionGuide() { pos = cell.Pos, rot = SpawnData.GetPlacementRotation(direction) });
                    //inter.nears.Cells.Add(new SpawnCoords() { cellPosition = cell.PosXZ, placement = direction });
                    detected = true;
                }
            }

            if (detected == false) // Checking further 
            {
                bool selfColl = false;

                // First check if colliding with self
                for (int itr = 0; itr < InteriorRects.Count; itr++)
                {
                    var inR = InteriorRects[itr];
                    if (HelperRect.IsEqual(InteriorRects[itr], inter))
                        if (checkBRMid.Intersects(inR.Bound))
                        { selfColl = true; break; }
                }

                if (selfColl == false)
                {
                    // Then check others
                    for (int itr = 0; itr < InteriorRects.Count; itr++)
                    {
                        var inR = InteriorRects[itr]; if (HelperRect.IsEqual(inR, inter)) continue; // Ignoring self interior
                        if (checkBRMid.Intersects(inR.Bound))
                        {
                            if (inR.IndividualID == inter.IndividualID) if (inR.DuplicateID == inter.DuplicateID) break;
                            //counters.Add(new RestrictionCoords(inter.ID, cell, direction));
                            inter.counters.Cells.Add(new SpawnInstructionGuide() { pos = cell.Pos, rot = SpawnData.GetPlacementRotation(direction) });
                            //inter.counters.Cells.Add(new SpawnCoords() { cellPosition = cell.PosXZ, placement = direction });
                            detected = true;
                        }
                    }
                }
            }

            if (detected == false) // Checking further 
            {
                bool allNotColl = true;

                for (int itr = 0; itr < InteriorRects.Count; itr++)
                {
                    var inR = InteriorRects[itr];
                    if (inR.IndividualID == inter.IndividualID) continue; // Ignoring self interior
                    if (checkBRFar.Intersects(inR.Bound)) { allNotColl = false; break; }
                }

                if (allNotColl)
                {
                    //outsides.Add(new RestrictionCoords(inter.ID, cell, direction));
                    inter.outsides.Cells.Add(new SpawnInstructionGuide() { pos = cell.Pos, rot = SpawnData.GetPlacementRotation(direction) });
                    //inter.outsides.Cells.Add(new SpawnCoords() { cellPosition = cell.PosXZ, placement = direction });
                }
            }


            //UnityEngine.Debug.Log(inter.IndividualID + ":" + inter.TypeID + "   nears " + inter.nears.Cells.Count + " counteres " + inter.counters.Cells.Count + " outs " + inter.outsides.Cells.Count);
        }


    }
}