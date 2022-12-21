using FIMSpace.Generating.Checker;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
using FIMSpace.FEditor;
#endif

namespace FIMSpace.Generating
{
    /// <summary>
    /// Class containing prepared data to start procedural generation algorithm for a single FieldSetup
    /// It can define injections, ignored modificators, cell guides and more
    /// </summary>
    [System.Serializable]
    public class GeneratingPreparation
    {
        [System.NonSerialized] public FlexibleGeneratorSetup FlexSetup;
        //public int InitialSeed = 0;
        public FieldSetup RuntimeFieldSetup { get { if (runtimeFieldSetup == null) runtimeFieldSetup = GenerateRuntimeFieldSetup(); return runtimeFieldSetup; } }
        public FieldSetup ParentFieldSetup { get { return FlexSetup.FieldPreset; } }
        public CellsController gen { get { return FlexSetup.CellsController; } }

        /// <summary> Duplicated TargetFieldSetup for handling variables, injections independently from the project file preset </summary>
        /*[HideInInspector] *//*[SerializeField] */
        private FieldSetup runtimeFieldSetup;

        //public List<CheckerField> AdditionalCheckers;
        //public bool RunRules = true;

        public List<InjectionSetup> Injections;
        public List<SpawnInstruction> CellInstructions;

        //public List<FieldModification> DisabledMods = new List<FieldModification>();
        //public List<ModificatorsPack> IgnoredPacksForGenerating = new List<ModificatorsPack>();
        //public List<FieldModification> IgnoredModificationsForGenerating = new List<FieldModification>();

        //public List<FieldVariable> SwitchVariables = new List<FieldVariable>();
        //public List<FieldVariable> SwitchPackVariables = new List<FieldVariable>();

        //public List<CheckerField> OptionalCheckers = new List<CheckerField>();

        //public FieldSetupComposition Composition;
        /// <summary>
        /// Multiple depth serialize classes data lost prevention, that's why Composition is not 'GeneratingPreparation's variable
        /// </summary>
        public FieldSetupComposition Composition { get { if (FlexSetup == null) return null; PGGFlexibleGeneratorBase gen = (PGGFlexibleGeneratorBase)FlexSetup.ParentObject; if (gen == null) return null; return gen.Composition; } }


        #region Temporaries

        public List<FieldCell> _newCells;

        public void Initialize(FlexibleGeneratorSetup flex)
        {
            RefreshReferences(flex);
        }

        public void RefreshReferences(FlexibleGeneratorSetup flex)
        {
            FlexSetup = flex;
        }

        public void ReGenerateRuntimeFieldSetup()
        {
            runtimeFieldSetup = GenerateRuntimeFieldSetup();
        }

        public FieldSetup GenerateRuntimeFieldSetup()
        {
            FieldSetup setup;

            if (Composition != null && Composition.Prepared && Composition.OverrideEnabled)
            {
                setup = Composition.GetOverridedSetup();
            }
            else setup = ParentFieldSetup.GenerateRuntimeInstance();

            return setup;
        }


        /// <summary>
        /// Setting references, generating temporary copies etc.
        /// </summary>
        public void BEGIN_SetReferences(FlexibleGeneratorSetup flex)
        {
            RefreshReferences(flex);

            //if (runtimeFieldSetup == null ||( runtimeFieldSetup.InstantiatedOutOf.GetInstanceID() != ParentFieldSetup.GetInstanceID()))
            {
                runtimeFieldSetup = GenerateRuntimeFieldSetup();
            }

            _newCells = new List<FieldCell>();
        }

        internal FGenGraph<FieldCell, FGenPoint> _tempGraphScale2;
        internal FGenGraph<FieldCell, FGenPoint> _tempGraphScale3;
        internal FGenGraph<FieldCell, FGenPoint> _tempGraphScale4;
        internal FGenGraph<FieldCell, FGenPoint> _tempGraphScale5;
        internal FGenGraph<FieldCell, FGenPoint> _tempGraphScale6;

        /// <summary>
        /// Clearing all temporary scaled graphs variables
        /// </summary>
        private void ResetScaledGraphs()
        {
            _tempGraphScale2 = null;
            _tempGraphScale3 = null;
            _tempGraphScale4 = null;
            _tempGraphScale5 = null;
            _tempGraphScale6 = null;
        }

        #endregion


        #region Editor Related

