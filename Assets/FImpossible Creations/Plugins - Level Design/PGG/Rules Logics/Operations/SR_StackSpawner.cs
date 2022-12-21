using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

using UnityEngine;

namespace FIMSpace.Generating.Rules.Operations
{
    public class SR_StackSpawner : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Stack Spawner"; }
        public override string Tooltip() { return "Spawning multiple instances of choosed prefabs stacked one on another using object stamper algorithms\n" + base.Tooltip(); }
        public override bool CanBeGlobal() { return false; }
        public override bool CanBeNegated() { return false; }

        public EProcedureType Type { get { return EProcedureType.Coded; } }

        public Vector3 DropCastOrigin = Vector3.up;
        public Vector2 DropArea = new Vector2(0.5f, 0.5f);
        public float RaycastDistance = 10f;
        public LayerMask CollisionsLayer = 1 << 0;
        [Space(6)]
        [Range(0f, 1.15f)]
        public float OverlapRestriction = 0.9f;
        [Range(0f, 1.15f)]
        public float MinimumStandSpace = 0.8f;

        [Space(6)]
        public MinMax TargetSpawnCount = new MinMax(3, 5);

        [Space(6)]
        public bool Debug = false;

        [HideInInspector][Range(0f, 1f)] public float RandomScale = 0f;
        [HideInInspector] public Vector3 RandomScaleAxis = Vector3.one;
        //[HideInInspector] public int LimitStackingOnTop = -1;
        [HideInInspector] public List<GameObject> CustomPrefabsToSpawn = new List<GameObject>();

        [HideInInspector] public OStampPhysicalPlacementSetup PhysicalPlacement;

        #region Editor

#if UNITY_EDITOR

        private SerializedProperty _sp = null;
        private SerializedProperty _spPh = null;
        private SerializedProperty _spPhEn = null;
        private bool displayAdditional = false;
        public override void NodeBody(SerializedObject so)
        {
            EditorGUILayout.HelpBox("To see result in preview, remember to enable 'Run Additional Generators' in 'Test Generating Settings'!", MessageType.None);
            base.NodeBody(so);
        }

        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            if (_sp == null) _sp = so.FindProperty("RandomScale");
            GUILayout.Space(3);
            FGUI_Inspector.FoldHeaderStart(ref displayAdditional, "Additional Settings", FGUI_Resources.BGInBoxStyle);

            if (displayAdditional)
            {

                GUILayout.Space(8);
                EditorGUI.BeginChangeCheck();

                if (_spPh == null)
                {
                    _spPh = so.FindProperty("PhysicalPlacement");
                    _spPhEn = _spPh.FindPropertyRelative("Enabled");
                }

                PhysicalPlacement._Editor_DrawSetupToggle(_spPhEn);

                if (PhysicalPlacement._Editor_Foldout)
                {
                    PhysicalPlacement._Editor_DrawSetup(_spPh, false);
                    GUILayout.Space(4);
                }

                if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(this);
                GUILayout.Space(6);


                EditorGUILayout.LabelField("Randomization", EditorStyles.centeredGreyMiniLabel);
                GUILayout.Space(3);
                SerializedProperty sp = _sp.Copy();
                EditorGUILayout.PropertyField(sp); sp.Next(false);
                EditorGUILayout.PropertyField(sp); sp.Next(false);
                //EditorGUILayout.PropertyField(sp); sp.Next(false);
                EditorGUILayout.PropertyField(sp, true);
                GUILayout.Space(3);
            }
            GUILayout.EndVertical();

            base.NodeFooter(so, mod);
        }
