using FIMSpace.Generating.Checker;
using FIMSpace.Generating.PathFind;
using FIMSpace.Generating.Planning;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif  

namespace FIMSpace.Generating.RectOfFields
{
    [AddComponentMenu("FImpossible Creations/PGG/Simple Dungeon Generator", 102)]
    public class SimpleDungeonGenerator : PGGPlanGeneratorBase
    {
        [Header("Rooms Placement Setup")]
        public MinMax RoomsToCorridorDistance = new MinMax(5, 7);
        [Range(1, 3)] public int TunnelsThickness = 1;
        [Range(1, 4)] public int RoomsSeparation = 1;
        [Range(0f, 1f)] public float AddConnectionsChance = 0f;

        [HideInInspector] public bool RandomGuide = false;
        [HideInInspector] [Range(1, 8)] public int RestrictStartEndPointInDistance = 3;

        [Header("Grid Generation Section")]
        public EDungeonJoiningMode JoiningMode = EDungeonJoiningMode.RoomsAreSeparatedGrids;
        public enum EDungeonJoiningMode { RoomsAreSeparatedGrids, RoomsAndPathsAreSeparatedGrids, OneGrid }

        public FieldSetup CorridorPreset { get { if (plan == null) return null; return plan.CorridorSetup.FieldSetup; } }
        public SimplePathGuide CorridorGuide;// { get { return PlanGuides[0]; } }
        //public SimplePathGuide CorridorGuide { get { return PlanGuides[0]; } }

        private BuildPlanInstance mainCorridorInstance;
        private List<BuildPlanInstance> dungeonShapes = new List<BuildPlanInstance>();
        private List<Vector2Int> mainCorrPoints;

        public override FGenGraph<FieldCell, FGenPoint> PGG_Grid { get { return null; } }
        public override FieldSetup PGG_Setup { get { return null; } }

