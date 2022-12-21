using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FIMSpace.FEditor;
using UnityEditor.Callbacks;
using FIMSpace.Generating.Planning;
using FIMSpace.Hidden;
using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;

namespace FIMSpace.Generating
{
    public partial class BuildPlannerWindow : EditorWindow
    {
        public PGGStartupReferences StartupRefs;
        public static BuildPlannerWindow Get;
        List<GameObject> generated;

        BuildPlannerPreset tempPreset;
        BuildPlannerPreset projectPreset;
        BuildPlannerPreset selectedPreset;
        [HideInInspector] public BuildPlannerPreset UsingDraft = null;
        private ScriptableObject wasChecked = null;
        private bool isInDefaultDirectory = false;

        SerializedObject so_preset;
        Vector2 mainScroll = Vector2.zero;
        bool repaint = true;
        bool drawPrevSettings = true;
        bool drawGeneratingPanel = true;
        bool drawPlanners = true;
        //int selectedPlanner = -2;
        int drawBuildVariables = -1;
        private FieldDesignWindow.EDrawVarMode drawVariablesMode = FieldDesignWindow.EDrawVarMode.All;
        FieldPlanner _selectField = null;
        int _selectBuildLayer = 0;
        bool AutoRefreshPreview = false;

        Color bgC;
        int seed = 0;


        [Tooltip("Separation distance between rooms walls to avoid rooms overlapping")]
        [Range(0f, .49f)]
        public float WallsSeparation = 0f;


        #region Utilities

        [MenuItem("Window/FImpossible Creations/PGG Build Planner (Build Layout)", false, 50)]
        [MenuItem("Window/FImpossible Creations/Level Design/Build Planner Window", false, 51)]
        public static void Init()
        {
            BuildPlannerWindow window = (BuildPlannerWindow)GetWindow(typeof(BuildPlannerWindow));
            window.titleContent = new GUIContent("Build Planner", Resources.Load<Texture>("SPR_Planner"/*"SPR_BuildPlanner"*/));
            window.Show();

            //Rect p = window.position;
            //p.size = new Vector2(700, 400);
            //window.position = p;

            if (window.tempPreset == null) window.tempPreset = CreateInstance<BuildPlannerPreset>();
            Get = window;
        }


        [OnOpenAssetAttribute(1)]
        public static bool OpenBuildPlannerScriptableFile(int instanceID, int line)
        {
            Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj as BuildPlannerPreset != null)
            {
                Init();
                Get.projectPreset = obj as BuildPlannerPreset;
                return true;
            }

            return false;
        }


        #region Textures


        #endregion


        #endregion


        public static bool ForceAutoUpdate = false;
        private bool generatingInProgress = false;
        private static bool? PreDebugAsync = null;
        private bool _drawPresetParams = false;
        void FlagNodeGraphsForUpdateDisplay()
        {
            PlannerGraphWindow._RefreshDrawFlag = EditorApplication.timeSinceStartup;
        }

        void OnInspectorUpdate()
        {
            if (DebugStepPlay)
            {
                Repaint();
            }
            else
            {
                //if (projectPreset)
                //{
                //    if (projectPreset.IsGeneratingDone == false)
                //    {
                //        Repaint();
                //    }
                //}
            }
        }

        private void Update()
        {
            if (projectPreset)
            {
                projectPreset.UpdateGenerating();

                if (generatingInProgress)
                {
                    GeneratedPreview = projectPreset.LatestGenerated;
                    repaint = true;
                    Repaint();
                    //UnityEngine.Debug.Log("progr");

                    if (projectPreset.IsGeneratingDone)
                    {

                        if (PlannerRuleBase.Debugging)
                        {
                            if (PreDebugAsync != null) { projectPreset.AsyncGenerating = PreDebugAsync.Value; PreDebugAsync = null; }

                            Get.projectPreset.OnIteractionCallback = null;
                            repaint = true;
                            PlannerRuleBase.Debugging = false;
                            //ForceUpdateView();

                            _watch.Stop();
                            _lastestGenMs = _watch.ElapsedMilliseconds;
                        }

                        generatingInProgress = false;
                        FlagNodeGraphsForUpdateDisplay();
                        repaint = true;
                        //UnityEngine.Debug.Log("done");
                    }
                }
            }
        }

