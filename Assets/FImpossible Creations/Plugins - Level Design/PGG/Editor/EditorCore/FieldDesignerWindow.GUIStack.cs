using UnityEngine;
using UnityEditor;
using FIMSpace.FEditor;
using FIMSpace.Hidden;

namespace FIMSpace.Generating
{
    public partial class FieldDesignWindow
    {
        enum EDesignerGridMode { RectangleGrid, RandomGenerate, Paint, BranchedGeneration }
        EDesignerGridMode gridMode = EDesignerGridMode.RectangleGrid;

        bool drawScenePreviewSetts = false;
        bool drawPreviewSetts = false;
        bool drawTestGenSetts = false;
        bool drawPresetParams = true;
        bool drawInstructions = false;
        //bool drawSelfInj = false;
        int drawVariables = -1;
        public enum EDrawVarMode { All, Variables, Materials, GameObjects }
        EDrawVarMode drawVariablesMode = EDrawVarMode.All;
        bool drawPacks = false;
        bool drawPack = true;
        bool drawUtilMods = false;
        int selectedPackIndex = 0;

        [HideInInspector]
        public FieldSetup UsingDraft = null;
        static int variablesPage = 0;

        #region Textures

        int latestFilesInDraft = 0;

        #endregion

        void DrawFieldGenWindowGUI(FieldSetup set)
        {

            #region Preview Settings

            EditorGUI.BeginChangeCheck();

            if (advancedMode)
            {

                GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                EditorGUILayout.BeginHorizontal();
                FGUI_Inspector.FoldHeaderStart(ref drawPreviewSetts, " Preview Settings", null, FGUI_Resources.Tex_Preview);

                #region Center Cam Button

                SceneView view = SceneView.lastActiveSceneView;
                if (view != null)
                    if (view.camera != null)
                    {
                        float referenceScale = 8f;
                        if (projectPreset != null) referenceScale = projectPreset.GetCellUnitSize().x * 8;


                        if (Vector3.Distance(view.camera.transform.position, new Vector3(0, referenceScale, -referenceScale)) > referenceScale)
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(new GUIContent(" Center", EditorGUIUtility.IconContent("Camera Icon").image), FGUI_Resources.ButtonStyle, GUILayout.Height(19)))
                            {
                                FrameCenter(referenceScale);
                            }
                        }

                        float angleDiff = Quaternion.Angle(view.camera.transform.rotation, Quaternion.identity);

                        if (angleDiff > 125)
                        {
                            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("RotateTool").image), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                            {
                                FrameCenter(referenceScale, true);
                            }
                        }
                    }

                EditorGUILayout.EndHorizontal();

                #endregion


                if (drawPreviewSetts)
                {

                    #region Test Generating Grid Settings

                    // Generation Help Fields

                    GUILayout.Space(5);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    FGUI_Inspector.FoldHeaderStart(ref drawTestGenSetts, " Preview Generating Settings", FGUI_Resources.BGInBoxBlankStyle, FGUI_Resources.Tex_MiniGear);


                    if (drawTestGenSetts)
                    {
                        GUILayout.Space(5);
                        EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                        EditorGUILayout.BeginHorizontal();

                        int preSeed = Seed;

                        string seedTitle = "Seed";
                        if (Seed != 0) seedTitle = "Fixed Seed"; else seedTitle = "Random Seed";

                        Seed = EditorGUILayout.IntField(seedTitle, Seed);
                        if (Seed == 0)
                        {

                            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Random, "Set Some Random Fixed Seed Number"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(18)))
                            {
                                Seed = Random.Range(-999, 999);
                                so_preset.ApplyModifiedProperties();
                                TriggerPreview();
                            }

                            EditorGUILayout.LabelField("(Random)", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(60));
                        }
                        else
                        {
                            if (GUILayout.Button("Randomize"))
                            {
                                Seed = Random.Range(-999, 999);
                                so_preset.ApplyModifiedProperties();
                                TriggerPreview();
                            }
                            else
                            {
                                if (Seed != preSeed)
                                {
                                    if (so_preset != null) so_preset.ApplyModifiedProperties();
                                    TriggerPreview();
                                }
                            }
                        }

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndVertical();

