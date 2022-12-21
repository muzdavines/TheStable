#if UNITY_EDITOR
using UnityEditor;
using FIMSpace.FEditor;
#endif

using System.Collections.Generic;
using UnityEngine;
using System;

namespace FIMSpace.Generating
{
    [AddComponentMenu("FImpossible Creations/PGG/Grid Painter", 0)]
    public class GridPainter : PGGGeneratorBase
    {
        public FGenGraph<FieldCell, FGenPoint> grid = new FGenGraph<FieldCell, FGenPoint>();

        public Transform PGG_Transform { get { return transform; } }


        #region Root Support

        public override FieldSetup PGG_Setup
        {
            get
            {
                if (Composition != null) if (Composition.UseComposition)
                    {
                        return Composition.GetSetup;
                    }
                return FieldPreset;
            }
        }

        public override FGenGraph<FieldCell, FGenPoint> PGG_Grid
        {
            get
            {
                if (grid == null || grid.AllApprovedCells.Count == 0) LoadCells();
                return grid;
            }
        }

        #endregion


        [HideInInspector][SerializeField] private List<PainterCell> cellsMemory = new List<PainterCell>();
        //[HideInInspector] [SerializeField] public List<PainterCell> InternalCellsMemory { get { return cellsMemory; } }
        public enum EPaintMode { Cells, Instructions }
        [HideInInspector][SerializeField] public int PaintingID = -1;

        #region Add Header Options

#if UNITY_EDITOR

        [MenuItem("CONTEXT/GridPainter/Clear whole Grid and Instructions")]
        private static void ClearWholeGrid(MenuCommand menuCommand)
        {
            GridPainter painter = menuCommand.context as GridPainter;
            if (painter)
            {
                painter.ClearGenerated();
                painter.grid = new FGenGraph<FieldCell, FGenPoint>();
                painter.ClearSavedCells();
                painter.CellsInstructions.Clear();
            }
        }

        //[MenuItem("CONTEXT/GridPainter/Move All Cells by One to Left")]
        //private static void MoveAllCells(MenuCommand menuCommand)
        //{
        //    GridPainter painter = menuCommand.context as GridPainter;
        //    if (painter)
        //    {
        //        List<GridPainter> sel = new List<GridPainter>();
        //        for (int i = 0; i < Selection.gameObjects.Length; i++)
        //        {
        //            GridPainter p = Selection.gameObjects[i].GetComponent<GridPainter>();
        //            if (p)
        //            {
        //                for (int a = 0; a < p.Grid.AllCells.Count; a++)
        //                    p.grid.AllCells[a].Pos -= new Vector3Int(1, 0, 0);

        //                for (int a = 0; a < p.CellsInstructions.Count; a++)
        //                    p.CellsInstructions[a].pos -= new Vector3Int(1, 0, 0);

        //                p.SaveCells();
        //                p.LoadCells();
        //            }
        //        }
        //    }
        //}

#endif

        #endregion

        public List<PainterCell> GetAllPainterCells { get { return cellsMemory; } }

        [Space(3)]
        public FieldSetup FieldPreset;

        private FieldSetup generatingSetup;
        [Tooltip("Must have same cells size! Useful for generating simple house with exterior then interior")]
        public List<FieldSetup> AdditionalFieldSetups;
        public List<InjectionSetup> Injections;

        internal SpawnInstructionGuide Selected;
        [Tooltip("Instructions painted with scene view GUI, you can access their parameters here and modify manually")]
        public List<SpawnInstructionGuide> CellsInstructions = new List<SpawnInstructionGuide>();
        [Tooltip("If you want put cell datas in exact position of other grid painters\n(Cell commands INDEXES will be used! If on other painter is placed first command in list then this field setup will use first command in list)\n\n(WARNING, grid painters must have same cell size FieldSetups and world position of grid painter object must be in the same position as this grid painter)")]
        public List<GridPainter> AcquireCellDataFrom = new List<GridPainter>();

        [PGG_SingleLineTwoProperties("AllowOverlapInstructions", 0, 0, 10, 0, 3)]
        [Tooltip("If adding instructions on certain positions should automatically add cell to grid in same position. Useful for inject painting.")]
        public bool AddCellsOnInstructions = false;
        [Tooltip("If you want to add two instructions into single cell with painting")]
        [HideInInspector] public bool AllowOverlapInstructions = false;

        public enum EDebug { None, DrawGrid, DrawGridDetails }
        [PGG_SingleLineTwoProperties("Transprent", 60, 70, 10, -45, 3)]
        public EDebug Debug = EDebug.DrawGrid;
        [HideInInspector][Tooltip("Just making cell data rectanles transparent on gizmos")] public bool Transprent = true;


        [HideInInspector] public List<ModificatorsPack> ignoredPacksForGenerating = new List<ModificatorsPack>();
        private List<bool> _ignoredPacksToggleBackup = new List<bool>();

        [HideInInspector] public List<FieldModification> ignoredForGenerating = new List<FieldModification>();
        [HideInInspector] public List<FieldVariable> SwitchVariables = new List<FieldVariable>();
        [HideInInspector] public List<FieldVariable> SwitchPackVariables = new List<FieldVariable>();
        //[HideInInspector] public FieldSetupComposition Composition = null;


        #region Editor Related

        [HideInInspector] public bool _EditorGUI_DrawExtra = false;
        [HideInInspector] public bool _EditorGUI_DrawIgnoring = false;
        [HideInInspector] public bool _EditorGUI_DrawVars = false;
        [HideInInspector] public bool _EditorGUI_DrawPackVars = false;
        [HideInInspector] public bool _ModifyVars = false;
        [HideInInspector] public bool _ModifyPackVars = false;
        [HideInInspector] public int _EditorGUI_SelectedId = -1;

        [HideInInspector] public bool _Editor_Paint = false;
        [HideInInspector] public int _Editor_RadiusY = 1;
        [HideInInspector] public int _Editor_PaintRadius = 1;
        [HideInInspector] public int _Editor_YLevel = 0;
        [HideInInspector] public int _Editor_CommandsPage = 0;

        public enum EPaintSpaceMode { XZ, XY_2D }
        [HideInInspector] public EPaintSpaceMode _Editor_PaintSpaceMode = EPaintSpaceMode.XZ;

        [HideInInspector] public string _Editor_Instruction = "0";
        [HideInInspector] public bool _Editor_RotOrMovTool = false;
        [HideInInspector] public bool _Editor_ContinousMode = false;

        #endregion

        [HideInInspector] public FieldSetupComposition Composition;

        public bool Painting
        {
            get { return _Editor_Paint; }
            set { _Editor_Paint = value; }
        }

        public FGenGraph<FieldCell, FGenPoint> Grid { get { return grid; } }
        public int LoadCount { get { return cellsMemory.Count; } }

        private void Reset()
        {
            AutoRefresh = false;

            Composition = new FieldSetupComposition();
            Composition.OverrideEnabled = false;
            Composition.Prepared = false;
        }


        bool _GenFSetupPreGathered = false;

        /// <summary> Generating duplicate of field setup for overriding variables etc. </summary>
        public FieldSetup GetTargetGeneratingSetup()
        {
            if (_GenFSetupPreGathered) return generatingSetup;

            generatingSetup = FieldPreset;

            if (Composition != null)
            {
                if (Composition.OverrideEnabled && Composition.Prepared)
                {
                    generatingSetup = Composition.GetOverridedSetup();
                }
                else
                {
                    if (FieldPreset)
                        generatingSetup = FieldPreset.Copy();
                }
            }
            else
            {
                generatingSetup = FieldPreset.Copy();
            }

            _GenFSetupPreGathered = true;

            return generatingSetup;
        }


