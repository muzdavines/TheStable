#if UNITY_EDITOR
using UnityEditor;
using FIMSpace.FEditor;
#endif
using UnityEngine;
using System;

namespace FIMSpace.Generating
{
    [AddComponentMenu("FImpossible Creations/PGG/Work In Progress/Flexible Generator", 10)]
    public class FlexibleGenerator : PGGFlexibleGeneratorBase
    {
        public Vector2Int TestGridSize = new Vector2Int(5, 4);


        [Header("Playmode Settings")]
        [Tooltip("If async generation will encounter error, non async computing will be triggered anyway. Computing FieldSetup rules to define where which objects should be created (not instantiating game objects)")]
        public bool AsyncComputing = false;

        [Tooltip("[Coroutine Instantiation] Set to 1 to instantiate all objects instantly. Max frame delay consumption time for objects instantiating process on the scene.")]
        [FPD_Width(200)] public float InstantiationMaxSecondsDelay = 0.01f;
        //[Tooltip("How many game objects are allowed to instantiate no matter how long time it will take.")]
        //[FPD_Width(200)] public int MinimumInstantiationsInFrame = 1;
        public float GeneratingProgress { get { return Cells.InstantiationProgress; } }
        public bool FinishedGenerating { get { return GeneratingProgress >= 1f; } }

        public override void Prepare()
        {
            base.Prepare();

            if (TestGridSize != Vector2Int.zero)
            {
                base.ClearGenerated(true);

                // Generation area
                var nGrid = new FGenGraph<FieldCell, FGenPoint>();
                nGrid.Generate(TestGridSize.x, TestGridSize.y, Vector3Int.zero);
                Cells.SetWithGrid(nGrid);
            }

            // Run rules
            Cells.Generate(false, AsyncComputing, this);
        }


        public virtual void PrepareWithoutGridChanges()
        {
            base.Prepare();
        }


        public override void GenerateObjects()
        {
            if (TestGridSize == Vector2Int.zero) // Custom code generation
            {
                base.Prepare();
                Cells.Generate(false, AsyncComputing, this);

                bool instant = InstantiationMaxSecondsDelay >= 1f;
#if UNITY_EDITOR
                if (Application.isPlaying == false) instant = true;
#endif

                if (instant)
                    Cells.InstantiateAllRemaining();
                else
                    Cells.InstantiateInCourutine(this, InstantiationMaxSecondsDelay, 1);
            }
            else // Test Generation
            {
                if (!Cells.WaitingToBeSpawned) Prepare(); // Re-Generate

                bool instant = InstantiationMaxSecondsDelay >= 1f;
#if UNITY_EDITOR
                if (Application.isPlaying == false) instant = true;
#endif

                if (instant)
                {
                    Cells.InstantiateAllRemaining();

                    // Post events
                    if (!Cells.WaitingToBeSpawned) base.GenerateObjects();
                }
                else
                {
                    Cells.InstantiateInCourutine(this, InstantiationMaxSecondsDelay, 1);
                }
            }
        }


        protected override void DrawGizmos()
        {
            if (FieldSetup == null) return;
            float scale = FieldSetup.GetCellUnitSize().x;
            Gizmos.DrawWireCube(TestGridSize.V2toV3() * scale * 0.5f - new Vector3(0.5f, 0f, 0.5f) * scale, TestGridSize.V2toV3() * scale);
        }

        public override void ClearGenerated(bool destroy = true)
        {
            if (TestGridSize == Vector2Int.zero)
            {
                if (destroy)
                {
                    InstantiatedInfo.Clear(true);
                    Cells.SetAllDirty();
                }
            }
            else
                base.ClearGenerated(destroy);
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            if (FieldSetup == null) return;
            if (DataSetup != null) DataSetup.RefreshReferences(this);

            Cells.CheckIfGridPrepared();
            var grid = Cells.Grid;

            // Preparation for drawing
            Color preColor = GUI.color;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Handles.matrix = Gizmos.matrix;
            Gizmos.color = new Color(1f, 1f, 1f, 0.1f);

            Vector3 cSize = FieldSetup.GetCellUnitSize();

            // Below just drawing debug grid, instructions, draw position - no events or paint triggers - it's done in OnSceneGUI in editor script component

            #region Drawing grid

            for (int i = 0; i < grid.AllApprovedCells.Count; i++)
            {
                var cell = grid.AllApprovedCells[i];
                Vector3 genPosition = cell.WorldPos(cSize);
                Gizmos.DrawWireCube(genPosition, new Vector3(cSize.x, cSize.y * 0.2f, cSize.z));
            }

            if (Preparation != null)
                if (Preparation.CellInstructions != null)
                {
                    Gizmos.color = new Color(1f, 0.7f, 0.7f, 0.5f);
                    for (int i = 0; i < Preparation.CellInstructions.Count; i++)
                    {
                        var cell = grid.GetCell(Preparation.CellInstructions[i].gridPosition);
                        Vector3 genPosition = cell.WorldPos(cSize);
                        Gizmos.DrawCube(genPosition, new Vector3(cSize.x, cSize.y * 0.2f, cSize.z));
                    }
                }

            #endregion


            // Restore Gizmos
            Gizmos.color = preColor;
            Gizmos.matrix = Matrix4x4.identity;
            Handles.matrix = Matrix4x4.identity;

        }

#endif

    }

    #region Editor Related

#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(FlexibleGenerator))]
    public class FlexibleGeneratorEditor : PGGFlexibleGeneratorBaseEditor
    {
        public FlexibleGenerator Get { get { if (_get == null) _get = (FlexibleGenerator)target; return _get; } }
        private FlexibleGenerator _get;

        protected override void OnEnable()
        {
            _editor_drawAutoRefresh = false;
            Get.AutoRefresh = false;
            base.OnEnable();
        }

        protected override void DrawGUIBody()
        {
            base.DrawGUIBody();

            if (Get.AsyncComputing)
            {
                EditorGUILayout.HelpBox("Async generating is working only in playmode", MessageType.None);
                EditorGUILayout.HelpBox("WARNING: Async operations doesn't allow use of Unity references in code, use one of such will cause error and will generate level not async.\nAs for now, some rules nodes (especially material/prefab replacement using variables nodes), will cause async error, it will be walkarounded and available in next updates of PGG!", MessageType.Warning);
            }
        }

        protected override void DrawGeneratingButtons(bool drawPreview = true, bool drawClear = true)
        {
            if (Get.InstantiationMaxSecondsDelay > 1f) Get.InstantiationMaxSecondsDelay = 1f;
            if (Get.InstantiationMaxSecondsDelay <= 0f) Get.InstantiationMaxSecondsDelay = 0.01f;

            GUILayout.Space(3);
            EditorGUILayout.BeginHorizontal();

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

    }
#endif

    #endregion

}