        public override void Prepare()
        {
            if (BuildPlanPreset == null) return;
            if (BuildPlanPreset.Settings.Count == 0) return;

            base.Prepare();

            RefreshGuide();

            // Randomizing placement of start and end point of dungeon
            if (RandomGuide)
            {
                CorridorGuide.Start = new Vector2Int(-FGenerators.GetRandom(4, 12), -FGenerators.GetRandom(2, 8));
                CorridorGuide.End = new Vector2Int(FGenerators.GetRandom(14, 18), FGenerators.GetRandom(-14, 14));
                CorridorGuide.ChangeDirCost = FGenerators.GetRandom(.3f, .75f);
            }


            #region Generating Main Corridor

            dungeonShapes.Clear();
            mainCorridorInstance = new BuildPlanInstance(plan.RootChunkSetup, false, false);
            mainCorridorInstance.HelperID = -1; // -1 -> that means it's corridor
            mainCorridorInstance.Checker.Position = CorridorGuide.Start;

            List<List<Vector2Int>> mainCorrSetup = CheckerField.GeneratePathFindPointsFromStartToEnd(CorridorGuide);
            mainCorridorInstance.Checker.AddPathTowards(mainCorrSetup[0], CorridorGuide.PathThickness);
            mainCorridorInstance.Checker.Position += (new Vector2Int(-1, -1)); // Adjusting grid
            //mainCorridorInstance.Checker.PushAllChildPositions(new Vector2Int(-1, -1)); // Adjusting grid
            mainCorridorInstance.Checker.RecalculateMultiBounds();
            CorridorGuide.SpreadCheckerDataOn(mainCorridorInstance.Checker);
            dungeonShapes.Add(mainCorridorInstance);

            mainCorrPoints = mainCorrSetup[1]; // List 1 are points with curve start positions

            #endregion


            #region Generating Dungeon Rooms and paths to them


            int randomSpace = mainCorrSetup[0].Count / plan.GetToGenerateInteriorsCount() - 1;
            if (randomSpace < 0) randomSpace = 1;

            // Generating dungeon rooms (with paths towards them) in random placement around main corridor
            int iteration = 0;
            int aiteration = 0;
            for (int i = 0; i < plan.Settings.Count; i++)
            {
                var rm = plan.Settings[i];

                for (int r = 0; r < rm.Duplicates; r++) // repeating if we want to create few versions of the same dungeon room setup
                {

                    // Trying few times to find random and non-colliding with others placement
                    int maxTries = 25;
                    for (int t = 0; t < maxTries; t++)
                    {
                        aiteration++;

                        // Getting position square to create path to dungeon room from
                        int squareId = (iteration * randomSpace + 1) + FGenerators.GetRandom(0, randomSpace - 1);
                        Vector2 startPos = mainCorrSetup[0][squareId];

                        // If corridor goes up or down then we want branch left or right etc.
                        Vector2 mainPDir = PGGUtils.GetDirectionOver(mainCorrSetup[0], squareId, squareId + 1);
                        Vector2 branchDir = (mainPDir.x > 0) ? Vector2.up : Vector2.right;

                        float side = FGenerators.GetRandom(0, 2) == 1 ? -1f : 1f; // Left or right, up or down

                        //startPos += branchDir * (CorridorGuide.PathThickness-1);
                        startPos = mainCorridorInstance.Checker.NearestAlignFor(startPos.V2toV2Int(), (branchDir * side).V2toV2Int());

                        // Margins from main corridor start and end
                        if (Vector2Int.Distance(startPos.V2toV2Int(), CorridorGuide.Start) <= RestrictStartEndPointInDistance) continue;
                        if (Vector2Int.Distance(startPos.V2toV2Int(), CorridorGuide.End) <= RestrictStartEndPointInDistance) continue;


                        Vector2 endPos = startPos + branchDir * side * RoomsToCorridorDistance.GetRandom();

                        #region Generating path to dungeon room

                        SimplePathGuide pathFind = new SimplePathGuide();

                        pathFind.PathThickness = TunnelsThickness;
                        pathFind.ChangeDirCost = 0.6f;

                        pathFind.Start = startPos.V2toV2Int();
                        pathFind.End = endPos.V2toV2Int();

                        // Generate path from main corridor to dungeon room
                        var pathToRoom = CheckerField.GeneratePathFindPointsFromStartToEnd(pathFind);
                        BuildPlanInstance path = new BuildPlanInstance(plan.RootChunkSetup, false, false);
                        path.HelperID = -1; // -1 means it's corridor
                        path.Checker.Position = pathFind.Start; // Optional just for later debugging
                        path.Checker.AddPathTowards(pathToRoom[0], pathFind.PathThickness);
                        //path.Checker.RecalculateMultiBounds();
                        path.Checker.RemoveOnesCollidingWith(mainCorridorInstance.Checker);

                        #endregion


                        // Checking if path is not colliding with other paths
                        if (CheckIfColliding(path.Checker, 0)) continue; // Collides -> then another try
                        if (CheckAligningPointsCount(path.Checker) > TunnelsThickness) continue; // Wall on wall -> then another try

                        // Now let's generate dungeon room chamber rectangle
                        BuildPlanInstance room = new BuildPlanInstance(rm);
                        room.Checker = rm.GetChecker(true);

                        room.Checker.Position = endPos.V2toV2Int(); // centering by hand
                        room.Checker.Position += (branchDir * (Mathf.FloorToInt((float)room.Checker.GetSizeOnAxis(branchDir) * side)) / 2f).V2toV2Int();

                        // Removing overlapping squares
                        path.Checker.RemoveOnesCollidingWith(room.Checker);

                        // Checking if room is not colliding with other shapes
                        if (CheckIfColliding(room.Checker, RoomsSeparation, false)) continue; // Collides -> then another try

                        // Check if room restrictions are met
                        if (rm.CheckIfRestrictionAllows(room.Checker, dungeonShapes) == false) continue;

                        room.Connections.Add(path);
                        path.Connections.Add(mainCorridorInstance);
                        path.Connections.Add(room);
                        mainCorridorInstance.Connections.Add(path);
                        room.SpreadDataOn(mainCorridorInstance);

                        dungeonShapes.Add(path);
                        dungeonShapes.Add(room);

                        break;
                    }

                    iteration++;
                }

            }


            #endregion


            List<BuildPlanInstance> additionals = new List<BuildPlanInstance>();


            #region Generating additional path connections between dungeon rooms with connection propability setted


            for (int i = 0; i < dungeonShapes.Count; i++)
            {
                BuildPlanInstance ishape = dungeonShapes[i];
                if (ishape.HelperID == -1) continue; // No Corridors
                if (ishape.HaveFreeConnectionSlots() == false) continue; // Not allowed connecting with 
                if (AddConnectionsChance <= 0f) continue;

                if (FGenerators.GetRandom(0f, 1f) > AddConnectionsChance) continue;

                for (int o = 0; o < dungeonShapes.Count; o++)
                {
                    if (i == o) continue;

                    BuildPlanInstance oshape = dungeonShapes[o];
                    SingleInteriorSettings fRef = ishape.SettingsReference;

                    if (oshape.HelperID == -1) continue; // No Corridors
                    if (oshape.CanConnectWith(ishape) == false) continue; // Not yet connected with additional tunnel
                    if (oshape.HelperVar == 1f) continue; // If already connected with additional tunnel then ignore
                    if (Vector2.Distance(ishape.Checker.Position, oshape.Checker.Position) > RoomsToCorridorDistance.Max * 2) continue;

                    #region Generating path to dungeon room

                    var path = CheckerField.GenerateLinePoints(ishape.Checker.Position, oshape.Checker.Position, 0.5f);

                    BuildPlanInstance addPath = new BuildPlanInstance(plan.RootChunkSetup, false, false);
                    addPath.HelperID = -1; // -1 that means it is corridor
                    addPath.Checker.AddPathTowards(path, TunnelsThickness);
                    //addPath.Checker.RecalculateMultiBounds();
                    addPath.Checker.RemoveOnesCollidingWith(mainCorridorInstance.Checker);
                    addPath.Checker.RemoveOnesCollidingWith(ishape.Checker);
                    addPath.Checker.RemoveOnesCollidingWith(oshape.Checker);

                    if (CheckIfColliding(addPath.Checker, 0)) continue;

                    if (CheckAligningPointsCount(addPath.Checker) > 2) continue;

                    additionals.Add(addPath);

                    ishape.Connections.Add(addPath);
                    addPath.Connections.Add(ishape);
                    addPath.Connections.Add(oshape);
                    oshape.Connections.Add(addPath);
                    oshape.HelperVar = 1f;
                    ishape.HelperVar = 1f;

                    break;

                    #endregion

                }
            }


            #endregion


            for (int a = 0; a < additionals.Count; a++) dungeonShapes.Add(additionals[a]);


            // Deliver what's generated to the generator 
            GeneratorCheckers = new List<CheckerField>();
            for (int i = 0; i < dungeonShapes.Count; i++) GeneratorCheckers.Add(dungeonShapes[i].Checker);
            for (int i = 0; i < dungeonShapes.Count; i++) GeneratorCheckers.Add(dungeonShapes[i].Checker);
        }


