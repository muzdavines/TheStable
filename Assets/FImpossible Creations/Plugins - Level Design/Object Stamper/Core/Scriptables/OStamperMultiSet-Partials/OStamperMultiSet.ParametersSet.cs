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
        public List<MultiStamperSetParameters> PrefabSetSettings;

        [System.Serializable]
        public class MultiStamperSetParameters : PrefabReference
        {
            [HideInInspector] public OStamperSet TargetSet;

            public enum ECountLimit { OneLimitForThisSet, LimitPerPrefab }
            public ECountLimit LimitMode = ECountLimit.OneLimitForThisSet;

            public int MinPrefabsSpawnCount = 2;
            public int MaxPrefabsSpawnCount = 7;

            public int MinSpawnCountForWholeSet = 5;
            public int MaxSpawnCountForWholeSet = 8;

            public bool _EditorFoldout = false;

            public void RefreshReference()
            {
                if (TargetSet != null && TargetSet.Prefabs != null)
                    if (TargetSet.Prefabs.Count > 0)
                        if (TargetSet.Prefabs[0] != null)
                            SetPrefab ( TargetSet.Prefabs[0].CoreGameObject);
            }

            public int GetRandomLimitCount()
            {
                if (LimitMode == ECountLimit.OneLimitForThisSet)
                    return FGenerators.GetRandom(MinPrefabsSpawnCount, MaxPrefabsSpawnCount+1);
                else
                    return FGenerators.GetRandom(MinSpawnCountForWholeSet, MaxSpawnCountForWholeSet+1);
            }

            public int GetRandomLimitCount(int prefabId, OStamperMultiSet set)
            {
                if (TargetSet == null) return 0;
                if (TargetSet.Prefabs == null) return 0;
                if (prefabId < 0) return 0;
                if (prefabId >= TargetSet.Prefabs.Count) return 0;

                //PrefabSettings pr;
                GameObject prefab = TargetSet.Prefabs[prefabId].GameObject;

                if (prefab == null) return 0;

                for (int i = 0; i < set.PerPrefabSettings.Count; i++)
                {
                    if (set.PerPrefabSettings[i].GameObject == prefab)
                    {
                        return (int)FGenerators.GetRandom
                            (
                            (int)(set.PerPrefabSettings[i].Min * set.PerPrefabSettings[i].RefMax),
                            (int)(set.PerPrefabSettings[i].Max * set.PerPrefabSettings[i].RefMax) + 1
                            );
                    }
                }

                return 0;
            }


#if UNITY_EDITOR

            protected override void DrawGUIWithoutPrefab(int previewSize = 72, string predicate = "", Action removeCallback = null, bool drawPrefabField = true)
            {
                if (TargetSet != null)
                    EditorGUILayout.HelpBox("Assign prefabs to '" + TargetSet.name + "' Stamper Set!", MessageType.Warning);
                else
                    EditorGUILayout.HelpBox("Assign Stamper Set!", MessageType.Error);
            }

            protected override void DrawGUIWithPrefab(Color color, int previewSize = 72, string predicate = "", Action clickCallback = null, Action removeCallback = null, bool drawThumbnail = true, bool drawPrefabField = true)
            {
                if (TargetSet == null)
                {
                    EditorGUILayout.HelpBox("Assign Stamper Set!", MessageType.Error);
                    return;
                }

                Color bc = GUI.backgroundColor;
                GUI.backgroundColor = color;
                if (GUILayout.Button(new GUIContent(Preview), opt)) if (clickCallback != null) clickCallback.Invoke();
                GUI.backgroundColor = bc;

                EditorGUILayout.LabelField(predicate + TargetSet.name + " (" + TargetSet.Prefabs.Count + ")", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(previewSize));
            }



#endif
        }

    }
}