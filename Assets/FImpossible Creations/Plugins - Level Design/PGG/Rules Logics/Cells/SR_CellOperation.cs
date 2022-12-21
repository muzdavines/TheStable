using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Cells
{
    public class SR_CellOperation : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Cell Operation"; }
        public override string Tooltip() { return "Doing some operations on choosed cell"; }
        public EProcedureType Type { get { return EProcedureType.OnConditionsMet; } }

        public enum EOperation { ClearSpawn, ClearAllSpawns, OccupyCell, ClearAndOccupy, ClearAllAndOccupy }
        public EOperation Operation = EOperation.ClearSpawn;

        public Vector3Int TargetCellOffset = Vector3Int.zero;
        [PGG_SingleLineTwoProperties("PreventSpawn")]
        public bool OffsetWithRotation = false;
        [Tooltip("If you wan to prevent spawns for some tagged objects")]
        [HideInInspector] public string PreventSpawn = "";

        [Space(5)]
        [PGG_SingleLineSwitch("CheckMode", 60, "Select if you want to use Tags, SpawnStigma or CellData", 90)]
        [Tooltip("If you want to clear spawns with specific characteristics like tag etc.")]
        public string ToClearTag = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        public bool RotationOptions = false;
        [Tooltip("When value is Zero then parameter is not used!\nIf you want to destroy objects with same rotation as current spawn - with some angle tolerance")]
        [HideInInspector] public float RotationTolerance = 0f;
        [HideInInspector] public List<Vector3> OnRotations = new List<Vector3>();

        [HideInInspector] public CheckCellsSelectorSetup checkSetup = new CheckCellsSelectorSetup(true, false);
        [HideInInspector] public bool drawAdditionals = false;

#if UNITY_EDITOR

        public override void NodeBody(SerializedObject so)
        {
            Color preB = GUI.backgroundColor;
            if (drawAdditionals) GUI.backgroundColor = Color.gray;
            if (GUILayout.Button("Additional Options"))
            {
                drawAdditionals = !drawAdditionals;
            }

            GUI.backgroundColor = preB;

            if (drawAdditionals)
            {
                if (GUIIgnore.Count > 0) GUIIgnore.Clear();
            }
            else
            {
                if (GUIIgnore.Count == 0)
                {
                    GUIIgnore.Add("TargetCellOffset");
                    GUIIgnore.Add("OffsetWithRotation");
                    GUIIgnore.Add("ToClearTag");
                    GUIIgnore.Add("RotationOptions");
                }
            }

            base.NodeBody(so);
        }

        public override void NodeHeader()
        {
            base.NodeHeader();
            checkSetup.UseRotor = true;
            checkSetup.UseCondition = false;
            DrawMultiCellSelector(checkSetup, OwnerSpawner);
        }

        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            base.NodeFooter(so, mod);

            if (RotationOptions)
            {
                GUILayout.Space(3);
                EditorGUILayout.PropertyField(so.FindProperty("RotationTolerance"));
                GUILayout.Space(3);

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
            }

            so.ApplyModifiedProperties();
        }
