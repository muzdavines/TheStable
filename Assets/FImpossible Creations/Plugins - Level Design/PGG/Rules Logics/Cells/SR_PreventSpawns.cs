using UnityEngine;

namespace FIMSpace.Generating.Rules.Cells
{
    public class SR_PreventSpawns : SpawnRuleBase, ISpawnProcedureType
    {
        public EProcedureType Type { get { return EProcedureType.OnConditionsMet; } }
        public override string TitleName() { return "Prevent Spawns"; }
        public override string Tooltip() { return "Prevent spawning for other next spawners with selected tags"; }

        [Tooltip("When tag is left empty then preventing spawning any other object inside this cell!")]
        public string PreventSpawningForTagged = "";

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
            CellSelector_Execute(checkSetup, grid, cell, cell, thisSpawn, (FieldCell c, SpawnData s) => AddPrevent(c) );
        }

        public void AddPrevent(FieldCell cell)
        {
            cell.AddCellInstruction(GeneratePreventSpawns(PreventSpawningForTagged));
        }
    }
}