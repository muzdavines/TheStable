using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using FIMSpace.Generating.Checker;
using System;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

namespace FIMSpace.Generating
{
    public abstract class PGGFlexibleGeneratorBase : PGGGeneratorRoot
    {
        public bool GenerateOnStart = true;
        public bool AutoRefresh = true;
        public int Seed = 0;
        public bool RandomSeed = true;

        [HideInInspector] public PGGDataHolder Data;
        [HideInInspector] public bool UseDataHolder = true;
        /// <summary> Used if 'UseDataHolder' is set to false </summary>
        public FlexibleGeneratorSetup InternalSetup;

        [NonSerialized] public bool CodedUsage = false;

        #region Data Container Setup Auto Handling

        public FlexibleGeneratorSetup DataSetup
        {
            get
            {
                if (UseDataHolder)
                {
                    if (Data)
                    {
                        Data.Owner = this;
                        return Data.Setup;
                    }
                    else
                    {
                        PGGDataHolder holder = GetComponentInChildren<PGGDataHolder>();
                        if (holder) if (holder.Owner != gameObject) holder = null;

                        if (!holder)
                        {
                            GameObject holderObject = new GameObject("PGG Data Holder");
                            holderObject.transform.SetParent(transform);
                            holderObject.transform.ResetCoords();

                            holder = holderObject.AddComponent<PGGDataHolder>();
                        }

                        Data = holder;
                        Data.Refresh(this);

                        return Data.Setup;
                    }
                }
                else
                    return InternalSetup;
            }
        }

        #endregion

        public FieldSetup FieldSetup { get { return DataSetup.FieldPreset; } }
        public CellsController Cells { get { return DataSetup.CellsController; } }
        public InstantiatedFieldInfo InstantiatedInfo { get { return DataSetup.InstantiatedInfo; } }
        public GeneratingPreparation Preparation { get { return DataSetup.Preparation; } }

        [HideInInspector] public FieldSetupComposition Composition;

        [HideInInspector] public UnityEvent RunAfterGenerating;

        /// <summary> Can be used for some post-generation processes like rectangle fill (must be implemented-supported by the particular generator) </summary>
        [System.NonSerialized] public List<CheckerField> GeneratorCheckers = new List<CheckerField>();
        [HideInInspector] public bool _Editor_drawAdd = false;


        #region Root Support

        public override FieldSetup PGG_Setup
        {
            get
            {
                if (Composition != null) if (Composition.UseComposition)
                    {
                        return Composition.GetSetup;
                    }

                return FieldSetup;
            }
        }

        public override FGenGraph<FieldCell, FGenPoint> PGG_Grid { get { return Cells.Grid; } }

        #endregion



        protected virtual void Start()
        {
            if (CodedUsage) return;

            CheckIfInitialized();
            if (DataSetup != null) DataSetup.RefreshReferences(this);
            Cells.CheckIfGridPrepared();

            if (GenerateOnStart) GenerateObjects();
        }


        #region Run Generation Methods

        public void CheckIfInitialized()
        {
            if (UseDataHolder == false) if (InternalSetup == null) InternalSetup = new FlexibleGeneratorSetup();
            DataSetup.Initialize(this, Composition);
        }

        public virtual void GenerateObjects()
        {
            //Prepare();

            //// Run rules
            //Cells.Generate(true);

            // Post events
            RunPostGeneration();
            if (RunAfterGenerating != null) RunAfterGenerating.Invoke();
        }


        public virtual void Prepare()
        {
            CheckIfInitialized();
            if (DataSetup != null) DataSetup.RefreshReferences(this);
            Cells.CheckIfPrepared();
            Cells.CheckIfGridPrepared();

            RandomizeSeed();
            FGenerators.SetSeed(Seed);
            GeneratorCheckers.Clear();
        }


        public virtual void RandomizeSeed()
        {
            if (RandomSeed) Seed = UnityEngine.Random.Range(-999999, 999999);
        }


        public virtual void ClearGenerated(bool destroy = true)
        {
            if (destroy) InstantiatedInfo.Clear(true);
            Cells.ClearAll(); //
        }

