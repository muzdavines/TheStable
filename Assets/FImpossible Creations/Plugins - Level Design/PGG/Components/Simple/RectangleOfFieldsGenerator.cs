using FIMSpace.Generating.Checker;
using FIMSpace.Generating.RectOfFields;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif  
using UnityEngine;

namespace FIMSpace.Generating
{
    [AddComponentMenu("FImpossible Creations/PGG/Rectangle of Fields Generator", 103)]
    public class RectangleOfFieldsGenerator : PGGGeneratorBase
    {
        public bool RefreshEveryChange = true;
        [Space(6)]
        public Vector2Int PackingRectSize = new Vector2Int(10, 8);
        [Range(0, 6)] public int AdditionalSpacing = 0;
        public MinMax MinSingleRectSizeX = new MinMax(4, 6);
        public MinMax MaxSingleRectSizeY = new MinMax(4, 6);
        public MinMax MinMaxSizeOfFillRects = new MinMax(4, 12);
        [Tooltip("Useful when generating exterior walls and want to clear rooms walls on edges or so")]
        public int InstructionIdOnRectEdges = -1;
        public int InstructionIdOnRectNeightbours = -1;

        [Header("Corridor Setup is optional")]
        public bool UseCorridorGuide = false;
        public MinMax LimitDoorsCount = new MinMax(1, 3);
        public FieldSetup CorridorPreset;
        public PathFind.SimplePathGuide CorridorGuide;

        [Header("You must select at least one field preset")]
        public List<FieldOfRect> RoomPresets = new List<FieldOfRect>();
        [Header("Optional always in same place - Useful for example for elevator")]
        public List<FieldOfRectStatic> StaticRooms = new List<FieldOfRectStatic>();

        private RectOfFieldsInstance mainCorridorInstance = null;
        private List<RectOfFieldsInstance> rInstances = new List<RectOfFieldsInstance>();
        private List<RectOfFieldsInstance> rStatic = new List<RectOfFieldsInstance>();
        private List<RectOfFieldsInstance> rAll = new List<RectOfFieldsInstance>();

        private CheckerField fullRectCheck = new CheckerField();

        [HideInInspector] public int FromMainCorridorToRoomsGuideId = 0;
        [Space(5)]
        [HideInInspector] public int FromRoomsToCorridorsGuideId = 0;
        [HideInInspector] public int FromRoomsToRoomsGuideId = 0;
        [HideInInspector] public int FromRoomToAlreadyConnectedRoomGuideId = 0;

        public override FGenGraph<FieldCell, FGenPoint> PGG_Grid { get { return null; } }
        public override FieldSetup PGG_Setup { get { return null; } }