                        GUILayout.Space(2);
                        EditorGUIUtility.labelWidth = 175;
                        EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                        RunAdditionalGenerators = EditorGUILayout.Toggle(new GUIContent("Run Additional Generators", " Triggering Generate() on spawned components inplementing IGenerating interface"), RunAdditionalGenerators);
                        EditorGUILayout.EndVertical();
                        EditorGUIUtility.labelWidth = 0;

                        GUILayout.Space(5);

                        if (advancedMode)
                        {
                            gridMode = (EDesignerGridMode)EditorGUILayout.EnumPopup("Mode: ", gridMode);
                            GUILayout.Space(4);
                        }

                        if (gridMode != EDesignerGridMode.Paint)
                        {
                            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                            GUILayout.Space(3);

                            if (gridMode != EDesignerGridMode.RectangleGrid)
                            {
                                SizeX = MinMax.DrawGUI(SizeX, new GUIContent("SizeX"));
                                SizeY = MinMax.DrawGUI(SizeY, new GUIContent("SizeY"));
                                SizeZ = MinMax.DrawGUI(SizeZ, new GUIContent("SizeZ"));
                            }
                            else
                            {
                                Vector3Int size = new Vector3Int(SizeX.Min, SizeY.Min, SizeZ.Min);
                                size = EditorGUILayout.Vector3IntField("Test Grid Size (in cells):", size);
                                if (size.x < 1) size.x = 1;
                                if (size.y <= 0) size.y = 0;
                                if (size.z < 1) size.z = 1;

                                SizeX.Min = size.x; SizeX.Max = size.x;
                                SizeY.Min = size.y; SizeY.Max = size.y;
                                SizeZ.Min = size.z; SizeZ.Max = size.z;
                            }

                            GUILayout.Space(4);

                            if (gridMode == EDesignerGridMode.BranchedGeneration)
                            {
                                GUILayout.Space(4);
                                BranchLength = MinMax.DrawGUI(BranchLength, new GUIContent("BranchLength"));
                                TargetBranches = MinMax.DrawGUI(TargetBranches, new GUIContent("TargetBranches"));
                                CellsSpace = MinMax.DrawGUI(CellsSpace, new GUIContent("CellsSpace"));
                            }

                            EditorGUILayout.EndVertical();


                            if (advancedMode)
                            {
                                GUILayout.Space(3);
                                DrawAdditionalGen = EditorGUILayout.Foldout(DrawAdditionalGen, " Draw Advanced Options", true);

                                if (DrawAdditionalGen)
                                {
                                    EditorGUI.indentLevel++;
                                    GUILayout.Space(4);
                                    OffsetGrid = EditorGUILayout.Vector3IntField("Offset Grid", OffsetGrid);
                                    GUILayout.Space(6);

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUIUtility.labelWidth = 200;
                                    SendMessageAfterGenerateTo = (GameObject)EditorGUILayout.ObjectField("After Generate Send Message To", SendMessageAfterGenerateTo, typeof(GameObject), true);
                                    EditorGUIUtility.labelWidth = 0;
                                    PostGenerateMessage = EditorGUILayout.TextField(PostGenerateMessage);
                                    EditorGUILayout.EndHorizontal();
                                    GUILayout.Space(3);

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUIUtility.labelWidth = 200;
                                    SendMessageOnChangeTo = (GameObject)EditorGUILayout.ObjectField("On Change Send Message To", SendMessageOnChangeTo, typeof(GameObject), true);
                                    EditorGUIUtility.labelWidth = 0;
                                    OnChangeMessage = EditorGUILayout.TextField(OnChangeMessage);
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUI.indentLevel--;
                                    GUILayout.Space(3);
                                }
                            }

                        }

                        GUILayout.Space(4);


                        #region Guide Drawers List

                        //EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        //EditorGUILayout.BeginHorizontal();

                        //EditorGUILayout.LabelField("Spawn Guides (doors)");
                        //GUILayout.FlexibleSpace();
                        //if (guides.Count > 0) if (GUILayout.Button("-", GUILayout.Width(24))) guides.RemoveAt(guides.Count - 1);
                        //if (GUILayout.Button("+", GUILayout.Width(24))) guides.Add(CreateInstance<GuideDrawer>());
                        //EditorGUILayout.EndHorizontal();