        public override void GenerateObjects()
        {
            RefreshFieldVariables();
            LoadCells();
            Prepare();

            if (Generated == null) Generated = new List<InstantiatedFieldInfo>();
            if (RandomSeed) Seed = FGenerators.GetRandom(-99999, 99999);
            ClearGenerated();

            if (!_GenFSetupPreGathered) GetTargetGeneratingSetup();
            if (generatingSetup == null) return;

            FGenerators.SetSeed(Seed);
            List<SpawnInstruction> guides = new List<SpawnInstruction>();


            for (int i = 0; i < AcquireCellDataFrom.Count; i++)
                for (int a = 0; a < AcquireCellDataFrom[i].CellsInstructions.Count; a++)
                    CellsInstructions.Add(AcquireCellDataFrom[i].CellsInstructions[a]);


            for (int i = 0; i < CellsInstructions.Count; i++)
            {
                SpawnInstructionGuide instr = CellsInstructions[i];

                if (instr == null) continue;

                SpawnInstruction guide = new SpawnInstruction();
                Vector3 dir;

                if (instr.WorldRot)
                    dir = (Quaternion.Inverse(transform.rotation) * instr.rot) * Vector3.forward;
                else
                    dir = instr.rot * Vector3.forward;

                guide.desiredDirection = new Vector3Int(Mathf.RoundToInt(dir.x), 0, Mathf.RoundToInt(dir.z));
                guide.gridPosition = new Vector3Int(instr.pos.x, instr.pos.y, instr.pos.z);
                guide.useDirection = CellsInstructions[i].UseDirection;

                if (instr.CustomDefinition == null)
                {
                    if (instr.Id < generatingSetup.CellsInstructions.Count)
                        guide.definition = generatingSetup.CellsInstructions[instr.Id];
                }
                else
                {
                    if (instr.CustomDefinition.InstructionType != InstructionDefinition.EInstruction.None)
                        guide.definition = instr.CustomDefinition;
                    else
                        if (instr.Id < generatingSetup.CellsInstructions.Count)
                        guide.definition = generatingSetup.CellsInstructions[instr.Id];
                }

                guides.Add(guide);
            }

            Generated.Clear();

            for (int i = 0; i < ignoredForGenerating.Count; i++) { generatingSetup.Ignores.Add(ignoredForGenerating[i]); }

            _ignoredPacksToggleBackup.Clear();
            if (ignoredPacksForGenerating == null) ignoredPacksForGenerating = new List<ModificatorsPack>();
            PGGUtils.CheckForNulls(ignoredPacksForGenerating);
            for (int i = 0; i < ignoredPacksForGenerating.Count; i++) { _ignoredPacksToggleBackup.Add(ignoredPacksForGenerating[i].DisableWholePackage); ignoredPacksForGenerating[i].DisableWholePackage = true; }

            if (Injections != null) if (Injections.Count > 0) generatingSetup.SetTemporaryInjections(Injections);

            FieldVariablesBackup();
            FieldVariablesSetCustom();

            try
            {
                Generated.Add(IGeneration.GenerateFieldObjects(generatingSetup, grid, transform, true, guides, null, true));
                if (Injections != null) if (Injections.Count > 0) generatingSetup.ClearTemporaryInjections();
                FieldVariablesRestore();
            }
            catch (System.Exception exc)
            {
                UnityEngine.Debug.LogError("[PGG] Error when generating with GridPainter! Check the Log down below.");
                UnityEngine.Debug.LogException(exc);
                if (Injections != null) if (Injections.Count > 0) generatingSetup.ClearTemporaryInjections();
                FieldVariablesRestore();
            }

            if (AdditionalFieldSetups != null)
                for (int i = 0; i < AdditionalFieldSetups.Count; i++)
                {
                    if (AdditionalFieldSetups[i] == null) continue;
                    if (Injections != null) if (Injections.Count > 0) AdditionalFieldSetups[i].SetTemporaryInjections(Injections);
                    Generated.Add(IGeneration.GenerateFieldObjects(AdditionalFieldSetups[i], grid, transform, true, null, null, true));
                    //for (int g = 0; g < addGenerated.Count; g++) Generated.Add(addGenerated[g]);
                    if (Injections != null) if (Injections.Count > 0) AdditionalFieldSetups[i].ClearTemporaryInjections();
                }

            for (int i = 0; i < ignoredForGenerating.Count; i++) generatingSetup.Ignores.Remove(ignoredForGenerating[i]);
            for (int i = 0; i < ignoredPacksForGenerating.Count; i++) ignoredPacksForGenerating[i].DisableWholePackage = _ignoredPacksToggleBackup[i];


            for (int i = 0; i < AcquireCellDataFrom.Count; i++)
                for (int a = 0; a < AcquireCellDataFrom[i].CellsInstructions.Count; a++)
                    CellsInstructions.Remove(AcquireCellDataFrom[i].CellsInstructions[a]);

            base.GenerateObjects();

            //GeneratingPreparation prep = new GeneratingPreparation();
            //GenerationScheme scheme = new GenerationScheme(prep, generatingSetup, grid);
            //GenerateAsyncThread thr = new GenerateAsyncThread(scheme);
            //thr.Start();

            _GenFSetupPreGathered = false;
        }

        public void ReGenerate()
        {
            ClearGenerated();
            GenerateObjects();
        }


