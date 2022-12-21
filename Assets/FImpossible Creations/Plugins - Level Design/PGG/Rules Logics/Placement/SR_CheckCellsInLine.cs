using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Placement.Alpha
{
    public class SR_CheckCellsInLine : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Check cells in line"; }
        public override string Tooltip() { return "Checking cells placed in target direction"; }
        public EProcedureType Type { get { return EProcedureType.Rule; } }

        public int CellsLineLength = 3;
        public ESR_Space CheckedCellsMustBe = ESR_Space.Occupied;
        [HideInInspector] public ESR_NeightbourCondition NeightbourNeeds = ESR_NeightbourCondition.AllNeeded;
        [HideInInspector] [Range(0, 180)] public int ignoreAngled = 0;

        [PGG_SingleLineSwitch("CheckMode", 68, "Select if you want to use Tags, SpawnStigma or CellData", 110)]
        public string occupiedByTag = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        [HideInInspector] public bool DirectCheck = false;
        [HideInInspector] public Vector3Int OffsetOrigin = Vector3Int.zero;
        [HideInInspector] public NeightbourPlacement placement = new NeightbourPlacement();
        [HideInInspector] public QuarterRotationCheck quartRotor = new QuarterRotationCheck();
        [HideInInspector] public bool OverrideRotation = false;
        [HideInInspector] public float initRotation = 0;
        [HideInInspector] public float rotorEff = 90;
        [HideInInspector] public int spawnOn = 5;
        [HideInInspector] public bool EachRotor = false;
        [HideInInspector] public float[] CustomRotors = new float[4];


        #region Editor


#if UNITY_EDITOR
        private static string[] guiSet = new string[] { "┌", "▲", "┐", "◄", "●", "►", "└", "▼", "┘" };
        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            if (CheckedCellsMustBe == ESR_Space.Occupied)
            {
                //EditorGUILayout.PropertyField(so.FindProperty("occupiedBy"));
                EditorGUILayout.PropertyField(so.FindProperty("occupiedByTag"));
                EditorGUILayout.PropertyField(so.FindProperty("ignoreAngled"));
            }

            if (CellsLineLength < 1) CellsLineLength = 1;

            GUILayout.Space(9);
            Color c = GUI.color;
            EditorGUILayout.BeginHorizontal();


            // Neightbours
            EditorGUILayout.BeginVertical(GUILayout.Width(90));
            EditorGUILayout.LabelField("Direction", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(70) });
            NeightbourPlacement.DrawGUI(placement, guiSet, 22, 22, true);
            EditorGUILayout.PropertyField(so.FindProperty("NeightbourNeeds"), GUIContent.none, GUILayout.Width(80));
            EditorGUILayout.EndVertical();



            // Rotor
            EditorGUILayout.BeginVertical(GUILayout.Width(90));
            EditorGUILayout.LabelField("Rotor Check", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(70) });
            GUILayout.Space(2);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10); QuarterRotationCheck.DrawGUI(quartRotor);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(2);
            if (quartRotor.AnyChecked())
            {
                if (quartRotor.AllChecked())
                    EditorGUILayout.LabelField("(all 4 quarts)", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(70) });
                else
                    if (quartRotor.OnlyOneChecked())
                {
                    if (quartRotor.ISQuarter(1))
                        EditorGUILayout.LabelField("(just 'Direction')", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(100) });
                    else
                        EditorGUILayout.LabelField("(rotated 'check on')", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(100) });
                }
                else
                    if (quartRotor.Only180Checked())
                    EditorGUILayout.LabelField("(180° neightbours)", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(108) });
                else
                    EditorGUILayout.LabelField("(90° neightbours)", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(100) });
            }
            else
                EditorGUILayout.LabelField("Select at least one", EditorStyles.miniLabel, new GUILayoutOption[] { GUILayout.Width(100) });

            EditorGUIUtility.labelWidth = 50;
            EditorGUILayout.PropertyField(so.FindProperty("DirectCheck"), new GUIContent("Direct", "If neightbours offsets should be aligned with current cell rotation"));
            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.EndVertical();



            // Others
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Spawn Mods", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(70) });

            EditorGUIUtility.labelWidth = 108;
            EditorGUILayout.PropertyField(so.FindProperty("OverrideRotation"), GUILayout.Width(150));

            bool preEn = GUI.enabled;
            if (OverrideRotation == false) GUI.enabled = false;

            EditorGUIUtility.labelWidth = 78;
            EditorGUILayout.PropertyField(so.FindProperty("initRotation"), GUILayout.Width(110));

            // Rotor effect
            if (quartRotor.CountChecked() > 1)
            {
                EditorGUIUtility.labelWidth = 60;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(so.FindProperty("rotorEff"), GUILayout.Width(86));

                EditorGUIUtility.labelWidth = 1;
                EditorGUILayout.PropertyField(so.FindProperty("EachRotor"), GUILayout.Width(22));

                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = 0;
            }

            if (OverrideRotation == false) GUI.enabled = preEn;
            EditorGUIUtility.labelWidth = 36;
            EditorGUILayout.PropertyField(so.FindProperty("OffsetOrigin"), new GUIContent("Origin", "Offset neightbours check origin by this number of cells"));
            EditorGUIUtility.labelWidth = 0;


            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (CustomRotors == null || CustomRotors.Length == 0) CustomRotors = new float[4];
            if (EachRotor)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Rotors", GUILayout.Width(60));
                for (int i = 0; i < CustomRotors.Length; i++)
                    CustomRotors[i] = EditorGUILayout.FloatField(GUIContent.none, CustomRotors[i], GUILayout.Width(38));
                EditorGUILayout.EndHorizontal();
            }
        }
