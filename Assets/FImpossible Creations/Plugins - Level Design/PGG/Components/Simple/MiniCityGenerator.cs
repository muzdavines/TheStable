using FIMSpace.Generating.Checker;
using FIMSpace.Generating.Planning;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif  
using UnityEngine;

namespace FIMSpace.Generating.RectOfFields
{
    [AddComponentMenu("FImpossible Creations/PGG/Mini City Generator", 103)]
    public class MiniCityGenerator : PGGGeneratorBase
    {
        [Range(4, 64)] public int StreetsCount = 8;
        [Range(3, 6)] public int StreetThickness = 3;
        public MinMax StreetsLength = new MinMax(8, 14);
        public FieldSetup StreetsSetup;

        [Space(5)]
        [Range(8, 64)] public int BuildingsCount = 14;
        public MinMax BuildingsSize = new MinMax(6, 14);
        public List<FieldSetup> BuildingsSetups;
        [Tooltip("When buildings field setups contains FieldVariable 'EnableDoors' then this variable will be temporary setted to false when generating buildings")]
        public bool DisableDefaultDoors = true;

        public List<BuildPlanInstance> instances = new List<BuildPlanInstance>();
        public List<BuildPlanInstance> streets = new List<BuildPlanInstance>();
        private CheckerField fullStreet;

        public override FGenGraph<FieldCell, FGenPoint> PGG_Grid { get { return null; } }
        public override FieldSetup PGG_Setup { get { return null; } }

        public override void Prepare()
        {
            base.Prepare();

            instances.Clear();
            streets.Clear();
            fullStreet = new CheckerField();

            Vector2Int[] latestPos = new Vector2Int[4]; // Remembering end street positions for continous generation
            Vector2Int[] latestDir = new Vector2Int[4];
            for (int i = 0; i < latestPos.Length; i++) { latestPos[i] = Vector2Int.zero; latestDir[i] = Vector2Int.zero; }

            // Generating street fragments and buildings around them
            for (int i = 0; i < StreetsCount; i++)
            {
                int mod = i % 4;
                Vector2Int mainDir;

                // Cross out streets spreading out
                if (mod == 0) mainDir = Vector2Int.right;
                else if (mod == 1) mainDir = Vector2Int.up;
                else if (mod == 2) mainDir = Vector2Int.left;
                else mainDir = Vector2Int.down;

                BuildPlanInstance str = new BuildPlanInstance(null, false);

                if (i > 3) // After casting all 4 directions
                    if (FGenerators.GetRandom(0f, 1f) < 0.35f) // Chance to go with street to side in smaller distance
                    {
                        int randomSign = FGenerators.GetRandom(0f, 1f) > 0.5f ? 1 : -1;
                        mainDir = PGGUtils.GetRotatedFlatDirectionFrom(mainDir) * (randomSign);
                    }

                // Casting path line to desired position and remembering end position
                Vector2Int finalPos = latestPos[mod] + latestDir[mod] + mainDir * (StreetsLength.GetRandom());
                str.Checker.AddPathTowards(latestPos[mod] - mainDir + latestDir[mod], finalPos, 0.75f, StreetThickness, false);
                latestPos[mod] = finalPos;
                latestDir[mod] = mainDir;

                streets.Add(str);
                fullStreet.Join(str.Checker);
            }

            fullStreet.RecalculateMultiBounds();
            GeneratorCheckers.Add(fullStreet);

            for (int i = 0; i < BuildingsCount; i++)
            {
                BuildPlanInstance ins = new BuildPlanInstance(null, false);
                ins.Checker.SetSize(BuildingsSize.GetRandom(), BuildingsSize.GetRandom(), true);
                bool setted = false;

                // Max 32 tries for setting building in city with snapping and collisions check
                for (int t = 0; t < 32; t++)
                {
                    BuildPlanInstance str = streets[FGenerators.GetRandom(0, streets.Count)];
                    ins.Checker.Position = str.Checker.GetRandom(false) + new Vector2Int(FGenerators.GetRandom(-8,8), FGenerators.GetRandom(-8,8));
                    ins.Checker.SnapToOther(str.Checker, true);
                    
                    if ( CollidesWithAny(ins.Checker) == false)
                    {
                        setted = true;
                        break;
                    }
                }

                if (setted)
                {
                    GeneratorCheckers.Add(ins.Checker);
                    instances.Add(ins);
                }
            }

        }


        /// <summary>
        /// Additional method to check all current shapes collisions
        /// </summary>
        public bool CollidesWithAny(CheckerField ch)
        {
            if (ch.CollidesWith(fullStreet)) return true;

            for (int i = 0; i < instances.Count; i++)
                if (ch.CollidesWith(instances[i].Checker)) return true;

            return false;
        }


