#if UNITY_EDITOR
using UnityEditor;
using FIMSpace.FEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{

#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(OStamperSet))]
    public class OStamperSetEditor : UnityEditor.Editor
    {
        public OStamperSet Get { get { if (_get == null) _get = (OStamperSet)target; return _get; } }
        private OStamperSet _get;
        private int _s = -1;

        SerializedProperty sp_restr;

        private void OnEnable()
        {
            sp_restr = serializedObject.FindProperty("StampRestriction");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            FGUI_Inspector.DrawBackToObjectButton();
            FGUI_Inspector.DrawBackToGameObjectButton();
            EditorGUILayout.EndHorizontal();

            UnityEditor.EditorGUILayout.HelpBox("  Prepare prefabs to spawn with randomization", UnityEditor.MessageType.Info);

            serializedObject.Update();

            if (Get.AllowJustOnTags == null) Get.AllowJustOnTags = new List<string>();
            if (Get.DisallowOnTags == null) Get.DisallowOnTags = new List<string>();
            if (Get.IgnoreCheckOnTags == null) Get.IgnoreCheckOnTags = new List<string>();




            if (Get.Prefabs == null) Get.Prefabs = new List<OSPrefabReference>();

            GUILayout.Space(7);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.BeginChangeCheck();
            PrefabReference.DrawPrefabsList(Get.Prefabs, ref Get._editor_drawPrefabs, ref _s, ref Get._editor_drawThumbs, Color.gray, Color.green, EditorGUIUtility.currentViewWidth - 48, 72, true, Get);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
                Get.RefreshBounds();
                EditorUtility.SetDirty(Get);
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(7);




            //EditorGUILayout.PropertyField(serializedObject.FindProperty("StampersetTag"));

            GUILayout.Space(3);
            FGUI_Inspector.FoldHeaderStart(ref Get._editor_drawSettings, "Spawning Settings", FGUI_Resources.BGInBoxStyle);
            if (Get._editor_drawSettings)
            {
                GUILayout.Space(5f);
                DrawPropertiesExcluding(serializedObject, new string[] { "m_Script", "Prefabs" });

                GUILayout.Space(2f);
                if (GUILayout.Button("Refresh Bounds")) Get.RefreshBounds();
            }

            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            EditorGUILayout.LabelField("Restrictions for being spawned", FGUI_Resources.HeaderStyle);
            GUILayout.Space(6);

            EditorGUIUtility.labelWidth = 140;
            SerializedProperty spr = sp_restr.Copy();
            EditorGUILayout.PropertyField(spr); spr.Next(false);
            EditorGUILayout.PropertyField(spr);

            if (Get.StampRestriction != OStamperSet.EOSRaystriction.None && Get.StampRestriction != OStamperSet.EOSRaystriction.AvoidAnyOtherStamper)
            {
                spr.Next(false); EditorGUILayout.PropertyField(spr, true);
                EditorGUILayout.BeginHorizontal();
                spr.Next(false); EditorGUILayout.PropertyField(spr, true);
                if (Get.PlacementLimitCount == 0) EditorGUILayout.LabelField("(No Limit)", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(60));
                if (Get.PlacementLimitCount < 0) Get.PlacementLimitCount = 0;
                EditorGUILayout.EndHorizontal();

                //GUILayout.Space(5);
                spr.Next(false); EditorGUILayout.PropertyField(spr, true);
                spr.Next(false); EditorGUILayout.PropertyField(spr, true);
                //GUILayout.Space(5);
                //spr.Next(false); EditorGUILayout.PropertyField(spr, true);
                //spr.Next(false); EditorGUILayout.PropertyField(spr, true);
                //spr.Next(false); EditorGUILayout.PropertyField(spr, true);
            }
            else //if ( Get.PlacementRestriction == OStamperSet.EOSRaystriction.AvoidAnyOtherStamper)
            {
                spr.Next(false);
                spr.Next(false);

                //GUILayout.Space(5);
                spr.Next(false); EditorGUILayout.PropertyField(spr, true);
                spr.Next(false); EditorGUILayout.PropertyField(spr, true);
                //GUILayout.Space(5);
                //spr.Next(false); EditorGUILayout.PropertyField(spr, true);
                //spr.Next(false); EditorGUILayout.PropertyField(spr, true);
                //spr.Next(false); EditorGUILayout.PropertyField(spr, true);
            }

            EditorGUIUtility.labelWidth = 0;


            EditorGUILayout.EndVertical();


            if (Get.ReferenceBounds.size == Vector3.zero && Get.ReferenceBounds.center == Vector3.zero) Get.RefreshBounds();

            if (Get.AngleStepForAxis.x < 1f) Get.AngleStepForAxis.x = 1f;
            if (Get.AngleStepForAxis.y < 1f) Get.AngleStepForAxis.y = 1f;
            if (Get.AngleStepForAxis.z < 1f) Get.AngleStepForAxis.z = 1f;
            if (Get.AngleStepForAxis.x > 180f) Get.AngleStepForAxis.x = 180f;
            if (Get.AngleStepForAxis.y > 180f) Get.AngleStepForAxis.y = 180f;
            if (Get.AngleStepForAxis.z > 180f) Get.AngleStepForAxis.z = 180f;

            if (Get.RotationRanges.x < -180) Get.RotationRanges.x = -180f;
            if (Get.RotationRanges.x > 180) Get.RotationRanges.x = 180f;
            if (Get.RotationRanges.y < -180) Get.RotationRanges.y = -180f;
            if (Get.RotationRanges.y > 180) Get.RotationRanges.y = 180f;

            serializedObject.ApplyModifiedProperties();

        }
    }
#endif

}