using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using FIMSpace.Generating.Checker;
using FIMSpace.Generating.PathFind;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

namespace FIMSpace.Generating
{
    public abstract class PGGGeneratorBase : PGGGeneratorRoot
    {
        public bool GenerateOnStart = true;
        public bool AutoRefresh = true;
        public int Seed = 0;
        public bool RandomSeed = true;

        [HideInInspector] public List<InstantiatedFieldInfo> Generated = new List<InstantiatedFieldInfo>();
        [HideInInspector] public UnityEvent RunAfterGenerating;

        #region Post Generating Features

        [HideInInspector] public bool UseOutlineFill = false;
        [HideInInspector] public OutlineFillHelper OutlineFill = new OutlineFillHelper();

        [HideInInspector] public bool UseRectangleFill = false;
        [HideInInspector] public RectangleFillHelper RectangleFill = new RectangleFillHelper();

        #endregion


        /// <summary> Can be used for some post-generation processes like rectangle fill (must be implemented-supported by the particular generator) </summary>
        [System.NonSerialized] public List<CheckerField> GeneratorCheckers = new List<CheckerField>();
        [HideInInspector] public bool _Editor_drawAdd = false;


        protected virtual void Start()
        {
            if (GenerateOnStart) GenerateObjects();
        }


        #region Run Generation Methods

        public virtual void GenerateObjects()
        {
            RunPostGeneration();
            if (RunAfterGenerating != null) RunAfterGenerating.Invoke();
            _E_SetDirty();
        }

        public virtual void Prepare()
        {
            RandomizeSeed();
            FGenerators.SetSeed(Seed);
            GeneratorCheckers.Clear();
        }

        public virtual void RandomizeSeed()
        {
            if (RandomSeed) Seed = Random.Range(-999999, 999999);
        }


        public virtual void ClearGenerated(bool destroy = true)
        {
            if (Generated == null)
            {
                Generated = new List<InstantiatedFieldInfo>();
            }

            if (destroy)
            {
                List<GameObject> all = GetAllGeneratedObjects(true);

                for (int i = 0; i < all.Count; i++)
                    if (all[i] != null)
                        FGenerators.DestroyObject(all[i]);
            }

            Generated.Clear();

            _E_SetDirty();
        }

        #endregion


        #region Getting Generated Objects Methods


        public List<GameObject> GetAllGeneratedObjects(bool withContainers)
        {
            List<GameObject> allGen = new List<GameObject>();

            for (int i = 0; i < Generated.Count; i++)
            {
                PGGUtils.TransferFromListToList(Generated[i].Instantiated, allGen);
            }

            if (withContainers)
            {
                for (int i = 0; i < Generated.Count; i++)
                {
                    if (Generated[i] == null) continue;

                    allGen.Add(Generated[i].MainContainer);

                    for (int g = 0; g < Generated[i].InstantiationContainers.Count; g++)
                    {
                        if (Generated[i].InstantiationContainers[g] == null) continue;
                        if (Generated[i].InstantiationContainers[g].Transform == null) continue;
                        FGenerators.DestroyObject(Generated[i].InstantiationContainers[g].Transform.gameObject);
                    }
                }

            }

            return allGen;
        }


        public T GetComponentFromAllGenerated<T>() where T : Component
        {
            T comp = null;

            for (int g = 0; g < Generated.Count; g++)
            {
                InstantiatedFieldInfo tgtInfo = Generated[g];
                for (int i = 0; i < tgtInfo.Instantiated.Count; i++)
                {
                    comp = tgtInfo.Instantiated[i].GetComponentInChildren<T>();
                    if (comp) break;
                }
            }

            return comp;
        }


        public T GetComponentFromField<T>(FieldSetup targetFieldSetup) where T : Component
        {
            InstantiatedFieldInfo tgtInfo = null;

            for (int i = 0; i < Generated.Count; i++)
            {
                if (Generated[i].ParentSetup == targetFieldSetup) { tgtInfo = Generated[i]; break; }
            }

            if (tgtInfo == null) return null;

            T comp = null;
            for (int i = 0; i < tgtInfo.Instantiated.Count; i++)
            {
                comp = tgtInfo.Instantiated[i].GetComponentInChildren<T>();
                if (comp) break;
            }

            return comp;
        }