        public override void Prepare()
        {
            base.Prepare();

            rAll.Clear();
            rStatic.Clear();
            rInstances.Clear();


            #region Generating Main Corridor

            if (UseCorridorGuide)
            {
                mainCorridorInstance = new RectOfFieldsInstance(); // Null -> that means it's corridor
                mainCorridorInstance.Checker.Position = CorridorGuide.Start;

                var mainCorrSetup = CheckerField.GeneratePathFindPointsFromStartToEnd(CorridorGuide);
                mainCorridorInstance.Checker.AddPathTowards(mainCorrSetup[0], CorridorGuide.PathThickness);
                //mainCorridorInstance.Checker.RecalculateMultiBounds();
                mainCorridorInstance.Checker.Position -= new Vector2Int(1, 1);
                mainCorridorInstance.Setted = true;

                for (int i = mainCorridorInstance.Checker.ChildPos.Count - 1; i >= 0; i--)
                {
                    Vector2Int pos = mainCorridorInstance.Checker.WorldPos(i);
                    if (pos.x < -PackingRectSize.x) mainCorridorInstance.Checker.Remove(pos);
                    else
                    if (pos.x >= PackingRectSize.x) mainCorridorInstance.Checker.Remove(pos);
                    else
                    if (pos.y < -PackingRectSize.y - 1) mainCorridorInstance.Checker.Remove(pos);
                    else
                    if (pos.y > PackingRectSize.y - 2) mainCorridorInstance.Checker.Remove(pos);
                }

                rAll.Add(mainCorridorInstance);
            }

            #endregion

            #region Placing static rects

            for (int i = 0; i < StaticRooms.Count; i++)
            {
                RectOfFieldsInstance ins = new RectOfFieldsInstance();
                ins.FieldRefStatic = StaticRooms[i];

                ins.Checker.SetSize(StaticRooms[i].Size);

                ins.Checker.Position = StaticRooms[i].StaticPosition;
                ins.Setted = true;
                rStatic.Add(ins);
                rAll.Add(ins);
                if (UseCorridorGuide) mainCorridorInstance.Checker.RemoveOnesCollidingWith(ins.Checker);
            }

            #endregion

            #region Packing fields in rect

            // First calculate what rects we use
            // We shatter desired box onto small pieces
            fullRectCheck = new CheckerField();
            fullRectCheck.SetSize(PackingRectSize.x * 2, PackingRectSize.y * 2, true);
            fullRectCheck.Position += new Vector2Int(0, -1);

            // Clearing place for the corridor
            if (mainCorridorInstance != null) fullRectCheck.RemoveOnesCollidingWith(mainCorridorInstance.Checker);
            // Clearing place for the static rooms
            for (int i = 0; i < rStatic.Count; i++) fullRectCheck.RemoveOnesCollidingWith(rStatic[i].Checker);

            int toFill = fullRectCheck.ChildPos.Count;

            // Generating until packing shape size minus corridor is reached
            for (int i = 0; i < 1000; i++)
            {
                RectOfFieldsInstance shape = new RectOfFieldsInstance();
                shape.Checker.SetSize(MinSingleRectSizeX.GetRandom(), MaxSingleRectSizeY.GetRandom(), false);
                shape.Checker.Position += new Vector2Int(PackingRectSize.x * 10, 0);
                toFill -= shape.Checker.ChildPos.Count;
                rInstances.Add(shape);
                rAll.Add(shape);
                if (toFill <= 0) break;
            }

            int positionX = -PackingRectSize.x;
            int positionY = -PackingRectSize.y - 1;
            int lim = 0;

            // Placing shapes in packing rect starting from left down
            for (int i = 0; i < rInstances.Count; i++)
            {
                var ins = rInstances[i];
                ins.Checker.Position = new Vector2Int(positionX, positionY);

                var coll = CheckIfAnyCollides(ins.Checker);

                if (FGenerators.CheckIfIsNull(coll ))
                {
                    ins.Setted = true;
                }
                else
                {
                    ins.Checker.Position += new Vector2Int(coll.Position.x - ins.Checker.Position.x, ins.Checker.LastSettedSize.y * 2);
                    ins.Checker.SnapToOther(coll);
                    if (FGenerators.CheckIfIsNull(CheckIfAnyCollides(ins.Checker) )) ins.Setted = true;
                }

                if (ins.Setted)
                {
                    if (ins.Checker.GetBoundsMax().x >= PackingRectSize.x) ins.Setted = false;
                    if (ins.Checker.GetBoundsMax().y >= PackingRectSize.y - 1) ins.Setted = false;
                }

                if (ins.Setted == false) ins.Checker.PushPositionX(PackingRectSize.x * 3);

                if (ins.Setted)
                {
                    positionX = ins.Checker.Position.x;
                    positionX += ins.Checker.LastSettedSize.x / 2 + AdditionalSpacing;
                }
                else
                {
                    if (lim < 2) // Allowing to try placing box few times
                    {
                        lim++;
                        i--;
                    }
                    else
                        lim = 0;
                }

                // Always forwarding a bit
                positionX += ins.Checker.LastSettedSize.x / 2;

                if (positionX >= PackingRectSize.x)
                {
                    positionX = -PackingRectSize.x;
                    positionY += ins.Checker.GetSizeOnAxis(Vector2.up);
                }
            }

            // After filling we generate inverse fillment rect
            fullRectCheck = new CheckerField();
            fullRectCheck.SetSize(PackingRectSize.x * 2, PackingRectSize.y * 2, true);
            //fullRectCheck.Position += new Vector2Int(0, -1);
            fullRectCheck.PushAllChildPositions(new Vector2Int(0, -1));
            for (int i = 0; i < rAll.Count; i++) fullRectCheck.RemoveOnesCollidingWith(rAll[i].Checker);

            #endregion


            #region Now we create rects out of free space to fill it completely

            // Generating until full packing using fullRectCheck
            for (int i = 0; i < 1000; i++)
            {
                int size = MinMaxSizeOfFillRects.GetRandom();
                List<Vector2Int> nRect = fullRectCheck.FindConnectedShapeOfSize(size);

                if (nRect.Count > 0)
                    if (nRect.Count >= MinMaxSizeOfFillRects.Min)
                    {
                        RectOfFieldsInstance fillShape = new RectOfFieldsInstance(); //RoomPresets[0]
                        fillShape.Checker.Position = nRect[0];
                        fillShape.Checker.AddPositions(nRect);
                        fillShape.Setted = true;
                        fullRectCheck.RemoveOnesCollidingWith(fillShape.Checker); // Removing reverse fillment squares of this shape
                        rInstances.Add(fillShape);
                    }

                if (fullRectCheck.ChildPos.Count == 0) break;
            }

            #endregion


            #region Joining smallest rects with bigger ones

            // First joining left rects by inverse selection
            for (int i = 0; i < fullRectCheck.ChildPos.Count; i++)
            {
                CheckerField ch = FindAligningTo(fullRectCheck.ChildPosition(i));
                if (ch != null) ch.Add(fullRectCheck.ChildPosition(i));
            }

            #endregion

        }


        CheckerField CheckIfAnyCollides(CheckerField checker)
        {
            for (int i = 0; i < rAll.Count; i++)
            {
                if (rAll[i].Setted == false) continue;
                if (checker == rAll[i].Checker) continue;
                if (checker.CollidesWith(rAll[i].Checker)) return rAll[i].Checker;
            }

            return null;
        }

