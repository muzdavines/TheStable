using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Cells
{
    public class SR_AddCellDataString : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Add Cell Data String"; }
        public override string Tooltip() { return "Injecting cell data for current grid cell if all other nodes conditions are met"; }

        public string CellDataString = "";

        public EProcedureType Type { get { return EProcedureType.OnConditionsMet; } }

        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            cell.AddCustomData(CellDataString);
        }

    }
}