        [HideInInspector] public bool _EditorGUI_DrawExtra = false;
        [HideInInspector] public bool _EditorGUI_DrawIgnoring = false;
        [HideInInspector] public bool _EditorGUI_DrawVars = false;
        [HideInInspector] public bool _EditorGUI_DrawPackVars = false;
        [HideInInspector] public bool _ModifyVars = false;
        [HideInInspector] public bool _ModifyPackVars = false;
        [HideInInspector] public int _EditorGUI_SelectedId = -1;
        [HideInInspector] public int _Editor_CommandsPage = 0;
        [HideInInspector] public bool _EditorGUI_Foldout = false;

        #endregion


        /// <summary>
        /// Preparing provided instructions for cells
        /// </summary>
        public void PrepareGridPreInstructions(FGenGraph<FieldCell, FGenPoint> grid)
        {
            if (CellInstructions != null)
            {
                for (int i = 0; i < CellInstructions.Count; i++)
                {
                    if (CellInstructions[i].definition == null) continue;
                    if (CellInstructions[i].IsPreDefinition && CellInstructions[i].definition.InstructionType != InstructionDefinition.EInstruction.DoorHole) continue;
                    if (CellInstructions[i].IsPostDefinition) continue;

                    var cell = grid.GetCell(CellInstructions[i].gridPosition, false);

                    if (cell == null) continue;

                    cell.AddCellInstruction(CellInstructions[i]);
                }
            }
        }


        /// <summary>
        /// For generating there is created small copy of FieldSetup, if you do some changes in project file FieldSetup
        /// You should sync it with the copy
        /// </summary>
        //public void SyncGeneringWith(FieldSetup setup)
        //{
        //    if (setup == null) return;
        //    if (runtimeFieldSetup == null) return;

        //    runtimeFieldSetup.AddLightProbes = setup.AddLightProbes;
        //    runtimeFieldSetup.AddMultipleProbes = setup.AddLightProbes;
        //    runtimeFieldSetup.AddReflectionProbes = setup.AddLightProbes;
        //    runtimeFieldSetup.CellsInstructions = setup.CellsInstructions;
        //    runtimeFieldSetup.CellSize = setup.CellSize;
        //    runtimeFieldSetup.DontSpawnOn = setup.DontSpawnOn;
        //    runtimeFieldSetup.Ignores = setup.Ignores;
        //    runtimeFieldSetup.InfoText = setup.InfoText;
        //    runtimeFieldSetup.MainReflectionSettings = setup.MainReflectionSettings;
        //    runtimeFieldSetup.MiniReflSettings = setup.MiniReflSettings;
        //    runtimeFieldSetup.ModificatorPacks = setup.ModificatorPacks;
        //    runtimeFieldSetup.name = setup.name;
        //    runtimeFieldSetup.NonUniformSize = setup.NonUniformSize;
        //    runtimeFieldSetup.NonUniformCellSize = setup.NonUniformCellSize;
        //    runtimeFieldSetup.ProbesPerCell = setup.ProbesPerCell;
        //    runtimeFieldSetup.RootPack = setup.RootPack;
        //    runtimeFieldSetup.SelfInjections = setup.SelfInjections;
        //    runtimeFieldSetup.SmallerReflLowerSpaceThan = setup.SmallerReflLowerSpaceThan;
        //    runtimeFieldSetup.SmallerReflSettings = setup.SmallerReflSettings;
        //    runtimeFieldSetup.TriggerColliderGeneration = setup.TriggerColliderGeneration;
        //    runtimeFieldSetup.TriggerGenSettings = setup.TriggerGenSettings;
        //    runtimeFieldSetup.Variables = setup.Variables;
        //}


        #region Field Variables Related

        //public void RefreshFieldVariables()
        //{
        //    if (SwitchVariables == null) SwitchVariables = new List<FieldVariable>();
        //    if (ParentFieldSetup == null) return;

        //    if (SwitchVariables.Count == 0)
        //    {
        //        #region Add all elements from zero
        //        for (int i = 0; i < ParentFieldSetup.Variables.Count; i++)
        //        {
        //            SwitchVariables.Add(new FieldVariable(ParentFieldSetup.Variables[i]));
        //        }
        //        #endregion
        //    }
        //    else
        //    {
        //        PGGUtils.AdjustCount<FieldVariable>(SwitchVariables, ParentFieldSetup.Variables.Count);

