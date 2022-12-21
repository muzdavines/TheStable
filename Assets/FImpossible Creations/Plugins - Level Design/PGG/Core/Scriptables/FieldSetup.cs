using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    /// <summary>
    /// It's never sub-asset -> it's always project file asset
    /// </summary>
    [CreateAssetMenu(fileName = "FieldSetup_", menuName = "FImpossible Creations/Procedural Generation/Grid Field Setup (Spawning)", order = 100)]
    public partial class FieldSetup : ScriptableObject
    {
        public float CellSize = 2f;
        public Vector3 GetCellUnitSize() { if (NonUniformSize == false) return new Vector3(CellSize, CellSize, CellSize); else return NonUniformCellSize; }

        public bool NonUniformSize = false;
        public Vector3 NonUniformCellSize = new Vector3(2f, 2f, 2f);

        public string InfoText = "Here write custom note about FieldSetup";

        public List<FieldVariable> Variables = new List<FieldVariable>();

        [SerializeField][HideInInspector] private FieldModification DoorsModificator;
        [SerializeField][HideInInspector] private FieldModification EraseWallModificator;

        public List<InstructionDefinition> CellsInstructions;
        /// <summary> Returning 'CellsInstructions' but just with different name </summary>
        public List<InstructionDefinition> CellsCommands { get { return CellsInstructions; } }

        /// <summary> When generating copy of FieldSetup it is remembered </summary>
        public FieldSetup InstantiatedOutOf { get; private set; }

        [HideInInspector] public ModificatorsPack RootPack;
        /// <summary> Modificators for commands etc. </summary>
        [HideInInspector] public List<FieldModification> UtilityModificators = new List<FieldModification>();
        public List<ModificatorsPack> ModificatorPacks = new List<ModificatorsPack>();
        public List<FieldModification> Ignores = new List<FieldModification>();
        internal List<InjectionSetup> temporaryInjections = new List<InjectionSetup>();
        public string DontSpawnOn;
        public List<InjectionSetup> SelfInjections;

        [SerializeField][HideInInspector] private List<FieldModification> disabledMods = new List<FieldModification>();


        private void Awake()
        {
            if (CellsInstructions == null || CellsInstructions.Count == 0)
            {
                CellsInstructions = new List<InstructionDefinition>();
                InstructionDefinition def = new InstructionDefinition();
                def.Title = "Door Hole";
                def.TargetModification = DoorsModificator;
                def.Tags = "Props";
                def.InstructionType = InstructionDefinition.EInstruction.DoorHole;
                CellsInstructions.Add(def);

                def = new InstructionDefinition();
                def.Title = "Clear Wall";
                def.TargetModification = EraseWallModificator;
                def.InstructionType = InstructionDefinition.EInstruction.PostRunModificator;
                CellsInstructions.Add(def);
            }

            if (Variables == null || Variables.Count == 0)
            {
                Variables = new List<FieldVariable>();
                FieldVariable def = new FieldVariable("Spawn Propability Multiplier", 1f);
                def.helper.x = 0; def.helper.y = 5;
                Variables.Add(def);

                def = new FieldVariable("Spawn Count Multiplier", 1f);
                def.helper.x = 0; def.helper.y = 5;
                Variables.Add(def);
            }

        }


        #region Handling multiple scale graphs


        internal FGenGraph<FieldCell, FGenPoint> _tempGraphScale2;
        internal FGenGraph<FieldCell, FGenPoint> _tempGraphScale3;
        internal FGenGraph<FieldCell, FGenPoint> _tempGraphScale4;
        internal FGenGraph<FieldCell, FGenPoint> _tempGraphScale5;
        internal FGenGraph<FieldCell, FGenPoint> _tempGraphScale6;



        //public void PrepareGraph()
        //{
        //    for (int mp = 0; mp < ModificatorPacks.Count; mp++)
        //    {
        //        for (int md = 0; md < ModificatorPacks[mp].FieldModificators.Count; md++)
        //        {
        //            for (int sp = 0; sp < ModificatorPacks[mp].FieldModificators[md].Spawners.Count; sp++)
        //            {
        //                var spawner = ModificatorPacks[mp].FieldModificators[md].Spawners[sp];
        //                if (spawner.OnScalledGrid > 1)
        //                {

        //                }
        //            }
        //        }
        //    }
        //}

        // Clearing all temporary scaled graphs variables
        private void ResetScaledGraphs()
        {
            _tempGraphScale2 = null;
            _tempGraphScale3 = null;
            _tempGraphScale4 = null;
            _tempGraphScale5 = null;
            _tempGraphScale6 = null;
        }

        // Construct scaled graphs out of already placed cells
        public void PrepareSubGraphs(FGenGraph<FieldCell, FGenPoint> grid)
        {
            ResetScaledGraphs();

            if (grid.SubGraphs != null) grid.SubGraphs.Clear();

            grid.SubGraphs = new List<FGenGraph<FieldCell, FGenPoint>>();

            for (int s = 2; s <= 6; s++)
            {
                var gr = GetScaledGrid(grid, s, true);
                if (gr != null) grid.SubGraphs.Add(gr);
            }
        }

        public FGenGraph<FieldCell, FGenPoint> GetScaledGrid(FGenGraph<FieldCell, FGenPoint> baseGraph, int scale, bool generate = true)
        {
            if (scale == 2)
            {
                if (_tempGraphScale2 == null) { if (generate) { _tempGraphScale2 = baseGraph.GenerateScaledGraph(scale); } else return null; }
                return _tempGraphScale2;
            }
            else if (scale == 3)
            {
                if (_tempGraphScale3 == null) { if (generate) _tempGraphScale3 = baseGraph.GenerateScaledGraph(scale); else return null; }
                return _tempGraphScale3;
            }
            else if (scale == 4)
            {
                if (_tempGraphScale4 == null) { if (generate) _tempGraphScale4 = baseGraph.GenerateScaledGraph(scale); else return null; }
                return _tempGraphScale4;
            }
            else if (scale == 5)
            {
                if (_tempGraphScale5 == null) { if (generate) _tempGraphScale5 = baseGraph.GenerateScaledGraph(scale); else return null; }
                return _tempGraphScale5;
            }
            else if (scale == 6)
            {
                if (_tempGraphScale6 == null) { if (generate) _tempGraphScale6 = baseGraph.GenerateScaledGraph(scale); else return null; }
                return _tempGraphScale6;
            }

            return baseGraph;
        }

        #endregion


        /// <summary>
        /// Checking if all required references exists
        /// </summary>
        public void Validate()
        {
            if (RootPack == null)
            {
                RootPack = CreateInstance<ModificatorsPack>();
                RootPack.name = "Root";
                FGenerators.AddScriptableTo(RootPack, this, false);
            }

            if (RootPack != null)
            {
#if UNITY_EDITOR
                if (RootPack.PathsChecked == false)
                {
                    if (UnityEditor.AssetDatabase.GetAssetPath(RootPack) == UnityEditor.AssetDatabase.GetAssetPath(this))
                    {
                        RootPack.PathsChecked = true; // Same paths in case hide flags on the root pack
                    }
                }

                if (RootPack.PathsChecked == false)
                {
                    if (UnityEditor.AssetDatabase.Contains(this))
                    {
                        if (FGenerators.AssetContainsAsset(RootPack, this) == false)
                        {
                            FGenerators.AddScriptableTo(RootPack, this, false);
                        }
                    }
                }
#endif

                RootPack.ParentPreset = this;
            }

            if (ModificatorPacks == null)
            {
                ModificatorPacks = new List<ModificatorsPack>();
            }

            int rootPacks = 0;
            for (int i = ModificatorPacks.Count - 1; i >= 0; i--)
            {
                if (ModificatorPacks[i] == RootPack)
                {
                    rootPacks += 1;
                    if (rootPacks > 1)
                    {
                        ModificatorPacks.RemoveAt(i);
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(this);
#endif
                    }
                }
            }

        }

        internal FieldSetup Copy()
        {
            FieldSetup fs = Instantiate(this);
            fs.InstantiatedOutOf = this;
            return fs;
        }

        internal void ClearTemporaryContainers()
        {
            for (int p = 0; p < ModificatorPacks.Count; p++)
            {
                if (ModificatorPacks[p] == null) continue;

                for (int r = 0; r < ModificatorPacks[p].FieldModificators.Count; r++)
                {
                    if (ModificatorPacks[p].FieldModificators[r] == null) continue;
                    ModificatorPacks[p].FieldModificators[r].TemporaryContainer = null;
                }
            }

            if (DoorsModificator != null) DoorsModificator.TemporaryContainer = null;
            if (EraseWallModificator != null) EraseWallModificator.TemporaryContainer = null;
        }

        internal void RunTemporaryPreInjections(CellsController generationScheme)
        {
            if (temporaryInjections != null)
                for (int i = 0; i < temporaryInjections.Count; i++)
                {
                    if (temporaryInjections[i].Call == InjectionSetup.EGridCall.Pre)
                    {
                        if (temporaryInjections[i].Inject == InjectionSetup.EInjectTarget.Modificator)
                        {
                            if (temporaryInjections[i].Modificator != null)
                                generationScheme.RunModification(temporaryInjections[i].Modificator);
                        }
                        else if (temporaryInjections[i].Inject == InjectionSetup.EInjectTarget.Pack)
                        {
                            if (temporaryInjections[i].ModificatorsPack != null)
                                if (temporaryInjections[i].ModificatorsPack.DisableWholePackage == false)
                                    generationScheme.RunModificatorPack(temporaryInjections[i].ModificatorsPack);
                        }
                    }

                }
        }

        public void MoveModificatorToUtilityList(FieldModification modd, bool moveFromUtilityToRootPack = false)
        {
            if (!moveFromUtilityToRootPack) // Move to utility pack
            {
                if (!RootPack.FieldModificators.Contains(modd)) return;
                if (UtilityModificators.Contains(modd)) return;

                RootPack.FieldModificators.Remove(modd);
                UtilityModificators.Add(modd);

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
            else // Move to root pack
            {
                if (!UtilityModificators.Contains(modd)) return;
                if (RootPack.FieldModificators.Contains(modd)) return;

                RootPack.FieldModificators.Add(modd);
                UtilityModificators.Remove(modd);

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }


        public void AfterAllGenerating()
        {
            for (int i = 0; i < ModificatorPacks.Count; i++)
            {
                if (ModificatorPacks[i].DisableWholePackage) continue;

                for (int m = 0; m < ModificatorPacks[i].FieldModificators.Count; m++)
                {
                    AfterallGeneratingMod(ModificatorPacks[i].FieldModificators[m]);
                }
            }

            for (int i = 0; i < UtilityModificators.Count; i++)
            {
                AfterallGeneratingMod(UtilityModificators[i]);
            }

            for (int i = 0; i < CellsCommands.Count; i++)
            {
                if (CellsCommands[i].TargetModification)
                    if (!UtilityModificators.Contains(CellsCommands[i].TargetModification))
                        AfterallGeneratingMod(CellsCommands[i].TargetModification);

                if (CellsCommands[i].extraMod)
                    if (!UtilityModificators.Contains(CellsCommands[i].extraMod))
                        AfterallGeneratingMod(CellsCommands[i].extraMod);
            }
        }

        void AfterallGeneratingMod(FieldModification mod)
        {
            if (mod == null) return;
            if (mod.Enabled == false) return;

            for (int s = 0; s < mod.Spawners.Count; s++)
            {
                if (mod.Spawners[s].Enabled == false) continue;
                mod.Spawners[s].AfterGeneratingCall();
            }

            for (int s = 0; s < mod.SubSpawners.Count; s++)
            {
                if (mod.SubSpawners[s].Enabled == false) continue;
                mod.SubSpawners[s].AfterGeneratingCall();
            }
        }

    }
}