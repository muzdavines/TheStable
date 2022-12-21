#if UNITY_EDITOR
using UnityEditor;
#endif

using FIMSpace.Generating.Rules.Helpers;
using UnityEngine;

namespace FIMSpace.Generating.Rules.Collision.Legacy
{
    public class SR_DistanceInCell : SpawnRuleBase, ISpawnProcedureType
    {
        public EProcedureType Type { get { return EProcedureType.Rule; } }
        public override string TitleName() { return "Distance To Inside Cell"; }
        public override string Tooltip() { return "Checking distance to other spawns in current checked cell"; }

        [Space(3)] public ESR_DistanceRule DistanceMustBe = ESR_DistanceRule.Lower;
        public float CheckDistance = 0.1f;

        [Space(4)]

        [PGG_SingleLineSwitch("CheckMode", 68, "Select if you want to use Tags, SpawnStigma or CellData", 100)]
        public string AffectedTags = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        public ESR_DirectionMode CheckOffset = ESR_DirectionMode.NoOffset;
        public Vector3Int OffsetCellPosition = Vector3Int.zero;

        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            var targetCell = cell;

            if (CheckOffset != ESR_DirectionMode.NoOffset) if (OffsetCellPosition != Vector3Int.zero)
                {
                    Vector3Int off = OffsetCellPosition;
                    if (CheckOffset == ESR_DirectionMode.CellRotateDirection) off = SpawnRuleBase.GetOffset(spawn.GetRotationOffset(), off);
                    targetCell = grid.GetCell(cell.Pos + off, false);
                }

            if (FGenerators.CheckIfIsNull(targetCell)) return ;

            var spawns = targetCell.CollectSpawns(OwnerSpawner.ScaleAccess);

            Vector3 a = spawn.Offset + spawn.GetRotationOffset() * spawn.DirectionalOffset + spawn.TempPositionOffset;

            for (int s = 0; s < spawns.Count; s++)
            {
                if (spawns[s].OwnerMod == null) continue;
                if (spawns[s] == spawn) continue;

                if (string.IsNullOrEmpty(AffectedTags) == false) // If spawns must have ceratain tags
                {
                    if (SpawnRuleBase.SpawnHaveSpecifics(spawns[s], AffectedTags, CheckMode) == false) // Not found required tags then skip this spawn
                        continue;
                }

                Vector3 b = spawns[s].Offset + spawns[s].GetRotationOffset() * spawns[s].DirectionalOffset + spawns[s].TempPositionOffset;
                float distance = Vector3.Distance(a, b);

                if (DistanceMustBe == ESR_DistanceRule.Equal)
                {
                    if (distance != CheckDistance)
                        continue;
                }
                else if (DistanceMustBe == ESR_DistanceRule.Greater)
                {
                    if (distance < CheckDistance)
                        continue;
                }
                else if (DistanceMustBe == ESR_DistanceRule.Lower)
                {
                    if (distance > CheckDistance)
                        continue;
                }

                CellAllow = true;
                return;
            }
        }

    }
}