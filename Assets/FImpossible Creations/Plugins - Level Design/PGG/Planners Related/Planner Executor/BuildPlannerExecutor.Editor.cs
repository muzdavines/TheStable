#if UNITY_EDITOR
using UnityEditor;
using FIMSpace.FEditor;
using UnityEngine;
using FIMSpace.Generating.Planning;
using FIMSpace.Generating.Planning.GeneratingLogics;

namespace FIMSpace.Generating
{

    public partial class BuildPlannerExecutor : MonoBehaviour
    {

        [HideInInspector] public Object _plannerDraftsDirectory;


        #region Editor Related

#if UNITY_EDITOR
        public int _Editor_SelectedSetup = 0;
        public bool _Editor_PreviewingShape = false;
        public bool _Editor_drawInitShape = true;
        public bool _Editor_drawMainVariables = true;
        public bool _Editor_displaySetup = true;

        [HideInInspector] public bool _Editor_topInfoClicked = false;
        private void OnDrawGizmosSelected()
        {
            if (BuildPlannerPreset == null) return;
            if (BuildPlannerPreset.BasePlanners.Count == 0) return;
            if (_Editor_SelectedSetup > BuildPlannerPreset.BasePlanners.Count) return;

            var viewPlanner = BuildPlannerPreset.BasePlanners[_Editor_SelectedSetup];

            if (_Editor_PreviewingShape)
            {
                Matrix4x4 preMx = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                viewPlanner.GetInitialChecker().DrawFieldGizmos();
                Gizmos.matrix = preMx;
            }

            if (GeneratedPreview == null) return;
            if (GeneratedPainters.Count > 0) return;

            Gizmos.matrix = transform.localToWorldMatrix;
            Handles.matrix = Gizmos.matrix;
            PlanGenerationPrint.DrawPrintGizmos(GeneratedPreview);
            Gizmos.matrix = Matrix4x4.identity;
            Handles.matrix = Matrix4x4.identity;
        }

        #region Fields List

        private BuildPlannerPreset _SetupPreset = null;
        private int[] _SetupsIds = null;
        public int[] GetSetupsIDList(bool forceRefresh = false)
        {
            if (BuildPlannerPreset == null) return null;
            if (_SetupPreset != BuildPlannerPreset) forceRefresh = true;

            if (forceRefresh || _SetupsIds == null || _SetupsIds.Length != BuildPlannerPreset.BasePlanners.Count)
            {
                _SetupPreset = BuildPlannerPreset;
                _SetupsIds = new int[BuildPlannerPreset.BasePlanners.Count];
                for (int i = 0; i < BuildPlannerPreset.BasePlanners.Count; i++)
                {
                    _SetupsIds[i] = i;
                }
            }

            return _SetupsIds;
        }

        private GUIContent[] _SetupsNames = null;
        internal GUIContent[] GetSetupsNameList(bool forceRefresh = false)
        {
            if (BuildPlannerPreset == null) return null;
            if (_SetupPreset != BuildPlannerPreset) forceRefresh = true;

            if (forceRefresh || _SetupsNames == null || _SetupsNames.Length != BuildPlannerPreset.BasePlanners.Count)
            {
                _SetupPreset = BuildPlannerPreset;
                _SetupsNames = new GUIContent[BuildPlannerPreset.BasePlanners.Count];
                for (int i = 0; i < BuildPlannerPreset.BasePlanners.Count; i++)
                {
                    _SetupsNames[i] = new GUIContent("[" + i + "] " + BuildPlannerPreset.BasePlanners[i].name);
                }
            }

            return _SetupsNames;
        }

        internal void ResetSetups()
        {
            ClearGenerated();
            _plannerPrepare = null;
            ResetPlannerComposition();
        }


        #endregion

#endif

        #endregion

    }


