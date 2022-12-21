using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class FieldSetup : ScriptableObject
    {


        /// <summary>
        /// Running all field guides on choosed grid and placing spawn data inside cells
        /// </summary>
        public void RunPreInstructionsOnGraph(FGenGraph<FieldCell, FGenPoint> grid, List<SpawnInstruction> guides)
        {
            if (guides != null)
            {

                // First add all custom data
                for (int i = 0; i < guides.Count; i++)
                {
                    if (FGenerators.CheckIfIsNull(guides[i].definition))
                    {
                        continue;
                    }

                    if (guides[i].definition.InstructionType == InstructionDefinition.EInstruction.InjectDataString || guides[i].definition.InstructionType == InstructionDefinition.EInstruction.DoorHole)
                    {
                        var cell = grid.GetCell(guides[i].gridPosition, false);
                        if (FGenerators.CheckIfExist_NOTNULL(cell)) cell.AddCustomData(guides[i].definition.InstructionArgument);
                    }
                    else if (guides[i].definition.InstructionType == InstructionDefinition.EInstruction.SetGhostCell)
                    {
                        var cell = grid.GetCell(guides[i].gridPosition, false);
                        if (FGenerators.CheckIfExist_NOTNULL(cell)) cell.IsGhostCell = true;
                    }
                }

                // Then run pre modificators
                for (int i = 0; i < guides.Count; i++)
                {
                    if (FGenerators.CheckIfIsNull(guides[i].definition)) continue;
                    if (guides[i].definition.InstructionType == InstructionDefinition.EInstruction.PreRunModificator /*|| guides[i].definition.InstructionType == InstructionDefinition.EInstruction.DoorHole*/)
                    {
                        if (guides[i].definition.TargetModification != null)
                        {
                            bool preE = guides[i].definition.TargetModification.Enabled;
                            guides[i].definition.TargetModification.Enabled = true;
                            RunModificatorWithInstruction(grid, guides[i].definition.TargetModification, guides[i]);
                            guides[i].definition.TargetModification.Enabled = preE;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Running all field guides on choosed grid and placing spawn data inside cells
        /// </summary>
        public void RunPostInstructionsOnGraph(FGenGraph<FieldCell, FGenPoint> grid, List<SpawnInstruction> guides)
        {
            if (guides != null)
            {
                for (int i = 0; i < guides.Count; i++)
                {
                    if (FGenerators.CheckIfIsNull(guides[i].definition)) continue;
                    if (guides[i].definition.InstructionType == InstructionDefinition.EInstruction.PostRunModificator || guides[i].definition.InstructionType == InstructionDefinition.EInstruction.DoorHole)
                    {
                        //var cl = grid.GetCell(guides[i].gridPosition, true);

                        if (guides[i].definition.TargetModification != null)
                        {
                            bool preE = guides[i].definition.TargetModification.Enabled;
                            guides[i].definition.TargetModification.Enabled = true;
                            RunModificatorWithInstruction(grid, guides[i].definition.TargetModification, guides[i]);
                            guides[i].definition.TargetModification.Enabled = preE;
                        }
                    }
                }
            }
        }

        internal FieldSetup GenerateRuntimeInstance()
        {
            FieldSetup newSetup = Copy();

            newSetup.RootPack = Instantiate(newSetup.RootPack);
            newSetup.RootPack.ParentPreset = newSetup;

            newSetup.disabledMods.Clear();

            for (int i = 0; i < newSetup.ModificatorPacks.Count; i++)
            {
                if (ModificatorPacks[i] == RootPack)
                {
                    newSetup.ModificatorPacks[i] = newSetup.RootPack;
                    PrepareRuntimeModPack(newSetup, newSetup.ModificatorPacks[i], ModificatorPacks[i]);
                    continue;
                }

                newSetup.ModificatorPacks[i] = Instantiate(newSetup.ModificatorPacks[i]);
                newSetup.ModificatorPacks[i].ParentPreset = newSetup;
                PrepareRuntimeModPack(newSetup, newSetup.ModificatorPacks[i], ModificatorPacks[i]);
            }

            for (int i = 0; i < UtilityModificators.Count; i++)
            {
                var newMod = Instantiate(UtilityModificators[i]);
                newMod.ParentPreset = newSetup;
                newSetup.UtilityModificators[i] = newMod;
            }

            return newSetup;
        }

        void PrepareRuntimeModPack(FieldSetup newSetup, ModificatorsPack pack, ModificatorsPack sourcePack)
        {
            for (int m = 0; m < pack.FieldModificators.Count; m++)
            {
                var newMod = Instantiate(pack.FieldModificators[m]);
                newMod.ParentPack = pack;
                newMod.ParentPreset = newSetup;

                if (IsModDisabledForThisSetup(sourcePack.FieldModificators[m]))
                {
                    newMod.Enabled = false;
                    newSetup.Ignores.Add(newMod);
                    newSetup.AddToDisabled(newMod);
                }

                pack.FieldModificators[m] = newMod;
            }
        }


        public void RunMainInstructions(FGenGraph<FieldCell, FGenPoint> grid, List<SpawnInstruction> instructions = null)
        {
            if (instructions != null)
            {
                for (int i = 0; i < instructions.Count; i++)
                {
                    if (instructions[i].definition == null) { continue; }
                    if (instructions[i].IsPreDefinition && instructions[i].definition.InstructionType != InstructionDefinition.EInstruction.DoorHole) { /*UnityEngine.Debug.Log("zxc"); */continue; }
                    if (instructions[i].IsPostDefinition) {  continue; }
                    var cell = grid.GetCell(instructions[i].gridPosition, false);
                    if (cell == null) {  continue; }
                    cell.AddCellInstruction(instructions[i]);
                }
            }
        }

        /// <summary>
        /// Running all field modificators on choosed grid and placing spawn data inside cells
        /// </summary>
        public void RunRulesOnGraph(FGenGraph<FieldCell, FGenPoint> grid, List<FieldCell> randCells, List<FieldCell> randCells2, List<SpawnInstruction> instructions = null)
        {
            // Preparing provided instructions for cells
            RunMainInstructions(grid, instructions);

            //for (int p = 0; p < ModificatorPacks.Count; p++)
            //{
            //    if (ModificatorPacks[p] == null) continue;
            //    if (ModificatorPacks[p].DisableWholePackage) continue;

            //    for (int r = 0; r < ModificatorPacks[p].FieldModificators.Count; r++)
            //    {
            //        if (ModificatorPacks[p].FieldModificators[r] == null) continue;
            //        if (ModificatorPacks[p].FieldModificators[r].Enabled == false) continue;
            //        if (Ignores.Contains(ModificatorPacks[p].FieldModificators[r])) continue;
            //        if (IsEnabled(ModificatorPacks[p].FieldModificators[r]) == false) continue;

            //        ModificatorPacks[p].FieldModificators[r].PrepareVariablesWith(this);
            //    }
            //}

            // Cleaning nulls
            //for (int i = disabledMods.Count - 1; i >= 0; i--) if (disabledMods[i] == null) disabledMods.RemoveAt(i);
            PGGUtils.CheckForNulls(disabledMods);

            // Running grid rules
            for (int p = 0; p < ModificatorPacks.Count; p++)
            {
                if (ModificatorPacks[p] == null) continue;
                if (ModificatorPacks[p].DisableWholePackage) continue;
                RunModificatorPackOn(ModificatorPacks[p], grid, randCells, randCells2);
            }


        }

        public void RunModificatorPackOn(ModificatorsPack pack, FGenGraph<FieldCell, FGenPoint> grid, List<FieldCell> randCells, List<FieldCell> randCells2)
        {
            if (pack.SeedMode != ModificatorsPack.ESeedMode.None)
            {
                if (pack.SeedMode == ModificatorsPack.ESeedMode.Reset)
                {
                    FGenerators.SetSeed(FGenerators.LatestSeed);
                }
                else if (pack.SeedMode == ModificatorsPack.ESeedMode.Custom)
                {
                    FGenerators.SetSeed(pack.CustomSeed);
                }
            }

            for (int r = 0; r < pack.FieldModificators.Count; r++)
            {
                if (pack.FieldModificators[r] == null) continue;
                if (pack.FieldModificators[r].Enabled == false) continue;
                if (Ignores.Contains(pack.FieldModificators[r])) continue;
                if (IsEnabled(pack.FieldModificators[r]) == false) continue;

                if (pack.FieldModificators[r].VariantOf != null)
                    pack.FieldModificators[r].VariantOf.ModifyGraph(this, grid, randCells, randCells2, pack.FieldModificators[r]);
                else
                    pack.FieldModificators[r].ModifyGraph(this, grid, randCells, randCells2);
            }

            // After modificators call
            for (int r = 0; r < pack.FieldModificators.Count; r++)
            {
                if (pack.FieldModificators[r] == null) continue;
                if (pack.FieldModificators[r].Enabled == false) continue;
                if (Ignores.Contains(pack.FieldModificators[r])) continue;
                if (IsEnabled(pack.FieldModificators[r]) == false) continue;

                for (int s = 0; s < pack.FieldModificators[r].Spawners.Count; s++)
                {
                    pack.FieldModificators[r].Spawners[s].OnAfterModificatorsCall();
                }
            }
        }



        public void RunModificatorOnGrid(FGenGraph<FieldCell, FGenPoint> grid, List<FieldCell> randCells, List<FieldCell> randCells2, FieldModification modificator, bool dontRunIfDisabledByFieldSetup = true)
        {
            if (modificator == null)
            {
                UnityEngine.Debug.Log("[Interior Generator] Not assigned target modificator to " + name + "!");
                return;
            }

            if (modificator.Enabled == false) return;
            if (dontRunIfDisabledByFieldSetup) if (IsEnabled(modificator) == false) return;

            if (modificator.VariantOf != null)
                modificator.VariantOf.ModifyGraph(this, grid, randCells, randCells2, modificator);
            else
                modificator.ModifyGraph(this, grid, randCells, randCells2);
        }


        public void RunModificatorWithInstruction(FGenGraph<FieldCell, FGenPoint> grid, FieldModification modificator, SpawnInstruction guide)
        {
            if (modificator == null)
            {
                UnityEngine.Debug.Log("[Interior Generator] Not assigned target modificator to " + name + "!");
                return;
            }

            FieldCell cell = grid.GetCell(guide.gridPosition, false);
            if (FGenerators.CheckIfExist_NOTNULL(cell))
            {
                if (cell.InTargetGridArea)
                {
                    bool ignoreRestr = false;
                    if (FGenerators.CheckIfExist_NOTNULL(guide.definition)) if (guide.definition.InstructionType == InstructionDefinition.EInstruction.DoorHole) ignoreRestr = true;

                    if (guide.useDirection)
                        modificator.ModifyGraphCell(this, cell, grid, true, guide.FlatDirection, true, ignoreRestr);
                    else
                        modificator.ModifyGraphCell(this, cell, grid, true, null, true, ignoreRestr);
                }
                else
                {
                }
            }
        }

        public void RefreshUtilityMods()
        {
            for (int i = 0; i < UtilityModificators.Count; i++)
            {
                if (UtilityModificators[i] == null) continue;
                UtilityModificators[i].ParentPreset = this;

                if (UtilityModificators[i].ParentPack == null)
                {
                    UtilityModificators[i].ParentPack = RootPack;
                }
                else
                {
                    if (UtilityModificators[i].ParentPreset.UtilityModificators.Contains(UtilityModificators[i]) == false)
                    {
                        if (UtilityModificators[i].ParentPack.FieldModificators.Contains(UtilityModificators[i]) == false)
                        {
                            UtilityModificators[i].ParentPack = RootPack;
                        }
                        else
                        {
                        }
                    }
                }
            }

        }

    }

}