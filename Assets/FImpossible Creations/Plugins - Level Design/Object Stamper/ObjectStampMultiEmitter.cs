#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FIMSpace.Generating
{
    /// <summary>
    /// Implementation of spawning groups of prefabs with multiple OStamperSets on multiple spawn areas
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Level Design/Object Stamp Multi Emitter", 2)]
    public partial class ObjectStampMultiEmitter : ObjectStampEmitterBase, IGenerating
    {
        public OStampPhysicalPlacementSetup PhysicalPlacement;

        public OStamperMultiSet MultiSet;
        public List<SpawnArea> Areas;
        public List<GameObject> Spawned;
        public int Selected = -1;

        private int internalSelected = 0;

        private ObjectStamperEmittedInfo spawningInfo;
        public int editorSelected = -1;

        protected override OStamperSet GetStamperSet() { return MultiSet.PrefabsSets[internalSelected]; }

        protected override ObjectStamperEmittedInfo GetSpawnInfo() { return spawningInfo; }

        private void Start()
        {
            if (MultiSet == null)
            {
                Debug.Log("[Objects Stamper] No 'MultiSet' assigned to '" + name + "'!");
                return;
            }

            if (SpawnOnStart)
            {
                if (Spawned == null || Spawned.Count == 0)
                {
                    MultiSpawn();
                    IG_CallAfterGenerated();
                }
            }
        }



        public void Generate()
        {
            if (Spawned == null || Spawned.Count == 0) MultiSpawn();
        }

        public void PreviewGenerate() { }


        /// <summary>
        /// For physical simulation, call IG_CallAfterGenerated(); after MultiSpawn!
        /// </summary>
        public void MultiSpawn(bool multiDetach = false)
        {
            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;
            ClearAlreadySpawned();

            //try
            //{



            for (int i = 0; i < Areas.Count; i++) // Each Area - can be one but more often few
            {
                SpawnArea area = Areas[i];

                Transform parentSpace = null;
                if (area.Shape != SpawnArea.EShape.Points) parentSpace = transform.parent;

                for (int s = 0; s < area.Sets.Count; s++) // Each selected spawn set in area - can be one or can be multiple
                {
                    #region Null checks and preparation for references (stamperSet, setParams)

                    if (MultiSet == null) { UnityEngine.Debug.Log("[Object Stamp Multi Emitter] Null Multi Set! (" + name + ")"); break; }
                    if (MultiSet.PrefabSetSettings == null) { UnityEngine.Debug.Log("[Object Stamp Multi Emitter] Null PrefabSetSettings! (" + name + ")"); break; }

                    int setId = area.Sets[s];
                    if (setId >= MultiSet.PrefabsSets.Count || setId >= MultiSet.PrefabSetSettings.Count)
                    {
                        Debug.Log("[Object Stamp Multi Emitter] Wrong Set ID (out of bounds) (" + name + ")");
                        break;
                    }

                    if (setId < 0)
                    {
                        if (MultiSet.PrefabsSets.Count == 1) setId = 0;
                        UnityEngine.Debug.Log("[Object Stamp Multi Emitter] Not assigned Spawns Set in Spawn Area? (" + name + ")");
                    }

                    OStamperSet stamperSet = MultiSet.PrefabsSets[setId];
                    OStamperMultiSet.MultiStamperSetParameters setParams = MultiSet.PrefabSetSettings[setId];
                    internalSelected = setId;

                    if (stamperSet == null || setParams == null)
                    {
                        Debug.Log("[Object Stamp Multi Emitter] Null Stamper! (" + name + ")");
                        break;
                    }

                    #endregion


                    // One limit and randomly choosing prefabs
                    if (setParams.LimitMode == OStamperMultiSet.MultiStamperSetParameters.ECountLimit.OneLimitForThisSet)
                    {
                        int targetSpawnCount = Mathf.RoundToInt((float)setParams.GetRandomLimitCount() * area.Multiply[s]);
                        int count = 0;


                        for (int sp = 0; sp < targetSpawnCount; sp++)
                        {
                            if (area.Shape == SpawnArea.EShape.Points)
                                if (count >= area.Points.Count) break;

                            // Tries to find right position to spawn
                            for (int r = 0; r < MaxRetryAttempts + 1; r++) // When spawned successfully then loop breaks
                            {
                                transform.position = startPosition;
                                transform.rotation = startRotation;

                                EmitPoint ep = area.GetRandomLocalPoint();
                                Vector3 randomPosition = transform.TransformPoint(ep.pos);

                                transform.position = randomPosition;
                                transform.rotation = transform.rotation * ep.rot;

                                spawningInfo = stamperSet.Emit(false, parentSpace);
                                GameObject spawned = SpawnEmitPrefab(stamperSet);

                                if (spawned) 
                                {
                                    area.LatestSpawned.Add(spawned);
                                    count++; break; // Succesfully spawned
                                } 
                            }
                        }
                    }
                    else // Spawn limit per prefab in stamper set
                    {

                        for (int prId = 0; prId < MultiSet.PrefabsSets[internalSelected].Prefabs.Count; prId++)
                        {
                            int targetSpawnCount = Mathf.RoundToInt((float)setParams.GetRandomLimitCount(prId, MultiSet) * area.Multiply[s]);

                            //Debug.Log(MultiSet.PrefabsSets[internalSelected].Prefabs[prId].Prefab.name + " Spawn: " + targetSpawnCount);

                            for (int spawnEmissions = 0; spawnEmissions < targetSpawnCount; spawnEmissions++)
                            {
                                // Tries to find right position to spawn
                                for (int r = 0; r < MaxRetryAttempts + 1; r++) // When spawned successfully then loop breaks
                                {
                                    transform.position = startPosition;
                                    transform.rotation = startRotation;

                                    EmitPoint ep = area.GetRandomLocalPoint();
                                    Vector3 randomPosition = transform.TransformPoint(ep.pos);

                                    transform.position = randomPosition;
                                    transform.rotation = transform.rotation * ep.rot;

                                    spawningInfo = stamperSet.GenerateInfoForPrefab(MultiSet.PrefabsSets[internalSelected].Prefabs[prId], parentSpace);

                                    GameObject spawned = SpawnEmitPrefab(stamperSet);
                                    if (spawned) break; // Succesfully spawned
                                }
                            }
                        }

                    }
                }
            }


            //}
            //catch (System.Exception exc)
            //{
            //    Debug.LogException(exc);
            //    transform.position = startPosition;
            //}

            transform.position = startPosition;
            transform.rotation = startRotation;

            if (multiDetach || AlwaysDetachSpawned)
            {
                for (int i = 0; i < Spawned.Count; i++)
                {
                    if (Spawned[i] == null) continue;
                    Spawned[i].transform.SetParent(transform.parent, true);
                }

                if (multiDetach) Spawned.Clear();
            }
            else
            {
                for (int i = 0; i < Spawned.Count; i++)
                {
                    if (Spawned[i] == null) continue;
                    Spawned[i].transform.SetParent(transform, true);
                }
            }
        }


        protected override GameObject InternalInstatiatePrefab(bool raycasted, bool setParent = true)
        {
            //if (raycasted == false) return null;

            GameObject instantiated = base.InternalInstatiatePrefab(raycasted, false);
            if (instantiated) Spawned.Add(instantiated);
            return instantiated;
        }


        /// <summary>
        /// Destroying already instantiated objects by multi emitter from scene
        /// </summary>
        public void ClearAlreadySpawned()
        {
            if (Spawned == null) Spawned = new List<GameObject>();
            for (int i = 0; i < Spawned.Count; i++) FGenerators.DestroyObject(Spawned[i]);

            for (int a = 0; a < Areas.Count; a++)
            {
                var ar = Areas[a];
                if (ar == null) continue;
                if (ar.LatestSpawned == null) ar.LatestSpawned = new List<GameObject>(); else ar.LatestSpawned.Clear();
            }

            Spawned.Clear();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Selection.activeGameObject == gameObject) return;
            Color pc = Gizmos.color;
            Gizmos.matrix = transform.localToWorldMatrix;

            if (Areas != null)
                if (Areas.Count > 0) if (Areas[0] != null)
                    {
                        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.2f);
                        Gizmos.DrawCube(Vector3.zero, new Vector3(Areas[0].Size.x, Areas[0].Size.magnitude * 0.1f, Areas[0].Size.y));
                    }

            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = pc;
        }

