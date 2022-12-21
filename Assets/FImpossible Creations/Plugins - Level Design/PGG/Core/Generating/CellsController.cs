using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    /// <summary>
    /// Class with informations about computed operations and target objects to generate on grid cells for single FieldSetup.
    /// This are not yet instantiated objects on the scene.
    /// This is result of FieldSetup execution on defined grid.
    /// After all computations you basically get list of objects to instantiate in queue which should be created on scene with method InstantiateStep().
    /// </summary>
    [System.Serializable]
    public class CellsController
    {
        [System.NonSerialized] public FlexibleGeneratorSetup FlexSetup;

        public GeneratingPreparation ParentPreparation { get { return FlexSetup.Preparation; } }
        public FieldSetup RuntimeFieldSetup { get { return FlexSetup.RuntimeFieldSetup; } }
        public InstantiatedFieldInfo InstantiatedInfo { get { return FlexSetup.InstantiatedInfo; } }

        public int CellsCount { get { return GridCellsSave.Count; } }
        public int ToSpawnCount { get { return spawnQueue.Count; } }
        public bool WaitingToBeSpawned { get { return spawnQueue.Count > 0 || ToUpdate.Count > 0; } }


        [HideInInspector] [SerializeField] public List<FieldCell> GridCellsSave = new List<FieldCell>();

        /// <summary> Whole grid prepared for running modificators on </summary>
        [HideInInspector] public FGenGraph<FieldCell, FGenPoint> Grid;

        /// <summary> Cells wit random order </summary>
        [NonSerialized] public List<FieldCell> RandomCells1;

        /// <summary> Cells wit random order second variation </summary>
        [NonSerialized] public List<FieldCell> RandomCells2;

        [HideInInspector] public List<FieldCell> DirtyCells = new List<FieldCell>();

        /// <summary> Debugging list to show generating process iteration by iteration </summary>
        //[SerializeField] [HideInInspector] private List<SpawnData> _spawnsQueue = new List<SpawnData>();
        [SerializeField] [HideInInspector] private List<FieldCell> spawnQueue = new List<FieldCell>();

        [HideInInspector] public List<FieldCell> ToInstantiate = new List<FieldCell>();

        [HideInInspector] public List<FieldCell> ToUpdate = new List<FieldCell>();

        [HideInInspector] [SerializeField] private int phantomGenerationNumer = 0;

        public void Initialize(FlexibleGeneratorSetup flex)
        {
            RefreshReferences(flex);
            Grid = new FGenGraph<FieldCell, FGenPoint>(true);
            LatestToSpawnCount = 0;
        }

        public void RefreshReferences(FlexibleGeneratorSetup flex)
        {
            FlexSetup = flex;
            if (DirtyCells == null) DirtyCells = new List<FieldCell>();
        }

        internal void SetAllDirty()
        {
            if (Grid != null)
                if (DirtyCells != null)
                {
                    DirtyCells.Clear();
                    for (int i = 0; i < GridCellsSave.Count; i++) DirtyCells.Add(GridCellsSave[i]);
                }
        }


        #region Setup Related

        internal void RestoreGrid()
        {
            Grid = new FGenGraph<FieldCell, FGenPoint>(true);

            if (GridCellsSave != null)
                if (GridCellsSave.Count > 0)
                    Grid.RestoreGrid(GridCellsSave);

            SetAllDirty();
        }

        public void CheckIfGridPrepared()
        {
            if (Grid == null)
            {
                Grid = new FGenGraph<FieldCell, FGenPoint>(true);
                Grid.RestoreGrid(GridCellsSave);
            }
        }

        public void CheckIfPrepared()
        {
            ParentPreparation.BEGIN_SetReferences(FlexSetup);
            //InstantiatedInfo.RefreshReferences();
        }

        /// <summary> Running rules on changed cells - not instanitating but holding info about what to instantiate and where </summary>
        /// <param name="instantiateAllInstant"> Set to false if you want to instantiate objects manually with InstantiateStep() method </param>
        public void Generate(bool instantiateAllInstant, bool tryAsync = false, MonoBehaviour checkForAsyncFail = null)
        {
            if (tryAsync == false)
            {
                GenerateNonAsyncStack(instantiateAllInstant);
            }
            else
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    GenerateNonAsyncStack(instantiateAllInstant);
                else
                {
                    GenerateAsyncStack();
                    if (checkForAsyncFail != null) checkForAsyncFail.StartCoroutine(IECheckAsyncFail(instantiateAllInstant, checkForAsyncFail));
                }
#else
                    GenerateAsyncStack();
                    if (checkForAsyncFail != null) checkForAsyncFail.StartCoroutine(IECheckAsyncFail(instantiateAllInstant, checkForAsyncFail));
#endif
            }
        }

        private void GenerateNonAsyncStack(bool instantiateAllInstant)
        {
            if (phantomGenerationNumer == 0)
            {
                PrepareRandomCells();
                RunModificatorsWithPreparation();
                ExtractInstantiation(GridCellsSave);
                phantomGenerationNumer = 1;
            }
            else
            {
                RunDirtyCells(true);
            }

            if (instantiateAllInstant) InstantiateAllRemaining();
        }

        GenerateAsyncThread asyncOperation = null;
        public bool AsyncIsRunning { get { if (asyncOperation == null) return false; if (asyncOperation.Fail) return false; return !asyncOperation.IsDone; } }
        private void GenerateAsyncStack()
        {
            if (asyncOperation != null) if (asyncOperation.IsDone == false || asyncOperation.Fail)
                {
                    UnityEngine.Debug.Log("[PGG Async] Async operation must finish before calling next async operation!");
                    return;
                }

            Action generatingActions;

            if (phantomGenerationNumer == 0)
            {
                generatingActions = new Action(GenerateInitial);
            }
            else
            {
                generatingActions = new Action(GenerateOnDirty);
            }

            asyncOperation = new GenerateAsyncThread(generatingActions);
            asyncOperation.Start();
        }

        System.Collections.IEnumerator IECheckAsyncFail(bool instantiateAllInstant, MonoBehaviour caller)
        {
            while (true)
            {
                if (asyncOperation == null) yield break;

                if (asyncOperation.Fail)
                {
                    UnityEngine.Debug.Log("[PGG Async] Error occured during async operation! Some nodes are probably using UnityEngine methods which is not allowed in async methods!");
                    UnityEngine.Debug.Log("[PGG Async] CAN'T GENERATE ASYNC - Check logs");
                    GenerateNonAsyncStack(instantiateAllInstant);
                    yield break;
                }
                else
                {
                    if (asyncOperation.IsDone)
                    {
                        if (instantiateAllInstant) InstantiateAllRemaining();
                        yield break;
                    }
                }

                yield return null;
            }
        }

        void GenerateInitial()
        {
            PrepareRandomCells();
            RunModificatorsWithPreparation();
            ExtractInstantiation(GridCellsSave);
            phantomGenerationNumer = 1;
        }

        void GenerateOnDirty()
        {
            RunDirtyCells(true);
        }


        /// <summary> Run rules on cells marked as modified or new </summary>
        public void RunDirtyCells(bool dirtySurround = true)
        {
            if (DirtyCells == null) return;
            if (DirtyCells.Count == 0) return;

            // Prepare main/surrounding cells to refresh
            IdendityAndDirtySurroundingCells(DirtyCells, dirtySurround);

            // Preparing to run rules in dirty cells with current state of grid
            ClearPhantomSpawnsInCells(DirtyCells);

            // If some dirty cells are erased from grid - don't run rules on them
            List<FieldCell> toRun = GetOnlyCellsInGrid(DirtyCells);

            // Compute rules
            RunFieldSetupPacks(RuntimeFieldSetup, toRun);

            // Refreshing spawning queue
            spawnQueue.Clear();

            // Transfering computed cells to extract instantiation data from
            PGGUtils.TransferFromListToList(toRun, ToInstantiate);
            ExtractInstantiation(ToInstantiate);

            // Clear used lists
            ToInstantiate.Clear();
            DirtyCells.Clear();

            phantomGenerationNumer += 1;
        }

        List<FieldCell> GetOnlyCellsInGrid(List<FieldCell> cells)
        {
            List<FieldCell> newCells = new List<FieldCell>();

            for (int i = 0; i < cells.Count; i++)
                if (cells[i].InTargetGridArea)
                {
                    newCells.Add(cells[i]);
                }
                else
                {
                    if (ToUpdate.Contains(cells[i]) == false)
                    {
                        cells[i].Clear();
                        ToUpdate.Add(cells[i]);
                    }
                }

            return newCells;
        }

        /// <summary> Clearing current spawns out of cells if already containing to avoid duplicates </summary>
        public void ClearPhantomSpawnsInCells(List<FieldCell> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].GetSpawnsJustInsideCell().Clear();
            }
        }

        public void ClearPhantomSpawnsInAllCells()
        {
            ClearPhantomSpawnsInCells(GridCellsSave);
        }


        /// <summary> Extracting spawn datas from cell spawns </summary>
        public void ExtractInstantiation(List<FieldCell> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (!spawnQueue.Contains(cells[i]))
                    ExtractInstantiation(cells[i]);
            }
        }

        public void ExtractInstantiation(FieldCell cell)
        {
            spawnQueue.Add(cell);
            //List<SpawnData> spawns = cell.CollectSpawns();
            //for (int s = 0; s < spawns.Count; s++)
            //{
            //    _spawnsQueue.Add(spawns[s]);
            //}
        }

        void IdendityAndDirtySurroundingCells(List<FieldCell> mainCells, bool dirtySurround)
        {
            if (Grid == null)
            {
                UnityEngine.Debug.Log("[PGG] No GRID!");
                return;
            }

            // Making copy of all cells, because we will remove elements from provided list
            List<FieldCell> mainCellsCopy = new List<FieldCell>();
            PGGUtils.TransferFromListToList(mainCells, mainCellsCopy);

            List<FieldCell> cellsToUpdate = new List<FieldCell>();

            // Finding already generated cells and queueing to update
            for (int i = mainCells.Count - 1; i >= 0; i--)
            {
                CellInstanitations datas = null;

                // If was generating already on this cell
                if (InstantiatedInfo.InstantiatedInfoPerCell.TryGetValue(mainCells[i].Pos, out datas))
                {
                    if (datas.Count > 0)
                    {
                        if (mainCells[i].InTargetGridArea)
                        {
                            cellsToUpdate.Add(mainCells[i]);
                        }
                        else
                        {
                            if (!ToUpdate.Contains(mainCells[i])) ToUpdate.Add(mainCells[i]);
                        }

                        mainCells.RemoveAt(i);
                    }
                }
            }

            if (dirtySurround)
            {
                List<FieldCell> surr = new List<FieldCell>();

                // Adding surrounding cells to be updated with target cells
                for (int i = 0; i < mainCellsCopy.Count; i++)
                {
                    surr.Clear();
                    Grid.GetDistanceSquare2DList(surr, mainCellsCopy[i], new Vector3Int(1, 1, 1));

                    for (int c = 0; c < surr.Count; c++)
                    {
                        if (surr[c].InTargetGridArea)
                            if (DirtyCells.Contains(surr[c]) == false)
                                if (cellsToUpdate.Contains(surr[c]) == false)
                                {
                                    cellsToUpdate.Add(surr[c]);
                                }
                    }
                }
            }

            ClearPhantomSpawnsInCells(cellsToUpdate);
            RunFieldSetupPacks(RuntimeFieldSetup, cellsToUpdate);
            PGGUtils.TransferFromListToList(cellsToUpdate, ToUpdate, true);
        }

        public void PrepareRandomCells()
        {
            RandomCells1 = IGeneration.GetRandomizedCells(Grid);
            RandomCells2 = IGeneration.GetRandomizedCells(Grid);
        }

        public void InitializeGenerating()
        {
            ParentPreparation.BEGIN_SetReferences(FlexSetup);
        }


        #endregion


        /// <summary>
        /// Computing target objects to be generated on provided grid (not instantiating - just information which object where should be instantiated)
        /// </summary>
        public void GenerateFullSchemeDataFromBeginning()
        {
            PrepareRandomCells();
            RunModificatorsWithPreparation();
            //PGGUtils.TransferFromListToList(Spawns, ToSpawn);
        }


        #region Instantiate Queue Related


        /// <summary> Instantiate on scene single phantom data to instanitate / destroy not needed cells / update them </summary>
        public InstantiatedData InstantiateStep()
        {
            InstantiatedData instantiated = new InstantiatedData();

            // Replacing / removing updated objects on the grid
            if (ToUpdate.Count > 0)
            {
                for (int t = 0; t < ToUpdate.Count; t++)
                {
                    ToUpdate[t].RefreshParentCellRef(Grid);
                    InstantiatedInfo.ReplaceWith(ToUpdate[t], this);
                }

                ToUpdate.Clear();
            }

            // Instantiating new objects
            if (spawnQueue.Count > 0)
            {
                bool generated = false;
                if (spawnQueue[0] != null)
                {
                    FieldCell c = spawnQueue[0];

                    var spawns = c.GetSpawnsJustInsideCell();
                    
                    for (int s = 0; s < spawns.Count; s++)
                    {
                        SpawnData spawn = spawns[s];

                        //if (spawn.Prefab != null || spawn.DontSpawnMainPrefab )
                        {
                            var container = InstantiatedInfo.GetContainerOf(spawn.OwnerMod, true);

                            if (spawn.OwnerCell == null) { spawn.OwnerCell = Grid.GetCell(spawn.OwnerCellPos); }
                            instantiated = InstantiatedData.InstantiateSpawnData(spawn, RuntimeFieldSetup, container.Transform, container.Transform.localToWorldMatrix);

                            generated = true;
                            if (generated) InstantiatedInfo.AcquireInstantiation(instantiated);
                        }
                    }
                }

                spawnQueue.RemoveAt(0);

                //if (generated) InstantiatedInfo.AcquireInstantiation(instantiated);
            }

            return instantiated;
        }


        /// <summary> Instantiate on scene all phantom data to instanitate / destroy not needed cells / update them </summary>
        public void InstantiateAllRemaining()
        {
            int safety = int.MaxValue;

            while (WaitingToBeSpawned)
            {
                InstantiateStep();
                safety -= 1;
                if (safety < 0) { UnityEngine.Debug.Log("[PGG] Safety break to prevent crash"); break; }
            }
        }


        #endregion


        #region Generating Scheme Data and Updating Progressively



        private FieldCell GetCell(Vector3Int pos)
        {
            FieldCell cell = Grid.AddCell(pos);
            Grid.ApproveCell(cell);
            cell.GetCount += 1;
            return cell;
        }

        /// <summary> Adding new cell to the grid, if already in grid then just updating it </summary>
        public void AddCell(Vector3Int newCell)
        {
            UpdateCell(GetCell(newCell));
        }

        /// <summary> Adding new cells to the grid and running rules, if already in grid then just updating cells </summary>
        public void AddCells(List<Vector3Int> cells, bool updateIfExists = true)
        {
            List<FieldCell> cellsToCheck = new List<FieldCell>();

            if (updateIfExists)
            {
                // Acquiring or generating new cells on the grid
                for (int i = 0; i < cells.Count; i++)
                {
                    cellsToCheck.Add(GetCell(cells[i]));
                }
            }
            else
            {
                for (int i = 0; i < cells.Count; i++)
                {
                    var cell = Grid.GetCell(cells[i], false);
                    if (FGenerators.CheckIfIsNull(cell) || cell.InTargetGridArea == false)
                    {
                        cellsToCheck.Add(GetCell(cells[i]));
                    }
                }
            }

            UpdateCells(cellsToCheck);
        }

        /// <summary> Updating cell on current scheme, remembering changes in Diffs list </summary>
        public void UpdateCell(FieldCell cell)
        {
            //if ( GridCellsSave.Contains(cell) == false)
            if (cell.GetCount <= 1)
            {
                GridCellsSave.Add(cell);
                cell.GetCount = 2;
            }

            if (!DirtyCells.Contains(cell)) DirtyCells.Add(cell);
        }

        /// <summary> Updating cell on current scheme, remembering changes in Diffs list </summary>
        public void UpdateCells(List<FieldCell> cells)
        {
            for (int i = 0; i < cells.Count; i++) UpdateCell(cells[i]);
        }

        /// <summary> Removing cell from the grid and updating surroundings </summary>
        public void RemoveCell(Vector3Int pos)
        {
            FieldCell cell = null;

            if (Grid != null) cell = Grid.GetCell(pos, false);
            else { for (int i = 0; i < GridCellsSave.Count; i++) if (GridCellsSave[i].Pos == pos) { cell = GridCellsSave[i]; break; } }

            if (FGenerators.CheckIfExist_NOTNULL(cell))
            {
                GridCellsSave.Remove(cell);
                if (Grid != null) Grid.RemoveCell(cell);
                cell.Clear();
                if (DirtyCells.Contains(cell) == false) DirtyCells.Add(cell);
            }
        }


        /// <summary> Removing cells from the grid and updating surroundings </summary>
        public void RemoveCells(List<Vector3Int> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                RemoveCell(cells[i]);
            }
        }

        /// <summary> Clearing all cells from memory pernamently </summary>
        internal void ClearAll()
        {
            Grid = new FGenGraph<FieldCell, FGenPoint>(true);
            GridCellsSave.Clear();
            ToUpdate.Clear();
            ToInstantiate.Clear();
            spawnQueue.Clear();
            DirtyCells.Clear();
            phantomGenerationNumer = 0;
            InstantiatedInfo.Clear(true);
        }


        #endregion


        #region Execution of Procedural Logics setted up by FieldSetups


        /// <summary>
        /// Computing all FieldSetup logics with preparation settings
        /// resulting in cells and data ready for instantiation and debugging
        /// </summary>
        public void RunModificatorsWithPreparation()
        {
            //ParentPreparation.FieldVariablesSetCustom();

            //ParentPreparation.PrepareScaledGrids();
            //ParentPreparation.PrepareOptionalCheckers();

            ParentPreparation.PrepareGuides();
            ParentPreparation.RefreshSelfInfjection();
            ParentPreparation.PreparePresetVariables();

            #region TODO If using isolated grid injection then preparing separated grid for each definition
            #endregion


            ParentPreparation.RunPreInstructions();
            ParentPreparation.RunTemporaryPreInjections(this);

            ParentPreparation.RunMainInstructions();
            RunFieldSetupPacks(RuntimeFieldSetup);

            ParentPreparation.RunPostCellsRefill();
            ParentPreparation.RunTemporaryPostInjections(this);
            ParentPreparation.RunPostIntructions();


            #region TODO Apply Isolated Grid Spawns

            #endregion


            ParentPreparation.ClearTemporaryInjections();

            // Now scheme is ready for instantiation
        }

        public void RunFieldSetupPacks(FieldSetup setup, List<FieldCell> selectiveCells = null)
        {
            for (int p = 0; p < setup.ModificatorPacks.Count; p++)
            {
                ModificatorsPack pack = setup.ModificatorPacks[p];
                if (pack == null) continue;
                if (pack.DisableWholePackage) continue;

                RunModificatorPack(pack, selectiveCells);
            }
        }

        public void RunModificatorPack(ModificatorsPack pack, List<FieldCell> selectiveCells = null)
        {
            pack.PrepareSeed();
            List<FieldModification> toRun = pack.GetModListToRun(ParentPreparation);
            for (int i = 0; i < toRun.Count; i++) RunModification(toRun[i], selectiveCells);
        }


        public void RunModification(FieldModification mod, List<FieldCell> selectiveCells = null)
        {
            if (mod.VariantOf == null)
                mod.ModifyGraph(this, null, selectiveCells);
            else // Variant
                mod.VariantOf.ModifyGraph(this, mod, selectiveCells);
        }

        public void SetWithGrid(FGenGraph<FieldCell, FGenPoint> grid)
        {
            GridCellsSave.Clear();
            Grid = grid;

            for (int i = 0; i < grid.AllCells.Count; i++)
            {
                grid.AllCells[i].InTargetGridArea = true;
                GridCellsSave.Add(grid.AllCells[i]);
            }

            SetAllDirty();
        }


        #endregion


        #region Async Related

        public bool IsInstantiationCoroutineRunning { get { if (coroutineInstantiation == null) return false; return coroutineInstanceIsRunning; } }
        /// <summary> Returns true if computing rules or instantiating objects </summary>
        public bool IsGenerating { get { return IsInstantiationCoroutineRunning || AsyncIsRunning; } }
        public bool FinishedGenerating { get { return InstantiationProgress >=1f; } }
        public float InstantiationProgress { get { if (IsInstantiationCoroutineRunning == false) return 1f; if (LatestToSpawnCount <= 0f) return 1f; return 1f - ((float)ToSpawnCount / (float)LatestToSpawnCount); } }

        public int LatestToSpawnCount { get; private set; }
        private Coroutine coroutineInstantiation = null;
        private bool coroutineInstanceIsRunning = false;

        public void InstantiateInCourutine(MonoBehaviour caller, float instantiationMaxSecondsDelay, int minimumInstantiationsInFrame)
        {
            if (coroutineInstanceIsRunning == false)
            {
                //caller.StopCoroutine(coroutineInstantiation); else coroutineInstanceIsRunning = false;
                LatestToSpawnCount = ToSpawnCount;
                coroutineInstanceIsRunning = true;
                coroutineInstantiation = caller.StartCoroutine(IEInstantiateInCourutine(instantiationMaxSecondsDelay, minimumInstantiationsInFrame));
            }
        }

        private System.Collections.IEnumerator IEInstantiateInCourutine(float instantiationMaxSecondsDelay, int minimumInstantiationsInFrame)
        {
            yield return null;
            while (AsyncIsRunning) { yield return null; }

            coroutineInstanceIsRunning = true;

            if (minimumInstantiationsInFrame < 1) minimumInstantiationsInFrame = 1;
            if (instantiationMaxSecondsDelay < 0) instantiationMaxSecondsDelay = 0;

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Reset();

            int iterations = 0;
            int allIterations = 0;

            while (WaitingToBeSpawned)
            {
                watch.Start();
                InstantiateStep();
                watch.Stop();

                //UnityEngine.Debug.Log(allIterations + " watch time = " + (watch.ElapsedMilliseconds * 0.001f) + " ml = " + watch.ElapsedMilliseconds);
                iterations += 1;
                allIterations += 1;

                if (iterations > minimumInstantiationsInFrame)
                    // Instantiation takes too much time - skipping frame 
                    if (watch.ElapsedMilliseconds * 0.001f > instantiationMaxSecondsDelay)
                    {
                        watch.Reset();
                        iterations = 0;
                        yield return null;
                    }
            }

            coroutineInstanceIsRunning = false;
            yield break;
        }

        #endregion

    }

}