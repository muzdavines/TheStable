using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class FieldSetupComposition
    {
        /// <summary>
        /// Overriding Mod Pack variables and modificators within (with additional class 'ModOverrideHelper')
        /// </summary>
        [System.Serializable]
        public class PackOverrideHelper
        {
            public ModificatorsPack ParentPack;
            public bool SetEnabled = true;

            public List<FieldVariable> PackVariablesOverrides = new List<FieldVariable>();
            public List<ModOverrideHelper> PackModsOverrides = new List<ModOverrideHelper>();
            
            public void ReInitializeWith(ModificatorsPack pack)
            {
                ParentPack = pack;

                if (PackVariablesOverrides == null) PackVariablesOverrides = new List<FieldVariable>();
                if (PackModsOverrides == null) PackModsOverrides = new List<ModOverrideHelper>();

                PackModsOverrides.Clear();
                PackModsOverrides.Add(new ModOverrideHelper());

                FieldVariable.UpdateVariablesWith(PackVariablesOverrides, ParentPack.Variables);
                PGGUtils.AdjustCount(PackModsOverrides, ParentPack.FieldModificators.Count);

                SetEnabled = !pack.DisableWholePackage;

                if (PackModsOverrides.Count == ParentPack.FieldModificators.Count)
                    for (int i = 0; i < ParentPack.FieldModificators.Count; i++)
                    {
                        if (PackModsOverrides[i] == null) PackModsOverrides[i] = new ModOverrideHelper();
                        PackModsOverrides[i].SetEnabled = (ParentPack.FieldModificators[i].Enabled);
                        PackModsOverrides[i].UpdateModsCountWith(ParentPack.FieldModificators[i]);
                    }
            }

            public void RefreshFieldVariablesCount()
            {
                FieldVariable.UpdateVariablesWith(PackVariablesOverrides, ParentPack.Variables);
            }

            public ModOverrideHelper GetOverridesFor(FieldModification mod)
            {
                for (int i = 0; i < PackModsOverrides.Count; i++)
                {
                    if (PackModsOverrides[i].ParentMod == mod) return PackModsOverrides[i];
                }

                return null;
            }

            internal void AdjustFieldSetupVariables()
            {
                PGGUtils.AdjustCount(PackVariablesOverrides, ParentPack.Variables.Count, true);

                for (int i = 0; i < PackVariablesOverrides.Count; i++)
                {
                    if (PackVariablesOverrides[i] == null)
                        PackVariablesOverrides[i] = ParentPack.Variables[i].Copy();
                }
            }
        }

        internal Vector3 GetCellSize()
        {
            if (OverrideCellSize)
            {
                return OverridingCellSize;
            }

            if (GenType == EPGGGenType.FieldSetup)
            {
                return Setup.GetCellUnitSize();
            }

            return Vector3.one;
        }

        internal void RefreshPlannerVariablesCount(List<FieldVariable> variables)
        {
            PGGUtils.AdjustCount(PlannerVariablesOverrides, variables.Count);

            for (int i = 0; i < variables.Count; i++)
            {
                PlannerVariablesOverrides[i].UpdateVariableWith(variables[i]);
            }
        }


        /// <summary>
        /// Overriding prefabs of the field modificator
        /// </summary>
        [System.Serializable]
        public class ModOverrideHelper
        {
            public FieldModification ParentMod;
            //
            public bool SetEnabled = true;
            public List<PrefabReference> OverridePrefabs = new List<PrefabReference>();
            public OStamperSet OverrideStampSet;
            public OStamperMultiSet OverrideMultiSet;

            internal void UpdateModsCountWith(FieldModification fieldModification)
            {
                ParentMod = fieldModification;
                if (OverridePrefabs == null) OverridePrefabs = new List<PrefabReference>();
                PGGUtils.AdjustCount(OverridePrefabs, ParentMod.PrefabsList.Count, false);
            }
        }

        internal void AdjustFieldSetupVariables()
        {
            FieldSetup setup = Setup;
            if (JustMod) setup = JustMod.TryGetParentSetup();
            else if (JustModPack) setup = JustModPack.ParentPreset;
            if (setup == null) return;

            PGGUtils.AdjustCount(FieldSetupVariablesOverrides, setup.Variables.Count, true);

            for (int i = 0; i < FieldSetupVariablesOverrides.Count; i++)
            {
                if (FieldSetupVariablesOverrides[i] == null)
                    FieldSetupVariablesOverrides[i] = setup.Variables[i].Copy();
            }
        }

        internal void AdjustModPacksCount()
        {
            int preCount = Setup.ModificatorPacks.Count;

            PGGUtils.AdjustCount(FieldPackagesOverrides, Setup.ModificatorPacks.Count);

            bool refreshNeeded = false;
            if (FieldPackagesOverrides.Count != preCount) refreshNeeded = true;

            if (!refreshNeeded)
            {
                for (int i = 0; i < FieldPackagesOverrides.Count; i++)
                    if (FieldPackagesOverrides[i].ParentPack == null)
                    {
                        refreshNeeded = true;
                        break;
                    }
            }

            if (!refreshNeeded) return;

            ReinitializePacks();
        }

        void ReinitializePacks()
        {
            for (int i = 0; i < Setup.ModificatorPacks.Count; i++)
            {
                if (FieldPackagesOverrides[i] == null) FieldPackagesOverrides[i] = new PackOverrideHelper();
                FieldPackagesOverrides[i].ReInitializeWith(Setup.ModificatorPacks[i]);
            }
        }

        internal void AdjustUtilityModsCount()
        {
            int preCount = Setup.UtilityModificators.Count;
            PGGUtils.AdjustCount(UtilityModsOverrides, Setup.UtilityModificators.Count);

            bool refreshNeeded = false;
            if (UtilityModsOverrides.Count != preCount) refreshNeeded = true;

            if (!refreshNeeded)
            {
                for (int i = 0; i < UtilityModsOverrides.Count; i++)
                    if (UtilityModsOverrides[i].ParentMod == null)
                    {
                        refreshNeeded = true;
                        break;
                    }
            }

            if (!refreshNeeded) return;

            for (int i = 0; i < Setup.UtilityModificators.Count; i++)
            {
                if (UtilityModsOverrides[i] == null) UtilityModsOverrides[i] = new ModOverrideHelper();
                UtilityModsOverrides[i].UpdateModsCountWith(Setup.UtilityModificators[i]);
            }
        }


        internal FieldSetup GetOverridedSetup()
        {
            FieldSetup nSetup;

            if (GenType == EPGGGenType.FieldSetup)
                nSetup = ScriptableObject.Instantiate(Setup);
            else
                nSetup = ScriptableObject.CreateInstance<FieldSetup>();


            if (OverrideCellSize)
            {
                nSetup.CellSize = OverridingCellSize.x;
                nSetup.NonUniformSize = true;
                nSetup.NonUniformCellSize = OverridingCellSize;
            }


            if (GenType == EPGGGenType.FieldSetup)
            {
                for (int v = 0; v < nSetup.Variables.Count; v++)
                    nSetup.Variables[v].SetValue(FieldSetupVariablesOverrides[v]);

                for (int v = 0; v < Setup.CellsCommands.Count; v++)
                {
                    if (Setup.CellsCommands[v].extraMod)
                        nSetup.CellsCommands[v].extraMod = ScriptableObject.Instantiate(Setup.CellsCommands[v].extraMod);
                }

                for (int u = 0; u < Setup.UtilityModificators.Count; u++)
                {
                    nSetup.UtilityModificators[u] = ScriptableObject.Instantiate(Setup.UtilityModificators[u]);
                    ApplyOverridesOf(nSetup.UtilityModificators[u], UtilityModsOverrides[u]);

                    for (int c = 0; c < nSetup.CellsCommands.Count; c++)
                    {
                        var comm = nSetup.CellsCommands[c];
                        if (Setup.CellsCommands[c].TargetModification == Setup.UtilityModificators[u]) comm.TargetModification = nSetup.UtilityModificators[u];
                        if (Setup.CellsCommands[c].extraMod == Setup.UtilityModificators[u]) comm.extraMod = nSetup.UtilityModificators[u];
                    }
                }


                for (int p = 0; p < Setup.ModificatorPacks.Count; p++)
                {
                    nSetup.ModificatorPacks[p] = PrepareOverrideExecutionForModPack(Setup.ModificatorPacks[p], FieldPackagesOverrides[p]);
                }

            }
            else // else than FieldSetup
            {
                nSetup.Variables = FieldSetupVariablesOverrides;
                FieldSetup parentSetup = GetSetup;

                if (parentSetup)
                {
                    nSetup.CellsInstructions = parentSetup.CellsCommands;
                }

                if (GenType == EPGGGenType.Modificator)
                {
                    if (nSetup.RootPack == null) nSetup.RootPack = ModificatorsPack.CreateInstance<ModificatorsPack>();
                    FieldModification mod = FieldModification.Instantiate(JustMod);
                    ApplyOverridesOf(mod, UtilityModsOverrides[0]);
                    nSetup.RootPack.FieldModificators.Add(mod);
                    nSetup.ModificatorPacks.Add(nSetup.RootPack);
                }
                else if (GenType == EPGGGenType.ModPack)
                {
                    //if (nSetup.RootPack == null) nSetup.RootPack = ModificatorsPack.CreateInstance<ModificatorsPack>();
                    nSetup.RootPack = PrepareOverrideExecutionForModPack(JustModPack, FieldPackagesOverrides[0]);
                    nSetup.ModificatorPacks.Add(nSetup.RootPack);
                }

            }

            return nSetup;
        }

        public ModificatorsPack PrepareOverrideExecutionForModPack(ModificatorsPack sourcePack, PackOverrideHelper overrides)
        {
            ModificatorsPack pack = ScriptableObject.Instantiate(sourcePack);

            if (overrides.SetEnabled == false)
            {
                pack.DisableWholePackage = true;
            }

            for (int v = 0; v < pack.Variables.Count; v++)
            {
                pack.Variables[v].SetValue(overrides.PackVariablesOverrides[v]);
            }

            for (int m = 0; m < pack.FieldModificators.Count; m++)
            {
                pack.FieldModificators[m] = ScriptableObject.Instantiate(sourcePack.FieldModificators[m]);
                var mod = pack.FieldModificators[m];

                if (overrides.PackModsOverrides[m].SetEnabled == false)
                {
                    pack.FieldModificators[m].Enabled = false;
                }

                if (Setup != null)
                    if (Setup.IsModDisabledForThisSetup(sourcePack.FieldModificators[m]))
                    {
                        mod.Enabled = false;
                    }

                for (int s = 0; s < mod.PrefabsList.Count; s++)
                {
                    var modOverr = overrides.PackModsOverrides[m];
                    ApplyOverridesOf(mod, modOverr);
                }
            }

            return pack;
        }

        public void ApplyOverridesOf(FieldModification mod, ModOverrideHelper overrides)
        {
            if (mod.DrawSetupFor == FieldModification.EModificationMode.CustomPrefabs)
            {
                if (mod.PrefabsList.Count != overrides.OverridePrefabs.Count)
                {
                    //UnityEngine.Debug.Log("Mod " + mod.name + " override mod = " + overrides.ParentMod.name);
                }
                else
                {
                    for (int o = 0; o < mod.PrefabsList.Count; o++)
                    {
                        if (overrides.OverridePrefabs[o].GameObject == null) { continue; }
                        mod.PrefabsList[o].SetPrefab ( overrides.OverridePrefabs[o].CoreGameObject);
                        mod.PrefabsList[o].SetCollider ( overrides.OverridePrefabs[o].CoreCollider);
                    }
                }
            }
            else if (mod.DrawSetupFor == FieldModification.EModificationMode.ObjectMultiEmitter)
            {
                mod.OMultiStamp = overrides.OverrideMultiSet;
            }
            else if (mod.DrawSetupFor == FieldModification.EModificationMode.ObjectsStamp)
            {
                mod.OStamp = overrides.OverrideStampSet;
            }
        }

        internal FieldSetupComposition Copy()
        {
            return (FieldSetupComposition)MemberwiseClone();
        }

        /// <summary> Initial prepare single pack composition override </summary>
        internal void RefreshModPackSetup()
        {
            if (!JustModPack) return;
            if (FieldPackagesOverrides == null) FieldPackagesOverrides = new List<PackOverrideHelper>();
            if (FieldPackagesOverrides.Count == 0) FieldPackagesOverrides.Add(new PackOverrideHelper());
            RefreshWith(JustModPack);
        }

        /// <summary> Initial prepare single modificator composition override </summary>
        internal void RefreshModSetup()
        {
            if (!JustMod) return;
            if (UtilityModsOverrides == null) UtilityModsOverrides = new List<ModOverrideHelper>();
            if (UtilityModsOverrides.Count == 0) UtilityModsOverrides.Add(new ModOverrideHelper());
            ApplyOverridesOf(JustMod, UtilityModsOverrides[0]);
        }

        internal void RefreshWith(ModificatorsPack justModPack)
        {
            if (FieldPackagesOverrides[0] == null) FieldPackagesOverrides[0] = new PackOverrideHelper();
            FieldPackagesOverrides[0].ReInitializeWith(justModPack);
        }
    }
}