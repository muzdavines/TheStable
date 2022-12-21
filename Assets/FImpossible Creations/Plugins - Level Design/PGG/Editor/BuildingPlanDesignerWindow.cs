using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FIMSpace.FEditor;
using UnityEditor.Callbacks;
using FIMSpace.Generating.Planning;

namespace FIMSpace.Generating
{
    public partial class BuildingPlanDesignerWindow : EditorWindow
    {
        public static BuildingPlanDesignerWindow Get;
        GameObject mainGeneratedsContainer;
        List<GameObject> generated;

        BuildPlanPreset tempPreset;
        BuildPlanPreset projectPreset;
        BuildPlanPreset selectedPreset;

        SerializedObject so_preset;

        Vector2 mainScroll = Vector2.zero;
        bool repaint = true;
        bool drawPrevSettings = true;
        bool drawLegend = true;
        //bool drawConditions = false;

        bool _editorRootFoldout = true;

        bool LimitSize = false;
        Vector2Int SizeLimitX = new Vector2Int(-10, 10);
        Vector2Int SizeLimitZ = new Vector2Int(-10, 10);

        bool GuideCorridors = false;
        int guideFatness = 1;
        float changeDirectionCost = .3f;
        public Vector2Int StartGuidePos = new Vector2Int(0, 0);
        public EPlanGuideDirecion StartGuideDirection = EPlanGuideDirecion.Back;

        public Vector2Int EndGuidePos = new Vector2Int(0, 10);
        public EPlanGuideDirecion EndGuideDirection = EPlanGuideDirecion.Forward;


        Color bgC;
        bool foldout = true;
        int select = 0;
        int seed = 0;


        [Tooltip("Separation distance between rooms walls to avoid rooms overlapping")]
        [Range(0f, .49f)]
        public float WallsSeparation = 0f;

        [MenuItem("Window/FImpossible Creations/Level Design/Building Plan Designer (Legacy)", false, 152)]
        static void Init()
        {
            BuildingPlanDesignerWindow window = (BuildingPlanDesignerWindow)GetWindow(typeof(BuildingPlanDesignerWindow));
            window.titleContent = new GUIContent("Building Plan", Resources.Load<Texture>("SPR_IGSmall"));
            window.Show();
            if (window.tempPreset == null) window.tempPreset = CreateInstance<BuildPlanPreset>();
            Get = window;
        }


        [OnOpenAssetAttribute(1)]
        public static bool OpenBuildPlanScriptableFile(int instanceID, int line)
        {
            Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj as BuildPlanPreset != null)
            {
                Init();
                Get.projectPreset = obj as BuildPlanPreset;
                Get.GeneratePlan();
                return true;
            }

            return false;
        }


        void OnGUI()
        {
            if (Get == null) Init();


            mainScroll = EditorGUILayout.BeginScrollView(mainScroll);

            EditorGUILayout.HelpBox("Build Plan preview is visible in scene view at position 0,0,0\nPreview is just for testing, building plan can be used by components like FacilityGenerator and more others in the future", MessageType.Info);


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

            string fold = drawPrevSettings ? " ▼" : " ►";
            if (GUILayout.Button(fold + "  " + "Preview Settings", EditorStyles.label, GUILayout.Width(200))) drawPrevSettings = !drawPrevSettings;

            if (drawPrevSettings)
            {
                GUILayout.Space(5f);

                EditorGUI.BeginChangeCheck();
                PreviewSize = EditorGUILayout.Slider("Preview Scale", PreviewSize, 0.1f, 4f);
                drawLegend = EditorGUILayout.Toggle("Rooms Legend", drawLegend);

                GUILayout.Space(4f);
                WallsSeparation = EditorGUILayout.Slider("Walls Separation", WallsSeparation, 0f, 0.49f);

                GUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                seed = EditorGUILayout.IntField("Seed: ", seed);
                if (seed == 0) EditorGUILayout.LabelField("(Random)", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(50));
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Random, "Add 1 to seed for controlled generating checking"), FGUI_Resources.ButtonStyle, GUILayout.Width(20), GUILayout.Height(18))) { seed += 1; }
                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = 0;

                if (EditorGUI.EndChangeCheck()) GeneratePlan();
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(3);

            #endregion


            GUILayout.Space(5);

            // Build plan preset field
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 60;
            projectPreset = (BuildPlanPreset)EditorGUILayout.ObjectField("Preset:", projectPreset, typeof(BuildPlanPreset), false);
            if (GUILayout.Button("Create New", GUILayout.Width(94))) projectPreset = (BuildPlanPreset)FGenerators.GenerateScriptable(Instantiate(selectedPreset), "BP_");
            EditorGUILayout.EndHorizontal();


            // Drawing all room presets
            GUILayout.Space(9f);
            BuildPlanPresetEditor.DrawPresetList(selectedPreset.Settings, FGUI_Resources.BGInBoxStyle, "Room Presets", ref foldout, ref select, false, true, null, true);
            if (foldout)
            {
                BuildPlanPresetEditor.DrawSettings(selectedPreset, select, true, true, so_preset);

            }

            GUILayout.Space(9);

            // Drawing root corridor settings
            if (projectPreset != null)
            {
                if (Get.projectPreset.RootChunkSetup.FieldSetup == null)
                    SingleInteriorSettingsEditor.DrawGUIChunkSetup("Root Corridor (0 !!)", Get.projectPreset.RootChunkSetup, so_preset, so_preset.FindProperty("RootChunkSetup"), ref Get._editorRootFoldout);
                else
                    SingleInteriorSettingsEditor.DrawGUIChunkSetup("Root Corridor", Get.projectPreset.RootChunkSetup, so_preset, so_preset.FindProperty("RootChunkSetup"), ref Get._editorRootFoldout, true);
            }

            GUILayout.Space(5);


            EditorGUIUtility.labelWidth = 0;
            if (selectedPreset != null) if (planHelper == null) GeneratePlan();


            GUILayout.Space(3);
            if (GUILayout.Button("Generate Preview")) GeneratePlan();
            GUILayout.Space(5);

            if (Get.projectPreset != null)
                for (int i = 0; i < Get.projectPreset.Settings.Count; i++)
                {
                    if (Get.projectPreset.Settings[i] != null)
                        if (Get.projectPreset.Settings[i].RestrictPosition)
                        {
                            EditorGUILayout.HelpBox("'Restrict Position' is not visible in preview, it can be used by generators", MessageType.None);
                            break;
                        }
                }

            if (SingleInteriorSettingsEditor.IfChanged())
            {
                if (projectPreset != null)
                    EditorUtility.SetDirty(projectPreset);
            }


            EditorGUILayout.HelpBox("There is few more options for build plan when you select build plan asset file and look to the inspector window", MessageType.None);

            GUILayout.Space(3);

            EditorGUILayout.EndScrollView();


            #region Ending, applying and restoring

            if (so_preset != null) so_preset.ApplyModifiedProperties();

            if (repaint)
            {
                SceneView.RepaintAll();
                repaint = false;
            }


            #endregion

        }

    }
}