        CheckerField FindAligningTo(Vector2Int square)
        {
            for (int i = 0; i < rInstances.Count; i++)
            {
                if (rInstances[i].Checker.IsAligning(square))
                {
                    return rInstances[i].Checker;
                }
            }

            return null;
        }


        public override void GenerateObjects()
        {

            Prepare(); // Preparing packed rooms scheme

            // Generating grids out of packed rects
            RectOfGeneratingHelper corridorGrid = new RectOfGeneratingHelper(null);
            List<RectOfGeneratingHelper> roomsGrids = new List<RectOfGeneratingHelper>();
            List<RectOfGeneratingHelper> staticRoomGrids = new List<RectOfGeneratingHelper>();


            #region Setting up corridor grid if used

            if (UseCorridorGuide)
            {
                corridorGrid.fieldInstance = mainCorridorInstance;
                mainCorridorInstance.Checker.InjectToGrid(corridorGrid.grid);
            }

            #endregion


            #region Setting up grids for all rooms in rect

            for (int i = 0; i < rInstances.Count; i++)
            {
                if (rInstances[i].Setted == false) continue;

                RectOfGeneratingHelper room = new RectOfGeneratingHelper(rInstances[i]);
                rInstances[i].Checker.InjectToGrid(room.grid);
                roomsGrids.Add(room);
            }

            #endregion


            #region Setting up grids for static rooms

            for (int i = 0; i < rStatic.Count; i++)
            {
                RectOfGeneratingHelper room = new RectOfGeneratingHelper(rStatic[i]);
                room.inject = rStatic[i].FieldRefStatic.Injections;
                rStatic[i].Checker.InjectToGrid(room.grid);
                staticRoomGrids.Add(room);
            }

            #endregion


            #region Choosing most fitting presets settings for rooms

            List<RectOfFieldsInstance> notAssigned = new List<RectOfFieldsInstance>();
            for (int p = 0; p < RoomPresets.Count; p++) RoomPresets[p].Refresh();

            List<FieldOfRect> randomUnlimited = new List<FieldOfRect>();
            for (int p = 0; p < RoomPresets.Count; p++) if (RoomPresets[p].MaxCountOfThisRoom <= 0) randomUnlimited.Add(RoomPresets[p]);

            for (int i = 0; i < rInstances.Count; i++)
            {
                RectOfFieldsInstance room = rInstances[i];

                for (int p = 0; p < RoomPresets.Count; p++)
                {
                    var preset = RoomPresets[p];
                    if (preset.MaxCountOfThisRoom > 0) if (preset.AlreadyGenerated >= preset.MaxCountOfThisRoom) continue; // already used all

                    int roomSize = room.Checker.CountSize();
                    if (roomSize > preset.DesiredSizeInCells.Min && roomSize < preset.DesiredSizeInCells.Max)
                    {
                        room.FieldRef = preset;
                        preset.AlreadyGenerated++;
                        break;
                    }
                }

                if (room.FieldRef == null || room.FieldRef.Preset == null) notAssigned.Add(room);
            }

            if (randomUnlimited.Count == 0)
                if (RoomPresets.Count > 0)
                    randomUnlimited.Add(RoomPresets[0]);

            if (randomUnlimited.Count == 0)
            {
                for (int i = 0; i < notAssigned.Count; i++)
                {
                    if (RoomPresets.Count > 0)
                        notAssigned[i].FieldRef = RoomPresets[0];
                }
            }
            else
            {
                for (int i = 0; i < notAssigned.Count; i++)
                {
                    notAssigned[i].FieldRef = randomUnlimited[Random.Range(0, randomUnlimited.Count)];
                }
            }

            for (int i = 0; i < roomsGrids.Count; i++)
            {
                if (roomsGrids[i].fieldInstance.FieldRef == null) continue;
                roomsGrids[i].inject = roomsGrids[i].fieldInstance.FieldRef.Injections;
            }

            #endregion


            #region Additional Guides if wanted

            if (InstructionIdOnRectEdges >= 0)
            {
                CheckerField boundChecker = new CheckerField();
                boundChecker.SetSize(PackingRectSize.x * 2, PackingRectSize.y * 2, true);
                boundChecker.Position += (new Vector2Int(0, -1));
                //boundChecker.PushAllChildPositions(new Vector2Int(0, -1));

                List<Vector2Int> edges = boundChecker.GetEdgePositions();
                for (int i = 0; i < roomsGrids.Count; i++)
                    InjectBuildupInstructions(roomsGrids[i], boundChecker, edges, roomsGrids[i].fieldInstance.FieldRef.Preset);

                for (int i = 0; i < staticRoomGrids.Count; i++)
                    InjectBuildupInstructions(staticRoomGrids[i], boundChecker, edges, staticRoomGrids[i].fieldInstance.FieldRefStatic.Preset);

                corridorGrid.fieldInstance = mainCorridorInstance;
                InjectBuildupInstructions(corridorGrid, boundChecker, edges, CorridorPreset);
            }

            #endregion


            #region Generating door connections between rooms


            #region Preparing doors for corridor start and end if used

            if (UseCorridorGuide)
                if (CorridorPreset)
                {
                    // Guides for start and end dungeon points
                    if (CorridorGuide.StartGuideDoorInstruction > -1)
                    {
                        // Making hole from start point towards corridor back (should be static closed gates or so)
                        SpawnInstruction startDoorsGuide = PGGUtils.GenerateInstructionTowards(mainCorridorInstance.Checker,
                            CorridorGuide.Start, CorridorGuide.StartDir.GetDirection().V3toV3Int(), 5);

                        startDoorsGuide.definition = CorridorPreset.CellsInstructions[CorridorGuide.StartGuideDoorInstruction];
                        corridorGrid.guides.Add(startDoorsGuide);
                    }

                    if (CorridorGuide.EndGuideDoorInstruction > -1)
                    {
                        SpawnInstruction endDoorsGuide = PGGUtils.GenerateInstructionTowards(mainCorridorInstance.Checker,
                        CorridorGuide.End, CorridorGuide.EndDir.GetDirection().V3toV3Int(), 5); // 5 for centering

                        endDoorsGuide.definition = CorridorPreset.CellsInstructions[CorridorGuide.EndGuideDoorInstruction];
                        corridorGrid.guides.Add(endDoorsGuide);
                    }

                    mainCorridorInstance.IsMainCorridor = true;
                }

            #endregion


            #region Preparing doors for static rooms

            for (int i = 0; i < staticRoomGrids.Count; i++)
            {
                RectOfGeneratingHelper room = staticRoomGrids[i];
                if (room.fieldInstance.FieldRefStatic == null) continue;
                var sRoom = room.fieldInstance.FieldRefStatic;
                if (sRoom.Preset == null) continue;

                bool generateOtherConnectrions = !sRoom.OnlyCorridorConnection;
                int limit = 12;

                // Connection between room and corridor
                if (room.fieldInstance.Checker.IsAligning(mainCorridorInstance.Checker))
                {
                    RectOfGeneratingHelper corridor = corridorGrid;

                    if (sRoom.PrioritizeDoorsDirection != Vector3Int.zero)
                    {
                        // Making hole from room to corridor
                        SpawnInstruction instr = PGGUtils.GenerateInstructionTowards(room.fieldInstance.Checker, sRoom.StaticPosition + sRoom.PrioritizeOriginOffset, sRoom.PrioritizeDoorsDirection, 0, false);

                        if (sRoom.OverrideDoorholeCommand >= 0)
                            instr.definition = sRoom.Preset.CellsInstructions[sRoom.OverrideDoorholeCommand];
                        else
                            instr.definition = sRoom.Preset.CellsInstructions[FromRoomsToRoomsGuideId];

                        room.guides.Add(instr);


                        // Making hole from corridor to room
                        SpawnInstruction pathInstr = PGGUtils.GenerateInstructionTowards(corridor.fieldInstance.Checker, room.fieldInstance.Checker, 0, new Vector2Int(instr.helperCoords.x, instr.helperCoords.z));

                        if (sRoom.OverrideFromCorridorCommand >= 0)
                            pathInstr.definition = CorridorPreset.CellsInstructions[sRoom.OverrideFromCorridorCommand];
                        else
                            pathInstr.definition = CorridorPreset.CellsInstructions[FromRoomsToRoomsGuideId];

                        corridor.guides.Add(pathInstr);

                        corridor.fieldInstance.Connections.Add(room.fieldInstance);
                        room.fieldInstance.Connections.Add(corridor.fieldInstance);

                    }
                    else
                    {

                        // Generating guide for creating door on corridor side and room side
                        SpawnInstruction instr = PGGUtils.GenerateInstructionTowards(corridor.fieldInstance.Checker, room.fieldInstance.Checker, 0);

                        if (sRoom.OverrideFromCorridorCommand >= 0)
                            instr.definition = CorridorPreset.CellsInstructions[sRoom.OverrideFromCorridorCommand];
                        else
                            instr.definition = CorridorPreset.CellsInstructions[FromRoomsToRoomsGuideId];

                        corridor.guides.Add(instr);

                        corridor.fieldInstance.Connections.Add(room.fieldInstance);

                        // Making hole from room to corridor
                        SpawnInstruction pathInstr = PGGUtils.GenerateInstructionTowards(room.fieldInstance.Checker, corridor.fieldInstance.Checker, 0, new Vector2Int(instr.helperCoords.x, instr.helperCoords.z));

                        if (sRoom.OverrideDoorholeCommand >= 0)
                            pathInstr.definition = sRoom.Preset.CellsInstructions[sRoom.OverrideDoorholeCommand];
                        else
                            pathInstr.definition = sRoom.Preset.CellsInstructions[FromRoomsToRoomsGuideId];

                        room.guides.Add(pathInstr);

                        room.fieldInstance.Connections.Add(corridor.fieldInstance);

                    }

                }
                else // Not aligning to corridor so do any connection
                {
                    generateOtherConnectrions = true;
                    limit = 11;
                }

                // Generate connections with aligning rooms
                if (generateOtherConnectrions)
                {

                    #region Collecting rooms which have possibility for connection with this room

                    List<RectOfGeneratingHelper> aligning = new List<RectOfGeneratingHelper>();

                    for (int r = 0; r < roomsGrids.Count; r++)
                    {
                        var other = roomsGrids[r];
                        if (other.fieldInstance.Connections.Contains(room.fieldInstance)) continue; // Already connected

                        if (room.fieldInstance.Checker.IsAligning(other.fieldInstance.Checker))
                            aligning.Add(other);

                        if (aligning.Count >= limit) break;
                    }

                    #endregion

                    if (room.fieldInstance.FieldRefStatic != null)
                        if (room.fieldInstance.FieldRefStatic.Preset)
                            for (int a = 0; a < aligning.Count; a++)
                            {
                                RectOfGeneratingHelper other = aligning[a];
                                if (other.fieldInstance.FieldRef == null) continue;
                                if (other.fieldInstance.FieldRef.Preset == null) continue;

                                // Generating guide for creating door on room side and other room side
                                SpawnInstruction instr = PGGUtils.GenerateInstructionTowards(room.fieldInstance.Checker, other.fieldInstance.Checker, 0);
                                instr.definition = room.fieldInstance.FieldRefStatic.Preset.CellsInstructions[FromRoomsToRoomsGuideId];
                                room.guides.Add(instr);

                                room.fieldInstance.Connections.Add(other.fieldInstance);

                                // Making hole on other room to this room
                                SpawnInstruction pathInstr = PGGUtils.GenerateInstructionTowards(other.fieldInstance.Checker, room.fieldInstance.Checker, 0, new Vector2Int(instr.helperCoords.x, instr.helperCoords.z));
                                pathInstr.definition = other.fieldInstance.FieldRef.Preset.CellsInstructions[FromRoomToAlreadyConnectedRoomGuideId];
                                other.guides.Add(pathInstr);

                                other.fieldInstance.Connections.Add(room.fieldInstance);
                            }

                }
            }

            #endregion


            #region Preparing doors for rooms and corridor

            if (UseCorridorGuide)
            {
                if (CorridorPreset)
                    for (int i = 0; i < roomsGrids.Count; i++)
                    {
                        RectOfGeneratingHelper other = roomsGrids[i];
                        if (other.fieldInstance.FieldRef == null) continue;
                        if (other.fieldInstance.FieldRef.Preset == null) continue;

                        if (other.fieldInstance.Checker.IsAligning(mainCorridorInstance.Checker))
                        {
                            RectOfGeneratingHelper corridor = corridorGrid;

                            // Generating guide for creating door on room side and other room side
                            SpawnInstruction instr = PGGUtils.GenerateInstructionTowards(corridor.fieldInstance.Checker, other.fieldInstance.Checker, 0);
                            instr.definition = CorridorPreset.CellsInstructions[FromRoomsToRoomsGuideId];
                            corridor.guides.Add(instr);

                            corridor.fieldInstance.Connections.Add(other.fieldInstance);

                            // Making hole on other room to this room
                            SpawnInstruction pathInstr = PGGUtils.GenerateInstructionTowards(other.fieldInstance.Checker, corridor.fieldInstance.Checker, 0, new Vector2Int(instr.helperCoords.x, instr.helperCoords.z));
                            pathInstr.definition = other.fieldInstance.FieldRef.Preset.CellsInstructions[FromRoomsToRoomsGuideId];
                            other.guides.Add(pathInstr);

                            other.fieldInstance.Connections.Add(corridor.fieldInstance);
                        }

                    }
            }

            #endregion


            // Preparing doors between rooms
            for (int i = 0; i < roomsGrids.Count; i++)
            {
                RectOfGeneratingHelper room = roomsGrids[i];

                int limitConnections = -1;
                if (HaveConnectionWithCorridor(room.fieldInstance))
                    limitConnections = LimitDoorsCount.GetRandom(); // If already can go to corridor then dont create all connections
                if (limitConnections <= 0) limitConnections = -1;

                #region Collecting rooms which have possibility for connection with this room

                List<RectOfGeneratingHelper> aligning = new List<RectOfGeneratingHelper>();
                List<RectOfGeneratingHelper> reserve = new List<RectOfGeneratingHelper>();


                for (int r = 0; r < roomsGrids.Count; r++)
                {
                    if (r == i) continue;
                    var other = roomsGrids[r];

                    if (other.fieldInstance.Connections.Contains(room.fieldInstance)) continue; // Already connected

                    // Corridor connection relation
                    if (UseCorridorGuide)
                        if (limitConnections != -1)
                            if (HaveConnectionWithCorridor(other.fieldInstance))
                            {
                                reserve.Add(other);
                                continue;
                            }

                    if (room.fieldInstance.Checker.IsAligning(other.fieldInstance.Checker))
                        aligning.Add(other);

                    if (UseCorridorGuide) if (limitConnections != -1) if (aligning.Count >= limitConnections) break;
                }

                // Corridor connection relation
                if (UseCorridorGuide) if (limitConnections != -1) if (aligning.Count < limitConnections)
                        {
                            for (int r = 0; r < reserve.Count; r++)
                            {
                                aligning.Add(reserve[r]);
                                if (aligning.Count >= limitConnections) break;
                            }
                        }


                #endregion

                if (room.fieldInstance.FieldRef != null)
                    if (room.fieldInstance.FieldRef.Preset)
                        for (int a = 0; a < aligning.Count; a++)
                        {
                            RectOfGeneratingHelper other = aligning[a];

                            // Generating guide for creating door on room side and other room side
                            SpawnInstruction instr = PGGUtils.GenerateInstructionTowards(room.fieldInstance.Checker, other.fieldInstance.Checker, 0);
                            instr.definition = room.fieldInstance.FieldRef.Preset.CellsInstructions[FromRoomsToRoomsGuideId];
                            room.guides.Add(instr);

                            room.fieldInstance.Connections.Add(other.fieldInstance);

                            // Making hole on other room to this room
                            SpawnInstruction pathInstr = PGGUtils.GenerateInstructionTowards(other.fieldInstance.Checker, room.fieldInstance.Checker, 0, new Vector2Int(instr.helperCoords.x, instr.helperCoords.z));
                            pathInstr.definition = other.fieldInstance.FieldRef.Preset.CellsInstructions[FromRoomToAlreadyConnectedRoomGuideId];
                            other.guides.Add(pathInstr);

                            other.fieldInstance.Connections.Add(room.fieldInstance);
                        }

            }


            #endregion


            #region Using help guides if setted

            if (HelpGuides != null)
            {
                List<RectOfGeneratingHelper> allGrids = new List<RectOfGeneratingHelper>();
                if (UseCorridorGuide) allGrids.Add(corridorGrid);
                if (roomsGrids.Count > 0) PGGUtils.TransferFromListToList<RectOfGeneratingHelper>(roomsGrids, allGrids);
                if (StaticRooms.Count > 0) PGGUtils.TransferFromListToList<RectOfGeneratingHelper>(staticRoomGrids, allGrids);

                for (int i = 0; i < HelpGuides.Count; i++)
                {
                    var guide = HelpGuides[i];

                    // Find grid in which target cell exists
                    for (int g = 0; g < allGrids.Count; g++)
                    {
                        FGenCell cell = allGrids[g].grid.GetCell(guide.gridPosition, false);
                        if (FGenerators.CheckIfIsNull(cell )) continue;
                        if (cell.InTargetGridArea == false) continue;
                        //if (allGrids[i].fieldInstance.GetFieldSetup() == null) continue;

                        int definitionId = 0;
                        if (guide.helperType == EHelperGuideType.Spawn) definitionId = 1;
                        else if (guide.helperType == EHelperGuideType.ClearWall) definitionId = 2;

                        guide.definition = allGrids[g].fieldInstance.GetFieldSetup().CellsCommands[definitionId];

                        allGrids[g].guides.Add(guide);

                        break;
                    }

                }

            }

            #endregion


            #region Running instantiating

            // Running FieldSetups and Instantiating all objects
            ClearGenerated();

            Generated = new List<InstantiatedFieldInfo>();

            if (UseCorridorGuide)
            {
                if (CorridorPreset == null) { UnityEngine.Debug.Log("No Corridor preset!"); }
                else
                {
                    InstantiatedFieldInfo g = IGeneration.GenerateFieldObjectsWithContainer(CorridorPreset.name, CorridorPreset, corridorGrid.grid, transform, corridorGrid.guides, corridorGrid.inject);
                    Generated.Add(g);
                }
            }

            for (int i = 0; i < staticRoomGrids.Count; i++)
            {
                if (staticRoomGrids[i].grid.AllApprovedCells.Count <= 0) continue;
                if (staticRoomGrids[i].fieldInstance == null) continue;
                if (staticRoomGrids[i].fieldInstance.FieldRefStatic == null) continue;
                if (staticRoomGrids[i].fieldInstance.FieldRefStatic.Preset == null) continue;
                FieldSetup targetFieldSetup = staticRoomGrids[i].fieldInstance.FieldRefStatic.Preset;
                InstantiatedFieldInfo g = IGeneration.GenerateFieldObjectsWithContainer(targetFieldSetup.name, targetFieldSetup, staticRoomGrids[i].grid, transform, staticRoomGrids[i].guides, staticRoomGrids[i].inject);
                Generated.Add(g);
            }

            for (int i = 0; i < roomsGrids.Count; i++)
            {
                if (roomsGrids[i].grid.AllApprovedCells.Count <= 0) continue;
                FieldSetup targetFieldSetup = CorridorPreset;

                if (roomsGrids[i].fieldInstance != null)
                    if (roomsGrids[i].fieldInstance.FieldRef != null)
                        if (roomsGrids[i].fieldInstance.FieldRef.Preset != null)
                            targetFieldSetup = roomsGrids[i].fieldInstance.FieldRef.Preset;

                InstantiatedFieldInfo g = IGeneration.GenerateFieldObjectsWithContainer(targetFieldSetup.name, targetFieldSetup, roomsGrids[i].grid, transform, roomsGrids[i].guides, roomsGrids[i].inject);
                Generated.Add(g);
            }

            #endregion


            base.GenerateObjects();
        }