        public Transform GetGeneratedsContainerWhichNameContains(string partOfName, int generatedIndex = -1)
        {
            string toCheck = partOfName.ToLower();

            PGGUtils.CheckForNulls(Generated);

            for (int i = 0; i < Generated.Count; i++)
            {
                if (generatedIndex != -1) if (i != generatedIndex) continue;
                if (Generated[i].MainContainer == null) continue;

                for (int g = 0; g < Generated[i].MainContainer.transform.childCount; g++)
                {
                    Transform t = Generated[i].MainContainer.transform.GetChild(g);
                    if (t.name.ToLower().Contains(toCheck)) return t;
                }
            }

            return null;
        }


        #endregion


        #region Post Generation Features Implementations

        protected virtual void RunPostGeneration()
        {

            #region Supporting additional post-generation features

            if (UseOutlineFill)
            {
                var fills = OutlineFill.RunOnGenerator(this);
                for (int f = 0; f < fills.Count; f++)
                {
                    Generated.Add(fills[f]);

                    if (fills[f].OptionalCheckerFieldsData.Count > 0)
                        GeneratorCheckers.Add(fills[f].OptionalCheckerFieldsData[0]);
                }
            }

            if (UseRectangleFill)
            {
                var fill = RectangleFill.RunOnGenerator(this);
                if (fill != null) Generated.Add(fill);
            }

            #endregion

        }

        #endregion


        #region Gizmos


        #region Unity Editor

#if UNITY_EDITOR
        protected Color preColor;
        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
            preColor = GUI.color;

            DrawGizmos();

            Gizmos.color = preColor;
            Gizmos.matrix = Matrix4x4.identity;
        }

#endif

        #endregion


        protected void Gizmos_DrawRectangleFillShape(Vector3 cellSize)
        {
            if (UseRectangleFill)
            {
                Gizmos.color = new Color(0.1f, 0.9f, 0.15f, 0.3f);

                Vector3 off = new Vector3(cellSize.x * 1.5f, 0f, cellSize.z * 1.5f);
                if (RectangleFill.Size.x % 2 != 0) off.x += 1f;
                if (RectangleFill.Size.y % 2 != 0) off.z += 1f;
                Gizmos.DrawCube(Vector3.Scale(cellSize, RectangleFill.Center.V2toV3()) + off, Vector3.Scale(cellSize, RectangleFill.Size.V2toV3(0.1f)));
            }
        }


        protected virtual void DrawGizmos()
        {

        }

        #endregion


        /// <summary> Editor utility </summary>
        public void _E_SetDirty() 
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

    }