        public void OnChange()
        {
            if (AutoRefresh)
            {
                ClearSavedCells();
                SaveCells();
                LoadCells();
                GenerateObjects();
            }
            else
            {
                ClearSavedCells();
                SaveCells();
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void CheckMemoryForDuplicates()
        {
            if (cellsMemory == null) return;

            LoadCells();

            if (grid == null) return;

            List<Vector3Int> positions = new List<Vector3Int>();
            for (int g = 0; g < grid.AllApprovedCells.Count; g++) positions.Add(grid.AllApprovedCells[g].Pos);

            List<int> firsts = new List<int>();
            List<Vector3Int> saved = new List<Vector3Int>();

            // Check for cell memory duplicates
            for (int c = 0; c < cellsMemory.Count; c++)
            {
                Vector3Int pos = cellsMemory[c].pos;
                if (saved.Contains(pos) == false)
                    if (positions.Contains(pos))
                    {
                        firsts.Add(c);
                        saved.Add(pos);
                    }
            }

            //int removed = 0;
            for (int c = cellsMemory.Count - 1; c >= 0; c--)
            {
                if (!firsts.Contains(c)) { cellsMemory.RemoveAt(c); /*removed++;*/ }
            }

            //UnityEngine.Debug.Log("removed " + removed);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }


#if UNITY_EDITOR

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (FieldPreset == null) return;
            if (grid.AllCells.Count == 0) grid.AddCell(0, 0, 0);

            bool is2D = _Editor_PaintSpaceMode == EPaintSpaceMode.XY_2D;
            //if (is2D) _Editor_PaintRadius = 1;
            //if (is2D) _Editor_RadiusY = 1;

            Color preColor = GUI.color;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Handles.matrix = Gizmos.matrix;
            Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
            Vector3 cSize = FieldPreset.GetCellUnitSize();

            if (Debug != EDebug.None)
                for (int i = 0; i < grid.AllApprovedCells.Count; i++)
                {
                    var cell = grid.AllApprovedCells[i];
                    Vector3 genPosition = cell.WorldPos(cSize);

                    if ((!is2D && cell.Pos.y == _Editor_YLevel) || (is2D && cell.Pos.z == _Editor_YLevel))
                    {
                        if (is2D)
                            Gizmos.DrawWireCube(genPosition + new Vector3(0, cSize.y * 0.5f, 0), new Vector3(cSize.x, cSize.y, cSize.z * 0.2f));
                        else
                        {
                            Gizmos.DrawWireCube(genPosition, new Vector3(cSize.x, cSize.y * 0.2f, cSize.z));
                            if (cell.IsGhostCell) Gizmos.DrawCube(genPosition, new Vector3(cSize.x * 0.8f, cSize.y * 0.2f, cSize.z * 0.8f));
                        }

                        if (Debug == EDebug.DrawGridDetails)
                        {
                            Handles.Label(genPosition, new GUIContent(cell.Pos.x + ", " + cell.Pos.z), EditorStyles.centeredGreyMiniLabel);

                            if (FGenerators.CheckIfExist_NOTNULL(cell.ParentCell))
                            {
                                Handles.color = Color.cyan;
                                Handles.DrawLine(genPosition, genPosition + Vector3.up * cSize.y * 0.25f);
                                Handles.DrawLine(genPosition, cell.ParentCell.WorldPos(cSize));
                                Handles.color = Color.white;
                            }

                            for (int s = 0; s < cell.GetJustCellSpawnCount(); s++)
                            {
                                var spwn = cell.GetSpawnsJustInsideCell()[s];

                                if (spwn.Prefab == null) // Drawing empty position
                                {
                                    Color preC = Gizmos.color;
                                    Gizmos.color = new Color(0.1f, 1f, 0.1f, 0.9f);
                                    Gizmos.DrawWireSphere(spwn.GetWorldPositionWithFullOffset(FieldPreset, true), FieldPreset.GetCellUnitSize().x * 0.2f);
                                    Gizmos.color = preC;
                                }

                                for (int r = 0; r < spwn.Spawner.Rules.Count; r++)
                                {
                                    var rule = cell.GetSpawnsJustInsideCell()[s].Spawner.Rules[r];
                                    if (rule._EditorDebug)
                                    {
                                        rule.OnDrawDebugGizmos(FieldPreset, spwn, cell, grid);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Gizmos.color = new Color(1f, 1f, 1f, 0.05f);

                        if (is2D)
                            Gizmos.DrawWireCube(genPosition + new Vector3(0, cSize.y * 0.5f, 0), new Vector3(cSize.x * 0.9f, cSize.y * 0.9f, cSize.z * 0.05f));
                        else
                            Gizmos.DrawWireCube(genPosition, new Vector3(cSize.x * 0.9f, cSize.y * 0.05f, cSize.z * 0.9f));

                        Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
                    }
                }

            for (int i = 0; i < CellsInstructions.Count; i++)
            {
                var instr = CellsInstructions[i];
                Color nCol = Color.HSVToRGB((float)instr.Id * 0.1f % 1f, 0.7f, 0.8f);
                nCol.a = Transprent ? 0.185f : 1f;
                Gizmos.color = nCol;
                Vector3 pos = FieldPreset.TransformCellPosition((Vector3)instr.pos);
                Gizmos.DrawCube(pos, new Vector3(cSize.x, cSize.y * 0.2f, cSize.z));

                if (instr.UseDirection)
                {
                    Gizmos.color = new Color(nCol.r, nCol.g, nCol.b, 0.7f);
                    Vector3 dir;

                    if (instr.WorldRot)
                        dir = (Quaternion.Inverse(transform.rotation) * instr.rot) * Vector3.forward;
                    else
                        dir = instr.rot * Vector3.forward;

                    Vector3 dirP = new Vector3(Mathf.Abs(dir.x), Mathf.Abs(dir.y), Mathf.Abs(dir.z));
                    Gizmos.DrawCube(pos + dir * cSize.z * 0.25f + Vector3.up * cSize.y * 0.01f, (dirP * cSize.z * 0.8f + new Vector3(cSize.x, cSize.y, cSize.z) * 0.2f) * 0.5f);
                }
            }
            Gizmos.color = new Color(1f, 1f, 1f, 0.1f);

            if (Painting)
                if (FieldPreset != null)
                {
                    if (SceneView.currentDrawingSceneView != null)
                        if (SceneView.currentDrawingSceneView.camera != null)
                        {
                            if (PaintingID == -1) Gizmos.color = Color.white;
                            else if (PaintingID == -2) Gizmos.color = Color.black;
                            else Gizmos.color = Color.yellow;

                            if (!is2D) // 3D
                            {
                                Vector3 mousePos = transform.InverseTransformPoint(GridVisualize.GetMouseWorldPosition(transform.up, Event.current, SceneView.currentDrawingSceneView.camera, _Editor_YLevel, cSize.y, transform.position.y * Vector3.up));
                                Gizmos.DrawLine(mousePos, mousePos + Vector3.up * 1);
                                Vector3 gridPos = FVectorMethods.FlattenVector(mousePos, cSize);

                                Gizmos.DrawWireCube(gridPos, new Vector3(cSize.x, cSize.y * 0.2f, cSize.z));
                                Gizmos.DrawWireCube(gridPos, new Vector3(cSize.x, cSize.y * 0.2f, cSize.z));

                                if (_Editor_PaintRadius > 1 || _Editor_RadiusY > 1)
                                {
                                    int rad = Mathf.RoundToInt((float)_Editor_PaintRadius * 0.7f);

                                    for (int x = -rad; x < rad; x++)
                                        for (int ry = 0; ry < _Editor_RadiusY; ry++)
                                            for (int z = -rad; z < rad; z++)
                                            {
                                                if (x == 0 && ry == 0 && z == 0) continue;
                                                Gizmos.DrawWireCube(gridPos + new Vector3(cSize.x * x, cSize.y * ry, cSize.z * z), new Vector3(cSize.x, cSize.y * 0.2f, cSize.z));
                                            }
                                }
                            }
                            else // 2D
                            {
                                Vector3 mousePos = transform.InverseTransformPoint(GridVisualize.GetMouseWorldPosition2D(-transform.forward, Event.current, SceneView.currentDrawingSceneView.camera, _Editor_YLevel, cSize.y, transform.position.z * Vector3.forward));
                                Gizmos.DrawLine(mousePos, mousePos + Vector3.back * 1);
                                Vector3 gridPos = FVectorMethods.FlattenVector(mousePos - new Vector3(0f, cSize.y * 0.5f, 0f), cSize);

                                Gizmos.DrawWireCube(gridPos + new Vector3(0, cSize.y * 0.5f, 0f), new Vector3(cSize.x, cSize.y, cSize.z * 0.2f));

                                if (_Editor_PaintRadius > 1 || _Editor_RadiusY > 1)
                                {
                                    int rad = Mathf.RoundToInt((float)_Editor_PaintRadius * 0.7f);

                                    for (int x = -rad; x < rad; x++)
                                        for (int ry = -rad; ry < rad; ry++)
                                            for (int z = 0; z < _Editor_RadiusY; z++)
                                            {
                                                if (x == 0 && ry == 0 && z == 0) continue;
                                                Gizmos.DrawWireCube(gridPos + new Vector3(0f, cSize.y * 0.5f, 0) + new Vector3(cSize.x * x, cSize.y * ry, cSize.z * z), new Vector3(cSize.x, cSize.y, cSize.z * 0.2f));
                                            }
                                }
                            }

                        }
                }

            Gizmos.color = preColor;
            Gizmos.matrix = Matrix4x4.identity;
            Handles.matrix = Matrix4x4.identity;
        }

        public void GenerateDefaultCells(Vector3Int vector3)
        {
            grid.Generate(vector3.x, vector3.y, vector3.z, Vector3Int.zero);
            SaveCells();
            LoadCells();
        }

        #endregion

#endif


        #region Saving and unloading cells

        public void SaveCells()
        {
            if (grid.AllCells.Count <= 1) return;

            ClearSavedCells();

            for (int i = 0; i < grid.AllApprovedCells.Count; i++)
            {
                var cell = grid.AllApprovedCells[i];
                PainterCell pCell = new PainterCell();
                pCell.pos = cell.Pos;
                pCell.rot = Quaternion.identity;
                pCell.inGrid = cell.InTargetGridArea;
                cell.GridPainter_AssignDataTo(ref pCell);
                //pCell.Instructions = cell.GetInstructions();
                cellsMemory.Add(pCell);
            }
        }

        public void LoadCells()
        {
            if (cellsMemory.Count <= 0) return;

            grid = new FGenGraph<FieldCell, FGenPoint>();

            for (int i = 0; i < cellsMemory.Count; i++)
            {
                PainterCell pCell = cellsMemory[i];

                //RoomCell cell = new RoomCell();
                //cell.Pos = pCell.pos;
                //cell.InTargetGridArea = pCell.inGrid;

                if (pCell.inGrid)
                {
                    var cell = grid.AddCell(pCell.pos.x, pCell.pos.y, pCell.pos.z);
                    cell.GridPainter_AssignDataFrom(pCell);
                    //cell.ReplaceInstructions(pCell.Instructions);
                    //grid.AllApprovedCells.Add(cell);
                    //grid.AllCells.Add(cell);
                }
            }
        }

        public void ClearSavedCells()
        {
            cellsMemory.Clear();
        }

        [System.Serializable]
        public struct PainterCell
        {
            public Vector3Int pos;
            public Quaternion rot;
            public bool inGrid;
            public bool isGhost;

            public List<InstructionDefinition> Instructions;

            public bool isDirty;
            public List<SpawnData> spawns;
            public Vector3Int parentCell;
            public List<Vector3Int> childCells;

            // Temporary solution for planner executor cell datas copy
            [NonSerialized] public List<UnityEngine.Object> cellCustomObjects;
            [NonSerialized] public bool IsGhostCell;
            [NonSerialized] public List<string> cellCustomData;
            [NonSerialized] public Vector3 HelperVector;


            public void Move(Vector3Int newpos)
            {
                pos = newpos;
            }

            public void OffsetPos(Vector3Int off)
            {
                pos += off;
            }

            public bool AddInstruction(InstructionDefinition instr)
            {
                if (Instructions == null) Instructions = new List<InstructionDefinition>();

                for (int i = 0; i < Instructions.Count; i++)
                    if (Instructions[i].InstructionType == instr.InstructionType) return false;

                Instructions.Add(instr);
                return true;
            }

            internal void ClearInstructions()
            {
                Instructions.Clear();
            }
        }

        public int GetPainterCell(FieldCell graphCell)
        {
            if (graphCell == null) return -1;

            for (int i = 0; i < cellsMemory.Count; i++)
            {
                if (cellsMemory[i].pos == graphCell.Pos) return i;
            }

            return -1;
        }

        #endregion


        #region Field Variables Related

        public void RefreshFieldVariables()
        {
            if (SwitchVariables == null) SwitchVariables = new List<FieldVariable>();
            if (FieldPreset == null) return;

            if (SwitchVariables.Count == 0)
            {
                #region Add all elements from zero
                for (int i = 0; i < FieldPreset.Variables.Count; i++)
                {
                    SwitchVariables.Add(new FieldVariable(FieldPreset.Variables[i]));
                }
                #endregion
            }
            else
            {
                PGGUtils.AdjustCount<FieldVariable>(SwitchVariables, FieldPreset.Variables.Count);

                // Checking if same variable, if not then changing parameters
                for (int i = 0; i < FieldPreset.Variables.Count; i++)
                {
                    FieldVariable fv = FieldPreset.Variables[i];
                    FieldVariable v = SwitchVariables[i];
                    if (fv.Name != v.Name || fv.ValueType != v.ValueType)
                    {
                        SwitchVariables[i] = new FieldVariable(fv);
                        SwitchVariables[i].helperPackRef = FieldPreset.RootPack;
                    }
                }
            }


            if (SwitchPackVariables == null) SwitchPackVariables = new List<FieldVariable>();
            int countPackVars = CountFieldModsVariablesCount();

            if (SwitchPackVariables.Count == 0)
            {
                #region Add all elements from zero
                for (int i = 0; i < FieldPreset.ModificatorPacks.Count; i++)
                {
                    var pack = FieldPreset.ModificatorPacks[i];
                    if (pack == null) continue;

                    // Checking if same variable, if not then changing parameters
                    for (int p = 0; p < pack.Variables.Count; p++)
                    {
                        FieldVariable vr = pack.Variables[p];
                        vr.helperPackRef = pack;
                        SwitchPackVariables.Add(vr);
                    }
                }
                #endregion
            }
            else
            {
                PGGUtils.AdjustCount<FieldVariable>(SwitchPackVariables, countPackVars);

                int iter = 0;
                for (int i = 0; i < FieldPreset.ModificatorPacks.Count; i++)
                {
                    var pack = FieldPreset.ModificatorPacks[i];
                    if (pack == null) continue;

                    // Checking if same variable, if not then changing parameters
                    for (int p = 0; p < pack.Variables.Count; p++)
                    {
                        FieldVariable fv = pack.Variables[p];
                        FieldVariable v = SwitchPackVariables[iter];

                        if (fv.Name != v.Name || fv.ValueType != v.ValueType)
                        {
                            SwitchPackVariables[iter] = new FieldVariable(fv);
                        }

                        SwitchPackVariables[iter].helperPackRef = pack;

                        iter += 1;
                    }
                }
            }

        }


        private int CountFieldModsVariablesCount()
        {
            if (FieldPreset == null) return 0;

            int countPackVars = 0;
            for (int i = 0; i < FieldPreset.ModificatorPacks.Count; i++)
            {
                var pack = FieldPreset.ModificatorPacks[i];
                if (pack == null) continue;
                countPackVars += FieldPreset.ModificatorPacks[i].Variables.Count;
            }

            return countPackVars;
        }

        private List<FieldVariable> _fieldVariablesBackup;
        private List<FieldVariable> _packVariablesBackup;
        private void FieldVariablesBackup()
        {
            if (FieldPreset == null) return;
            if (_ModifyVars)
            {
                if (_fieldVariablesBackup == null) _fieldVariablesBackup = new List<FieldVariable>();
                _fieldVariablesBackup.Clear();

                for (int i = 0; i < FieldPreset.Variables.Count; i++)
                {
                    FieldVariable nVar = new FieldVariable(FieldPreset.Variables[i]);
                    _fieldVariablesBackup.Add(nVar);
                }
            }

            if (_ModifyPackVars)
            {
                if (_packVariablesBackup == null) _packVariablesBackup = new List<FieldVariable>();
                _packVariablesBackup.Clear();
                for (int i = 0; i < FieldPreset.ModificatorPacks.Count; i++)
                {
                    if (FieldPreset.ModificatorPacks[i] == null) continue;
                    for (int m = 0; m < FieldPreset.ModificatorPacks[i].Variables.Count; m++)
                    {
                        FieldVariable nVar = new FieldVariable(FieldPreset.ModificatorPacks[i].Variables[m]);
                        nVar.helperPackRef = FieldPreset.ModificatorPacks[i];
                        _packVariablesBackup.Add(nVar);
                    }
                }
            }
        }

        private void FieldVariablesSetCustom()
        {
            if (FieldPreset == null) return;

            if (_ModifyVars)
            {
                if (SwitchVariables.Count == FieldPreset.Variables.Count)
                {
                    for (int i = 0; i < SwitchVariables.Count; i++)
                    {
                        FieldPreset.Variables[i].SetValue(SwitchVariables[i]);
                    }
                }
            }

            if (_ModifyPackVars)
            {
                for (int m = 0; m < SwitchPackVariables.Count; m++)
                {
                    var fv = SwitchPackVariables[m];
                    if (fv == null) continue;
                    if (fv.helperPackRef == null) continue;
                    var mv = fv.helperPackRef.GetVariable(fv.Name);
                    if (mv == null) continue;
                    if (mv.ValueType != fv.ValueType) continue;
                    mv.SetValue(fv);
                }
            }
        }

        private void FieldVariablesRestore()
        {
            if (FieldPreset == null) return;

            if (_ModifyVars)
            {
                if (FieldPreset.Variables.Count == _fieldVariablesBackup.Count)
                {
                    for (int i = 0; i < FieldPreset.Variables.Count; i++)
                    {
                        FieldPreset.Variables[i].SetValue(_fieldVariablesBackup[i]);
                    }

                    _fieldVariablesBackup.Clear();
                }
            }

            if (_ModifyPackVars)
            {
                for (int m = 0; m < _packVariablesBackup.Count; m++)
                {
                    var fv = _packVariablesBackup[m];
                    if (fv == null) continue;
                    if (fv.helperPackRef == null) continue;
                    var mv = fv.helperPackRef.GetVariable(fv.Name);
                    if (mv == null) continue;
                    if (mv.ValueType != fv.ValueType) continue;
                    mv.SetValue(fv);
                }
            }
        }

        #endregion


    }




#if UNITY_EDITOR

    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(GridPainter))]
    public class InteriorPainterEditor : PGGGeneratorBaseEditor
    {
        public GridPainter Get { get { if (_get == null) _get = (GridPainter)target; return _get; } }
        private GridPainter _get;

        SerializedProperty sp_Lists;
        SerializedObject basSerializedObject;
        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (Get.LoadCount > 1) Get.LoadCells();

            sp_Lists = serializedObject.FindProperty("AdditionalFieldSetups");
            _ignore.Add("AdditionalFieldSetups");
            _ignore.Add("Injections");
            _ignore.Add("CellsInstructions");
            _ignore.Add("AcquireCellDataFrom");

            Get.CheckMemoryForDuplicates();

            basSerializedObject = serializedObject;
        }

        protected override void DrawGUIBody()
        {
            base.DrawGUIBody();

            FGUI_Inspector.LastGameObjectSelected = Get.gameObject;

            GUILayout.Space(4);


            if (Get.Painting) GUI.backgroundColor = Color.green;
            if (GUILayout.Button(Get.Painting ? "Stop Painting" : "Start Painting")) Get.Painting = !Get.Painting;
            if (Get.Painting) GUI.backgroundColor = Color.white;

            //if (Get._Editor_ContinousMode) GUI.backgroundColor = Color.green;
            //if (GUILayout.Button(new GUIContent("Continous Mode (BETA)", "Continous generation mode: No re-generating all objects every refresh but generating/refreshing only new painted cells and nearest surroundings")))
            //{
            //    Get._Editor_ContinousMode = !Get._Editor_ContinousMode;
            //    if (Get._Editor_ContinousMode) Get.RandomSeed = false;
            //}
            //if (Get._Editor_ContinousMode) GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate"))
            {
                Get.LoadCells();
                Get.GenerateObjects();

                var sel = GetAllSelected();
                for (int i = 0; i < sel.Count; i++)
                {
                    sel[i].LoadCells();
                    sel[i].GenerateObjects();
                }
            }

            if (Get.Generated != null) if (Get.Generated.Count > 0) if (GUILayout.Button("Clear Generated"))
                    {
                        Get.ClearGenerated();

                        var sel = GetAllSelected();
                        for (int i = 0; i < sel.Count; i++) sel[i].ClearGenerated();
                    }
            EditorGUILayout.EndHorizontal();


            GUILayout.Space(4);

            FGUI_Inspector.FoldHeaderStart(ref Get._EditorGUI_DrawExtra, "More Options", FGUI_Resources.BGInBoxStyle);

            if (Get._EditorGUI_DrawExtra)
            {
                GUILayout.Space(3);
                if (Get.Composition != null)
                    if (Get.Composition.Setup != Get.FieldPreset)
                    {
                        Get.Composition.Setup = Get.FieldPreset;
                    }

                FieldSetupComposition.DrawCompositionGUI(Get, Get.Composition, true);

                GUILayout.Space(3);
                EditorGUILayout.LabelField("Features below may be replaced and contained by compositions in the future versions!", EditorStyles.centeredGreyMiniLabel);
                GUILayout.Space(3);


                EditorGUI.indentLevel++;
                GUILayout.Space(3);
                SerializedProperty sp = sp_Lists.Copy();
                EditorGUILayout.PropertyField(sp, true); sp.Next(false);
                EditorGUILayout.PropertyField(sp, true); sp.Next(false);
                EditorGUILayout.PropertyField(sp, true); sp.Next(false);
                EditorGUILayout.PropertyField(sp, true);
                GUILayout.Space(3);
                EditorGUI.indentLevel--;

                EditorGUI.indentLevel++;
                base.DrawGUIFooter();
                EditorGUI.indentLevel--;



                GUILayout.Space(4);
                EditorGUILayout.BeginVertical(FGUI_Resources.BGBoxStyle);
                EditorGUILayout.LabelField("This two tabs will be removed in future versions", EditorStyles.centeredGreyMiniLabel);

                if (Get.FieldPreset != null)
                {


                    GUILayout.Space(3);
                    FGUI_Inspector.FoldSwitchableHeaderStart(ref Get._ModifyVars, ref Get._EditorGUI_DrawVars, "FieldSetup Variables values for generating", FGUI_Resources.BGInBoxStyle);
                    GUILayout.Space(3);

                    if (Get._EditorGUI_DrawVars && Get._ModifyVars)
                    {
                        Get.RefreshFieldVariables();
                        for (int i = 0; i < Get.SwitchVariables.Count; i++)
                        {
                            FieldVariable.Editor_DrawTweakableVariable(Get.SwitchVariables[i]);
                        }
                    }


                    EditorGUILayout.EndVertical();



                    if (Get.SwitchPackVariables.Count > 0)
                    {
                        GUILayout.Space(3);
                        FGUI_Inspector.FoldSwitchableHeaderStart(ref Get._ModifyPackVars, ref Get._EditorGUI_DrawPackVars, "Mod Packs Variables values for generating", FGUI_Resources.BGInBoxStyle);
                        GUILayout.Space(3);

                        if (Get._EditorGUI_DrawPackVars && Get._ModifyPackVars)
                        {
                            Get.RefreshFieldVariables();
                            for (int i = 0; i < Get.SwitchPackVariables.Count; i++)
                            {
                                FieldVariable.Editor_DrawTweakableVariable(Get.SwitchPackVariables[i]);
                            }
                        }

                        EditorGUILayout.EndVertical();
                    }



                    GUILayout.Space(3);
                    FGUI_Inspector.FoldHeaderStart(ref Get._EditorGUI_DrawIgnoring, "Select Ignored Modificators", FGUI_Resources.BGInBoxStyle);
                    GUILayout.Space(3);

                    if (Get._EditorGUI_DrawIgnoring)
                    {

                        if (Get.FieldPreset.ModificatorPacks.Count > 0)
                        {
                            EditorGUILayout.BeginHorizontal();

                            if (GUILayout.Button(" < ", GUILayout.Width(40))) Get._EditorGUI_SelectedId--;

                            EditorGUILayout.LabelField((Get._EditorGUI_SelectedId + 1) + " / " + Get.FieldPreset.ModificatorPacks.Count, EditorStyles.centeredGreyMiniLabel);

                            if (GUILayout.Button(" > ", GUILayout.Width(40))) Get._EditorGUI_SelectedId++;

                            if (Get._EditorGUI_SelectedId >= Get.FieldPreset.ModificatorPacks.Count) Get._EditorGUI_SelectedId = 0;
                            if (Get._EditorGUI_SelectedId < 0) Get._EditorGUI_SelectedId = Get.FieldPreset.ModificatorPacks.Count - 1;

                            EditorGUILayout.EndHorizontal();

                            if (Get._EditorGUI_SelectedId == -1)
                                DrawIgnoresList(Get.FieldPreset.RootPack);
                            else
                                DrawIgnoresList(Get.FieldPreset.ModificatorPacks[Get._EditorGUI_SelectedId]);
                        }
                        else
                        {
                            DrawIgnoresList(Get.FieldPreset.RootPack);
                        }

                    }



                    EditorGUILayout.EndVertical();


                }


                EditorGUILayout.EndVertical();

            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(2);

        }


        List<GridPainter> GetAllSelected()
        {
            List<GridPainter> sel = new List<GridPainter>();
            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                GridPainter p = Selection.gameObjects[i].GetComponent<GridPainter>();
                if (p != Get) if (p) sel.Add(p);
            }

            return sel;
        }

        private void DrawIgnoresList(ModificatorsPack pack)
        {
            bool preE = GUI.enabled;
            GUILayout.Space(2);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            bool pselected = !Get.ignoredPacksForGenerating.Contains(pack);
            bool pre = pselected;
            pselected = EditorGUILayout.Toggle(pselected, GUILayout.Width(18));
            EditorGUILayout.LabelField(pack.name + " Mods Pack", FGUI_Resources.HeaderStyle);

            if (pselected != pre)
            {
                if (pselected == false)
                {
                    Get.ignoredPacksForGenerating.Add(pack);
                    serializedObject.Update();
                    EditorUtility.SetDirty(Get);
                }
                else
                {
                    Get.ignoredPacksForGenerating.Remove(pack);
                    serializedObject.Update();
                    EditorUtility.SetDirty(Get);
                }
            }

            GUI.enabled = false;
            EditorGUILayout.ObjectField(pack, typeof(ModificatorsPack), false, GUILayout.Width(60));
            GUI.enabled = preE;
            EditorGUILayout.EndHorizontal();

            if (pselected == false) GUI.enabled = false;

            GUILayout.Space(4);
            for (int i = 0; i < pack.FieldModificators.Count; i++)
            {
                var mod = pack.FieldModificators[i];
                EditorGUILayout.BeginHorizontal();

                bool selected = !Get.ignoredForGenerating.Contains(mod);
                pre = selected;

                selected = EditorGUILayout.Toggle(selected, GUILayout.Width(18));

                EditorGUIUtility.labelWidth = 60;
                EditorGUILayout.ObjectField(selected ? "Enabled" : "Ignored", mod, typeof(FieldModification), true);
                EditorGUIUtility.labelWidth = 0;

                if (selected != pre)
                {
                    if (selected == false)
                    {
                        Get.ignoredForGenerating.Add(mod);
                        serializedObject.Update();
                        EditorUtility.SetDirty(Get);
                    }
                    else
                    {
                        Get.ignoredForGenerating.Remove(mod);
                        serializedObject.Update();
                        EditorUtility.SetDirty(Get);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            if (Get.ignoredForGenerating.Count > 0)
            {
                GUILayout.Space(4);

                if (GUILayout.Button("Clear All Ignores"))
                {
                    Get.ignoredForGenerating.Clear();
                    serializedObject.Update();
                    EditorUtility.SetDirty(Get);
                }
            }

            GUILayout.Space(4);
            GUI.enabled = preE;

        }

        protected override void DrawGUIFooter()
        {
        }


        #region Scene GUI

        private void OnSceneGUI()
        {
            if (SceneView.currentDrawingSceneView == null) return;
            if (SceneView.currentDrawingSceneView.camera == null) return;
            if (Selection.activeGameObject != Get.gameObject) return;
            if (Get.FieldPreset == null) return;

            Undo.RecordObject(Get, "PGGGridP");

            Vector3 cSize = Get.FieldPreset.GetCellUnitSize();
            Color preH = Handles.color;

            if (Get.Grid != null)

                if (Get._Editor_Paint == false)
                    if (Get.GetAllPainterCells.Count > 0)
                    {
                        Handles.color = new Color(0.2f, 1f, 0.2f, 0.8f);

                        for (int i = Get.CellsInstructions.Count - 1; i >= 0; i--)
                        {
                            if (Get.CellsInstructions[i] == null)
                            {
                                Get.CellsInstructions.RemoveAt(i);
                                continue;
                            }

                            var instr = Get.CellsInstructions[i];
                            float size = cSize.x;
                            Vector3 genPosition = Get.transform.TransformPoint(Get.FieldPreset.TransformCellPosition((Vector3)instr.pos));
                            //Vector3 genPosition = Get.transform.TransformPoint((Vector3)instr.pos * size);

                            if (Handles.Button(genPosition, Quaternion.identity, size * 0.2f, size * 0.2f, Handles.SphereHandleCap))
                            {
                                if (Get.Selected == instr) Get.Selected = null; else Get.Selected = instr;
                            }

                            if (Get.Selected == instr)
                            {
                                if (DrawButton(EditorGUIUtility.IconContent(Get._Editor_RotOrMovTool ? "MoveTool" : "RotateTool"), genPosition - Get.transform.forward * size * 0.4f - Get.transform.right * size * 0.4f, size * 0.9f))
                                {
                                    Get._Editor_RotOrMovTool = !Get._Editor_RotOrMovTool;
                                }

                                if (Get._Editor_RotOrMovTool)
                                {
                                    Handles.Label(genPosition - Get.transform.forward * size * 0.44f - Get.transform.right * size * 0.57f, new GUIContent(FGUI_Resources.Tex_Info, "Switch back to cell rotation tool if you want to use directional mode. If you leave this guide in movement mode then rotation will not be used!"));
                                }

                                instr.UseDirection = !Get._Editor_RotOrMovTool;

                                //if (Get.CellsInstructions.Count > 1)
                                //{
                                //    if (DrawButton(new GUIContent(instr.Id.ToString()), genPosition + Get.transform.forward * size * 0.4f , size * 0.9f)) instr.Id += 1;
                                //    if (DrawButton(new GUIContent("+"), genPosition + Get.transform.forward * size * 0.4f + Get.transform.right * size * 0.4f, size * 0.9f)) instr.Id += 1;
                                //    if (DrawButton(new GUIContent("-"), genPosition + Get.transform.forward * size * 0.3f + Get.transform.right * size * 0.5f, size * 0.9f)) instr.Id -= 1;
                                //    if (instr.Id < 0) instr.Id = Get.CellsInstructions.Count - 1;
                                //    if (instr.Id > Get.CellsInstructions.Count - 1) instr.Id = 0;
                                //}

                                if (Get._Editor_RotOrMovTool == false)
                                {
                                    Get.Selected.rot = FEditor_TransformHandles.RotationHandle(Get.Selected.rot, genPosition, size * 0.75f);
                                    Get.Selected.rot = FVectorMethods.FlattenRotation(Get.Selected.rot, 45f);
                                }
                                else
                                {
                                    Get.Selected.pos = PGGUtils.V3toV3Int(Get.transform.InverseTransformPoint(FEditor_TransformHandles.PositionHandle(genPosition, Get.transform.rotation * instr.rot, size * 0.75f)) / size);
                                    //Get.Selected.pos = FVectorMethods.FlattenVector(Get.Selected.pos, Get.RoomPreset.CellSize);
                                }

                                Quaternion r;
                                if (instr.WorldRot)
                                    r = Get.Selected.rot;
                                else
                                    r = Get.transform.rotation * Get.Selected.rot;

                                FGUI_Handles.DrawArrow(genPosition, Quaternion.Euler(r.eulerAngles), 0.5f, 0f, 1f);

                                if (instr != null)
                                {
                                    if (instr.CustomDefinition.InstructionType != InstructionDefinition.EInstruction.None)
                                        Handles.Label(genPosition + Get.transform.forward * size * 0.3f, new GUIContent(instr.CustomDefinition.Title));
                                    else
                                        Handles.Label(genPosition + Get.transform.forward * size * 0.3f, new GUIContent(Get.FieldPreset.CellsInstructions[instr.Id].Title));
                                }
                            }
                        }

                    }


            Handles.color = preH;
            Handles.BeginGUI();

            float dpiH = FGenerators.EditorUIScale;
            int hOffset = (int)(Screen.height / dpiH);

            GridVisualize.DrawPaintGUI(ref Get._Editor_Paint, hOffset);

            if (Get._Editor_Paint)
            {
                //if (Get._Editor_PaintSpaceMode == GridPainter.EPaintSpaceMode.XZ)
                {
                    Rect radRect = new Rect(15, hOffset - 140, 140, 20);
                    Get._Editor_PaintRadius = Mathf.RoundToInt(GUI.HorizontalSlider(radRect, Get._Editor_PaintRadius, 1, 4));
                    radRect = new Rect(radRect);
                    radRect.y -= 18;
                    GUI.Label(radRect, new GUIContent("Radius"));

                    radRect = new Rect(15, hOffset - 179, 140, 20);
                    Get._Editor_RadiusY = Mathf.RoundToInt(GUI.HorizontalSlider(radRect, Get._Editor_RadiusY, 1, 4));
                    radRect = new Rect(radRect);
                    radRect.y -= 18;
                    GUI.Label(radRect, new GUIContent("Radius Y"));
                }

                Rect refreshRect = new Rect(160, hOffset - 75, 120, 20);
                Get.AutoRefresh = GUI.Toggle(refreshRect, Get.AutoRefresh, "Auto Refresh");

                if (!Get.AutoRefresh)
                {
                    refreshRect.position += new Vector2(116, 0);

                    if (GUI.Button(refreshRect, "Generate"))
                    {
                        Get.LoadCells();
                        Get.GenerateObjects();

                        var sel = GetAllSelected();
                        for (int i = 0; i < sel.Count; i++)
                        {
                            sel[i].LoadCells();
                            sel[i].GenerateObjects();
                        }
                    }
                }
            }
            else
            {
                Rect refreshRect = new Rect(160, hOffset - 75, 120, 20);
                if (GUI.Button(refreshRect, "Generate"))
                {
                    Get.LoadCells();
                    //Get.ClearGenerated();
                    Get.GenerateObjects();

                    var sel = GetAllSelected();
                    for (int i = 0; i < sel.Count; i++)
                    {
                        sel[i].LoadCells();
                        //sel[i].ClearGenerated();
                        sel[i].GenerateObjects();
                    }
                }
            }

            Color preC = GUI.backgroundColor;
            Rect bRect;

            int x = 160;
            int y = (int)(22 * dpiH);
            bRect = new Rect(x, y, 160, 40);

            string focusTxt = Get._Editor_PaintSpaceMode == GridPainter.EPaintSpaceMode.XZ ? "Focus On Y Level: " : "Focus On Z Depth: ";

            GUI.Label(bRect, focusTxt + Get._Editor_YLevel, FGUI_Resources.HeaderStyle);
            bRect = new Rect(x + 150, y, 20, 20);
            if (GUI.Button(bRect, "▲")) Get._Editor_YLevel++;


            if (Get._Editor_Paint)
            {
                bRect = new Rect(x + 185, y, 55, 20);
                GUI.Label(bRect, "shift + A", FGUI_Resources.HeaderStyle);
            }

            bRect = new Rect(x + 150, y + 22 * dpiH, 20, 20);
            if (GUI.Button(bRect, "▼")) Get._Editor_YLevel--;

            if (Get._Editor_Paint)
            {
                bRect = new Rect(x + 185, y + 21 * dpiH, 55, 20);
                GUI.Label(bRect, "shift + Z", FGUI_Resources.HeaderStyle);
            }

            bRect = new Rect(x + 20 * dpiH, y + 31 * dpiH, 120, 20);
            bool is2D = Get._Editor_PaintSpaceMode == GridPainter.EPaintSpaceMode.XY_2D;
            is2D = GUI.Toggle(bRect, is2D, " Paint 2D");
            Get._Editor_PaintSpaceMode = is2D ? GridPainter.EPaintSpaceMode.XY_2D : GridPainter.EPaintSpaceMode.XZ;

            //bRect = new Rect(x + 200, y - 7 * dpiH, 300, 100);
            //GUI.Label(bRect, "If you don't see 'Paint' button on the bottom of\nscene view then you must change dpi settings\nof unity editor and restart it.");


            if (string.IsNullOrEmpty(Get.FieldPreset.InfoText) == false)
            {
                GUIContent cont = new GUIContent("Info:\n" + Get.FieldPreset.InfoText);
                Vector2 sz = EditorStyles.label.CalcSize(cont); sz = new Vector2(sz.x * 1.04f + 8, sz.y * 1.1f);
                bRect = new Rect((Screen.width / dpiH) - sz.x, hOffset - 45 * dpiH - sz.y, sz.x, sz.y);
                GUI.Label(bRect, cont);
            }


            DrawCellTools();
            Handles.EndGUI();

            //if (Get.Painting)
            if (Get.FieldPreset != null)
                if (Event.current.type == EventType.MouseMove)
                    SceneView.RepaintAll();

            int eButton = Event.current.button;

            if (Get.PaintingID <= -1)
            {
                var cell = GridVisualize.ProcessInputEvents(ref Get._Editor_Paint, Get.grid, Get.FieldPreset, ref Get._Editor_YLevel, Get.transform, true, cSize.y, is2D);

                if (cell != null)
                {
                    if (Get._Editor_PaintRadius > 1 || Get._Editor_RadiusY > 1)
                    {
                        int rad = Mathf.RoundToInt((float)Get._Editor_PaintRadius * 0.7f);

                        if (!is2D)
                        {
                            for (int rx = -rad; rx < rad; rx++)
                                for (int ry = 0; ry < Get._Editor_RadiusY; ry++)
                                    for (int rz = -rad; rz < rad; rz++)
                                    {
                                        if (rx == 0 && ry == 0 && rz == 0) continue;
                                        Vector3Int radPos = cell.Pos + new Vector3Int(rx, ry, rz);

                                        if (cell.InTargetGridArea)
                                        {
                                            Get.grid.AddCell(radPos);
                                            cell.IsGhostCell = Get.PaintingID == -2;
                                        }
                                        else
                                        {
                                            Get.grid.RemoveCell(radPos);
                                            cell.IsGhostCell = false;
                                        }
                                    }
                        }
                        else
                        {
                            for (int rx = -rad; rx < rad; rx++)
                                for (int ry = -rad; ry < rad; ry++)
                                    for (int rz = 0; rz < Get._Editor_RadiusY; rz++)
                                    {
                                        if (rx == 0 && ry == 0 && rz == 0) continue;
                                        Vector3Int radPos = cell.Pos + new Vector3Int(rx, ry, rz);

                                        if (cell.InTargetGridArea)
                                        {
                                            Get.grid.AddCell(radPos);
                                            cell.IsGhostCell = Get.PaintingID == -2;
                                        }
                                        else
                                        {
                                            Get.grid.RemoveCell(radPos);
                                            cell.IsGhostCell = false;
                                        }
                                    }
                        }


                    }
                    else
                    {
                        if (FGenerators.CheckIfExist_NOTNULL(cell)) cell.IsGhostCell = Get.PaintingID == -2;
                    }

                    Get.OnChange();
                }
            }
            else
            {
                bool change = false;
                bool rem = false;

                var e = Event.current;
                if (e != null) if (e.isMouse) if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag) if (Event.current.button == 1) rem = true;

                var cell = GridVisualize.ProcessInputEvents(ref Get._Editor_Paint, Get.grid, Get.FieldPreset, ref Get._Editor_YLevel, Get.transform, false, cSize.y, is2D);

                if (cell != null)
                {
                    int already = -1;
                    for (int i = 0; i < Get.CellsInstructions.Count; i++)
                    {
                        if (Get.CellsInstructions[i] == null) continue;
                        if (Get.CellsInstructions[i].pos == cell.Pos)
                        {
                            already = i;
                            if (Get.AllowOverlapInstructions)
                            {
                                if (Get.CellsInstructions[i].Id == Get.PaintingID)
                                {
                                    break;
                                }
                                else
                                    if (eButton == 0) already = -1;
                            }
                            else
                                break;
                        }
                    }

                    if (already == -1)
                    {

                        if (eButton == 0)
                        {
                            SpawnInstructionGuide instr = new SpawnInstructionGuide();
                            instr.pos = cell.Pos;
                            instr.Id = Get.PaintingID;

                            Get.CellsInstructions.Add(instr);
                            if (Get.AddCellsOnInstructions) Get.grid.AddCell(cell.Pos);

                            change = true;
                            basSerializedObject.Update();
                        }
                    }
                    else
                    {
                        if (rem)
                        {
                            Get.CellsInstructions.RemoveAt(already);
                            change = true;
                            basSerializedObject.Update();
                        }
                    }

                    if (change) Get.OnChange();
                }
            }

        }

        public void DrawCellTools()
        {
            if (Get.FieldPreset == null) return;

            Rect bRect = new Rect(15, 15, 132, 24);
            Color preC = GUI.backgroundColor;

            if (Get.PaintingID == -1) GUI.backgroundColor = Color.green;
            if (GUI.Button(bRect, "Paint Cells "))
            {
                Get.PaintingID = -1;
            }

            bRect.position += new Vector2(0, bRect.size.y + 2);
            //gRect.position += new Vector2(bRect.size.x + 10, 0);
            //gRect.size = new Vector2(90, 24);
            GUI.backgroundColor = preC;

            if (Get.PaintingID == -2) GUI.backgroundColor = Color.green;
            if (GUI.Button(bRect, "Ghost Cell")) Get.PaintingID = -2;
            GUI.backgroundColor = preC;


            int commandsPerPage = 7;
            int totalPages = ((Get.FieldPreset.CellsInstructions.Count + (commandsPerPage - 1)) / commandsPerPage);

            bRect.y += 10;

            if (totalPages > 1)
            {
                float yy = bRect.y + FGenerators.EditorUIScale * 24;
                Rect btRect = new Rect(bRect.x, yy, 20, 18);
                if (GUI.Button(btRect, "<")) { Get._Editor_CommandsPage -= 1; if (Get._Editor_CommandsPage < 0) Get._Editor_CommandsPage = totalPages - 1; }
                btRect = new Rect(bRect.x + 24, yy, 86, 18);
                GUI.Label(btRect, (Get._Editor_CommandsPage + 1) + " / " + totalPages, FGUI_Resources.HeaderStyle);
                btRect = new Rect(bRect.x + 100, yy, 20, 18);
                if (GUI.Button(btRect, ">")) { Get._Editor_CommandsPage += 1; if (Get._Editor_CommandsPage >= totalPages) Get._Editor_CommandsPage = 0; }
                bRect.y += 20;
            }

            bRect.y += 4 * FGenerators.EditorUIScale;
            bRect.height -= 4;
            bRect.width += 6;
            float padding = 23 * FGenerators.EditorUIScale;

            if (Get._Editor_CommandsPage >= totalPages) Get._Editor_CommandsPage = 0;
            int startI = Get._Editor_CommandsPage * commandsPerPage;

            for (int i = startI; i < startI + commandsPerPage; i++)
            {
                if (i >= Get.FieldPreset.CellsInstructions.Count) break;

                bRect.y += padding;
                if (Get.PaintingID == i) GUI.backgroundColor = Color.green;

                if (FGenerators.CheckIfExist_NOTNULL(Get.FieldPreset.CellsInstructions[i]))
                    if (GUI.Button(bRect, Get.FieldPreset.CellsInstructions[i].Title))
                        Get.PaintingID = i;

                if (Get.PaintingID == i) GUI.backgroundColor = preC;
            }

            //for (int i = 0; i < Get.FieldPreset.CellsInstructions.Count; i++)
            //{
            //    bRect.y += padding;
            //    if (Get.PaintingID == i) GUI.backgroundColor = Color.green;

            //    if (FGenerators.CheckIfExist_NOTNULL(Get.FieldPreset.CellsInstructions[i]))
            //        if (GUI.Button(bRect, Get.FieldPreset.CellsInstructions[i].Title))
            //        {
            //            Get.PaintingID = i;
            //        }
            //    GUI.backgroundColor = preC;
            //}

            GUI.backgroundColor = preC;
        }

        bool DrawButton(GUIContent content, Vector3 pos, float size)
        {
            float sc = HandleUtility.GetHandleSize(pos);
            float hSize = Mathf.Sqrt(size) * 32 - sc * 16;

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

        void TextField(ref string s, Vector3 pos, float size, float widthMul)
        {
            float sc = HandleUtility.GetHandleSize(pos);
            float hSize = Mathf.Sqrt(size) * 32 - sc * 16;

            if (hSize > 0f)
            {
                Handles.BeginGUI();
                Vector2 pos2D = HandleUtility.WorldToGUIPoint(pos);
                float hhSize = hSize / 2f;
                s = GUI.TextField(new Rect(pos2D.x - hhSize, pos2D.y - hhSize, hSize * widthMul, hSize), s);

                Handles.EndGUI();
            }
        }

        #endregion


    }


#endif
}