using FIMSpace.Generating.Planning;
using FIMSpace.Generating.Checker;
using FIMSpace.Generating.PathFind;
using System;
using System.Collections.Generic;
using UnityEngine;
using FIMSpace.Hidden;

namespace FIMSpace.Generating
{
    public partial class FacilityPlanHelper
    {
        public BuildPlanPreset PlanPreset;
        public PGGPlanGeneratorBase ParentGenerator;
        public bool PrioritizeConnectionWithCorridor = true;
        public bool RandomIteration = true;

        public FacilityPlanHelper(BuildPlanPreset planPreset, PGGPlanGeneratorBase generator)
        {
            PlanPreset = planPreset;
            ParentGenerator = generator;
        }

        bool limited = false;
        Vector2Int XLimit;
        Vector2Int ZLimit;
        public float wallsSeparation = 0f;

        public void SetLimits(Vector2Int xLimit, Vector2Int zLimit)
        {
            limited = true;
            XLimit = xLimit;
            ZLimit = zLimit;
        }


        internal CheckerField limitChecker = null;
        internal BuildPlanInstance rootCorridors = null;
        internal List<BuildPlanInstance> rooms = new List<BuildPlanInstance>();
        internal List<BuildPlanInstance> allRects = new List<BuildPlanInstance>();
        List<SimplePathGuide> guidesUsed = null;


        #region Prepare Section


        /// <summary>
        /// You can set limits for building plan with method SetLimits() called before this metohod
        /// </summary>
        internal void Prepare(List<SimplePathGuide> pathGuides = null)
        {
            finalLog = "";

            #region Preparing limit Rectangle

            if (limited)
            {
                limitChecker = new CheckerField();
                Vector2Int size = new Vector2Int();
                size.x = Mathf.Abs(XLimit.x - XLimit.y);
                size.y = Mathf.Abs(ZLimit.x - ZLimit.y);

                limitChecker.SetSize(size.x, size.y, true);

                Vector2Int center = new Vector2Int();
                center.x = Mathf.RoundToInt(Mathf.Lerp(XLimit.x, XLimit.y, 0.5f));
                center.y = Mathf.RoundToInt(Mathf.Lerp(ZLimit.x, ZLimit.y, 0.5f));
                limitChecker.Position = center + new Vector2Int(1, 0);
            }

            #endregion


            rootCorridors = new BuildPlanInstance(null);
            rootCorridors.SetSettings(PlanPreset.CorridorSetup);
            rootCorridors.Enabled = true;
            allRects.Add(rootCorridors);
            guidesUsed = pathGuides;


            #region Path Guided Corridors from component

            if (pathGuides != null)
                for (int i = 0; i < pathGuides.Count; i++)
                {
                    var guide = pathGuides[i];
                    rootCorridors.Checker.Join(guide.GenerateChecker(false)); // Spread after adding rest of the corridors
                }

            #endregion


            #region Generate Auto Corridors from preset settings

            // Generating initial corridor if not used guides
            if (pathGuides == null || pathGuides.Count == 0)
            {
                var corr = GetCorridor(PlanPreset.RootChunkSetup.InternalSetup.BranchLength.GetRandom(), PlanPreset.RootChunkSetup.InternalSetup.BranchThickness.GetRandom(), true);
                rootCorridors.Checker.Join(corr);
            }

            for (int i = 0; i < PlanPreset.RootChunkSetup.InternalSetup.TargetBranches.GetRandom(); i++)
            {
                var corr = GetCorridor(PlanPreset.RootChunkSetup.InternalSetup.BranchLength.GetRandom(), PlanPreset.RootChunkSetup.InternalSetup.BranchThickness.GetRandom());
                rootCorridors.Checker.Join(corr);
            }

            // Spreading restrictions if any setted
            if (UseRestrictions)
                if (pathGuides != null)
                    for (int i = 0; i < pathGuides.Count; i++)
                        pathGuides[i].SpreadCheckerDataOn(rootCorridors.Checker);

            #endregion


            rootCorridors.Checker.RecalculateMultiBounds();
            rooms.Clear();


            #region Generate Rooms

            // Preparing collection of rooms and removing from this list with randomization
            List<BuildPlanInstance> roomsToCheck = new List<BuildPlanInstance>();

            for (int r = 0; r < PlanPreset.Settings.Count; r++)
            {
                for (int d = 0; d < PlanPreset.Settings[r].Duplicates; d++)
                {
                    BuildPlanInstance ins = new BuildPlanInstance(PlanPreset.Settings[r], false);
                    roomsToCheck.Add(ins);
                    ins.Enabled = false;
                    rooms.Add(ins);
                    if (PrioritizeConnectionWithCorridor) allRects.Add(ins);
                }
            }

            // Few tries to find all needed connections for rooms (handling not founding - very rare case when settings for rooms are kinda wrong)
            for (int safety = 0; safety < 8; safety++)
            {
                int toCheck = roomsToCheck.Count;

                if (RandomIteration)
                {
                    // Placement for rooms on corridors
                    for (int i = 0; i < toCheck; i++)
                    {
                        int inter = FGenerators.GetRandom(0, roomsToCheck.Count);
                        FindRoomPlacement(roomsToCheck[inter]);
                        roomsToCheck[inter].AssignFieldSetupReference(roomsToCheck[inter]);
                        roomsToCheck.RemoveAt(inter);
                    }
                }
                else
                {
                    // Placement for rooms on corridors
                    for (int i = 0; i < toCheck; i++)
                    {
                        FindRoomPlacement(roomsToCheck[i]);
                    }
                }
            }

            #endregion


            // Deliver what's generated to the generator component
            if (ParentGenerator)
            {
                ParentGenerator.GeneratorCheckers = new List<CheckerField>();
                for (int i = 0; i < allRects.Count; i++) ParentGenerator.GeneratorCheckers.Add(allRects[i].Checker);
            }


            if (string.IsNullOrEmpty(finalLog) == false)
                UnityEngine.Debug.Log("PGG Generating LOG: " + finalLog);
        }