        //        // Checking if same variable, if not then changing parameters
        //        for (int i = 0; i < ParentFieldSetup.Variables.Count; i++)
        //        {
        //            FieldVariable fv = ParentFieldSetup.Variables[i];
        //            FieldVariable v = SwitchVariables[i];
        //            if (fv.Name != v.Name || fv.ValueType != v.ValueType)
        //            {
        //                SwitchVariables[i] = new FieldVariable(fv);
        //                SwitchVariables[i].helperPackRef = ParentFieldSetup.RootPack;
        //            }
        //        }
        //    }


        //    if (SwitchPackVariables == null) SwitchPackVariables = new List<FieldVariable>();
        //    int countPackVars = CountFieldModsVariablesCount();

        //    if (SwitchPackVariables.Count == 0)
        //    {
        //        #region Add all elements from zero
        //        for (int i = 0; i < ParentFieldSetup.ModificatorPacks.Count; i++)
        //        {
        //            var pack = ParentFieldSetup.ModificatorPacks[i];
        //            if (pack == null) continue;

        //            // Checking if same variable, if not then changing parameters
        //            for (int p = 0; p < pack.Variables.Count; p++)
        //            {
        //                FieldVariable vr = pack.Variables[p];
        //                vr.helperPackRef = pack;
        //                SwitchPackVariables.Add(vr);
        //            }
        //        }
        //        #endregion
        //    }
        //    else
        //    {
        //        PGGUtils.AdjustCount<FieldVariable>(SwitchPackVariables, countPackVars);

        //        int iter = 0;
        //        for (int i = 0; i < ParentFieldSetup.ModificatorPacks.Count; i++)
        //        {
        //            var pack = ParentFieldSetup.ModificatorPacks[i];
        //            if (pack == null) continue;

        //            // Checking if same variable, if not then changing parameters
        //            for (int p = 0; p < pack.Variables.Count; p++)
        //            {
        //                FieldVariable fv = pack.Variables[p];
        //                FieldVariable v = SwitchPackVariables[iter];

        //                if (fv.Name != v.Name || fv.ValueType != v.ValueType)
        //                {
        //                    SwitchPackVariables[iter] = new FieldVariable(fv);
        //                }

        //                SwitchPackVariables[iter].helperPackRef = pack;

        //                iter += 1;
        //            }
        //        }
        //    }

        //}


        internal void ClearTemporaryInjections()
        {
            if (runtimeFieldSetup.SelfInjections != null)
                if (runtimeFieldSetup.temporaryInjections != null)
                    if (Injections != null)
                    {
                        for (int i = 0; i < runtimeFieldSetup.SelfInjections.Count; i++) runtimeFieldSetup.temporaryInjections.Remove(runtimeFieldSetup.SelfInjections[i]);
                        for (int i = 0; i < Injections.Count; i++) runtimeFieldSetup.temporaryInjections.Remove(Injections[i]);
                    }
        }

        //internal void ClearPostCells()
        //{
        //    for (int i = 0; i < _newCells.Count; i++)
        //    {
        //        _tempTargetGrid.RemoveCell(_newCells[i]);
        //    }

        //    _newCells.Clear();
        //}

        internal void RunPostIntructions()
        {
            runtimeFieldSetup.RunPostInstructionsOnGraph(gen.Grid, CellInstructions);
        }

        internal void RunTemporaryPostInjections(CellsController generationScheme)
        {
            if (runtimeFieldSetup.temporaryInjections != null)
                for (int i = 0; i < runtimeFieldSetup.temporaryInjections.Count; i++)
                    if (runtimeFieldSetup.temporaryInjections[i].Call == InjectionSetup.EGridCall.Post)
                    {
                        if (runtimeFieldSetup.temporaryInjections[i].Inject == InjectionSetup.EInjectTarget.Modificator)
                        {
                            if (runtimeFieldSetup.temporaryInjections[i].Modificator != null)
                            {
                                generationScheme.RunModification(runtimeFieldSetup.temporaryInjections[i].Modificator);
                            }
                        }
                        else if (runtimeFieldSetup.temporaryInjections[i].Inject == InjectionSetup.EInjectTarget.Pack )
                        {
                            if (runtimeFieldSetup.temporaryInjections[i].ModificatorsPack != null)
                                if (runtimeFieldSetup.temporaryInjections[i].ModificatorsPack.DisableWholePackage == false)
                                    generationScheme.RunModificatorPack(runtimeFieldSetup.temporaryInjections[i].ModificatorsPack);
                        }
                    }
        }

