using UnityEngine;
using UnityEditor;
using FIMSpace.FEditor;
using UnityEditor.Callbacks;
using FIMSpace.Hidden;

namespace FIMSpace.Generating
{
    public partial class FieldDesignWindow : EditorWindow
    {
        public PGGStartupReferences StartupRefs;
        public static FieldDesignWindow Get;

        GameObject mainGeneratedsContainer;
        InstantiatedFieldInfo generated;

        FieldSetup projectPreset;
        SerializedObject so_preset;
        private ScriptableObject wasChecked = null;
        private bool isInDefaultDirectory = false;


        #region Window Edit Variables

        Vector2 mainScroll = Vector2.zero;

        public int Seed = 0;
        public MinMax SizeX = new MinMax(6, 6);
        public MinMax SizeY = new MinMax(0, 0);
        public MinMax SizeZ = new MinMax(4, 4);
        public Vector3Int OffsetGrid = new Vector3Int(0, 0, 0);
        public MinMax BranchLength = new MinMax(5, 9);
        public MinMax TargetBranches = new MinMax(4, 6);
        public MinMax CellsSpace = new MinMax(1, 3);
        public bool RunAdditionalGenerators = false;
        public bool DrawAdditionalGen = false;

        public GameObject SendMessageAfterGenerateTo;
        public string PostGenerateMessage;

        public GameObject SendMessageOnChangeTo;
        public string OnChangeMessage;

        bool repaint = false;
        static int viewed = 0;

        #endregion


        [MenuItem("Window/FImpossible Creations/PGG Field Designer Window", false, 49)]
        [MenuItem("Window/FImpossible Creations/Level Design/Field Designer Window", false, 51)]
        #region Initializing Field Designer Window
        static void Init()
        {
            FieldDesignWindow window = (FieldDesignWindow)GetWindow(typeof(FieldDesignWindow));
            viewed++;

            window.titleContent = new GUIContent("Field Designer", Resources.Load<Texture>("SPR_FieldDesigner"));
            window.Show();

            Get = window;
            if (Get) if (Get.Repose) window.position = new Rect(300, 100, 450, 500);
        }

        public static void FrameCenter(float distance, bool onlyRot = false, float zMul = -0.75f, float xMul = 0.1f, float onlyRotAngle = 45f)
        {
            SceneView view = SceneView.lastActiveSceneView;
            if (view == null) return;

            var tgt = view.camera;

            if (onlyRot == false)
            {
                tgt.transform.position = new Vector3(0f, distance, distance * zMul);
                tgt.transform.rotation = Quaternion.LookRotation(-tgt.transform.position.normalized);
                tgt.transform.position = new Vector3(distance * xMul, tgt.transform.position.y, tgt.transform.position.z);
            }
            else
            {
                tgt.transform.rotation = Quaternion.Euler(onlyRotAngle, 0, 0);
            }

            view.AlignViewToObject(tgt.transform);
        }
        #endregion


        #region Open window on double-click on FieldDesigner File

        [OnOpenAssetAttribute(1)]
        public static bool OpenFieldScriptableFile(int instanceID, int line)
        {
            Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj as FieldSetup != null)
            {
                OpenFieldSetupFileInWindow(obj as FieldSetup);
                return true;
            }

            return false;
        }
        #endregion


        public static void OpenFieldSetupFileInWindow(FieldSetup fs)
        {
            if (Get != null) Get.Repose = false;

            Init();
            Get.projectPreset = fs;

            Get.drawTestGenSetts = false;

            Get.ClearAllGeneratedGameObjects();
            Get.GenerateBaseFieldGrid();
            Get.TriggerRefresh(false);
        }


        #region Advanced Def

        bool isSelectedPainter = false;
        bool advancedMode = false;
        private void OnEnable()
        {
            advancedMode = EditorPrefs.GetBool("PGGAdv", false);

            if (StartupRefs == null) return;
            if (StartupRefs.FSDraftsdirectory == null) return;
            string path = AssetDatabase.GetAssetPath(StartupRefs.FSDraftsdirectory);
            var files = System.IO.Directory.GetFiles(path, "*.asset");
            if (files != null) latestFilesInDraft = files.Length;
        }

