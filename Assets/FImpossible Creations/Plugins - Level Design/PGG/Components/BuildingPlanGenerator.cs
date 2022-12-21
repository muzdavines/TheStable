#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FIMSpace.Generating.Planning
{
    [AddComponentMenu("FImpossible Creations/PGG/Building Plan Generator", 1)]
    public class BuildingPlanGenerator : MonoBehaviour
    {
        public bool GenrateOnGameStart = false;
        public bool RandomSeed = true;
        public int Seed = 0;

        [Space(3)]
        public BuildPlanPreset BuildingFloorPreset;
        [Range(0f, 0.49f)]
        [HideInInspector] public float WallsSeparation = 0f;
        private PlanHelper planHelper;

        [HideInInspector] public List<GameObject> Generated = new List<GameObject>();
        [HideInInspector] public UnityEvent RunAfterGenerating;

        [HideInInspector] public bool LimitSize = false;
        [HideInInspector] public Vector2Int SizeLimitX = new Vector2Int(-10, 10);
        [HideInInspector] public Vector2Int SizeLimitZ = new Vector2Int(-10, 10);

        [HideInInspector] public bool UseGuides = false;
        public List<PlanPathGuide> PlanGuides = new List<PlanPathGuide>();

        [HideInInspector] public bool _Editor_drawAdd = false;

        [System.Serializable]
        public class PlanPathGuide
        {
            public Vector2Int Start = new Vector2Int(-3, -6);
            public EPlanGuideDirecion StartDir = EPlanGuideDirecion.Back;
            public Vector2Int End = new Vector2Int(3, 6);
            public EPlanGuideDirecion EndDir = EPlanGuideDirecion.Forward;
            [Range(1, 5)] public int CellsSpace = 1;
            [Range(0f, 1f)] public float ChangeDirCost = .35f;
        }

        public struct RoomGuide
        {
            int MinDistanceTo;
            float MinDistance;
        }


        private void Start()
        {
            if (GenrateOnGameStart) Generate(WallsSeparation);
        }


        public PlanGeneratingHelpContainer GenerateScheme(float wallsSeparation)
        {
            ClearGenerated();
            if (RandomSeed) Seed = FGenerators.GetRandom(-99999, 99999);

            if (BuildingFloorPreset == null) return null;


            // Generating building floor plan
            FGenerators.SetSeed(Seed);

            planHelper = new PlanHelper(BuildingFloorPreset);
            if (LimitSize) planHelper.SetLimits(SizeLimitX, SizeLimitZ);

            if (UseGuides)
            {
                for (int i = 0; i < PlanGuides.Count; i++)
                {
                    var g = PlanGuides[i];
                    planHelper.GeneratePathFindedCorridor(g.Start, g.End, g.StartDir.GetDirection2D(), g.EndDir.GetDirection2D(), g.CellsSpace, g.ChangeDirCost);
                }
            }

            planHelper.GenerateCorridors(BuildingFloorPreset.RootChunkSetup.InternalSetup.TargetBranches.GetRandom() - 1, wallsSeparation);
            planHelper.GenerateRooms(wallsSeparation);


            // Preparing corridors grid --------------------------------------
            PlanGeneratingHelpContainer planContainer = new PlanGeneratingHelpContainer();

            planContainer.grid = IGeneration.GetEmptyFieldGraph();
            planContainer.guides = new List<SpawnInstruction>();

            // First generating corridors with ID = -1 -> building up grid for corridors and spawning
            for (int i = 0; i < planHelper.InteriorRects.Count; i++)
            {
                PlanHelper.HelperRect room = planHelper.InteriorRects[i];
                if (room.TypeID != -1) continue;

                planContainer.planRect = room;

                room.GenerateGraphCells(planContainer.grid);

                List<SpawnRestrictionsGroup> restr = room.GetRestrictionsList();

                for (int cr = 0; cr < restr.Count; cr++)
                {
                    for (int rcl = 0; rcl < restr[cr].Cells.Count; rcl++)
                        planContainer.guides.Add(restr[cr].Cells[rcl].GenerateGuide(planHelper.PlanPreset.RootChunkSetup.FieldSetup, restr[cr]));
                }
            }


            List<SpawnInstruction> fromCorridorGuides = new List<SpawnInstruction>();

            // Reserving places for corridor door wall holes
            for (int c = 0; c < planHelper.ConnectionRects.Count; c++)
            {
                var connection = planHelper.ConnectionRects[c];

                if (connection.Found == false) continue;

                if (connection.Connection1.TypeID == -1 || connection.Connection2.TypeID == -1)
                {
                    var sett = BuildingFloorPreset.Settings[connection.Connection1.IndividualID];
                    if (connection.Connection1.TypeID == -1) sett = planHelper.PlanPreset.CorridorSetup;

                    var guide = connection.GenerateGuide(sett.FieldSetup, sett, false);
                    var checkCell = planContainer.grid.GetCell(guide.gridPosition.x, guide.gridPosition.y, guide.gridPosition.z, false);

                    // Detecting that connection grid cell is on the other side of corridor (inside other room door cell)
                    if (checkCell == null || checkCell.InTargetGridArea == false)
                    {
                        fromCorridorGuides.Add(guide);
                        var sett2 = BuildingFloorPreset.Settings[connection.Connection2.IndividualID];
                        if (connection.Connection2.TypeID == -1) sett2 = planHelper.PlanPreset.CorridorSetup;

                        guide = connection.GenerateGuide(sett2.FieldSetup, sett2, true, 1.1f);
                        //guide = connection.GenerateGuide(EHelperGuideType.Doors, true, 1.1f);
                        planContainer.guides.Add(guide);
                    }

                    //else
                    //{
                    //    fromCorridorGuides.Add(guide);
                    //    //UnityEngine.Debug.Log("guide pos " + guide.gridPosition);
                    //}

                    //UnityEngine.Debug.Log("guide pos " + guide.gridPosition);
                    //corridors.guides.Add(guide);
                }
            }



            // Preparing interior rooms graphs -----------------------------------------------------------------
            planContainer.interiors = new List<PlanGeneratingHelpContainer>();


            // First generate all interior containers to communicate between structures later
            for (int i = 0; i < planHelper.InteriorRects.Count; i++)
            {
                var room = planHelper.InteriorRects[i];
                if (room.TypeID == -1) continue;

                PlanGeneratingHelpContainer interior = new PlanGeneratingHelpContainer();
                interior.planRect = room;
                interior.grid = IGeneration.GetEmptyFieldGraph();
                room.GenerateGraphCells(interior.grid);

                interior.guides = new List<SpawnInstruction>();

                List<SpawnRestrictionsGroup> restr = room.GetRestrictionsList();
                for (int cr = 0; cr < restr.Count; cr++)
                {
                    for (int rcl = 0; rcl < restr[cr].Cells.Count; rcl++)
                        interior.guides.Add(restr[cr].Cells[rcl].GenerateGuide(room.SettingsRef.FieldSetup, restr[cr]));
                }

                planContainer.interiors.Add(interior);
            }


            // Generating door connections between rooms and between room and corridor from room side
            for (int i = 0; i < planContainer.interiors.Count; i++)
            {
                PlanGeneratingHelpContainer interior = planContainer.interiors[i];
                var room = interior.planRect;

                // Checking each room for connections
                // Reserving places for doors
                for (int c = 0; c < planHelper.ConnectionRects.Count; c++)
                {
                    var connection = planHelper.ConnectionRects[c];

                    if (connection.Found == false) continue;
                    if (connection.Connection1.TypeID != room.TypeID && connection.Connection2.TypeID != room.TypeID) continue;
                    if (connection.Connection1.TypeID == -1) continue;

                    if (connection.Connection1.TypeID == room.TypeID) // Reserving doors for first room side
                    {
                        SpawnInstruction guide;

                        // From room to room or from room to corridor
                        if (connection.Connection1.TypeID != -1)
                        {
                            var sett = planHelper.PlanPreset.Settings[connection.Connection1.IndividualID];
                            guide = connection.GenerateGuide(sett.FieldSetup, sett);
                            interior.guides.Add(guide);
                        }

                        // From room to another room on other side without corridor connection
                        if (connection.Connection1.TypeID != -1 && connection.Connection2.TypeID != -1)
                        {
                            PlanGeneratingHelpContainer connectionInterior = new PlanGeneratingHelpContainer() { guides = null };

                            // Getting counter connection room
                            for (int s = 0; s < planContainer.interiors.Count; s++)
                                if (planContainer.interiors[s].planRect.IndividualID == connection.Connection2.IndividualID)
                                {
                                    connectionInterior = planContainer.interiors[s];

                                    if (connectionInterior.guides != null)
                                    {
                                        var sett2 = planHelper.PlanPreset.Settings[connection.Connection2.IndividualID];
                                        guide = connection.GenerateGuide(sett2.FieldSetup, sett2, true);
                                        connectionInterior.guides.Add(guide);
                                    }
                                }
                        }
                    }
                }

            }

            lastGenerated = planContainer;
            return planContainer;
        }

        PlanGeneratingHelpContainer lastGenerated = null;
        public void Generate(float wallsSeparation)
        {
            //if (RandomSeed) Seed = FGenerators.GetRandom(-99999, 99999);
            //ClearGenerated();

            if (BuildingFloorPreset == null) return;
            PlanGeneratingHelpContainer scheme = GenerateScheme(wallsSeparation);
            if (scheme == null) return;

            // Spawning corridor
            AddToGenerated(IGeneration.GenerateFieldObjects(BuildingFloorPreset.RootChunkSetup.FieldSetup, scheme.grid, GenerateTransformContainer(scheme), true, scheme.guides, null, true).Instantiated);

            // Generating Interiors
            for (int i = 0; i < scheme.interiors.Count; i++)
            {
                if (scheme.interiors[i].planRect.TypeID == -1) continue;

                var sch = BuildingFloorPreset.Settings[scheme.interiors[i].planRect.IndividualID];
                if (sch == null)
                {
                    UnityEngine.Debug.Log("No Scheme! " + i);
                    continue;
                }

                var setup = sch.FieldSetup;
                if (setup == null)
                {
                    UnityEngine.Debug.Log("No Field Setup! " + i + " in " + BuildingFloorPreset.Settings[scheme.interiors[i].planRect.IndividualID].GetName());
                    continue;
                }

                if (sch.InjectMods != null)
                    if (sch.InjectMods.Count > 0)
                        setup.SetTemporaryInjections(sch.InjectMods);

                AddToGenerated(
                    IGeneration.GenerateFieldObjects
                    (
                    setup,
                    scheme.interiors[i].grid,
                    GenerateTransformContainer(scheme.interiors[i]), true,
                    scheme.interiors[i].guides,
                    scheme.interiors[i].planRect.totalSepOffset,
                    true
                    ).Instantiated);

                if (sch.InjectMods != null)
                    if (sch.InjectMods.Count > 0)
                        setup.ClearTemporaryInjections();
            }

            if (RunAfterGenerating != null) RunAfterGenerating.Invoke();
            lastGenerated = scheme;
        }


        public void ClearGenerated()
        {
            for (int i = 0; i < Generated.Count; i++)
                if (Generated[i] != null)
                    FGenerators.DestroyObject(Generated[i]);

            Generated.Clear();
        }


        public void AddToGenerated(List<GameObject> list)
        {
            if (list == null) return;

            for (int i = 0; i < list.Count; i++)
                if (!Generated.Contains(list[i]))
                    Generated.Add(list[i]);
        }


        public Transform GenerateTransformContainer(PlanGeneratingHelpContainer targetContainer)
        {
            // TODO: On build no parenting?
            GameObject cnt = new GameObject();

            if (targetContainer.planRect.SettingsRef != null)
            {
                cnt.name = targetContainer.planRect.SettingsRef.GetName();
                if (targetContainer.planRect.TypeID == -1) cnt.name = "Corridor-" + cnt.name;
            }

            cnt.transform.SetParent(transform);
            cnt.transform.localPosition = Vector3.zero;
            cnt.transform.localRotation = Quaternion.identity;

            Generated.Add(cnt);
            return cnt.transform;
        }


#if UNITY_EDITOR
        public bool DrawDebugGrid = false;
        private void OnDrawGizmosSelected()
        {
            if (DrawDebugGrid == false) return;
            if (planHelper == null) return;
            if (planHelper.PlanPreset == null) return;
            if (planHelper.PlanPreset.RootChunkSetup == null) return;
            if (planHelper.PlanPreset.RootChunkSetup.FieldSetup == null) return;

            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Handles.matrix = Gizmos.matrix;

            Vector3 csize = planHelper.PlanPreset.RootChunkSetup.FieldSetup.GetCellUnitSize();
            csize.y *= 0.1f;
            float size = csize.x;

            for (int i = 0; i < planHelper.InteriorRects.Count; i++)
            {
                var room = planHelper.InteriorRects[i];
                var roomCells = room.GenerateGraphCells();

                if (room.SettingsRef != null) if (room.SettingsRef.FieldSetup != null) size = room.SettingsRef.FieldSetup.CellSize;

                foreach (var cell in roomCells)
                {
                    Gizmos.color = planHelper.PlanPreset.GetIDColor(room.TypeID, 0.3f);

                    Gizmos.DrawWireCube(cell.WorldPos(size), new Vector3(size, 0.1f, size));
                    Handles.Label(cell.WorldPos(size), new GUIContent(cell.PosXZ.ToString()), EditorStyles.centeredGreyMiniLabel);
                }
            }

            if (lastGenerated != null)
                if (lastGenerated.guides != null)
                {
                    Gizmos.color = new Color(1f, 1f, 0.1f, 0.7f);
                    //for (int i = 0; i < lastGenerated.guides.Count; i++)
                    //{
                    //    var guide = lastGenerated.guides[i];
                    //    Gizmos.DrawWireCube(guide.gridPosition * (int)size + guide.desiredDirection * (int)(size * 0.4f), new Vector3(size * 0.5f, 0.1f, size * 0.5f));
                    //}

                    for (int i = 0; i < lastGenerated.interiors.Count; i++)
                    {
                        for (int g = 0; g < lastGenerated.interiors[i].guides.Count; g++)
                        {
                            var guide = lastGenerated.interiors[i].guides[i];
                            Gizmos.DrawWireCube(guide.gridPosition * (int)size + guide.desiredDirection * (int)(size * 0.4f), new Vector3(size * 0.5f, 0.1f, size * 0.5f));
                        }

                    }
                }

            if (UseGuides)
            {
                for (int i = 0; i < PlanGuides.Count; i++)
                {
                    var g = PlanGuides[i];

                    Gizmos.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
                    Vector3 pos = new Vector3(g.Start.x, 0, g.Start.y) * size - csize;
                    Vector3 d = g.StartDir.GetDirection() * size;
                    Gizmos.DrawRay(pos, d);
                    Gizmos.DrawLine(pos + d, Vector3.Lerp(pos, pos + d, 0.7f) + Vector3.right * 0.12f * size);
                    Gizmos.DrawLine(pos + d, Vector3.Lerp(pos, pos + d, 0.7f) - Vector3.right * 0.12f * size);
                    Gizmos.DrawCube(pos, csize);

                    pos = new Vector3(g.End.x, 0, g.End.y) * size - csize;
                    d = g.EndDir.GetDirection() * size;
                    Gizmos.DrawRay(pos, d);
                    Gizmos.DrawLine(pos + d, Vector3.Lerp(pos, pos + d, 0.7f) + Vector3.right * 0.12f * size);
                    Gizmos.DrawLine(pos + d, Vector3.Lerp(pos, pos + d, 0.7f) - Vector3.right * 0.12f * size);
                    Gizmos.DrawCube(pos, csize);

                }
            }


            if (LimitSize)
            {
                Gizmos.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
                Vector3 off = new Vector3(csize.x * 0.5f, 0f, csize.z * -0.5f);
                Gizmos.DrawLine(new Vector3(SizeLimitX.x, 0, SizeLimitZ.x) * size + off, new Vector3(SizeLimitX.y, 0, SizeLimitZ.x) * size + off);
                Gizmos.DrawLine(new Vector3(SizeLimitX.y, 0, SizeLimitZ.x) * size + off, new Vector3(SizeLimitX.y, 0, SizeLimitZ.y) * size + off);
                Gizmos.DrawLine(new Vector3(SizeLimitX.y, 0, SizeLimitZ.y) * size + off, new Vector3(SizeLimitX.x, 0, SizeLimitZ.y) * size + off);
                Gizmos.DrawLine(new Vector3(SizeLimitX.x, 0, SizeLimitZ.y) * size + off, new Vector3(SizeLimitX.x, 0, SizeLimitZ.x) * size + off);
            }


            Gizmos.matrix = Matrix4x4.identity;
            Handles.matrix = Matrix4x4.identity;
        }
#endif


        /// <summary>
        /// Helper container to make generating algorithm more readable
        /// </summary>
        public class PlanGeneratingHelpContainer
        {
            public PlanHelper.HelperRect planRect;
            public FGenGraph<FieldCell, FGenPoint> grid;
            public List<SpawnInstruction> guides;
            public List<PlanGeneratingHelpContainer> interiors;
        }

    }


#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(BuildingPlanGenerator))]
    public class ExampleBuildingPlanGeneratorEditor : UnityEditor.Editor
    {
        public BuildingPlanGenerator Get { get { if (_get == null) _get = (BuildingPlanGenerator)target; return _get; } }
        private BuildingPlanGenerator _get;
        bool displayEvent = false;
        SerializedProperty sp_add;

        private void OnEnable()
        {
            sp_add = serializedObject.FindProperty("LimitSize");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This component is using old algorithms, it may be deleted in future versions!", MessageType.Warning);
            EditorGUILayout.HelpBox("This component is not supporting non-rectangle shapes for rooms!", MessageType.None);

            FGUI_Inspector.LastGameObjectSelected = Get.gameObject;

            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "PlanGuides");

            if (Get.BuildingFloorPreset != null)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("WallsSeparation"));
            }

            GUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Preview")) { Get.GenerateScheme(Get.WallsSeparation); SceneView.RepaintAll(); }
            if (GUILayout.Button("Generate")) Get.Generate(Get.WallsSeparation);
            EditorGUILayout.EndHorizontal();

            if (Get.Generated != null) if (Get.Generated.Count > 0) if (GUILayout.Button("Clear Generated")) Get.ClearGenerated();


            displayEvent = EditorGUILayout.Foldout(displayEvent, "Event After Generating", true);
            if (displayEvent) EditorGUILayout.PropertyField(serializedObject.FindProperty("RunAfterGenerating"));

            GUILayout.Space(5);


            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            GUILayout.Space(3);
            string ff = Get._Editor_drawAdd ? "▼" : "▲";
            if (GUILayout.Button(ff + "  Additional Parameters  " + ff, FGUI_Resources.HeaderStyle)) Get._Editor_drawAdd = !Get._Editor_drawAdd;
            GUILayout.Space(3);

            if (Get._Editor_drawAdd)
            {
                SerializedProperty sp = sp_add.Copy();

                EditorGUILayout.PropertyField(sp); sp.Next(false);
                if (Get.LimitSize) EditorGUILayout.PropertyField(sp); sp.Next(false);
                if (Get.LimitSize) EditorGUILayout.PropertyField(sp); sp.Next(false);

                GUILayout.Space(7);
                /*EditorGUILayout.PropertyField(sp); //Fill fully*/
                EditorGUILayout.PropertyField(sp); sp.Next(false);
                EditorGUILayout.PropertyField(sp); sp.Next(false);

                if (Get.UseGuides)
                {
                    GUILayout.Space(3);
                    EditorGUILayout.PropertyField(sp);
                }

            }

            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();

            if (Get.BuildingFloorPreset == null)
            {
                EditorGUILayout.HelpBox("No Build Plan Preset Assigned!", MessageType.Warning);
            }
            else
            {
                if (Get.BuildingFloorPreset.RootChunkSetup != null)
                    if (Get.BuildingFloorPreset.RootChunkSetup.FieldSetup == null)
                        EditorGUILayout.HelpBox("Corridor Preset is not assigned inside Build Plan!", MessageType.Warning);

            }
        }
    }
#endif

}