        private void OnValidate() { Prepare(); }


        public override void GenerateObjects()
        {
            if (CorridorPreset == null) return;

            Prepare(); // Preparing dungeon scheme

            // Generating grids out of dungeon shapes
            GridPlanGeneratingHelper mainCorridorsGrid = new GridPlanGeneratingHelper();
            List<GridPlanGeneratingHelper> subCorridorsGrids = new List<GridPlanGeneratingHelper>();
            List<GridPlanGeneratingHelper> dungeonRoomsGrids = new List<GridPlanGeneratingHelper>();

            // Generating main corridors grid
            for (int i = 0; i < dungeonShapes.Count; i++)
            {
                var shape = dungeonShapes[i];
                if (shape.HelperID == 0) continue; // Ignoring rooms

                // Checking if inject them
                if (JoiningMode == EDungeonJoiningMode.RoomsAndPathsAreSeparatedGrids)
                    if (shape != mainCorridorInstance) continue; // if everything separated then ignore all excluding main corridor

                mainCorridorsGrid.fieldInstance = (shape);
                shape.Checker.InjectToGrid(mainCorridorsGrid.grid);
            }


            #region If we want path corridors to be separated

            if (JoiningMode == EDungeonJoiningMode.RoomsAndPathsAreSeparatedGrids)
            {
                for (int i = 0; i < dungeonShapes.Count; i++)
                {
                    #region Selecting corridors
                    var shape = dungeonShapes[i]; // Checking if inject them
                    if (shape == mainCorridorInstance) continue;
                    if (shape.HelperID == 0) continue;
                    #endregion

                    GridPlanGeneratingHelper corridorGrid = new GridPlanGeneratingHelper(shape);
                    shape.Checker.InjectToGrid(corridorGrid.grid);
                    subCorridorsGrids.Add(corridorGrid);
                }
            }

            #endregion


            #region Preparing dungeon rooms grids

            for (int i = 0; i < dungeonShapes.Count; i++)
            {
                // Selecting rooms
                var shape = dungeonShapes[i];
                if (shape.HelperID == -1) continue;

                if (JoiningMode != EDungeonJoiningMode.OneGrid)
                {
                    GridPlanGeneratingHelper roomGrid = new GridPlanGeneratingHelper(shape);
                    shape.Checker.InjectToGrid(roomGrid.grid);
                    dungeonRoomsGrids.Add(roomGrid);
                }
                else
                {
                    shape.Checker.InjectToGrid(mainCorridorsGrid.grid);
                }
            }

            #endregion


            #region Preparing guides for doors

            // Guides for start and end dungeon points
            // Making hole from start point towards corridor back (should be static closed gates or so)
            mainCorridorsGrid.guides.Add(CorridorGuide.GenerateStartDoorHoleInstructionOn(mainCorridorInstance.Checker, plan.RootChunkSetup.FieldSetup));
            mainCorridorsGrid.guides.Add(CorridorGuide.GenerateEndDoorHoleInstructionOn(mainCorridorInstance.Checker, plan.RootChunkSetup.FieldSetup));


            // Guides for dungeon room entrances
            for (int i = 0; i < dungeonRoomsGrids.Count; i++)
            {
                GridPlanGeneratingHelper room = dungeonRoomsGrids[i];

                // Finding connection blocks for doorways between dungeon rooms and path corridors
                for (int c = 0; c < room.fieldInstance.Connections.Count; c++)
                {
                    BuildPlanInstance pathConnection = room.fieldInstance.Connections[c];
                    if (pathConnection == mainCorridorInstance) continue;

                    SpawnInstruction instr = PGGUtils.GenerateInstructionTowards(room.fieldInstance.Checker, pathConnection.Checker, room.fieldInstance.SettingsReference);
                    room.guides.Add(instr);

                    GridPlanGeneratingHelper corridor = mainCorridorsGrid;

                    #region Getting right corridor grid with it's path shape reference

                    if (JoiningMode == EDungeonJoiningMode.RoomsAndPathsAreSeparatedGrids)
                        for (int s = 0; s < subCorridorsGrids.Count; s++)
                        {
                            GridPlanGeneratingHelper sub = subCorridorsGrids[s];
                            if (sub.fieldInstance == (pathConnection))
                            {
                                corridor = sub;
                                break;
                            }
                        }

                    #endregion

                    // Making hole on path - to room side

                    SpawnInstruction pathInstr = PGGUtils.GenerateInstructionTowards(pathConnection.Checker, room.fieldInstance.Checker, 5, new Vector2Int(instr.helperCoords.x, instr.helperCoords.z));
                    pathInstr.definition = CorridorPreset.CellsInstructions[plan.CorridorSetup.DoorHoleCommandID];
                    corridor.guides.Add(pathInstr);
                }
            }


            // Guides for entrances from main corridor to paths 
            if (JoiningMode == EDungeonJoiningMode.RoomsAndPathsAreSeparatedGrids)
            {
                for (int s = 0; s < subCorridorsGrids.Count; s++)
                {
                    GridPlanGeneratingHelper path = subCorridorsGrids[s];

                    for (int c = 0; c < subCorridorsGrids[s].fieldInstance.Connections.Count; c++)
                    {
                        BuildPlanInstance cn = subCorridorsGrids[s].fieldInstance.Connections[c];
                        if (cn != mainCorridorsGrid.fieldInstance) continue;

                        CheckerField pathChecker = path.fieldInstance.Checker;
                        CheckerField mainCorrChecker = cn.Checker;

                        Vector2Int nearestOwn = pathChecker.NearestPoint(mainCorrChecker);
                        Vector2Int nearestOther = mainCorrChecker.NearestPoint(nearestOwn);
                        // Centering
                        nearestOwn = pathChecker.GetCenterOnEdge(nearestOwn, nearestOther - nearestOwn, 6, mainCorrChecker);
                        nearestOther = mainCorrChecker.NearestPoint(nearestOwn);

                        // Full source way to generate doors below --------

                        // Making hole on path - to main corridor side
                        SpawnInstruction instr = new SpawnInstruction();
                        instr.definition = CorridorPreset.CellsInstructions[plan.CorridorSetup.DoorHoleCommandID];
                        instr.desiredDirection = (nearestOther - nearestOwn).V2toV3Int();
                        instr.useDirection = true;
                        instr.gridPosition = path.fieldInstance.Checker.FromWorldToGridPos(nearestOwn).V2toV3Int();
                        path.guides.Add(instr);

                        // Making hole on main Corridor - to path side
                        SpawnInstruction mainCorrInstr = new SpawnInstruction();
                        mainCorrInstr.definition = CorridorPreset.CellsInstructions[plan.CorridorSetup.DoorHoleCommandID];
                        mainCorrInstr.desiredDirection = (nearestOwn - nearestOther).V2toV3Int();
                        mainCorrInstr.useDirection = true;
                        mainCorrInstr.gridPosition = cn.Checker.FromWorldToGridPos(nearestOther).V2toV3Int();
                        mainCorridorsGrid.guides.Add(mainCorrInstr);
                    }

                }
            }

            #endregion


            #region Running instantiating

            // Running FieldSetups and Instantiating all objects
            ClearGenerated();

            // Putting all in separated transforms
            Generated.Add(mainCorridorsGrid.GenerateOnGrid(transform));

            for (int i = 0; i < subCorridorsGrids.Count; i++)
                Generated.Add(subCorridorsGrids[i].GenerateOnGrid(transform));

            for (int i = 0; i < dungeonRoomsGrids.Count; i++)
                Generated.Add(dungeonRoomsGrids[i].GenerateOnGrid(transform));

            #endregion


            base.GenerateObjects();
        }


