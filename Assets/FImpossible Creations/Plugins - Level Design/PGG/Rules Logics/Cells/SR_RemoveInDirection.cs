#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Cells.Legacy
{
    public class SR_RemoveInDirection : SpawnRuleBase, ISpawnProcedureType
    {
        public EProcedureType Type { get { return EProcedureType.OnConditionsMet; } }
        public override string TitleName() { return "Remove In Direction"; }
        public override string Tooltip() { return "Getting cells at desired direction and removing spawns if conditions are met"; }


        [PGG_SingleLineSwitch("CheckMode", 68, "Select if you want to use Tags, SpawnStigma or CellData", 40)]
        public string MustHaveTag = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        [Space(3)] public ESR_DirectionMode CellOffsetMode = ESR_DirectionMode.NoOffset;
        public Vector3Int OffsetCell = Vector3Int.zero;

        //[Space(5)]
        //public Vector3Int RemoveInDirection = new Vector3Int(0, 0, 1);
        [Space(6)] public ESR_AngleRemovalMode RemoveWhen = ESR_AngleRemovalMode.InFront;
        public ESR_DirectionMode UseSelfRotation = ESR_DirectionMode.WorldDirection;
        [Range(0, 181)] public float DegreesTolerance = 45f;

        //[Space(5)] [Range(0, 181)] public int CompareOffsetsAngle = 181;

#if UNITY_EDITOR
        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            base.NodeFooter(so, mod);

            GUIIgnore.Clear();
            if ( CellOffsetMode == ESR_DirectionMode.NoOffset) GUIIgnore.Add("OffsetCell");

            so.ApplyModifiedProperties();
        }
#endif


        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            var targetCell = cell;

            if (CellOffsetMode != ESR_DirectionMode.NoOffset) if (OffsetCell != Vector3Int.zero)
                {
                    Vector3Int off = OffsetCell;
                    if (CellOffsetMode == ESR_DirectionMode.CellRotateDirection) off = GetOffset(thisSpawn.GetRotationOffset(), off);
                    targetCell = grid.GetCell(cell.Pos + off, false);
                }

            if (FGenerators.CheckIfIsNull(targetCell )) return;


            var spawns = targetCell.CollectSpawns(OwnerSpawner.ScaleAccess);

            for (int s = 0; s < spawns.Count; s++)
            {
                if (spawns[s].OwnerMod == null) continue;
                if (spawns[s] == thisSpawn) continue;

                if (string.IsNullOrEmpty(MustHaveTag) == false) // If spawns must have ceratain tags
                {
                    if (SpawnHaveSpecifics(spawns[s], MustHaveTag, CheckMode) == false) // Not found required tags then skip this spawn
                        continue;
                }

                // Checking directional offset angle towards target spawn data
                if (DegreesTolerance < 181)
                {
                    float aa = CompareDirectionalAngle(thisSpawn, spawns[s], RemoveWhen, UseSelfRotation == ESR_DirectionMode.WorldDirection);

                    //if (cell.Pos == new Vector3Int(4, 0, 3))
                    //{
                    //    UnityEngine.Debug.Log(" " + cell.Pos + "  angle = " + aa);
                    //    //CompareDirectionalAngle(thisSpawn, spawns[s], RemoveWhen, UseSelfRotation == ESR_DirectionMode.WorldDirection, cell);
                    //}

                    if (Mathf.Abs(aa) > DegreesTolerance)
                    //if (CompareOffsetDirectionalAngle(thisSpawn, spawns[s], SpawnRules.GetRemovalAngle(RemoveWhen)) > DegreesTolerance)
                    {

                        continue; // Not met angle requirement then skip this spawn
                    }
                }

                // All requirements met then remove tagged
                spawns[s].Enabled = false;
                targetCell.RemoveSpawnFromCell(spawns[s]);

                return;
            }
        }

    }
}