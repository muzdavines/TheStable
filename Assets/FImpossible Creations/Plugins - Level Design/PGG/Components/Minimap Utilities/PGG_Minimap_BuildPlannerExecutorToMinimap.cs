using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

namespace FIMSpace.Generating
{
    [AddComponentMenu("FImpossible Creations/PGG/Minimap/PGG Build Planner Executor to Minimap")]
    [DefaultExecutionOrder(10)]
    public class PGG_Minimap_BuildPlannerExecutorToMinimap : MonoBehaviour
    {
        public PGG_MinimapHandler TargetMinimap;
        public BuildPlannerExecutor Executor;

        [Space(3)]
        public EGenerateMode GenerateTextureMode = EGenerateMode.TargetTextureResolution;
        [Tooltip("Border offset will improve look of room when it's rotated in any angle. Value is in world units scale.")]
        public float BorderPaddingOffset = 1f;
        [Tooltip("To add text ui object on top of generated field on the minimap")]
        public GameObject AddTextPrefab = null;

        [Space(5)]
        [HideInInspector] public FieldMinimapSetup DefaultSettings;


        void Reset()
        {
            Executor = GetComponent<BuildPlannerExecutor>();
            if (!Executor) Executor = GetComponentInChildren<BuildPlannerExecutor>();
            if (!Executor) Executor = GetComponentInParent<BuildPlannerExecutor>();
        }


        public void GenerateAfterPlannerExecutor()
        {
            if (Executor == null) return;
            if (Executor.GeneratedPreview == null) return;
            if (Executor.GeneratedPreview.PlannerResults == null) return;

            for (int g = 0; g < Executor.GeneratedGenerators.Count; g++)
            {
                var gen = Executor.GeneratedGenerators[g];
                BuildPlannerReference bpRef = gen.GetComponent<BuildPlannerReference>();

                var settings = GetSettingsFor(bpRef);
                var bSettings = GetBaseSettingsFor(bpRef);

                bool justImageExecuted = false;

                if (bSettings.JustImage)
                {
                    GameObject imagePf = settings.ImagePrefab;
                    if (imagePf == null) imagePf = DefaultSettings.ImagePrefab;

                    if (imagePf)
                    {
                        PGG_Minimap_WorldToMinimap wTo = gen.gameObject.AddComponent<PGG_Minimap_WorldToMinimap>();
                        wTo.TargetMinimap = TargetMinimap;
                        wTo.TargetLayer = EMinimapLayer.Background;
                        wTo.ColorizeGeneratedUI = settings.RoomPaintColor;
                        wTo.UIPrefab = imagePf;
                        justImageExecuted = true;
                    }
                }


                if (!justImageExecuted)
                {
                    PGG_Minimap_GridToMinimap mini = gen.gameObject.AddComponent<PGG_Minimap_GridToMinimap>();

                    mini.TargetMinimap = TargetMinimap;
                    mini.GenerateOutOf = gen;
                    mini.GenerateTextureMode = GenerateTextureMode;
                    mini.BorderPaddingOffset = BorderPaddingOffset;

                    mini.ImagePrefab = settings.ImagePrefab;
                    if (mini.ImagePrefab == null) mini.ImagePrefab = DefaultSettings.ImagePrefab;

                    if (settings.TargetResolution == 128)
                        mini.TargetResolution = bSettings.TargetResolution;
                    else
                        mini.TargetResolution = settings.TargetResolution;

                    if (mini.TargetResolution == 128)
                        mini.TargetResolution = DefaultSettings.TargetResolution;

                    mini.OverridePaintColor = settings.RoomPaintColor;

                    if (AddTextPrefab)
                        if (settings.ReplaceName != "")
                        {
                            mini.TextPrefab = AddTextPrefab;
                            mini.TextToSet = settings.ReplaceName;
                        }
                }

                //if (settings.ScaleBounds == 1f)
                //    mini.ScalePaintBounds = bSettings.ScaleBounds;
                //else
                //    mini.ScalePaintBounds = settings.ScaleBounds;

                if (AddTextPrefab)
                    PGG_Minimap_GridToMinimap.GenerateTextOn(AddTextPrefab, bpRef.GetWorldBoundsCenter, settings.ReplaceName, TargetMinimap);
            }

        }


        FieldMinimapSetup GetBaseSettingsFor(BuildPlannerReference bpRef)
        {
            FieldMinimapSetup settings = DefaultSettings;

            for (int o = 0; o < PlannerSettingsOverrides.Count; o++)
            {
                PlannerMinimapSetup minSetup = PlannerSettingsOverrides[o];
                if (!minSetup.MainOverride.Use) continue;
                if (o != bpRef.BuildPlannerIndex) continue;

                settings = minSetup.MainOverride;

                break;
            }

            return settings;
        }