        public void OnGUI()
        {
            PGGUtils.SetDarkerBacgroundOnLightSkin();

            if (Get == null) Init();

            if (Event.current.type == EventType.Layout)
            {
                UseEditorEvents();

                if (_selectField != null)
                {
                    AssetDatabase.OpenAsset(_selectField);
                    _selectField = null;
                }
            }

            mainScroll = EditorGUILayout.BeginScrollView(mainScroll);

            //if (FieldPlannerWindow.Get) if (FieldPlannerWindow.LatestFieldPlanner == null)
            //    {
            //        if (projectPreset != null)
            //            if (selectedPlanner >= 0)
            //                if (projectPreset.BasePlanners.Count > 0)
            //                {
            //                    int id = selectedPlanner;
            //                    if (selectedPlanner < projectPreset.BasePlanners.Count)
            //                        FieldPlannerWindow.LatestFieldPlanner = projectPreset.BasePlanners[id];
            //                }
            //    }


            #region Preparations and headers

            bgC = GUI.backgroundColor;

            Get = this;
            if (projectPreset == null) selectedPreset = tempPreset;
            else selectedPreset = projectPreset;
            if (selectedPreset != null) so_preset = new SerializedObject(selectedPreset);

            if (generated == null) generated = new List<GameObject>();

            #endregion

            #region Preview Generating Settings

            GUILayout.Space(3);
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            string fold = drawPrevSettings ? "  ▼" : "  ►";
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(fold + "  " + "Preview Settings", FGUI_Resources.Tex_Preview), EditorStyles.label, GUILayout.Height(19), GUILayout.MaxWidth(200))) drawPrevSettings = !drawPrevSettings;
            //if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Preview), EditorStyles.label, GUILayout.Width(20))) drawPrevSettings = !drawPrevSettings;
            GUILayout.FlexibleSpace();

            // Center Button
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null)
                if (view.camera != null)
                {
                    float referenceScale = 11f;
                    if (projectPreset != null) referenceScale = 11;

                    if (Vector3.Distance(view.camera.transform.position, new Vector3(0, referenceScale, -referenceScale)) > referenceScale)
                    {
                        if (GUILayout.Button(new GUIContent(" Center", EditorGUIUtility.IconContent("Camera Icon").image), FGUI_Resources.ButtonStyle, GUILayout.Height(19)))
                        {
                            FieldDesignWindow.FrameCenter(referenceScale, false, -0.15f, 0.1f, 70f);
                        }
                    }

                    float angleDiff = Quaternion.Angle(view.camera.transform.rotation, Quaternion.identity);

                    if (angleDiff > 125)
                    {
                        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("RotateTool").image), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                        {
                            FieldDesignWindow.FrameCenter(referenceScale, true, -0.18f, 0.1f, 70f);
                        }
                    }
                }

            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Info, "Click to display info about the Build Planner Window"), EditorStyles.label, GUILayout.Width(16), GUILayout.Height(16)))
            {
                EditorUtility.DisplayDialog("Build Planner Window", "Build Planner Window is dedicated for setting up, designing and debugging areas to generate objects on.\n\nBuild Planner Window works independently from FieldSetups.\n\nTo make final use of the Build Planner for your game project, you need to use 'Build Planner Executor' component.", "OK");
            }

            EditorGUILayout.EndHorizontal();

            if (drawPrevSettings)
            {
                GUILayout.Space(5f);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                seed = EditorGUILayout.IntField("Fixed Seed: ", seed);
                if (seed == 0) EditorGUILayout.LabelField("(0 is Random)", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(69));

                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Random, "Add 1 to seed for controlled generating checking"), FGUI_Resources.ButtonStyle, GUILayout.Width(20), GUILayout.Height(18)))
                {
                    seed += 1;
                    ClearGeneration();
                    RunGeneration();
                    FlagNodeGraphsForUpdateDisplay();
                }

                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = 0;
                if (EditorGUI.EndChangeCheck()) { }


                if (so_preset != null)
                {
                    GUILayout.Space(4);
                    EditorGUIUtility.labelWidth = 0;
                    EditorGUILayout.BeginHorizontal();
                    //EditorGUILayout.PropertyField(so_preset.FindProperty("UseBoundsWorkflow"));
                    EditorGUILayout.PropertyField(so_preset.FindProperty("AsyncGenerating"));
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(4);
                }

            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(3);

            #endregion


            GUILayout.Space(3);


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
                        if (StartupRefs.FieldPlannerDraftsDirectory)
                        {
                            string qPath = AssetDatabase.GetAssetPath(StartupRefs.FieldPlannerDraftsDirectory);
                            string sPath = AssetDatabase.GetAssetPath(projectPreset);
                            qPath = System.IO.Path.GetFileName(qPath);
                            sPath = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(sPath));
                            if (sPath.Contains(qPath)) isInDefaultDirectory = true;
                        }
                }
            }

            #endregion

            #region Preset Field

            Color preBGCol = GUI.backgroundColor;

            // Build plan preset field
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            EditorGUILayout.BeginHorizontal();

            Texture foldTex = _drawPresetParams ? FGUI_Resources.Tex_DownFoldGray : FGUI_Resources.Tex_RightFoldGray;

            if (GUILayout.Button(new GUIContent(" Preset:", foldTex), EditorStyles.label, GUILayout.Height(19), GUILayout.Width(70)))
            {
                _drawPresetParams = !_drawPresetParams;
            }

            projectPreset = (BuildPlannerPreset)EditorGUILayout.ObjectField(projectPreset, typeof(BuildPlannerPreset), false);
            bool wasAutoFileButtons = false;

            if (StartupRefs)
                if (StartupRefs.FieldPlannerDraftsDirectory)
                {
                    wasAutoFileButtons = true;

                    #region Button to display menu of draft field setup files

                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_DownFold, "Display quick selection menu for FieldSetups contained in the drafts directory"), EditorStyles.label, GUILayout.Width(18), GUILayout.Height(16)))
                    {
                        string path = AssetDatabase.GetAssetPath(StartupRefs.FieldPlannerDraftsDirectory);
                        var files = System.IO.Directory.GetFiles(path, "*.asset");
                        if (files != null)
                        {
                            GenericMenu draftsMenu = new GenericMenu();

                            draftsMenu.AddItem(new GUIContent("None"), projectPreset == null, () => { projectPreset = null; });

                            for (int i = 0; i < files.Length; i++)
                            {
                                BuildPlannerPreset fs = AssetDatabase.LoadAssetAtPath<BuildPlannerPreset>(files[i]);
                                if (fs) draftsMenu.AddItem(new GUIContent(fs.name), projectPreset == fs, () => { projectPreset = fs; });
                            }

                            draftsMenu.ShowAsContext();
                        }
                    }


                    #endregion

                    if (projectPreset != null)
                        ModificatorsPackEditor.DrawRenameScriptableButton(projectPreset);

                    if (projectPreset == null) GUI.backgroundColor = Color.green;
                    else GUI.backgroundColor = new Color(0.75f, 1f, 0.75f, 1f);

                    if (GUILayout.Button(new GUIContent("New", "Generates new Planner Setup file in the PGG drafts directory, after that you can move draft file into project directories with 'Move' button which will appear"), GUILayout.Width(44)))
                    {
                        string path;

                        path = AssetDatabase.GetAssetPath(StartupRefs.FieldPlannerDraftsDirectory);
                        var files = System.IO.Directory.GetFiles(path, "*.asset");
                        path += "/FS_NewFieldSetup" + (files.Length + 1) + ".asset";

                        BuildPlannerPreset scrInstance = CreateInstance<BuildPlannerPreset>();

                        if (string.IsNullOrEmpty(path))
                            path = FGenerators.GenerateScriptablePath(scrInstance, "FS_NewFieldSetup");

                        if (!string.IsNullOrEmpty(path))
                        {
                            UnityEditor.AssetDatabase.CreateAsset(scrInstance, path);
                            AssetDatabase.SaveAssets();
                            projectPreset = scrInstance;
                        }

                        //projectPreset = (FieldSetup)FGenerators.GenerateScriptable(CreateInstance<FieldSetup>(), "FS_");
                    }

                    GUI.backgroundColor = preBGCol;

                    if (GUILayout.Button(FGUI_Resources.GUIC_More, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(19)))
                    {

                        GenericMenu draftsMenu = new GenericMenu();

                        #region Move Preset Options

                        if (isInDefaultDirectory)
                        {
                            if (projectPreset != null)
                                draftsMenu.AddItem(new GUIContent("Move current preset to choosed game project directory"), false, () =>
                                {

                                    string path = FGenerators.GetPathPopup("Move 'Field Setup' file to new directory in project", "FS_Your FieldSetup");
                                    if (!string.IsNullOrEmpty(path))
                                    {
                                        UnityEditor.AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(projectPreset), path);
                                        AssetDatabase.SaveAssets();
                                    }

                                    wasChecked = null;

                                });
                        }
                        else
                        {
                            if (projectPreset != null)
                                draftsMenu.AddItem(new GUIContent("Move preset file from project to default PGG Drafts directory"), false, () =>
                                {

                                    string path = AssetDatabase.GetAssetPath(StartupRefs.FieldPlannerDraftsDirectory);

                                    if (!string.IsNullOrEmpty(path))
                                    {
                                        path += "/" + projectPreset.name + ".asset";
                                        UnityEditor.AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(projectPreset), path);
                                        AssetDatabase.SaveAssets();
                                    }

                                    wasChecked = null;

                                });
                        }

                        #endregion

                        draftsMenu.AddItem(new GUIContent("+ Create new Build Planner Preset in selected project directory"), false, () =>
                        {
                            var newPres = (BuildPlannerPreset)FGenerators.GenerateScriptable(CreateInstance<BuildPlannerPreset>(), "BP_");
                            if (newPres != null)
                            {
                                if (AssetDatabase.Contains(newPres))
                                    projectPreset = newPres;
                            }
                        });


                        draftsMenu.ShowAsContext();
                    }
                }


            if (wasAutoFileButtons)
            {
                //if (GUILayout.Button(new GUIContent("+", "Create new Build Planner Preset in selected project directory"), GUILayout.Width(24)))
                //{
                //    var newPres = (BuildPlannerPreset)FGenerators.GenerateScriptable(CreateInstance<BuildPlannerPreset>(), "BP_");
                //    if (newPres != null)
                //    {
                //        if (AssetDatabase.Contains(newPres))
                //            projectPreset = newPres;
                //    }
                //}
            }
            else if (GUILayout.Button("Create New", GUILayout.Width(94)))
            {
                var newPres = (BuildPlannerPreset)FGenerators.GenerateScriptable(CreateInstance<BuildPlannerPreset>(), "BP_");
                if (newPres != null)
                {
                    if (AssetDatabase.Contains(newPres))
                        projectPreset = newPres;
                }
            }

            EditorGUILayout.EndHorizontal();

            if (projectPreset)
                if (_drawPresetParams)
                {
                    GUILayout.Space(3);
                    EditorGUILayout.LabelField("Custom Info:", EditorStyles.centeredGreyMiniLabel);

                    EditorGUI.BeginChangeCheck();
                    projectPreset.CustomInfo = EditorGUILayout.TextArea(projectPreset.CustomInfo);
                    if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(projectPreset);
                }

            EditorGUILayout.EndVertical();

            #endregion


            GUILayout.Space(7);

            if (projectPreset != null)
            {
                FieldDesignWindow.DrawFieldVariablesList(projectPreset.BuildVariables, FGUI_Resources.BGInBoxStyle, "Build Variables", ref drawBuildVariables, ref drawVariablesMode, Get, false);

                if (projectPreset.BuildLayers.Count > 1)
                {
                    DrawFieldLayersList();
                }

                _selectField = DrawFieldPlannersList(projectPreset.GetPlanners(_selectBuildLayer), projectPreset, FGUI_Resources.BGInBoxStyle, "Field Planners", ref drawPlanners);

                if (projectPreset.AsyncGenerating)
                    if (projectPreset.IsGenerating && projectPreset.IsGeneratingDone == false)
                    {
                        if (projectPreset.GeneratingProgress < 1f)
                        {
                            var barRect = GUILayoutUtility.GetLastRect();
                            float progr = projectPreset.GeneratingProgressSmooth;
                            barRect.width = barRect.width * progr;
                            barRect.height = 2;
                            float aMul = 1f;
                            if (progr < 0.25f) aMul = Mathf.InverseLerp(0f, 0.25f, progr);
                            if (progr > 0.8f) aMul = Mathf.InverseLerp(0.8f, 1f, progr);

                            GUI.color = new Color(0.1f, 1f, 0.1f, 0.3f * aMul);
                            GUI.Box(barRect, GUIContent.none, FGUI_Resources.BGInBoxStyleH);
                            GUI.color = preBGCol;
                        }
                    }
            }

            #region Generating Buttons

            GUILayout.Space(3);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            FGUI_Inspector.FoldHeaderStart(ref drawGeneratingPanel, "Generating Buttons", null);

            GUILayout.Space(7);
            GUILayout.FlexibleSpace();
            EditorGUIUtility.labelWidth = 85;
            AutoRefreshPreview = EditorGUILayout.Toggle(new GUIContent("Auto Preview:", "Automatically run all planners procedures every change in node ports connections"), AutoRefreshPreview);
            EditorGUIUtility.labelWidth = 0;
            GUILayout.Space(5);

            GUILayout.EndHorizontal();

            if (drawGeneratingPanel)
            {
                if (projectPreset == null)
                {
                    EditorGUILayout.HelpBox(" Select Build Planner Preset First!", MessageType.Info);
                }
                else
                {

                    GUILayout.Space(3);

                    EditorGUILayout.BeginHorizontal();

                    GUILayout.Space(7);
                    GUI.backgroundColor = new Color(0.7f, 0.82f, 1f);
                    if (GUILayout.Button(new GUIContent("  Next Preview", FGUI_Resources.Tex_Refresh), GUILayout.Height(20), GUILayout.MinWidth(96)))
                    {
                        ClearGeneration();
                        RunGeneration();
                    }
                    GUI.backgroundColor = preBGCol;

                    GUILayout.Space(2);
                    if (GUILayout.Button(new GUIContent("  Step-Debug", FGUI_Resources.Tex_Debug), GUILayout.Height(20), GUILayout.MinWidth(94)))
                    {
                        ClearGeneration();
                        RunDebuggedGeneration();
                    }

                    if (GeneratedPreview != null)
                    {
                        GUILayout.Space(2);
                        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Remove, "Clear the layout preview"), GUILayout.Height(20), GUILayout.MinWidth(36)))
                        {
                            ClearGeneration();
                        }
                    }

                    //GUILayout.Space(7);
                    //GUILayout.FlexibleSpace();
                    //EditorGUIUtility.labelWidth = 85;
                    //AutoRefreshPreview = EditorGUILayout.Toggle(new GUIContent("Auto Preview:", "Automatically run all planners procedures every change"), AutoRefreshPreview);
                    //EditorGUIUtility.labelWidth = 0;
                    //GUILayout.Space(5);

                    GUILayout.Space(7);
                    EditorGUILayout.EndHorizontal();

                    if (Selection.activeGameObject)
                    {
                        BuildPlannerExecutor exec = Selection.activeGameObject.GetComponent<BuildPlannerExecutor>();
                        if (exec)
                        {
                            GUILayout.Space(3);

                            if (GUILayout.Button(" Call 'Generate' on the Executor (inspector needed) ", GUILayout.Height(20)))
                            {
                                exec.Generate();
                            }

                            if (exec.Generated.Count > 0)
                            {
                                if (GUILayout.Button(" Call 'Clear' on the selected Executor ", GUILayout.Height(20)))
                                {
                                    exec.ClearGenerated();
                                }
                            }
                        }
                    }

                }

                GUILayout.Space(4f);
            }

            GUILayout.Space(3);
            EditorGUILayout.EndVertical();

            GUILayout.Space(3);

            #endregion

            EditorGUILayout.EndScrollView();

            if (projectPreset)
                if (ForceAutoUpdate || projectPreset._Editor_GraphNodesChangedForced)
                {
                    ForceAutoUpdate = false;

                    if (AutoRefreshPreview || projectPreset._Editor_GraphNodesChangedForced)
                    {
                        projectPreset._Editor_GraphNodesChangedForced = false;
                        ClearGeneration();
                        RunGeneration();
                        repaint = true;
                    }
                }


            #region Ending, applying and restoring

            if (so_preset != null) so_preset.ApplyModifiedProperties();

            if (repaint)
            {
                SceneView.RepaintAll();
                repaint = false;
                FieldPlannerWindow.RefreshGraphView();
            }


            #endregion


            if (SceneView.lastActiveSceneView)
            {
#if UNITY_2019_4_OR_NEWER
                if (SceneView.lastActiveSceneView.drawGizmos == false)
                {
                    EditorGUILayout.HelpBox("GIZMOS are disabled on the scene view and preview is not visible!", MessageType.Info);
                    if (GUILayout.Button("Enable Gizmos on Scene View")) SceneView.lastActiveSceneView.drawGizmos = true;
                }
#endif
            }

            PGGUtils.EndVerticalIfLightSkin();
        }


        #region Triggering Generating

        public PlanGenerationPrint GeneratedPreview = null;
        public bool DebugStepPlay = false;
        public int DebugStep = 0;
        public List<PlanGenerationPrint> GeneratedDebugSteps = new List<PlanGenerationPrint>();

        public PlanGenerationPrint DisplayedPrint
        {
            get
            {
                if (DebugStep <= -1) return GeneratedPreview;
                if (DebugStep >= GeneratedDebugSteps.Count) return GeneratedPreview;
                if (GeneratedDebugSteps.Count > 0) return GeneratedDebugSteps[DebugStep];
                return GeneratedPreview;
            }
        }

        public void ClearGeneration()
        {
            PlanGenerationPrint.SelectedPlannerResult = null;

            if (PreDebugAsync != null) { projectPreset.AsyncGenerating = PreDebugAsync.Value; PreDebugAsync = null; }
            PlannerRuleBase.Debugging = false;
            if (projectPreset) projectPreset.ClearGeneration();
            GeneratedDebugSteps.Clear();
            GeneratedPreview = null;
            repaint = true;
        }

        int GetTargetSeed()
        {
            int tgtSeed = seed;
            if (seed == 0) tgtSeed = Random.Range(-1000, 1000);
            return tgtSeed;
        }

        public void RunGeneration()
        {
            generatingInProgress = true;
            PlannerRuleBase.Debugging = false;
            Get.projectPreset.OnIteractionCallback = null;
            _watch.Stop();

            for (int g = 0; g < Get.projectPreset.BasePlanners.Count; g++)
            {
                Get.projectPreset.BasePlanners[g].ParentBuildPlanner = Get.projectPreset;
            }

            Get.projectPreset.RunProceduresAndGeneratePrint(GetTargetSeed());
        }


        public static double _lastestGenMs = 0;
        public static System.Diagnostics.Stopwatch _watch = new System.Diagnostics.Stopwatch();

        public void RunDebuggedGeneration()
        {
            _watch.Reset();
            _watch.Start();

            PlannerRuleBase.Debugging = true;
            DebugStepPlay = false;
            DebugStep = -1;
            Get.projectPreset.OnIteractionCallback = OnPrintIteration;

            PreDebugAsync = projectPreset.AsyncGenerating;
            projectPreset.AsyncGenerating = false;

            generatingInProgress = true;
            Get.projectPreset.RunProceduresAndGeneratePrint(GetTargetSeed());
        }

        public void OnPrintIteration(PlanGenerationPrint print)
        {
            GeneratedDebugSteps.Add(print.Copy());
        }

        #endregion


        #region Drawing GUI

        bool _layersOrderFoldout = false;
        public void DrawFieldLayersList()
        {

            Color bgc = GUI.backgroundColor;
            Color preC = GUI.color;

            GUI.color = Color.yellow;
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            GUI.color = bgc;

            EditorGUILayout.BeginHorizontal();
            string fold = _layersOrderFoldout ? " ▼" : " ►";
            if (GUILayout.Button(fold + "  Build Layers Execution Order (" + projectPreset.BuildLayers.Count + ")", EditorStyles.label)) _layersOrderFoldout = !_layersOrderFoldout;
            if (_layersOrderFoldout) { if (GUILayout.Button("+ Layer", GUILayout.Width(72))) { AddNewBuildLayer(); } }
            EditorGUILayout.EndHorizontal();

            if (_layersOrderFoldout)
            {
                int toRemove = -1;
                for (int i = 0; i < projectPreset.BuildLayers.Count; i++)
                {
                    BuildPlannerPreset.BuildPlannerLayer layer = projectPreset.BuildLayers[i];
                    EditorGUILayout.BeginHorizontal();
                    if (_selectBuildLayer == i) GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("" + i, GUILayout.Width(20))) { _selectBuildLayer = i; }
                    if (_selectBuildLayer == i) GUI.backgroundColor = preC;

                    GUILayout.Space(6);
                    EditorGUIUtility.labelWidth = 46;
                    layer.Name = EditorGUILayout.TextField("Name: ", layer.Name);
                    EditorGUIUtility.labelWidth = 0;
                    GUILayout.Space(6);

                    EditorGUI.BeginChangeCheck();
                    if (i > 0) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowUp, "Move this element to be executed before above one"), GUILayout.Width(24))) { BuildPlannerPreset.BuildPlannerLayer temp = projectPreset.BuildLayers[i - 1]; projectPreset.BuildLayers[i - 1] = projectPreset.BuildLayers[i]; projectPreset.BuildLayers[i] = temp; }
                    if (i < projectPreset.BuildLayers.Count - 1) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowDown, "Move this element to be executed after below one"), GUILayout.Width(24))) { BuildPlannerPreset.BuildPlannerLayer temp = projectPreset.BuildLayers[i + 1]; projectPreset.BuildLayers[i + 1] = projectPreset.BuildLayers[i]; projectPreset.BuildLayers[i] = temp; }
                    if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(projectPreset);

                    GUILayout.Space(6);



                    if (projectPreset.BuildLayers.Count > 1)
                    {
                        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 1f);
                        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Remove), FGUI_Resources.ButtonStyle, GUILayout.Width(19), GUILayout.Height(19))) { toRemove = i; }
                        GUI.backgroundColor = preC;
                    }


                    EditorGUILayout.EndHorizontal();

                }

                if (toRemove >= 0)
                    if (projectPreset.BuildLayers.Count > 1)
                        projectPreset.BuildLayers.RemoveAt(toRemove);
            }

            GUILayout.EndVertical();

        }


        public FieldPlanner DrawFieldPlannersList(List<FieldPlanner> toDraw, BuildPlannerPreset planPreset, GUIStyle style, string title, ref bool foldout, bool moveButtons = true)
        {
            if (toDraw == null) return null;
            FieldPlanner tosel = null;
            int selected = -1;

            Color bgc = GUI.backgroundColor;
            Color preC = GUI.color;

            GUI.color = Color.green;
            EditorGUILayout.BeginVertical(style);
            GUI.color = bgc;

            EditorGUILayout.BeginHorizontal();
            string fold = foldout ? " ▼" : " ►";
            if (GUILayout.Button(fold + "  " + title + " (" + toDraw.Count + ")", EditorStyles.label, GUILayout.Width(130))) foldout = !foldout;

            if (foldout)
            {

                GUILayout.FlexibleSpace();

                if (projectPreset.BuildLayers.Count > 1)
                {
                    _selectBuildLayer = EditorGUILayout.IntPopup(_selectBuildLayer, projectPreset.GetLayersNameList(), projectPreset.GetLayersIDList());
                }

                // TODO: BuildLayersImplementation
                //if (GUILayout.Button(ModificatorsPackEditor._UtilIcon, EditorStyles.label, GUILayout.Width(22), GUILayout.Height(22)))
                //{
                //    GenericMenu menu = new GenericMenu();

                //    menu.AddItem(new GUIContent("+ Add New Build Plan Layer"), false, () =>
                //    {
                //        AddNewBuildLayer();
                //    });

                //    menu.ShowAsContext();
                //}

                if (GUILayout.Button(FGUI_Resources.GUIC_Info, EditorStyles.label, GUILayout.Width(17)))
                {
                    EditorUtility.DisplayDialog("Field Planners", "Each 'Field Planner' (FP) can be different  interior area/room  you place on build plan.\n\nEach FP can be like the Setup, for multiple rooms of same type (use 'Instances' for multiple copies of FP on the Build Plan).\nYou can create relations, align each FP to each other creating layout you need in your project.\n\nThere also will be additional features for FP as defining area to execute other build planner preset on (like defining house building area with simple shape and then triggering on it complex build plan setup)", " Ok");
                }

                GUILayout.Space(4);

                GUI.backgroundColor = Color.green;
                if (GUILayout.Button(new GUIContent("+", "Adding new FieldPlanner to selected Layer")))
                {
                    AddNewPlanner(planPreset, "", _selectBuildLayer);
                }

                GUI.backgroundColor = bgC;
            }

            EditorGUILayout.EndHorizontal();

            if (foldout)
            {
                GUILayout.Space(4);
                Color preBg = GUI.backgroundColor;

                if (toDraw.Count > 0)
                {
                    for (int i = 0; i < toDraw.Count; i++)
                    {
                        FieldPlanner draw = toDraw[i];

                        if (draw.PreparationWasStarted && draw.PreparationDone == false)
                        {
                            GUI.color = new Color(0.35f, 1f, 0.1f, 1f);
                        }
                        else if (draw.ExecutionWasStarted && draw.ExecutionDone == false)
                        {
                            GUI.color = new Color(0.1f, 1f, 0.1f, 1f);
                        }

                        Color inPreC = GUI.color;

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.BeginHorizontal();

                        if (draw != null)
                        {
                            EditorGUI.BeginChangeCheck();
                            toDraw[i].DisableWholePlanner = !EditorGUILayout.Toggle(!toDraw[i].DisableWholePlanner, GUILayout.Width(16));
                            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(toDraw[i]);
                            if (toDraw[i].DisableWholePlanner) GUI.color = new Color(1f, 1f, 1f, 0.5f); else GUI.color = inPreC;
                        }

                        GUIContent lbl = new GUIContent(i.ToString());
                        float wdth = EditorStyles.label.CalcSize(lbl).x;

                        //EditorGUILayout.LabelField(lbl, );
                        if (selected == -1)
                            if (FieldPlannerWindow.Get)
                                if (FieldPlannerWindow.LatestFieldPlanner == toDraw[i])
                                    selected = i;

                        if (selected == i) GUI.backgroundColor = Color.green;
                        //bool runChange = false;

                        Color plannerColor = PlanGenerationPrint.GeneratePlannerColor(i);

                        if (GUILayout.Button(lbl, GUILayout.Width(wdth + 8)))
                        {
                            //if (Selection.activeObject is ModificatorsPack) Selection.activeObject = null;

                            if (selected == i) selected = -1;
                            else
                            {
                                //selected = i;
                                //if (toDraw[i]) AssetDatabase.OpenAsset(toDraw[i]);
                                if (toDraw[i]) tosel = (toDraw[i]);
                                FieldPlannerEditor.Editor_ResetNameEdit();
                                //runChange = true;
                            }
                        }

                        GUILayout.Space(3);

                        GUI.backgroundColor = plannerColor;
                        bool preE = GUI.enabled;

                        if (GUILayout.Button(PGGUtils._PlannerIcon, EditorStyles.label, GUILayout.Width(22), GUILayout.Height(19)))
                        {
                            //selected = i;
                            if (toDraw[i]) tosel = (toDraw[i]);
                            FieldPlannerEditor.Editor_ResetNameEdit();
                        }

                        GUI.color = plannerColor;

                        if (GUILayout.Button(PGGUtils._CellIcon, EditorStyles.label, GUILayout.Width(24), GUILayout.Height(18)))
                        {
                            //
                            //selected = i;
                            if (toDraw[i]) tosel = (toDraw[i]);
                            FieldPlannerEditor.Editor_ResetNameEdit();
                        }

                        GUI.color = preC;

                        EditorGUI.BeginChangeCheck();

                        //string contrNme = "pL" + i; GUI.SetNextControlName(contrNme);
                        toDraw[i].name = EditorGUILayout.TextField(toDraw[i].name);

                        //if (GUI.GetNameOfFocusedControl() == contrNme)
                        //{
                        //    if (selected != i)
                        //    {
                        //        selected = i;
                        //        if (toDraw[i]) tosel = (toDraw[i]);
                        //        FieldPlannerEditor.Editor_ResetNameEdit();
                        //    }
                        //}

                        //toDraw[i] = (FieldPlanner)EditorGUILayout.ObjectField(toDraw[i], typeof(FieldPlanner), false);
                        if (EditorGUI.EndChangeCheck())
                        {
                            //if (selected != i)
                            //{
                            //    selected = i;
                            //    if (toDraw[i]) tosel = (toDraw[i]);
                            //    FieldPlannerEditor.Editor_ResetNameEdit();
                            //}

                            EditorUtility.SetDirty(planPreset);
                        }

                        GUI.backgroundColor = preC;

                        if (moveButtons)
                        {
                            EditorGUI.BeginChangeCheck();
                            if (i > 0) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowUp, "Move this element to be executed before above one"), GUILayout.Width(24))) { FieldPlanner temp = toDraw[i - 1]; toDraw[i - 1] = toDraw[i]; toDraw[i] = temp; }
                            if (i < toDraw.Count - 1) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowDown, "Move this element to be executed after below one"), GUILayout.Width(24))) { FieldPlanner temp = toDraw[i + 1]; toDraw[i + 1] = toDraw[i]; toDraw[i] = temp; }

                            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(planPreset);
                        }

                        //ModificatorsPackEditor.DrawRenameScriptableButton(toDraw[i], true);

                        GUILayout.Space(2);
                        toDraw[i]._EditorGUI_DrawVisibilitySwitchButton();
                        GUILayout.Space(2);

                        GUI.backgroundColor = new Color(1f, 0.525f, 0.525f, 1f);
                        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Remove), FGUI_Resources.ButtonStyle, new GUILayoutOption[] { GUILayout.Height(19), GUILayout.Width(24) })) { toDraw.RemoveAt(i); EditorUtility.SetDirty(planPreset); AssetDatabase.SaveAssets(); ClearUnused(planPreset); break; }
                        GUI.backgroundColor = bgc;
                        GUI.color = inPreC;
                        EditorGUILayout.EndHorizontal();

                        //if (runChange) FieldPlannerWindow.forceChanged = true;

                        GUI.color = preBg;
                    }

                    if (FieldPlannerWindow.Get == null)
                    {
                        GUILayout.Space(5);
                        EditorGUILayout.HelpBox("Select any of the Field Planners to edit it in new window", MessageType.Info);
                        GUILayout.Space(3);
                    }

                }
                else
                {
                    GUILayout.Space(3);
                    GUI.backgroundColor = Color.green;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(8);
                    if (GUILayout.Button(" + Add first Field Planner + ")) { AddNewPlanner(planPreset, "First Field Planner"); }
                    GUILayout.Space(8);
                    EditorGUILayout.EndHorizontal();
                    GUI.backgroundColor = preBg;
                    GUILayout.Space(3);
                    //EditorGUILayout.LabelField("No object in list", EditorStyles.centeredGreyMiniLabel);
                }
            }

            EditorGUILayout.EndVertical();

            return tosel;
        }


        public void AddNewPlanner(BuildPlannerPreset planner, string targetName = "", int addToLayer = 0)
        {
            so_preset.Update();
            FieldPlanner temp = ScriptableObject.CreateInstance<FieldPlanner>();

            if (string.IsNullOrEmpty(targetName)) targetName = "New Field Planner";
            temp.name = targetName;

            temp.hideFlags = HideFlags.HideInHierarchy;
            temp.ParentBuildPlanner = planner;
            FGenerators.AddScriptableTo(temp, planner, false, false);


            planner.BuildLayers[addToLayer].FieldPlanners.Add(temp);

            EditorUtility.SetDirty(temp);

            so_preset.ApplyModifiedProperties();

            EditorUtility.SetDirty(planner);
            //AssetDatabase.SaveAssets();

        }


        public void AddNewBuildLayer()
        {
            so_preset.Update();

            BuildPlannerPreset.BuildPlannerLayer nLayer = new BuildPlannerPreset.BuildPlannerLayer();
            nLayer.Name = "Build Layer " + (projectPreset.BuildLayers.Count + 1);
            projectPreset.BuildLayers.Add(nLayer);

            so_preset.ApplyModifiedProperties();
            EditorUtility.SetDirty(projectPreset);
        }


        public void ClearUnused(BuildPlannerPreset planner)
        {
            if (AssetDatabase.Contains(planner) == false) return;

            string path = AssetDatabase.GetAssetPath(planner);
            if (string.IsNullOrEmpty(path)) return;

            //List<UnityEngine.Object> used = new List<UnityEngine.Object>();

            //UnityEngine.Debug.Log("TODO To Update Removing");
            //for (int i = 0; i < planner.Planners.Count; i++)
            //{
            //    FieldPlanner fPlan = planner.Planners[i];
            //    if (fPlan == null) continue;
            //    used.Add(fPlan);
            //    if (fPlan.ShapeGenerator) used.Add(fPlan.ShapeGenerator);
            //    for (int r = 0; r < fPlan.FProcedures.Count; r++) used.Add(fPlan.FProcedures[r]);
            //    for (int r = 0; r < fPlan.FPostProcedures.Count; r++) used.Add(fPlan.FPostProcedures[r]);
            //}

            //var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

            //for (int i = 0; i < assets.Length; i++)
            //{
            //    assets[i].hideFlags = HideFlags.None;
            //    if (used.Contains(assets[i]) == false) GameObject.DestroyImmediate(assets[i], true);
            //    else
            //        assets[i].hideFlags = HideFlags.HideInHierarchy;
            //}

            Get.projectPreset.ClearUnusedAssetsInside(false, false);

            //AssetDatabase.SaveAssets();
        }


        #endregion



        public static void AddEditorEvent(System.Action ac)
        {
            EditorEvents.Add(ac);
        }

        static List<System.Action> EditorEvents = new List<System.Action>();
        public static void UseEditorEvents()
        {
            for (int i = 0; i < EditorEvents.Count; i++)
            {
                if (EditorEvents[i] != null) EditorEvents[i].Invoke();
            }

            EditorEvents.Clear();
        }

    }
}