#endif

        #endregion


        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (Enabled == false || Ignore) return;

            bool done = false;
            if (quartRotor.ISQuarter(1))
                if (CheckLine(cell, spawn, Vector3.zero, grid, restrictDirection))
                {
                    SetPlacementStats(ref spawn, EachRotor ? CustomRotors[0] : 0);
                    done = true;
                }

            if (!done)
                if (quartRotor.ISQuarter(2))
                    if (CheckLine(cell, spawn, new Vector3(0, 90, 0), grid, restrictDirection))
                    {
                        SetPlacementStats(ref spawn, EachRotor ? CustomRotors[1] : rotorEff);
                        done = true;
                    }

            if (!done)
                if (quartRotor.ISQuarter(3))
                    if (CheckLine(cell, spawn, new Vector3(0, 180, 0), grid, restrictDirection))
                    {
                        SetPlacementStats(ref spawn, EachRotor ? CustomRotors[2] : rotorEff * 2);
                        done = true;
                    }

            if (!done)
                if (quartRotor.ISQuarter(4))
                    if (CheckLine(cell, spawn, new Vector3(0, 270, 0), grid, restrictDirection))
                    {
                        SetPlacementStats(ref spawn, EachRotor ? CustomRotors[3] : rotorEff * 3);
                    }
        }


        private bool CheckLine(FieldCell cell, SpawnData spawn, Vector3 rotationOffset, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            Quaternion rot = DirectCheck ? Quaternion.Euler(spawn.RotationOffset + spawn.TempRotationOffset + rotationOffset) : Quaternion.Euler(rotationOffset);
            List<FieldCell> toCheck = new List<FieldCell>();

            Vector3 checkDirStep = (NeightbourPlacement.GetDirection(placement.GetFirstSelectedNeightbourID()));
            Vector3Int offset = GetCollOffset(rot, checkDirStep);

            int start = placement.m ? 0 : 1;

            // Getting cells to check
            for (int i = start; i <= CellsLineLength; i++)
            {
                if (restrictDirection != null) if (restrictDirection.Value != Vector3.zero) if (offset.x != restrictDirection.Value.x || offset.z != restrictDirection.Value.z) continue;

                Vector3Int oPos = cell.Pos + OffsetOrigin + offset * (i);
                FieldCell cl = grid.GetCell(oPos, false);

                toCheck.Add(cl);
            }

            // Checking cells with logics
            bool allCorrect = false;
            int correctCount = 0;
            int need = CellsLineLength + (placement.m ? 1 : 0);
            Vector3 checkD = ((Vector3)offset).normalized;

            for (int i = 0; i < toCheck.Count; i++)
            {
                if (SpawnRules.CheckNeightbourCellAllowAngled(CheckedCellsMustBe, toCheck[i], occupiedByTag, checkD, ignoreAngled, CheckMode))
                    {
                        if (NeightbourNeeds == ESR_NeightbourCondition.AtLeastOne)
                        {
                            allCorrect = true;
                            break;
                        }
                        else if (NeightbourNeeds == ESR_NeightbourCondition.AllNeeded)
                        {
                            correctCount += 1;
                            if (correctCount == need)
                            {
                                allCorrect = true;
                                break;
                            }
                        }
                    }
            }

            return allCorrect;
        }

        //public bool CheckAdditionalRequirements(RoomCell cell, Quaternion checkDir)
        //{
        //    if (CheckedCellsMustBe == ESR_Space.Occupied)
        //        if (string.IsNullOrEmpty(occupiedByTag) == false)
        //            if (ignoreAngled > 0)
        //            {
        //                var spawns = GetAllTaggedSpawns(cell, occupiedByTag);
        //                for (int i = 0; i < spawns.Count; i++)
        //                {
        //                    var spawn = spawns[i];
        //                    float angle = Quaternion.Angle(checkDir, spawn.)
        //                }
        //            }

        //    return true;
        //}

        public override void ResetRule(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset)
        {
            base.ResetRule(grid, preset);
            rot = null;
        }


        public Vector3? rot = null;
        public void SetPlacementStats(ref SpawnData spawn, float? angle = null)
        {
            if (angle != null)
            {
                rot = Vector3.up * (initRotation + angle.Value);
                if (OverrideRotation) if (rot != null) spawn.RotationOffset = rot.Value;
            }

            CellAllow = true;
        }


        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (OverrideRotation) if (rot != null) spawn.RotationOffset = rot.Value;
        }


        Vector3Int GetCollOffset(Quaternion rot, Vector3 dir)
        {
            Vector3 off = rot * dir;
            return new Vector3Int(Mathf.RoundToInt(off.x), Mathf.RoundToInt(off.y), Mathf.RoundToInt(off.z));
        }

    }
}