using FIMSpace.Generating.Rules;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    [System.Serializable]
    public partial class FieldSpawner
    {
        public bool Enabled = true;
        public string Name;
        public string SpawnerTag = "";

        internal bool Prepared = false;

        public int StampPrefabID;
        public FieldModification Parent;
        public List<SpawnRuleBase> Rules = new List<SpawnRuleBase>();
        public FieldModification.EModificationMode Mode = FieldModification.EModificationMode.CustomPrefabs;

        public enum ESR_CellOrder { Ordered, Reversed, Random, RandomReversed, Random2, Random2Reversed, TotalRandom }
        public ESR_CellOrder CellCheckMode = ESR_CellOrder.Ordered;
        public bool _Editor_SpawnerAdvancedOptionsFoldout = false;
        [Tooltip("Multiplying cells used by spawner, if set to 2 then spawn will have size of 4 cells (2x2)\nIt will generate additional grid basing on size 1 grid, you can manage if this spawner should see other grid cells with lower or higher scale")]
        [Range(1, 6)] public int OnScalledGrid = 0;

        public GameObject TemporaryPrefabOverride { get; private set; } = null;
        internal void SetTemporaryPrefabToSpawn(GameObject p)
        {
            TemporaryPrefabOverride = p;
        }

        [Tooltip("How many times this spawner should be repeated over all grid cells")]
        [Range(1, 8)] public int Repeat = 1;
        [Tooltip("If you don't want to inherit rotations of previous object spawned in cell then you can untoggle this (warning! in many cases it's easier to setup rules with this iheritance)")]
        public bool DontInheritRotations = false;
        [Tooltip("After generating target object, switching it and it's child transforms to 'Static'")]
        public bool SwitchSpawnedToStatic = false;

        public bool InheritRotations { get { return !DontInheritRotations; } }

        internal int _currentRepeat = 0;

        public bool MultipleToSpawn = false;

        /// <summary> Since Unity is serializing as false even when writing var = true </summary>
        public bool UseGlobalRules = true;
        /// <summary> Since Unity is serializing as false even when writing var = true </summary>
        public bool UseParentPackageRules = true;


        /// <summary> Just because unity is serializing UseGlobalRules as false on old files :/ </summary>
        [SerializeField][HideInInspector] private bool _wasEnablingGlobalRules = false;
        void _EditorRefreshSpawner()
        {
            if (_wasEnablingGlobalRules) return;
            _wasEnablingGlobalRules = true;
            UseGlobalRules = true;
            UseParentPackageRules = true;
        }

        public bool HaveParentPackageRules()
        {
            _EditorRefreshSpawner();

            if (Parent != null)
                if (Parent.ParentPack != null)
                    if (Parent.ParentPack.CallOnAllSpawners != null)
                        if (Parent.ParentPack.CallOnAllSpawners.Rules != null)
                            return true;
            return false;
        }


        public enum ESR_CellHierarchyAccess
        {
            //[Tooltip("Not checking any cells")]
            //DisableSpawer = 0,
            [Tooltip("Checking only cells with same grid scale")]
            SameScale = 1,
            //[Tooltip("Checking only cells with higher grid scale")]
            //HigherScale = 2,
            //[Tooltip("Checking only cells with lower grid scale")]
            //LowerScale = 3,
            [Tooltip("Checking cells with same or higher grid scale")]
            HigherAndSame = 4,
            [Tooltip("Checking cells with same or lower grid scale")]
            LowerAndSame = 5,
            //[Tooltip("Checking all available grids with different scales")]
            //All = 6,
        }

        /// <summary> Supporting accessing scaled grids </summary>
        public ESR_CellHierarchyAccess ScaleAccess = ESR_CellHierarchyAccess.SameScale;
        public FieldSpawner(int stampPrefabID, FieldModification.EModificationMode mode, FieldModification parent)
        {
            StampPrefabID = stampPrefabID;
            Parent = parent;
            Mode = mode;
            DontInheritRotations = false;
            //Rules = new List<SpawnRuleBase>();
            UseGlobalRules = true;
            UseParentPackageRules = true;

            if (StampPrefabID == -3) { Name = "Random Emitter"; }
            else
            if (StampPrefabID == -2) { Name = "Empty"; }
            else
            if (StampPrefabID == -1) { Name = "Random"; }
            else
            {
                if (SObjectAvailable()) Name = parent.GetPrefabRef(StampPrefabID).GameObject.name; else Name = "NULL!";
            }
        }


        #region Main rules add remove and utils


        public bool SObjectAvailable()
        {
            if (Parent == null) return false;

            if (TemporaryPrefabOverride != null) return false;

            if (Mode == FieldModification.EModificationMode.ObjectsStamp)
            {
                if (Parent.OStamp == null) return false;
                if (Parent.OStamp.Prefabs == null) return false;
                if (StampPrefabID >= Parent.OStamp.Prefabs.Count) return false;
            }
            else if (Mode == FieldModification.EModificationMode.ObjectMultiEmitter)
            {
                if (Parent.OMultiStamp == null) return false;
                if (Parent.OMultiStamp.PrefabsSets == null) return false;
                if (StampPrefabID >= Parent.OMultiStamp.PrefabsSets.Count) return false;
            }

            return true;
        }


        internal void AddRule(Type ruleType)
        {
            SpawnRuleBase rule = ScriptableObject.CreateInstance(ruleType) as SpawnRuleBase;
            AddRule(rule);
        }

        internal void AddRule(SpawnRuleBase rule)
        {
            if (rule != null)
            {
                rule.SetOwner(this);
                Rules.Add(rule);
                Parent.AddAsset(rule);
                rule.hideFlags = HideFlags.HideInHierarchy;
            }
        }


        internal void RemoveRule(SpawnRuleBase rule)
        {
            rule.hideFlags = HideFlags.None;
            if (Rules.Contains(rule)) Rules.Remove(rule);
            Parent.RemoveAsset(rule);
        }


        #endregion


        public void RefreshRulesOwner()
        {
            for (int i = 0; i < Rules.Count; i++)
            {
                if (Rules[i] == null) continue;
                Rules[i].SetOwner(this);
            }
        }


        /// <summary> Custom events executed after all modificators (durin grid generating) </summary>
        [NonSerialized] public List<Action> OnPostCallEvents = new List<Action>();

        private static List<SpawnRuleBase> _RulesToCheck;

        /// <summary>
        /// Running spawner and it's rules on grid's cell
        /// </summary>
        public SpawnData RunSpawnerOnCell(FieldModification mod, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3 desiredDirection, FieldModification childMod, bool dontUseGlobalRules = false, bool ignoreRestrictions = false, bool isAsync = false)
        {
            _EditorRefreshSpawner();

            #region Checking for restrictions

            #region Debugging drawing
            //if (IGeneration.Debugging)
            //{
            //    if (restrictions != null)
            //    {
            //        int cells = 0;
            //        for (int i = 0; i < restrictions.Count; i++)
            //        {
            //            cells += restrictions[i].Cells.Count;
            //            for (int c = 0; c < restrictions[i].Cells.Count; c++)
            //            {
            //                Debug.DrawRay(IGeneration.V2ToV3(restrictions[i].Cells[c].cellPosition) * 2f, SpawnData.GetPlacementDirection(restrictions[i].Cells[c].placement), Color.yellow, 1.1f);
            //            }
            //        }
            //        UnityEngine.Debug.Log("rCount = " + restrictions.Count + " cells = " + cells);
            //    }
            //    else
            //        UnityEngine.Debug.Log("rCount = NULL"); // Guides
            //}
            #endregion


            if (!ignoreRestrictions)
            {
                if (cell.HaveInstructions())
                {
                    for (int i = 0; i < cell.GetInstructions().Count; i++)
                    {
                        var instr = cell.GetInstructions()[i];
                        if (instr.IsPreSpawn == false && instr.definition.InstructionType != InstructionDefinition.EInstruction.DoorHole) continue;

                        // No tag needed, just dont spawn anything here
                        if (instr.definition.InstructionType == InstructionDefinition.EInstruction.PreventAllSpawn)
                        {
                            return null;
                        }
                        else
                        {
                            if (instr.definition.InstructionType == InstructionDefinition.EInstruction.PreventSpawnSelective || instr.definition.InstructionType == InstructionDefinition.EInstruction.DoorHole)
                            {

                                //UnityEngine.Debug.Log("jojojo");
                                //UnityEngine.Debug.DrawRay(cell.WorldPos(), Vector3.up, Color.green, 1.01f);
                                bool isTagged;


                                // Searching for tags
                                if (string.IsNullOrEmpty(instr.definition.Tags)) isTagged = false;
                                else
                                {
                                    string[] splitted = instr.definition.Tags.Split(',');
                                    isTagged = SpawnRuleBase.HaveTags(mod.ModTag, splitted);
                                    if (!isTagged) isTagged = SpawnRuleBase.HaveTags(SpawnerTag, splitted);
                                    if (!isTagged) if (HasPackageTag()) isTagged = SpawnRuleBase.HaveTags(GetPackageTag(false), splitted);
                                }

                                // Prevent spawning selective mods or mod packs
                                if (!isTagged)
                                    if (instr.definition.InstructionType == InstructionDefinition.EInstruction.PreventSpawnSelective)
                                    {
                                        if (instr.definition.extraMod != null || instr.definition.extraPack != null)
                                        {
                                            if (instr.definition.extraMod != null)
                                                if (FGenerators.CheckIfExist_NOTNULL(SpawnRuleBase.CellSpawnsHaveModificator(cell, instr.definition.extraMod)))
                                                    isTagged = true;

                                            if (!isTagged)
                                                if (instr.definition.extraPack != null)
                                                    if (FGenerators.CheckIfExist_NOTNULL(SpawnRuleBase.CellSpawnsHaveModPack(cell, instr.definition.extraPack)))
                                                        isTagged = true;
                                        }
                                    }


                                if (isTagged)
                                {
                                    // Found cell and it have tag required for restriction to be applied
                                    return null;
                                }

                            }

                        }
                    }
                }
            }

            #endregion


            SpawnData spawnData = null;

            if (TemporaryPrefabOverride == null)
            {
                if (MultipleToSpawn)
                {
                    var selected = FEngineering.GetLayermaskValues(StampPrefabID, mod.GetPRSpawnOptionsCount());
                    if (selected.Length > 0)
                        spawnData = SpawnData.GenerateSpawn(this, childMod == null ? mod : childMod, cell, selected[FGenerators.GetRandom(0, selected.Length)], null, null, null, null, SpawnData.ESpawnMark.Omni, !isAsync);
                }
            }

            if (spawnData == null) spawnData = SpawnData.GenerateSpawn(this, childMod == null ? mod : childMod, cell, StampPrefabID, null, null, null, null, SpawnData.ESpawnMark.Omni, !isAsync);
            spawnData.OwnerMod.ParentPreset = preset;
            spawnData.ExecutedFrom = preset;

            var spawns = cell.CollectSpawns();

            // Init spawn parameters like rotation etc.
            if (InheritRotations) if (spawns.Count > 0) spawnData.RotationOffset = spawns[spawns.Count - 1].RotationOffset;

            // Preparing rule list to check logics to allow spawn-injection
            if (_RulesToCheck == null) _RulesToCheck = new List<SpawnRuleBase>();
            else _RulesToCheck.Clear();


            // Check for nulls
            PGGUtils.CheckForNulls(Rules);


            #region Preparing rules for run


            // Global rule pre
            if (UseGlobalRules)
                if (!dontUseGlobalRules)
                    for (int i = 0; i < mod._tempGlobalRulesPre.Count; i++)
                    {
                        mod._tempGlobalRulesPre[i].Refresh();
                        mod._tempGlobalRulesPre[i].CheckRuleOn(mod, ref spawnData, preset, cell, grid, desiredDirection);
                        _RulesToCheck.Add(mod._tempGlobalRulesPre[i]);
                    }


            // Main spawner rules --------------------------
            for (int i = 0; i < Rules.Count; i++)
            {
                Rules[i].SetOwner(this);

                if (Rules[i].Enabled == false || Rules[i].Ignore) continue;
                if (Rules[i].Global == true) continue;

                Rules[i].Refresh();

                ISpawnProcedureType t = Rules[i] as ISpawnProcedureType;
                if (t != null) if (t.Type == SpawnRuleBase.EProcedureType.OnConditionsMet) continue;

                Rules[i].CheckRuleOn(mod, ref spawnData, preset, cell, grid, desiredDirection);
                _RulesToCheck.Add(Rules[i]);
            }


            // Global rule post
            if (UseGlobalRules)
                for (int i = 0; i < mod._tempGlobablRulesPost.Count; i++)
                {
                    mod._tempGlobablRulesPost[i].Refresh();
                    mod._tempGlobablRulesPost[i].CheckRuleOn(mod, ref spawnData, preset, cell, grid, desiredDirection);
                    _RulesToCheck.Add(mod._tempGlobablRulesPost[i]);
                }


            // Global Parent Package Rules
            if (UseParentPackageRules)
            {
                if (HaveParentPackageRules())
                    for (int i = 0; i < Parent.ParentPack.CallOnAllSpawners.Rules.Count; i++)
                    {
                        var rl = Parent.ParentPack.CallOnAllSpawners.Rules[i];
                        if (rl == null) continue;
                        if (rl.Enabled == false) continue;
                        rl.Refresh();
                        rl.SetOwner(this);
                        rl.CheckRuleOn(mod, ref spawnData, preset, cell, grid, desiredDirection);
                        _RulesToCheck.Add(rl);
                    }
            }


            #endregion


            // Checking rules logics to allow cell spawn-inject process
            bool allRequirementsMet = false;

            if (_RulesToCheck.Count == 0) allRequirementsMet = true; // No rules then just inject-spawn
            else
                for (int i = 0; i < _RulesToCheck.Count; i++) // Check all rules logic requirements
                {
                    SpawnRuleBase rule = _RulesToCheck[i];
                    if (rule == null) continue;

                    // We must know if we can stop and spawn or we just stop without spawn or we continue checking (or set spawn and continue?)
                    Vector3? commDir = null;
                    if (desiredDirection != Vector3.zero) commDir = desiredDirection;
                    var result = rule.RunSpawnMod(preset, mod, cell, ref spawnData, grid, commDir);

                    if (result == SpawnRuleBase.EGRuleResult.StopAndSpawn)
                    {
                        allRequirementsMet = true;
                        break;
                    }
                    else if (result == SpawnRuleBase.EGRuleResult.Continue) // Mostly used
                    {
                        if (i == _RulesToCheck.Count - 1) allRequirementsMet = true; // Last continue check then allow (all requirements met)
                        continue;
                    }
                    else if (result == SpawnRuleBase.EGRuleResult.Stop)
                    {
                        allRequirementsMet = false;
                        break;
                    }
                    else if (result == SpawnRuleBase.EGRuleResult.SetSpawnAndContinue)
                    {
                        allRequirementsMet = true;
                        continue;
                    }
                    else if (result == SpawnRuleBase.EGRuleResult.HoldSpawning)
                    {
                        allRequirementsMet = false;
                        break;
                    }
                    else
                    {
                        allRequirementsMet = true;
                    }
                }

            if (allRequirementsMet) // Success -> Inject spawn data onto grid cell
            {
                // Nodes with actions
                if (FGenerators.CheckIfExist_NOTNULL(spawnData))
                    for (int i = 0; i < Rules.Count; i++)
                    {
                        if (Rules[i].Enabled == false || Rules[i].Ignore) continue;
                        if (Rules[i].Global == true) continue;
                        ISpawnProcedureType t = Rules[i] as ISpawnProcedureType;

                        if (t != null)
                            if (t.Type == SpawnRuleBase.EProcedureType.OnConditionsMet || t.Type == SpawnRuleBase.EProcedureType.Coded)
                                Rules[i].OnConditionsMetAction(mod, ref spawnData, preset, cell, grid);
                    }

                if (FGenerators.CheckIfExist_NOTNULL(spawnData))
                {
                    //UnityEngine.Debug.Log("mod._tempGlobablRulesOnConditions " + mod._tempGlobablRulesOnConditions.Count + "   :   " + mod._tempGlobablRulesPost.Count + "  :   " + mod._tempGlobalRulesPre.Count);
                    // Global rules on conditions met
                    for (int i = 0; i < mod._tempGlobablRulesOnConditions.Count; i++)
                    {
                        mod._tempGlobablRulesOnConditions[i].Refresh();
                        mod._tempGlobablRulesOnConditions[i].OnConditionsMetAction(mod, ref spawnData, preset, cell, grid);
                    }

                    // Mods Pack Call On All
                    if (UseParentPackageRules)
                    {
                        if (HaveParentPackageRules())
                            for (int i = 0; i < Parent.ParentPack.CallOnAllSpawners.Rules.Count; i++)
                            {
                                var rl = Parent.ParentPack.CallOnAllSpawners.Rules[i];
                                if (rl.Enabled == false) continue;

                                ISpawnProcedureType t = rl as ISpawnProcedureType;

                                if (t != null)
                                    if (t.Type == SpawnRuleBase.EProcedureType.OnConditionsMet || t.Type == SpawnRuleBase.EProcedureType.Coded)
                                        rl.OnConditionsMetAction(mod, ref spawnData, preset, cell, grid);
                            }
                    }


                    // Sending signal to rules about successfull logic operation
                    for (int i = 0; i < Rules.Count; i++)
                    {
                        if (Rules[i].Enabled == false || Rules[i].Ignore) continue;
                        Rules[i].OnAddSpawnUsingRule(mod, spawnData, cell, grid);
                    }


                    // Sending to global rules
                    if (!dontUseGlobalRules)
                    {
                        if (UseGlobalRules)
                        {
                            for (int i = 0; i < mod._tempGlobalRulesPre.Count; i++) mod._tempGlobalRulesPre[i].OnAddSpawnUsingRule(mod, spawnData, cell, grid);
                            for (int i = 0; i < mod._tempGlobablRulesPost.Count; i++) mod._tempGlobablRulesPost[i].OnAddSpawnUsingRule(mod, spawnData, cell, grid);
                            for (int i = 0; i < mod._tempGlobablRulesOnConditions.Count; i++) mod._tempGlobablRulesOnConditions[i].OnAddSpawnUsingRule(mod, spawnData, cell, grid);
                        }
                    }


                    // Mods Pack Call On All
                    if (UseParentPackageRules)
                    {
                        if (HaveParentPackageRules())
                            for (int i = 0; i < Parent.ParentPack.CallOnAllSpawners.Rules.Count; i++)
                            {
                                if (Parent.ParentPack.CallOnAllSpawners.Rules[i] == null) continue;
                                if (Parent.ParentPack.CallOnAllSpawners.Rules[i].Enabled == false) continue;
                                Parent.ParentPack.CallOnAllSpawners.Rules[i].OnAddSpawnUsingRule(mod, spawnData, cell, grid);
                            }
                    }


                    #region Old Debug
                    //if (OnConditionsMet == ESR_Action.AddSpawnIfFree) if (cell.Spawns.Count > 0) return null;
                    //if (OnConditionsMet == ESR_Action.Destroy)
                    //{
                    //    cell.Spawns.Clear();
                    //    return null;
                    //}

                    //if (OnConditionsMet == ESR_Action.ReplaceSpawn) cell.Spawns.Clear();
                    #endregion

                    if (spawnData.Enabled) cell.AddSpawnToCell(spawnData);
                }
            }


            #region Checking post spawn instructions

            if (cell.HaveInstructions())
            {
                for (int i = 0; i < cell.GetInstructions().Count; i++)
                {
                    var instr = cell.GetInstructions()[i];
                    if (instr.IsPostSpawn == false)
                    {
                        continue;
                    }

                    bool isTagged = false;

                    // Searching for tags
                    if (string.IsNullOrEmpty(instr.definition.Tags)) isTagged = true;
                    else
                    {
                        string[] splitted = instr.definition.Tags.Split(',');
                        isTagged = SpawnRuleBase.HaveTags(mod.ModTag, splitted);
                        if (!isTagged) isTagged = SpawnRuleBase.HaveTags(SpawnerTag, splitted);
                        if (!isTagged) if (HasPackageTag()) isTagged = SpawnRuleBase.HaveTags(GetPackageTag(false), splitted);
                    }

                    if (isTagged)
                    {
                        if (instr.definition.InstructionType == InstructionDefinition.EInstruction.InjectStigma)
                        {
                            spawnData.AddCustomStigma(instr.definition.InstructionArgument);
                        }
                    }

                }


            }

            #endregion


            if (allRequirementsMet == false) spawnData = null;

            return spawnData;
        }


        public void OnAfterModificatorsCall()
        {
            if (OnPostCallEvents != null)
                for (int i = 0; i < OnPostCallEvents.Count; i++)
                {
                    if (OnPostCallEvents[i] == null) continue;
                    OnPostCallEvents[i].Invoke();
                }
        }

        internal bool IsExecutionReversed()
        {
            return (CellCheckMode == ESR_CellOrder.Reversed || CellCheckMode == ESR_CellOrder.RandomReversed || CellCheckMode == ESR_CellOrder.Random2Reversed);
        }

        public bool HasPackageTag()
        {
            if (Parent != null)
                if (Parent.ParentPack != null)
                    if (!string.IsNullOrEmpty(Parent.ParentPack.TagForAllSpawners))
                        return true;

            return false;
        }

        public string GetPackageTag(bool checkIfNoRef = true)
        {
            if (checkIfNoRef) if (HasPackageTag() == false) return "";
            return Parent.ParentPack.TagForAllSpawners;
        }

        public FieldSpawner Copy()
        {
            FieldSpawner copy = (FieldSpawner)this.MemberwiseClone();
            copy.Name = Name + "-Copy";
            copy.SpawnerTag = SpawnerTag;
            copy.StampPrefabID = StampPrefabID;
            copy.Parent = Parent;
            copy.Mode = Mode;
            //copy.OnConditionsMet = OnConditionsMet;
            copy.CellCheckMode = CellCheckMode;
            copy.OnScalledGrid = OnScalledGrid;
            copy.UseGlobalRules = UseGlobalRules;
            copy.UseParentPackageRules = UseParentPackageRules;

            copy.Rules = new List<SpawnRuleBase>();

            for (int i = 0; i < Rules.Count; i++)
            {
                SpawnRuleBase copied = Rules[i].CustomCopy(this);

                if (copied == null)
                {
                    if (Rules[i]._CopyPasteSupported() == false) continue;
                    if (Rules[i].AllowDuplicate() == false) continue;
                }

                if (copied == null) copied = ScriptableObject.Instantiate(Rules[i]);
                copy.Rules.Add(copied);
                copied.OwnerSpawner = copy;
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.AddObjectToAsset(copied, copy.Parent);
#endif
            }

            return copy;
        }

        public bool wasPreGeneratingPrepared { get; private set; } = false;
        public void PreGenerating()
        {
            wasPreGeneratingPrepared = true;
            TemporaryPrefabOverride = null;

            if (OnPostCallEvents == null) OnPostCallEvents = new List<Action>();
            else OnPostCallEvents.Clear();

            // Check for sub-spawners pre-generating
            //if (Rules == null) return;
            //for (int r = 0; r < Rules.Count; r++)
            //{
            //    var rl = Rules[r];
            //    if (rl == null) continue;
            //    if (rl.GetType() == typeof(Rules.QuickSolutions.SR_CallSubSpawner))
            //    {
            //        Rules.QuickSolutions.SR_CallSubSpawner subSp = rl as Rules.QuickSolutions.SR_CallSubSpawner;
            //        if ( subSp.toCall.PreGenerating();
            //    }
            //}
        }

        public void AfterGeneratingCall()
        {
            wasPreGeneratingPrepared = false;

            //if (Rules == null) return;
            //for (int r = 0; r < Rules.Count; r++)
            //{
            //    var rl = Rules[r];
            //    if (rl == null) continue;
            //    Rules[r].OnAfterAllGeneratedCall();
            //}
        }


        /// <summary>
        /// Returns null if there is no extra spawns!
        /// Extra spawns are generated by Wall Placer node or Get Coordinates when using 'Spawn on each side' feature
        /// It is generating additional spawns ignoring other rules check
        /// </summary>
        public List<SpawnData> GetExtraSpawns()
        {
            List<SpawnData> tempSpawns = null;
            if (Rules == null) return null;

            for (int i = 0; i < Rules.Count; i++)
            {
                var rule = Rules[i];

                if (rule == null) continue;
                if (rule.Enabled == false) continue;
                var tempSpawnsList = rule.GetTempSpawns;

                if (tempSpawnsList == null) continue;
                if (tempSpawnsList.Count < 2) continue;

                if (tempSpawns == null) tempSpawns = new List<SpawnData>();
                for (int t = 1; t < tempSpawnsList.Count; t++) tempSpawns.Add(tempSpawnsList[t]);
            }

            return tempSpawns;
        }

    }
}