#endif



        private static List<GameObject> _toPhysSimulate;
        public void IG_CallAfterGenerated()
        {
            if (PhysicalPlacement.Enabled == false) return;

            if (_toPhysSimulate == null) _toPhysSimulate = new List<GameObject>(); else _toPhysSimulate.Clear();

            for (int a = 0; a < Areas.Count; a++)
            {
                if (Areas[a].ApplyPhysicalSimulation == false) continue;

                for (int g = 0; g < Areas[a].LatestSpawned.Count; g++)
                {
                    _toPhysSimulate.Add(Areas[a].LatestSpawned[g]);
                }
            }

            PhysicalPlacement.ProceedOn(_toPhysSimulate);
        }


        public override void SpawnIfNotEmittedYet()
        {
            if (Spawned == null || Spawned.Count == 0) MultiSpawn();
        }

        #region Spawn Area


        [System.Serializable]
        public class SpawnArea
        {
            public string Name;
            public SpawnArea(string name)
            {
                Name = name;
            }

            public enum EShape { Rectangle, Circle, Points, Volume }
            public EShape Shape = EShape.Rectangle;
            [Tooltip("For example making circle shape out of disc shape for spawning objects\nFollow editor scene window gizmos")]
            [Range(0f, 1f)] public float NoInside = 0f;
            //[Range(0f, 1f)] public float NoNearEdges = 0f;

            public Vector3 Center = Vector3.zero;
            public Vector2 Size = Vector2.one;
            public Vector3 VSize = Vector3.one;
            public List<int> Sets;
            public List<float> Multiply;
            public bool PointsFoldout = true;
            public List<EmitPoint> Points;

            [Tooltip("If using physical placement simulation settigns then you can exclude some areas from simulation if you wish.")]
            public bool ApplyPhysicalSimulation = true;

            [NonSerialized] public List<GameObject> LatestSpawned = new List<GameObject>();

            public EmitPoint GetRandomLocalPoint()
            {
                if (Shape == EShape.Rectangle)
                {
                    float r = FGenerators.GetRandom(NoInside, 1f) / 2f;
                    float x = FGenerators.GetRandom(r, .5f);
                    float y = FGenerators.GetRandom(r, .5f);

                    if (FGenerators.GetRandom(0f, 1f) < 0.5f) x = -x;
                    if (FGenerators.GetRandom(0f, 1f) < 0.5f) y = -y;

                    if (FGenerators.GetRandom(0f, 1f) < 0.5f)
                        x *= FGenerators.GetRandom(-1f, 1f);
                    else
                        y *= FGenerators.GetRandom(-1f, 1f);

                    return new EmitPoint(new Vector3(Size.x * x, 0f, Size.y * y) + Center);
                }
                else
                if (Shape == EShape.Circle)
                {
                    float radius = FGenerators.GetRandom(NoInside, 1f) * Size.x;
                    float per = FGenerators.GetRandom(0f, Mathf.PI * 2f);
                    return new EmitPoint(new Vector3(Mathf.Sin(per) * radius, 0f, Mathf.Cos(per) * radius) + Center);
                }
                else if (Shape == EShape.Points)
                {
                    return Points[FGenerators.GetRandom(0, Points.Count)];
                }
                else if (Shape == EShape.Volume)
                {
                    float x = FGenerators.GetRandom(-.5f, .5f);
                    float y = FGenerators.GetRandom(-.5f, .5f);
                    float z = FGenerators.GetRandom(-.5f, .5f);

                    return new EmitPoint(new Vector3(VSize.x * x, VSize.y * y, VSize.z * z) + Center);
                }

                return new EmitPoint(Vector3.zero);
            }
        }

        #endregion


        [System.Serializable]
        public class EmitPoint
        {
            public Vector3 pos;
            public Quaternion rot;

            public EmitPoint(Vector3 pos)
            {
                this.pos = pos;
                rot = Quaternion.identity;
            }

            public EmitPoint(Vector3 pos, Quaternion rot) : this(pos)
            {
                this.rot = rot;
            }
        }

    }