        FieldMinimapSetup GetSettingsFor(BuildPlannerReference bpRef)
        {
            FieldMinimapSetup settings = DefaultSettings;

            for (int o = 0; o < PlannerSettingsOverrides.Count; o++)
            {
                PlannerMinimapSetup minSetup = PlannerSettingsOverrides[o];
                if (!minSetup.MainOverride.Use) continue;
                #region MyRegion
                //FieldPlanner rootPlanner = bpRef.Planner;
                //if (rootPlanner.IsDuplicate) rootPlanner = rootPlanner.DuplicateParent;
                //if (rootPlanner != Executor.BuildPlannerPreset.BasePlanners[i]) continue;
                #endregion
                if (o != bpRef.BuildPlannerIndex) continue;

                settings = minSetup.MainOverride;

                for (int i = 0; i < minSetup.InstancesOverrides.Count; i++)
                {
                    if (i != bpRef.BuildPlannerInstanceID) continue;

                    var instSett = minSetup.InstancesOverrides[i];
                    if (instSett.Use == false) continue;

                    settings = minSetup.InstancesOverrides[i];
                    break;
                }

                break;
            }

            return settings;
        }


        [System.Serializable]
        public class FieldMinimapSetup
        {
            [Tooltip("Maximum number of pixels for generated texture width or height (depending if grid is wider or taller)")]
            public int TargetResolution = 128;
            //[Range(0.5f, 1.25f)] public float ScaleBounds = 1f;
            public Color RoomPaintColor = Color.white;
            [Tooltip("At start there will be generated this prefab and texture will be applied to it's image component as sprite")]
            public GameObject ImagePrefab;

            [HideInInspector] public string ReplaceName = "";
            [HideInInspector] public bool Use = false;
            [Tooltip("Stretch provided image prefab on the generated field bounds - useful only for rectangular fields!")]
            [HideInInspector] public bool JustImage = false;
        }

        [System.Serializable]
        public class PlannerMinimapSetup
        {
            public FieldMinimapSetup MainOverride;
            public List<FieldMinimapSetup> InstancesOverrides;
        }


        [HideInInspector] public List<PlannerMinimapSetup> PlannerSettingsOverrides;

        public void RefreshOverridesList()
        {
            if (PlannerSettingsOverrides == null) PlannerSettingsOverrides = new List<PlannerMinimapSetup>();
            if (Executor.BuildPlannerPreset == null) return;
            if (Executor.BuildPlannerPreset.BasePlanners == null) return;

            PGGUtils.AdjustCount(PlannerSettingsOverrides, Executor.BuildPlannerPreset.BasePlanners.Count);

            for (int i = 0; i < PlannerSettingsOverrides.Count; i++)
            {
                var overr = PlannerSettingsOverrides[i];
                if (overr == null) continue;
                if (overr.InstancesOverrides == null) overr.InstancesOverrides = new List<FieldMinimapSetup>();
                PGGUtils.AdjustCount(overr.InstancesOverrides, Executor.BuildPlannerPreset.BasePlanners[i].Instances);
            }
        }


    }


    #region Editor Class

#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(PGG_Minimap_BuildPlannerExecutorToMinimap))]
    public class PGG_Minimap_BuildPlannerExecutorToMinimapEditor : UnityEditor.Editor
    {
        public PGG_Minimap_BuildPlannerExecutorToMinimap Get { get { if (_get == null) _get = (PGG_Minimap_BuildPlannerExecutorToMinimap)target; return _get; } }
        private PGG_Minimap_BuildPlannerExecutorToMinimap _get;

        SerializedProperty sp_Executor;
        SerializedProperty sp_OverridesList;
        SerializedProperty sp_DefaultSettings;

        int _selectedSetup = 0;
        int _selectedInstance = 0;
        bool _setupsFoldout = false;

        void OnEnable()
        {
            sp_DefaultSettings = serializedObject.FindProperty("DefaultSettings");
            sp_Executor = serializedObject.FindProperty("Executor");
            sp_OverridesList = serializedObject.FindProperty("PlannerSettingsOverrides");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (Get.Executor == null)
            {
                GUILayout.Space(4f);
                EditorGUILayout.PropertyField(sp_Executor);
                GUILayout.Space(4f);
                UnityEditor.EditorGUILayout.HelpBox("Build Planner Executor reference is needed!", UnityEditor.MessageType.Error);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            UnityEditor.EditorGUILayout.HelpBox("This component will help setting up minimap parameters for generated fields.", UnityEditor.MessageType.None);

            if (Get.PlannerSettingsOverrides != null) if (Get.PlannerSettingsOverrides.Count == 0) Get.RefreshOverridesList();

            GUILayout.Space(4f);
            DrawPropertiesExcluding(serializedObject, "m_Script");

            GUILayout.Space(7f);
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            GUILayout.Label("Default Settings:", EditorStyles.boldLabel);
            DrawMinimapSetup(sp_DefaultSettings.Copy(), Get.DefaultSettings);
            EditorGUILayout.EndVertical();

            GUILayout.Space(7f);
            BuildPlannerExecutor exc = Get.Executor;
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(FGUI_Resources.GetFoldSimbolTex(_setupsFoldout, true), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
            {
                _setupsFoldout = !_setupsFoldout;
            }

            if (GUILayout.Button("Individual Settings For:", EditorStyles.boldLabel, GUILayout.Width(140), GUILayout.Height(18)))
            {
                _setupsFoldout = !_setupsFoldout;
            }

            _selectedSetup = EditorGUILayout.IntPopup(_selectedSetup, exc.GetSetupsNameList(), exc.GetSetupsIDList());

            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Refresh, "If you changed number of rooms within Build Plan Executor, you should call this refresh button"), FGUI_Resources.ButtonStyle, GUILayout.Width(20), GUILayout.Height(18)))
            {
                Get.RefreshOverridesList();
            }