        internal void RunPostCellsRefill()
        {
            if (CellInstructions != null)
                for (int i = 0; i < CellInstructions.Count; i++)
                    if (CellInstructions[i].IsModRunner)
                    {
                        FieldCell nCell = gen.Grid.GetCell(CellInstructions[i].gridPosition, false);

                        if (FGenerators.CheckIfIsNull(nCell))
                        {
                            nCell = gen.Grid.GetCell(CellInstructions[i].gridPosition, true);
                            _newCells.Add(nCell);
                        }

                        nCell.InTargetGridArea = true;
                    }
        }

        internal void RunTemporaryPreInjections(CellsController generationScheme)
        {
            runtimeFieldSetup.RunTemporaryPreInjections(generationScheme);
        }

        internal void RunMainInstructions()
        {
            runtimeFieldSetup.RunMainInstructions(gen.Grid, CellInstructions);
        }

        internal void RunPreInstructions()
        {
            runtimeFieldSetup.RunPreInstructionsOnGraph(gen.Grid, CellInstructions);
        }

        internal void RefreshSelfInfjection()
        {
            if (runtimeFieldSetup != null)
            {
                if (runtimeFieldSetup.temporaryInjections == null) runtimeFieldSetup.temporaryInjections = new List<InjectionSetup>();
                runtimeFieldSetup.temporaryInjections.Clear();

                if (runtimeFieldSetup.SelfInjections != null)
                    for (int i = 0; i < runtimeFieldSetup.SelfInjections.Count; i++) runtimeFieldSetup.temporaryInjections.Add(runtimeFieldSetup.SelfInjections[i]);

                if (Injections != null)
                    for (int i = 0; i < Injections.Count; i++) runtimeFieldSetup.temporaryInjections.Add(Injections[i]);
            }
        }




        internal void PrepareScaledGrids()
        {
            if (IGeneration.CheckIfScaledGraphsNeeded(runtimeFieldSetup, CellInstructions))
            {
                if (_tempGraphScale2 == null || gen.Grid.SubGraphs == null)
                {
                    PrepareSubGraphs(gen.Grid);
                }
            }
        }

        //internal void PrepareOptionalCheckers()
        //{
        //if (OptionalCheckers != null)
        //{
        //    if (OptionalCheckers.Count > 0) _tempTargetInstantiation.OptionalCheckerFieldsData = new List<CheckerField>();

        //    for (int i = 0; i < OptionalCheckers.Count; i++)
        //    {
        //        if (FGenerators.CheckIfExist_NOTNULL(OptionalCheckers[i]))
        //        {
        //            _tempTargetInstantiation.OptionalCheckerFieldsData.Add(OptionalCheckers[i]);
        //            OptionalCheckers[i].HelperReference = runtimeFieldSetup;
        //        }
        //    }
        //}
        //}

        //private int CountFieldModsVariablesCount()
        //{
        //    if (runtimeFieldSetup == null) return 0;

        //    int countPackVars = 0;
        //    for (int i = 0; i < runtimeFieldSetup.ModificatorPacks.Count; i++)
        //    {
        //        var pack = runtimeFieldSetup.ModificatorPacks[i];
        //        if (pack == null) continue;
        //        countPackVars += runtimeFieldSetup.ModificatorPacks[i].Variables.Count;
        //    }

        //    return countPackVars;
        //}


        //public void FieldVariablesSetCustom()
        //{
        //    if (runtimeFieldSetup == null) return;

        //    if (_ModifyVars)
        //    {
        //        if (SwitchVariables.Count == runtimeFieldSetup.Variables.Count)
        //        {
        //            for (int i = 0; i < SwitchVariables.Count; i++)
        //            {
        //                runtimeFieldSetup.Variables[i].SetValue(SwitchVariables[i]);
        //            }
        //        }
        //    }

        //    if (_ModifyPackVars)
        //    {
        //        for (int m = 0; m < SwitchPackVariables.Count; m++)
        //        {
        //            var fv = SwitchPackVariables[m];
        //            if (fv == null) continue;
        //            if (fv.helperPackRef == null) continue;
        //            var mv = fv.helperPackRef.GetVariable(fv.Name);
        //            if (mv == null) continue;
        //            if (mv.ValueType != fv.ValueType) continue;
        //            mv.SetValue(fv);
        //        }
        //    }
        //}