        bool CheckIfColliding(CheckerField checker, int radius, bool ignoreMainCorridors = false)
        {
            for (int i = 0; i < dungeonShapes.Count; i++)
            {
                if (ignoreMainCorridors) if (dungeonShapes[i] == mainCorridorInstance) continue;
                if (checker.CollidesWithRadius(dungeonShapes[i].Checker, radius)) return true;
            }

            return false;
        }


        int CheckAligningPointsCount(CheckerField checker)
        {
            int aligns = 0;

            for (int i = 0; i < dungeonShapes.Count; i++)
                aligns += dungeonShapes[i].Checker.AlignPointsCount(checker);

            return aligns;
        }


        internal void RefreshGuide()
        {
            //if (PlanGuides == null) PlanGuides = new List<SimplePathGuide>();
            //if (PlanGuides.Count == 0) PlanGuides.Add(new SimplePathGuide());
        }


        #region Gizmos

        [Range(0f, 1f)] // Debugging
        private float progr = 1f;

        private void OnDrawGizmosSelected()
        {
            if (CorridorPreset == null) return;

            Color preColor = GUI.color;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);

            Vector3 presetCellSize = CorridorPreset.GetCellUnitSize();
            presetCellSize.y *= 0.1f;
            float cellSizeX = presetCellSize.x;