        /// <summary> Additional guides used in custom coding, helperType == Door -> command 0, Spawn -> command 1, ClearWall -> command 2 </summary>
        public List<SpawnInstruction> HelpGuides = null;

        bool HaveConnectionWithCorridor(RectOfFieldsInstance ins, bool deep = false)
        {
            for (int i = 0; i < ins.Connections.Count; i++)
            {
                if (ins.Connections[i].IsMainCorridor) return true;

                if (deep)
                    for (int c = 0; c < ins.Connections[i].Connections.Count; c++)
                        if (ins.Connections[i].Connections[c].IsMainCorridor)
                            return true;
            }

            return false;
        }


        void InjectBuildupInstructions(RectOfGeneratingHelper room, CheckerField boundChecker, List<Vector2Int> edges, FieldSetup field)
        {
            if (InstructionIdOnRectEdges >= field.CellsInstructions.Count) return;
            if (InstructionIdOnRectNeightbours >= field.CellsInstructions.Count) return;

            var checker = room.fieldInstance.Checker;

            // Adding remove walls on edges guides
            for (int c = 0; c < edges.Count; c++)
                if (checker.ContainsWorldPos(edges[c]))
                {
                    SpawnInstruction guide;

                    #region Adding ghost cell guides to prevent removing inside walls in neightbour rooms

                    if (InstructionIdOnRectNeightbours >= 0)
                    {
                        Vector2Int checkPos = edges[c] + new Vector2Int(1, 0);
                        if (boundChecker.ContainsWorldPos(checkPos) && checker.ContainsWorldPos(checkPos) == false) { guide = new SpawnInstruction(); guide.gridPosition = boundChecker.FromWorldToGridPos(checkPos).V2toV3Int(); guide.definition = field.CellsInstructions[InstructionIdOnRectNeightbours]; room.guides.Add(guide); }
                        checkPos = edges[c] + new Vector2Int(-1, 0);
                        if (boundChecker.ContainsWorldPos(checkPos) && checker.ContainsWorldPos(checkPos) == false) { guide = new SpawnInstruction(); guide.gridPosition = boundChecker.FromWorldToGridPos(checkPos).V2toV3Int(); guide.definition = field.CellsInstructions[InstructionIdOnRectNeightbours]; room.guides.Add(guide); }
                        checkPos = edges[c] + new Vector2Int(0, 1);
                        if (boundChecker.ContainsWorldPos(checkPos) && checker.ContainsWorldPos(checkPos) == false) { guide = new SpawnInstruction(); guide.gridPosition = boundChecker.FromWorldToGridPos(checkPos).V2toV3Int(); guide.definition = field.CellsInstructions[InstructionIdOnRectNeightbours]; room.guides.Add(guide); }
                        checkPos = edges[c] + new Vector2Int(0, -1);
                        if (boundChecker.ContainsWorldPos(checkPos) && checker.ContainsWorldPos(checkPos) == false) { guide = new SpawnInstruction(); guide.gridPosition = boundChecker.FromWorldToGridPos(checkPos).V2toV3Int(); guide.definition = field.CellsInstructions[InstructionIdOnRectNeightbours]; room.guides.Add(guide); }
                    }

                    #endregion

                    // Remove outer walls guides
                    guide = new SpawnInstruction();
                    guide.gridPosition = boundChecker.FromWorldToGridPos(edges[c]).V2toV3Int();
                    guide.definition = field.CellsInstructions[InstructionIdOnRectEdges];
                    room.guides.Add(guide);
                }
        }