        /// <summary>
        /// Must be called after preparing Temporary Injections
        /// Preparing all rules variables if used
        /// </summary>
        public void PreparePresetVariables()
        {

            #region Refreshing all rules variables preparement state

            //for (int p = 0; p < runtimeFieldSetup.ModificatorPacks.Count; p++)
            //{
            //    if (runtimeFieldSetup.ModificatorPacks[p] == null) continue;
            //    if (runtimeFieldSetup.ModificatorPacks[p].DisableWholePackage) continue;

            //    for (int r = 0; r < runtimeFieldSetup.ModificatorPacks[p].FieldModificators.Count; r++)
            //    {
            //        if (runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r] == null) continue;
            //        if (runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r].Enabled == false) continue;
            //        if (runtimeFieldSetup.Ignores.Contains(runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r])) continue;
            //        if (runtimeFieldSetup.IsEnabled(runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r]) == false) continue;

            //        for (int s = 0; s < runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r].Spawners.Count; s++)
            //        {
            //            if (FGenerators.CheckIfIsNull(runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r].Spawners[s])) continue;
            //            runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r].Spawners[s].Prepared = false;

            //            for (int rl = 0; rl < runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r].Spawners[s].Rules.Count; rl++)
            //            {
            //                if (FGenerators.CheckIfIsNull(runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r].Spawners[s].Rules[rl])) continue;
            //                runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r].Spawners[s].Rules[rl].VariablesPrepared = false;
            //            }
            //        }
            //    }
            //}

            for (int c = 0; c < runtimeFieldSetup.CellsCommands.Count; c++)
            {
                if (FGenerators.CheckIfIsNull(runtimeFieldSetup.CellsCommands[c])) continue;
                if (FGenerators.CheckIfIsNull(runtimeFieldSetup.CellsCommands[c].TargetModification)) continue;

                for (int s = 0; s < runtimeFieldSetup.CellsCommands[c].TargetModification.Spawners.Count; s++)
                {
                    if (FGenerators.CheckIfIsNull(runtimeFieldSetup.CellsCommands[c].TargetModification.Spawners[s])) continue;

                    for (int rl = 0; rl < runtimeFieldSetup.CellsCommands[c].TargetModification.Spawners[s].Rules.Count; rl++)
                    {
                        if (FGenerators.CheckIfIsNull(runtimeFieldSetup.CellsCommands[c].TargetModification.Spawners[s].Rules[rl])) continue;
                        runtimeFieldSetup.CellsCommands[c].TargetModification.Spawners[s].Rules[rl].VariablesPrepared = false;
                    }
                }
            }

            #endregion


            #region Applying field setup variables for temporary injections

            if (runtimeFieldSetup.temporaryInjections != null)
                for (int i = 0; i < runtimeFieldSetup.temporaryInjections.Count; i++)
                {
                    var inj = runtimeFieldSetup.temporaryInjections[i];
                    if (FGenerators.CheckIfIsNull(inj)) continue;

                    // No assigned pack and no assigned modificator -> checking for variable override
                    if (inj.ModificatorsPack == null && inj.Modificator == null)
                    {
                        if (inj.OverrideVariables)
                        {
                            if (inj.Overrides == null || inj.Overrides.Count == 0) continue;

                            for (int ov = 0; ov < inj.Overrides.Count; ov++)
                            {
                                var vari = runtimeFieldSetup.GetVariable(inj.Overrides[ov].Name);

                                if (i < inj.Overrides.Count)
                                    if (FGenerators.CheckIfExist_NOTNULL(vari)) vari.SetValue(inj.Overrides[i]);
                            }
                        }

                        continue;
                    }

                    if (inj.Inject == InjectionSetup.EInjectTarget.Pack || inj.Inject == InjectionSetup.EInjectTarget.PackOnlyForAccessingVariables)
                    {
                        if (inj.ModificatorsPack == null) continue;
                        for (int m = 0; m < inj.ModificatorsPack.FieldModificators.Count; m++)
                        {
                            if (inj.ModificatorsPack.FieldModificators[m] == null) continue;
                            inj.ModificatorsPack.FieldModificators[m].PrepareVariablesWith(runtimeFieldSetup, true, inj);
                        }
                    }
                    else if (inj.Inject == InjectionSetup.EInjectTarget.Modificator  )
                    {
                        if (inj.Modificator == null) continue;
                        inj.Modificator.PrepareVariablesWith(runtimeFieldSetup, true, inj);
                    }
                    else if (inj.Inject == InjectionSetup.EInjectTarget.ModOnlyForAccessingVariables)
                    {
                        if (inj.Modificator == null) continue;
                        inj.Modificator.PrepareVariablesWith(runtimeFieldSetup, true, inj);

                        // Preparing all modificators with this variable injection

                        for (int p = 0; p < runtimeFieldSetup.ModificatorPacks.Count; p++)
                        {
                            if (runtimeFieldSetup.ModificatorPacks[p] == null) continue;
                            if (runtimeFieldSetup.ModificatorPacks[p].DisableWholePackage) continue;

                            for (int r = 0; r < runtimeFieldSetup.ModificatorPacks[p].FieldModificators.Count; r++)
                            {
                                if (runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r] == null) continue;
                                if (runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r].Enabled == false) continue;
                                if (runtimeFieldSetup.Ignores.Contains(runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r])) continue;
                                if (runtimeFieldSetup.IsEnabled(runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r]) == false) continue;
                                runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r].PrepareVariablesWith(runtimeFieldSetup, true, inj);
                            }
                        }

                        for (int c = 0; c < runtimeFieldSetup.CellsCommands.Count; c++)
                        {
                            if (FGenerators.CheckIfIsNull(runtimeFieldSetup.CellsCommands[c])) continue;
                            if (FGenerators.CheckIfIsNull(runtimeFieldSetup.CellsCommands[c].TargetModification)) continue;
                            runtimeFieldSetup.CellsCommands[c].TargetModification.PrepareVariablesWith(runtimeFieldSetup, true, inj);
                        }
                    }
                }

            #endregion


            #region Preparing default variables for modificators if not injected

            //for (int p = 0; p < runtimeFieldSetup.ModificatorPacks.Count; p++)
            //{
            //    if (runtimeFieldSetup.ModificatorPacks[p] == null) continue;
            //    if (runtimeFieldSetup.ModificatorPacks[p].DisableWholePackage) continue;

            //    for (int r = 0; r < runtimeFieldSetup.ModificatorPacks[p].FieldModificators.Count; r++)
            //    {
            //        if (runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r] == null) continue;
            //        if (runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r].Enabled == false) continue;
            //        if (runtimeFieldSetup.Ignores.Contains(runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r])) continue;
            //        if (runtimeFieldSetup.IsEnabled(runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r]) == false) continue;
            //        runtimeFieldSetup.ModificatorPacks[p].FieldModificators[r].PrepareVariablesWith(runtimeFieldSetup);
            //    }

            //    for (int c = 0; c < runtimeFieldSetup.CellsCommands.Count; c++)
            //    {
            //        if (FGenerators.CheckIfIsNull(runtimeFieldSetup.CellsCommands[c])) continue;
            //        if (FGenerators.CheckIfIsNull(runtimeFieldSetup.CellsCommands[c].TargetModification)) continue;
            //        runtimeFieldSetup.CellsCommands[c].TargetModification.PrepareVariablesWith(runtimeFieldSetup);
            //    }
            //}

            #endregion

        }

        internal void PrepareGuides()
        {
            for (int i = 0; i < CellInstructions.Count; i++)
            {
                if (CellInstructions[i].definition == null)
                {
                    if (CellInstructions[i].HelperID >= 0)
                    {
                        if (CellInstructions[i].HelperID < FlexSetup.RuntimeFieldSetup.CellsCommands.Count)
                        {
                            SpawnInstruction instr = CellInstructions[i];
                            instr.definition = FlexSetup.RuntimeFieldSetup.CellsCommands[CellInstructions[i].HelperID];
                            CellInstructions[i] = instr;
                        }
                    }
                }
            }


            //for (int i = 0; i < CellInstructions.Count; i++)
            //{
            //    FieldCell cell = gen.Grid.GetCell(CellInstructions[i].gridPosition, false);
            //    if (FGenerators.CheckIfExist_NOTNULL(cell))
            //    {
            //        if (cell.GuidesIn == null) cell.GuidesIn = new List<SpawnInstruction>();
            //        cell.GuidesIn.Add(CellInstructions[i]);
            //    }
            //}
        }

        #endregion


        #region More Preparations

        // Construct scaled graphs out of already placed cells
        public void PrepareSubGraphs(FGenGraph<FieldCell, FGenPoint> grid)
        {
            ResetScaledGraphs();

            if (grid.SubGraphs != null) grid.SubGraphs.Clear();

            grid.SubGraphs = new List<FGenGraph<FieldCell, FGenPoint>>();

            for (int s = 2; s <= 6; s++)
            {
                var gr = IGeneration.GetScaledGrid(grid, this, s, true);
                if (gr != null) grid.SubGraphs.Add(gr);
            }
        }

        #endregion


        #region Editor GUI

