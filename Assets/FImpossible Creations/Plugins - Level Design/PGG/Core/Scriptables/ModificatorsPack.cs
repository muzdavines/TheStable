using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    /// <summary>
    /// It's never sub-asset except being FieldSetup root modification package
    /// Most of the code is inside Editor class of ModificatorsPack
    /// </summary>
    [CreateAssetMenu(fileName = "ModPack_", menuName = "FImpossible Creations/Procedural Generation/Grid Field Modifications Pack", order = 102)]
    public partial class ModificatorsPack : ScriptableObject
    {
        [HideInInspector] public List<FieldModification> FieldModificators = new List<FieldModification>();
        [HideInInspector] public FieldSetup ParentPreset;
        public static bool _Editor_LockBrowser = false;
        public bool DisableWholePackage = false;
        public ESeedMode SeedMode = ESeedMode.None;

        public enum ESeedMode { None, Reset, Custom }
        public int CustomSeed = 0;

        public enum EModPackType { Base, ShallowCopy, VariantCopy }
        public EModPackType ModPackType = EModPackType.Base;

        [Tooltip("WARNING: You need to know what you're doing!\nAdding tag for all spawners executed by package")]
        public string TagForAllSpawners = "";

        public enum EPackCombine { None, CombineAll, CombineAllAndSetStatic }
        [Tooltip("Forcing all spawned object of this mods package to be combined into single mesh (per material) after being generated")]
        public EPackCombine CombineSpawns = EPackCombine.None;

        [HideInInspector] public List<FieldVariable> Variables = new List<FieldVariable>();

        /// <summary> Required container for 'CallOnAllSpawners' Spawner </summary>
        [HideInInspector] public FieldModification CallOnAllMod;
        [HideInInspector] public FieldSpawner CallOnAllSpawners;
        [HideInInspector] public bool _EditorDisplayCallOnAll = false;

        /// <summary> Just flag for checking if pack is contained by FieldSetup, unity is not detecting relations properly when using hideFlags on the pack so we check asset paths one time for a compilation </summary>
        [NonSerialized] internal bool PathsChecked = false;

        internal FieldVariable GetVariable(string name)
        {
            for (int i = 0; i < Variables.Count; i++)
                if (Variables[i].Name == name) return Variables[i];

            return null;
        }

        internal void PrepareSeed()
        {
            if (SeedMode != ModificatorsPack.ESeedMode.None)
            {
                if (SeedMode == ModificatorsPack.ESeedMode.Reset)
                {
                    FGenerators.SetSeed(FGenerators.LatestSeed);
                }
                else if (SeedMode == ModificatorsPack.ESeedMode.Custom)
                {
                    FGenerators.SetSeed(CustomSeed);
                }
            }
        }

        internal List<FieldModification> GetModListToRun(GeneratingPreparation preparation)
        {
            List<FieldModification> mods = new List<FieldModification>();

            for (int r = 0; r < FieldModificators.Count; r++)
            {
                if (FieldModificators[r] == null) continue;
                if (FieldModificators[r].Enabled == false) continue;
                //if (preparation.IgnoredModificationsForGenerating.Contains(FieldModificators[r])) continue;
                mods.Add(FieldModificators[r]);
            }

            return mods;
        }

        public FieldSetup TryGetParentSetup()
        {
            if (ParentPreset) return ParentPreset;

            for (int i = 0; i < FieldModificators.Count; i++)
            {
                FieldSetup fs = FieldModificators[i].TryGetParentSetup();
                if (fs) return fs;
            }

            return null;
        }

        public void RefreshModsSpawnersOwners()
        {
            if (FieldModificators == null) return;

            for (int m = 0; m < FieldModificators.Count; m++)
            {
                var mod = FieldModificators[m];
                if (mod == null) continue;
                if (mod.Enabled == false) continue;
                mod.RefreshSpawnersOwners();
            }
        }
    }
}