    /// <summary>
    /// FM: Editor class component to enchance controll over component from inspector window
    /// </summary>
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(BuildPlannerExecutor))]
    public class BuildPlannerExecutorEditor : UnityEditor.Editor
    {
        public BuildPlannerExecutor Get { get { if (_get == null) _get = (BuildPlannerExecutor)target; return _get; } }
        private BuildPlannerExecutor _get;

        SerializedProperty sp_Planner;
        SerializedProperty sp_Async;
        SerializedProperty sp_Seed;
        SerializedProperty sp_GenerateOnStart;


        Color preGuiC;
        Color preBg;

        #region Planner Bar

        ScriptableObject wasChecked = null;
        bool isInDefaultDirectory = false;

        public static Texture _TEX_FolderDir { get { if (__Tex_folderdir != null) return __Tex_folderdir; __Tex_folderdir = EditorGUIUtility.IconContent("Folder Icon").image; return __Tex_folderdir; } }
        private static Texture __Tex_folderdir = null;

        #endregion


        public override bool RequiresConstantRepaint()
        {
            return Get.IsGenerating;
        }

        bool onEnabled = false;

        private void OnEnable()
        {
            onEnabled = false;
            sp_Planner = serializedObject.FindProperty("Planner");
            sp_Async = serializedObject.FindProperty("Async");
            sp_Seed = serializedObject.FindProperty("Seed");
            sp_GenerateOnStart = serializedObject.FindProperty("GenerateOnStart");

            Get.ValidateSetups();
            Get.ResetPlannerComposition();
            Get.RefreshVariablesReferences();
        }


        public override void OnInspectorGUI()
        {
            PGGUtils.SetDarkerBacgroundOnLightSkin();

            try
            {

                if (Event.current.type == EventType.Layout)
                    if (targetPlanSelection != null)
                    {
                        Get.BuildPlannerPreset = targetPlanSelection;
                        targetPlanSelection = null;
                    }

                if (Application.isPlaying == false)
                {
                    if (Get.IsGenerating)
                    {
                        Get.UpdateGeneratingProgress();
                    }
                }

                Gizmos.matrix = Get.transform.localToWorldMatrix;
                Handles.matrix = Gizmos.matrix;
                bool filled = true;

                bool preE = GUI.enabled;
                preGuiC = GUI.color;
                preBg = GUI.backgroundColor;

                if (Get._Editor_topInfoClicked == false)
                {
                    if (GUILayout.Button("Changing variables names or Adding/Removing Planners in the BuildPlannerPreset requires resetting composition!", EditorStyles.helpBox)) Get._Editor_topInfoClicked = true;
                }

                serializedObject.Update();

                GUILayout.Space(4f);

                #region Planner Bar


                #region Checking project file if it's in drafts directory

                if (wasChecked != Get.BuildPlannerPreset)
                {
                    isInDefaultDirectory = false;

                    wasChecked = Get.BuildPlannerPreset;

                    if (Get.BuildPlannerPreset != null)
                    {
                        if (Get._plannerDraftsDirectory)
                        {
                            string qPath = AssetDatabase.GetAssetPath(Get._plannerDraftsDirectory);
                            string sPath = AssetDatabase.GetAssetPath(Get.BuildPlannerPreset);
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
                GUILayout.BeginHorizontal(FGUI_Resources.BGInBoxStyle);

                GUILayout.BeginVertical();
                GUILayout.Space(2);

                EditorGUIUtility.labelWidth = 55;
                if (GUILayout.Button(new GUIContent("Preset:", "Click to open Build Planner window"), EditorStyles.boldLabel, GUILayout.Width(60))) { if (Get.BuildPlannerPreset) AssetDatabase.OpenAsset(Get.BuildPlannerPreset); }
                EditorGUIUtility.labelWidth = 0;

                GUILayout.EndVertical();

                BuildPlannerPreset prePreset = Get.BuildPlannerPreset;
                Get.BuildPlannerPreset = (BuildPlannerPreset)EditorGUILayout.ObjectField(Get.BuildPlannerPreset, typeof(BuildPlannerPreset), false);

                if (prePreset != Get.BuildPlannerPreset)
                {
                    selectedDupl = 0;
                    Get._Editor_SelectedSetup = 0;
                    Get.ResetSetups();
                }

                //bool wasAutoFileButtons = false;

                if (Get._plannerDraftsDirectory)
                {
                    //wasAutoFileButtons = true;

                    #region Button to display menu of draft field setup files

                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_DownFold, "Display quick selection menu for FieldSetups contained in the drafts directory"), EditorStyles.label, GUILayout.Width(18), GUILayout.Height(16)))
                    {
                        string path = AssetDatabase.GetAssetPath(Get._plannerDraftsDirectory);
                        var files = System.IO.Directory.GetFiles(path, "*.asset");
                        if (files != null)
                        {
                            GenericMenu draftsMenu = new GenericMenu();

                            draftsMenu.AddItem(new GUIContent("None"), Get.BuildPlannerPreset == null, () => { Get.BuildPlannerPreset = null; });

                            for (int i = 0; i < files.Length; i++)
                            {
                                BuildPlannerPreset fs = AssetDatabase.LoadAssetAtPath<BuildPlannerPreset>(files[i]);
                                if (fs) draftsMenu.AddItem(new GUIContent(fs.name), Get.BuildPlannerPreset == fs, () =>
                                {
                                    targetPlanSelection = fs;
                                    if (Get.BuildPlannerPreset != fs) Get.ResetSetups();
                                });
                            }

                            draftsMenu.ShowAsContext();
                        }
                    }


                    #endregion

                    if (Get.BuildPlannerPreset != null)
                    {
                        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Rename, "Rename BuildPlannerPreset name (Save-File popup window will not create file, it will just take written name)"), new GUILayoutOption[2] { GUILayout.Width(32), GUILayout.Height(20) }))
                        {
                            string filename = EditorUtility.SaveFilePanelInProject("Type your title (no file will be created)", Get.BuildPlannerPreset.name, "", "Type your title (no file will be created)");
                            if (!string.IsNullOrEmpty(filename))
                            {
                                filename = System.IO.Path.GetFileNameWithoutExtension(filename);
                                if (!string.IsNullOrEmpty(filename))
                                {
                                    Get.BuildPlannerPreset.name = filename;
                                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(Get.BuildPlannerPreset), filename);
                                    EditorUtility.SetDirty(Get.BuildPlannerPreset);
                                }
                            }
                        }
                    }

                    if (Get.BuildPlannerPreset == null) GUI.backgroundColor = Color.green;
                    else GUI.backgroundColor = new Color(0.75f, 1f, 0.75f, 1f);

                    if (GUILayout.Button(new GUIContent("New", "Generates new Planner Setup file in the PGG drafts directory, after that you can move draft file into project directories with 'Move' button which will appear"), GUILayout.Width(44)))
                    {
                        string path;

                        path = AssetDatabase.GetAssetPath(Get._plannerDraftsDirectory);
                        var files = System.IO.Directory.GetFiles(path, "*.asset");
                        path += "/FS_NewFieldSetup" + (files.Length + 1) + ".asset";

                        BuildPlannerPreset scrInstance = CreateInstance<BuildPlannerPreset>();

                        if (string.IsNullOrEmpty(path))
                            path = FGenerators.GenerateScriptablePath(scrInstance, "FS_NewFieldSetup");

                        if (!string.IsNullOrEmpty(path))
                        {
                            UnityEditor.AssetDatabase.CreateAsset(scrInstance, path);
                            AssetDatabase.SaveAssets();
                            Get.BuildPlannerPreset = scrInstance;
                        }

                        //projectPreset = (FieldSetup)FGenerators.GenerateScriptable(CreateInstance<FieldSetup>(), "FS_");
                    }

                    GUI.backgroundColor = preBGCol;



                    //    if (isInDefaultDirectory)
                    //    {
                    //        if (Get.Planner != null)
                    //            if (GUILayout.Button(new GUIContent(" Move", _TEX_FolderDir, "Move 'Field Setup' file to choosed project directory"), GUILayout.Height(20), GUILayout.Width(74)))
                    //            {
                    //                string path = FGenerators.GetPathPopup("Move 'Field Setup' file to new directory in project", "FS_Your FieldSetup");
                    //                if (!string.IsNullOrEmpty(path))
                    //                {
                    //                    UnityEditor.AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(Get.Planner), path);
                    //                    AssetDatabase.SaveAssets();
                    //                }

                    //                wasChecked = null;
                    //            }
                    //    }
                    //    else
                    //    {
                    //        if (Get._plannerDraftsDirectory != null)
                    //            if (Get.Planner != null)
                    //                if (GUILayout.Button(new GUIContent(" Back", _TEX_FolderDir, "Move 'Field Setup' file from project to default PGG Drafts directory"), GUILayout.Height(20), GUILayout.Width(56)))
                    //                {
                    //                    string path = AssetDatabase.GetAssetPath(Get._plannerDraftsDirectory);

                    //                    if (!string.IsNullOrEmpty(path))
                    //                    {
                    //                        path += "/" + Get.Planner.name + ".asset";
                    //                        UnityEditor.AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(Get.Planner), path);
                    //                        AssetDatabase.SaveAssets();
                    //                    }

                    //                    wasChecked = null;
                    //                }
                    //    }
                }


                //if (Get.Planner)
                //{
                //    if (wasAutoFileButtons)
                //    {
                //        if (GUILayout.Button(new GUIContent("+", "Create new Build Planner Preset in selected project directory"), GUILayout.Width(24)))
                //        {
                //            var nPres = (BuildPlannerPreset)FGenerators.GenerateScriptable(Instantiate(Get.Planner), "BP_");
                //            if (nPres != null)
                //            {
                //                if (AssetDatabase.Contains(nPres))
                //                {
                //                    Get.Planner = nPres;
                //                }
                //            }
                //        }
                //    }
                //    else
                //        if (GUILayout.Button("Create New", GUILayout.Width(94))) Get.Planner = (BuildPlannerPreset)FGenerators.GenerateScriptable(Instantiate(Get.Planner), "BP_");
                //}
                //else
                //{
                //    if (wasAutoFileButtons)
                //    {
                //        if (GUILayout.Button(new GUIContent("+", "Create new Build Planner Preset in selected project directory"), GUILayout.Width(24)))
                //        {
                //            var nPres = (BuildPlannerPreset)FGenerators.GenerateScriptable(CreateInstance<BuildPlannerPreset>(), "BP_");
                //            if (nPres != null)
                //            {
                //                if (AssetDatabase.Contains(nPres))
                //                {
                //                    Get.Planner = nPres;
                //                }
                //            }
                //        }
                //    }
                //    else
                //        if (GUILayout.Button("Create New", GUILayout.Width(94))) Get.Planner = (BuildPlannerPreset)FGenerators.GenerateScriptable(CreateInstance<BuildPlannerPreset>(), "BP_");
                //}


                if (GUILayout.Button(PGGUtils.TEX_MenuIcon, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(19)))
                {

                    GenericMenu draftsMenu = new GenericMenu();

                    #region Move Preset Options

                    if (isInDefaultDirectory)
                    {
                        if (Get.BuildPlannerPreset != null)
                            draftsMenu.AddItem(new GUIContent("Move current preset to choosed game project directory"), false, () =>
                            {

                                string path = FGenerators.GetPathPopup("Move BuildPlannerPreset file to new directory in project", "BP_Your BuildPlanner");
                                if (!string.IsNullOrEmpty(path))
                                {
                                    UnityEditor.AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(Get.BuildPlannerPreset), path);
                                    AssetDatabase.SaveAssets();
                                }

                                wasChecked = null;

                            });
                    }
                    else
                    {
                        if (Get.BuildPlannerPreset != null)
                            draftsMenu.AddItem(new GUIContent("Move preset file from project to default PGG Drafts directory"), false, () =>
                            {

                                string path = AssetDatabase.GetAssetPath(Get._plannerDraftsDirectory);

                                if (!string.IsNullOrEmpty(path))
                                {
                                    path += "/" + Get.BuildPlannerPreset.name + ".asset";
                                    UnityEditor.AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(Get.BuildPlannerPreset), path);
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
                                Get.BuildPlannerPreset = newPres;
                        }
                    });


                    draftsMenu.ShowAsContext();

                }

                GUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = 0;

                #endregion


                #endregion

                bool allowDisplaySetup = true;

                if (Get.BuildPlannerPreset == null) allowDisplaySetup = false;

                if (allowDisplaySetup)
                    if (Get._Editor_SelectedSetup >= Get.BuildPlannerPreset.BasePlanners.Count)
                    {
                        allowDisplaySetup = false;
                        Get._Editor_SelectedSetup = 0;
                    }

                if (allowDisplaySetup)
                {
                    if (Get.ResetPlannerComposition())
                    {
                        serializedObject.Update();
                        serializedObject.ApplyModifiedProperties();
                    }

                    string naming = Get.BuildPlannerPreset.name.Replace("BP_", "");

                    //if (naming.StartsWith("B"))
                    //{
                    //    naming = naming.Replace("BP_", "");
                    //}

                    for (int i = 0; i < Get.PlannerPrepare.FieldSetupCompositions.Count; i++) if (Get.PlannerPrepare.FieldSetupCompositions[i].IsSettedUp == false) { filled = false; break; }


                    GUILayout.BeginVertical();

                    if (Get.BuildPlannerPreset.BuildVariables.Count > 0)
                    {
                        GUILayout.Space(8f);

                        GUILayout.BeginHorizontal();

                        if (filled)
                        {
                            if (GUILayout.Button(new GUIContent(Get._Editor_drawMainVariables ? FGUI_Resources.Tex_DownFoldGray : FGUI_Resources.Tex_UpFoldGray), EditorStyles.label, GUILayout.Width(24), GUILayout.Height(18))) Get._Editor_drawMainVariables = !Get._Editor_drawMainVariables;
                        }

                        if (GUILayout.Button("<color=#AAAAFF>" + naming + "</color>  Setup", FGUI_Resources.HeaderStyle)) Get._Editor_drawMainVariables = !Get._Editor_drawMainVariables;

                        if (filled)
                        {
                            if (GUILayout.Button(new GUIContent(Get._Editor_drawMainVariables ? FGUI_Resources.Tex_DownFoldGray : FGUI_Resources.Tex_UpFoldGray), EditorStyles.label, GUILayout.Width(24), GUILayout.Height(18))) Get._Editor_drawMainVariables = !Get._Editor_drawMainVariables;
                        }

                        if (Get.BuildPlannerPreset)
                        {
                            if (!string.IsNullOrEmpty(Get.BuildPlannerPreset.CustomInfo))
                            {
                                if (GUILayout.Button(FGUI_Resources.GUIC_Info, FGUI_Resources.ButtonStyle, GUILayout.Width(21), GUILayout.Height(18)))
                                {
                                    EditorUtility.DisplayDialog(Get.BuildPlannerPreset.name + " Info:", Get.BuildPlannerPreset.CustomInfo, "Ok");
                                }
                            }
                        }

                        GUILayout.EndHorizontal();
                    }

                    //GUILayout.LabelField(naming + " Setup", FGUI_Resources.HeaderStyle);
                    //GUILayout.LabelField("Prepare the  <color=#AAAAFF>" + naming + "</color>  Plan Generating", FGUI_Resources.HeaderStyle);

                    if (filled)
                    {
                        GUILayout.Space(3f);

                        if (Get.BuildPlannerPreset.BuildVariables.Count > 0)
                        {
                            //FGUI_Inspector.FoldHeaderStart(ref drawMainVariables, "Main Parameters", FGUI_Resources.BGInBoxStyle);

                            EditorGUI.BeginChangeCheck();

                            if (Get.BuildPlannerPreset.BuildVariables.Count != Get.PlannerPrepare.PlannerVariablesOverrides.Count)
                            {
                                FieldVariable.UpdateVariablesWith(Get.PlannerPrepare.PlannerVariablesOverrides, Get.BuildPlannerPreset.BuildVariables);
                            }

                            if (Get._Editor_drawMainVariables)
                            {
                                GUILayout.Space(3f);

                                if (Get.BuildPlannerPreset.BuildVariables.Count == 0)
                                {
                                    GUILayout.Space(6f);
                                    GUILayout.Label("Build Planner don't have any variables", EditorStyles.centeredGreyMiniLabel);
                                    GUILayout.Space(6f);
                                }
                                else
                                {
                                    for (int i = 0; i < Get.PlannerPrepare.PlannerVariablesOverrides.Count; i++)
                                    {
                                        Get.PlannerPrepare.PlannerVariablesOverrides[i].UpdateVariableWith(Get.BuildPlannerPreset.BuildVariables[i]);
                                        object tr = FieldVariable.Editor_DrawTweakableVariable(Get.PlannerPrepare.PlannerVariablesOverrides[i], Get.PlannerPrepare.PlannerVariablesOverrides[i].additionalHelperRef);

                                        if (tr is Transform)
                                        {
                                            Transform trs = tr as Transform;
                                            Get.PlannerPrepare.PlannerVariablesOverrides[i].additionalHelperRef = trs;
                                        }
                                        else
                                        {
                                            Get.PlannerPrepare.PlannerVariablesOverrides[i].additionalHelperRef = null;
                                        }
                                    }
                                }
                            }

                            if (EditorGUI.EndChangeCheck()) Dirty();

                            GUILayout.Space(3f);

                        }
                        else
                        {
                            //GUILayout.HelpBox("No Build Variables in Planner Preset", MessageType.None);
                        }

                    }
                    GUILayout.EndVertical();

                    GUILayout.Space(6f);

                    if (Get.BuildPlannerPreset.BasePlanners.Count > 0)
                    {
                        EditorGUI.BeginChangeCheck();

                        GUILayout.Space(2f);
                        GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

                        if (!filled)
                        {
                            EditorGUILayout.HelpBox("  Fill all references with FieldSetups, to start setting up details", MessageType.Info);

                            GUILayout.Space(2f);

                            for (int i = 0; i < Get.PlannerPrepare.FieldSetupCompositions.Count; i++)
                            {
                                var compos = Get.PlannerPrepare.FieldSetupCompositions[i];

                                //FieldSetup pre = compos.Setup;

                                if (Get.BuildPlannerPreset.BasePlanners[i] != null)
                                {
                                    if (Get.BuildPlannerPreset.BasePlanners[i].FieldType == FieldPlanner.EFieldType.InternalField)
                                    {
                                        compos.GenType = EPGGGenType.None;
                                        continue;
                                    }


                                    DrawGenTypeField(compos, Get.BuildPlannerPreset.BasePlanners[i].name);
                                    //compos.Setup = (FieldSetup)EditorGUILayout.ObjectField(Get.BuildPlannerPreset.BasePlanners[i].name, compos.Setup, typeof(FieldSetup), false);

                                    if (_drawGenTypeChanged)
                                    {
                                        compos.Clear();
                                        compos.RefreshPlannerShapesSupport(Get.BuildPlannerPreset.BasePlanners[i]);
                                        _drawGenTypeChanged = false;
                                        //UnityEngine.Debug.Log("change");
                                    }
                                }
                            }
                        }
                        else
                        {

                            if (Get.BuildPlannerPreset)
                                if (Get._Editor_displaySetup)
                                {
                                    var viewPlanner = Get.BuildPlannerPreset.BasePlanners[Get._Editor_SelectedSetup];
                                    Color plCol = PlanGenerationPrint.GeneratePlannerColor(viewPlanner.IndexOnPreset, 0.8f, 1f);

                                    GUILayout.Space(1);

                                    GUILayout.BeginHorizontal(GUILayout.Height(20)); GUILayout.ExpandWidth(true);

                                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_DownFoldGray), EditorStyles.label, GUILayout.Width(24), GUILayout.Height(18)))
                                    {
                                        Get._Editor_displaySetup = !Get._Editor_displaySetup;
                                    }

                                    GUILayout.Space(-2);
                                    if (GUILayout.Button(PGGUtils._PlannerIcon, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
                                    {
                                        AssetDatabase.OpenAsset(viewPlanner);
                                    }
                                    GUILayout.Space(4);

                                    Get._Editor_SelectedSetup = EditorGUILayout.IntPopup(Get._Editor_SelectedSetup, Get.GetSetupsNameList(), Get.GetSetupsIDList());

                                    GUILayout.Space(2);

                                    ComposSetupType(Get.PlannerPrepare.FieldSetupCompositions[Get._Editor_SelectedSetup]);

                                    GUI.color = plCol;
                                    EditorGUILayout.LabelField(PGGUtils._CellIcon, GUILayout.Width(20));
                                    GUI.color = preGuiC;

                                    //GUILayout.Space(4);

                                    //if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_LeftFold), GUILayout.Height(20))) { SwitchPlannerSelection(Get._Editor_SelectedSetup - 1); }
                                    ////GUILayout.LabelField(" " + (selectedSetup + 1) + " / " + Get.Planner.Planners.Count, FGUI_Resources.HeaderStyle);
                                    //if (GUILayout.Button(" " + (Get._Editor_SelectedSetup + 1) + " / " + Get.Planner.Planners.Count + "  ", FGUI_Resources.HeaderStyle, GUILayout.ExpandWidth(true))) { displaySetup = !displaySetup; }
                                    //if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_RightFold), GUILayout.Height(20))) { SwitchPlannerSelection(Get._Editor_SelectedSetup + 1); }
                                    GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 1f);
                                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Remove, "Reset all setups to default"), FGUI_Resources.ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                                    {
                                        if (UnityEditor.EditorUtility.DisplayDialog("Are you sure?", "Are you sure to reset settings of the setups?", "Yes", "No"))
                                            Get.ResetSetups();
                                    }

                                    GUILayout.EndHorizontal();
                                    GUILayout.Space(2f);

                                    FieldSetupComposition selected = Get.PlannerPrepare.FieldSetupCompositions[Get._Editor_SelectedSetup];

                                    if (viewPlanner == null || selected == null)
                                    {
                                        EditorGUILayout.HelpBox("Target planner is null! Some unexpected error occured, try removing empty planners from the BuildPlannerPreset", MessageType.Error);
                                    }
                                    else
                                    {
                                        //GUILayout.LabelField(viewPlanner.name + " Setup", FGUI_Resources.HeaderStyle);
                                        GUILayout.Space(3f);

                                        GUI.color = plCol;
                                        GUI.backgroundColor = PlanGenerationPrint.GeneratePlannerColor(viewPlanner.IndexOnPreset, 0.8f, 1f);
                                        GUILayout.BeginHorizontal(FGUI_Resources.BGInBoxStyle);
                                        GUI.color = preGuiC;

                                        selected.OverrideEnabled = EditorGUILayout.Toggle(GUIContent.none, selected.OverrideEnabled, GUILayout.Width(18));
                                        //selected.Setup = (FieldSetup)EditorGUILayout.ObjectField(selected.Setup, typeof(FieldSetup), false);

                                        if (DrawGenSelector(selected))
                                        {
                                            selected.PrepareWithCurrentlyChoosed(Get, Get.BuildPlannerPreset.BasePlanners[Get._Editor_SelectedSetup]);
                                            selected.Prepared = false;
                                        }

                                        GUI.backgroundColor = preBg;

                                        FieldSetupComposition.DrawCompositionGUI2(this, selected, viewPlanner);

                                        GUILayout.EndHorizontal();

                                        BuildPlannerExecutor.PlannerDuplicatesSupport overrSupp = null;

                                        bool useOverr = false;
                                        if (selected.ParentFieldPlanner)
                                        {
                                            if (selected.ParentFieldPlanner.Duplicates > 0 || selected.ParentFieldPlanner.ExposeInstanceCount)
                                            {
                                                FGUI_Inspector.DrawUILine(0.4f, 0.3f, 1, 8);

                                                GUILayout.BeginHorizontal();

                                                if (selected.ParentFieldPlanner)
                                                {
                                                    if (selected.ParentFieldPlanner.ExposeInstanceCount)
                                                    {
                                                        selected.Instances = EditorGUILayout.IntField("Instances: ", selected.Instances);
                                                        if (selected.Instances < 1) selected.Instances = 1;
                                                    }
                                                    else
                                                    {
                                                        selected.Instances = selected.ParentFieldPlanner.Instances;
                                                        if (selected.Instances < 1) selected.Instances = 1;
                                                    }
                                                }

                                                EditorGUIUtility.labelWidth = 94;
                                                GUILayout.Space(7);

                                                if (Get.PlannerPrepare.DuplicatesOverrides.Count > 0 && Get._Editor_SelectedSetup < Get.PlannerPrepare.DuplicatesOverrides.Count)
                                                {
                                                    // Fill main lists count adjustements
                                                    Get.AdjustDuplicatesCounts();
                                                    Get.AdjustTargetDuplicatesCount(Get._Editor_SelectedSetup);

                                                    Get.PlannerPrepare.UseDuplicatesOverrides[Get._Editor_SelectedSetup] = EditorGUILayout.Toggle(new GUIContent("Use Overrides", "Allowing to use separated composition / shape generator for each of the duplicate"), Get.PlannerPrepare.UseDuplicatesOverrides[Get._Editor_SelectedSetup], GUILayout.Width(120));
                                                    GUILayout.EndHorizontal();

                                                    useOverr = Get.PlannerPrepare.UseDuplicatesOverrides[Get._Editor_SelectedSetup];
                                                    overrSupp = Get.PlannerPrepare.DuplicatesOverrides[Get._Editor_SelectedSetup];
                                                }
                                                else
                                                {
                                                    Get.AdjustDuplicatesCounts();
                                                    Get.AdjustTargetDuplicatesCount(Get._Editor_SelectedSetup);
                                                }

                                            }
                                        }


                                        if (useOverr)
                                        {
                                            GUILayout.Space(4);
                                            GUI.color = Color.green;
                                            GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyleH);
                                            GUI.color = preGuiC;

                                            if (selectedDupl >= selected.Duplicates) selectedDupl = 0;
                                            if (Get._Editor_SelectedSetup >= overrSupp.DuplicatesCompositions.Count)
                                            {
                                                EditorGUILayout.HelpBox("Refresh Required", MessageType.None);
                                            }
                                            else
                                            {
                                                GUILayout.BeginHorizontal();

                                                if (overrSupp.DuplicatesCompositions.Count != selected.Duplicates)
                                                    PGGUtils.AdjustCount(overrSupp.DuplicatesCompositions, selected.Duplicates, false);

                                                var idList = GetDuplicatesIDList(Get._Editor_SelectedSetup, selected.Duplicates, selected.ParentFieldPlanner.name);
                                                selectedDupl = EditorGUILayout.IntPopup(selectedDupl, GetDuplicatesNameList(false), idList, GUILayout.MinWidth(90));

                                                FieldSetupComposition duplCompos = Get.PlannerPrepare.DuplicatesOverrides[Get._Editor_SelectedSetup].DuplicatesCompositions[selectedDupl];

                                                if (duplCompos == null)
                                                {
                                                    overrSupp.DuplicatesCompositions[selectedDupl] = new FieldSetupComposition();
                                                    duplCompos = overrSupp.DuplicatesCompositions[selectedDupl];
                                                    duplCompos.ParentFieldPlanner = Get.BuildPlannerPreset.BasePlanners[Get._Editor_SelectedSetup];
                                                }

                                                if (duplCompos.Setup == null)
                                                {
                                                    duplCompos.Setup = Get.PlannerPrepare.FieldSetupCompositions[Get._Editor_SelectedSetup].Setup;
                                                }

                                                if (duplCompos.Prepared)
                                                {
                                                    //FieldSetup preSetup = duplCompos.Setup;
                                                    if (DrawGenSelector(duplCompos))
                                                    {
                                                        duplCompos.PrepareWithCurrentlyChoosed(Get, Get.BuildPlannerPreset.BasePlanners[Get._Editor_SelectedSetup]);
                                                    }
                                                }

                                                FieldSetupComposition.DrawCompositionGUI2(this, duplCompos, viewPlanner);

                                                GUILayout.EndHorizontal();
                                            }

                                        }


                                        GUILayout.Space(6f);


                                        if (selected.ParentFieldPlanner)
                                            if (selected.ParentFieldPlanner.ShapeGenerator != null)
                                            {
                                                if (selected.InitShapes != null)
                                                    if (selected.InitShapes.Count > 0)
                                                        if (
                                                            selected.InitShapes[0] == null
                                                            || selected.ParentFieldPlanner.ShapeGenerator.GetType() != selected.InitShapes[0].GetType()
                                                            )
                                                        {
                                                            selected.ReloadShape();
                                                        }

                                                if (selected.ParentFieldPlanner.ShapeGenerator is Planning.GeneratingLogics.SG_NoShape)
                                                {

                                                }
                                                else
                                                {

                                                    if (selected.ParentFieldPlanner.ExposeShape)
                                                    {

                                                        #region Initial Shape

                                                        // Init shape
                                                        if (Get._Editor_PreviewingShape) GUI.backgroundColor = Color.green;
                                                        GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyleH);
                                                        GUILayout.BeginHorizontal();
                                                        if (Get._Editor_PreviewingShape) GUI.backgroundColor = preBg;
                                                        GUI.backgroundColor = new Color(.2f, 1f, .8f, 1f);
                                                        FGUI_Inspector.FoldHeaderStart(ref Get._Editor_drawInitShape, " Prepare Initial Shape   <color=#888888><size=10>for " + selected.ParentFieldPlanner.name + "</size></color>");

                                                        if (Get._Editor_PreviewingShape)
                                                        {
                                                            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Refresh), EditorStyles.label, GUILayout.Height(17), GUILayout.Width(20)))
                                                            {
                                                                if (Get.RandomSeed == false) FGenerators.SetSeed(Get.Seed);
                                                                viewPlanner._tempOverrideShape = selected.InitShapes[0];
                                                                viewPlanner.PrepareInitialChecker();
                                                                SceneView.RepaintAll();
                                                            }
                                                        }

                                                        GUI.backgroundColor = Get._Editor_PreviewingShape ? Color.green : Color.white;
                                                        ShapeGeneratorBase displayShape = selected.InitShapes[0];

                                                        if (useOverr)
                                                        {

                                                            if (overrSupp.DuplicatesShapes[selectedDupl] == null)
                                                            {
                                                                if (GUILayout.Button("Override", GUILayout.Width(70)))
                                                                {
                                                                    overrSupp.DuplicatesShapes[selectedDupl] = (ShapeGeneratorBase)Instantiate(selected.InitShapes[0]);
                                                                    displayShape = overrSupp.DuplicatesShapes[selectedDupl];
                                                                }
                                                            }
                                                            else
                                                            {
                                                                GUI.backgroundColor = Color.green;
                                                                displayShape = overrSupp.DuplicatesShapes[selectedDupl];

                                                                if (GUILayout.Button("Overriding", GUILayout.Width(76)))
                                                                {
                                                                    if (overrSupp.DuplicatesShapes[selectedDupl] != null)
                                                                        DestroyImmediate(overrSupp.DuplicatesShapes[selectedDupl]);

                                                                    overrSupp.DuplicatesShapes[selectedDupl] = null;
                                                                    displayShape = selected.InitShapes[0];
                                                                }
                                                                GUI.backgroundColor = preBGCol;
                                                            }
                                                        }

                                                        if (GUILayout.Button("Preview", GUILayout.Width(60)))
                                                        {
                                                            if (Get.RandomSeed == false) FGenerators.SetSeed(Get.Seed);
                                                            Get._Editor_PreviewingShape = !Get._Editor_PreviewingShape;
                                                            viewPlanner._tempOverrideShape = displayShape;
                                                            viewPlanner.PrepareInitialChecker();
                                                            SceneView.RepaintAll();
                                                        }

                                                        GUI.backgroundColor = preBg;

                                                        GUILayout.EndHorizontal();
                                                        GUILayout.EndVertical();

                                                        if (Get._Editor_drawInitShape)
                                                        {
                                                            if (Get._Editor_PreviewingShape) GUI.backgroundColor = Color.green;
                                                            GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                                                            GUI.backgroundColor = preBg;

                                                            GUILayout.Space(3);

                                                            if (selected.InitShapes == null || selected.InitShapes.Count == 0) selected.RefreshWith(this, viewPlanner, selected.Setup);

                                                            var shape = displayShape;

                                                            SerializedObject selShape = new SerializedObject(shape);
                                                            GUILayout.Space(1);
                                                            GUILayout.Label(shape.TitleName(), EditorStyles.centeredGreyMiniLabel);
                                                            GUILayout.Space(7);

                                                            EditorGUI.BeginChangeCheck();
                                                            shape.DrawGUI(selShape);

                                                            if (EditorGUI.EndChangeCheck())
                                                            {
                                                                selShape.ApplyModifiedProperties();
                                                                EditorUtility.SetDirty(Get);
                                                            }

                                                            //FGenerators.DrawAllPropsOf(selShape);
                                                            GUILayout.Space(5);

                                                            GUILayout.EndVertical();
                                                        }

                                                        GUI.backgroundColor = preBg;


                                                        #endregion

                                                    }

                                                }

                                            }



                                        if (useOverr) GUILayout.EndVertical();


                                        if (selected.GenType != EPGGGenType.None)
                                        {

                                            FGUI_Inspector.DrawUILine(0.4f, 0.3f, 1, 8);


                                            if (viewPlanner.FVariables.Count == 0)
                                            {
                                                GUILayout.Space(-1f);
                                                GUILayout.Label("Field Planner don't have any variables", EditorStyles.centeredGreyMiniLabel);
                                                GUILayout.Space(-4f);
                                            }
                                            else
                                            {
                                                if (selected.ParentFieldPlanner)
                                                    EditorGUILayout.LabelField("Prepare Field Variables of " + selected.ParentFieldPlanner.name, EditorStyles.centeredGreyMiniLabel);

                                                // Variables
                                                GUILayout.Space(4f);

                                                if (selected.PlannerVariablesOverrides.Count != selected.ParentFieldPlanner.Variables.Count)
                                                {
                                                    selected.RefreshPlannerVariablesCount(selected.ParentFieldPlanner.Variables);
                                                }

                                                for (int i = 0; i < selected.PlannerVariablesOverrides.Count; i++)
                                                {
                                                    selected.PlannerVariablesOverrides[i].UpdateVariableWith(selected.ParentFieldPlanner.Variables[i]);

                                                    if (selected.PlannerVariablesOverrides[i].helpForFieldCommand)
                                                    {
                                                        selected.PlannerVariablesOverrides[i].helpForFieldCommandRef = selected.Setup;
                                                    }

                                                    object tr = FieldVariable.Editor_DrawTweakableVariable(selected.PlannerVariablesOverrides[i], selected.PlannerVariablesOverrides[i].additionalHelperRef);

                                                    if (tr is Transform)
                                                    {
                                                        Transform trs = tr as Transform;
                                                        selected.PlannerVariablesOverrides[i].additionalHelperRef = trs;
                                                    }
                                                    else
                                                    {
                                                        selected.PlannerVariablesOverrides[i].additionalHelperRef = null;
                                                    }
                                                }

                                                GUILayout.Space(4f);
                                            }

                                        }

                                    }

                                    GUILayout.Space(5f);
                                }
                                else
                                {
                                    GUILayout.Space(2);
                                    GUILayout.BeginHorizontal(GUILayout.Height(20)); GUILayout.ExpandWidth(true);

                                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_RightFoldGray), EditorStyles.label, GUILayout.Width(24), GUILayout.Height(18)))
                                    {
                                        Get._Editor_displaySetup = !Get._Editor_displaySetup;
                                    }

                                    Get._Editor_SelectedSetup = EditorGUILayout.IntPopup(Get._Editor_SelectedSetup, Get.GetSetupsNameList(), Get.GetSetupsIDList());
                                    GUILayout.EndHorizontal();
                                    GUILayout.Space(2f);

                                    //GUILayout.BeginHorizontal(GUILayout.Height(20));
                                    //if (GUILayout.Button("▲", EditorStyles.label, GUILayout.Width(26))) { displaySetup = !displaySetup; }
                                    //if (GUILayout.Button(" " + (Get._Editor_SelectedSetup + 1) + " / " + Get.Planner.Planners.Count + " ", FGUI_Resources.HeaderStyle, GUILayout.ExpandWidth(true))) { displaySetup = !displaySetup; }
                                    //if (GUILayout.Button("▲", EditorStyles.label, GUILayout.Width(26))) { displaySetup = !displaySetup; }
                                    //GUILayout.EndHorizontal();
                                    //GUILayout.Space(2f);
                                }

                        }

                        if (EditorGUI.EndChangeCheck()) Dirty();

                        GUILayout.EndVertical();
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("BuildPlannerPreset don't contains any Planner!", MessageType.Warning);
                    }

                    FGUI_Inspector.DrawUILine(0.4f, 0.3f, 1, 16, 1);
                    GUILayout.BeginHorizontal();

                    if (!filled) GUI.enabled = false;
                    GUI.backgroundColor = new Color(0.67f, 0.82f, 1f, 1f);
                    if (GUILayout.Button(new GUIContent("  Generate Preview ", FGUI_Resources.Tex_Refresh), GUILayout.Height(20)))
                    {
                        Get.GeneratePreview();
                    }
                    GUI.backgroundColor = preBg;
                    if (!filled) GUI.enabled = true;

                    GUILayout.Space(7);
                    EditorGUIUtility.labelWidth = 44; // Async
                    EditorGUILayout.PropertyField(sp_Async, GUILayout.Width(68));
                    SerializedProperty spc = sp_Async.Copy(); spc.Next(false);
                    EditorGUIUtility.labelWidth = 10; // Async
                    EditorGUILayout.PropertyField(spc, GUILayout.Width(32));

                    EditorGUIUtility.labelWidth = 40; // Seed
                    SerializedProperty sp_seed = sp_Seed.Copy();
                    //GUILayout.Space(3);

                    if (Get.RandomSeed)
                    {
                        GUI.enabled = false;
                        EditorGUILayout.IntField("Seed", Get.Seed, GUILayout.Width(89)); sp_seed.Next(false);
                        if (Get.RandomSeed) GUI.enabled = preE;
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(sp_seed, GUILayout.Width(89)); sp_seed.Next(false);
                    }

                    GUILayout.Space(6);
                    EditorGUIUtility.labelWidth = 24;
                    EditorGUILayout.PropertyField(sp_seed, new GUIContent(FGUI_Resources.Tex_Random, "Random seed every generation"), GUILayout.Width(44));
                    EditorGUIUtility.labelWidth = 0;
                    GUILayout.Space(6);

                    GUILayout.EndHorizontal();

                    GUILayout.Space(6);

                    if (filled)
                    {
                        bool toClear = false;

                        if (Get.Generated.Count > 0 || Get.GeneratedPreview != null) toClear = true;

                        GUILayout.BeginHorizontal();
                        //if (GUILayout.Button("Generate"))
                        //{
                        //    Get.Generate();
                        //}

                        GUILayoutOption opt;
                        if (toClear) opt = GUILayout.Height(25); else opt = GUILayout.Height(25);

                        GUI.backgroundColor = new Color(0.75f, 1f, 0.85f, 1f);
                        if (GUILayout.Button(new GUIContent("   Generate Objects ", FGUI_Resources.Tex_GearSetup), opt))
                        {
                            Get.SwitchFromPreviewGen();
                            if (Get.Generated.Count > 0) Get.ClearGenerated();
                            Get.GenerateGridPainters();
                            Dirty();
                        }
                        GUI.backgroundColor = preBg;

                        if (toClear) if (GUILayout.Button(new GUIContent("  Clear ", FGUI_Resources.Tex_Remove), opt))
                            {
                                Get.ClearGenerated();
                                SceneView.RepaintAll();
                                Dirty();
                            }

                        if (displayEvent) GUI.color = new Color(.7f, .7f, .7f, 1f);
                        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("EventSystem Icon").image, "Display 'After Generating' Event"), GUILayout.Width(28), GUILayout.Height(25)))
                        {
                            displayEvent = !displayEvent;
                        }
                        if (displayEvent) GUI.color = preGuiC;

                        //GUILayout.FlexibleSpace();
                        GUILayout.Space(8);
                        EditorGUIUtility.labelWidth = 110;
                        EditorGUILayout.PropertyField(sp_GenerateOnStart, GUILayout.Width(130));
                        EditorGUIUtility.labelWidth = 0;

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.Space(2);

                }
                else
                {
                    GUILayout.Space(4);

                    if (Get.BuildPlannerPreset == null)
                        EditorGUILayout.HelpBox("Assign Build Planner Preset to display options", MessageType.Info);
                    else
                        EditorGUILayout.HelpBox("Empty Build Planner! Prepare some Field Planners in the 'Build Planner' window", MessageType.Info);
                }

                GUILayout.Space(6f);


                if (Get.BuildPlannerPreset != null)
                {
                    if (filled)
                    {
                        if (Get.Async)
                        {
                            if (Get.FlexibleGen)
                                EditorGUILayout.HelpBox("As for now, async+coroutine generating is not using grid painters", MessageType.None);
                        }

                        //EditorGUILayout.BeginHorizontal();
                        //GUILayout.Space(10);
                        //displayEvent = EditorGUILayout.Foldout(displayEvent, "Event After Generating", true);
                        //EditorGUILayout.EndHorizontal();

                        if (displayEvent)
                        {
                            GUILayout.Space(4);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("RunAfterGenerating"));
                            GUILayout.Space(-2);
                        }
                    }
                }

                GUILayout.Space(2f);
                serializedObject.ApplyModifiedProperties();

            }
            catch (System.Exception exc)
            {
                if (PGGUtils._Editor_IsExceptionIgnored(exc) == false)
                {
                    UnityEngine.Debug.Log("[Harmless PGG GUI Warning] Exception Index = " + exc.HResult);
                    Debug.LogException(exc);
                }
            }

            Gizmos.matrix = Matrix4x4.identity;
            Handles.matrix = Gizmos.matrix;
            onEnabled = true;

            PGGUtils.EndVerticalIfLightSkin();
        }



        /// <summary> True if changed </summary>
        public bool DrawGenSelector(FieldSetupComposition compos)
        {
            bool changed = false;

            if (compos.GenType == EPGGGenType.FieldSetup)
            {
                FieldSetup p = compos.Setup; compos.Setup = (FieldSetup)EditorGUILayout.ObjectField(compos.Setup, typeof(FieldSetup), true);
                if (p != compos.Setup) changed = true;
            }
            else if (compos.GenType == EPGGGenType.Modificator)
            {
                FieldModification p = compos.JustMod; compos.JustMod = (FieldModification)EditorGUILayout.ObjectField(compos.JustMod, typeof(FieldModification), true);
                if (p != compos.JustMod) changed = true;
            }
            else if (compos.GenType == EPGGGenType.ModPack)
            {
                ModificatorsPack p = compos.JustModPack; compos.JustModPack = (ModificatorsPack)EditorGUILayout.ObjectField(compos.JustModPack, typeof(ModificatorsPack), true);
                if (p != compos.JustModPack) changed = true;
            }

            if (changed)
            {
                compos.Clear();
                _drawGenTypeChanged = true;
                //UnityEngine.Debug.Log("change");
                compos.RefreshPlannerShapesSupport(compos.ParentFieldPlanner);
                //if ( compos.GenType == EPGGGenType.FieldSetup) compos.RefreshWith(this, compos.ParentFieldPlanner);
            }

            return changed;
        }



        bool displayEvent = false;

        int selectedDupl = 0;
        private int[] _DuplicIds = null;
        private int _lastDuplPlanIdx = -1;
        public int[] GetDuplicatesIDList(int plannerIndex, int targetDuplicates, string defaultName)
        {
            bool forceRefresh = false;
            if (plannerIndex != _lastDuplPlanIdx)
            {
                forceRefresh = true;
                _lastDuplPlanIdx = plannerIndex;
            }

            if (forceRefresh || _DuplicIds == null || _DuplicIds.Length != targetDuplicates)
            {
                _DuplicIds = new int[targetDuplicates];
                for (int i = 0; i < targetDuplicates; i++)
                {
                    _DuplicIds[i] = i;
                }

                GetDuplicatesNameList(true, defaultName);
            }

            return _DuplicIds;
        }

        private GUIContent[] _DuplicNames = null;
        public GUIContent[] GetDuplicatesNameList(bool forceRefresh, string defaultName = "")
        {
            if (_DuplicIds == null) return null;

            if (forceRefresh || _DuplicNames == null || _DuplicNames.Length != _DuplicIds.Length)
            {
                _DuplicNames = new GUIContent[_DuplicIds.Length];
                for (int i = 0; i < _DuplicIds.Length; i++)
                {
                    _DuplicNames[i] = new GUIContent("[" + (i + 1) + "] " + defaultName);
                }
            }
            return _DuplicNames;
        }


        BuildPlannerPreset targetPlanSelection = null;

        private void OnSceneGUI()
        {
            if (Selection.activeGameObject != Get.gameObject) return;

            if (Get.BuildPlannerPreset == null) return;
            if (Get.BuildPlannerPreset.BasePlanners.Count == 0) return;
            if (Get._Editor_SelectedSetup >= Get.BuildPlannerPreset.BasePlanners.Count)
            {
                Get._Editor_SelectedSetup = 0;
                if (Get._Editor_SelectedSetup >= Get.BuildPlannerPreset.BasePlanners.Count) return;
            }

            var viewPlanner = Get.BuildPlannerPreset.BasePlanners[Get._Editor_SelectedSetup];

            if (viewPlanner == null) return;
            if (Get.PlannerPrepare == null) return;
            if (Get._Editor_SelectedSetup >= Get.PlannerPrepare.FieldSetupCompositions.Count)
            {
                Get._Editor_SelectedSetup = 0;
                if (Get._Editor_SelectedSetup >= Get.PlannerPrepare.FieldSetupCompositions.Count) return;
            }



            for (int i = 0; i < Get.PlannerPrepare.PlannerVariablesOverrides.Count; i++)
            {
                if (Get.PlannerPrepare.PlannerVariablesOverrides[i].displayOnScene)
                {
                    if (Get.PlannerPrepare.PlannerVariablesOverrides[i].ValueType == FieldVariable.EVarType.Vector3)
                    {
                        Vector3 newVal = Get.PlannerPrepare.PlannerVariablesOverrides[i].GetVector3Value();

                        Vector3 wPos = Get.transform.TransformPoint(newVal);
                        Vector3 preVal = wPos;
                        Handles.SphereHandleCap(0, wPos, Quaternion.identity, 0.5f, EventType.Repaint);
                        Handles.Label(preVal + Vector3.up, new GUIContent(Get.PlannerPrepare.PlannerVariablesOverrides[i].Name));
                        //wPos = FEditor_TransformHandles.PositionHandle(wPos, Quaternion.identity, 2f);
                        //newVal = Get.transform.InverseTransformPoint(wPos);
                        //if ((preVal - newVal).sqrMagnitude > 0.0001f) Get.PlannerPrepare.PlannerVariablesOverrides[i].SetValue(newVal);
                    }
                }
            }



            var selected = Get.PlannerPrepare.FieldSetupCompositions[Get._Editor_SelectedSetup];

            for (int i = 0; i < selected.PlannerVariablesOverrides.Count; i++)
            {
                Transform trs = null;
                if (selected.PlannerVariablesOverrides[i].allowTransformFollow)
                    if (selected.PlannerVariablesOverrides[i].additionalHelperRef)
                    {
                        trs = selected.PlannerVariablesOverrides[i].additionalHelperRef as Transform;
                        if (trs)
                        {
                            if (onEnabled == false)
                            {
                                selected.PlannerVariablesOverrides[i].SetValue(Get.transform.InverseTransformPoint(trs.position));
                            }

                            //Matrix4x4 preMx = Handles.matrix;
                            //Handles.Label(trs.position + Vector3.up, new GUIContent(selected.PlannerVariablesOverrides[i].Name));
                            //Handles.SphereHandleCap(0, trs.position, trs.rotation, 0.3f, EventType.Repaint);
                            //selected.PlannerVariablesOverrides[i].SetValue(Get.transform.InverseTransformPoint(trs.position));
                            //Handles.matrix = preMx;
                        }
                    }

                if (selected.PlannerVariablesOverrides[i].displayOnScene)
                {
                    if (selected.PlannerVariablesOverrides[i].ValueType == FieldVariable.EVarType.Vector3)
                    {
                        Vector3 newVal = selected.PlannerVariablesOverrides[i].GetVector3Value();

                        Vector3 wPos = Get.transform.TransformPoint(newVal);
                        Vector3 preVal = wPos;

                        //Matrix4x4 preMx = Handles.matrix;
                        //Handles.matrix = Get.transform.localToWorldMatrix;
                        Handles.Label(preVal + Vector3.up, new GUIContent(selected.PlannerVariablesOverrides[i].Name));
                        wPos = FEditor_TransformHandles.PositionHandle(wPos, Quaternion.identity, 2f);

                        newVal = Get.transform.InverseTransformPoint(wPos);
                        if ((preVal - newVal).sqrMagnitude > 0.0001f) selected.PlannerVariablesOverrides[i].SetValue(newVal);

                        if (trs) trs.position = wPos;

                        //Handles.matrix = preMx;

                    }
                }
            }

            if (Get.GeneratedPreview == null) return;
            if (Get.GeneratedPainters.Count > 0) return;

            Handles.matrix = Get.transform.localToWorldMatrix;
            PlanGenerationPrint.DrawPrintHandles(Get.GeneratedPreview);
            Handles.matrix = Matrix4x4.identity;

            Handles.BeginGUI();
            PlanGenerationPrint.DrawCellsSceneGUI(12);
            Handles.EndGUI();
        }


        private void SwitchPlannerSelection(int target)
        {
            Get._Editor_SelectedSetup = target;
            if (Get._Editor_SelectedSetup >= Get.BuildPlannerPreset.BasePlanners.Count) Get._Editor_SelectedSetup = 0;
            if (Get._Editor_SelectedSetup < 0) Get._Editor_SelectedSetup = Get.BuildPlannerPreset.BasePlanners.Count - 1;
        }

        bool _drawGenTypeChanged = false;
        public bool DrawGenTypeField(FieldSetupComposition compos, string title)
        {
            _drawGenTypeChanged = false;
            Color preBg = GUI.backgroundColor;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(title, GUILayout.Width(120));

            bool ready = false;

            if (compos.GenType == EPGGGenType.None) { ready = true; EditorGUILayout.HelpBox("Disabled", MessageType.None); }
            if (compos.GenType == EPGGGenType.FieldSetup) { FieldSetup p = compos.Setup; compos.Setup = (FieldSetup)EditorGUILayout.ObjectField(compos.Setup, typeof(FieldSetup), false); if (compos.Setup != null) ready = true; if (p != compos.Setup) _drawGenTypeChanged = true; }
            else if (compos.GenType == EPGGGenType.Modificator) { FieldModification p = compos.JustMod; compos.JustMod = (FieldModification)EditorGUILayout.ObjectField(compos.JustMod, typeof(FieldModification), false); if (compos.JustMod != null) ready = true; if (p != compos.JustMod) _drawGenTypeChanged = true; }
            else if (compos.GenType == EPGGGenType.ModPack) { ModificatorsPack p = compos.JustModPack; compos.JustModPack = (ModificatorsPack)EditorGUILayout.ObjectField(compos.JustModPack, typeof(ModificatorsPack), false); if (compos.JustModPack != null) ready = true; if (p != compos.JustModPack) _drawGenTypeChanged = true; }

            if (ready) GUI.backgroundColor = new Color(0.5f, 0.9f, 0.5f, 1f);
            ComposSetupType(compos);
            if (ready) GUI.backgroundColor = preBg;
            if (_drawGenTypeChanged)
            {
                compos.RefreshPlannerShapesSupport(compos.ParentFieldPlanner);

                compos.Clear();
                //UnityEngine.Debug.Log("change");
            }
            EditorGUILayout.EndHorizontal();

            return ready;
        }

        public void ComposSetupType(FieldSetupComposition compos, int width = 88)
        {
            compos.GenType = (EPGGGenType)EditorGUILayout.EnumPopup(compos.GenType, GUILayout.Width(width));
        }

        void Dirty()
        {
            EditorUtility.SetDirty(Get);
        }

    }

}
#endif