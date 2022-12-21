using UnityEngine;
using System.Collections.Generic;

namespace FIMSpace.Generating
{
    public partial class FieldDesignWindow
    {
        FGenGraph<FieldCell, FGenPoint> grid;
        List<FieldCell> randomizedGridCells;
        List<FieldCell> randomizedGridCells2;
        //List<GuideDrawer> guides = new List<GuideDrawer>();
        //List<RestrictionDrawer> restrictions = new List<RestrictionDrawer>();

        /// <summary>
        /// Generating base grid to run rules on
        /// </summary>
        void GenerateBaseFieldGrid()
        {
            ResetRandomizing(-1);

            grid = new FGenGraph<FieldCell, FGenPoint>();
            grid.Generate(SizeX.GetRandom(), SizeY.GetRandom(), SizeZ.GetRandom(), OffsetGrid);

            if (gridMode == EDesignerGridMode.BranchedGeneration)
            {
                Vector3Int currentPos = new Vector3Int(OffsetGrid.x, 0, OffsetGrid.y);

                for (int b = 0; b < TargetBranches.GetRandom(); b++)
                {
                    float rand = FGenerators.GetRandom(0, 4);

                    Vector3Int direction;
                    if (rand == 0) direction = new Vector3Int(1, 0, 0);
                    else if (rand == 1) direction = new Vector3Int(0, 0, 1);
                    else if (rand == 2) direction = new Vector3Int(-1, 0, 0);
                    else direction = new Vector3Int(0, 0, -1);

                    int fatness = CellsSpace.GetRandom();
                    if (fatness < 1) fatness = 1;

                    for (int l = 0; l < BranchLength.GetRandom(); l++)
                    {
                        grid.AddCell(currentPos.x, currentPos.y, currentPos.z);

                        for (int x = 0; x < fatness; x++)
                            for (int z = 0; z < fatness; z++)
                                grid.AddCell(currentPos.x + x, currentPos.y, currentPos.z + z);

                        currentPos += direction;
                    }
                }
            }

            repaint = true;
        }

        public bool UsePainterGenerating { get { return applyChangesOnPainter && usedPainter; } }

        /// <summary>
        /// Running preset modificators rules on grid
        /// </summary>
        public void RunFieldCellsRules()
        {
            if (grid == null) return;
            if (projectPreset == null) return;

            IGeneration.ClearCells(grid);
            ResetRandomizing();

            randomizedGridCells = IGeneration.GetRandomizedCells(grid);
            randomizedGridCells2 = IGeneration.GetRandomizedCells(grid);

            if (projectPreset.RequiresScaledGraphs())
                projectPreset.PrepareSubGraphs(grid);

            IGeneration.PreparePresetVariables(projectPreset);
            projectPreset.RunPreInstructionsOnGraph(grid, null);
            projectPreset.RunRulesOnGraph(grid, randomizedGridCells, randomizedGridCells2, null);
            repaint = true;
        }


        /// <summary>
        /// Spawning room prefabs
        /// </summary>
        public void RunFieldCellsSpawningGameObjects()
        {
            if (projectPreset == null) return;
            ClearAllGeneratedGameObjects();
            if (grid == null) GenerateBaseFieldGrid();

            generated = IGeneration.GenerateFieldObjects(projectPreset, grid, GetGeneratorSceneContainer().transform, false, null, null, RunAdditionalGenerators);

            if (SendMessageAfterGenerateTo)
            {
                SendMessageAfterGenerateTo.SendMessage(PostGenerateMessage, SendMessageOptions.DontRequireReceiver);
            }

            repaint = true;
        }


        #region Designer Utilities


        //List<SpawnInstruction> GetTestGuides()
        //{
        //    List<SpawnInstruction> guides = new List<SpawnInstruction>();
        //    for (int i = 0; i < this.guides.Count; i++) guides.Add(this.guides[i].guide);
        //    return guides;
        //}

        //List<SpawnRestrictions> GetTestRestrictions()
        //{
        //    List<SpawnRestrictions> restr = new List<SpawnRestrictions>();
        //    for (int i = 0; i < this.restrictions.Count; i++) restr.Add(this.restrictions[i].restrictions);
        //    return restr;
        //}

        void ClearAllGeneratedGameObjects()
        {
            if (generated == null) return;
            if (generated.Instantiated == null) return;

            for (int i = 0; i < generated.Instantiated.Count; i++)
            {
                if (generated.Instantiated[i] == null) continue;
                FGenerators.DestroyObject(generated.Instantiated[i]);
            }

            if (generated.MainContainer != null)
                FGenerators.DestroyObject(generated.MainContainer);

            GameObject c = GetGeneratorSceneContainer();
            if (c) FGenerators.DestroyObject(c);

            generated.Instantiated.Clear();
        }


        public void ResetRandomizing(int offset = 0)
        {
            if (Seed == 0)
                FGenerators.SetSeed(Random.Range(-9999, 9999) + offset);
            else
                FGenerators.SetSeed(Seed + offset);
        }


        GameObject GetGeneratorSceneContainer()
        {
            if (mainGeneratedsContainer == null)
            {
                if (projectPreset != null)
                    mainGeneratedsContainer = new GameObject(projectPreset.name + "-Container");
                else
                    mainGeneratedsContainer = new GameObject("Generator-Container");
            }

            return mainGeneratedsContainer;
        }



        #endregion

    }
}