        class RectOfGeneratingHelper
        {
            public FGenGraph<FieldCell, FGenPoint> grid;
            public List<SpawnInstruction> guides;
            public List<InjectionSetup> inject;
            public RectOfFieldsInstance fieldInstance;

            public RectOfGeneratingHelper(RectOfFieldsInstance ins)
            {
                grid = IGeneration.GetEmptyFieldGraph();
                guides = new List<SpawnInstruction>();
                inject = null;

                if (FGenerators.CheckIfIsNull(ins ))
                    fieldInstance = new RectOfFieldsInstance();
                else
                    fieldInstance = ins;
            }

            public void AddInject(InjectionSetup inj)
            {
                if (inject == null) inject = new List<InjectionSetup>();
                inject.Add(inj);
            }
        }


        private void OnValidate()
        {
            if (RefreshEveryChange) Prepare();

            for (int i = 0; i < RoomPresets.Count; i++) RoomPresets[i].CheckReset();
            for (int i = 0; i < StaticRooms.Count; i++) StaticRooms[i].CheckReset();
        }


        #region Gizmos

        protected override void DrawGizmos()
        {

            Vector3 presetCellSize = new Vector3(2, 1, 2);
            if (RoomPresets.Count > 0) if (RoomPresets[0].Preset) presetCellSize = RoomPresets[0].Preset.GetCellUnitSize();
            if (CorridorPreset) presetCellSize = CorridorPreset.GetCellUnitSize();

            presetCellSize.y *= 0.1f;
            float cellSizeX = presetCellSize.x;

            // Draw all shapes
            if (mainCorridorInstance != null)
            {
                Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
                mainCorridorInstance.DrawGizmos(cellSizeX);
            }

            float step = 1f / (float)rInstances.Count;
            for (int i = 0; i < rInstances.Count; i++)
            {
                if (rInstances[i].Setted == false) continue;

                Gizmos.color = Color.HSVToRGB((step * i + (i % 2 == 0 ? 0.35f : 0f)) % 1, 0.7f, 0.6f);
                rInstances[i].DrawGizmos(cellSizeX);
            }

            Gizmos.color = new Color(0.1f, 0.1f, 0.1f, 0.4f);
            for (int i = 0; i < rStatic.Count; i++)
            {
                rStatic[i].DrawGizmos(cellSizeX);
            }

            CorridorGuide.DrawGizmos(cellSizeX, presetCellSize);

            Gizmos.color = new Color(0.2f, 0.8f, 0.3f, 0.5f);
            fullRectCheck.DrawGizmos(cellSizeX);

            presetCellSize.y *= 0.1f;
            Gizmos.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
            Vector3 off = new Vector3(presetCellSize.x * 0.5f, 0f, presetCellSize.z * -0.5f);
            float xs = PackingRectSize.x * 1f;
            float zs = PackingRectSize.y * 1f;
            Gizmos.DrawLine(new Vector3(-xs, 0, -zs) * cellSizeX + off, new Vector3(xs, 0, -zs) * cellSizeX + off);
            Gizmos.DrawLine(new Vector3(xs, 0, -zs) * cellSizeX + off, new Vector3(xs, 0, zs) * cellSizeX + off);
            Gizmos.DrawLine(new Vector3(xs, 0, zs) * cellSizeX + off, new Vector3(-xs, 0, zs) * cellSizeX + off);
            Gizmos.DrawLine(new Vector3(-xs, 0, zs) * cellSizeX + off, new Vector3(-xs, 0, -zs) * cellSizeX + off);

        }