#endif

        #endregion

        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);
            CellAllow = true;
        }

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            _EditorDebug = Debug;
        }

        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            SpawnData spwn = thisSpawn;

            Action<GameObject> stackSpawn =
            (o) =>
            {
                GameObject spawner = new GameObject("Spawner");
                Matrix4x4 mx = GetMatrix(spwn);

                spawner.transform.position = mx.MultiplyPoint(Vector3.zero);
                spawner.transform.rotation = mx.rotation;

                var me = spawner.AddComponent<ObjectStampMultiEmitter>();
                me.MultiSet = CreateInstance<OStamperMultiSet>();
                me.MultiSet.name = "0";
                me.MultiSet.PrefabsSets = new List<OStamperSet>();

                me.PhysicalPlacement = new OStampPhysicalPlacementSetup();
                me.PhysicalPlacement.CopySettingsFromTo(PhysicalPlacement, me.PhysicalPlacement);

                if (OwnerSpawner.Mode == FieldModification.EModificationMode.ObjectsStamp)
                {
                    if (mod.OStamp) me.MultiSet.PrefabsSets.Add(mod.OStamp);
                }
                else if (OwnerSpawner.Mode == FieldModification.EModificationMode.ObjectMultiEmitter)
                {
                    if (mod.OMultiStamp) me.MultiSet = mod.OMultiStamp;
                }
                else
                {
                    OStamperSet spawns;
                    spawns = CreateInstance<OStamperSet>();
                    spawns.Prefabs = new List<OSPrefabReference>();
                    spawns.RayCheckLayer = CollisionsLayer;
                    spawns.OverlapCheckMask = CollisionsLayer;
                    spawns.RayDistanceMul = RaycastDistance;
                    spawns.OverlapCheckScale = OverlapRestriction;
                    spawns.MinimumStandSpace = MinimumStandSpace;

                    spawns.RandScaleAxis = RandomScaleAxis;
                    spawns.RandomizeScale = RandomScale;

                    //if (LimitStackingOnTop > -1)
                    //{
                    //    spawns.StampRestriction = OStamperSet.EOSRaystriction.AllowStackOnSelected;
                    //    spawns.RestrictionSets = new List<OStamperSet>();
                    //    spawns.RestrictionSets.Add(spawns);
                    //    spawns.PlacementLimitCount = LimitStackingOnTop;
                    //}

                    if (CustomPrefabsToSpawn.Count > 0)
                    {
                        for (int i = 0; i < CustomPrefabsToSpawn.Count; i++)
                        {
                            GameObject ob = CustomPrefabsToSpawn[i];
                            var pRefs = new OSPrefabReference();
                            pRefs.SetPrefab(ob);
                            spawns.Prefabs.Add(pRefs);
                            pRefs.OnPrefabChanges();
                        }
                    }
                    else
                    {

                        if (OwnerSpawner.MultipleToSpawn == false)
                        {
                            if (OwnerSpawner.StampPrefabID < 0) // Random
                            {
                                for (int i = 0; i < mod.PrefabsList.Count; i++)
                                {
                                    var pRefs = new OSPrefabReference();
                                    pRefs.SetPrefab(mod.PrefabsList[i].CoreGameObject);
                                    spawns.Prefabs.Add(pRefs);
                                    pRefs.OnPrefabChanges();
                                }
                            }
                            else
                            {
                                var pRefs = new OSPrefabReference();
                                pRefs.SetPrefab(spwn.Prefab);
                                spawns.Prefabs.Add(pRefs);
                                pRefs.OnPrefabChanges();
                            }
                        }
                        else // Multiple to spawn
                        {
                            var selected = FEngineering.GetLayermaskValues(OwnerSpawner.StampPrefabID, mod.GetPRSpawnOptionsCount());
                            for (int i = 0; i < selected.Length; i++)
                            {
                                var pRefs = new OSPrefabReference();
                                pRefs.SetPrefab(mod.PrefabsList[selected[i]].CoreGameObject);
                                spawns.Prefabs.Add(pRefs);
                                pRefs.OnPrefabChanges();
                            }
                        }

                    }

                    spawns.name = "0";

                    me.MultiSet.PrefabSetSettings = new List<OStamperMultiSet.MultiStamperSetParameters>();
                    OStamperMultiSet.MultiStamperSetParameters mPar = new OStamperMultiSet.MultiStamperSetParameters();
                    mPar.SetPrefab(spwn.Prefab);
                    mPar.TargetSet = spawns;
                    me.MultiSet.PrefabSetSettings.Add(mPar);

                    mPar.MinPrefabsSpawnCount = TargetSpawnCount.Min;
                    mPar.MaxPrefabsSpawnCount = TargetSpawnCount.Max;
                    mPar.MaxSpawnCountForWholeSet = TargetSpawnCount.Max;

                    me.MultiSet.PrefabsSets.Add(spawns);
                }

                me.Areas = new List<ObjectStampMultiEmitter.SpawnArea>();
                var sArea = new ObjectStampMultiEmitter.SpawnArea("0");
                sArea.Size = DropArea;
                sArea.Center = Vector3.zero;
                sArea.Sets = new List<int>();
                sArea.Sets.Add(0);
                sArea.Multiply = new List<float>();
                sArea.Multiply.Add(1f);
                me.Areas.Add(sArea);

                me.MultiSet.PrefabSetSettings[0].RefreshReference();
                me.MultiSet.PrefabSetSettings[0].OnPrefabChanges();
                me.MultiSet.PrefabsSets[0].RefreshBounds();

                spwn.AdditionalGenerated = new List<GameObject>();
                spwn.AdditionalGenerated.Add(spawner);
                spwn.DontSpawnMainPrefab = true;
            };

            thisSpawn.OnGeneratedEvents.Add(stackSpawn);

        }

        Matrix4x4 GetMatrix(SpawnData spawn)
        {
            Quaternion spawnRot = spawn.GetRotationOffset();
            Vector3 pos = spawn.GetWorldPositionWithFullOffset() + spawnRot * DropCastOrigin;
            return Matrix4x4.TRS(pos, spawnRot, Vector3.one);
        }

#if UNITY_EDITOR
        public override void OnDrawDebugGizmos(FieldSetup preset, SpawnData spawn, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            base.OnDrawDebugGizmos(preset, spawn, cell, grid);

            Gizmos.color = new Color(0.8f, 1f, 0.8f, 0.3f);
            Gizmos.matrix = GetMatrix(spawn);

            Gizmos.DrawCube(Vector3.zero, new Vector3(DropArea.x, 0.02f, DropArea.y));

            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = _DbPreCol;
        }
#endif

    }
}