#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(ObjectStampMultiEmitter))]
    public class ObjectsStampMultiEmitterEditor : ObjectsStampEmitterBaseEditor
    {
        public ObjectStampMultiEmitter Get { get { if (_get == null) _get = (ObjectStampMultiEmitter)target; return _get; } }
        private ObjectStampMultiEmitter _get;

        SerializedProperty sp_pf;
        SerializedProperty sp_areas;

        private SerializedProperty sp_PhysicalPlacement;
        private SerializedProperty sp_PhysicalPlacementEnabled;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (Get.Areas == null) Get.Areas = new List<ObjectStampMultiEmitter.SpawnArea>();
            if (Get.Spawned == null) Get.Spawned = new List<GameObject>();

            sp_pf = serializedObject.FindProperty("MultiSet");
            sp_areas = serializedObject.FindProperty("Areas");

            sp_PhysicalPlacement = serializedObject.FindProperty("PhysicalPlacement");
            sp_PhysicalPlacementEnabled = sp_PhysicalPlacement.FindPropertyRelative("Enabled");
        }


        private string[] setEmitters;
        private int[] setEmittersId;


        protected override void DrawProperties()
        {
            base.DrawProperties();
            GUILayout.Space(5);

            Color bc = GUI.backgroundColor;

            // Prefabs Set Field -------------------------------
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 90;
            EditorGUILayout.PropertyField(sp_pf);
            if (Get.MultiSet && Get.MultiSet.PrefabsSets != null) EditorGUILayout.LabelField("(" + Get.MultiSet.PrefabsSets.Count + ")", GUILayout.Width(24));
            if (GUILayout.Button("Create New", GUILayout.Width(84))) Get.MultiSet = (OStamperMultiSet)FGenerators.GenerateScriptable(CreateInstance<OStamperMultiSet>(), "OMS_");
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;


            GUILayout.Space(4f);
            if (Get.MultiSet == null)
            {
                EditorGUILayout.HelpBox("  First assign Multi Objects Stamper Set!", MessageType.Info);
                serializedObject.ApplyModifiedProperties();
                return;
            }


            if (Get.MultiSet != null)
            {
                if (Get.MultiSet.PrefabsSets != null)
                {
                    setEmitters = new string[Get.MultiSet.PrefabsSets.Count + 1];
                    setEmittersId = new int[Get.MultiSet.PrefabsSets.Count + 1];
                    setEmitters[0] = "None";
                    setEmittersId[0] = -1;
                }
            }


            if (setEmitters != null)
                if (Get.MultiSet != null)
                    for (int i = 1; i < setEmitters.Length; i++)
                    {
                        if (Get.MultiSet.PrefabsSets[i - 1] == null) continue;

                        setEmitters[i] = Get.MultiSet.PrefabsSets[i - 1].name;
                        setEmittersId[i] = i - 1;
                    }

            if (Get.Areas.Count == 0)
            {
                GUI.backgroundColor = Color.green;
                GUI.color = Color.yellow;
            }

            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Spawn Areas (" + Get.Areas.Count + ")", "Add spawn areas to prepare placement for generated objects"));

            if (Get.Areas.Count == 0)
            {
                GUI.backgroundColor = Color.white;
                GUI.color = Color.white;
            }

            if (GUILayout.Button("+", GUILayout.Width(24)))
            {
                var nArea = new ObjectStampMultiEmitter.SpawnArea("Area " + Get.Areas.Count.ToString());

                if (Get.MultiSet.PrefabsSets.Count > 0)
                {
                    if (nArea.Sets == null) nArea.Sets = new List<int>();
                    if (nArea.Multiply == null) nArea.Multiply = new List<float>();
                    nArea.Sets.Add(0);
                    nArea.Multiply.Add(1f);
                    Get.Selected = 0;
                }

                Get.Areas.Add(nArea);
                Changed();
                EditorGUILayout.EndHorizontal();
                return;
            }

            EditorGUILayout.EndHorizontal();

            float width = 0;
            float size = 24;
            GUILayoutOption opt = GUILayout.Width(size);

            if (Get.Selected >= Get.Areas.Count) Get.Selected = -1;

            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < Get.Areas.Count; i++)
            {
                width += 24;
                if (i == Get.Selected) GUI.backgroundColor = Color.green;
                if (GUILayout.Button(i.ToString(), opt)) { if (Get.Selected == i) Get.Selected = -1; else Get.Selected = i; }
                if (i == Get.Selected) GUI.backgroundColor = bc;

                if (width + size > EditorGUIUtility.currentViewWidth - 60) { width = 0f; EditorGUILayout.EndHorizontal(); EditorGUILayout.BeginHorizontal(); }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();


            if (Get.Selected > -1)
            {

                ObjectStampMultiEmitter.SpawnArea area = Get.Areas[Get.Selected];
                if (area != null)
                {
                    GUILayout.Space(7);

                    SerializedProperty prop = sp_areas.GetArrayElementAtIndex(Get.Selected);
                    SerializedProperty spc = prop.Copy();

                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

                    prop.Next(true);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent(area.Name, "Select desired shape of how you want to place your generated objects"), GUILayout.Width(100));

                    prop.NextVisible(false); EditorGUILayout.PropertyField(prop, GUIContent.none);

                    GUILayout.FlexibleSpace();

                    Color prebc = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(1f, 0.635f, 0.635f, 1f);

                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Remove), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(24), GUILayout.Height(24) })) { Get.Areas.RemoveAt(Get.Selected); Changed(); }
                    GUI.backgroundColor = prebc;

                    EditorGUILayout.EndHorizontal();


                    if (Get.PhysicalPlacement.Enabled)
                    {
                        EditorGUIUtility.labelWidth = 160;
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(spc.FindPropertyRelative("ApplyPhysicalSimulation"));
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(FGUI_Resources.Tex_Physics, EditorStyles.label, GUILayout.Height(18), GUILayout.Width(20))) { area.ApplyPhysicalSimulation = !area.ApplyPhysicalSimulation; Changed(); }
                        EditorGUILayout.EndHorizontal();
                        EditorGUIUtility.labelWidth = 0;
                        GUILayout.Space(4);
                    }


                    if (area.Shape == ObjectStampMultiEmitter.SpawnArea.EShape.Volume)
                    {
                        //prop.NextVisible(false); 
                        prop.NextVisible(false); prop.NextVisible(false); EditorGUILayout.PropertyField(prop); prop.NextVisible(false);
                        //prop.NextVisible(false); EditorGUILayout.PropertyField(prop);
                    }
                    else if (area.Shape != ObjectStampMultiEmitter.SpawnArea.EShape.Points)
                    {
                        prop.NextVisible(false); EditorGUILayout.PropertyField(prop);
                        //prop.NextVisible(false); EditorGUILayout.PropertyField(prop);
                    }
                    else
                    {
                        prop.NextVisible(false);
                        //prop.NextVisible(false);
                    }

                    prop.NextVisible(false);

                    if (area.Shape != ObjectStampMultiEmitter.SpawnArea.EShape.Points) EditorGUILayout.PropertyField(prop);
                    //EditorGUILayout.PropertyField(prop, new GUIContent("Offset Points"));

                    prop.NextVisible(false);

                    if (area.Shape == ObjectStampMultiEmitter.SpawnArea.EShape.Rectangle)
                        EditorGUILayout.PropertyField(prop);
                    else
                    if (area.Shape == ObjectStampMultiEmitter.SpawnArea.EShape.Circle)
                        prop.vector2Value = new Vector2(EditorGUILayout.FloatField("Radius", prop.vector2Value.x), prop.vector2Value.x);
                    else
                    if (area.Shape == ObjectStampMultiEmitter.SpawnArea.EShape.Points)
                    {
                        if (area.Points.Count == 0) area.Points.Add(new ObjectStampMultiEmitter.EmitPoint(Vector3.right));

                        GUILayout.Space(2);
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        GUILayout.Space(2);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(" " + (area.PointsFoldout ? "▼" : "►") + "  Points (" + area.Points.Count + ")", EditorStyles.label)) { area.PointsFoldout = !area.PointsFoldout; }
                        //GUILayout.FlexibleSpace();
                        if (GUILayout.Button("+", GUILayout.Width(24))) { area.Points.Add(new ObjectStampMultiEmitter.EmitPoint(Vector3.forward)); Changed(); }
                        EditorGUILayout.EndHorizontal();

                        if (area.PointsFoldout)
                        {
                            SerializedProperty sp_p = sp_areas.GetArrayElementAtIndex(Get.Selected).FindPropertyRelative("Points");
                            SerializedProperty sp_pf = sp_p.Copy();
                            sp_pf.NextVisible(true);

                            for (int i = 0; i < sp_p.arraySize; i++)
                            {
                                sp_pf.NextVisible(false);
                                SerializedProperty sp_v = sp_pf.FindPropertyRelative("pos");
                                sp_v.vector3Value = EditorGUILayout.Vector3Field(new GUIContent("Point [" + i + "]"), sp_v.vector3Value);
                                sp_v.NextVisible(false); sp_v.quaternionValue = Quaternion.Euler(EditorGUILayout.Vector3Field(new GUIContent("Rotation"), sp_v.quaternionValue.eulerAngles));
                                //EditorGUILayout.PropertyField(sp_pf, new GUIContent("Point [" + i + "]"));
                                GUILayout.Space(7);
                            }
                        }

                        EditorGUILayout.EndVertical();

                    }

                    GUILayout.Space(5);
                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyleH);

                    if (area.Sets == null)
                    {
                        area.Sets = new List<int>();
                        area.Multiply = new List<float>();
                    }

                    if (area.Points == null) area.Points = new List<ObjectStampMultiEmitter.EmitPoint>();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Spawns for this area (" + area.Sets.Count + ")" + (area.Sets.Count == 0 ? " !!!" : ""), "Select stampers from stamper multi set to spawn with this shape"));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("+", GUILayout.Width(24))) { area.Sets.Add(-1); area.Multiply.Add(1f); Changed(); }
                    EditorGUILayout.EndHorizontal();

                    for (int i = 0; i < area.Sets.Count; i++)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        EditorGUILayout.BeginHorizontal();
                        area.Sets[i] = EditorGUILayout.IntPopup(area.Sets[i], setEmitters, setEmittersId);

                        GUI.backgroundColor = new Color(1f, 0.635f, 0.635f, 1f);

                        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Remove), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(24), GUILayout.Height(24) }))
                        {
                            area.Sets.RemoveAt(i);
                            area.Multiply.RemoveAt(i);
                            serializedObject.ApplyModifiedProperties();
                            Changed();
                            return;
                        }

                        GUI.backgroundColor = prebc;
                        EditorGUILayout.EndHorizontal();

                        EditorGUIUtility.fieldWidth = 36;
                        area.Multiply[i] = EditorGUILayout.Slider(new GUIContent("Multiply Count", "Multiplying defined spawn count inside stamper file"), area.Multiply[i], 0f, 2f);
                        EditorGUIUtility.fieldWidth = 0;

                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.EndVertical();


                    EditorGUILayout.EndVertical();
                }
            }

            GUI.backgroundColor = bc;

            GUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("  Test Emit", FGUI_Resources.Tex_Movement), GUILayout.Height(22)))
            {
                Get.MultiSpawn();
                Get.IG_CallAfterGenerated();
                //Get._EditorEmitPreview();
            }

            if (GUILayout.Button(new GUIContent("  Emit and Detach", FGUI_Resources.Tex_Movement), GUILayout.Height(22)))
            {
                Get.MultiSpawn(true);
                Get.IG_CallAfterGenerated();
                //Get._EditorEmitPreview();
            }

            EditorGUILayout.EndHorizontal();

            if (Get.Spawned.Count > 0)
                if (GUILayout.Button(new GUIContent("  Clear Spawned", FGUI_Resources.Tex_Remove), GUILayout.Height(22)))
                {
                    Get.ClearAlreadySpawned();
                    Changed();
                    //if (Get._editorPreview) FGenerators.DestroyObject(Get._editorPreview);
                    //Get.spawningInfo = Get.PrefabsSet.Emit();
                }
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


        private void Changed()
        {
            EditorUtility.SetDirty(Get);
        }

        private void OnSceneGUI()
        {
            if (Get.Spawned == null) Get.Spawned = new List<GameObject>();
            //CompareFunction compare = Handles.zTest;

            Color preH = Handles.color;
            float step = 1f / (float)Get.Areas.Count;

            refSize = 0.05f;
            for (int i = 0; i < Get.Areas.Count; i++)
            {
                float sz = Get.transform.TransformVector(Get.Areas[i].Size).magnitude;
                if (sz > refSize) refSize = sz;
            }

            for (int i = 0; i < Get.Areas.Count; i++)
            {
                if (i == Get.Selected) continue;
                Color newCol = Color.HSVToRGB(step * (float)i, 0.9f, 0.9f);
                newCol.a = 0.3f;
                Handles.color = newCol;
                if (FGenerators.CheckIfExist_NOTNULL(Get.Areas[i])) DrawArea(Get.Areas[i], i);
            }

            if (Get.Selected > -1)
            {
                if (Get.Areas != null)
                    if (Get.Areas.Count > 0)
                        if (Get.Selected >= 0)
                        {
                            if (Get.Areas[Get.Selected].Shape == ObjectStampMultiEmitter.SpawnArea.EShape.Circle)
                                Handles.color = new Color(1f, 1f, 1f, 0.25f);
                            else
                                Handles.color = new Color(1f, 1f, 1f, 0.7f);

                            DrawArea(Get.Areas[Get.Selected], Get.Selected, true);
                        }
            }

            Handles.color = preH;

            //Handles.zTest = compare;
        }



        float refSize = 0.1f;

        //float refScale = 0f;
        private void DrawArea(ObjectStampMultiEmitter.SpawnArea area, int id, bool selected = false)
        {
            if (area == null) return;
            Color backCol = Handles.color;
            //float refSize = Get.transform.TransformVector(area.Size).magnitude;
            //if (Event.current == null && Event.current.type == EventType.MouseDown) refScale = refSize;

            Handles.matrix = Get.transform.localToWorldMatrix;

            //Handles.Label(area.Center, "[" + id + "]");
            //Handles.Label(area.Center, new GUIContent(Get.MultiSet.PrefabsSets[id].Prefabs[0].Preview));


            if (area.Shape == ObjectStampMultiEmitter.SpawnArea.EShape.Circle)
            {
                //if (selected)
                DrawCircle(area.Center, area.Size.x, area.NoInside);
                //else
                //    Handles.DrawWireArc(area.Center, Vector3.up, Vector3.forward, 360f, area.Size.x);

            }
            else
            if (area.Shape == ObjectStampMultiEmitter.SpawnArea.EShape.Rectangle)
            {
                //if (selected)
                DrawRect(area.Center, area.Size, area.NoInside);
                //else
                //    Handles.DrawWireCube(area.Center, new Vector3(area.Size.x, 0.01f, area.Size.y));
            }
            else if (area.Shape == ObjectStampMultiEmitter.SpawnArea.EShape.Volume)
            {
                Handles.DrawWireCube(area.Center, area.VSize);
            }

            Handles.matrix = Matrix4x4.identity;
            Vector3 p = Get.transform.TransformPoint(area.Center);
            Handles.color = Color.cyan;
            area.Center = Get.transform.InverseTransformPoint(FEditor_TransformHandles.PositionHandle(p, Get.transform.rotation, 0.3f, true, false, false));
            //area.Center = Get.transform.InverseTransformPoint( Handles.FreeMoveHandle(p, Quaternion.identity, refSize * 0.1f, Vector3.zero, Handles.CircleHandleCap));
            Handles.color = backCol;

            if (selected)
            {
                if (area.Shape == ObjectStampMultiEmitter.SpawnArea.EShape.Volume)
                {
                    Vector3 VSize = FEditor_TransformHandles.ScaleHandle(new Vector3(area.VSize.x, area.VSize.y, area.VSize.z), p + Get.transform.TransformVector(new Vector3(refSize * 0.15f, 0f, refSize * 0.15f)), Quaternion.identity, 0.4f, area.Shape == ObjectStampMultiEmitter.SpawnArea.EShape.Circle, true);
                    area.VSize = VSize;
                }
                else
                if (area.Shape != ObjectStampMultiEmitter.SpawnArea.EShape.Points)
                {
                    Vector3 size = FEditor_TransformHandles.ScaleHandle(new Vector3(area.Size.x, area.Size.x, area.Size.y), p + Get.transform.TransformVector(new Vector3(refSize * 0.15f, 0f, refSize * 0.15f)), Quaternion.identity, 0.4f, area.Shape == ObjectStampMultiEmitter.SpawnArea.EShape.Circle, true);
                    area.Size = new Vector2(size.x, size.z);
                }

                if (area.Shape == ObjectStampMultiEmitter.SpawnArea.EShape.Points)
                    for (int i = 0; i < area.Points.Count; i++)
                    {
                        Handles.DrawSolidDisc(Get.transform.TransformPoint(area.Points[i].pos), Vector3.up, refSize * 0.0075f);
                        area.Points[i].pos = Get.transform.InverseTransformPoint(FEditor_TransformHandles.PositionHandle(Get.transform.TransformPoint(area.Points[i].pos), Get.transform.rotation * area.Points[i].rot, 0.4f, true, false));
                        //area.Points[i].rot = FEditor_TransformHandles.RotationHandle(area.Points[i].rot, Get.transform.TransformPoint(area.Points[i].pos),  0.2f, true);
                    }

            }
            else
            {

                bool drawedButton = false;
                if (SceneView.currentDrawingSceneView != null && SceneView.currentDrawingSceneView.camera != null)
                    if (Get.MultiSet)
                        if (Get.MultiSet.PrefabsSets.Count > 0)
                            if (id < Get.MultiSet.PrefabsSets.Count)
                                if (Get.MultiSet.PrefabsSets[id])
                                    if (Get.MultiSet.PrefabsSets[id].Prefabs != null)
                                        if (Get.MultiSet.PrefabsSets[id].Prefabs.Count > 0)
                                        {
                                            if (area.Sets != null && area.Sets.Count > 0)
                                            {
                                                drawedButton = true;
                                                Handles.BeginGUI();
                                                Handles.SetCamera(SceneView.currentDrawingSceneView.camera);

                                                Vector3 buttonPos;
                                                if (area.Shape == ObjectStampMultiEmitter.SpawnArea.EShape.Points)
                                                    buttonPos = Get.transform.TransformPoint(area.Center);
                                                else
                                                    buttonPos = Get.transform.TransformPoint(area.Center + area.Size.x * (Vector3.left / 2) * area.NoInside);

                                                if (DrawButton(new GUIContent(Get.MultiSet.PrefabSetSettings[area.Sets[0]].Preview), buttonPos + Vector3.left * refSize * 0.05f, 2200f)) Get.Selected = id;
                                                //if (DrawButton(new GUIContent(Get.MultiSet.PrefabsSets[id].Prefabs[0].Preview), Get.transform.TransformPoint(area.Center), 2200f)) Get.Selected = id;
                                                Handles.EndGUI();
                                            }
                                        }

                if (!drawedButton)
                    if (Handles.Button(p, Quaternion.identity, refSize * 0.1f, refSize * (drawedButton ? 0.0025f : 0.1f), Handles.SphereHandleCap))
                        Get.Selected = id;

                if (area.Shape == ObjectStampMultiEmitter.SpawnArea.EShape.Points)
                    for (int i = 0; i < area.Points.Count; i++)
                        Handles.SphereHandleCap(0, Get.transform.TransformPoint(area.Points[i].pos), Quaternion.identity, refSize * 0.05f, EventType.Repaint);
            }

        }


        bool DrawButton(GUIContent content, Vector3 pos, float size)
        {
            float sc = HandleUtility.GetHandleSize(pos);
            float hSize = Mathf.Sqrt(size) * 2f - sc * 16;
            hSize /= 2f;

            if (hSize > 0f)
            {
                Handles.BeginGUI();
                Vector2 pos2D = HandleUtility.WorldToGUIPoint(pos);
                float hhSize = hSize / 2f;
                if (GUI.Button(new Rect(pos2D.x - hhSize, pos2D.y - hhSize, hSize, hSize), content))
                {
                    Handles.EndGUI();
                    return true;
                }

                Handles.EndGUI();
            }

            return false;
        }


        private Vector3 sizeW;
        private Vector3 sizeF;
        void DrawRect(Vector3 position, Vector2 size, float noInside = 1f)
        {
            List<Vector3> verts = new List<Vector3>();
            sizeW = new Vector3(size.x / 2f, 1f, size.y / 2f);
            sizeF = new Vector3(size.x / 2f, 1f, size.y / 2f) * noInside;

            if (noInside <= 0f)
            {
                verts.Add(VWrld(new Vector3(-1f, 0f, 1f)));
                verts.Add(VWrld(new Vector3(1f, 0f, 1f)));
                verts.Add(VWrld(new Vector3(1f, 0f, -1f)));
                verts.Add(VWrld(new Vector3(-1f, 0f, -1f)));

                for (int i = 0; i < verts.Count; i++) verts[i] += position;
                Handles.DrawAAConvexPolygon(verts.ToArray());
            }
            else
            {
                verts.Add(VWrld(new Vector3(-1f, 0f, 1f)));
                verts.Add(VWrld(new Vector3(1f, 0f, 1f)));
                verts.Add(VFill(new Vector3(1f, 0f, 1f)));
                verts.Add(VFill(new Vector3(-1f, 0f, 1f)));

                for (int i = 0; i < verts.Count; i++) verts[i] += position;
                Handles.DrawAAConvexPolygon(verts.ToArray());
                verts.Clear();


                verts.Add(VWrld(new Vector3(-1f, 0f, 1f)));
                verts.Add(VWrld(new Vector3(-1f, 0f, -1f)));
                verts.Add(VFill(new Vector3(-1f, 0f, -1f)));
                verts.Add(VFill(new Vector3(-1f, 0f, 1f)));

                for (int i = 0; i < verts.Count; i++) verts[i] += position;
                Handles.DrawAAConvexPolygon(verts.ToArray());
                verts.Clear();




                verts.Add(VWrld(new Vector3(-1f, 0f, -1f)));
                verts.Add(VWrld(new Vector3(1f, 0f, -1f)));
                verts.Add(VFill(new Vector3(1f, 0f, -1f)));
                verts.Add(VFill(new Vector3(-1f, 0f, -1f)));

                for (int i = 0; i < verts.Count; i++) verts[i] += position;
                Handles.DrawAAConvexPolygon(verts.ToArray());
                verts.Clear();


                verts.Add(VWrld(new Vector3(1f, 0f, 1f)));
                verts.Add(VWrld(new Vector3(1f, 0f, -1f)));
                verts.Add(VFill(new Vector3(1f, 0f, -1f)));
                verts.Add(VFill(new Vector3(1f, 0f, 1f)));

                for (int i = 0; i < verts.Count; i++) verts[i] += position;
                Handles.DrawAAConvexPolygon(verts.ToArray());
                verts.Clear();

            }

        }


        void DrawCircle(Vector3 position, float radius, float noInside = 1f)
        {
            List<Vector3> verts = new List<Vector3>();
            sizeW = new Vector3(radius, 1f, radius);
            sizeF = new Vector3(radius, 1f, radius) * noInside;

            float steps = 20f;
            float step = 360f / steps;

            if (noInside <= 0f)
            {
                for (int r = 0; r < (int)steps; r++)
                {
                    float sin = Mathf.Sin(step * r * Mathf.Deg2Rad);
                    float cos = Mathf.Cos(step * r * Mathf.Deg2Rad);
                    verts.Add(VWrld(new Vector3(sin, 0f, cos)));
                }

                for (int i = 0; i < verts.Count; i++) verts[i] += position;
                Handles.DrawAAConvexPolygon(verts.ToArray());
            }
            else
            {
                int quart = (int)(steps / 4f);

                for (int r = 0; r <= quart; r++)
                {
                    float sin = Mathf.Sin(step * r * Mathf.Deg2Rad); float cos = Mathf.Cos(step * r * Mathf.Deg2Rad);
                    verts.Add(VWrld(new Vector3(sin, 0f, cos)));
                }

                for (int r = quart; r >= 2; r -= 1)
                {
                    float sin = Mathf.Sin(step * r * Mathf.Deg2Rad); float cos = Mathf.Cos(step * r * Mathf.Deg2Rad);
                    verts.Add(VFill(new Vector3(sin, 0f, cos)));
                }

                for (int i = 0; i < verts.Count; i++) verts[i] += position;
                Handles.DrawAAConvexPolygon(verts.ToArray());
                verts.Clear();



                for (int r = quart; r <= quart * 2; r++)
                {
                    float sin = Mathf.Sin(step * r * Mathf.Deg2Rad); float cos = Mathf.Cos(step * r * Mathf.Deg2Rad);
                    verts.Add(VWrld(new Vector3(sin, 0f, cos)));
                }

                for (int r = quart * 2; r >= quart + 2; r -= 1)
                {
                    float sin = Mathf.Sin(step * r * Mathf.Deg2Rad); float cos = Mathf.Cos(step * r * Mathf.Deg2Rad);
                    verts.Add(VFill(new Vector3(sin, 0f, cos)));
                }

                for (int i = 0; i < verts.Count; i++) verts[i] += position;
                Handles.DrawAAConvexPolygon(verts.ToArray());
                verts.Clear();




                for (int r = quart * 2; r <= quart * 3; r++)
                {
                    float sin = Mathf.Sin(step * r * Mathf.Deg2Rad); float cos = Mathf.Cos(step * r * Mathf.Deg2Rad);
                    verts.Add(VWrld(new Vector3(sin, 0f, cos)));
                }

                for (int r = quart * 3; r >= quart * 2 + 2; r -= 1)
                {
                    float sin = Mathf.Sin(step * r * Mathf.Deg2Rad); float cos = Mathf.Cos(step * r * Mathf.Deg2Rad);
                    verts.Add(VFill(new Vector3(sin, 0f, cos)));
                }

                for (int i = 0; i < verts.Count; i++) verts[i] += position;
                Handles.DrawAAConvexPolygon(verts.ToArray());
                verts.Clear();



                for (int r = quart * 3; r <= quart * 4; r++)
                {
                    float sin = Mathf.Sin(step * r * Mathf.Deg2Rad); float cos = Mathf.Cos(step * r * Mathf.Deg2Rad);
                    verts.Add(VWrld(new Vector3(sin, 0f, cos)));
                }

                for (int r = quart * 4; r >= quart * 3 + 2; r -= 1)
                {
                    float sin = Mathf.Sin(step * r * Mathf.Deg2Rad); float cos = Mathf.Cos(step * r * Mathf.Deg2Rad);
                    verts.Add(VFill(new Vector3(sin, 0f, cos)));
                }

                for (int i = 0; i < verts.Count; i++) verts[i] += position;
                Handles.DrawAAConvexPolygon(verts.ToArray());
                verts.Clear();


            }

        }


        private Vector3 VWrld(Vector3 pos) { return Vector3.Scale(pos, sizeW); }
        private Vector3 VFill(Vector3 pos) { return Vector3.Scale(pos, sizeF); }

    }
#endif


}