        #endregion

    }

    #region Inspector window with some enchancements


#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(RectangleOfFieldsGenerator))]
    public class SimpleRectOfFieldsGeneratorEditor : PGGGeneratorBaseEditor
    {
        public RectangleOfFieldsGenerator Get { get { if (_get == null) _get = (RectangleOfFieldsGenerator)target; return _get; } }
        private RectangleOfFieldsGenerator _get;
        bool drawGen = false;

        protected override void DrawGUIHeader()
        {
            UnityEditor.EditorGUILayout.HelpBox("This component is just simple generator for choosed 'Field Setups' push-packed into defined rect size", UnityEditor.MessageType.Info);
            base.DrawGUIHeader();
        }

        protected override void DrawGUIBody()
        {
            drawGen = EditorGUILayout.Foldout(drawGen, "Draw Guides Ids to Choose", true);

            if (drawGen)
            {
                SerializedProperty sp = serializedObject.FindProperty("FromMainCorridorToRoomsGuideId");
                EditorGUIUtility.labelWidth = 250;
                EditorGUILayout.PropertyField(sp); sp.Next(false);
                EditorGUILayout.PropertyField(sp); sp.Next(false);
                EditorGUILayout.PropertyField(sp); sp.Next(false);
                EditorGUILayout.PropertyField(sp);
                EditorGUIUtility.labelWidth = 0;
            }


            GUILayout.Space(4);
            if (GUILayout.Button("Preview"))
            {
                Get.ClearGenerated();
                Get.Prepare();
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Generate"))
            {
                Get.GenerateObjects();
            }

            if (Get.Generated != null) if (Get.Generated.Count > 0) if (GUILayout.Button("Clear Generated")) Get.ClearGenerated();
        }

        protected override void DrawGUIFooter()
        {
            base.DrawGUIFooter();
            UnityEditor.EditorGUILayout.HelpBox("All FieldSetups must have same Cell Size!", UnityEditor.MessageType.Info);
            UnityEditor.EditorGUILayout.HelpBox("Walls in FieldSetups must be offsetted from grid margins to be able for fat walls", UnityEditor.MessageType.Info);
        }
    }
#endif


    #endregion

}