                        //for (int i = guides.Count - 1; i >= 0; i--) if (guides[i] == null) guides.RemoveAt(i); // Cleaning from nulls

                        //GUILayout.Space(2);
                        //for (int i = 0; i < guides.Count; i++)
                        //{
                        //    GUILayout.Space(2);
                        //    guides[i].DrawMe();
                        //}

                        //EditorGUILayout.EndVertical();

                        #endregion


                        #region Restrictions Drawers List

                        //EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        //EditorGUILayout.BeginHorizontal();

                        //EditorGUILayout.LabelField("Spawn Restrictions (preset's globals)", GUILayout.Width(220));
                        //GUILayout.FlexibleSpace();
                        //if (restrictions.Count > 0) if (GUILayout.Button("-", GUILayout.Width(24))) restrictions.RemoveAt(restrictions.Count - 1);
                        //if (GUILayout.Button("+", GUILayout.Width(24))) restrictions.Add(CreateInstance<RestrictionDrawer>());
                        //EditorGUILayout.EndHorizontal();

                        //for (int i = restrictions.Count - 1; i >= 0; i--) if (restrictions[i] == null) restrictions.RemoveAt(i); // Cleaning from nulls

                        //GUILayout.Space(2);
                        //for (int i = 0; i < restrictions.Count; i++)
                        //{
                        //    GUILayout.Space(2);
                        //    restrictions[i].DrawMe();
                        //}

                        //EditorGUILayout.EndVertical();