        [Range(1, 4)]
        public int Precision = 1;
        public bool UseRestrictions = true;

        string finalLog = "";

        /// <summary>
        /// Finding combination with smallest final bounds
        /// </summary>
        void FindRoomPlacement(BuildPlanInstance room)
        {
            CheckerField sourceState = room.Checker.Copy();
            Bounds smallest = new Bounds(Vector3.zero, Vector3.one * float.MaxValue);
            CheckerField smallestState = room.Checker.Copy();
            int attachedTo = -1;
            int[] rands = new int[4];

            int rots = 2;
            if (Precision > 3) rots = 4;
            int correct = 0;

            // Checking rotated option
            for (int rot = 0; rot < rots; rot++)
            {
                CheckerField inititalState = sourceState.Copy();
                if (rot > 0) inititalState.Rotate(1);

                // On all building plan objects
                for (int r = 0; r < allRects.Count; r++)
                {
                    if (allRects[r] == room) { continue; } // Ignoring self
                    if (allRects[r].Enabled == false) continue; // If rect not placed on plan ignore it
                    if (room.CanConnectWith(allRects[r]) == false) continue; // All desired connections already reached
                    //if (allRects[r].CanConnectWith(room) == false) continue; // All desired connections already reached

                    var randSquares = allRects[r].Checker.GetRandomizedPositionsCopy(); // Copy for randomized placement
                    CheckerField otherChecker = allRects[r].Checker;

                    int incr = 2;
                    if (Precision <= 2) incr = 4;

                    // Align on every square of checked rect field
                    for (int q = 0; q < randSquares.Count; q += incr) // skip 2 to speed up generation (less precise, +=1 -> checking every square)
                    {
                        for (int rnd = 0; rnd < 4; rnd++) rands[rnd] = FGenerators.GetRandom(0, 4); // Random directions - to search randomly then break quicker if found (for precision = 1)

                        // Finding accomodable placement alignign to choosed rect
                        for (int rri = 0; rri < 4; rri++)
                        {

                            if (Precision == 1)
                            {
                                if (attachedTo != -1) break; // Optimization for smaller precision - quicker preview
                            }
                            else if (Precision < 4)
                                if (correct > 5 * Precision) break;


                            int rr = rands[rri];

                            room.Checker = inititalState.Copy();

                            Vector2Int dir = CheckerField.GetDirection(rr);
                            Vector2Int startPos = otherChecker.Position + randSquares[q];
                            room.Checker.Position = startPos;
                            room.Checker.SetAlignNextToPosition(otherChecker, startPos, dir);

                            // Check if room restrictions are met
                            if (UseRestrictions)
                                if (room.SettingsReference.CheckIfRestrictionAllows(room.Checker, allRects, false) == false)
                                {
                                    continue;
                                }

                            if (limited) if (limitChecker.ContainsFully(room.Checker) == false)
                                {
                                    continue;
                                }// Out of bounds limits

                            //if (room.SettingsReference.GetName() != "Treasure Room")
                            if (OverlappingWithOthers(room.Checker))
                            {
                                continue;
                            }// If overlapping with corridor or other rooms

                            // Walls Separation feature
                            if (wallsSeparation != 0) room.Checker.FloatingOffset += dir.V2toV3().normalized * wallsSeparation; else room.Checker.FloatingOffset = Vector3.zero;
                            if (wallsSeparation > 0f) if (WallSeparationOverlappingWithOthers(room.Checker))
                                {
                                    continue;
                                }

                            //if (room.SettingsReference.GetName() != "Treasure Room")
                            if (allRects[r] != rootCorridors) // When it's not main corridor we want connect with many aligning points
                            {
                                if (room.Checker.AlignPointsCount(otherChecker) <= room.Checker.GetSizeOnAxis(dir))
                                {
                                    continue;
                                }
                            }

                            // Checking bounds state for this checker setup
                            room.Enabled = true;
                            Bounds b = CurrentBounds();
                            room.Enabled = false;

                            if (b.size.magnitude <= smallest.size.magnitude)
                            {
                                smallest = b;
                                smallestState = room.Checker.Copy();
                                attachedTo = r;
                                correct++;
                                //if (Precision == 1) break;
                            }

                        }

                    }
                }
            }


            if (attachedTo > -1)
            {
                //FDebug.DrawBounds3D(smallest, Color.red, 2f);
                room.Enabled = true;
                room.Checker = smallestState;

                room.Checker.FloatingOffset += allRects[attachedTo].Checker.FloatingOffset;

                //if (attachedTo != 0) UnityEngine.Debug.Log(room.Settings.CustomName + " Attach to " + attachedTo + " " + allRects[attachedTo].Settings.CustomName);
                // We must define connections before generating
                room.Connections.Add(allRects[attachedTo]);
                room.SpreadDataOn(rootCorridors);
                room.SpreadDataOn(room);

                allRects[attachedTo].Connections.Add(room);
                if (!PrioritizeConnectionWithCorridor) if (allRects.Contains(room) == false) allRects.Add(room);
            }
            else
            {
                if (FGenerators.CheckIfIsNull(room) || FGenerators.CheckIfIsNull(room.SettingsReference))
                    //UnityEngine.Debug.Log("Couldn't find connection for some room");
                    finalLog += ("\nCouldn't find connection for some room");
                else
                    finalLog += ("\nCouldn't find connection for " + room.SettingsReference.GetName() + " (maybe check if restrictions are not too harsh? or generate longer corridors)");
                //UnityEngine.Debug.Log("Couldn't find connection for " + room.SettingsReference.GetName() + " (maybe check if restrictions are not too harsh? or generate longer corridors)");
            }
        }

