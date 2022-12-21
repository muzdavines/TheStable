using UnityEngine;

namespace FIMSpace.Generating.Rules.Other
{
    public class SR_DebugLog : SpawnRuleBase, ISpawnProcedureType
    {
        public EProcedureType Type { get { return EProcedureType.OnConditionsMet; } }

        public override string TitleName() { return "Debug Log"; }
        public override string Tooltip() { return "Just doing Debug.Log when spawner's conditions are met"; }

        public string ToLog = "Example Log";
        public bool LogCellPosition = true;

        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            string fullLog = ToLog;

            if (LogCellPosition) fullLog += "   Cell: " + cell.Pos;

            Debug.Log(fullLog);
        }
    }
}