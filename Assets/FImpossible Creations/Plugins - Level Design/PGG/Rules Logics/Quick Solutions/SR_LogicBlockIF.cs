#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Rules.QuickSolutions
{
    public class SR_LogicBlockIF : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Logic If Block"; }
        public override string Tooltip() { return "You can put additional conditions in this block and trigger events if conditions are met\n" + base.Tooltip(); }
        public EProcedureType Type { get { return EProcedureType.Coded; } }
        public override bool CanBeGlobal() { return false; }
        public override bool CanBeNegated() { return false; }
        internal override bool AllowDuplicate() { return false; }

        public bool Test = false;
        [SerializeField] [HideInInspector] private FieldSpawner spawner;

        /// <summary> Internal cell allow is used with 'JustTriggerEvent' mode, it let's/not let to trigger event but not breaking spawning </summary>
        bool internalCellAllow = false;
        public enum EResult
        { JustTriggerEvent, AllowOrBreakSpawning }
        [Space(3)]
        [Tooltip("If you want use this logic block to trigger some event or stop spawning parent spawner")]
        public EResult BlockResult = EResult.JustTriggerEvent;

        void RefreshCustomSpawner()
        {
            if (OwnerSpawner != null)
            {
                if (spawner == null) spawner = new FieldSpawner(0, FieldModification.EModificationMode.CustomPrefabs, OwnerSpawner.Parent);
                if (spawner.Rules == null) spawner.Rules = new List<SpawnRuleBase>();
                spawner.Parent = OwnerSpawner.Parent;
                spawner.Enabled = true;

                for (int i = 0; i < spawner.Rules.Count; i++)
                {
                    if (spawner.Rules[i] == null) continue;
                    spawner.Rules[i].DisableDrawingGlobalSwitch = true;

                    ISpawnProcedureType rType = spawner.Rules[i] as ISpawnProcedureType;
                    if (rType == null) continue;

                    if (rType.Type == EProcedureType.Rule)
                        spawner.Rules[i].DrawLogicSwitch = true;
                    else
                        spawner.Rules[i].DrawLogicSwitch = false;

                }
            }
        }


        #region Refreshing rules

        public override void Refresh()
        {
            base.Refresh();
            RefreshCustomSpawner();
            RefreshSpawnerRules();
        }

        public override void ResetRule(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset)
        {
            base.ResetRule(grid, preset);

            for (int i = 0; i < spawner.Rules.Count; i++)
            {
                var rule = spawner.Rules[i];
                if (FGenerators.CheckIfExist_NOTNULL(rule)) rule.ResetRule(grid, preset);
            }
        }

        void RefreshSpawnerRules()
        {
            for (int i = 0; i < spawner.Rules.Count; i++)
            {
                var rule = spawner.Rules[i];
                if (FGenerators.CheckIfExist_NOTNULL(rule)) rule.Refresh();
            }
        }

        #endregion


        #region Drawing Inspector Window

#if UNITY_EDITOR

        public override void NodeBody(SerializedObject so)
        {
            RefreshCustomSpawner();
            EditorGUILayout.HelpBox("Node executes changes if all conditions met", MessageType.None);

            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0f, 1f, 0f, 0.75f);
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyleH);
            GUI.backgroundColor = bc;

            if (GUIIgnore.Count != 1) GUIIgnore.Add("Test");
            base.NodeBody(so);

            EditorGUILayout.HelpBox("WARNING: Work In Progress Node, use it wisely.", MessageType.None);

            if (spawner != null)
            {
                if (spawner.Rules == null || spawner.Rules.Count == 0)
                {
                    EditorGUILayout.HelpBox("Add rules to IF BLOCK to draw them", MessageType.Info);
                }
                else
                {
                    spawner.DrawSpawnerGUIBody();
                }
            }

            FGUI_Inspector.DrawUILine(0.3f, 0.5f, 1, 6);

            GUI.backgroundColor = new Color(.5f, 1f, .5f, 0.9f);
            spawner.DrawInspector();
            GUI.backgroundColor = bc;
        }

        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            GUILayout.Space(3);
            EditorGUILayout.EndVertical();
            base.NodeFooter(so, mod);
        }
#endif

        #endregion


        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            CellAllow = true; // No spawn prevent by default - for JustTriggerEvent
            internalCellAllow = false;
            bool atLeastOne = false;
            bool broken = false;

            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);

            for (int i = 0; i < spawner.Rules.Count; i++)
            {
                var rule = spawner.Rules[i];
                if (rule.Enabled == false) continue;

                // Internal check custom rule if it's conditions are met
                rule.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);

                ISpawnProcedureType rType = spawner.Rules[i] as ISpawnProcedureType;
                ERuleLogic currentLogic = ERuleLogic.AND_Required;

                bool checkRuleWith = true;

                if (rType != null) // Identify defined role of rule
                {
                    if (rType.Type == EProcedureType.Rule) currentLogic = rule.Logic;
                    else if (rType.Type == EProcedureType.Event || rType.Type == EProcedureType.OnConditionsMet) checkRuleWith = false;
                }

                if (checkRuleWith == false) continue; // Some of rules like 'Event' type should not influence allow/disallow logics

                if (currentLogic == ERuleLogic.AND_Required)
                {
                    if (rule.CellAllow == false) // Required but conditions not met then disallow any further events
                    {
                        broken = true;
                        break;
                    }
                    else // Required and conditions met, go further with rules check then
                    {
                        atLeastOne = true;
                    }
                }
                else if (currentLogic == ERuleLogic.OR)
                {
                    if (rule.CellAllow == false)
                    {
                        // If 'OR' rule's conditions not met then don't do anything
                    }
                    else
                    {
                        // If 'OR' rule's conditions are met then tell that something can work out in this cell
                        atLeastOne = true;
                    }
                }

            }

            if (!broken) if (atLeastOne) internalCellAllow = true;

            if (BlockResult == EResult.AllowOrBreakSpawning)
            {
                CellAllow = internalCellAllow;
            }
        }


        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            if (internalCellAllow == false) { return; }

            for (int i = 0; i < spawner.Rules.Count; i++)
            {
                var rule = spawner.Rules[i];
                if (FGenerators.CheckIfIsNull(rule)) continue;
                if (rule.Enabled == false) continue;

                rule.CellInfluence(preset, mod, cell, ref thisSpawn, grid);
                rule.OnConditionsMetAction(mod, ref thisSpawn, preset, cell, grid);
            }

            base.OnConditionsMetAction(mod, ref thisSpawn, preset, cell, grid);
        }


    }
}