#if UNITY_EDITOR



        public static void DrawPreparation(GeneratingPreparation Get, SerializedProperty property)
        {
            GUILayout.Space(4);

            FGUI_Inspector.FoldHeaderStart(ref Get._EditorGUI_Foldout, "Generating Preparation Settings", FGUI_Resources.BGInBoxStyle);

            if (Get._EditorGUI_Foldout)
            {

                EditorGUI.indentLevel++;
                GUILayout.Space(3);
                SerializedProperty sp = property.FindPropertyRelative("Injections");
                EditorGUILayout.PropertyField(sp, true); sp.Next(false);
                EditorGUILayout.PropertyField(sp, true);
                GUILayout.Space(3);
                EditorGUI.indentLevel--;

                if (Get.FlexSetup != null)
                {
                    if (Get.Composition != null)
                        if (Get.Composition.Setup != Get.FlexSetup.FieldPreset)
                        {
                            Get.Composition.Setup = Get.FlexSetup.FieldPreset;
                        }

                    FieldSetupComposition.DrawCompositionGUI(Get.FlexSetup.ParentObject, Get.Composition, true);
                    GUILayout.Space(3);
                }

                //EditorGUI.indentLevel++;
                //base.DrawGUIFooter();
                //EditorGUI.indentLevel--;

                #region Backup Code

                //if (Get.ParentFieldSetup != null)
                //{

                //    //GUILayout.Space(3);
                //    //FGUI_Inspector.FoldSwitchableHeaderStart(ref Get._ModifyVars, ref Get._EditorGUI_DrawVars, "FieldSetup Variables values for generating", FGUI_Resources.BGInBoxStyle);
                //    //GUILayout.Space(3);

                //    //if (Get._EditorGUI_DrawVars && Get._ModifyVars)
                //    //{
                //    //    Get.RefreshFieldVariables();
                //    //    for (int i = 0; i < Get.SwitchVariables.Count; i++)
                //    //    {
                //    //        FieldVariable.Editor_DrawTweakableVariable(Get.SwitchVariables[i]);
                //    //    }
                //    //}

                //    //EditorGUILayout.EndVertical();



                //    //if (Get.SwitchPackVariables.Count > 0)
                //    //{
                //    //    GUILayout.Space(3);
                //    //    FGUI_Inspector.FoldSwitchableHeaderStart(ref Get._ModifyPackVars, ref Get._EditorGUI_DrawPackVars, "Mod Packs Variables values for generating", FGUI_Resources.BGInBoxStyle);
                //    //    GUILayout.Space(3);

                //    //    if (Get._EditorGUI_DrawPackVars && Get._ModifyPackVars)
                //    //    {
                //    //        Get.RefreshFieldVariables();
                //    //        for (int i = 0; i < Get.SwitchPackVariables.Count; i++)
                //    //        {
                //    //            FieldVariable.Editor_DrawTweakableVariable(Get.SwitchPackVariables[i]);
                //    //        }
                //    //    }

                //    //    EditorGUILayout.EndVertical();
                //    //}



                //    //GUILayout.Space(3);
                //    //FGUI_Inspector.FoldHeaderStart(ref Get._EditorGUI_DrawIgnoring, "Select Ignored Modificators", FGUI_Resources.BGInBoxStyle);
                //    //GUILayout.Space(3);

                //    //if (Get._EditorGUI_DrawIgnoring)
                //    //{

                //    //    if (Get.ParentFieldSetup.ModificatorPacks.Count > 0)
                //    //    {
                //    //        EditorGUILayout.BeginHorizontal();

                //    //        if (GUILayout.Button(" < ", GUILayout.Width(40))) Get._EditorGUI_SelectedId--;

                //    //        EditorGUILayout.LabelField((Get._EditorGUI_SelectedId + 1) + " / " + Get.ParentFieldSetup.ModificatorPacks.Count, EditorStyles.centeredGreyMiniLabel);

                //    //        if (GUILayout.Button(" > ", GUILayout.Width(40))) Get._EditorGUI_SelectedId++;

                //    //        if (Get._EditorGUI_SelectedId >= Get.ParentFieldSetup.ModificatorPacks.Count) Get._EditorGUI_SelectedId = 0;
                //    //        if (Get._EditorGUI_SelectedId < 0) Get._EditorGUI_SelectedId = Get.ParentFieldSetup.ModificatorPacks.Count - 1;

                //    //        EditorGUILayout.EndHorizontal();

                //    //        if (Get._EditorGUI_SelectedId == -1)
                //    //            DrawIgnoresList(Get, Get.ParentFieldSetup.RootPack, sp.serializedObject);
                //    //        else
                //    //            DrawIgnoresList(Get, Get.ParentFieldSetup.ModificatorPacks[Get._EditorGUI_SelectedId], sp.serializedObject);
                //    //    }
                //    //    else
                //    //    {
                //    //        DrawIgnoresList(Get, Get.ParentFieldSetup.RootPack, sp.serializedObject);
                //    //    }

                //    //}

                //    EditorGUILayout.EndVertical();

                //}
                //else
                //{
                //    EditorGUILayout.HelpBox("No Target Field Setup assigned in Preparation Settings!", MessageType.Warning);
                //}

                #endregion



                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(3);
        }


        //public static void DrawIgnoresList(GeneratingPreparation Get, ModificatorsPack pack, SerializedObject so)
        //{
        //    bool preE = GUI.enabled;
        //    GUILayout.Space(2);
        //    EditorGUILayout.BeginVertical();
        //    EditorGUILayout.BeginHorizontal();

        //    bool pselected = !Get.IgnoredPacksForGenerating.Contains(pack);
        //    bool pre = pselected;
        //    pselected = EditorGUILayout.Toggle(pselected, GUILayout.Width(18));
        //    EditorGUILayout.LabelField(pack.name + " Mods Pack", FGUI_Resources.HeaderStyle);

        //    if (pselected != pre)
        //    {
        //        if (pselected == false)
        //        {
        //            Get.IgnoredPacksForGenerating.Add(pack);
        //            so.Update();
        //            EditorUtility.SetDirty(so.targetObject);
        //        }
        //        else
        //        {
        //            Get.IgnoredPacksForGenerating.Remove(pack);
        //            so.Update();
        //            EditorUtility.SetDirty(so.targetObject);
        //        }
        //    }

        //    GUI.enabled = false;
        //    EditorGUILayout.ObjectField(pack, typeof(ModificatorsPack), false, GUILayout.Width(60));
        //    GUI.enabled = preE;
        //    EditorGUILayout.EndHorizontal();

        //    if (pselected == false) GUI.enabled = false;

        //    GUILayout.Space(4);
        //    for (int i = 0; i < pack.FieldModificators.Count; i++)
        //    {
        //        var mod = pack.FieldModificators[i];
        //        EditorGUILayout.BeginHorizontal();

        //        bool selected = !Get.IgnoredModificationsForGenerating.Contains(mod);
        //        pre = selected;

        //        selected = EditorGUILayout.Toggle(selected, GUILayout.Width(18));

        //        EditorGUIUtility.labelWidth = 60;
        //        EditorGUILayout.ObjectField(selected ? "Enabled" : "Ignored", mod, typeof(FieldModification), true);
        //        EditorGUIUtility.labelWidth = 0;

        //        if (selected != pre)
        //        {
        //            if (selected == false)
        //            {
        //                Get.IgnoredModificationsForGenerating.Add(mod);
        //                so.Update();
        //                EditorUtility.SetDirty(so.targetObject);
        //            }
        //            else
        //            {
        //                Get.IgnoredModificationsForGenerating.Remove(mod);
        //                so.Update();
        //                EditorUtility.SetDirty(so.targetObject);
        //            }
        //        }

        //        EditorGUILayout.EndHorizontal();
        //    }

        //    EditorGUILayout.EndVertical();

        //    if (Get.IgnoredModificationsForGenerating.Count > 0)
        //    {
        //        GUILayout.Space(4);

        //        if (GUILayout.Button("Clear All Ignores"))
        //        {
        //            Get.IgnoredModificationsForGenerating.Clear();
        //            so.Update();
        //            EditorUtility.SetDirty(so.targetObject);
        //        }
        //    }

        //    GUILayout.Space(4);
        //    GUI.enabled = preE;

        //}



#endif

        #endregion

    }

}