        #endregion


        #region Getting Generated Objects Methods


        public List<GameObject> GetAllGeneratedObjects(bool withContainers)
        {
            List<GameObject> allGen = new List<GameObject>();

            for (int i = 0; i < DataSetup.CellsController.InstantiatedInfo.Instantiated.Count; i++)
            {
                PGGUtils.TransferFromListToList(DataSetup.CellsController.InstantiatedInfo.Instantiated, allGen);
            }

            if (withContainers)
            {
                for (int i = 0; i < DataSetup.CellsController.InstantiatedInfo.InstantiationContainers.Count; i++)
                {
                    if (DataSetup.CellsController.InstantiatedInfo.InstantiationContainers[i] == null) continue;
                    if (DataSetup.CellsController.InstantiatedInfo.InstantiationContainers[i].Transform == null) continue;

                    allGen.Add(DataSetup.CellsController.InstantiatedInfo.InstantiationContainers[i].Transform.gameObject);
                }

            }

            return allGen;
        }


        #endregion


        #region Post Generation Features Implementations

        protected virtual void RunPostGeneration()
        {

            #region Supporting additional post-generation features

            //if (UseOutlineFill)
            //{
            //    var fills = OutlineFill.RunOnGenerator(this);
            //    for (int f = 0; f < fills.Count; f++)
            //    {
            //        Generated.Add(fills[f]);

            //        if (fills[f].OptionalCheckerFieldsData.Count > 0)
            //            GeneratorCheckers.Add(fills[f].OptionalCheckerFieldsData[0]);
            //    }
            //}

            //if (UseRectangleFill)
            //{
            //    var fill = RectangleFill.RunOnGenerator(this);
            //    if (fill != null) Generated.Add(fill);
            //}

            #endregion

        }

        #endregion


        #region Gizmos


        #region Unity Editor


#if UNITY_EDITOR
        protected Color preColor;
        private void OnDrawGizmosSelected()
        {
            CheckIfInitialized();

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
            //if (UseRectangleFill)
            //{
            //    Gizmos.color = new Color(0.1f, 0.9f, 0.15f, 0.3f);

            //    Vector3 off = new Vector3(cellSize.x * 1.5f, 0f, cellSize.z * 1.5f);
            //    if (RectangleFill.Size.x % 2 != 0) off.x += 1f;
            //    if (RectangleFill.Size.y % 2 != 0) off.z += 1f;
            //    Gizmos.DrawCube(Vector3.Scale(cellSize, RectangleFill.Center.V2toV3()) + off, Vector3.Scale(cellSize, RectangleFill.Size.V2toV3(0.1f)));
            //}
        }


        protected virtual void DrawGizmos()
        {

        }

        #endregion


        #region Utilities

        /// <summary>
        /// Cleaning scene out of additional generated objects
        /// </summary>
        protected virtual void OnDestroy()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false) if (Data) if (Data.Owner == this) FGenerators.DestroyObject(Data.gameObject);
#endif
        }

        #endregion

    }


