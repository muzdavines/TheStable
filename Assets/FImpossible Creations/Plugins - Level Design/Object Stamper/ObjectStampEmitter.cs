#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating
{
    /// <summary>
    /// Implementation of spawning prefabs with selected OStamperSet with gizmos preview
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Level Design/Object Stamp Emitter", 1)]
    public class ObjectStampEmitter : ObjectStampEmitterBase, IGenerating
    {
        public bool AlwaysDrawPreview = false;
        public bool ReplaceAlreadySpawned = true;
        [Range(0f, 1f)] public float SpawnPropability = 1f;
        public OStamperSet PrefabsSet;

        public OStampPhysicalPlacementSetup PhysicalPlacement;

        // Spawning related
        public ObjectStamperEmittedInfo spawningInfo;

        public GameObject SpawnedObject;

        // Editor related
        public GameObject _editorPreview;

        public bool _displaySimplifiedSet = true;

        private void Start()
        {
            if (PrefabsSet == null)
            {
                Debug.Log("[Objects Stamper] No 'Objects Stamper Set' assigned to '" + name + "'!");
                return;
            }

            if (SpawnOnStart)
            {
                if (RandomizeOnStart)
                {
                    spawningInfo = PrefabsSet.Emit(false, transform.parent); // Random emission from set
                }

                SpawnedObject = SpawnEmitPrefab();
                IG_CallAfterGenerated();
            }
            else
                if (_editorPreview)
                SpawnedObject = _editorPreview;
        }


        public void Generate()
        {
            if (RandomizeOnStart || spawningInfo.ChoosedPrefab == null) spawningInfo = PrefabsSet.Emit(false, transform.parent); // Random emission from set
            SpawnEmitPrefab();
        }

        public void PreviewGenerate() { }

        protected override OStamperSet GetStamperSet()
        {
            return PrefabsSet;
        }

        protected override ObjectStamperEmittedInfo GetSpawnInfo()
        {
            if (spawningInfo.ChoosedPrefab == null) spawningInfo = PrefabsSet.Emit(false, transform.parent);
            return spawningInfo;
        }

        public void _EditorEmitPreview()
        {
            ClearPreviews();
            _editorPreview = SpawnEmitPrefab(PrefabsSet);
            IG_CallAfterGenerated();
        }

        public void _EditorEmitAndDetach()
        {
            ClearPreviews();
            GameObject emitted = SpawnEmitPrefab(PrefabsSet);

            if (emitted != null)
            {
                GameObject pre = SpawnedObject;
                SpawnedObject = emitted;
                IG_CallAfterGenerated();
                SpawnedObject = pre;
                emitted.transform.SetParent(transform.parent, true);
                emitted.transform.SetAsLastSibling();
            }
        }

        public void ClearPreviews()
        {
            if (_editorPreview) FGenerators.DestroyObject(_editorPreview);
            if (SpawnedObject) FGenerators.DestroyObject(SpawnedObject);
        }

        protected override GameObject InternalInstatiatePrefab(bool raycasted, bool setParent = true)
        {
            if (SpawnPropability < 1f)
                if (SpawnPropability < FGenerators.GetRandom(0f, 1f)) return null;

            if (ReplaceAlreadySpawned)
            {
                if (_editorPreview != null) FGenerators.DestroyObject(_editorPreview);

                if (SpawnedObject != null) FGenerators.DestroyObject(SpawnedObject);
            }
            else
                if (_editorPreview != null) _editorPreview.transform.SetParent(null, true);

            return base.InternalInstatiatePrefab(raycasted, setParent);
        }


        public void IG_CallAfterGenerated()
        {
            if (PhysicalPlacement.Enabled == false) return;

            if (SpawnedObject) PhysicalPlacement.ProceedOn(SpawnedObject);
            else if (_editorPreview) PhysicalPlacement.ProceedOn(_editorPreview);
        }


        #region Gizmos (Most of the code for the component is here)

#if UNITY_EDITOR

        /// <summary>
        /// Drawing Green or Yellow mesh when object is not selected (only when AlwaysDrawPreview == true)
        /// </summary>
        private void OnDrawGizmos()
        {
            if (Application.isPlaying) return;
            if (PrefabsSet == null) return;
            if (PrefabsSet.Prefabs == null) return;
            if (PrefabsSet.Prefabs.Count == 0) return;
            if (Selection.activeGameObject == gameObject) return;
            if (AlwaysDrawPreview == false) return;

            Color c = Gizmos.color;
            DrawGizmosPreview(false);

            if (RaycastSpawn)
            {
                OStamperSet.PlacementVolumeRaycastingData volume = PrefabsSet.GetRaycastingVolumeFor(GetSpawnInfo(), transform);
                spawningResult = PrefabsSet.CheckRestrictionsOn(volume);

                if (spawningResult.allow == false)
                {
                    var overlapCheckResult = PrefabsSet.CheckOverlapOnFullLineCast(GetSpawnInfo(), volume);
                    if (overlapCheckResult.allow) spawningResult = overlapCheckResult;
                }

                if (spawningResult.allow || UseRestrictions == false)
                {
                    DrawGizmosSpawningPreview(false);
                }
            }

            Gizmos.color = c;
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying) return;
            if (PrefabsSet == null) return;
            if (PrefabsSet.Prefabs == null) return;
            if (PrefabsSet.Prefabs.Count == 0) return;

            Color bc = Gizmos.color;
            Color bh = Handles.color;

            // Draw navigation -----------------------------------------
            Gizmos.color = new Color(1f, 0.4f, 0.4f, 0.75f);
            Handles.color = new Color(1f, 0.4f, 0.4f, 0.5f);

            // Draw Preview basing on choosed spawn info
            if (spawningInfo.ChoosedPrefab == null) spawningInfo = PrefabsSet.Emit(false, transform.parent);
            else if (spawningInfo.SetReference != PrefabsSet) spawningInfo = PrefabsSet.Emit(false, transform.parent);

            Bounds refBounds = PrefabsSet.ReferenceBounds;
            if (spawningInfo.ChoosedPrefab != null) refBounds = spawningInfo.PrefabReference.ReferenceBounds;

            Gizmos.matrix = transform.localToWorldMatrix;
            Handles.matrix = transform.localToWorldMatrix;

            // Draw yellow preview mesh
            DrawGizmosPreview(true);

            Gizmos.matrix = Matrix4x4.identity;
            Handles.matrix = Matrix4x4.identity;

            #region Drawing raycasting arrow guide

            if (RaycastSpawn)
            {
                if (PrefabsSet.RayDistanceMul > 0f)
                {
                    Vector3 startCast = GetRayOrigin(false);
                    Vector3 castRay = GetCastVector(false);

                    //castRay = castRay.normalized * spawningInfo.SetReference.ReferenceBounds.size.magnitude * spawningInfo.SetReference.RayDistanceMul;

                    //Vector3 rotatedBoundAxis = GetSpawnInfo().PrefabReference.GetRotatedBoundsDimension(transform.rotation, castRay.normalized);

                    Vector3 castSide = OStamperSet.GetShiftedAxis(castRay).normalized;
                    Vector3 castEnd = startCast + castRay;


                    float refScale = GetStamperSet().ReferenceBounds.extents.magnitude * 0.15f;

                    // Drawing raycasting arrow guide
                    Gizmos.DrawLine(startCast, castEnd);
                    Gizmos.DrawRay(castEnd, -castRay.normalized * refScale + castSide * refScale);
                    Gizmos.DrawRay(castEnd, -castRay.normalized * refScale - castSide * refScale);
                }
                else
                {
                    PrefabsSet.RayDistanceMul = 0f;
                }
            }

            #endregion Drawing raycasting arrow guide

            // Drawing green mesh preview on raycasted surface
            if (RaycastSpawn)
                if (Selection.activeGameObject == gameObject)
                {
                    OStamperSet.PlacementVolumeRaycastingData volume = PrefabsSet.GetRaycastingVolumeFor(GetSpawnInfo(), transform);
                    spawningResult = PrefabsSet.CheckRestrictionsOn(volume);

                    if (spawningResult.allow == false)
                    {
                        var overlapCheckResult = PrefabsSet.CheckOverlapOnFullLineCast(GetSpawnInfo(), volume);
                        if (overlapCheckResult.allow) spawningResult = overlapCheckResult;
                    }

                    if (spawningResult.allow || UseRestrictions == false)
                    {
                        DrawGizmosSpawningPreview(true);
                    }
                    else
                    {
                        Handles.Label(spawningResult.originHit.point == Vector3.zero ? (transform.position + GetSpawnInfo().PrefabReference.ReferenceBoundsFull.extents) : spawningResult.originHit.point, new GUIContent("  Enter for info", FGUI_Resources.Tex_Error, spawningResult.info));
                    }
                }

            Handles.color = bh;
            Gizmos.color = bc;
        }

        private void DrawGizmosPreview(bool drawBounds)
        {
            if (Application.isPlaying) return;

            Matrix4x4 preMatrix = Gizmos.matrix;

            Color cg = Gizmos.color;
            Bounds refBounds = PrefabsSet.ReferenceBounds;
            if (spawningInfo.ChoosedPrefab != null) refBounds = spawningInfo.PrefabReference.ReferenceBounds;

            RaycastHit h = spawningResult.originHit;

            bool draw = true;
            if (h.transform != null) if (Vector3.Distance(spawningResult.targetPosition, transform.position) < GetCastVector(false).magnitude * 0.1f) draw = false;

            if (draw)
                if (spawningInfo.ChoosedPrefab != null)
                {
                    Gizmos.matrix = spawningInfo.GetMatrixFor(transform);

                    OSPrefabReference prr = spawningInfo.PrefabReference;

                    // Draw preview Mesh
                    if (prr != null)
                        if (prr.GetMesh() != null)
                        {
                            Gizmos.color = new Color(.75f, .75f, .2f, h.transform == null ? 0.75f : 0.125f);
                            Vector3 scale = Vector3.one; if (prr.GameObject) scale = prr.GameObject.transform.localScale;
                            Gizmos.DrawMesh(prr.GetMesh(), Gizmos.matrix.inverse.MultiplyPoint(transform.position), Quaternion.identity, scale);
                        }
                }

            if (drawBounds)
            {
                Gizmos.color = new Color(1f, 1f, 1f, 0.4f);
                Gizmos.DrawWireCube(refBounds.center, refBounds.size);
            }

            if (GetStamperSet() != null)
            {
                if (GetStamperSet().MinimumStandSpace > 0f)
                {
                    Gizmos.color = new Color(0f, 0f, 0f, 0.4f);
                    Gizmos.DrawCube(refBounds.center, refBounds.size * GetStamperSet().MinimumStandSpace);
                }
            }

            Gizmos.color = cg;
            Gizmos.matrix = preMatrix;
        }

        private void DrawGizmosSpawningPreview(bool drawHitCross)
        {
            OStamperSet.PlacementVolumeRaycastingData volume = PrefabsSet.GetRaycastingVolumeFor(GetSpawnInfo(), transform);
            spawningResult = PrefabsSet.CheckRestrictionsOn(volume);

            if (spawningResult.allow == false)
            {
                var overlapCheckResult = PrefabsSet.CheckOverlapOnFullLineCast(GetSpawnInfo(), volume);
                if (overlapCheckResult.allow) spawningResult = overlapCheckResult;
            }

            if (spawningResult.allow || UseRestrictions == false)
            {
                RaycastHit h = spawningResult.originHit;
                if (volume.backupFullLineCast.transform) h = volume.backupFullLineCast;
                Vector3 spawnPos = spawningResult.targetPosition;

                if (h.transform)
                {
                    if (drawHitCross)
                    {
                        // Yellow Normal Line
                        Gizmos.color = new Color(1f, 1f, 0.1f, 0.85f);
                        float refScale = PrefabsSet.ReferenceBounds.extents.magnitude;
                        Gizmos.DrawRay(h.point, h.normal * refScale * 0.3f);

                        // Yellow Normal Cross
                        Quaternion hitLook = Quaternion.LookRotation(h.normal, transform.up);
                        Vector3 off = hitLook * Vector3.up * refScale * 0.3f;
                        Gizmos.DrawRay(h.point - off, off * 2f);

                        off = hitLook * Vector3.right * refScale * 0.3f;
                        Gizmos.DrawRay(h.point - off, off * 2f);
                    }

                    // Preparing green mesh draw
                    Gizmos.matrix = spawningInfo.GetMatrixFor(transform, h.point, spawningInfo.GetRotationOn(transform, h.normal));
                    OSPrefabReference spawnPrefabRef = spawningInfo.PrefabReference;

                    if (spawnPrefabRef != null)
                        if (spawnPrefabRef.GetMesh() != null)
                        {
                            Gizmos.color = new Color(0.2f, .85f, 0.2f, 0.55f);
                            Vector3 drawPos = Gizmos.matrix.inverse.MultiplyPoint(spawningInfo.GetSpawnPosition(transform, h, spawnPos));
                            Vector3 scale = Vector3.one; if (spawnPrefabRef.GameObject) scale = spawnPrefabRef.GameObject.transform.localScale;
                            Gizmos.DrawMesh(spawnPrefabRef.GetMesh(), drawPos, Quaternion.identity, scale);
                        }
                }
            }
        }
#endif

        public override void SpawnIfNotEmittedYet()
        {
            if (SpawnedObject != null) return;
            SpawnEmitPrefab(PrefabsSet);
        }


        #endregion Gizmos (Most of the code for the component is here)
    }