            EditorGUILayout.EndHorizontal();


            if (_setupsFoldout)
                if (Get.PlannerSettingsOverrides.Count > 0)
                {
                    if (_selectedSetup < 0 || _selectedSetup >= Get.PlannerSettingsOverrides.Count) _selectedSetup = 0;
                    PGG_Minimap_BuildPlannerExecutorToMinimap.PlannerMinimapSetup setupList = Get.PlannerSettingsOverrides[_selectedSetup];

                    if (setupList != null)
                    {
                        SerializedProperty sp_overr = sp_OverridesList.GetArrayElementAtIndex(_selectedSetup);
                        SerializedProperty sp_overrSet = sp_overr.FindPropertyRelative("MainOverride");

                        EditorGUILayout.BeginHorizontal();

                        EditorGUIUtility.labelWidth = 160;
                        setupList.MainOverride.Use = EditorGUILayout.ToggleLeft(" Override default settings for:", setupList.MainOverride.Use);
                        EditorGUIUtility.labelWidth = 0;

                        GUI.enabled = setupList.MainOverride.Use;

                        EditorGUILayout.ObjectField(Get.Executor.BuildPlannerPreset.BasePlanners[_selectedSetup], typeof(FieldSetup), true);
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(4f);
                        DrawMinimapSetup(sp_overrSet, setupList.MainOverride, true);
                        GUILayout.Space(4f);


                        if (GUI.enabled)
                        {
                            if (setupList.InstancesOverrides.Count > 1)
                            {
                                if (_selectedInstance >= setupList.InstancesOverrides.Count) _selectedInstance = 0;

                                var instanceOver = setupList.InstancesOverrides[_selectedInstance];

                                EditorGUILayout.BeginHorizontal();

                                EditorGUIUtility.labelWidth = 180;
                                instanceOver.Use = EditorGUILayout.ToggleLeft(" Override default settings for instance: [" + _selectedInstance + "]", instanceOver.Use);
                                EditorGUIUtility.labelWidth = 0;

                                GUILayout.Space(6);
                                if (GUILayout.Button(FGUI_Resources.Tex_ArrowLeft, FGUI_Resources.ButtonStyle, GUILayout.Width(20), GUILayout.Height(17)))
                                {
                                    _selectedInstance -= 1;
                                    if (_selectedInstance < 0) _selectedInstance = setupList.InstancesOverrides.Count - 1;
                                    if (_selectedInstance >= setupList.InstancesOverrides.Count) _selectedInstance = 0;
                                }
                                if (GUILayout.Button(FGUI_Resources.Tex_ArrowRight, FGUI_Resources.ButtonStyle, GUILayout.Width(20), GUILayout.Height(17)))
                                {
                                    _selectedInstance += 1;
                                    if (_selectedInstance >= setupList.InstancesOverrides.Count) _selectedInstance = 0;
                                }

                                EditorGUILayout.EndHorizontal();

                                if (instanceOver.Use)
                                {
                                    SerializedProperty sp_overrIns = sp_overr.FindPropertyRelative("InstancesOverrides").GetArrayElementAtIndex(_selectedInstance);
                                    DrawMinimapSetup(sp_overrIns, instanceOver);
                                }

                                GUILayout.Space(4f);

                            }
                        }


                        GUI.enabled = true;
                    }

                }

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(6);
            EditorGUILayout.HelpBox("To generate minimap, add after generation event in Executor component and call 'GenerateAfterPlannerExecutor' of this component! ", MessageType.Info);

        }

        void DrawMinimapSetup(SerializedProperty sp, PGG_Minimap_BuildPlannerExecutorToMinimap.FieldMinimapSetup setup, bool isRoomSet = false)
        {
            sp.Next(true); EditorGUILayout.PropertyField(sp, true);
            sp.Next(false); EditorGUILayout.PropertyField(sp, true);
            sp.Next(false); EditorGUILayout.PropertyField(sp, true);
            //sp.Next(false); EditorGUILayout.PropertyField(sp, true);
            if (Get.DefaultSettings == setup) return;

            sp.Next(false);
            if (Get.AddTextPrefab)
            {
                EditorGUILayout.PropertyField(sp, true);
            }

            if (isRoomSet)
            {
                sp.Next(false);
                sp.Next(false);
                EditorGUILayout.PropertyField(sp, true);
            }
        }
    }
#endif
    #endregion


}