        public Bounds CurrentBounds()
        {
            Bounds b = rootCorridors.Checker.GetBoundingBox();
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].Enabled)
                    b.Encapsulate(rooms[i].Checker.GetBoundingBox());
            }

            return b;
        }


        bool OverlappingWithOthers(CheckerField checker)
        {
            if (rootCorridors.Checker.CollidesWith(checker)) return true;

            for (int i = 0; i < allRects.Count; i++)
            {
                if (allRects[i].Checker == checker) continue;
                if (allRects[i].Checker.CollidesWith(checker)) return true;
            }

            return false;
        }

        bool WallSeparationOverlappingWithOthers(CheckerField checker)
        {
            // The counter offset directions are ok, can be overlapping with seaparation checks,
            // but the directions to which we are pushing room must be free

            if (checker.FloatingOffset == Vector3.zero) return false;

            List<Vector2Int> offsets = new List<Vector2Int>();
            if (checker.FloatingOffset.x > 0) offsets.Add(new Vector2Int(-1, 0));
            else
            if (checker.FloatingOffset.x < 0) offsets.Add(new Vector2Int(1, 0));
            else
            { offsets.Add(Vector2Int.left); offsets.Add(Vector2Int.right); }

            if (checker.FloatingOffset.y > 0) offsets.Add(new Vector2Int(0, -1));
            else
            if (checker.FloatingOffset.y < 0) offsets.Add(new Vector2Int(0, 1));
            else
            { offsets.Add(Vector2Int.up); offsets.Add(Vector2Int.down); }

            if (offsets.Contains(Vector2Int.up) && offsets.Contains(Vector2Int.right)) offsets.Add(new Vector2Int(1, 1));
            if (offsets.Contains(Vector2Int.down) && offsets.Contains(Vector2Int.left)) offsets.Add(new Vector2Int(-1, -1));

            if (offsets.Contains(Vector2Int.down) && offsets.Contains(Vector2Int.right)) offsets.Add(new Vector2Int(1, -1));
            if (offsets.Contains(Vector2Int.up) && offsets.Contains(Vector2Int.left)) offsets.Add(new Vector2Int(-1, 1));

            for (int o = 0; o < offsets.Count; o++)
            {
                if (rootCorridors.Checker.OffsettedCollidesWith(checker, offsets[o])) return true;

                for (int i = 0; i < allRects.Count; i++)
                {
                    if (allRects[i].Checker == checker) continue;
                    if (allRects[i].Checker.OffsettedCollidesWith(checker, offsets[o])) return true;
                }
            }

            return false;
        }


        public CheckerField GetCorridor(int size, int thickness, bool reset = false, int separation = 3)
        {
            CheckerField checker = new CheckerField();

            if (reset)
            {
                if (FGenerators.GetRandom(0f, 1f) < 0.5f)
                    checker.AddPathTowards(Vector2Int.down * (size / 2), Vector2Int.up * (size / 2), 0.3f, thickness, false);
                else
                    checker.AddPathTowards(Vector2Int.left * (size / 2), Vector2Int.right * (size / 2), 0.3f, thickness, false);

                //checker.RecalculateMultiBounds();
            }
            else
            {
                // Checking every point on checker for alignment
                var positionsToCheck = rootCorridors.Checker.GetRandomizedPositionsCopy();

                for (int pos = 0; pos < positionsToCheck.Count; pos++)
                {
                    Vector2Int getRandom = positionsToCheck[pos];

                    #region Only points far from guides
                    if (guidesUsed != null)
                    {
                        bool isOk = true;

                        for (int g = 0; g < guidesUsed.Count; g++)
                        {
                            //int refDist = Mathf.RoundToInt(Vector2Int.Distance(guidesUsed[g].Start, guidesUsed[g].End) * 0.2f);
                            //if (refDist < 3) refDist = 3;
                            int refDist = separation;

                            if (Vector2Int.Distance(getRandom, guidesUsed[g].Start) < refDist) isOk = false;
                            if (isOk == false) break;
                            if (Vector2Int.Distance(getRandom, guidesUsed[g].End) < refDist) isOk = false;
                            if (isOk == false) break;
                        }

                        if (isOk == false) continue;
                    }
                    #endregion

                    Vector2Int edged = rootCorridors.Checker.GetNearestEdge(getRandom, true);
                    Vector2Int dir = ((Vector2)(edged - getRandom)).normalized.V2toV2Int();

                    bool done = false;
                    for (int side = 0; side < 2; side++)
                    {
                        CheckerField pathCheck = new CheckerField();
                        int sign = side == 0 ? 1 : -1;

                        pathCheck.AddPathTowards(edged, edged + dir * sign * size, 0.3f, thickness, false);
                        //pathCheck.RecalculateMultiBounds();

                        // If out of limit square then ignore
                        if (limited) if (limitChecker.ContainsFully(pathCheck) == false) continue;

                        if (pathCheck.CollidesWith(rootCorridors.Checker)) continue;

                        int distanceLSide = pathCheck.CheckCollisionDistanceInDirection(rootCorridors.Checker, PGGUtils.GetRotatedFlatDirectionFrom(dir));
                        int distanceRSide = pathCheck.CheckCollisionDistanceInDirection(rootCorridors.Checker, PGGUtils.GetRotatedFlatDirectionFrom(dir).Negate());

                        if (distanceLSide == 1 || distanceRSide == 1) continue; // too near

                        checker = pathCheck;
                        if (separation <= 1) { done = true; break; }

                        // Checking separation from other corridors, if bigger then overriding other options
                        if (distanceLSide == -1)
                        {
                            if (distanceRSide >= separation) { done = true; break; }
                        }
                        else if (distanceRSide == -1)
                        {
                            if (distanceLSide >= separation) { done = true; break; }
                        }
                        else
                        {
                            if ((distanceLSide >= separation && distanceRSide >= separation)) { done = true; break; }
                        }
                    }

                    if (done) break;
                }

            }

            return checker;
        }


        #endregion


        internal List<InstantiatedFieldInfo> GenerateObjects(Transform parent)
        {

            #region Warning checks

            if (PlanPreset.RootChunkSetup == null) { UnityEngine.Debug.Log("Corridor Setup in building plan must be set!"); return null; }

            if (PlanPreset.RootChunkSetup.FieldSetup == null)
            {
                UnityEngine.Debug.Log("Corridor Preset in building plan must be set!");
                return null;
            }

            if (PlanPreset.Settings.Count > 0) if (PlanPreset.Settings[0].FieldSetup == null)
                {
                    UnityEngine.Debug.Log("Can't generate because there is no fieldPreset assigned to room setup!");
                    return null;
                }

            #endregion


            // Generating grids out of packed rects
            FacilityGridGeneratingHelper corridorGrid = new FacilityGridGeneratingHelper(null);
            List<FacilityGridGeneratingHelper> roomsGrids = new List<FacilityGridGeneratingHelper>();


            // Setting up corridor
            corridorGrid.fieldInstance = rootCorridors;
            rootCorridors.Checker.InjectToGrid(corridorGrid.grid);

            // Injecting guides data if used
            if (guidesUsed != null)
                for (int g = 0; g < guidesUsed.Count; g++)
                {
                    var guide = guidesUsed[g];
                    guide.InjectStartDataIntoGrid(corridorGrid.grid);
                    guide.InjectEndDataIntoGrid(corridorGrid.grid);
                }


            #region Setting up grids for all rooms in rect

            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].Enabled == false) continue;

                FacilityGridGeneratingHelper room = new FacilityGridGeneratingHelper(rooms[i]);
                rooms[i].Checker.InjectToGrid(room.grid);
                roomsGrids.Add(room);
            }

            #endregion


            #region Generating door connections between rooms


            #region Preparing doors for corridor start and end if used paths

            if (guidesUsed != null)
            {
                // Guides for start and end paths points
                for (int i = 0; i < guidesUsed.Count; i++)
                {
                    // Making hole from start point towards corridor back
                    SpawnInstruction startDoorsGuide = PGGUtils.GenerateInstructionTowards(rootCorridors.Checker,
                        guidesUsed[i].Start, guidesUsed[i].StartDir.GetDirection().V3toV3Int(), guidesUsed[i].GetStartCenterRange());

                    startDoorsGuide.definition = PlanPreset.RootChunkSetup.FieldSetup.CellsInstructions[guidesUsed[i].StartGuideDoorInstruction];
                    corridorGrid.guides.Add(startDoorsGuide);

                    SpawnInstruction endDoorsGuide = PGGUtils.GenerateInstructionTowards(rootCorridors.Checker,
                        guidesUsed[i].End, guidesUsed[i].EndDir.GetDirection().V3toV3Int(), guidesUsed[i].GetEndCenterRange());

                    endDoorsGuide.definition = PlanPreset.RootChunkSetup.FieldSetup.CellsInstructions[guidesUsed[i].EndGuideDoorInstruction];
                    corridorGrid.guides.Add(endDoorsGuide);
                }
            }

            #endregion


            #region Preparing doors for rooms and corridor

            for (int c = 0; c < rootCorridors.Connections.Count; c++)
            {
                FacilityGridGeneratingHelper corridor = corridorGrid;
                var connectedWith = rootCorridors.Connections[c];
                var other = FindGridIn(connectedWith, roomsGrids);

                if (FGenerators.CheckIfIsNull(other)) continue;

                // Generating guide for creating door on corridor side to other room
                SpawnInstruction instr = PGGUtils.GenerateInstructionTowards(corridor.fieldInstance.Checker, other.fieldInstance.Checker, other.fieldInstance.SettingsReference.GetCenterRange());
                instr.definition = PlanPreset.RootChunkSetup.FieldSetup.CellsInstructions[PlanPreset.RootChunkSetup.DoorHoleCommandID];
                corridor.guides.Add(instr);

                // Making hole from other room inside to corridor
                SpawnInstruction pathInstr = PGGUtils.GenerateInstructionTowards(other.fieldInstance.Checker, corridor.fieldInstance.Checker, 0, new Vector2Int(instr.helperCoords.x, instr.helperCoords.z));
                if (other.fieldInstance.SettingsReference.DoorHoleCommandID >= other.fieldInstance.SettingsReference.FieldSetup.CellsInstructions.Count)
                {
                    Debug.Log("[Generating Error] Wrong Door hole command number in " + other.fieldInstance.SettingsReference.GetName() + "! (Id = " + other.fieldInstance.SettingsReference.DoorHoleCommandID + " when " + other.fieldInstance.SettingsReference.FieldSetup.CellsInstructions.Count + " available!)");
                }
                else
                {
                    pathInstr.definition = other.fieldInstance.SettingsReference.FieldSetup.CellsInstructions[other.fieldInstance.SettingsReference.DoorHoleCommandID];
                    other.guides.Add(pathInstr);
                }
            }

            #endregion


            #region Preparing doors for rooms connected with other rooms


            for (int r = 0; r < roomsGrids.Count; r++) // Every room one after another
            {
                FacilityGridGeneratingHelper thisRoom = roomsGrids[r];
                if (thisRoom.fieldInstance.Enabled == false) continue; // If this room is not enabled then ignore it and check next one

                // Proceeding to generate door guide for each room's connection created during building prepare
                for (int i = 0; i < roomsGrids[r].fieldInstance.Connections.Count; i++)
                {
                    BuildPlanInstance connection = roomsGrids[r].fieldInstance.Connections[i];

                    if (connection == rootCorridors) continue; // Ignore corridors

                    // We want to identify generating helper with connection instance
                    FacilityGridGeneratingHelper other = FindGridIn(connection, roomsGrids);

                    if (FGenerators.CheckIfIsNull(other)) continue;

                    if (other.fieldInstance.Enabled == false) continue;

                    // Generating guide for creating door on room side and other room side
                    SpawnInstruction instr = PGGUtils.GenerateInstructionTowards(thisRoom.fieldInstance.Checker, other.fieldInstance.Checker, thisRoom.fieldInstance.SettingsReference.GetCenterRange());
                    instr.definition = thisRoom.fieldInstance.SettingsReference.FieldSetup.CellsInstructions[thisRoom.fieldInstance.SettingsReference.DoorHoleCommandID];
                    thisRoom.guides.Add(instr);

                    // Making hole on other room to this room
                    SpawnInstruction pathInstr = PGGUtils.GenerateInstructionTowards(other.fieldInstance.Checker, thisRoom.fieldInstance.Checker, 0, new Vector2Int(instr.helperCoords.x, instr.helperCoords.z));
                    pathInstr.definition = other.fieldInstance.SettingsReference.FieldSetup.CellsInstructions[other.fieldInstance.SettingsReference.DoorHoleCommandID];
                    other.guides.Add(pathInstr);

                    other.fieldInstance.Connections.Remove(thisRoom.fieldInstance); // Avoiding double doors
                }
            }



            #endregion


            #endregion


            #region Running instantiating

            List<InstantiatedFieldInfo> fieldsGeneration = new List<InstantiatedFieldInfo>();

            // Running FieldSetups and Instantiating all objects
            fieldsGeneration.Add(PlanPreset.RootChunkSetup.GenerateOnGrid(corridorGrid.grid, corridorGrid.guides, parent, Vector3.zero));

            for (int i = 0; i < roomsGrids.Count; i++)
            {
                if (roomsGrids[i].grid.AllApprovedCells.Count <= 0) continue;
                fieldsGeneration.Add(roomsGrids[i].fieldInstance.SettingsReference.GenerateOnGrid(roomsGrids[i].grid, roomsGrids[i].guides, parent, roomsGrids[i].fieldInstance.Checker.FloatingOffset));
            }

            #endregion


            return fieldsGeneration;
        }


        FacilityGridGeneratingHelper FindGridIn(BuildPlanInstance target, List<FacilityGridGeneratingHelper> searchIn)
        {
            for (int i = 0; i < searchIn.Count; i++)
                if (searchIn[i].fieldInstance == target) return searchIn[i];

            return null;
        }


        class FacilityGridGeneratingHelper
        {
            public FGenGraph<FieldCell, FGenPoint> grid;
            public List<SpawnInstruction> guides;
            public BuildPlanInstance fieldInstance;

            public FacilityGridGeneratingHelper(BuildPlanInstance instance)
            {
                grid = IGeneration.GetEmptyFieldGraph();
                guides = new List<SpawnInstruction>();
                fieldInstance = instance;
            }

            public bool HaveFreeConnectionSlots()
            {
                return fieldInstance.HaveFreeConnectionSlots();
            }
        }

    }
}