#if UNITY_EDITOR

    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(ObjectStampEmitter))]
    public class ObjectsStampEmitterEditor : ObjectsStampEmitterBaseEditor
    {
        public ObjectStampEmitter Get { get { if (_get == null) _get = (ObjectStampEmitter)target; return _get; } }
        private ObjectStampEmitter _get;

        private SerializedProperty sp_pf;
        private SerializedProperty sp_pfDrawPrev;
        private SerializedProperty sp_PhysicalPlacement;
        private SerializedProperty sp_PhysicalPlacementEnabled;

        protected override void OnEnable()
        {
            base.OnEnable();
            sp_pf = serializedObject.FindProperty("PrefabsSet");
            sp_pfDrawPrev = serializedObject.FindProperty("AlwaysDrawPreview");
            sp_PhysicalPlacement = serializedObject.FindProperty("PhysicalPlacement");
            sp_PhysicalPlacementEnabled = sp_PhysicalPlacement.FindPropertyRelative("Enabled");
        }

        protected override void DrawProperties()
        {
            base.DrawProperties(); // Draw spawning params

            GUILayout.Space(5);


            #region Drawing Scriptable Stamper Set Inspector Preview

            if (Get.PrefabsSet) EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            //if (Get.PrefabsSet == null) EditorGUILayout.HelpBox("Stamper Set preset is needed for component to work!", MessageType.Info);

            // Prefabs Set Field -------------------------------
            EditorGUILayout.BeginHorizontal();

            if (Get.PrefabsSet)
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Default), new GUILayoutOption[] { GUILayout.Width(24), GUILayout.Height(20) })) Get._displaySimplifiedSet = !Get._displaySimplifiedSet;

            EditorGUIUtility.labelWidth = 90;
            EditorGUILayout.PropertyField(sp_pf);
            if (Get.PrefabsSet && Get.PrefabsSet.Prefabs != null) EditorGUILayout.LabelField("(" + Get.PrefabsSet.Prefabs.Count + ")", GUILayout.Width(24));
            if (GUILayout.Button("Create New", GUILayout.Width(84))) Get.PrefabsSet = (OStamperSet)FGenerators.GenerateScriptable(CreateInstance<OStamperSet>(), "OS_");
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;
            GUILayout.Space(4f);

            // Prefabs settings quick view ---------------------
            if (Get.PrefabsSet)
            {
                EditorGUI.BeginChangeCheck();

                SerializedObject pfs = new SerializedObject(Get.PrefabsSet);
                SerializedProperty sp = pfs.GetIterator();
                sp.Next(true);

                if (Get._displaySimplifiedSet)
                {
                    sp.NextVisible(false);
                    sp.NextVisible(false); EditorGUILayout.PropertyField(sp);
                    sp.NextVisible(false);
                    sp.NextVisible(false);// EditorGUILayout.PropertyField(sp);

                    // Draw Min Max Slider for rotation ranges
                    GUILayout.Space(6);
                    EditorGUILayout.BeginHorizontal();
                    float n = Get.PrefabsSet.RotationRanges.x;
                    float p = Get.PrefabsSet.RotationRanges.y;
                    EditorGUILayout.MinMaxSlider(new GUIContent(sp.displayName, sp.tooltip), ref n, ref p, -180f, 180f);

                    Get.PrefabsSet.RotationRanges = new Vector2(Mathf.Round(n), Mathf.Round(p));
                    EditorGUILayout.LabelField(Get.PrefabsSet.RotationRanges.x + "\x00B0 to " + Get.PrefabsSet.RotationRanges.y + "\x00B0", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();

                    sp.NextVisible(false);
                    sp.NextVisible(false);/* EditorGUILayout.PropertyField(sp);*/
                    sp.NextVisible(false); EditorGUILayout.PropertyField(sp);
                    sp.NextVisible(false);
                    sp.NextVisible(false); EditorGUILayout.PropertyField(sp);
                    sp.NextVisible(false);
                    sp.NextVisible(false); EditorGUILayout.PropertyField(sp);
                    sp.NextVisible(false);
                    sp.NextVisible(false);
                    sp.NextVisible(false);
                    sp.NextVisible(false); EditorGUILayout.PropertyField(sp);
                }
                else
                {
                    sp.NextVisible(false);
                    sp.NextVisible(false);
                    do { EditorGUILayout.PropertyField(sp); } while (sp.NextVisible(false));
                }

                pfs.ApplyModifiedProperties();

                if (EditorGUI.EndChangeCheck()) Get.spawningInfo = Get.PrefabsSet.RefreshEmitInfo(Get.spawningInfo, Get.transform.parent);

                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(4f);
            EditorGUIUtility.labelWidth = 145;
            SerializedProperty sp_bottom = sp_pfDrawPrev.Copy();
            EditorGUILayout.PropertyField(sp_bottom);
            EditorGUIUtility.labelWidth = 165;
            sp_bottom.NextVisible(false); EditorGUILayout.PropertyField(sp_bottom);
            sp_bottom.NextVisible(false); EditorGUILayout.PropertyField(sp_bottom);
            EditorGUIUtility.labelWidth = 0;
            GUILayout.Space(4f);

            if (Get.PrefabsSet == null)
            {
                EditorGUILayout.HelpBox("  First assign Objects Stamper Set!", MessageType.Info);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            #endregion Drawing Scriptable Stamper Set Inspector Preview


            if (GUILayout.Button(new GUIContent("  Randomize Preview", FGUI_Resources.Tex_Refresh), GUILayout.Height(22)))
            {
                if (Get._editorPreview) FGenerators.DestroyObject(Get._editorPreview);
                Get.spawningInfo = Get.PrefabsSet.Emit(false, Get.transform.parent);
                repaint = true;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("  Test Emit", FGUI_Resources.Tex_Movement), GUILayout.Height(22)))
            {
                Get._EditorEmitPreview();
                repaint = true;
            }

            if (GUILayout.Button(new GUIContent("  Emit and Detach", FGUI_Resources.Tex_Movement), GUILayout.Height(22)))
            {
                Get._EditorEmitAndDetach();
                Get.spawningInfo = Get.PrefabsSet.Emit(false, Get.transform.parent);
                repaint = true;
            }
            EditorGUILayout.EndHorizontal();


            if (Get._editorPreview || Get.SpawnedObject)
            {
                if (GUILayout.Button(new GUIContent("  Clear Spawned", FGUI_Resources.Tex_Remove), GUILayout.Height(22)))
                {
                    Get.ClearPreviews();
                    EditorUtility.SetDirty(Get);
                    //if (Get._editorPreview) FGenerators.DestroyObject(Get._editorPreview);
                    //Get.spawningInfo = Get.PrefabsSet.Emit();
                }
            }

            #region Hide

            //if (GUILayout.Button(new GUIContent("  Check Placement", FGUI_Resources.TexMotionIcon), GUILayout.Height(22)))
            //{
            //    OStamperSet.PlacementVolumeRaycastingData volume = Get.PrefabsSet.GetRaycastingVolumeFor(Get.GetSpawnInfo(), Get.transform);
            //    OStamperSet.RaycastingRestrictionsCheckResult result = Get.PrefabsSet.CheckRestrictionsOn(Get.GetSpawnInfo(), volume);
            //    if ( result.allow == false)
            //    {
            //        var overlapCheckResult = Get.PrefabsSet.CheckOverlapOnFullLineCast(Get.GetSpawnInfo(), volume);
            //        //Debug.Log("Result: " + overlapCheckResult.info);
            //    }
            //}

            #endregion Hide
        }


        protected override void _DrawLastProperties()
        {

            GUILayout.Space(8);

            EditorGUI.BeginChangeCheck();

            Get.PhysicalPlacement._Editor_DrawSetupToggle(sp_PhysicalPlacementEnabled);

            if (Get.PhysicalPlacement._Editor_Foldout)
            {
                Get.PhysicalPlacement._Editor_DrawSetup(sp_PhysicalPlacement, false);
                GUILayout.Space(4);
            }

            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(Get);

            GUILayout.Space(6);

        }

    }

#endif
}