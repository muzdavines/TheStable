using System;
using System.Collections.Generic;
using UnityEngine;
using FIMSpace.Generating.Rules;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating
{
    /// <summary>
    /// It's always sub-asset -> it's never project file asset
    /// </summary>
    public abstract partial class SpawnRuleBase : ScriptableObject
    {
        public virtual string TitleName() { return GetType().Name; }
        public virtual string Tooltip() { string tooltipHelp = "(" + GetType().Name; ISpawnProcedureType prc = this as ISpawnProcedureType; if (prc != null) tooltipHelp += " : " + prc.Type.ToString().ToUpper(); return tooltipHelp + ")"; }
        public virtual bool CanBeGlobal() { return true; }
        public virtual bool CanBeNegated() { return true; }
        public virtual bool EditorIsLoading() { return false; }
        internal virtual bool AllowDuplicate() { return true; }
        /// <summary> Displaying AND / OR switch logics box </summary>
        internal bool DrawLogicSwitch = false;


        public enum EProcedureType
        {
            [Tooltip("Conditions + changing spawn properties")]
            Procedure,
            [Tooltip("Conditions for spawning (CheckRuleOn)")]
            Rule,
            [Tooltip("Just changing spawns properties (Cell Influence)")]
            Event,
            [Tooltip("Will be executed at the end only when all conditions are met (+OnConditionsMetAction)")]
            OnConditionsMet,
            [Tooltip("Running CheckRule and OnConditionsMet to give wide variety access")]
            Coded,
            [Tooltip("Outputting value (not used yet)")]
            Output
        }

        public enum ERuleLogic
        {
            [Tooltip("Pass rules forward only if this rule is completed")]
            AND_Required,
            [Tooltip("Pass rules forward even rule is not completed")]
            OR,
            //[Tooltip("Stop passing rules forward if this rule is completed or not completed")]
            //BREAK_Stop,
            //[Tooltip("Stop passing rules forward if this rule is completed and not allowing to execute spawn")]
            //HardStopSpawning
        }

        [Tooltip("Basic if 'and' 'or' logic frames")]
        [HideInInspector] public ERuleLogic Logic = ERuleLogic.AND_Required;
        [Tooltip("Inversing rule requirements")]
        [HideInInspector] public bool Negate = false;
        [HideInInspector] public bool Global = false;
        [NonSerialized] public bool DisableDrawingGlobalSwitch = false;
        public enum EGRuleResult { Undefined = -1, StopAndSpawn, Continue, Stop, SetSpawnAndContinue, HoldSpawning }
        internal Vector2 _editor_scroll = Vector2.zero;
        [HideInInspector] public bool Enabled = true;
        [HideInInspector] public bool Ignore = false;
        [HideInInspector] public bool _editor_drawRule = true;
        [HideInInspector, NonSerialized] public FieldSpawner OwnerSpawner;

        /// <summary> (dynamic - every cell check) Flag to inform that current checked cell by algorithm requirements are met </summary>
        [HideInInspector] public bool CellAllow = false;
        /// <summary> Useful for min-max rules to run rule again if for example min spawn count requirement not met </summary>
        [HideInInspector] public bool AllConditionsMet = true;

        [NonSerialized] public bool _EditorDebug = false;
        [NonSerialized] protected Color _DbPreCol;
        [NonSerialized] public List<string> GUIIgnore = new List<string>();


        /// <summary> Executed every time when checked on some cell </summary>
        public virtual void Refresh()
        {
            CellAllow = false;

            if (this is ISpawnProcedureType)
            {
                ISpawnProcedureType t = this as ISpawnProcedureType;
                if (t.Type == EProcedureType.Event || t.Type == EProcedureType.OnConditionsMet)
                {
                    //Logic = ERuleLogic.OR;
                    CellAllow = true;
                }
            }
        }


        /// <summary> [Base is empty] Executed every time when starting sequence of checking all cells with rules </summary>
        public virtual void ResetRule(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset) { }
        /// <summary> [Base is empty] Executed only first time when starting sequence of checking all cells with rules </summary>
        public virtual void PreGenerateResetRule(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset, FieldSpawner callFrom) { }

        /// <summary> [Base is empty] </summary>
        public virtual void OnAddSpawnUsingRule(FieldModification mod, SpawnData spawn, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        { }

        /// <summary> Executed on every cell until conditions are met </summary>
        public virtual void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (tempSpawns != null) tempSpawns.Clear();
        }

        /// <summary> [Base is empty] Executed only once when occupied cell is known after running all rule checks </summary>
        public virtual void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        { }

        /// <summary> [Base is empty] Executed at the end of rules if all conditions met, only when implements ISpawnProcedureType with EProcedureType.OnConditionsMet type or Coded type </summary>
        public virtual void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        { }

        /// <summary> [Base is empty] Executed when parent modificator's generating compleates </summary>
        public virtual void OnGeneratingCompleated(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset) 
        { }

        ///// <summary> [Base is empty] Called like Post Event, after all objects generation by FieldSetup </summary>
        //public virtual void OnAfterAllGeneratedCall()
        //{ }


        /// <summary>
        /// If implementing FieldSetup variable acess this method must be overrided and return used variables
        /// </summary>
        public virtual List<SpawnerVariableHelper> GetVariables() { return null; }
        /// <summary> Can be overrided to define field variables required types </summary>
        public virtual void GUIRefreshVariables() {  }

        public FieldSetup TryGetParentFieldSetup()
        {
            if (OwnerSpawner != null)
                if (OwnerSpawner.Parent != null)
                {
                    if (OwnerSpawner.Parent.ParentPack != null) if (OwnerSpawner.Parent.ParentPack.ParentPreset != null) return OwnerSpawner.Parent.ParentPack.ParentPreset;
                    if (OwnerSpawner.Parent.ParentPreset != null) return OwnerSpawner.Parent.ParentPreset;
                }

            return null;
        }

        public ModificatorsPack TryGetParentModPack()
        {
            if (OwnerSpawner != null)
                if (OwnerSpawner.Parent != null)
                    if (OwnerSpawner.Parent.ParentPack != null) return OwnerSpawner.Parent.ParentPack;

            return null;
        }


        /// <summary>
        /// Running rule algorithm on single cell
        /// </summary>
        public EGRuleResult RunSpawnMod(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            bool complete = CellAllow;
            if (Negate) complete = !complete;

            if (complete)
            {
                CellInfluence(preset, mod, cell, ref spawn, grid, restrictDirection);

                //if (Logic == ERuleLogic.BREAK_Stop)
                //{
                //    return EGRuleResult.StopAndSpawn;
                //}
                //else
                //if (Logic == ERuleLogic.HardStopSpawning)
                //{
                //    return EGRuleResult.HoldSpawning;
                //}
                //else
                if (Logic == ERuleLogic.OR)
                {
                    return EGRuleResult.Continue;
                }
                else
                if (Logic == ERuleLogic.AND_Required)
                {
                    return EGRuleResult.Continue;
                }

            }
            else
            {

                if (Logic == ERuleLogic.OR)
                {
                    return EGRuleResult.Continue;
                }
                //else if (Logic == ERuleLogic.HardStopSpawning)
                //{
                //    return EGRuleResult.Continue;
                //}
                else if (Logic == ERuleLogic.AND_Required)
                {
                    return EGRuleResult.Stop;
                }
                //else if (Logic == ERuleLogic.BREAK_Stop)
                //{
                //    return EGRuleResult.Continue;
                //}

            }

            if (complete) return EGRuleResult.StopAndSpawn; else return EGRuleResult.Stop;
        }


        internal void SetOwner(FieldSpawner spawner)
        {
            OwnerSpawner = spawner;
        }


        /// <summary> Useful for custom coded rules</summary>
        protected List<SpawnData> tempSpawns;
        public List<SpawnData> GetTempSpawns { get { return tempSpawns; } }

        [NonSerialized] public bool VariablesPrepared = false;

        /// <summary> Useful for custom coded rules</summary>
        protected void AddTempData(SpawnData data, SpawnData parent)
        {
            if (tempSpawns == null) tempSpawns = new List<SpawnData>();
            data.isTemp = true;
            tempSpawns.Add(data);
            if (FGenerators.CheckIfExist_NOTNULL(parent)) parent.AddChildSpawn(data);
        }

        internal virtual bool _CopyPasteSupported() { return true; }
        /// <summary> Override _CopyPasteSupported to modify copy-paste feature </summary>
        internal virtual void PasteOtherProperties(SpawnRuleBase spawnRuleBase, bool force = false)
        {
#if UNITY_EDITOR
            SerializedObject otherRule = new SerializedObject(spawnRuleBase);
            SerializedProperty otherRlIter = otherRule.GetIterator();
            SerializedObject thisRule = new SerializedObject(this);

            if (!force)
            {
                if (otherRlIter.NextVisible(true))
                {
                    while (otherRlIter.NextVisible(true))
                    {
                        SerializedProperty prop_element = thisRule.FindProperty(otherRlIter.name);
                        if (prop_element != null && prop_element.propertyType == otherRlIter.propertyType)
                        {
                            if (prop_element.propertyType == SerializedPropertyType.ObjectReference)
                                if (prop_element.objectReferenceValue != null)
                                {
                                    if (prop_element.objectReferenceValue.GetType() == typeof(FieldSetup)) continue;
                                    if (prop_element.objectReferenceValue.GetType() == typeof(FieldModification)) continue;
                                    if (prop_element.objectReferenceValue.GetType() == typeof(ModificatorsPack)) continue;
                                    if (prop_element.objectReferenceValue.GetType() == typeof(FieldSpawner)) continue;
                                    if (prop_element.objectReferenceValue.GetType() == typeof(List<SpawnData>)) continue;
                                    if (prop_element.objectReferenceValue.GetType() == typeof(List<string>)) continue;
                                }

                            thisRule.CopyFromSerializedProperty(otherRlIter);
                        }
                    }
                }
            }
            else
            {
                if (otherRlIter.Next(true))
                {
                    while (otherRlIter.Next(true))
                    {
                        SerializedProperty prop_element = thisRule.FindProperty(otherRlIter.name);

                        if (prop_element != null && prop_element.propertyType == otherRlIter.propertyType)
                        {
                            if (prop_element.propertyType == SerializedPropertyType.ObjectReference)
                                if (prop_element.objectReferenceValue != null)
                                {
                                    if (prop_element.objectReferenceValue.GetType() == typeof(SpawnRuleBase)) continue;
                                    if (prop_element.objectReferenceValue.GetType() == typeof(FieldSetup)) continue;
                                    if (prop_element.objectReferenceValue.GetType() == typeof(FieldModification)) continue;
                                    if (prop_element.objectReferenceValue.GetType() == typeof(ModificatorsPack)) continue;
                                    if (prop_element.objectReferenceValue.GetType() == typeof(FieldSpawner)) continue;
                                    if (prop_element.objectReferenceValue.GetType() == typeof(List<SpawnData>)) continue;
                                    if (prop_element.objectReferenceValue.GetType() == typeof(List<string>)) continue;
                                }

                            if (prop_element.propertyType == SerializedPropertyType.Float)
                                prop_element.floatValue = otherRlIter.floatValue;
                            else if (prop_element.propertyType == SerializedPropertyType.Integer)
                                prop_element.intValue = otherRlIter.intValue;
                            else if (prop_element.propertyType == SerializedPropertyType.Boolean)
                                prop_element.boolValue = otherRlIter.boolValue;
                            else if (prop_element.propertyType == SerializedPropertyType.Enum)
                                prop_element.enumValueIndex = otherRlIter.enumValueIndex;
                            else if (prop_element.propertyType == SerializedPropertyType.String)
                                prop_element.stringValue = otherRlIter.stringValue;
                            else if (prop_element.propertyType == SerializedPropertyType.Color)
                                prop_element.colorValue = otherRlIter.colorValue;
                            else if (prop_element.propertyType == SerializedPropertyType.Quaternion)
                                prop_element.quaternionValue = otherRlIter.quaternionValue;
                            else if (prop_element.propertyType == SerializedPropertyType.Vector2)
                                prop_element.vector2Value = otherRlIter.vector2Value;
                            else if (prop_element.propertyType == SerializedPropertyType.Vector3)
                                prop_element.vector3Value = otherRlIter.vector3Value;
                            else if (prop_element.propertyType == SerializedPropertyType.Vector2Int)
                                prop_element.vector2IntValue = otherRlIter.vector2IntValue;
                            else if (prop_element.propertyType == SerializedPropertyType.Vector3Int)
                                prop_element.vector3IntValue = otherRlIter.vector3IntValue;
                            //else
                            //    thisRule.CopyFromSerializedPropertyIfDifferent(otherRlIter.Copy());
                        }
                    }
                }
            }

            thisRule.ApplyModifiedProperties();
#endif
        }

    }

}