#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PGGGeneratorBase))]
    public abstract class PGGGeneratorBaseEditor : UnityEditor.Editor
    {
        public PGGGeneratorBase bGet { get { if (_bget == null) _bget = (PGGGeneratorBase)target; return _bget; } }
        private PGGGeneratorBase _bget;

        protected static List<string> _ignore;
        private static string[] _iignore;

        bool displayEvent = false;
        SerializedProperty sp_props;
        SerializedProperty sp_scr;
        protected SerializedProperty sp_UseOutlineFill;
        protected Color preGuiCol;
        protected Color preGuiBGCol;

        protected virtual void OnEnable()
        {
            sp_props = serializedObject.FindProperty("GenerateOnStart");
            sp_scr = serializedObject.FindProperty("m_Script");
            sp_UseOutlineFill = serializedObject.FindProperty("UseOutlineFill");

            _iignore = null;
            _ignore = new List<string>() { "m_Script", "GenerateOnStart", "AutoRefresh", "RandomSeed", "Seed" };
        }

        public override void OnInspectorGUI()
        {
            preGuiCol = GUI.color;
            preGuiBGCol = GUI.backgroundColor;

            if (_iignore == null) _iignore = _ignore.ToArray();

            DrawGUIHeader();
            serializedObject.Update();

            GUILayout.Space(4);
            SerializedProperty sp = sp_props.Copy();
            EditorGUILayout.BeginHorizontal(FGUI_Resources.BGInBoxStyle);
            EditorGUIUtility.labelWidth = 120;
            EditorGUILayout.PropertyField(sp); sp.Next(false);
            EditorGUIUtility.labelWidth = 24; // Refresh
            EditorGUILayout.PropertyField(sp, new GUIContent(FGUI_Resources.Tex_Refresh, "Auto refresh preview every change inside inspector window"), GUILayout.Width(44)); sp.Next(false);
            GUILayout.Space(6);


            EditorGUIUtility.labelWidth = 40; // Seed
            EditorGUILayout.PropertyField(sp, GUILayout.Width(100)); sp.Next(false);
            GUILayout.Space(6);
            EditorGUIUtility.labelWidth = 24;
            EditorGUILayout.PropertyField(sp, new GUIContent(FGUI_Resources.Tex_Random, "Random seed every generation"), GUILayout.Width(44));
            EditorGUIUtility.labelWidth = 0;
            GUILayout.Space(6);
            EditorGUILayout.EndHorizontal();

            DrawGUIBeforeDefaultInspector();

            GUILayout.Space(4f);
            DrawPropertiesExcluding(serializedObject, _iignore);

            DrawGUIBody();

            DrawGUIFooter();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawGUIBeforeDefaultInspector()
        {

        }

        protected virtual void DrawGUIHeader()
        {
            bool preE = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.PropertyField(sp_scr);
            GUI.enabled = preE;
        }

        protected virtual void DrawGeneratingButtons(bool drawPreview = true, bool drawClear = true)
        {
            EditorGUILayout.BeginHorizontal();

            if (drawPreview)
                if (GUILayout.Button("Preview"))
                {
                    bGet.ClearGenerated();
                    bGet.Prepare();
                    SceneView.RepaintAll();
                }

            if (GUILayout.Button("Generate"))
            {
                bGet.GenerateObjects();
            }

            if (bGet)
                if (bGet.Generated != null)
                    if (bGet.Generated.Count > 0)
                    {
                        if (GUILayout.Button("Clear", GUILayout.Width(50)))
                        {
                            bGet.ClearGenerated();
                        }
                    }

            EditorGUILayout.EndHorizontal();
        }

        protected virtual void DrawGUIBody()
        {

            /*
            DrawDefaultInspector();
            GUILayout.Space(4f);
			
			// if (Application.isPlaying) 
            if (GUILayout.Button("Do Something")) Get.tag = Get.tag;
			*/

        }

        protected virtual void DrawGUIFooter()
        {
            displayEvent = UnityEditor.EditorGUILayout.Foldout(displayEvent, "Event After Generating", true);
            if (displayEvent) UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("RunAfterGenerating"));
        }


        /// <summary> Drawing "Additional Settings" </summary>
        protected virtual void DrawAdditionalTab()
        {
            AdditionalTabHeader();
        }

        /// <summary> If you want to place additional tab in different placement inside inspector window, override "DrawAdditionalTab() {EMPTY}" and then use this method </summary>
        protected virtual void AdditionalTabHeader()
        {
            GUILayout.Space(5);

            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            GUILayout.Space(3);
            string ff = bGet._Editor_drawAdd ? "▼" : "▲";
            if (GUILayout.Button(ff + "  Additional Parameters  " + ff, FGUI_Resources.HeaderStyle)) bGet._Editor_drawAdd = !bGet._Editor_drawAdd;
            GUILayout.Space(3);

            if (bGet._Editor_drawAdd)
            {
                EditorGUI.BeginChangeCheck();
                DrawAdditionalSettingsContent();
                GUILayout.Space(5);
                EditorGUI.EndChangeCheck();
            }

            EditorGUILayout.EndVertical();

            GUILayout.Space(5);
        }


        protected virtual void DrawAdditionalSettingsContent()
        {
            DrawBasePostGenerationFetaures();
        }

        /// <summary>
        /// Drawing "Outline Fill" and "Rectangle Fill"
        /// </summary>
        protected void DrawBasePostGenerationFetaures()
        {
            SerializedProperty sp = sp_UseOutlineFill.Copy();

            GUILayout.Space(2);
            EditorGUILayout.PropertyField(sp); sp.Next(false);
            if (bGet.UseOutlineFill)
            {
                GUILayout.Space(3);
                FGenerators.DrawSomePropsOf(serializedObject.FindProperty("OutlineFill"), 3, true);
            }

            GUILayout.Space(5);
            sp.Next(false);
            EditorGUILayout.PropertyField(sp); sp.Next(false);
            if (bGet.UseRectangleFill)
            {
                GUILayout.Space(3);
                FGenerators.DrawSomePropsOf(serializedObject.FindProperty("RectangleFill"), 3, true);
            }
        }

    }
#endif

}