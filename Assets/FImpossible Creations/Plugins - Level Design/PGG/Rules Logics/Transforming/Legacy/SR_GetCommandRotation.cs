#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Transforming.Legacy
{
    public class SR_GetCommandRotation : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Get Command Rotation"; }
        public override string Tooltip() { return "Getting rotation out of command placed in cell"; }
        public EProcedureType Type { get { return EProcedureType.Event; } }

        [Tooltip("Leave empty to get rotation from first found command with rotation enabled")]
        public string OnlyFromSpecificCellData = "";
        [Tooltip("Just for quick tweak rotation angle if needed")]
        public Vector3 AdditionalAngleOffset = Vector3.zero;

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (cell.GuidesIn == null) return; // No Commands
            if (cell.GuidesIn.Count == 0) return;

            SpawnInstruction targetCommand = cell.GuidesIn[0];

            if (string.IsNullOrEmpty(OnlyFromSpecificCellData) == false)
            {
                bool found = false;
                for (int i = 0; i < cell.GuidesIn.Count; i++)
                {
                    InstructionDefinition command = cell.GuidesIn[i].definition;
                    if (FGenerators.CheckIfIsNull(command))
                        if (cell.GuidesIn[i].HelperID < preset.CellsCommands.Count) command = preset.CellsCommands[cell.GuidesIn[i].HelperID];

                    if (FGenerators.CheckIfIsNull(command)) continue;

                    if ( command.InstructionType == InstructionDefinition.EInstruction.InjectDataString || command.InstructionType == InstructionDefinition.EInstruction.DoorHole)
                    {
                        if ( command.InstructionArgument == OnlyFromSpecificCellData)
                        {
                            targetCommand = cell.GuidesIn[i];
                            found = true;
                            break;
                        }
                    }

                }

                if (!found)
                    return;
            }


            Vector3 angles = AdditionalAngleOffset;
            angles += Quaternion.LookRotation(targetCommand.desiredDirection).eulerAngles;
            spawn.RotationOffset = angles;

        }
    }
}