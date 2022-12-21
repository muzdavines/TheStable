#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{

#if UNITY_EDITOR
    /// </summary>
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(OStamperMultiSet))]
    public class OStamperMultiSetEditor : UnityEditor.Editor
    {
        public OStamperMultiSet Get { get { if (_get == null) _get = (OStamperMultiSet)target; return _get; } }
        private OStamperMultiSet _get;

        public override void OnInspectorGUI()
        {
            FGUI_Inspector.LastObjSelected = Get;
            FGUI_Inspector.DrawBackToGameObjectButton();

            UnityEditor.EditorGUILayout.HelpBox("  Prepare prefab stamper sets for multi emitter", UnityEditor.MessageType.Info);
            serializedObject.Update();

            GUILayout.Space(4f);


            #region Refresh references for settings

            if (Get.PrefabsSets == null) Get.PrefabsSets = new List<OStamperSet>();
            if (Get.PrefabSetSettings == null) Get.PrefabSetSettings = new List<OStamperMultiSet.MultiStamperSetParameters>();

            if (Get.PrefabSetSettings.Count != Get.PrefabsSets.Count)
            {
                if (Get.PrefabsSets.Count > Get.PrefabSetSettings.Count)
                    for (int i = Get.PrefabSetSettings.Count; i < Get.PrefabsSets.Count; i++) Get.PrefabSetSettings.Add(new OStamperMultiSet.MultiStamperSetParameters());

                if (Get.PrefabSetSettings.Count > Get.PrefabsSets.Count)
                    for (int i = Get.PrefabSetSettings.Count - 1; i >= Get.PrefabsSets.Count; i--) Get.PrefabSetSettings.RemoveAt(i);
            }

            for (int i = 0; i < Get.PrefabSetSettings.Count; i++)
            {
                Get.PrefabSetSettings[i].TargetSet = Get.PrefabsSets[i];
                Get.PrefabSetSettings[i].RefreshReference();
            }

            if (Get.FocusOn >= Get.PrefabsSets.Count) Get.FocusOn = -1;
            if (Get.FocusOn > 0) if (Get.PrefabsSets[Get.FocusOn] == null) Get.FocusOn = -1;

            if (Get.PerPrefabSettings == null) Get.PerPrefabSettings = new List<OStamperMultiSet.PrefabSettings>();


            #endregion


            FGenerators.DrawScriptableModificatorList<OStamperSet>(Get.PrefabsSets, FGUI_Resources.BGInBoxStyle, "Prefab Sets", ref Get._editorDrawStamps, true, false, Get, "[0]", "OS_");
            GUILayout.Space(6f);


            GUILayout.Space(6f);
            if (Get.FocusOn == -1) EditorGUILayout.HelpBox("  Click on Thumbnails for more settings", MessageType.None);
            GUILayout.Space(2f);

            int sel = Get.FocusOn;

            if (Get.FocusOn > -1)
            {
                OStamperMultiSet.MultiStamperSetParameters set = Get.PrefabSetSettings[Get.FocusOn];
                //
                if (set.TargetSet == null)
                {
                    EditorGUILayout.HelpBox("SetParameters doesn't have assigned 'StamperSet'!", MessageType.Warning);
                }
                else
                {
                    EditorGUI.BeginChangeCheck();

                    if (set.LimitMode == OStamperMultiSet.MultiStamperSetParameters.ECountLimit.LimitPerPrefab)
                    {
                        if (set.MinPrefabsSpawnCount == 0 && set.MaxPrefabsSpawnCount == 0)
                        {
                            set.MinPrefabsSpawnCount = 2;
                            set.MaxPrefabsSpawnCount = 7;
                        }

                        #region Refresh per prefab settings lists

                        // Remembering needed prefabs in list
                        List<GameObject> toInclude = new List<GameObject>();
                        for (int i = 0; i < set.TargetSet.Prefabs.Count; i++)
                        {
                            if (set.TargetSet.Prefabs[i] == null) continue;
                            if (set.TargetSet.Prefabs[i].GameObject == null) continue;
                            toInclude.Add(set.TargetSet.Prefabs[i].GameObject);
                        }

                        // Clearing prefabs already existing in list
                        for (int i = Get.PerPrefabSettings.Count - 1; i >= 0; i--)
                        {
                            if (Get.SetHashExists(Get.PerPrefabSettings[i].ParametersId) == false) Get.PerPrefabSettings.RemoveAt(i);
                            if (toInclude.Contains(Get.PerPrefabSettings[i].GameObject)) toInclude.Remove(Get.PerPrefabSettings[i].GameObject);
                        }

                        bool dd = false;
                        // Adding prefabs not yet added then
                        for (int i = 0; i < toInclude.Count; i++)
                        {
                            if (set.TargetSet == null) break;
                            OStamperMultiSet.PrefabSettings pfs = new OStamperMultiSet.PrefabSettings();
                            pfs.SetPrefab( toInclude[i]);
                            pfs.TargetSet = set.TargetSet;
                            pfs.ParametersId = set.TargetSet.GetInstanceID();
                            //pfs.Parameters = set;
                            Get.PerPrefabSettings.Add(pfs);
                            dd = true;
                        }

                        if (dd) EditorUtility.SetDirty(Get);

                        #endregion
                    }


                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

                    EditorGUILayout.BeginHorizontal();
                    PrefabReference.DrawPrefabField(Get.PrefabSetSettings[Get.FocusOn], Color.gray, "", 100, () => { sel = -1; }, null, true, Get);

                    GUILayout.Space(5);
                    EditorGUILayout.BeginVertical();

                    set.LimitMode = (OStamperMultiSet.MultiStamperSetParameters.ECountLimit)EditorGUILayout.EnumPopup(set.LimitMode);

                    EditorGUILayout.BeginHorizontal();
                    if (set.LimitMode == OStamperMultiSet.MultiStamperSetParameters.ECountLimit.LimitPerPrefab)
                        EditorGUILayout.LabelField("Limit Ranges:", GUILayout.Width(80));
                    else
                        EditorGUILayout.LabelField("Count:", GUILayout.Width(50));

                    EditorGUIUtility.labelWidth = 4;
                    set.MinPrefabsSpawnCount = EditorGUILayout.IntField(" ", set.MinPrefabsSpawnCount, GUILayout.Width(40));
                    EditorGUILayout.LabelField(" up to", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(38));
                    set.MaxPrefabsSpawnCount = EditorGUILayout.IntField(" ", set.MaxPrefabsSpawnCount, GUILayout.Width(40));
                    EditorGUIUtility.labelWidth = 0;

                    if (set.MinPrefabsSpawnCount == 0 && set.MaxPrefabsSpawnCount == 0)
                        set.MaxPrefabsSpawnCount = 1;
                    //EditorGUILayout.LabelField("(No Limit)", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(56));

                    EditorGUILayout.EndHorizontal();



                    if (set.LimitMode == OStamperMultiSet.MultiStamperSetParameters.ECountLimit.LimitPerPrefab)
                    {
                        EditorGUILayout.HelpBox("Limit how many object from this set can be spawned by total", MessageType.None);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 4;
                        set.MinSpawnCountForWholeSet = EditorGUILayout.IntField(" ", set.MinSpawnCountForWholeSet, GUILayout.Width(40));
                        EditorGUILayout.LabelField(" up to", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(38));
                        set.MaxSpawnCountForWholeSet = EditorGUILayout.IntField(" ", set.MaxSpawnCountForWholeSet, GUILayout.Width(40));
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUILayout.EndHorizontal();
                    }



                    EditorGUILayout.EndVertical();


                    EditorGUILayout.EndHorizontal();



                    if (set.LimitMode == OStamperMultiSet.MultiStamperSetParameters.ECountLimit.LimitPerPrefab)
                    {
                        int pfPreviewSize = 60;
                        float pfCurrWidth = 0f;
                        EditorGUILayout.BeginHorizontal();

                        EditorGUI.BeginChangeCheck();

                        for (int i = 0; i < Get.PerPrefabSettings.Count; i++)
                            if (Get.PerPrefabSettings[i].TargetSet == set.TargetSet)
                            //if (Get.PerPrefabSettings[i].ParametersId == set.TargetSet.GetInstanceID())
                            {
                                Get.PerPrefabSettings[i].SetRef(set);
                                PrefabReference.DrawPrefabField(Get.PerPrefabSettings[i], Color.gray, "", pfPreviewSize, null, null, true, Get);

                                pfCurrWidth += pfPreviewSize;
                                if (pfCurrWidth + pfPreviewSize > EditorGUIUtility.currentViewWidth - 48)
                                {
                                    pfCurrWidth = 0f;
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.BeginHorizontal();
                                }
                            }

                        // Cleaning nulls
                        for (int i = Get.PerPrefabSettings.Count - 1; i >= 0; i--)
                        {
                            if (Get.PerPrefabSettings == null) Get.PerPrefabSettings.RemoveAt(i);
                            else if (Get.PerPrefabSettings[i].TargetSet == null) Get.PerPrefabSettings.RemoveAt(i);
                            else if (!Get.PrefabsSets.Contains(Get.PerPrefabSettings[i].TargetSet)) Get.PerPrefabSettings.RemoveAt(i);
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorUtility.SetDirty(Get);
                        }

                        EditorGUILayout.EndHorizontal();
                    }


                    EditorGUILayout.EndVertical();

                    if (set.MinPrefabsSpawnCount < 0) set.MinPrefabsSpawnCount = 0;
                    if (set.MaxPrefabsSpawnCount < set.MinPrefabsSpawnCount) set.MaxPrefabsSpawnCount = set.MinPrefabsSpawnCount;
                    if (set.MinPrefabsSpawnCount > set.MaxPrefabsSpawnCount) set.MinPrefabsSpawnCount = set.MaxPrefabsSpawnCount;

                    if (EditorGUI.EndChangeCheck())
                        EditorUtility.SetDirty(Get);
                }
            }

            GUILayout.Space(7);
            //EditorGUILayout.BeginVertical();

            //float currWidth = 0f;
            //int previewSize = 80;
            //EditorGUILayout.BeginHorizontal();

            //Color bgc = GUI.backgroundColor;
            //for (int i = 0; i < Get.PrefabSetSettings.Count; i++)
            //{
            //    PrefabReference.DrawPrefabField(Get.PrefabSetSettings[i], i == Get.FocusOn ? Color.green : Color.gray, "", previewSize, () => { if (sel == i) sel = -1; else sel = i; }, null, true);

            //    currWidth += previewSize;
            //    if (currWidth + previewSize > EditorGUIUtility.currentViewWidth - 42)
            //    {
            //        currWidth = 0f;
            //        EditorGUILayout.EndHorizontal();
            //        EditorGUILayout.BeginHorizontal();
            //    }
            //}

            //EditorGUILayout.EndHorizontal();
            //EditorGUILayout.EndVertical();

            Get.FocusOn = sel;
            DrawPrefabPreviewButtons(Get.PrefabSetSettings, ref Get.FocusOn, false, Get);

            //if (GUILayout.Button("Debug Clean")) Get.PerPrefabSettings.Clear();

            serializedObject.ApplyModifiedProperties();
        }

        public static void DrawPrefabPreviewButtons(List<OStamperMultiSet.MultiStamperSetParameters> PrefabSetSettings, ref int focus, bool drawRemoveButton = false, UnityEngine.Object toDirty = null)
        {
            int sel = focus;

            EditorGUILayout.BeginVertical();
            float currWidth = 0f;
            int previewSize = 80;
            EditorGUILayout.BeginHorizontal();
            int toRemove = -1;

            Color bgc = GUI.backgroundColor;
            for (int i = 0; i < PrefabSetSettings.Count; i++)
            {
                if (drawRemoveButton)
                    PrefabReference.DrawPrefabField(PrefabSetSettings[i], i == focus ? Color.green : Color.gray, "", previewSize, () => { if (sel == i) sel = -1; else sel = i; }, () => { toRemove = i; }, true, toDirty);
                else
                    PrefabReference.DrawPrefabField(PrefabSetSettings[i], i == focus ? Color.green : Color.gray, "", previewSize, () => { if (sel == i) sel = -1; else sel = i; }, null, true, toDirty);

                currWidth += previewSize;
                if (currWidth + previewSize > EditorGUIUtility.currentViewWidth - 42)
                {
                    currWidth = 0f;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
            }

            if (toRemove >= 0 && toRemove < PrefabSetSettings.Count)
            {
                PrefabSetSettings.RemoveAt(toRemove);
            }

            focus = sel;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

    }
#endif

}