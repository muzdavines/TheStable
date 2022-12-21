using FIMSpace.Generating.Rules;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class FieldModification : ScriptableObject
    {
        public void ModifyGraphCell(FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, bool makeNotDestroyable = false, Vector3? desiredDirection = null, bool isGuide = false, bool ignoreRestrictions = false)
        {

            for (int s = 0; s < Spawners.Count; s++) // Checking all mod's spawners
            {
                var spawner = Spawners[s];

                if (spawner.OnScalledGrid > 1)
                {
                    if (spawner.OnScalledGrid != grid.ReferenceScale)
                    {
                        if (grid.SubGraphs != null)
                        {
                            if (grid.SubGraphs.Count != 0)
                            {
                                var ngraph = grid.GetCorrespondingSubGraph(spawner.OnScalledGrid);
                                if (ngraph == null) break;
                                var nCell = grid.GetCorrespondingSubGraphCell(cell, ngraph);
                                if (nCell == null) { /*UnityEngine.Debug.Log("NI MA");*/ break; } /*else UnityEngine.Debug.Log("JEST");*/
                                grid = ngraph;
                                cell = nCell;
                            }
                        }
                    }
                }


                // If spawner is not enabled we skip it
                if (spawner.Enabled == false) { break; }

                // Refreshing all rules before checking cell
                for (int i = 0; i < spawner.Rules.Count; i++)
                {
                    if (spawner.Rules[i] == null) continue;
                    spawner.Rules[i].SetOwner(spawner);
                    spawner.Rules[i].ResetRule(grid, preset);
                }

                PGGUtils.CheckForNulls<SpawnRuleBase>(spawner.Rules);
                //for (int i = spawner.Rules.Count - 1; i >= 0; i--) if (spawner.Rules[i] == null) spawner.Rules.RemoveAt(i);

                if (isGuide)
                {
                    // If it's guide we must re-arrange global rules now
                    List<SpawnRuleBase> pre_tempGlobalRulesPre = _tempGlobalRulesPre;
                    List<SpawnRuleBase> pre_tempGlobablRulesPost = _tempGlobablRulesPost;
                    List<SpawnRuleBase> pre_tempGlobablRulesOnConditions = _tempGlobablRulesOnConditions;

                    _tempGlobalRulesPre = new List<SpawnRuleBase>();
                    _tempGlobablRulesPost = new List<SpawnRuleBase>();
                    _tempGlobablRulesOnConditions = new List<SpawnRuleBase>();

                    PreGenerateSpawnersRefresh(spawner.Parent.Spawners, grid, preset, true);
                    PreGenerateSpawnersRefresh(spawner.Parent.SubSpawners, grid, preset, false);

                    spawner.RunSpawnerOnCell(this, preset, cell, grid, desiredDirection == null ? Vector3.zero : desiredDirection.Value, null, false, ignoreRestrictions);

                    _tempGlobalRulesPre = pre_tempGlobalRulesPre;
                    _tempGlobablRulesPost = pre_tempGlobablRulesPost;
                    _tempGlobablRulesOnConditions = pre_tempGlobablRulesOnConditions;
                }
                else
                {
                    spawner.RunSpawnerOnCell(this, preset, cell, grid, desiredDirection == null ? Vector3.zero : desiredDirection.Value, null, false, ignoreRestrictions);
                }

            }

        }


        /// <summary>
        /// Preparing field variables for spawn rules
        /// </summary>
        internal void PrepareVariablesWith(FieldSetup preset, bool getParentIfExists = false, InjectionSetup overrider = null)
        {
            if (Spawners == null) Spawners = new List<FieldSpawner>();

            for (int s = 0; s < Spawners.Count; s++)
            {
                if (FGenerators.CheckIfIsNull(Spawners[s])) continue;

                for (int r = 0; r < Spawners[s].Rules.Count; r++)
                {
                    if (FGenerators.CheckIfIsNull(Spawners[s].Rules[r])) continue;
                    if (Spawners[s].Rules[r].VariablesPrepared) continue;

                    var spawnerVariables = Spawners[s].Rules[r].GetVariables();
                    if (spawnerVariables == null) continue;

                    for (int vr = 0; vr < spawnerVariables.Count; vr++)
                    {
                        var spawnerVar = spawnerVariables[vr];

                        if (string.IsNullOrEmpty(spawnerVar.name)) continue;

                        FieldSetup parent = preset;
                        ModificatorsPack pack = null;

                        if (getParentIfExists)
                        {
                            parent = TryGetParentSetup();
                            if (parent == null) parent = preset;
                        }

                        pack = ParentPack;
                        if (parent) if (ParentPack == parent.RootPack) pack = null;

                        var pVar = parent.GetVariable(spawnerVar.name);

                        if (pack)
                        {
                            if (FGenerators.CheckIfIsNull(pVar))
                            {
                                pVar = pack.GetVariable(spawnerVar.name);
                            }
                        }

                        if (FGenerators.CheckIfIsNull(pVar)) continue;

                        bool overrided = false;
                        if (FGenerators.CheckIfExist_NOTNULL(overrider))
                        {
                            if (overrider.OverrideVariables)
                            {
                                if (overrider.Overrides != null)
                                    for (int ov = 0; ov < overrider.Overrides.Count; ov++)
                                    {
                                        if (overrider.Overrides[ov].Name == pVar.Name)
                                        {
                                            pVar.Prepared = true;
                                            spawnerVar.reference = overrider.Overrides[ov];
                                            overrided = true;
                                            Spawners[s].Rules[r].VariablesPrepared = true;
                                            break;
                                        }
                                    }
                            }
                        }

                        if (!overrided)
                        {
                            spawnerVar.reference = pVar;
                        }
                    }
                }
            }
        }

        internal List<SpawnRuleBase> _tempGlobalRulesPre = new List<SpawnRuleBase>();
        internal List<SpawnRuleBase> _tempGlobablRulesPost = new List<SpawnRuleBase>();
        internal List<SpawnRuleBase> _tempGlobablRulesOnConditions = new List<SpawnRuleBase>();

        private void ArrangeGlobalRules(FieldSpawner spawner, List<SpawnRuleBase> preC, List<SpawnRuleBase> postC, List<SpawnRuleBase> onCondC)
        {
            if (spawner.Enabled == false) return;

            for (int i = 0; i < spawner.Rules.Count; i++)
            {
                if (spawner.Rules[i] == null) continue;
                if (spawner.Rules[i].Enabled == false || spawner.Rules[i].Ignore) continue;
                if (spawner.Rules[i].Global == false) continue;

                ISpawnProcedureType t = spawner.Rules[i] as ISpawnProcedureType;
                if (t != null)
                    if (t.Type == SpawnRuleBase.EProcedureType.OnConditionsMet)
                    {
                        onCondC.Add(spawner.Rules[i]);
                        continue;
                    }

                bool post = false;
                if (spawner.Rules.Count > 1) if (i == spawner.Rules.Count - 1) post = true;
                if (!post) if (i > spawner.Rules.Count / 2) post = true;

                if (post == false)
                    preC.Add(spawner.Rules[i]);
                else
                    postC.Add(spawner.Rules[i]);
            }
        }

        /// <summary>
        /// Running all mod's rules algorithms on all cells
        /// </summary>
        public virtual void ModifyGraph(FieldSetup preset, FGenGraph<FieldCell, FGenPoint> grid, List<FieldCell> randomizedCells, List<FieldCell> randomizedCells2, FieldModification childMod = null)
        {
            if (grid != null)
            {
                // Arranging global rules
                _tempGlobalRulesPre.Clear();
                _tempGlobablRulesPost.Clear();
                _tempGlobablRulesOnConditions.Clear();

                PreGenerateSpawnersRefresh(Spawners, grid, preset, true);
                PreGenerateSpawnersRefresh(SubSpawners, grid, preset, false);

                for (int s = 0; s < Spawners.Count; s++) // Checking all mod's spawners on all cells
                {
                    var spawner = Spawners[s];

                    // If spawner is not enabled we skip it
                    if (spawner.Enabled == false) continue;
                    if (spawner.Repeat <= 0) spawner.Repeat = 1;


                    for (int rep = 0; rep < spawner.Repeat; rep++)
                    {
                        spawner._currentRepeat = rep;
                        // TODO: while(rules filled -> break; else rulesToDo[] -> if min-max not met -> do until met

                        // Refreshing all rules before checking all available cells
                        for (int i = 0; i < spawner.Rules.Count; i++)
                        {
                            if (spawner.Rules[i] == null) continue;
                            spawner.Rules[i].ResetRule(grid, preset);
                        }

                        // Global Parent Package Rules
                        if (spawner.UseParentPackageRules) if (spawner.HaveParentPackageRules())
                            {
                                for (int i = 0; i < spawner.Parent.ParentPack.CallOnAllSpawners.Rules.Count; i++)
                                {
                                    var rl = spawner.Parent.ParentPack.CallOnAllSpawners.Rules[i];
                                    rl.ResetRule(grid, preset);
                                    rl.DisableDrawingGlobalSwitch = true;
                                }
                            }


                        // Refreshing global rules
                        for (int i = 0; i < _tempGlobalRulesPre.Count; i++) _tempGlobalRulesPre[i].ResetRule(grid, preset);
                        for (int i = 0; i < _tempGlobablRulesPost.Count; i++) _tempGlobablRulesPost[i].ResetRule(grid, preset);
                        for (int i = 0; i < _tempGlobablRulesOnConditions.Count; i++) _tempGlobablRulesOnConditions[i].ResetRule(grid, preset);

                        for (int i = spawner.Rules.Count - 1; i >= 0; i--) if (spawner.Rules[i] == null) spawner.Rules.RemoveAt(i);

                        if (spawner.OnScalledGrid <= 1)
                        {
                            // Running spawner with it's rules inside over all cells
                            if (spawner.CellCheckMode == FieldSpawner.ESR_CellOrder.Ordered)
                            {
                                for (int i = 0; i < grid.AllApprovedCells.Count; i++) // Go through all cells on grid with [s] Rule
                                {
                                    if (string.IsNullOrEmpty(preset.DontSpawnOn) == false) if (FGenerators.CheckIfExist_NOTNULL(SpawnRuleBase.CellSpawnsHaveTag(grid.AllApprovedCells[i], preset.DontSpawnOn))) continue;
                                    spawner.RunSpawnerOnCell(this, preset, grid.AllApprovedCells[i], grid, Vector3.zero, childMod);
                                }
                            }
                            else if (spawner.CellCheckMode == FieldSpawner.ESR_CellOrder.Reversed)
                            {
                                for (int i = grid.AllApprovedCells.Count - 1; i >= 0; i--) // Go through all cells on grid with [s] Rule
                                {
                                    if (string.IsNullOrEmpty(preset.DontSpawnOn) == false) if (FGenerators.CheckIfExist_NOTNULL(SpawnRuleBase.CellSpawnsHaveTag(grid.AllApprovedCells[i], preset.DontSpawnOn))) continue;
                                    spawner.RunSpawnerOnCell(this, preset, grid.AllApprovedCells[i], grid, Vector3.zero, childMod);
                                }
                            }
                            else if (spawner.CellCheckMode == FieldSpawner.ESR_CellOrder.Random)
                            {
                                for (int i = 0; i < randomizedCells.Count; i++) // Go through all cells on grid with [s] Rule
                                {
                                    if (string.IsNullOrEmpty(preset.DontSpawnOn) == false) if (FGenerators.CheckIfExist_NOTNULL(SpawnRuleBase.CellSpawnsHaveTag(randomizedCells[i], preset.DontSpawnOn))) continue;
                                    spawner.RunSpawnerOnCell(this, preset, randomizedCells[i], grid, Vector3.zero, childMod);
                                }
                            }
                            else if (spawner.CellCheckMode == FieldSpawner.ESR_CellOrder.RandomReversed)
                            {
                                for (int i = randomizedCells.Count - 1; i >= 0; i--) // Go through all cells on grid with [s] Rule
                                {
                                    if (string.IsNullOrEmpty(preset.DontSpawnOn) == false) if (FGenerators.CheckIfExist_NOTNULL(SpawnRuleBase.CellSpawnsHaveTag(randomizedCells[i], preset.DontSpawnOn))) continue;
                                    spawner.RunSpawnerOnCell(this, preset, randomizedCells[i], grid, Vector3.zero, childMod);
                                }
                            }
                            else if (spawner.CellCheckMode == FieldSpawner.ESR_CellOrder.Random2)
                            {
                                for (int i = 0; i < randomizedCells2.Count; i++) // Go through all cells on grid with [s] Rule
                                {
                                    if (string.IsNullOrEmpty(preset.DontSpawnOn) == false) if (FGenerators.CheckIfExist_NOTNULL(SpawnRuleBase.CellSpawnsHaveTag(randomizedCells2[i], preset.DontSpawnOn))) continue;
                                    spawner.RunSpawnerOnCell(this, preset, randomizedCells2[i], grid, Vector3.zero, childMod);
                                }
                            }
                            else if (spawner.CellCheckMode == FieldSpawner.ESR_CellOrder.Random2Reversed)
                            {
                                for (int i = randomizedCells2.Count - 1; i >= 0; i--) // Go through all cells on grid with [s] Rule
                                {
                                    if (string.IsNullOrEmpty(preset.DontSpawnOn) == false) if (FGenerators.CheckIfExist_NOTNULL(SpawnRuleBase.CellSpawnsHaveTag(randomizedCells2[i], preset.DontSpawnOn))) continue;
                                    spawner.RunSpawnerOnCell(this, preset, randomizedCells2[i], grid, Vector3.zero, childMod);
                                }
                            }
                            else if (spawner.CellCheckMode == FieldSpawner.ESR_CellOrder.TotalRandom)
                            {
                                List<FieldCell> randCells = IGeneration.GetRandomizedCells(grid);
                                for (int i = 0; i < randCells.Count; i++) // Go through all cells on grid with [s] Rule
                                {
                                    if (string.IsNullOrEmpty(preset.DontSpawnOn) == false) if (FGenerators.CheckIfExist_NOTNULL(SpawnRuleBase.CellSpawnsHaveTag(randCells[i], preset.DontSpawnOn))) continue;
                                    spawner.RunSpawnerOnCell(this, preset, randCells[i], grid, Vector3.zero, childMod);
                                }
                            }

                        }
                        else // Running on scalled grid
                        {
                            var scGrid = preset.GetScaledGrid(grid, spawner.OnScalledGrid, false);
                            //UnityEngine.Debug.Log("Scaled grid! " + spawner.OnScalledGrid + " cells = " + scGrid.AllCells.Count);

                            if (spawner.CellCheckMode == FieldSpawner.ESR_CellOrder.TotalRandom)
                            {
                                List<FieldCell> randCells = IGeneration.GetRandomizedCells(scGrid);
                                for (int i = 0; i < randCells.Count; i++) // Go through all cells on grid with [s] Rule
                                {
                                    if (string.IsNullOrEmpty(preset.DontSpawnOn) == false) if (FGenerators.CheckIfExist_NOTNULL(SpawnRuleBase.CellSpawnsHaveTag(randCells[i], preset.DontSpawnOn))) continue;
                                    spawner.RunSpawnerOnCell(this, preset, randCells[i], scGrid, Vector3.zero, childMod);
                                    //if (sp is null == false) UnityEngine.Debug.Log("Generated Spawn! " + sp.Enabled + " sp pf = " + sp.Prefab + "  " + sp.OwnerMod);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < scGrid.AllApprovedCells.Count; i++) // Go through all cells on grid with [s] Rule
                                {
                                    if (string.IsNullOrEmpty(preset.DontSpawnOn) == false) if (FGenerators.CheckIfExist_NOTNULL(SpawnRuleBase.CellSpawnsHaveTag(scGrid.AllApprovedCells[i], preset.DontSpawnOn))) continue;
                                    spawner.RunSpawnerOnCell(this, preset, scGrid.AllApprovedCells[i], scGrid, Vector3.zero, childMod);
                                }
                            }
                        }
                    }
                }

                PostCall(grid, preset, Spawners);
                PostCall(grid, preset, SubSpawners);
            }
        }

        public void RefreshSpawnersOwners()
        {
            for (int s = 0; s < Spawners.Count; s++)
            {
                if (Spawners[s] == null) continue;
                Spawners[s].RefreshRulesOwner();
            }
        }

        void PostCall(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset, List<FieldSpawner> spawners)
        {
            for (int s = 0; s < spawners.Count; s++) // Post call
            {
                var spawner = spawners[s];
                if (spawner == null) continue;
                if (spawner.Enabled == false) continue;
                for (int r = 0; r < spawner.Rules.Count; r++)
                {
                    var rle = spawner.Rules[r];
                    if (rle == null) continue;
                    if (rle.Enabled == false) continue;
                    rle.OnGeneratingCompleated(grid, preset);
                }
            }
        }

        /// <summary>
        /// Running single field modificator's rules algorithms on all cells marked as dirty
        /// </summary>
        public virtual void ModifyGraph(CellsController cellsContr, FieldModification childModIfVariant = null, List<FieldCell> selectiveCells = null, bool resetRules = true)
        {
            var preset = cellsContr.RuntimeFieldSetup;
            var grid = cellsContr.Grid;


            // Arranging global rules
            _tempGlobalRulesPre.Clear();
            _tempGlobablRulesPost.Clear();
            _tempGlobablRulesOnConditions.Clear();


            PreGenerateSpawnersRefresh(Spawners, grid, preset, true);
            PreGenerateSpawnersRefresh(SubSpawners, grid, preset, false);


            for (int s = 0; s < Spawners.Count; s++) // Checking all mod's spawners on all cells
            {
                var spawner = Spawners[s];

                // If spawner is not enabled we skip it
                if (spawner.Enabled == false) continue;
                if (spawner.Repeat <= 0) spawner.Repeat = 1;

                for (int rep = 0; rep < spawner.Repeat; rep++)
                {
                    spawner._currentRepeat = rep;

                    // Refreshing all rules before checking all available cells
                    if (resetRules)
                        for (int i = 0; i < spawner.Rules.Count; i++)
                        {
                            if (spawner.Rules[i] == null) continue;
                            spawner.Rules[i].ResetRule(grid, preset);
                        }

                    // Global Parent Package Rules
                    if (resetRules)
                        if (spawner.UseParentPackageRules)
                            if (spawner.HaveParentPackageRules())
                            {
                                for (int i = 0; i < spawner.Parent.ParentPack.CallOnAllSpawners.Rules.Count; i++)
                                {
                                    var rl = spawner.Parent.ParentPack.CallOnAllSpawners.Rules[i];
                                    rl.ResetRule(grid, preset);
                                    rl.DisableDrawingGlobalSwitch = true;
                                }
                            }

                    // Refreshing global rules
                    if (resetRules)
                    {
                        for (int i = 0; i < _tempGlobalRulesPre.Count; i++) _tempGlobalRulesPre[i].ResetRule(grid, preset);
                        for (int i = 0; i < _tempGlobablRulesPost.Count; i++) _tempGlobablRulesPost[i].ResetRule(grid, preset);
                        for (int i = 0; i < _tempGlobablRulesOnConditions.Count; i++) _tempGlobablRulesOnConditions[i].ResetRule(grid, preset);
                        //for (int i = 0; i < preGRules.Count; i++) preGRules[i].ResetRule(grid, preset);
                        //for (int i = 0; i < postGRules.Count; i++) postGRules[i].ResetRule(grid, preset);
                        //for (int i = 0; i < onCondGRules.Count; i++) onCondGRules[i].ResetRule(grid, preset);
                    }

                    PGGUtils.CheckForNulls(spawner.Rules);

                    if (selectiveCells == null)
                    {
                        if (spawner.OnScalledGrid <= 1)
                        {
                            var cellToRun = GetGridCellsToExecuteSpawnersOn(spawner, cellsContr);
                            ExecuteRuleOnCells(spawner, cellsContr, cellToRun, spawner.IsExecutionReversed(), childModIfVariant);
                        }
                        else // Running on scalled grid
                        {
                            var scGrid = preset.GetScaledGrid(grid, spawner.OnScalledGrid, false);
                            var cellToRun = scGrid.AllApprovedCells;
                            if (spawner.CellCheckMode == FieldSpawner.ESR_CellOrder.TotalRandom) cellToRun = IGeneration.GetRandomizedCells(scGrid);

                            ExecuteRuleOnCells(spawner, cellsContr, cellToRun, spawner.IsExecutionReversed(), childModIfVariant);
                        }
                    }
                    else
                    {
                        ExecuteRuleOnCells(spawner, cellsContr, selectiveCells, false, childModIfVariant);
                    }

                }
            }


            PostCall(grid, preset, Spawners);
            PostCall(grid, preset, SubSpawners);

        }

        /// <summary>
        /// <param name="childMod"> Needed if executing variant </param>
        /// </summary>
        void ExecuteRuleOnCells(FieldSpawner spawner, CellsController cellsContr, List<FieldCell> cells, bool reverse = false, FieldModification childMod = null)
        {
            if (reverse == false)
            {
                for (int i = 0; i < cells.Count; i++) // Go through all cells on grid with [s] Rule
                {
                    if (string.IsNullOrEmpty(cellsContr.RuntimeFieldSetup.DontSpawnOn) == false) if (FGenerators.CheckIfExist_NOTNULL(SpawnRuleBase.CellSpawnsHaveTag(cells[i], cellsContr.RuntimeFieldSetup.DontSpawnOn))) continue;
                    SpawnData data = spawner.RunSpawnerOnCell(this, cellsContr.RuntimeFieldSetup, cells[i], cellsContr.Grid, Vector3.zero, childMod, false, false, cellsContr.AsyncIsRunning);
                    //if (FGenerators.CheckIfExist_NOTNULL(data)) scheme._DebugSpawns.Add(data);
                }
            }
            else
            {
                for (int i = cells.Count - 1; i >= 0; i--) // Go through all cells on grid with [s] Rule
                {
                    if (string.IsNullOrEmpty(cellsContr.RuntimeFieldSetup.DontSpawnOn) == false) if (FGenerators.CheckIfExist_NOTNULL(SpawnRuleBase.CellSpawnsHaveTag(cells[i], cellsContr.RuntimeFieldSetup.DontSpawnOn))) continue;
                    SpawnData data = spawner.RunSpawnerOnCell(this, cellsContr.RuntimeFieldSetup, cells[i], cellsContr.Grid, Vector3.zero, childMod, false, false, cellsContr.AsyncIsRunning);
                    //if (FGenerators.CheckIfExist_NOTNULL(data)) scheme._DebugSpawns.Add(data);
                }
            }
        }


        void PreGenerateSpawnersRefresh(List<FieldSpawner> spawners, FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset, bool manageGlobalRules)
        {
            for (int s = 0; s < spawners.Count; s++)
            {
                var spawner = spawners[s];
                if (spawner == null) continue;

                spawner.RefreshRulesOwner();

                if (manageGlobalRules)
                {
                    // Preparing global rules for all spawners
                    ArrangeGlobalRules(spawner, _tempGlobalRulesPre, _tempGlobablRulesPost, _tempGlobablRulesOnConditions);
                }

                if (spawner.wasPreGeneratingPrepared) continue;
                spawner.PreGenerating();

                for (int i = 0; i < spawner.Rules.Count; i++)
                {
                    if (spawner.Rules[i] == null) continue;
                    spawner.Rules[i].PreGenerateResetRule(grid, preset, spawner);
                }
            }
        }


        /// <summary>
        /// Getting cells for running rules on
        /// </summary>
        public List<FieldCell> GetGridCellsToExecuteSpawnersOn(FieldSpawner spawner, CellsController cellsContr)
        {
            if (spawner.CellCheckMode == FieldSpawner.ESR_CellOrder.Ordered ||
                spawner.CellCheckMode == FieldSpawner.ESR_CellOrder.Reversed)
            {
                return cellsContr.Grid.AllApprovedCells;
            }
            else
            if (spawner.CellCheckMode == FieldSpawner.ESR_CellOrder.Random ||
                spawner.CellCheckMode == FieldSpawner.ESR_CellOrder.RandomReversed)
            {
                return cellsContr.RandomCells1;
            }
            else
            if (spawner.CellCheckMode == FieldSpawner.ESR_CellOrder.Random2 ||
                spawner.CellCheckMode == FieldSpawner.ESR_CellOrder.Random2Reversed)
            {
                return cellsContr.RandomCells2;
            }
            else if (spawner.CellCheckMode == FieldSpawner.ESR_CellOrder.TotalRandom)
            {
                return IGeneration.GetRandomizedCells(cellsContr.Grid);
            }

            return cellsContr.Grid.AllApprovedCells;
        }

    }
}