                        #endregion

                    }


                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();


                    #endregion

                    GUILayout.Space(4);


                    #region Scene Preview Settings

                    FGUI_Inspector.FoldHeaderStart(ref drawScenePreviewSetts, " Scene Preview Settings", FGUI_Resources.BGInBoxStyle, null);

                    if (drawScenePreviewSetts)
                    {
                        GUILayout.Space(3);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 90;
                        AutoRefreshPreview = EditorGUILayout.Toggle(new GUIContent("Auto Preview", "Automatically run all rules on grid every change, good for using AUTO SPAWN or PREVIEW (For PREVIEW : Scene Preview Settings -> Alpha set to higher than zero)"), AutoRefreshPreview);

                        GUILayout.FlexibleSpace();
                        EditorGUIUtility.labelWidth = 40;
                        if (PreviewAlpha < Mathf.Epsilon)
                        {
                            EditorGUIUtility.fieldWidth = 26;
                            PreviewAlpha = EditorGUILayout.Slider("Alpha", PreviewAlpha, 0f, 1f);
                            EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Info, "Preview without spawning is disabled -> It's quicker than spawning but needs clean mesh setup inside prefabs"), GUILayout.Width(16));
                        }
                        else
                        {
                            PreviewAlpha = EditorGUILayout.Slider("Alpha", PreviewAlpha, 0f, 1f);
                        }

                        EditorGUIUtility.fieldWidth = 0;

                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(1);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 110;
                        ColorizePreview = EditorGUILayout.Toggle("Colorize Preview", ColorizePreview);

                        EditorGUIUtility.labelWidth = 78;

                        GUILayout.FlexibleSpace();
                        EditorGUIUtility.labelWidth = 86;
                        DrawWhenFocused = EditorGUILayout.Toggle(new GUIContent("Only Focused", "Drawing gizmos and gui only when Field Design window or Scene View is focused"), DrawWhenFocused);
                        EditorGUIUtility.labelWidth = 78;
                        DrawScreenGUI = EditorGUILayout.Toggle("Screen GUI", DrawScreenGUI);
                        //if (AutoRefreshPreview)
                        //{
                        //    GUILayout.FlexibleSpace();
                        //    PreviewAutoSpawn = EditorGUILayout.Toggle("Auto Spawn", PreviewAutoSpawn);
                        //}

                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(1);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 92;
                        DrawGrid = EditorGUILayout.Toggle("Draw Grid", DrawGrid);
                        GUILayout.FlexibleSpace();
                        EditorGUIUtility.labelWidth = 81;
                        DrawEmptys = EditorGUILayout.Toggle("Draw Emptys", DrawEmptys); GUILayout.Space(6);
                        EditorGUIUtility.labelWidth = 110;
                        AutoDestroy = EditorGUILayout.Toggle(new GUIContent("Destroy On Close", "Destroy generated preview objects on closing window"), AutoDestroy);
                        //EditorGUIUtility.labelWidth = 62;
                        //Repose = EditorGUILayout.Toggle("Repose", Repose);
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(5);
                    }

                    EditorGUILayout.EndVertical();

                    #endregion

                }

                if (EditorGUI.EndChangeCheck())
                {
                    repaint = true;
                }

                EditorGUILayout.EndVertical();
            }

            #endregion



            GUILayout.Space(4f);

            EditorGUI.BeginChangeCheck();

            // Preset field
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            GUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 48;

            if (projectPreset == null) GUI.color = new Color(1f, 1f, 0.4f, 1f);

            Texture foldTex = drawPresetParams ? FGUI_Resources.Tex_DownFoldGray : FGUI_Resources.Tex_RightFoldGray;

            if (GUILayout.Button(new GUIContent(" Preset:", foldTex), EditorStyles.label, GUILayout.Height(19), GUILayout.Width(70)))
            {
                drawPresetParams = !drawPresetParams;
            }

            projectPreset = (FieldSetup)EditorGUILayout.ObjectField(projectPreset, typeof(FieldSetup), false);
            if (projectPreset == null) GUI.color = new Color(1f, 1f, 1f, 1f);

            bool wasAutoFileButtons = false;



            if (StartupRefs)
                if (StartupRefs.FSDraftsdirectory)
                {
                    wasAutoFileButtons = true;

                    #region Button to display menu of draft field setup files

                    if (latestFilesInDraft > 0)
                        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_DownFold, "Display quick selection menu for FieldSetups contained in the drafts directory"), EditorStyles.label, GUILayout.Width(18), GUILayout.Height(16)))
                        {
                            string path = AssetDatabase.GetAssetPath(StartupRefs.FSDraftsdirectory);
                            var files = System.IO.Directory.GetFiles(path, "*.asset");
                            if (files != null)
                            {
                                latestFilesInDraft = files.Length;

                                GenericMenu draftsMenu = new GenericMenu();
                                draftsMenu.AddItem(new GUIContent("None"), projectPreset == null, () => { projectPreset = null; });
                                int currently = 0;

                                for (int i = 0; i < files.Length; i++)
                                {
                                    FieldSetup fs = AssetDatabase.LoadAssetAtPath<FieldSetup>(files[i]);
                                    if (fs)
                                    {
                                        if (currently == 6)
                                        {
                                            draftsMenu.AddItem(new GUIContent("... Categorizing rest of the presets ..."), false, () => { });
                                            draftsMenu.AddItem(GUIContent.none, false, () => { });
                                        }

                                        if (currently >= 6)
                                        {
                                            int fmods = fs.CountFieldModificators();

                                            string pred = "";
                                            if (fmods < 5) pred = "Less than 5 Mods In Setup/";
                                            else if (fmods < 15) pred = "Less than 15 Mods In Setup/";
                                            else if (fmods < 25) pred = "Less than 25 Mods In Setup/";
                                            else pred = "Rest of the presets/";

                                            draftsMenu.AddItem(new GUIContent(pred + fs.name), projectPreset == fs, () => { projectPreset = fs; });
                                        }
                                        else
                                        {
                                            draftsMenu.AddItem(new GUIContent(fs.name), projectPreset == fs, () => { projectPreset = fs; });
                                        }

                                        currently += 1;
                                    }
                                }

                                draftsMenu.ShowAsContext();
                            }
                        }


                    #endregion


                    if (projectPreset != null)
                    {
                        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Rename, "Rename FieldSetup name (Save-File popup window will not create file, it will just take written name)"), new GUILayoutOption[2] { GUILayout.Width(32), GUILayout.Height(20) }))
                        {
                            string filename = EditorUtility.SaveFilePanelInProject("Type your title (no file will be created)", projectPreset.name, "", "Type your title (no file will be created)");
                            if (!string.IsNullOrEmpty(filename))
                            {
                                filename = System.IO.Path.GetFileNameWithoutExtension(filename);
                                if (!string.IsNullOrEmpty(filename))
                                {
                                    projectPreset.name = filename;
                                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(projectPreset), filename);
                                    EditorUtility.SetDirty(projectPreset);
                                }
                            }
                        }
                    }


                    if (projectPreset == null) GUI.backgroundColor = Color.green;
                    else GUI.backgroundColor = new Color(0.75f, 1f, 0.75f, 1f);

                    if (GUILayout.Button(new GUIContent("New", "Generates new Field Setup file in the PGG drafts directory, after that you can move draft file into project directories with 'Move' button which will appear"), GUILayout.Width(44)))
                    {
                        string path;

                        path = AssetDatabase.GetAssetPath(StartupRefs.FSDraftsdirectory);
                        var files = System.IO.Directory.GetFiles(path, "*.asset");
                        latestFilesInDraft = files.Length;
                        path += "/FS_NewFieldSetup" + (files.Length + 1) + ".asset";

                        FieldSetup scrInstance = CreateInstance<FieldSetup>();

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



                    //if (isInDefaultDirectory)
                    //{
                    //    if (projectPreset != null)
                    //        if (GUILayout.Button(new GUIContent(" Move", PGGUtils.TEX_FolderDir, "Move 'Field Setup' file to choosed project directory"), GUILayout.Height(20), GUILayout.Width(74)))
                    //        {
                    //            string path = FGenerators.GetPathPopup("Move 'Field Setup' file to new directory in project", "FS_Your FieldSetup");
                    //            if (!string.IsNullOrEmpty(path))
                    //            {
                    //                UnityEditor.AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(projectPreset), path);
                    //                AssetDatabase.SaveAssets();
                    //            }

                    //            wasChecked = null;
                    //        }
                    //}
                    //else
                    //{
                    //    if (StartupRefs.FSDraftsdirectory != null)
                    //        if (projectPreset != null)
                    //            if (GUILayout.Button(new GUIContent(" Back", PGGUtils.TEX_FolderDir, "Move 'Field Setup' file from project to default PGG Drafts directory"), GUILayout.Height(20), GUILayout.Width(56)))
                    //            {
                    //                string path = AssetDatabase.GetAssetPath(StartupRefs.FSDraftsdirectory);

                    //                if (!string.IsNullOrEmpty(path))
                    //                {
                    //                    path += "/" + projectPreset.name + ".asset";
                    //                    UnityEditor.AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(projectPreset), path);
                    //                    AssetDatabase.SaveAssets();
                    //                }

                    //                wasChecked = null;
                    //            }
                    //}



                    if (GUILayout.Button(PGGUtils.TEX_MenuIcon, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(19)))
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
                                        UnityEditor.AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(StartupRefs.FSDraftsdirectory), path);
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

                                    string path = AssetDatabase.GetAssetPath(StartupRefs.FSDraftsdirectory);

                                    if (!string.IsNullOrEmpty(path))
                                    {
                                        path += "/" + projectPreset.name + ".asset";
                                        UnityEditor.AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(Get.projectPreset), path);
                                        AssetDatabase.SaveAssets();
                                    }

                                    wasChecked = null;

                                });
                        }

                        #endregion

                        draftsMenu.AddItem(new GUIContent("+ Create new FieldSetup in selected project directory"), false, () =>
                        {
                            var newPres = (FieldSetup)FGenerators.GenerateScriptable(CreateInstance<FieldSetup>(), "FS_");
                            if (newPres != null)
                            {
                                if (AssetDatabase.Contains(newPres))
                                    projectPreset = newPres;
                            }
                        });

                        draftsMenu.ShowAsContext();

                    }


                    {
                        //if (UsingDraft == null)
                        //{
                        //    if (GUILayout.Button(new GUIContent( "Use Draft", "Using single file for FieldSetup, if you press this button, previous draft will be removed!"), GUILayout.Width(64)))
                        //    {
                        //        UsingDraft = CreateInstance<FieldSetup>();
                        //        UsingDraft.name = "FS_Draft";
                        //        UnityEditor.AssetDatabase.CreateAsset(UsingDraft, AssetDatabase.GetAssetPath(StartupRefs.FSDraftsdirectory) + "/FS_Draft.asset");
                        //        projectPreset = UsingDraft;
                        //    }
                        //}
                        //else
                        //{
                        //    if (projectPreset == UsingDraft)
                        //    {
                        //        if (GUILayout.Button("Export Draft", GUILayout.Width(78)))
                        //        {
                        //            AssetDatabase.SaveAssets();
                        //            string p = FGenerators.GenerateScriptablePath(UsingDraft, "FS_");
                        //            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(UsingDraft), p);
                        //            UsingDraft = null;
                        //            AssetDatabase.SaveAssets();
                        //            projectPreset = AssetDatabase.LoadAssetAtPath<FieldSetup>(p);
                        //        }

                        //        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Remove, "Clear Draft Field Setup File"), FGUI_Resources.ButtonStyle, GUILayout.Width(20), GUILayout.Height(18)))
                        //        {
                        //            projectPreset = null;
                        //            UsingDraft = null;
                        //        }
                        //    }
                        //}
                    }

                }

            if (!wasAutoFileButtons)
            {
                if (GUILayout.Button("Create New", GUILayout.Width(94))) { projectPreset = (FieldSetup)FGenerators.GenerateScriptable(CreateInstance<FieldSetup>(), "FS_"); }
            }
            else
            {
                //if (GUILayout.Button(new GUIContent("+", "Generate new Field Setup in choosed project directory"), GUILayout.Width(21))) { projectPreset = (FieldSetup)FGenerators.GenerateScriptable(CreateInstance<FieldSetup>(), "FS_"); }
            }

            EditorGUILayout.EndHorizontal();

            if (so_preset != null && projectPreset != null)
            {

                #region Room Preset Draw with Modificators Packs etc.

                EditorGUIUtility.labelWidth = 0;

                so_preset.Update();
                SerializedProperty iter = so_preset.GetIterator();

                if (iter != null)
                {
                    iter.Next(true);
                    iter.NextVisible(false);

                    //if (advancedMode)
                    //{
                    //    GUI.backgroundColor = new Color(.98f, .98f, .79f, 1f);
                    //    FGUI_Inspector.FoldHeaderStart(ref drawPresetParams, " Field Setup Parameters", FGUI_Resources.HeaderBoxStyle, FGUI_Resources.Tex_Prepare);
                    //    GUI.backgroundColor = Color.white;
                    //}
                    //else
                    //    drawPresetParams = true;
                    EditorGUILayout.BeginVertical();

                    if (drawPresetParams)
                    {
                        GUILayout.Space(4f);

                        EditorGUI.BeginChangeCheck();

                        SerializedProperty sp_uni = so_preset.FindProperty("NonUniformSize");
                        var sp_univ = sp_uni.Copy(); sp_univ.Next(false);
                        iter.NextVisible(false);

                        EditorGUILayout.BeginHorizontal();

                        if (sp_uni.boolValue == false)
                            EditorGUILayout.PropertyField(iter, new GUIContent("Grid's Single Cell Size:", "In Units"));
                        else
                            EditorGUILayout.PropertyField(sp_univ);

                        if (EditorGUI.EndChangeCheck())
                        {
                            so_preset.ApplyModifiedProperties();
                            TriggerRefresh(false);
                        }

                        EditorGUIUtility.labelWidth = 8;
                        EditorGUILayout.PropertyField(sp_uni, new GUIContent(" ", "Enabling / disabling switch for non uniform size for cells"), GUILayout.Width(32));
                        EditorGUILayout.EndHorizontal();

                        iter.NextVisible(false);
                        iter.NextVisible(false);

                        SerializedProperty sp_prestOffs = iter.Copy();


                        if (advancedMode)
                        {
                            GUILayout.Space(3);

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(new GUIContent("Info: ", "There you can write your custom info for the field setup, like minimum cell count info for all logics to work correctly etc."), GUILayout.Width(32));
                            projectPreset.InfoText = EditorGUILayout.TextArea(projectPreset.InfoText);
                            EditorGUILayout.EndHorizontal();
                            EditorGUIUtility.labelWidth = 0;
                            GUILayout.Space(2);
                        }


                        string title = "Cells Instructions Commands";
                        bool drawInstr = true;
                        if (!advancedMode)
                        {
                            title = "Cell Commands  FOR GRID PAINTER";
                            EditorGUIUtility.labelWidth = 0;
                            drawInstr = isSelectedPainter || (projectPreset.CellsCommands.Count != 2 && projectPreset.CellsCommands.Count > 0);
                            GUILayout.Space(3);
                        }
                        else
                        {
                            GUILayout.Space(5);
                        }

                        bool prevarview = drawInstructions;

                        if (drawInstr)
                            DrawCellInstructionDefinitionsList(ref variablesPage, projectPreset.CellsInstructions, FGUI_Resources.BGInBoxStyle, title, ref drawInstructions, true, projectPreset);

                        if (prevarview != drawInstructions)
                        {
                            projectPreset.RefreshUtilityMods();
                        }

                        if (advancedMode)
                        {
                            EditorGUI.BeginChangeCheck();

                            GUILayout.Space(5);
                            DrawFieldVariablesList(projectPreset.Variables, FGUI_Resources.BGInBoxStyle, "Field Variables", ref drawVariables, ref drawVariablesMode, projectPreset);

                            if (EditorGUI.EndChangeCheck())
                            {
                                so_preset.ApplyModifiedProperties();
                                TriggerRefresh(false);
                            }
                        }

                        GUILayout.Space(3);
                    }

                    EditorGUILayout.EndVertical();

                    if (advancedMode) EditorGUILayout.EndVertical();

                    GUILayout.Space(2);
                    if (advancedMode) GUILayout.Space(4);

                    FGUI_Inspector.BeginVertical(FGUI_Resources.BGInBoxStyle, new Color(1f, 1f, 1f, 1f));
                    DrawMods();
                    EditorGUILayout.EndVertical();

                }

                #endregion

            }
            else
            {
                EditorGUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck()) repaint = true;
            EditorGUIUtility.labelWidth = 0;

        }


        void TriggerPreview()
        {
            Get.ClearAllGeneratedGameObjects();
            if (gridMode != EDesignerGridMode.Paint) Get.GenerateBaseFieldGrid();

            Get.RunFieldCellsRules();
            if (Get.PreviewAutoSpawn) Get.RunFieldCellsSpawningGameObjects();
        }


        public bool GenButtonsFoldout = false;
        public bool PostEventsFoldout = false;
        void DrawPostEvents()
        {
            GUILayout.Space(-3);
            if (so_preset == null) return;
            SerializedProperty sp = so_preset.FindProperty("AddReflectionProbes");
            if (sp == null) return;
            EditorGUILayout.PropertyField(sp);
            sp.NextVisible(false); if (projectPreset.AddReflectionProbes) EditorGUILayout.PropertyField(sp);
            sp.NextVisible(false); if (projectPreset.AddReflectionProbes && projectPreset.MainReflectionSettings) EditorGUILayout.PropertyField(sp);
            if (projectPreset.AddReflectionProbes && projectPreset.AddMultipleProbes)
            {
                SerializedProperty spp = so_preset.FindProperty("SmallerReflSettings");
                EditorGUILayout.PropertyField(spp);
                spp.Next(false); EditorGUILayout.PropertyField(spp);
                spp.Next(false); EditorGUILayout.PropertyField(spp);
                spp.Next(false); EditorGUILayout.PropertyField(spp);
            }

            sp.NextVisible(false); EditorGUILayout.PropertyField(sp);

            sp.NextVisible(false); if (projectPreset.AddLightProbes) EditorGUILayout.PropertyField(sp);
            EditorGUIUtility.labelWidth = 180;
            sp.NextVisible(false); EditorGUILayout.PropertyField(sp);
            EditorGUIUtility.labelWidth = 0;

            if (projectPreset.TriggerColliderGeneration == FieldSetup.ETriggerGenerationMode.MultipleBoxesFill)
            {
                SerializedProperty spp = so_preset.FindProperty("TriggerGenSettings");
                EditorGUILayout.PropertyField(spp, true);
            }

            //SerializedProperty sp_weld = so_preset.FindProperty("WeldCombined");
            //EditorGUILayout.PropertyField(sp_weld);
            //sp.NextVisible(false); EditorGUILayout.PropertyField(sp);
        }

    }
}