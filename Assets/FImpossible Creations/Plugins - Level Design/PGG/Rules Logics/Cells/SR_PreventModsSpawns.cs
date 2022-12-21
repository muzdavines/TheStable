#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using FIMSpace.Generating.Rules.Helpers;

namespace FIMSpace.Generating.Rules.Cells
{
    public class SR_PreventModsSpawns : SpawnRuleBase, ISpawnProcedureType
    {
        public EProcedureType Type { get { return EProcedureType.OnConditionsMet; } }
        public override string TitleName() { return "Prevent Mods Spawns"; }
        public override string Tooltip() { return "Prevent spawning for other selected modificators or mod packs inside this cell"; }

        public FieldModification PreventSpawnMod;
        public ModificatorsPack PreventSpawnPack;
        [HideInInspector] public CheckCellsSelectorSetup checkSetup = new CheckCellsSelectorSetup(true, false);

        #region Editor stuff
#if UNITY_EDITOR

        public override void NodeHeader()
        {
            base.NodeHeader();
            checkSetup.UseCondition = false;
            DrawMultiCellSelector(checkSetup, OwnerSpawner);
        }
#endif
        #endregion


        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            CellSelector_Execute(checkSetup, grid, cell, cell, thisSpawn, (FieldCell c, SpawnData s) => PreventOnCell(c));
        }

        public void PreventOnCell(FieldCell cell)
        {
            SpawnInstruction preventSpawn;
            preventSpawn = new SpawnInstruction();
            preventSpawn.definition = new InstructionDefinition();

            if (PreventSpawnMod != null || PreventSpawnPack != null)
            {
                preventSpawn = new SpawnInstruction();
                preventSpawn.definition = new InstructionDefinition();
                preventSpawn.definition.InstructionType = InstructionDefinition.EInstruction.PreventSpawnSelective;

                preventSpawn.definition.extraMod = PreventSpawnMod;
                preventSpawn.definition.extraPack = PreventSpawnPack;

                cell.AddCellInstruction(preventSpawn);
            }
        }

    }
}