        #endregion

        Color preCol;
        Color preBGCol;
        void OnGUI()
        {
            PGGUtils.SetDarkerBacgroundOnLightSkin();

            preCol = GUI.color;
            preBGCol = GUI.backgroundColor;

            if (Get == null) Init();

            mainScroll = EditorGUILayout.BeginScrollView(mainScroll);


            #region Top Header

            GUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();
            string modeTitle = "Beginner";

            if (advancedMode)
            {
                modeTitle = "Advanced";
                GUI.backgroundColor = new Color(0.25f, 0.7f, 1f, 1f);
            }
            else
                GUI.backgroundColor = new Color(0.5f, 0.5f, 1f, 0.9f);


            if (GUILayout.Button(new GUIContent(modeTitle, "Current Mode: " + modeTitle + "\nDisplaying more or less options in the FieldDesigner window to be more friendly for novice users."), FGUI_Resources.ButtonStyle, GUILayout.Width(72)))
            {
                advancedMode = !advancedMode;
                if (advancedMode) DrawScreenGUI = true; else drawPack = true;
                EditorPrefs.SetBool("PGGAdv", advancedMode);
            }

            GUI.backgroundColor = preBGCol;

            EditorGUILayout.LabelField("Prepare Field Setup with dynamic preview", FGUI_Resources.HeaderStyle);
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2);

            if (Selection.gameObjects != null)
                if (Selection.gameObjects.Length > 0)
                {
                    isSelectedPainter = Selection.gameObjects[0].GetComponent<GridPainter>();
                    if (!isSelectedPainter) isSelectedPainter = Selection.gameObjects[0].GetComponent<FlexiblePainter>();
                }

            #endregion


            #region Advanced / Beginner mode switches

            if (!advancedMode)
            {
                AutoRefreshPreview = true;
                DrawWhenFocused = false;
                PreviewAlpha = 0f;
                //ColorizePreview = true;
                DrawGrid = true;
                DrawEmptys = true;
                AutoDestroy = true;
                DrawScreenGUI = false;
                gridMode = EDesignerGridMode.RectangleGrid;
            }

            #endregion



            #region Checking project file if it's in drafts directory


