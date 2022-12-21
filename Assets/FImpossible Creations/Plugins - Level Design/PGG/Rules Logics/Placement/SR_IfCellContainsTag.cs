using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

namespace FIMSpace.Generating.Rules.Placement
{
    public partial class SR_IfCellContainsTag : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "If cell contains Tag"; }
        public override string Tooltip() { return "Checking cell spawns tags to allow or disallow spawning\n[Lightweight]"; }
        public EProcedureType Type { get { return EProcedureType.Rule; } }

        [PGG_SingleLineSwitch("CheckMode", 68, "Select if you want to use Tags, SpawnStigma or CellData", 40)]
        public string Tag = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        [HideInInspector] public bool UseRotationsCheck = false;
        [HideInInspector] public bool OnlyWithSameDirection = false;
        [HideInInspector] [Range(0, 181)] public int CompareOffsetsAngle = 181;
        [HideInInspector] public CheckCellsSelectorSetup checkSetup = new CheckCellsSelectorSetup(true, true);


        #region Editor stuff
#if UNITY_EDITOR

        public override void NodeHeader()
        {
            base.NodeHeader();
            DrawMultiCellSelector(checkSetup, OwnerSpawner);
        }

        public override void NodeBody(SerializedObject so)
        {
            checkSetup.UseRotor = true;
            checkSetup.UseCondition = true;

            GUIIgnore.Clear(); GUIIgnore.Add("Tag");
            base.NodeBody(so);

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            SerializedProperty sp = so.FindProperty("Tag");
            EditorGUILayout.PropertyField(sp);

            EditorGUIUtility.labelWidth = 8;
            UseRotationsCheck = EditorGUILayout.Toggle(new GUIContent(" ", "Use rotations check for choosing desired spawn"), UseRotationsCheck, GUILayout.Width(40));
            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.EndHorizontal();

            if (UseRotationsCheck)
            {
                GUILayout.Space(4);
                sp.Next(false);
                sp.Next(false); EditorGUILayout.PropertyField(sp);
                sp.Next(false); EditorGUILayout.PropertyField(sp);
            }

        }
#endif
        #endregion


        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);

            if (string.IsNullOrEmpty(Tag)) return;

            if (CheckMode == ESR_Details.CellData)
            {
                CellAllow = CellSelector_CheckCondition(checkSetup, grid, cell, spawn, (FieldCell fc) => { return CellHaveData(fc, Tag); });

                #region Backup

                //if (CellSelector_CustomSelection(checkSetup) == false)
                //{
                //    if (CellHaveData(cell, Tag)) CellAllow = true;
                //}
                //else
                //{
                //    //CellAllow = CellSelector_CheckCondition(checkSetup, grid, cell, spawn, (FieldCell fc) => { return CellHaveData(fc, Tag); });
                //    bool condMet = CellSelector_InitialCondition(checkSetup);
                //    for (int i = 0; i < CellSelector_SelectedCount(checkSetup); i++)
                //    {
                //        bool isOk = CellHaveData(CellSelector_GetSingleSelectedCell(checkSetup, grid, cell, spawn, i), Tag);
                //        if (checkSetup.Condition == ESR_NeightbourCondition.AtLeastOne) 
                //        { if (isOk) { condMet = true; break; } }
                //        else if (!isOk) { condMet = false; break; } // All needed
                //    }

                //    if (condMet) CellAllow = true;
                //}

                #endregion

                return;
            }

            CellAllow = CellSelector_CheckCondition(checkSetup, grid, cell, spawn, OwnerSpawner, (FieldCell fc, SpawnData spawnD, FieldSpawner spawner) => { return CheckAllow(fc, spawnD, spawner); });
        }

        //
        bool CheckAllow(FieldCell cell, SpawnData spawn, FieldSpawner spawner)
        {
            if (cell.IsNull()) return false;

            var spawns = cell.CollectSpawns(spawner.ScaleAccess);

            for (int s = 0; s < spawns.Count; s++)
            {
                if (spawns[s].OwnerMod == null) continue;
                if (spawns[s] == spawn) continue;

                if (SpawnHaveSpecifics(spawns[s], Tag, CheckMode))
                {

                    if (UseRotationsCheck)
                    {
                        bool rotMet = false;

                        if (OnlyWithSameDirection)
                        {
                            float angle = Quaternion.Angle(Quaternion.Euler(spawn.RotationOffset), Quaternion.Euler(spawns[s].RotationOffset));
                            if (angle < 1f)
                            {
                                rotMet = true;
                            }
                        }
                        else
                        {
                            rotMet = true;
                        }

                        if (CompareOffsetsAngle < 181)
                        {
                            if (CompareOffsetDirectionalAngle(spawn, spawns[s]) <= CompareOffsetsAngle)
                            {
                                return true; // Met angle 
                            }
                        }
                        else
                        {
                            if (rotMet)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }


    }
}