using FIMSpace.Generating.Checker;
using FIMSpace.Generating.Rules;
using FIMSpace.Hidden;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.PathFind
{
    [System.Serializable]
    public class RectangleFillHelper
    {
        public Vector2Int Center = new Vector2Int(0, 0);
        public Vector2Int Size = new Vector2Int(8, 8);

        public enum ETypeToRun { FieldSetup, SingleModificator, ModPack }

        [PGG_SingleLineSelector(new string[] { "FieldSetup", "Mod", "ModPack" }, 130, "", 70)]
        public ETypeToRun ToRun = ETypeToRun.FieldSetup;
        public FieldSetup FieldSetup;
        public FieldModification Mod;
        public ModificatorsPack ModPack;

        public InstantiatedFieldInfo RunOnGenerator(PGGGeneratorBase generator)
        {
            if (generator.GeneratorCheckers == null || generator.GeneratorCheckers.Count == 0)
            {
                UnityEngine.Debug.Log("[PGG - Rectangle Fill] It seems rect fill is not implemented in this generator!");
                return null;
            }

            CheckerField inverseRect = new CheckerField();
            inverseRect.SetSize(Size, Size.Divide(2) );
            inverseRect.Position = inverseRect.FromWorldToGridPos(Center);

            for (int i = 0; i < generator.GeneratorCheckers.Count; i++)
            {
                inverseRect.RemoveOnesCollidingWith(generator.GeneratorCheckers[i], false);
            }


            #region Running Field Setup / Mod Pack / Mod depends what selected, on the checker field

            var grid = IGeneration.GetEmptyFieldGraph();
            inverseRect.InjectToGrid(grid);

            if ( ToRun == ETypeToRun.FieldSetup)
            {
                if ( FieldSetup != null)
                {
                    return IGeneration.GenerateFieldObjects(FieldSetup, grid, generator.transform, true, null, null, true, inverseRect);
                }
            }
            else if (ToRun == ETypeToRun.ModPack)
            {
                if (ModPack != null)
                {
                    FieldSetup singlePackField = IGeneration.GenerateTemporaryFieldSetupWith(ModPack);
                    if ( singlePackField) return IGeneration.GenerateFieldObjects(singlePackField, grid, generator.transform, true, null, null, true, inverseRect);
                }
            }
            else if (ToRun == ETypeToRun.SingleModificator)
            {
                if (Mod != null)
                {
                    FieldSetup singleModField = IGeneration.GenerateTemporaryFieldSetupWith(Mod);
                    if ( singleModField) return IGeneration.GenerateFieldObjects(singleModField, grid, generator.transform, true, null, null, true, inverseRect);
                }
            }

            #endregion


            return null;
        }
    }

}