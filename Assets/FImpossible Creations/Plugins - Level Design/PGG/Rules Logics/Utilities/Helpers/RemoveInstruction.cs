#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Helpers
{
    [System.Serializable]
    public class RemoveInstruction
    {

        //[PGG_SingleLineSwitch("CheckMode", 68, "Select if you want to use Tags, SpawnStigma or CellData", 40)]
        public string AffectedTags = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        public int RemoveCount = 1;
        public ESR_DirectionMode CheckOffset = ESR_DirectionMode.NoOffset;
        public Vector3Int OffsetCellPosition = Vector3Int.zero;

        // RemoveOffsettedInDirection
        public ESR_AngleRemovalMode RemoveWhen = ESR_AngleRemovalMode.Any;
        public ESR_DirectionMode UseSelfRotation = ESR_DirectionMode.WorldDirection;
        [Range(0, 181)] public float DegreesTolerance = 45f;

        int removed = 0;
        public bool ProceedRemoving(FieldSpawner spawner, ref SpawnData spawn, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            var targetCell = cell;

            if (CheckOffset != ESR_DirectionMode.NoOffset) if (OffsetCellPosition != Vector3Int.zero)
                {
                    Vector3Int off = OffsetCellPosition;
                    if (CheckOffset == ESR_DirectionMode.CellRotateDirection) off = SpawnRuleBase.GetOffset(spawn.GetRotationOffset(), off);
                    targetCell = grid.GetCell(cell.Pos + off, false);
                }

            //if (FGenerators.CheckIfIsNull( targetCell ) )
            //{
            //    //UnityEngine.Debug.Log("tgt cell is null");
            //    return false;
            //}

            removed = 0;
            SpawnRuleBase.CellSelector_Execute(checkSetup, grid, targetCell, targetCell, spawn, (FieldCell f, SpawnData s) => ProceedRemovingOnCell(f, s, spawner));

            #region Commented

            //var spawns = targetCell.CollectSpawns(spawner.ScaleAccess);

            //for (int s = spawns.Count - 1; s >= 0; s--)
            //{
            //    if (spawns[s].OwnerMod == null)
            //    {
            //        //UnityEngine.Debug.Log("owner null");
            //        continue;
            //    }
            //    if (spawns[s] == spawn)
            //    {
            //        //UnityEngine.Debug.Log("its this");
            //        continue;
            //    }

            //    if (string.IsNullOrEmpty(AffectedTags) == false) // If spawns must have ceratain tags
            //    {
            //        if (SpawnRuleBase.SpawnHaveSpecifics(spawns[s], AffectedTags, CheckMode) == false) // Not found required tags then skip this spawn
            //            continue;
            //    }

            //    if (RemoveWhen != ESR_AngleRemovalMode.Any)
            //    {
            //        // Checking directional offset angle towards target spawn data
            //        if (DegreesTolerance < 181)
            //        {
            //            float aa = SpawnRuleBase.CompareDirectionalAngle(spawn, spawns[s], RemoveWhen, UseSelfRotation == ESR_DirectionMode.WorldDirection);

            //            if (Mathf.Abs(aa) > DegreesTolerance)
            //                continue; // Not met angle requirement then skip this spawn
            //        }
            //    }

            //    // All requirements met then remove spawns
            //    spawns[s].Enabled = false;
            //    targetCell.RemoveSpawnFromCell(spawns[s]);
            //    removed += 1;

            //    if (RemoveCount != 0)
            //    {
            //        if (removed == RemoveCount) return true;
            //    }
            //}

            #endregion

            return removed > 0; // removed spawns > 0 means removed something during this method
        }

        public void ProceedRemovingOnCell(FieldCell cell, SpawnData spawn, FieldSpawner spawner)
        {
            if (FGenerators.CheckIfIsNull(cell)) return;
            if (RemoveCount != 0) if (removed == RemoveCount) return;

            var spawns = cell.CollectSpawns(spawner.ScaleAccess);

            for (int s = spawns.Count - 1; s >= 0; s--)
            {
                if (spawns[s].OwnerMod == null)
                {
                    //UnityEngine.Debug.Log("owner null");
                    continue;
                }
                if (spawns[s] == spawn)
                {
                    //UnityEngine.Debug.Log("its this");
                    continue;
                }

                if (string.IsNullOrEmpty(AffectedTags) == false) // If spawns must have ceratain tags
                {
                    if (SpawnRuleBase.SpawnHaveSpecifics(spawns[s], AffectedTags, CheckMode) == false) // Not found required tags then skip this spawn
                        continue;
                }

                if (RemoveWhen != ESR_AngleRemovalMode.Any)
                {
                    // Checking directional offset angle towards target spawn data
                    if (DegreesTolerance < 181)
                    {
                        float aa = SpawnRuleBase.CompareDirectionalAngle(spawn, spawns[s], RemoveWhen, UseSelfRotation == ESR_DirectionMode.WorldDirection);

                        if (Mathf.Abs(aa) > DegreesTolerance)
                            continue; // Not met angle requirement then skip this spawn
                    }
                }

                // All requirements met then remove spawns
                spawns[s].Enabled = false;
                cell.RemoveSpawnFromCell(spawns[s]);
                removed += 1;
                
                if (RemoveCount != 0)
                {
                    if (removed == RemoveCount) return;
                }
            }
        }

        [HideInInspector] public CheckCellsSelectorSetup checkSetup = new CheckCellsSelectorSetup(true, false);

#if UNITY_EDITOR
        public static void DrawGUI(SerializedProperty sp, RemoveInstruction instr)
        {
            if (sp == null) return;
            if (instr == null) return;
            if (sp.Next(true) == false) return;

            //try
            //{
                GUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 94;
                EditorGUILayout.PropertyField(sp); // Affected Tags
                PGGInspectorUtilities.DrawDetailsSwitcher(ref instr.CheckMode, 58);
                sp.Next(false);

                if (FGenerators.CheckIfExist_NOTNULL(instr))
                {
                    instr.checkSetup.UseCondition = false;
                    SpawnRuleBase.DrawMultiCellSelector(instr.checkSetup, null);
                }

                GUILayout.FlexibleSpace();
                //GUILayout.Space(8);
                EditorGUIUtility.labelWidth = 90;
                sp.Next(false);

                if (instr.RemoveCount <= 0) // Remove Count
                {
                    instr.RemoveCount = 0;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(sp, GUILayout.Width(120));
                    EditorGUILayout.LabelField("(all)", GUILayout.Width(30));
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.PropertyField(sp, GUILayout.Width(124));
                    GUILayout.Space(8);
                }

                EditorGUIUtility.labelWidth = 0;

                GUILayout.EndHorizontal();

                if (instr.CheckOffset != ESR_DirectionMode.NoOffset) GUILayout.Space(5);
                sp.Next(false); EditorGUILayout.PropertyField(sp); // Offset Cell
                sp.Next(false);
                if (instr.CheckOffset != ESR_DirectionMode.NoOffset) EditorGUILayout.PropertyField(sp);  // Offset cell position
                sp.Next(false);

                if (instr.RemoveWhen != ESR_AngleRemovalMode.Any) GUILayout.Space(5);

                EditorGUILayout.PropertyField(sp); // Remove When
                sp.Next(false);

                if (instr.RemoveWhen != ESR_AngleRemovalMode.Any)
                {
                    EditorGUILayout.PropertyField(sp);
                    sp.Next(false); EditorGUILayout.PropertyField(sp);
                }
                else
                {
                    sp.Next(false);
                }

            //}
            //catch (System.Exception)
            //{

            //}

        }
#endif

    }
}