            // Draw all shapes
            Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
            for (int i = 0; i < dungeonShapes.Count; i++) dungeonShapes[i].DrawGizmos(cellSizeX);
            CorridorGuide.DrawGizmos(cellSizeX, presetCellSize);

            #region Draw Debug Progress

            if (mainCorrPoints != null)
            {
                Vector3 off = new Vector3(-0f, 0f, -0f);
                var pp = PGGUtils.GetProgessPositionOverLines(mainCorrPoints, progr);
                Gizmos.DrawSphere(pp.V2toV3() * cellSizeX + off, cellSizeX * 0.3f);

                var pd = PGGUtils.GetDirectionOverLines(mainCorrPoints, progr);
                Gizmos.DrawRay(pp.V2toV3() * cellSizeX + off, pd.V2toV3() * cellSizeX);
            }

            #endregion

            Gizmos.color = preColor;
            Gizmos.matrix = Matrix4x4.identity;
        }

        #endregion

    }


    #region Inspector window with some enchancements


#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(SimpleDungeonGenerator))]
    public class SimpleDungeonGeneratorEditor : PGGPlanGeneratorBaseEditor
    {
        public SimpleDungeonGenerator Get { get { if (_get == null) _get = (SimpleDungeonGenerator)target; return _get; } }
        private SimpleDungeonGenerator _get;
        SerializedProperty sp_guide;
        SerializedProperty sp_guideSett;

        protected override void OnEnable()
        {
            base.OnEnable();

            sp_guideSett = serializedObject.FindProperty("RandomGuide");
            sp_guide = serializedObject.FindProperty("CorridorGuide");
            //sp_guide = serializedObject.FindProperty("PlanGuides");
            //sp_guide.Next(true); /*sp_guide.Next(true); sp_guide.Next(false);sp_guide.Next(false);*/
        }

        protected override void DrawGUIHeader()
        {
            Get.RefreshGuide();

            UnityEditor.EditorGUILayout.HelpBox("This component is just simple example for generating basic dungeons with small amount of code", UnityEditor.MessageType.Info);
            base.DrawGUIHeader();
        }



        protected override void DrawGUIBody()
        {
            base.DrawGUIBody();

            GUILayout.Space(5);
            SerializedProperty sp = sp_guideSett.Copy();
            EditorGUILayout.PropertyField(sp);
            sp.Next(false); EditorGUILayout.PropertyField(sp);

            GUILayout.Space(5);
            DrawGeneratingButtons();
            AdditionalTabHeader();
        }

        protected override void UnderPresetGUI(SerializedProperty sp)
        {
            GUILayout.Space(3);
            EditorGUILayout.PropertyField(sp_guide, new GUIContent("Dungeon Path Guides"));
            GUILayout.Space(-15);
            base.UnderPresetGUI(sp);
        }

        protected override void DrawAdditionalTab() { }

        protected override void DrawAdditionalSettingsContent()
        {
            base.DrawAdditionalSettingsContent();
        }


        protected override void DrawGUIFooter()
        {
            UnityEditor.EditorGUILayout.HelpBox("All FieldSetups must have same Cell Size!", UnityEditor.MessageType.Info);
            UnityEditor.EditorGUILayout.HelpBox("Walls in FieldSetups must be offsetted from grid margins to be able for thick walls", UnityEditor.MessageType.Info);
            base.DrawGUIFooter();

            if (Get.BuildPlanPreset == null)
            {
                EditorGUILayout.HelpBox("No Build Plan Preset Assigned!", MessageType.Warning);
            }
            else
            {
                if (Get.BuildPlanPreset.RootChunkSetup != null)
                    if (Get.BuildPlanPreset.RootChunkSetup.FieldSetup == null)
                        EditorGUILayout.HelpBox("Corridor Preset is not assigned inside Build Plan!", MessageType.Warning);

            }
        }

    }
#endif


    #endregion

}