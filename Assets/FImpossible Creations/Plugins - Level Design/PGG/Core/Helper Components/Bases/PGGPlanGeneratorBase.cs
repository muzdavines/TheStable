using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using FIMSpace.Generating.Planning;
using FIMSpace.Generating.PathFind;
using FIMSpace.Generating.Rules;
using FIMSpace.Generating.Checker;
using System;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

namespace FIMSpace.Generating
{
    public abstract class PGGPlanGeneratorBase : PGGGeneratorBase
    {
        [HideInInspector] public BuildPlanPreset BuildPlanPreset;
        /// <summary> Returns 'BuildPlanPreset' just with different variable name </summary>
        protected BuildPlanPreset plan { get { return BuildPlanPreset; } }

        [HideInInspector] public bool LimitSize = false;
        [HideInInspector] public MinMax SizeLimitX = new MinMax(-10, 10);
        [HideInInspector] public MinMax SizeLimitZ = new MinMax(-10, 10);

        [HideInInspector] public bool UseGuides = false;
        public List<SimplePathGuide> PlanGuides = new List<SimplePathGuide>();


        #region Helper Gizmos


        protected override void DrawGizmos()
        {
            base.DrawGizmos();

            Vector3 presetCellSize = new Vector3(2, 1, 2);
            if (plan != null) if (plan.RootChunkSetup != null) presetCellSize = plan.RootChunkSetup.FieldSetup.GetCellUnitSize();
            presetCellSize.y *= 0.1f;
            float cellSizeX = presetCellSize.x;

            if (LimitSize)
            {
                Gizmos.color = new Color(0.9f, 0.9f, 0.9f, 0.6f);
                Gizmos_DrawLimitRectangle(presetCellSize);
            }

            if (plan != null)
                if (FGenerators.CheckIfExist_NOTNULL(plan.RootChunkSetup))
                    Gizmos_DrawRectangleFillShape(presetCellSize);
        }


        protected void Gizmos_DrawLimitRectangle(Vector3 cellSize)
        {
            Vector3 off = new Vector3(cellSize.x * 1.5f, 0f, cellSize.z * 0.5f);
            Gizmos.DrawLine(new Vector3(SizeLimitX.Min, 0, SizeLimitZ.Min) * cellSize.x + off, new Vector3(SizeLimitX.Max, 0, SizeLimitZ.Min) * cellSize.x + off);
            Gizmos.DrawLine(new Vector3(SizeLimitX.Max, 0, SizeLimitZ.Min) * cellSize.x + off, new Vector3(SizeLimitX.Max, 0, SizeLimitZ.Max) * cellSize.x + off);
            Gizmos.DrawLine(new Vector3(SizeLimitX.Max, 0, SizeLimitZ.Max) * cellSize.x + off, new Vector3(SizeLimitX.Min, 0, SizeLimitZ.Max) * cellSize.x + off);
            Gizmos.DrawLine(new Vector3(SizeLimitX.Min, 0, SizeLimitZ.Max) * cellSize.x + off, new Vector3(SizeLimitX.Min, 0, SizeLimitZ.Min) * cellSize.x + off);
        }


        #endregion

    }


#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PGGPlanGeneratorBase))]
    public abstract class PGGPlanGeneratorBaseEditor : PGGGeneratorBaseEditor
    {
        public PGGPlanGeneratorBase pGet { get { if (_pget == null) _pget = (PGGPlanGeneratorBase)target; return _pget; } }
        private PGGPlanGeneratorBase _pget;

        protected SerializedProperty sp_BuildingPlanPreset;
        protected SerializedProperty sp_LimSiz;
        protected SerializedProperty sp_guides;


        protected override void OnEnable()
        {
            base.OnEnable();
            sp_BuildingPlanPreset = serializedObject.FindProperty("BuildPlanPreset");
            sp_LimSiz = serializedObject.FindProperty("LimitSize");
            sp_guides = serializedObject.FindProperty("PlanGuides");
            _ignore.Add("PlanGuides");
        }


        protected override void DrawGUIBeforeDefaultInspector()
        {
            GUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();
            if (pGet.BuildPlanPreset == null) GUI.color = new Color(1f, 1f, 0.3f, 1f);
            EditorGUILayout.PropertyField(sp_BuildingPlanPreset);

            if (GUILayout.Button(new GUIContent("New", "Generating BuildingPlan preset file in choosed project directory"), GUILayout.Width(40)))
            {
                var temp = (BuildPlanPreset)FGenerators.GenerateScriptable(CreateInstance<BuildPlanPreset>(), "BP_");
                AssetDatabase.SaveAssets();
                if (temp != null) if (AssetDatabase.Contains(temp)) pGet.BuildPlanPreset = temp;
            }
            if (pGet.BuildPlanPreset == null) GUI.color = preGuiCol;

            EditorGUILayout.EndHorizontal();

            if (pGet.BuildPlanPreset)
            {
                UnderPresetGUI(sp_BuildingPlanPreset.Copy());
            }

            GUILayout.Space(6);
        }


        protected virtual void UnderPresetGUI(SerializedProperty sp)
        {
            //sp.NextVisible(false); EditorGUILayout.PropertyField(sp);
            //sp.NextVisible(false); EditorGUILayout.PropertyField(sp);
        }

        protected override void DrawGUIBody()
        {
            base.DrawGUIBody();
            DrawAdditionalTab();
        }

        protected override void DrawAdditionalSettingsContent()
        {
            DrawBasePostGenerationFetaures();
        }

        protected void DrawGuides()
        {
            EditorGUI.indentLevel++;
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(sp_guides, new GUIContent("Generator Path Guides"));
            GUILayout.Space(4);
            EditorGUI.indentLevel--;
        }

    }

#endif

}