#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PGGFlexibleGeneratorBase))]
    public abstract class PGGFlexibleGeneratorBaseEditor : UnityEditor.Editor
    {
        public PGGFlexibleGeneratorBase bGet { get { if (_bget == null) _bget = (PGGFlexibleGeneratorBase)target; return _bget; } }
        private PGGFlexibleGeneratorBase _bget;

        protected static List<string> _ignore;
        private static string[] _iignore;

        bool displayEvent = false;
        SerializedProperty sp_props;
        SerializedProperty sp_scr;
        SerializedProperty sp_internalSetup;
        SerializedProperty sp_holderSetup;
        SerializedProperty sp_tgtsetup;
        protected SerializedProperty sp_UseOutlineFill;
        protected Color preGuiCol;
        protected Color preGuiBGCol;

        /// <summary> Draw "generate on start" etc. </summary>
        protected bool _editor_drawHeaderPanel = true;
        protected bool _editor_drawGenerateOnStart = true;
        protected bool _editor_drawAutoRefresh = true;
        protected bool _editor_drawRandomNumber = true;


        public override bool RequiresConstantRepaint()
        {
            if (bGet.Cells != null) return bGet.Cells.IsGenerating;
            return false;
        }


        protected virtual void OnEnable()
        {
            sp_props = serializedObject.FindProperty("GenerateOnStart");
            sp_scr = serializedObject.FindProperty("m_Script");
            sp_UseOutlineFill = serializedObject.FindProperty("UseOutlineFill");
            sp_internalSetup = serializedObject.FindProperty("InternalSetup");

            RefreshHolder();

            _iignore = null;
            _ignore = new List<string>() { "m_Script", "InternalSetup", "GenerateOnStart", "AutoRefresh", "RandomSeed", "Seed" };
        }

        void RefreshHolder()
        {
            var setup = bGet.DataSetup; // Refreshing setup by get;
            bool reset = false;

            if (bGet.UseDataHolder) if (sp_holderSetup == null) reset = true;

            if (reset)
            {
                SerializedObject so = new SerializedObject(bGet.Data);
                sp_holderSetup = so.FindProperty("Setup");
            }

            if (bGet.UseDataHolder) sp_tgtsetup = sp_holderSetup;
            else sp_tgtsetup = sp_internalSetup;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This component is using work in progress generating approach!\nIf something will not work as required, please go back to main components until this approach is completed in the next updates of PGG.", MessageType.Info);

            RefreshHolder();

            preGuiCol = GUI.color;
            preGuiBGCol = GUI.backgroundColor;

            bGet.CheckIfInitialized();

            if (_iignore == null) _iignore = _ignore.ToArray();

            DrawGUIHeader();
            serializedObject.Update();

            if (_editor_drawHeaderPanel)
                if (_editor_drawAutoRefresh || _editor_drawGenerateOnStart || _editor_drawRandomNumber)
                {
                    #region Drawing Header Panel

                    GUILayout.Space(4);
                    SerializedProperty sp = sp_props.Copy();
                    EditorGUILayout.BeginHorizontal(FGUI_Resources.BGInBoxStyle);
                    EditorGUIUtility.labelWidth = 120;
                    if (_editor_drawGenerateOnStart) EditorGUILayout.PropertyField(sp); sp.Next(false);
                    EditorGUIUtility.labelWidth = 24; // Refresh
                    if (_editor_drawAutoRefresh) EditorGUILayout.PropertyField(sp, new GUIContent(FGUI_Resources.Tex_Refresh, "Auto refresh preview every change inside inspector window"), GUILayout.Width(44)); sp.Next(false);
                    GUILayout.Space(6);

                    EditorGUIUtility.labelWidth = 40; // Seed
                    if (_editor_drawRandomNumber) EditorGUILayout.PropertyField(sp, GUILayout.Width(100)); sp.Next(false);
                    GUILayout.Space(6);
                    EditorGUIUtility.labelWidth = 24;
                    if (_editor_drawRandomNumber) EditorGUILayout.PropertyField(sp, new GUIContent(FGUI_Resources.Tex_Random, "Random seed every generation"), GUILayout.Width(44));
                    EditorGUIUtility.labelWidth = 0;
                    GUILayout.Space(6);
                    EditorGUILayout.EndHorizontal();

                    #endregion
                }

            DrawGUIBeforeDefaultInspector();

            GUILayout.Space(4f);
            //EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            SerializedProperty spS = sp_tgtsetup.FindPropertyRelative("FieldPreset");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(spS, true);
            GUILayout.Space(4f);

            if (EditorGUI.EndChangeCheck())
            {
                spS.serializedObject.ApplyModifiedProperties();
                var _s = bGet.DataSetup;
                _s.Initialize(bGet, bGet.Composition);
            }

            DrawPropertiesExcluding(serializedObject, _iignore);
            GUILayout.Space(4f);

            if (spS.objectReferenceValue == null)
            {
                GUILayout.Space(3);
                EditorGUILayout.HelpBox("Field Setup must be assigned for component to work!", MessageType.Warning);
            }
            else
            {
                DrawGUIBody();

                spS.Next(false);
                if (bGet.DataSetup.Preparation != null) GeneratingPreparation.DrawPreparation(bGet.DataSetup.Preparation, spS);

                DrawAdditionalTab();

                DrawGUIFooter();
            }

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
            GUILayout.Space(3);
            EditorGUILayout.BeginHorizontal();

            bool can = false;
            if (bGet.DataSetup.FieldPreset != null || bGet.Composition.IsSettedUp) can = true;

            GUI.enabled = can;

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

            GUI.enabled = true;

            if (bGet)
                if (bGet.DataSetup.CellsController != null)
                    if (bGet.DataSetup.CellsController.InstantiatedInfo != null)
                        if (bGet.DataSetup.CellsController.InstantiatedInfo.Instantiated != null)
                            if (bGet.DataSetup.CellsController.InstantiatedInfo.Instantiated.Count > 0)
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
            DrawGeneratingButtons(false, true);
            GUILayout.Space(3);
            if (bGet.FieldSetup) DrawGeneratingDebug(bGet.Cells);
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

            FGUI_Inspector.FoldHeaderStart(ref bGet._Editor_drawAdd, "Additionals", FGUI_Resources.BGInBoxStyle);

            if (bGet._Editor_drawAdd)
            {
                EditorGUI.BeginChangeCheck();
                DrawAdditionalSettingsContent();
                GUILayout.Space(5);
                EditorGUI.EndChangeCheck();
            }

            GUILayout.EndVertical();

            GUILayout.Space(5);
        }


        protected virtual void DrawAdditionalSettingsContent()
        {
            DrawBasePostGenerationFetaures();

            GUILayout.Space(3);

            EditorGUI.indentLevel++;
            displayEvent = UnityEditor.EditorGUILayout.Foldout(displayEvent, "Event After Generating", true);
            if (displayEvent) UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("RunAfterGenerating"));
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Drawing "Outline Fill" and "Rectangle Fill"
        /// </summary>
        protected void DrawBasePostGenerationFetaures()
        {
            //SerializedProperty sp = sp_UseOutlineFill.Copy();

            //GUILayout.Space(2);
            //EditorGUILayout.PropertyField(sp); sp.Next(false);
            //if (bGet.UseOutlineFill)
            //{
            //    GUILayout.Space(3);
            //    FGenerators.DrawSomePropsOf(serializedObject.FindProperty("OutlineFill"), 3, true);
            //}

            //GUILayout.Space(5);
            //sp.Next(false);
            //EditorGUILayout.PropertyField(sp); sp.Next(false);
            //if (bGet.UseRectangleFill)
            //{
            //    GUILayout.Space(3);
            //    FGenerators.DrawSomePropsOf(serializedObject.FindProperty("RectangleFill"), 3, true);
            //}
        }


        protected void DrawGeneratingDebug(CellsController gen)
        {
            if (gen == null) return;

            if (gen.IsGenerating)
            {
                Color preC = GUI.color;
                GUI.color = Color.green;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Generating In Progress", UnityEditor.EditorStyles.centeredGreyMiniLabel);
                if (gen.AsyncIsRunning) EditorGUILayout.LabelField("Computing rules on grid Async...", UnityEditor.EditorStyles.boldLabel);
                if (gen.IsInstantiationCoroutineRunning)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Instantiating " + gen.InstantiatedInfo.Instantiated.Count + " / " + gen.LatestToSpawnCount, UnityEditor.EditorStyles.boldLabel);
                    GUILayout.Space(8);
                    EditorGUILayout.LabelField(Mathf.Round(gen.InstantiationProgress * 100f) + "%", UnityEditor.EditorStyles.boldLabel, GUILayout.Width(70));
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                GUI.color = preC;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Cells Count: " + gen.CellsCount);
            EditorGUILayout.LabelField("Waiting to Generate: " + gen.ToInstantiate.Count);
            EditorGUILayout.LabelField("Waiting to Update: " + gen.DirtyCells.Count);
            EditorGUILayout.LabelField("Waiting Datas to Spawn: " + gen.ToSpawnCount);
            EditorGUILayout.EndVertical();
        }

    }
#endif

}