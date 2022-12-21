using System.Collections.Generic;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Placement
{
    public partial class SR_MultiCheckCellNeightbours : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Multi check cell neightbours"; }
        public override string Tooltip() { return "Flexible rule to check neightbour cells on grid with multiple conditions\n[Mediumweight]"; }
        public EProcedureType Type { get { return EProcedureType.Rule; } }

        [HideInInspector] public QuarterRotationCheck quartRotor = new QuarterRotationCheck();
        [HideInInspector] public bool OverrideRotation = true;
        [HideInInspector] public float initRotation = 0;
        [HideInInspector] public float rotorEff = 90;
        [HideInInspector] public bool EachRotor = false;
        [HideInInspector] public float[] CustomRotors = new float[4];

        public List<NeightbourCheckRule> CheckRules = new List<NeightbourCheckRule>();


        [HideInInspector] public bool CheckPosit90 = false;
        [HideInInspector] public bool CheckNeg90 = false;
        [HideInInspector] public bool NotCheckDefaultPitch = false;

        [HideInInspector] public bool OverridePitchRotation = true;
        [HideInInspector] public float InitPitchRotation = 0;


        [System.Serializable]
        public class NeightbourCheckRule
        {
            public ESR_Space CheckedCellsMustBe = ESR_Space.Empty;

            //[PGG_SingleLineSwitch("CheckMode", 68, "Select if you want to use Tags, SpawnStigma or CellData", 90)]
            public string occupiedByTag = "";
            [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

            public Vector3Int OffsetOrigin = Vector3Int.zero;
            [Tooltip("Enable offset checking cells origin with current rotation offset")]
            [HideInInspector] public bool R = false;

            public bool DirectCheck = false;

            public NeightbourPlacement placement = new NeightbourPlacement();
            public bool Negate = false;
            public ESR_NeightbourCondition NeightbourNeeds = ESR_NeightbourCondition.AllNeeded;

            [Tooltip("Acquiring all available rotation processes out of other nodes for direct check")]
            [HideInInspector] public bool FullRotGet = false;

            #region Editor


#if UNITY_EDITOR
            public void DrawGUI(SerializedProperty sp, FieldModification mod, SR_MultiCheckCellNeightbours root)
            {
                if (sp == null) return;
                sp.serializedObject.Update();
                GUILayout.Space(5);
                Color c = GUI.color;

                EditorGUILayout.BeginHorizontal();
                SerializedProperty spNN = sp.FindPropertyRelative("NeightbourNeeds");
                if (spNN == null) return;

                // Neightbours
                EditorGUILayout.BeginVertical(GUILayout.Width(90));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Check On", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(60) });
                if (DrawMultiCellSelector(placement.AdvancedSetup, null))
                { if (placement == null) { placement = new NeightbourPlacement(); sp.serializedObject.ApplyModifiedProperties(); } CheckCellsSelectorWindow.Init(placement.AdvancedSetup, null); placement.Advanced_OnSelectorSwitch(); sp.serializedObject.ApplyModifiedProperties(); }

                EditorGUILayout.EndHorizontal();
                NeightbourPlacement.DrawGUI(placement);
                EditorGUILayout.PropertyField(spNN, GUIContent.none, GUILayout.Width(80));
                EditorGUILayout.EndVertical();

                // Others
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Spawn Mods", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(70) });

                EditorGUILayout.PropertyField(sp.FindPropertyRelative("CheckedCellsMustBe"));
                EditorGUILayout.PropertyField(sp.FindPropertyRelative("Negate"));
                if (CheckedCellsMustBe == ESR_Space.Occupied)
                {
                    //EditorGUILayout.PropertyField(so.FindProperty("occupiedBy"));
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(sp.FindPropertyRelative("occupiedByTag"));
                    PGGInspectorUtilities.DrawDetailsSwitcher(ref CheckMode);
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space(5);

                EditorGUIUtility.labelWidth = 36;
                SerializedProperty sp_offOr = sp.FindPropertyRelative("OffsetOrigin");

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(sp_offOr, new GUIContent("Origin", "Offset neightbours check origin by this number of cells"));
                EditorGUIUtility.labelWidth = 14;
                sp_offOr.Next(false); EditorGUILayout.PropertyField(sp_offOr, GUILayout.Width(30));
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();

                // Rotor
                //EditorGUILayout.BeginHorizontal();
                //EditorGUIUtility.labelWidth = 14;
                //sp_offOr.Next(false); EditorGUILayout.PropertyField(sp_offOr, GUILayout.Width(60) );
                //EditorGUIUtility.labelWidth = 50;

                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 48;
                EditorGUILayout.PropertyField(sp.FindPropertyRelative("DirectCheck"), new GUIContent("Direct", "If neightbours offsets should be aligned with current cell rotation"));
                EditorGUIUtility.labelWidth = 0;
                //EditorGUILayout.EndHorizontal();

                if (DirectCheck)
                {
                    EditorGUIUtility.labelWidth = 78;
                    EditorGUILayout.PropertyField(sp.FindPropertyRelative("FullRotGet"));
                }
                EditorGUIUtility.labelWidth = 0;

                EditorGUILayout.EndHorizontal();



                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();


            }
#endif

            #endregion

        }



        #region Editor

        int selected = -1;
#if UNITY_EDITOR

#if UNITY_2019_4_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)] void OnReload() { removeDisplayed = 0; } // Just to fix strange Unity error
#endif
        [System.NonSerialized] int removeDisplayed = 0; // Just to fix strange Unity error
        public override bool EditorIsLoading() { return removeDisplayed < 4; }

        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            if (Event.current.type == EventType.Layout)
                removeDisplayed += 1;

            if (Event.current.type == EventType.Repaint)
            {
                GUIIgnore.Clear(); //
            }

            if (removeDisplayed > 4)
            {
                if (Event.current.type == EventType.Repaint) GUIIgnore.Add("CheckRules");
            }
            else
            {
                if (CheckRules == null) { CheckRules = new List<NeightbourCheckRule>(); }
                SerializedProperty sp_list = so.FindProperty("CheckRules");
                if (sp_list != null)
                {
                    if (sp_list.isExpanded == false) { sp_list.isExpanded = true; }
                    if (removeDisplayed > 1) for (int i = 0; i < sp_list.arraySize; i++) { if (sp_list.GetArrayElementAtIndex(i) == null) continue; sp_list.GetArrayElementAtIndex(i).isExpanded = true; }
                }
            }


            //if (GUIIgnore.Count == 0) { GUIIgnore.Add("_test"); }
            if (CheckRules == null) { CheckRules = new List<NeightbourCheckRule>(); }
            else
            {
                if (CheckRules.Count == 0) CheckRules.Add(new NeightbourCheckRule());

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("<", GUILayout.Width(32))) selected--;
                if (selected == -1)
                    EditorGUILayout.LabelField("Rotor Setup (" + CheckRules.Count + ")", FEditor.FGUI_Resources.HeaderStyle);
                else
                    EditorGUILayout.LabelField("Neightbours Check " + (selected + 1) + "/" + CheckRules.Count, FEditor.FGUI_Resources.HeaderStyle);

                if (selected > -1) if (GUILayout.Button("-", GUILayout.Width(32))) { CheckRules.RemoveAt(selected); }
                if (GUILayout.Button("+", GUILayout.Width(32))) { CheckRules.Add(new NeightbourCheckRule()); }
                if (GUILayout.Button(">", GUILayout.Width(32))) selected++;

                if (selected < -1) selected = CheckRules.Count - 1;
                if (selected > CheckRules.Count - 1) selected = -1;

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(6);

            if (selected == -1)
            {
                EditorGUILayout.BeginHorizontal();

                // Rotor ------------------------------------------
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
                            EditorGUILayout.LabelField("(just 'check on')", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(100) });
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


                EditorGUILayout.EndVertical();
                GUILayout.Space(8);

                // Others -------------------------------------------

                EditorGUILayout.BeginVertical();

                GUILayout.Space(6);
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


                // Custom Rotors -------------------------------------------
                if (CustomRotors == null || CustomRotors.Length == 0) CustomRotors = new float[4];
                if (EachRotor)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Rotors", GUILayout.Width(60));
                    for (int i = 0; i < CustomRotors.Length; i++)
                        CustomRotors[i] = EditorGUILayout.FloatField(GUIContent.none, CustomRotors[i], GUILayout.Width(38));
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();



                EditorGUILayout.BeginVertical();

                GUILayout.Space(-6);
                EditorGUILayout.LabelField("Pitch Rotation Options", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(120));

                bool preEne = GUI.enabled;
                if (CheckPosit90 == false && CheckNeg90 == false) { NotCheckDefaultPitch = false; GUI.enabled = false; }
                GUI.backgroundColor = Color.gray;
                if (!NotCheckDefaultPitch) GUI.backgroundColor = Color.green;
                if (GUILayout.Button(new GUIContent("Check default Rotor", "Disable it if you don't want to check default flat space neightbour check like pitch rotation is = 0 (so the Yaw settings)")))
                {
                    NotCheckDefaultPitch = !NotCheckDefaultPitch;
                }
                GUI.enabled = preEne;

                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = Color.gray;
                if (CheckPosit90) GUI.backgroundColor = Color.green;
                if (GUILayout.Button("╔90\x00B0DOWN"))
                {
                    CheckPosit90 = !CheckPosit90;
                }

                GUI.backgroundColor = Color.gray;
                if (CheckNeg90) GUI.backgroundColor = Color.green;
                if (GUILayout.Button("╝ 90\x00B0UP"))
                {
                    CheckNeg90 = !CheckNeg90;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                if (CheckPosit90 == false && CheckNeg90 == false) { GUI.enabled = false; }
                
                EditorGUIUtility.labelWidth = 148;
                if (OverrideRotation)
                    EditorGUILayout.PropertyField(so.FindProperty("OverridePitchRotation"), GUILayout.Width(190));

                if (OverrideRotation == false) GUI.enabled = false;

                EditorGUIUtility.labelWidth = 118;
                EditorGUILayout.PropertyField(so.FindProperty("InitPitchRotation"), GUILayout.Width(150));
                GUI.enabled = preEne;

                EditorGUILayout.EndVertical();



                EditorGUILayout.EndHorizontal();
            }
            else
            {
                SerializedProperty spRulesList = so.FindProperty("CheckRules");
                CheckRules[selected].DrawGUI(spRulesList.GetArrayElementAtIndex(selected), mod, this);
            }
        }
#endif

        #endregion




        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (Enabled == false || Ignore) return;

            bool done = false;
            if (!NotCheckDefaultPitch) done = CheckRotor(Vector3.zero, mod, ref spawn, preset, cell, grid, restrictDirection);

            if (done) return;

            if (CheckPosit90) done = CheckRotor(new Vector3(90, 0, 0), mod, ref spawn, preset, cell, grid, restrictDirection);
            if (done) return;

            if (CheckNeg90) done = CheckRotor(new Vector3(-90, 0, 0), mod, ref spawn, preset, cell, grid, restrictDirection);
        }



        public bool CheckRotor(Vector3 extraAngles, FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {

            bool done = false;
            if (quartRotor.ISQuarter(1))
                if (CheckNeightbourCellAllow(cell, spawn, Vector3.zero + extraAngles, grid, restrictDirection, extraAngles))
                {
                    SetPlacementStats(ref spawn, EachRotor ? CustomRotors[0] : 0, extraAngles);
                    done = true;
                }

            if (!done)
                if (quartRotor.ISQuarter(2))
                    if (CheckNeightbourCellAllow(cell, spawn, new Vector3(0, 90, 0) + extraAngles, grid, restrictDirection, extraAngles))
                    {
                        SetPlacementStats(ref spawn, EachRotor ? CustomRotors[1] : rotorEff, extraAngles);
                        done = true;
                    }

            if (!done)
                if (quartRotor.ISQuarter(3))
                    if (CheckNeightbourCellAllow(cell, spawn, new Vector3(0, 180, 0) + extraAngles, grid, restrictDirection, extraAngles))
                    {
                        SetPlacementStats(ref spawn, EachRotor ? CustomRotors[2] : rotorEff * 2, extraAngles);
                        done = true;
                    }

            if (!done)
                if (quartRotor.ISQuarter(4))
                    if (CheckNeightbourCellAllow(cell, spawn, new Vector3(0, 270, 0) + extraAngles, grid, restrictDirection, extraAngles))
                    {
                        SetPlacementStats(ref spawn, EachRotor ? CustomRotors[3] : rotorEff * 3, extraAngles);
                        done = true;
                    }

            return done;
        }



        private bool CheckNeightbourCellAllow(FieldCell cell, SpawnData spawn, Vector3 rotationOffset, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection , Vector3 extraAngles)
        {
            bool allOk = true;

            for (int r = 0; r < CheckRules.Count; r++)
            {
                var rule = CheckRules[r];

                Quaternion rot;

                if (rule.DirectCheck)
                {
                    if (rule.FullRotGet)
                    {
                        rot = Quaternion.Euler(spawn.GetFullRotationOffset() + rotationOffset);
                    }
                    else
                    {
                        if ((spawn.RotationOffset + rotationOffset) == Vector3.zero)
                            rot = Quaternion.Euler(spawn.TempRotationOffset);
                        else
                            rot = Quaternion.Euler(spawn.RotationOffset + rotationOffset);
                    }
                }
                else
                    rot = Quaternion.Euler(rotationOffset);

                List<FieldCell> toCheck = new List<FieldCell>();

                if (rule.placement.UseAdvancedSetup == false)
                {
                    // Getting cells to check
                    for (int i = 0; i < 9; i++)
                    {
                        NeightbourPlacement.ENeightbour nn = (NeightbourPlacement.ENeightbour)i;
                        if (rule.placement.IsSelected(nn) == false) continue;

                        Vector3Int offset = GetOffset(rot, NeightbourPlacement.GetDirection(nn));
                        if (restrictDirection != null) if (restrictDirection.Value != Vector3.zero) if (offset.x != restrictDirection.Value.x || offset.z != restrictDirection.Value.z) continue;

                        var tgtCell = cell;

                        if (rule.OffsetOrigin != Vector3Int.zero)
                        {
                            if (rule.R)
                            {
                                Vector3Int tgtCellOffset = Vector3Int.zero;
                                Vector3 crot = spawn.RotationOffset;
                                crot = (Quaternion.Euler(crot) * (Vector3)rule.OffsetOrigin);
                                tgtCellOffset += new Vector3Int(Mathf.RoundToInt(crot.x), Mathf.RoundToInt(crot.y), Mathf.RoundToInt(crot.z));

                                if (tgtCellOffset != Vector3Int.zero) tgtCell = grid.GetCell(cell.Pos + tgtCellOffset, true);
                            }
                            else
                            {
                                tgtCell = grid.GetCell(cell.Pos + rule.OffsetOrigin, true);
                            }
                        }

                        Vector3Int oPos = tgtCell.Pos + offset;
                        FieldCell cl = grid.GetCell(oPos, false);

                        toCheck.Add(cl);
                    }
                }
                else // Advanced cells checking
                {
                    if (rule.DirectCheck == false) rot = Quaternion.identity;

                    int rotor = 0;
                    if (rotationOffset.y == 90) rotor = 1;
                    else if (rotationOffset.y == 180) rotor = 2;
                    else if (rotationOffset.y == 270) rotor = 3;
                    if (rule.placement.AdvancedSetup == null) rule.placement.AdvancedSetup = new List<Vector3Int>();

                    for (int i = 0; i < rule.placement.AdvancedSetup.Count; i++)
                    {
                        Vector3Int oPos = cell.Pos + rule.OffsetOrigin + rule.placement.Advanced_Rotate(rule.placement.AdvancedSetup[i], rot, rotor);
                        FieldCell cl = grid.GetCell(oPos, false);
                        toCheck.Add(cl);
                    }

                }


                // Checking cells with logics
                bool allCorrect = false;
                int correctCount = 0;

                for (int i = 0; i < toCheck.Count; i++)
                {
                    if (SpawnRules.CheckNeightbourCellAllow(rule.CheckedCellsMustBe, toCheck[i], rule.occupiedByTag, rule.CheckMode))
                    {
                        if (rule.NeightbourNeeds == ESR_NeightbourCondition.AtLeastOne)
                        {
                            allCorrect = true;
                            //break;
                        }
                        else if (rule.NeightbourNeeds == ESR_NeightbourCondition.AllNeeded)
                        {
                            correctCount += 1;

                            if (rule.placement.UseAdvancedSetup == false)
                            {
                                if (correctCount == rule.placement.SelectedCount())
                                {
                                    allCorrect = true;
                                    //break;
                                }
                            }
                            else
                            {
                                if (correctCount == rule.placement.AdvancedSetup.Count)
                                {
                                    allCorrect = true;
                                    //break;
                                }
                            }
                        }
                    }

                }


                if (rule.Negate) allCorrect = !allCorrect;

                if (allCorrect == false)
                {
                    allOk = false;
                    break;
                }

            }

            return allOk;
        }

        public override void ResetRule(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset)
        {
            base.ResetRule(grid, preset);
            rot = null;
        }

        public Vector3? rot = null;

        public void SetPlacementStats(ref SpawnData spawn, float? angle, Vector3 extraAngles)
        {
            if (angle != null)
            {
                rot = Vector3.up * (initRotation + angle.Value);
                if (OverrideRotation) if (rot != null)
                    {
                        spawn.RotationOffset = rot.Value;

                        if (OverridePitchRotation)
                            if (extraAngles != Vector3.zero)
                            {
                                Vector3 pitchR = extraAngles + new Vector3(InitPitchRotation, 0, 0);
                                spawn.RotationOffset += pitchR;
                                rot = rot.Value + pitchR;
                            }
                    }
            }

            CellAllow = true;
        }

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (OverrideRotation) if (rot != null) spawn.RotationOffset = rot.Value;
        }

    }
}