using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Cells.Legacy
{
    public class SR_RemoveOnRotations : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Remove On Rotations"; }
        public override string Tooltip() { return "Checking state of choosed cell to allow or disallow spawn when spawns rotations are met"; }


        [PGG_SingleLineSwitch("CheckMode", 68, "Select if you want to use Tags, SpawnStigma or CellData", 40)]
        public string MustHaveTag = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        public bool RemoveJustOne = true;
        public Vector3Int OffsetCell = Vector3Int.zero;
        public bool OffsetWithRotation = false;
        [Space(5)]
        [HideInInspector] public List<Vector3> OnRotations = new List<Vector3>();
        public EProcedureType Type { get { return EProcedureType.OnConditionsMet; } }

#if UNITY_EDITOR
        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            base.NodeFooter(so, mod);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("On Rotations: ");
            if (GUILayout.Button("+", GUILayout.Width(24))) OnRotations.Add(new Vector3());
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < OnRotations.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                OnRotations[i] = EditorGUILayout.Vector3Field("[" + i + "]", OnRotations[i]);
                if (GUILayout.Button("X", GUILayout.Width(24))) { OnRotations.RemoveAt(i); break; }
                EditorGUILayout.EndHorizontal();
            }

            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(so.targetObject);

            so.ApplyModifiedProperties();
        }
#endif

        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            var targetCell = cell;

            if (OffsetCell != Vector3Int.zero)
            {
                Vector3Int off = OffsetCell;
                if (OffsetWithRotation) off = GetOffset(thisSpawn.GetRotationOffset(), off);
                targetCell = grid.GetCell(cell.Pos + off, false);
            }

            if (FGenerators.CheckIfIsNull(targetCell )) return;

            var spawns = targetCell.CollectSpawns(OwnerSpawner.ScaleAccess);

            for (int s = 0; s < spawns.Count; s++)
            {
                if (spawns[s].OwnerMod == null) continue;
                if (spawns[s] == thisSpawn) continue;


                if (string.IsNullOrEmpty(MustHaveTag) == false) // There are tags to check
                {
                    if (SpawnHaveSpecifics(spawns[s], MustHaveTag, CheckMode) == false) // Not found required tags then skip this spawn
                    {
                        continue;
                    }
                }

                // Rotation check
                if (OnRotations != null)
                {
                    if (OnRotations.Count > 0)
                    {
                        bool was = false;
                        for (int i = 0; i < OnRotations.Count; i++)
                            if (spawns[s].RotationOffset == OnRotations[i])
                            {
                                was = true;
                                break; // Break rotations check loop
                            }

                        if (!was) continue; // Wasnt required rotation then skip this spawn
                    }
                }

                // All requirements met then remove tagged
                spawns[s].Enabled = false;
                targetCell.RemoveSpawnFromCell(spawns[s]);

                if (RemoveJustOne) return;
            }
        }

    }
}