        public override void GenerateObjects()
        {
            ClearGenerated();

            if ( StreetsSetup == null)
            {
                UnityEngine.Debug.Log("[Mini City Generator] No FieldSetup for streets! Can't generate!");
                return;
            }

            if (BuildingsSetups.Count == 0 || BuildingsSetups[0] == null)
            {
                UnityEngine.Debug.Log("[Mini City Generator] No FieldSetup for buildings! Can't generate!");
                return;
            }

            if (FGenerators.CheckIfIsNull( fullStreet ))
            {
                UnityEngine.Debug.Log("No Full Street Checker!");
                return;
            }

            // Setting up streets grid and instantiating
            GridPlanGeneratingHelper streetsGrid = new GridPlanGeneratingHelper(null);
            fullStreet.InjectToGrid(streetsGrid.grid);
            streetsGrid.SimplierAssign = StreetsSetup;

            Generated = new List<InstantiatedFieldInfo>();

            // Generating streets and adding to generated list to collect all generated objects
            Generated.Add( streetsGrid.GenerateOnGrid(transform) );

            #region Disable default doors for buildings

            List<InjectionSetup> injectNoDoors = new List<InjectionSetup>();

            if (DisableDefaultDoors)
            {
                InjectionSetup inj = new InjectionSetup(null, InjectionSetup.EGridCall.Pre);
                inj.OverrideVariables = true;
                inj.Overrides = new List<FieldVariable>();
                inj.Overrides.Add(new FieldVariable("EnableDoors", 0));
                injectNoDoors.Add(inj);
            }

            #endregion


            // Preparing buildings grids
            List<GridPlanGeneratingHelper> buildings = new List<GridPlanGeneratingHelper>();
            for (int i = 0; i < instances.Count; i++)
            {
                GridPlanGeneratingHelper build = new GridPlanGeneratingHelper(null);
                instances[i].Checker.InjectToGrid(build.grid);
                build.SimplierAssign = BuildingsSetups[FGenerators.GetRandom(0, BuildingsSetups.Count)];
                buildings.Add(build);
            }

            // Generate Doors Towards Street
            for (int i = 0; i < buildings.Count; i++)
            {
                SpawnInstruction instr = PGGUtils.GenerateInstructionTowardsSimple(instances[i].Checker, fullStreet, 5);
                instr.definition = buildings[i].SimplierAssign.CellsCommands[0];
                buildings[i].guides.Add(instr);
            }

            // Instantiating buildings
            for (int i = 0; i < buildings.Count; i++)
            {
                Generated.Add(buildings[i].GenerateOnGrid(transform, injectNoDoors));
            }

            base.GenerateObjects(); // Running optional unity event
        }



        #region Gizmos

        protected override void DrawGizmos()
        {
            if (FGenerators.CheckIfIsNull(fullStreet )) return;

            Vector3 presetCellSize = new Vector3(2, 1, 2);
            if (FGenerators.CheckIfExist_NOTNULL(StreetsSetup)) presetCellSize = StreetsSetup.GetCellUnitSize();
            presetCellSize.y *= 0.1f;
            float cellSizeX = presetCellSize.x;

            fullStreet.DrawGizmos(cellSizeX);

            float step = 1f / (float)instances.Count;
            for (int i = 0; i < instances.Count; i++)
            {
                Gizmos.color = Color.HSVToRGB((step * i + (i % 2 == 0 ? 0.35f : 0f)) % 1, 0.7f, 0.6f);
                instances[i].DrawGizmos(cellSizeX);
            }

            Gizmos_DrawRectangleFillShape(presetCellSize);
        }
        
        #endregion

    }
        

    #region Inspector window with some enchancements


#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(MiniCityGenerator))]
    public class MiniCityGeneratorEditor : PGGGeneratorBaseEditor
    {
        public MiniCityGenerator Get { get { if (_get == null) _get = (MiniCityGenerator)target; return _get; } }
        private MiniCityGenerator _get;

        protected override void OnHeaderGUI()
        {
            UnityEditor.EditorGUILayout.HelpBox("In future versions this component will generate more precise city plans!", MessageType.None);

            base.OnHeaderGUI();
        }

        protected override void DrawGUIBody()
        {
            UnityEditor.EditorGUILayout.HelpBox("Buildings FieldSetups must have doors modificator in 0 index to generate door models->to street correctly", MessageType.None);

            GUILayout.Space(4);
            if (GUILayout.Button("Preview"))
            {
                Get.ClearGenerated();
                Get.Prepare();
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Generate"))
            {
                Get.Prepare();
                Get.GenerateObjects();
            }

            if (Get.Generated != null) if (Get.Generated.Count > 0) if (GUILayout.Button("Clear Generated")) Get.ClearGenerated();

            DrawAdditionalTab();
        }

    }
#endif


    #endregion

}