#endif


        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            base.OnConditionsMetAction(mod, ref thisSpawn, preset, cell, grid);

            Vector3Int tgtCellPos = cell.Pos + TargetCellOffset;

            if (OffsetWithRotation)
            {
                Vector3 rot = thisSpawn.RotationOffset;

                if (rot != Vector3.zero)
                {
                    rot = (Quaternion.Euler(rot) * (Vector3)TargetCellOffset);
                    tgtCellPos = cell.Pos + new Vector3Int(Mathf.RoundToInt(rot.x), Mathf.RoundToInt(rot.y), Mathf.RoundToInt(rot.z));
                }
            }

            var tgtCell = cell;
            if ( tgtCellPos != cell.Pos) tgtCell = grid.GetCell(tgtCellPos);

            CellSelector_Execute(checkSetup, grid, cell, tgtCell, thisSpawn, (FieldCell c, SpawnData s) => ExecuteOnCell(cell, c, s));

            //if (FGenerators.CheckIfExist_NOTNULL((tgtCell)))
            //{
            //   ExecuteOnCell(cell, tgtCell, thisSpawn);

            #region Commented


            //SpawnData tgtSpawn = GetSpawnDataWithSpecifics(tgtCell, ToClearTag, CheckMode);

            //if (RotationOptions)
            //{
            //    if (OnRotations.Count > 0)
            //    {
            //        tgtSpawn = null;

            //        var datas = GetAllSpecificSpawns(tgtCell, ToClearTag, CheckMode, null, null);

            //        for (int d = 0; d < datas.Count; d++)
            //        {
            //            var data = datas[d];
            //            bool any = false;

            //            for (int i = 0; i < OnRotations.Count; i++)
            //            {
            //                float angle = Quaternion.Angle(Quaternion.Euler(data.RotationOffset), Quaternion.Euler(thisSpawn.RotationOffset + OnRotations[i]));
            //                //UnityEngine.Debug.Log("Angle( " + data.RotationOffset + " vs " + (thisSpawn.RotationOffset + OnRotations[i]) + " = " + angle);

            //                if (Mathf.Abs(angle) < 2f)
            //                {
            //                    tgtSpawn = data;
            //                    any = true;
            //                    break;
            //                }

            //            }

            //            if (any)
            //            {
            //                //UnityEngine.Debug.Log("Any! " );
            //                break;
            //            }

            //        }
            //    }
            //}

            //if (!string.IsNullOrEmpty(PreventSpawn))
            //{
            //    tgtCell.AddCellInstruction(GeneratePreventSpawns(PreventSpawn));
            //}

            //if (string.IsNullOrEmpty(ToClearTag))
            //{
            //    if (Operation == EOperation.ClearAllSpawns)
            //    {
            //        tgtCell.RemoveAllSpawnsFromCell();
            //    }
            //    else if (Operation == EOperation.ClearAndOccupy)
            //    {
            //        cell.OccupyOther(tgtCell);
            //    }
            //    else if (Operation == EOperation.OccupyCell)
            //    {
            //        cell.OccupyOther(tgtCell);
            //    }
            //    else if (Operation == EOperation.ClearAllAndOccupy)
            //    {
            //        tgtCell.RemoveAllSpawnsFromCell();
            //        cell.OccupyOther(tgtCell);
            //    }
            //}
            //else
            //{
            //    if (tgtSpawn != null)
            //    {
            //        if (Operation == EOperation.ClearSpawn)
            //        {
            //            tgtCell.RemoveSpawnFromCell(tgtSpawn);
            //        }
            //        else if (Operation == EOperation.ClearAllSpawns)
            //        {
            //            if (string.IsNullOrEmpty(ToClearTag) == false)
            //            {
            //                var spawns = tgtCell.CollectSpawns(OwnerSpawner.ScaleAccess);
            //                for (int i = spawns.Count - 1; i >= 0; i--)
            //                {
            //                    if (SpawnRuleBase.SpawnHaveSpecifics(spawns[i], ToClearTag, CheckMode)) spawns[i].Enabled = false; //tgtCell.Spawns.RemoveAt(i);
            //                }
            //            }
            //            else
            //                tgtCell.RemoveAllSpawnsFromCell();
            //        }
            //        else if (Operation == EOperation.ClearAndOccupy)
            //        {
            //            tgtCell.RemoveSpawnFromCell(tgtSpawn);
            //            cell.OccupyOther(tgtSpawn.ParentCell);
            //        }
            //        else if (Operation == EOperation.OccupyCell)
            //        {
            //            cell.OccupyOther(tgtSpawn.ParentCell);
            //        }
            //        else if (Operation == EOperation.ClearAllAndOccupy)
            //        {
            //            tgtCell.RemoveAllSpawnsFromCell();
            //            cell.OccupyOther(tgtSpawn.ParentCell);
            //        }
            //    }
            //}

            #endregion
            //}
        }

        void ExecuteOnCell(FieldCell origin, FieldCell newCell, SpawnData thisSpawn)
        {
            if (FGenerators.CheckIfIsNull((newCell))) return;

            SpawnData tgtSpawn = GetSpawnDataWithSpecifics(newCell, ToClearTag, CheckMode);

            if (RotationOptions)
            {
                if (OnRotations.Count > 0 || RotationTolerance > 0.005f)
                {
                    tgtSpawn = null;

                    var datas = GetAllSpecificSpawns(newCell, ToClearTag, CheckMode, null, null);

                    for (int d = 0; d < datas.Count; d++)
                    {
                        var data = datas[d];
                        bool any = false;

                        if (RotationTolerance > 0.005f)
                        {
                            float angle = Quaternion.Angle(data.GetRotationOffset(), thisSpawn.GetRotationOffset());
                            if (angle < RotationTolerance)
                            {
                                tgtSpawn = data;
                                any = true;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (!any)
                            for (int i = 0; i < OnRotations.Count; i++)
                            {
                                float angle = Quaternion.Angle(Quaternion.Euler(data.RotationOffset), Quaternion.Euler(thisSpawn.RotationOffset + OnRotations[i]));

                                if (Mathf.Abs(angle) < 2f)
                                {
                                    tgtSpawn = data;
                                    any = true;
                                    break;
                                }

                            }

                        if (any)
                        {
                            break;
                        }

                    }
                }
            }

            if (!string.IsNullOrEmpty(PreventSpawn))
            {
                newCell.AddCellInstruction(GeneratePreventSpawns(PreventSpawn));
            }

            if (string.IsNullOrEmpty(ToClearTag))
            {
                if (Operation == EOperation.ClearAllSpawns)
                {
                    newCell.RemoveAllSpawnsFromCell();
                }
                else if (Operation == EOperation.ClearAndOccupy)
                {
                    origin.OccupyOther(newCell);
                }
                else if (Operation == EOperation.OccupyCell)
                {
                    origin.OccupyOther(newCell);
                }
                else if (Operation == EOperation.ClearAllAndOccupy)
                {
                    newCell.RemoveAllSpawnsFromCell();
                    origin.OccupyOther(newCell);
                }
            }
            else
            {
                if (tgtSpawn != null)
                {
                    if (Operation == EOperation.ClearSpawn)
                    {
                        newCell.RemoveSpawnFromCell(tgtSpawn);
                    }
                    else if (Operation == EOperation.ClearAllSpawns)
                    {
                        if (string.IsNullOrEmpty(ToClearTag) == false)
                        {
                            var spawns = newCell.CollectSpawns(OwnerSpawner.ScaleAccess);
                            for (int i = spawns.Count - 1; i >= 0; i--)
                            {
                                if (SpawnRuleBase.SpawnHaveSpecifics(spawns[i], ToClearTag, CheckMode)) spawns[i].Enabled = false;
                            }
                        }
                        else
                            newCell.RemoveAllSpawnsFromCell();
                    }
                    else if (Operation == EOperation.ClearAndOccupy)
                    {
                        newCell.RemoveSpawnFromCell(tgtSpawn);
                        origin.OccupyOther(tgtSpawn.OwnerCell);
                    }
                    else if (Operation == EOperation.OccupyCell)
                    {
                        origin.OccupyOther(tgtSpawn.OwnerCell);
                    }
                    else if (Operation == EOperation.ClearAllAndOccupy)
                    {
                        newCell.RemoveAllSpawnsFromCell();
                        origin.OccupyOther(tgtSpawn.OwnerCell);
                    }
                }
            }
        }

    }
}