            if (wasChecked != projectPreset)
            {
                so_preset = null;
                isInDefaultDirectory = false;

                wasChecked = projectPreset;

                if (projectPreset != null)
                {
                    so_preset = new SerializedObject(projectPreset);

                    if (StartupRefs != null)
                        if (StartupRefs.FSDraftsdirectory)
                        {
                            string qPath = AssetDatabase.GetAssetPath(StartupRefs.FSDraftsdirectory);
                            string sPath = AssetDatabase.GetAssetPath(projectPreset);
                            qPath = System.IO.Path.GetFileName(qPath);
                            sPath = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(sPath));
                            if (sPath.Contains(qPath)) isInDefaultDirectory = true;
                        }
                }
            }

            #endregion


            #region Generating or validating base preset

            Get = this;

            if (projectPreset != null)
            {
                if (so_preset == null) so_preset = new SerializedObject(projectPreset);
                projectPreset.Validate();
            }

            #endregion


            GUILayout.Space(3);
            DrawFieldGenWindowGUI(projectPreset); // Main GUI in this method

            GUILayout.Space(3);

            EditorGUILayout.BeginHorizontal();
            if (grid == null)
            {
                GenerateBaseFieldGrid();
                if (AutoRefreshPreview) if(!UsePainterGenerating) RunFieldCellsRules();
            }

            EditorGUILayout.EndHorizontal();

            if (projectPreset != null)
            {
                if (grid != null)
                {
                    if (projectPreset.ModificatorPacks.Count > 0)
                    {

                        if (advancedMode)
                        {
                            GUILayout.Space(2);

                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                            EditorGUILayout.BeginHorizontal();
                            FGUI_Inspector.FoldHeaderStart(ref GenButtonsFoldout, "Generating Buttons");

                            if (GenButtonsFoldout == false)
                            {
                                GUILayout.FlexibleSpace();
                                EditorGUIUtility.labelWidth = 39;
                                PreviewAutoSpawn = EditorGUILayout.Toggle(new GUIContent("Auto:", "Auto preview and auto spawning"), PreviewAutoSpawn);
                                if (PreviewAutoSpawn) AutoRefreshPreview = PreviewAutoSpawn;
                                EditorGUIUtility.labelWidth = 0;
                                GUILayout.Space(4);

                                if (generated != null)
                                    if (generated.Instantiated != null)
                                        if (generated.Instantiated.Count > 0)
                                            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Remove, "Clear Preview"), GUILayout.Width(26), GUILayout.Height(21))) ClearAllGeneratedGameObjects();


                                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Refresh, "Generate new grid and spawn objects"), GUILayout.Width(26), GUILayout.Height(21)))
                                {
                                    GenerateRefresh();
                                }

                            }

                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);
                            if (GenButtonsFoldout)
                            {

                                GUILayout.Space(5);
                                EditorGUILayout.BeginHorizontal();

                                if (gridMode != EDesignerGridMode.Paint)
                                {
                                    if (Seed == 0) if (GUILayout.Button("Randomize"))
                                        {
                                            ClearAllGeneratedGameObjects();
                                            GenerateBaseFieldGrid();
                                            TriggerRefresh(false);
                                        }
                                }
                                else
                                {
                                    if (Seed == 0) if (GUILayout.Button("Run Rules"))
                                        {
                                            TriggerRefresh(false);
                                        }
                                }

                                EditorGUIUtility.labelWidth = 85;

                                AutoRefreshPreview = EditorGUILayout.Toggle(new GUIContent("Auto Preview", "Automatically run all rules on grid every change, good for using AUTO SPAWN or PREVIEW (For PREVIEW : Scene Preview Settings -> Alpha set to higher than zero)"), AutoRefreshPreview);
                                if (AutoRefreshPreview) PreviewAutoSpawn = EditorGUILayout.Toggle(new GUIContent("Auto Spawn", "Automatically spawning objects every change occured. WARNING it can make your preview very slow when used on bigger preview grids with many objects to spawn, in such cases switch it off and use 'Run Spawners' button manually"), PreviewAutoSpawn);
                                EditorGUIUtility.labelWidth = 0;

                                if (!AutoRefreshPreview)
                                {
                                    if (GUILayout.Button(new GUIContent("Run Mods Rules")))
                                    {
                                        RunFieldCellsRules();
                                        if (AutoRefreshPreview) if (PreviewAutoSpawn) RunFieldCellsSpawningGameObjects();
                                    }
                                }

                                //if (AutoRefreshPreview)
                                if (GUILayout.Button("Run Spawners"))
                                {
                                    RunFieldCellsSpawningGameObjects();
                                }

                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginHorizontal();

                                if (GUILayout.Button(new GUIContent(" Generate New Grid and Spawn", FGUI_Resources.Tex_Refresh), GUILayout.Height(20)))
                                {
                                    GenerateRefresh();
                                }

                                GridPainter painter = null;
                                if (Selection.activeGameObject != null) painter = Selection.activeGameObject.GetComponent<GridPainter>();
                                if (painter || usedPainter) { if (painter) usedPainter = painter; if (GUILayout.Button("Painter - Generate")) TryGenerateWithGridPainter(usedPainter); /*if (painter.Generated.Count > 0) if (GUILayout.Button("Painter - Clear")) { painter.ClearGenerated(); usedPainter = true; }*/ } //else usedPainter = null;

                                EditorGUILayout.EndHorizontal();


                                if (generated != null)
                                    if (generated.Instantiated != null)
                                        if (generated.Instantiated.Count > 0)
                                            if (GUILayout.Button(new GUIContent(" Clear Generated", FGUI_Resources.Tex_Remove), GUILayout.Height(20))) ClearAllGeneratedGameObjects();

                                //if ( repaint)
                                //for (int m = 0; m < projectPreset.ModificatorPacks.Count; m++)
                                //{
                                //        if (projectPreset.ModificatorPacks[m] == null) continue;
                                //        SerializedObject spm = new SerializedObject(projectPreset.ModificatorPacks[m]);
                                //        spm.ApplyModifiedProperties();
                                //    }


                            }
                            else
                                GUILayout.Space(-7);

                            EditorGUILayout.EndVertical();


                            if (GenButtonsFoldout)
                            if (usedPainter)
                                {
                                EditorGUIUtility.labelWidth = 180;
                                applyChangesOnPainter = EditorGUILayout.Toggle("Apply changes on Grid Painter:", applyChangesOnPainter);
                                GUILayout.Space(5);
                                EditorGUIUtility.labelWidth = 0;
                            }

                            EditorGUILayout.EndVertical();
                        }
                        else
                        {
                            GUILayout.Space(2);
                            EditorGUILayout.BeginHorizontal();

                            if (GUILayout.Button(new GUIContent("  Re-Generate Objects", FGUI_Resources.Tex_Refresh), GUILayout.Height(22)))
                            {
                                GenerateRefresh();
                            }

                            GridPainter painter = null;
                            if (Selection.activeGameObject != null) painter = Selection.activeGameObject.GetComponent<GridPainter>();
                            if (painter || usedPainter) { if (painter) usedPainter = painter; if (GUILayout.Button("Painter - Generate")) { TryGenerateWithGridPainter(usedPainter); } } //else usedPainter = null;

                            if (generated != null)
                                if (generated.Instantiated != null)
                                    if (generated.Instantiated.Count > 0)
                                        if (GUILayout.Button(new GUIContent("  Clear Generated", FGUI_Resources.Tex_Remove), GUILayout.Height(22))) ClearAllGeneratedGameObjects();

                            EditorGUILayout.EndHorizontal();

                            if (usedPainter)
                            {
                                EditorGUIUtility.labelWidth = 180;
                                applyChangesOnPainter = EditorGUILayout.Toggle("Apply changes on Grid Painter:", applyChangesOnPainter);
                                EditorGUIUtility.labelWidth = 0;
                                GUILayout.Space(5);
                            }

                            EditorGUILayout.EndVertical();
                        }

                    }

                    GUILayout.Space(3);
                }

                GUILayout.Space(3);

                // Post Events
                FGUI_Inspector.FoldHeaderStart(ref PostEventsFoldout, "Field Setup Post Events", EditorStyles.helpBox);
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);
                if (PostEventsFoldout) DrawPostEvents(); else GUILayout.Space(-7);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                GUILayout.Space(4);
            }



            EditorGUILayout.EndScrollView();

            if (so_preset != null) if (so_preset.targetObject != null) so_preset.ApplyModifiedProperties();

            if (repaint)
            {
                SceneView.RepaintAll();
                repaint = false;
            }

            PGGUtils.EndVerticalIfLightSkin();
        }

        void GenerateRefresh()
        {
            if (UsePainterGenerating)
            {
                usedPainter.ClearGenerated();
                usedPainter.GenerateObjects();
            }
            else
            {
                IGeneration.ClearCells(grid);
                GenerateBaseFieldGrid();
                RunFieldCellsRules();
                RunFieldCellsSpawningGameObjects();
            }
        }

        void TryGenerateWithGridPainter(GridPainter painter)
        {
            if (painter == null) return;

            if (painter) usedPainter = painter;

            if (painter.FieldPreset == null) painter.FieldPreset = projectPreset;

            if (painter.GetAllPainterCells.Count <= 1)
            {
                painter.LoadCells();
                if (painter.GetAllPainterCells.Count <= 1) painter.GenerateDefaultCells(new Vector3Int(3, 0, 4));
            }

            usedPainter.LoadCells();
            usedPainter.GenerateObjects();
        }

        GridPainter usedPainter = null;
        bool applyChangesOnPainter = false;

        /// <summary>
        /// Repainting scene and grid refresh
        /// </summary>
        public void TriggerRefresh(bool refreshGrid = true)
        {

            if (AutoRefreshPreview)
            {
                if (UsePainterGenerating)
                {
                    GenerateRefresh();
                }
                else
                {

                    Get.ClearAllGeneratedGameObjects();
                    if (gridMode != EDesignerGridMode.Paint) if (refreshGrid) GenerateBaseFieldGrid();

                    RunFieldCellsRules();
                    if (PreviewAutoSpawn) RunFieldCellsSpawningGameObjects();
                }
            }

            SceneView.RepaintAll();
            repaint = true;
        }


    }
}