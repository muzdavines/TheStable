using FIMSpace.Generating.Checker;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.PathFind
{
    [System.Serializable]
    public class OutlineFillHelper
    {
        [Range(1, 5)] public int Thickness = 1;
        public CheckerField.ECheckerMeasureMode Mode = CheckerField.ECheckerMeasureMode.Rectangular;

        public enum ETypeToRun { FieldSetup, SingleModificator, ModPack, FieldCommand }

        [PGG_SingleLineSelector(new string[] { "FieldSetup", "Mod", "ModPack", "CommandName" }, 130, "", 70)]
        public ETypeToRun ToRun = ETypeToRun.FieldSetup;
        public FieldSetup FieldSetup;
        public FieldModification Mod;
        public ModificatorsPack ModPack;
        public string CommandName = "Field Outline Command Name";

        public List<InstantiatedFieldInfo> RunOnGenerator(PGGGeneratorBase generator)
        {
            if (generator.GeneratorCheckers == null || generator.GeneratorCheckers.Count == 0)
            {
                UnityEngine.Debug.Log("[PGG - Rectangle Fill] It seems outline fill is not implemented in this generator!");
                return null;
            }

            List<CheckerField> outlineShapes = new List<CheckerField>();

            for (int i = 0; i < generator.GeneratorCheckers.Count; i++)
            {
                outlineShapes.Add(generator.GeneratorCheckers[i].GetOutlineChecker(Thickness, Mode));
            }

            // Remove outlines overlappings with generator's checkers and with other outlines
            for (int i = 0; i < outlineShapes.Count; i++)
            {
                var outline = outlineShapes[i];

                // Remove overlaps with other generator shapes
                for (int ch = 0; ch < generator.GeneratorCheckers.Count; ch++) outline.RemoveOnesCollidingWith(generator.GeneratorCheckers[ch]);

                // Remove overlaps with other outlines
                for (int o = i - 1; o >= 0; o--) outline.RemoveOnesCollidingWith(outlineShapes[o]);
            }

            List<InstantiatedFieldInfo> outlinesGenerated = new List<InstantiatedFieldInfo>();

            #region Running Field Setup / Mod Pack / Mod depends what selected, on the checker fields

            bool loggedNoCommandRef = false; // Just preventing multiple logs in console

            for (int o = 0; o < outlineShapes.Count; o++)
            {
                if (FGenerators.CheckIfIsNull(outlineShapes[o])) continue;
                if (outlineShapes[o].CountSize() <= 1) continue;

                var grid = IGeneration.GetEmptyFieldGraph();
                outlineShapes[o].InjectToGrid(grid);

                if (ToRun == ETypeToRun.FieldSetup)
                {
                    if (FieldSetup != null)
                    {
                        outlinesGenerated.Add(IGeneration.GenerateFieldObjects(FieldSetup, grid, generator.transform, true, null, null, true, outlineShapes[o]));
                    }
                }
                else if (ToRun == ETypeToRun.ModPack)
                {
                    if (ModPack != null)
                    {
                        FieldSetup singlePackField = IGeneration.GenerateTemporaryFieldSetupWith(ModPack);
                        if (singlePackField) outlinesGenerated.Add(IGeneration.GenerateFieldObjects(singlePackField, grid, generator.transform, true, null, null, true, outlineShapes[o]));
                    }
                }
                else if (ToRun == ETypeToRun.SingleModificator)
                {
                    if (Mod != null)
                    {
                        FieldSetup singleModField = IGeneration.GenerateTemporaryFieldSetupWith(Mod);
                        if (singleModField) outlinesGenerated.Add(IGeneration.GenerateFieldObjects(singleModField, grid, generator.transform, true, null, null, true, outlineShapes[o]));
                    }
                }
                else if (ToRun == ETypeToRun.FieldCommand)
                {
                    if (string.IsNullOrEmpty(CommandName) == false)
                    {
                        if (outlineShapes[o].HelperReference == null)
                        {
                            if (!loggedNoCommandRef) UnityEngine.Debug.Log("[PGG - Outline] It seems that CheckerField don't have reference to FieldSetup which was using the CheckerField (not implemented in code?)");
                        }
                        else
                        {
                            InstructionDefinition command = outlineShapes[o].HelperReference.FindCellInstruction(CommandName);

                            if (FGenerators.CheckIfExist_NOTNULL(command))
                            {
                                FieldSetup singleModField = IGeneration.GenerateTemporaryFieldSetupWith(command, outlineShapes[o].HelperReference);
                                if (singleModField) outlinesGenerated.Add(IGeneration.GenerateFieldObjects(singleModField, grid, generator.transform, true, null, null, true, outlineShapes[o]));
                            }
                            else
                            {
                                UnityEngine.Debug.Log("[PGG - Outline] Can't find command called '" + CommandName + "' inside " + outlineShapes[o].HelperReference + " FieldSetup!");
                            }
                        }
                    }
                }

            }

            #endregion


            return outlinesGenerated;
        }
    }

}