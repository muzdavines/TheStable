using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class OStamperMultiSet 
    {

        public List<PrefabSettings> PerPrefabSettings;


        [System.Serializable]
        public class PrefabSettings : PrefabReference
        {
            [HideInInspector] public OStamperSet TargetSet;
            public int ParametersId;

            /// <summary> Value from zero to one (0f-1f) </summary>
            public float Min = 0f;
            /// <summary> Value from zero to one (0f-1f) </summary>
            public float Max = 1f;

            /// <summary> Value from zero to any number </summary>
            public int RefMin = 1;
            /// <summary> Value from zero to any number </summary>
            public int RefMax = 7;

            protected override void DrawGUIWithoutPrefab(int previewSize = 72, string predicate = "", Action removeCallback = null, bool drawPrefabField = true)
            {
#if UNITY_EDITOR
                EditorGUILayout.HelpBox("Assign Prefab!", MessageType.Error);
#endif
            }

            protected override void DrawGUIWithPrefab(Color color, int previewSize = 72, string predicate = "", Action clickCallback = null, Action removeCallback = null, bool drawThumbnail = true, bool drawPrefabField = true)
            {
#if UNITY_EDITOR
                GameObject Prefab = GameObject;
                if (Prefab == null)
                {
                    EditorGUILayout.HelpBox("Assign Prefab!", MessageType.Error);
                    return;
                }

                Color bc = GUI.backgroundColor;
                GUI.backgroundColor = color;
                if (GUILayout.Button(new GUIContent(Preview), opt)) if (clickCallback != null) clickCallback.Invoke();
                GUI.backgroundColor = bc;

                EditorGUILayout.LabelField(predicate + Prefab.name, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(previewSize));
                EditorGUIUtility.fieldWidth = 16;

                GUILayoutOption[] opts = new GUILayoutOption[] { GUILayout.Width(previewSize - 28), GUILayout.Height(14) };
                GUILayoutOption[] opts2 = new GUILayoutOption[] { GUILayout.Width(16), GUILayout.Height(14) };

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(((int)(Min * RefMax)).ToString(), EditorStyles.centeredGreyMiniLabel, opts2);
                Min = GUILayout.HorizontalSlider((int)(Min * RefMax), RefMin, RefMax, opts) / (float)RefMax;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(((int)(Max * RefMax)).ToString(), EditorStyles.centeredGreyMiniLabel, opts2);
                Max = GUILayout.HorizontalSlider((int)(Max * RefMax), RefMin, RefMax, opts) / (float)RefMax;
                EditorGUILayout.EndHorizontal();

                //if ((int)(Max * RefMax) > RefMax) Max = 1f / (float)RefMax;
                if (Max > 1f) Max = 1f;
                if (Min < 0f) Min = 0f;
                if (Max < Min) Max = Min;
                if (Min > Max) Min = Max;
                //if ((int)(Min * RefMax) < RefMin) if (RefMin > 0) Min = 1f / (float)RefMin;

                //Min = (float)EditorGUILayout.IntSlider("", (int)(Min * RefMax), RefMin, RefMax, GUILayout.Width(previewSize)) / (float)RefMax;
                //Max = (float)EditorGUILayout.IntSlider("", (int)(Max * RefMax), RefMin, RefMax, GUILayout.Width(previewSize)) / (float)RefMax;
                EditorGUIUtility.fieldWidth = 0;
#endif
            }


            public void SetRef(MultiStamperSetParameters set)
            {
                RefMin = set.MinPrefabsSpawnCount;
                RefMax = set.MaxPrefabsSpawnCount;
            }
        }


    }

}