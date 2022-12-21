using FIMSpace.Generating.Checker;
using FIMSpace.Generating.Planning;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public static partial class IGeneration
    {
        public static bool Debugging = false;
        public static FGenGraph<FieldCell, FGenPoint> GetEmptyFieldGraph()
        {
            return new FGenGraph<FieldCell, FGenPoint>();
        }

        public static void ClearCells(FGenGraph<FieldCell, FGenPoint> grid)
        {
            if (grid != null)
            {
                for (int i = 0; i < grid.AllCells.Count; i++)
                    grid.AllCells[i].Clear();
            }
        }

        // Preparing required upscaled grids
        public static void PrepareScaledGrids(FieldSetup preset, FGenGraph<FieldCell, FGenPoint> grid, List<SpawnInstruction> guides = null)
        {
            if (CheckIfScaledGraphsNeeded(preset, guides))
                if (preset._tempGraphScale2 == null || grid.SubGraphs == null)
                    preset.PrepareSubGraphs(grid);
        }

        /// <summary>
        /// Refreshing self injections if used
        /// </summary>
        public static void PrepareSelfInjections(FieldSetup preset)
        {
            if (preset != null)
                if (preset.SelfInjections != null)
                    if (preset.SelfInjections.Count > 0)
                    {
                        if (preset.temporaryInjections == null)
                            preset.temporaryInjections = new List<InjectionSetup>();

                        for (int i = 0; i < preset.SelfInjections.Count; i++)
                            preset.temporaryInjections.Add(preset.SelfInjections[i]);
                    }
        }

        /// <summary>
        /// Rrestoring grid after guides
        /// </summary>
        public static void ClearCellsFromGrid(FGenGraph<FieldCell, FGenPoint> grid, List<FieldCell> toClear)
        {
            for (int i = 0; i < toClear.Count; i++) grid.RemoveCell(toClear[i]);
        }

        public static void ClearTemporaryInjections(FieldSetup preset)
        {
            if (preset.SelfInjections != null)
                if (preset.temporaryInjections != null)
                    for (int i = 0; i < preset.SelfInjections.Count; i++)
                        preset.temporaryInjections.Remove(preset.SelfInjections[i]);
        }

        public static List<FieldCell> GetRandomizedCells(FGenGraph<FieldCell, FGenPoint> grid)
        {
            List<FieldCell> pack = new List<FieldCell>();
            List<FieldCell> randomizedCells = new List<FieldCell>();

            if (grid != null)
                for (int i = 0; i < grid.AllApprovedCells.Count; i++)
                    pack.Add(grid.AllApprovedCells[i]);

            while (pack.Count > 0)
            {
                int index = FGenerators.GetRandom(0, pack.Count);
                randomizedCells.Add(pack[index]);
                pack.RemoveAt(index);
            }

            return randomizedCells;
        }

        internal static Vector3 V2ToV3(Vector2Int p)
        {
            return new Vector3(p.x, 0, p.y);
        }



        public static bool CheckIfScaledGraphsNeeded(FieldSetup preset, List<SpawnInstruction> guides = null)
        {
            if (preset == null) return false;

            if (preset.RequiresScaledGraphs()) return true;

            if (guides != null)
                for (int g = 0; g < guides.Count; g++)
                    if (guides[g].definition != null)
                        if (guides[g].definition.TargetModification)
                            if (guides[g].definition.TargetModification.RequiresScaledGraphs()) return true;

            return false;

        }

        /// <summary>
        /// Must be called after calling preset.SetTemporaryInjections()
        /// </summary>
        public static void PreparePresetVariables(FieldSetup preset)
        {

            #region Preparing all rules variables if used


            #region Refreshing all rules variables preparement state

            for (int p = 0; p < preset.ModificatorPacks.Count; p++)
            {
                if (preset.ModificatorPacks[p] == null) continue;
                if (preset.ModificatorPacks[p].DisableWholePackage) continue;

                for (int r = 0; r < preset.ModificatorPacks[p].FieldModificators.Count; r++)
                {
                    if (preset.ModificatorPacks[p].FieldModificators[r] == null) continue;
                    if (preset.ModificatorPacks[p].FieldModificators[r].Enabled == false) continue;
                    if (preset.Ignores.Contains(preset.ModificatorPacks[p].FieldModificators[r])) continue;
                    if (preset.IsEnabled(preset.ModificatorPacks[p].FieldModificators[r]) == false) continue;

                    for (int s = 0; s < preset.ModificatorPacks[p].FieldModificators[r].Spawners.Count; s++)
                    {
                        if (FGenerators.CheckIfIsNull(preset.ModificatorPacks[p].FieldModificators[r].Spawners[s] )) continue;
                        preset.ModificatorPacks[p].FieldModificators[r].Spawners[s].Prepared = false;

                        // Supporting "Tag for all spawners"
                        //if ( !string.IsNullOrEmpty(preset.ModificatorPacks[p].TagForAllSpawners))
                        //{
                        //    preset.ModificatorPacks[p].FieldModificators[r].Spawners[s].SpawnerTag = preset.ModificatorPacks[p].TagForAllSpawners;
                        //}

                        for (int rl = 0; rl < preset.ModificatorPacks[p].FieldModificators[r].Spawners[s].Rules.Count; rl++)
                        {
                            if (FGenerators.CheckIfIsNull(preset.ModificatorPacks[p].FieldModificators[r].Spawners[s].Rules[rl] )) continue;
                            preset.ModificatorPacks[p].FieldModificators[r].Spawners[s].Rules[rl].VariablesPrepared = false;
                        }
                    }
                }
            }

            for (int c = 0; c < preset.CellsCommands.Count; c++)
            {
                if (FGenerators.CheckIfIsNull(preset.CellsCommands[c] )) continue;
                if (FGenerators.CheckIfIsNull(preset.CellsCommands[c].TargetModification )) continue;
                if (FGenerators.CheckIfIsNull(preset.CellsCommands[c].TargetModification.Spawners )) continue;

                for (int s = 0; s < preset.CellsCommands[c].TargetModification.Spawners.Count; s++)
                {
                    if (FGenerators.CheckIfIsNull(preset.CellsCommands[c].TargetModification.Spawners[s] )) continue;

                    for (int rl = 0; rl < preset.CellsCommands[c].TargetModification.Spawners[s].Rules.Count; rl++)
                    {
                        if (FGenerators.CheckIfIsNull(preset.CellsCommands[c].TargetModification.Spawners[s].Rules[rl] )) continue;
                        preset.CellsCommands[c].TargetModification.Spawners[s].Rules[rl].VariablesPrepared = false;
                    }
                }
            }

            #endregion

            // Applying field setup variables for temporary injections
            if (preset.temporaryInjections != null)
                for (int i = 0; i < preset.temporaryInjections.Count; i++)
                {
                    var inj = preset.temporaryInjections[i];
                    if (FGenerators.CheckIfIsNull(inj )) continue;

                    // No assigned pack and no assigned modificator -> checking for variable override
                    if (inj.ModificatorsPack == null && inj.Modificator == null)
                    {
                        if (inj.OverrideVariables)
                        {
                            if (inj.Overrides == null || inj.Overrides.Count == 0) continue;

                            for (int ov = 0; ov < inj.Overrides.Count; ov++)
                            {
                                var vari = preset.GetVariable(inj.Overrides[ov].Name);

                                if (i < inj.Overrides.Count)
                                    if (FGenerators.CheckIfExist_NOTNULL(vari )) vari.SetValue(inj.Overrides[i]);
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
                            inj.ModificatorsPack.FieldModificators[m].PrepareVariablesWith(preset, true, inj);
                        }
                    }
                    else if (inj.Inject == InjectionSetup.EInjectTarget.Modificator)
                    {
                        if (inj.Modificator == null) continue;
                        inj.Modificator.PrepareVariablesWith(preset, true, inj);
                    }
                    else if (inj.Inject == InjectionSetup.EInjectTarget.ModOnlyForAccessingVariables)
                    {
                        if (inj.Modificator == null) continue;
                        inj.Modificator.PrepareVariablesWith(preset, true, inj);

                        // Preparing all modificators with this variable injection

                        for (int p = 0; p < preset.ModificatorPacks.Count; p++)
                        {
                            if (preset.ModificatorPacks[p] == null) continue;
                            if (preset.ModificatorPacks[p].DisableWholePackage) continue;

                            for (int r = 0; r < preset.ModificatorPacks[p].FieldModificators.Count; r++)
                            {
                                if (preset.ModificatorPacks[p].FieldModificators[r] == null) continue;
                                if (preset.ModificatorPacks[p].FieldModificators[r].Enabled == false) continue;
                                if (preset.Ignores.Contains(preset.ModificatorPacks[p].FieldModificators[r])) continue;
                                if (preset.IsEnabled(preset.ModificatorPacks[p].FieldModificators[r]) == false) continue;
                                preset.ModificatorPacks[p].FieldModificators[r].PrepareVariablesWith(preset, true, inj);
                            }
                        }

                        for (int c = 0; c < preset.CellsCommands.Count; c++)
                        {
                            if (FGenerators.CheckIfIsNull(preset.CellsCommands[c] )) continue;
                            if (FGenerators.CheckIfIsNull(preset.CellsCommands[c].TargetModification )) continue;
                            preset.CellsCommands[c].TargetModification.PrepareVariablesWith(preset, true, inj);
                        }
                    }
                }


            // Preparing default variables for modificators if not injected
            for (int p = 0; p < preset.ModificatorPacks.Count; p++)
            {
                if (preset.ModificatorPacks[p] == null) continue;
                if (preset.ModificatorPacks[p].DisableWholePackage) continue;

                for (int r = 0; r < preset.ModificatorPacks[p].FieldModificators.Count; r++)
                {
                    if (preset.ModificatorPacks[p].FieldModificators[r] == null) continue;
                    if (preset.ModificatorPacks[p].FieldModificators[r].Enabled == false) continue;
                    if (preset.Ignores.Contains(preset.ModificatorPacks[p].FieldModificators[r])) continue;
                    if (preset.IsEnabled(preset.ModificatorPacks[p].FieldModificators[r]) == false) continue;
                    preset.ModificatorPacks[p].FieldModificators[r].PrepareVariablesWith(preset);
                }

                for (int c = 0; c < preset.CellsCommands.Count; c++)
                {
                    if (FGenerators.CheckIfIsNull(preset.CellsCommands[c] )) continue;
                    if (FGenerators.CheckIfIsNull(preset.CellsCommands[c].TargetModification )) continue;
                    preset.CellsCommands[c].TargetModification.PrepareVariablesWith(preset);
                }
            }


            #endregion

        }


        public static void RestorePresetVariables(FieldSetup preset)
        {
            //for (int p = 0; p < preset.ModificatorPacks.Count; p++)
            //{
            //    if (preset.ModificatorPacks[p] == null) continue;
            //    if (preset.ModificatorPacks[p].DisableWholePackage) continue;

            //    for (int r = 0; r < preset.ModificatorPacks[p].FieldModificators.Count; r++)
            //    {
            //        if (preset.ModificatorPacks[p].FieldModificators[r] == null) continue;
            //        if (preset.ModificatorPacks[p].FieldModificators[r].Enabled == false) continue;
            //        if (preset.Ignores.Contains(preset.ModificatorPacks[p].FieldModificators[r])) continue;
            //        if (preset.IsEnabled(preset.ModificatorPacks[p].FieldModificators[r]) == false) continue;

            //        for (int s = 0; s < preset.ModificatorPacks[p].FieldModificators[r].Spawners.Count; s++)
            //        {
            //            if (FGenerators.CheckIfIsNull(preset.ModificatorPacks[p].FieldModificators[r].Spawners[s])) continue;

            //            // Supporting "Tag for all spawners"
            //            //if (!string.IsNullOrEmpty(preset.ModificatorPacks[p].TagForAllSpawners))
            //            //{
            //            //    preset.ModificatorPacks[p].FieldModificators[r].Spawners[s].TempSpawnerTag = "";
            //            //}
            //        }
            //    }
            //}

        }


        #region Temporary Field Setup Generating


        /// <summary>
        /// Generating temporary FieldSetup to be able to run mod package on some grid
        /// </summary>
        public static FieldSetup GenerateTemporaryFieldSetupWith(ModificatorsPack putModPackInside, FieldSetup scaleReferenceField = null)
        {
            FieldSetup singlePackField = FieldSetup.CreateInstance<FieldSetup>();

            // Field Setup needs to know what scale to use
            FieldSetup parentField = scaleReferenceField;

            // If no scale reference pack provided let's try find modificators parent for it
            if ( parentField == null)
            {
                parentField = putModPackInside.ParentPreset;
            }

            if (parentField == null)
            {
                UnityEngine.Debug.Log("[PGG - Rectangle Fill] Can't find parent FieldSetup of " + putModPackInside + " to define size of grid!");
                return null;
            }

            singlePackField.CellSize = parentField.CellSize;
            singlePackField.ModificatorPacks.Add(putModPackInside);

            return singlePackField;
        }


        /// <summary>
        /// Generating temporary FieldSetup to be able to run single modificator on some grid
        /// </summary>
        public static FieldSetup GenerateTemporaryFieldSetupWith(FieldModification singleModificator, FieldSetup scaleReferenceField = null)
        {
            FieldSetup singleModField = FieldSetup.CreateInstance<FieldSetup>();

            // Field Setup needs to know what scale to use
            FieldSetup parentField = scaleReferenceField;

            // If no scale reference pack provided let's try find modificators parent for it
            if (parentField == null)
            {
                parentField = singleModificator.TryGetParentSetup();
            }

            if (parentField == null)
            {
                UnityEngine.Debug.Log("[PGG - Rectangle Fill] Can't find parent FieldSetup of " + singleModificator + " to define size of grid!");
                return null;
            }

            singleModField.CellSize = parentField.CellSize;
            singleModField.RootPack = ModificatorsPack.CreateInstance<ModificatorsPack>();
            singleModField.ModificatorPacks = new List<ModificatorsPack>();
            ModificatorsPack tempPack = ModificatorsPack.CreateInstance<ModificatorsPack>();
            tempPack.FieldModificators = new List<FieldModification>();
            tempPack.FieldModificators.Add(singleModificator);
            singleModField.ModificatorPacks.Add(tempPack);

            return singleModField;
        }

        /// <summary>
        /// Generating temporary FieldSetup to be able to run single modificator on some grid
        /// </summary>
        public static FieldSetup GenerateTemporaryFieldSetupWith(InstructionDefinition command, FieldSetup scaleReferenceField )
        {
            FieldSetup singleModField = FieldSetup.CreateInstance<FieldSetup>();

            // Field Setup needs to know what scale to use
            FieldSetup parentField = scaleReferenceField;

            if (parentField == null)
            {
                UnityEngine.Debug.Log("[PGG - Rectangle Fill] Can't find parent FieldSetup of " + command.Title + " to define size of grid!");
                return null;
            }

            singleModField.CellSize = parentField.CellSize;
            singleModField.RootPack = ModificatorsPack.CreateInstance<ModificatorsPack>();
            singleModField.ModificatorPacks = new List<ModificatorsPack>();
            ModificatorsPack tempPack = ModificatorsPack.CreateInstance<ModificatorsPack>();
            tempPack.FieldModificators = new List<FieldModification>();
            tempPack.FieldModificators.Add(command.TargetModification);
            singleModField.ModificatorPacks.Add(tempPack);

            return singleModField;
        }


        /// <summary>
        /// Generating temporary FieldSetup to be able to run few selected modificators on some grid
        /// </summary>
        public static FieldSetup GenerateTemporaryFieldSetupWith(List<FieldModification> fewModificators, FieldSetup scaleReferenceField = null)
        {
            if (fewModificators.Count == 0 || fewModificators[0] == null)
            {
                UnityEngine.Debug.Log("[PGG - Temporary Field Setup] Modificators list don't have some required elements!");
                return null;
            }

            FieldSetup fewModsField = FieldSetup.CreateInstance<FieldSetup>();

            // Field Setup needs to know what scale to use
            FieldSetup parentField = scaleReferenceField;

            // If no scale reference pack provided let's try find modificators parent for it
            if (parentField == null)
            {
                parentField = fewModificators[0].TryGetParentSetup();
            }

            if (parentField == null)
            {
                UnityEngine.Debug.Log("[PGG - Rectangle Fill] Can't find parent FieldSetup of " + fewModificators + " to define size of grid!");
                return null;
            }

            fewModsField.CellSize = parentField.CellSize;
            fewModsField.RootPack = ModificatorsPack.CreateInstance<ModificatorsPack>();
            fewModsField.ModificatorPacks = new List<ModificatorsPack>();
            ModificatorsPack tempPack = ModificatorsPack.CreateInstance<ModificatorsPack>();
            tempPack.FieldModificators = new List<FieldModification>();
            for (int i = 0; i < fewModificators.Count; i++) tempPack.FieldModificators.Add(fewModificators[i]);
            fewModsField.ModificatorPacks.Add(tempPack);

            return fewModsField;
        }

        #endregion



        private static Bounds GetBounds(FGenGraph<FieldCell, FGenPoint> grid, List<GameObject> generateds, FieldSetup preset, Matrix4x4 transformMatrix, Vector3 worldOffset)
        {
            bool setted = false;
            Bounds modelBounds = new Bounds();

            for (int i = 0; i < generateds.Count; i++)
            {
                Renderer r = generateds[i].GetComponentInChildren<Renderer>();
                if (r)
                {
                    if (setted == false)
                    {
                        modelBounds = new Bounds(r.bounds.center, r.bounds.size);
                        setted = true;
                    }
                    else
                        modelBounds.Encapsulate(r.bounds);
                }
                else
                {
                    Collider c = generateds[i].GetComponentInChildren<Collider>();
                    if (c)
                    {
                        if (setted == false)
                        {
                            modelBounds = new Bounds(c.bounds.center, c.bounds.size);
                            setted = true;
                        }
                        else
                            modelBounds.Encapsulate(c.bounds);
                    }
                }
            }

            if (!setted) modelBounds.center += worldOffset;
            return modelBounds;
        }


        /// <summary>
        /// Converts bound to cell position starting in left down corner
        /// </summary>
        public static Vector3Int ConvertBoundsStartPosition(Bounds bound)
        {
            return new Vector3Int(Mathf.RoundToInt(bound.min.x), 0, Mathf.RoundToInt(bound.min.z));
        }

        /// <summary>
        /// Prepare upscaled grids for field setups
        /// </summary>
        public static FGenGraph<FieldCell, FGenPoint> GetScaledGrid(FGenGraph<FieldCell, FGenPoint> baseGraph, GeneratingPreparation prep,int scale, bool generate = true)
        {
            if (scale == 2)
            {
                if (prep._tempGraphScale2 == null) { if (generate) { prep._tempGraphScale2 = baseGraph.GenerateScaledGraph(scale); } else return null; }
                return prep._tempGraphScale2;
            }
            else if (scale == 3)
            {
                if (prep._tempGraphScale3 == null) { if (generate) prep._tempGraphScale3 = baseGraph.GenerateScaledGraph(scale); else return null; }
                return prep._tempGraphScale3;
            }
            else if (scale == 4)
            {
                if (prep._tempGraphScale4 == null) { if (generate) prep._tempGraphScale4 = baseGraph.GenerateScaledGraph(scale); else return null; }
                return prep._tempGraphScale4;
            }
            else if (scale == 5)
            {
                if (prep._tempGraphScale5 == null) { if (generate) prep._tempGraphScale5 = baseGraph.GenerateScaledGraph(scale); else return null; }
                return prep._tempGraphScale5;
            }
            else if (scale == 6)
            {
                if (prep._tempGraphScale6 == null) { if (generate) prep._tempGraphScale6 = baseGraph.GenerateScaledGraph(scale); else return null; }
                return prep._tempGraphScale6;
            }

            return baseGraph;
        }

    }
}