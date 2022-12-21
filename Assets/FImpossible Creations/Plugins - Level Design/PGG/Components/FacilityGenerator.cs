using FIMSpace.Generating.PathFind;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif  
using UnityEngine;

namespace FIMSpace.Generating
{
    [AddComponentMenu("FImpossible Creations/PGG/Facility Generator", 2)]
    public class FacilityGenerator : PGGPlanGeneratorBase
    {
        [Tooltip("Makes walls separated from each other slightly to avoid wall overlaps if your FieldSetup is using outside edges of grid for walls")]
        [Range(0f, 0.49f)] public float WallsSeparation = 0f;

        [Tooltip("Precision with which algorithm should check available placement for rooms, it can make plan generating time much longer")]
        [Range(1, 3)] public int Precision = 1;

        [Tooltip("If position distance restrictions etc. setted up inside BuildPlan Preset should be used during generation")]
        [HideInInspector] public bool UseRestrictions = true;
        [Tooltip("Randomize or not build plan rooms generating order")]
        [HideInInspector] public bool ShufflePlanOrder = true;

        [Tooltip("Disable/Enable free allow rooms to connect with any other room")]
        [HideInInspector] public bool PrioritizeConnectionWithCorridor = true;
        //[Tooltip("Using a bit less precise calculations but much more quicker")]
        //[HideInInspector] public bool FastBounds = true;

        public override FGenGraph<FieldCell, FGenPoint> PGG_Grid { get { return null; } }
        public override FieldSetup PGG_Setup { get { return null; } }


        [Space(3)]
        // For facility generator all algorithm all placed in separated class -> FacilityPlanHelper
        private FacilityPlanHelper planHelper;

        public override void Prepare()
        {
            base.Prepare();

            if (plan == null)
            {
                UnityEngine.Debug.Log("No Building plan preset!");
                return;
            }

            planHelper = new FacilityPlanHelper(plan, this);
            planHelper.PrioritizeConnectionWithCorridor = PrioritizeConnectionWithCorridor;
            planHelper.UseRestrictions = UseRestrictions;
            planHelper.RandomIteration = ShufflePlanOrder;
            planHelper.Precision = Precision;
            PreparePlan();
        }

        void PreparePlan()
        {
            List<SimplePathGuide> pathGuides = UseGuides ? PlanGuides : null;
            if (LimitSize) planHelper.SetLimits(SizeLimitX.ToVector2Int, SizeLimitZ.ToVector2Int);
            planHelper.wallsSeparation = WallsSeparation;
            planHelper.Prepare(pathGuides);
        }


        private void OnValidate()
        {
            if (AutoRefresh) Prepare();

            if (PlanGuides != null)
                for (int i = 0; i < PlanGuides.Count; i++)
                    if (PlanGuides[i].PathThickness == 0) PlanGuides[i].SetDefaultSettings();
        }


        public override void GenerateObjects()
        {
            ClearGenerated();

            Prepare();
            Generated = planHelper.GenerateObjects(transform);

            base.GenerateObjects();
        }


        #region Gizmos

#if UNITY_EDITOR

        protected override void DrawGizmos()
        {
            if (plan == null || plan.RootChunkSetup == null || plan.RootChunkSetup.FieldSetup == null)
            {
                Handles.Label(transform.position, new GUIContent("! No Corridor FieldSetup in plan !"));
                return;
            }

            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);

            Vector3 presetCellSize = new Vector3(2, 1, 2);
            if (plan != null) if (plan.RootChunkSetup != null) presetCellSize = plan.RootChunkSetup.FieldSetup.GetCellUnitSize();
            presetCellSize.y *= 0.1f;
            float cellSizeX = presetCellSize.x;

            if ( LimitSize)
            {
                Gizmos.color = new Color(0.9f, 0.9f, 0.9f, 0.3f);
                Gizmos_DrawLimitRectangle(presetCellSize);
            }

            if (UseGuides)
            {
                Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.3f);
                for (int i = 0; i < PlanGuides.Count; i++)
                    PlanGuides[i].DrawGizmos(cellSizeX, presetCellSize, 2f);
            }

            if (planHelper == null) return;

            //if ( planHelper.limitChecker != null)
            //{
            //    planHelper.limitChecker.DrawGizmos(presetCellSize.x);
            //}

            // Draw all shapes
            for (int i = 0; i < planHelper.rooms.Count; i++)
            {
                Gizmos.color = planHelper.PlanPreset.GetIDColor(planHelper.rooms[i].SettingsReference.GetIdIndex(plan), 0.75f);
                planHelper.rooms[i].Checker.DrawGizmos(cellSizeX);
            }

            if (planHelper != null)
            {
                Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.4f);
                planHelper.rootCorridors.DrawGizmos(cellSizeX);
            }

        }

#endif


        #endregion

    }



    #region Inspector window with some enchancements


#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(FacilityGenerator))]
    public class BuildingPlanGenerator2Editor : PGGPlanGeneratorBaseEditor
    {
        public FacilityGenerator Get { get { if (_get == null) _get = (FacilityGenerator)target; return _get; } }
        private FacilityGenerator _get;
        private SerializedProperty sp_UseRestrictions;

        protected override void OnEnable()
        {
            base.OnEnable();
            _ignore.Add("WallsSeparation");
            _ignore.Add("Precision");
            sp_UseRestrictions = serializedObject.FindProperty("UseRestrictions");
        }

        protected override void UnderPresetGUI(SerializedProperty sp)
        {
            base.UnderPresetGUI(sp);

            if (Get.BuildPlanPreset)
            {
                var sp_separ = sp_BuildingPlanPreset.Copy(); sp_separ.NextVisible(false); sp_separ.NextVisible(false);
                EditorGUILayout.PropertyField(sp_separ); sp_separ.NextVisible(false);
                EditorGUILayout.PropertyField(sp_separ);
            }

            GUILayout.Space(-9);
        }

        protected override void DrawGUIBody()
        {
            SerializedProperty sp = sp_UseRestrictions.Copy();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(sp); sp.Next(false);
            GUILayout.FlexibleSpace();
            EditorGUIUtility.labelWidth = 114;
            EditorGUILayout.PropertyField(sp); sp.Next(false);
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 204;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(sp); sp.Next(false);
            //GUILayout.FlexibleSpace();
            //EditorGUIUtility.labelWidth = 77;
            //EditorGUILayout.PropertyField(sp); 
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;

            GUILayout.Space(2);
            DrawGeneratingButtons();

            base.DrawGUIBody();
        }

        protected override void DrawAdditionalSettingsContent()
        {
            base.DrawAdditionalSettingsContent();
            DrawGuides();
        }

        protected override void DrawGUIFooter()
        {
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