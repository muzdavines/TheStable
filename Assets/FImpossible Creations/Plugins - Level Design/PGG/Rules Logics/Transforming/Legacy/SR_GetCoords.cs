using System.Collections.Generic;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.QuickSolutions
{
    public class SR_GetCoords : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Get Coordinates"; }
        public override string Tooltip() { return "Getting spawning coordinates (position,rotation) from target spawn with choosed tag and offsetting for desired placement\nor leave tag field empty to just set custom coordinates.\n" + base.Tooltip(); }

        public EProcedureType Type { get { return EProcedureType.Coded; } }

        [PGG_SingleLineSwitch("CheckMode", 68, "Select if you want to use Tags, SpawnStigma or CellData", 105)]
        public string GetFromTagged = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        [Space(5)]
        [PGG_SingleLineSwitch("OffsetMode", 58, "Select if you want to offset postion with cell size or world units", 140)]
        [HideInInspector] public Vector3 WorldOffset = Vector3.zero;
        [HideInInspector] public ESR_Measuring OffsetMode = ESR_Measuring.Units;

        [Tooltip("Translation accordingly with object's rotation")]
        [HideInInspector] public Vector3 DirectionalOffset = Vector3.zero;

        [Tooltip("Add this translation to current spawns' translation instead of overriding")]
        [HideInInspector] public bool StackOffset = false;

        [Space(5)]
        [HideInInspector] public Vector3 RotationEulerOffset = Vector3.zero;

        [Space(5)]
        [HideInInspector] public bool GetScale = false;
        [HideInInspector] public Vector3 ScaleMultiplier = Vector3.one;

        [Space(5)]
        [HideInInspector] public Vector3 RandomOffsets = Vector3.zero;
        [HideInInspector] public Vector3 RandomLocalRotation = Vector3.zero;
        [HideInInspector] public Vector3 RandomScale = Vector3.zero;

        [Space(5)]
        [HideInInspector] public Vector3 PivotOffset = Vector3.zero;
        [HideInInspector] public Vector3 MultiplyGetted = Vector3.one;

        [Space(7)]
        [HideInInspector] public bool DontSpawnIfNoTagged = false;
        /*[PGG_SingleLineTwoProperties("AdditiveOffset")] */
        [HideInInspector] public bool RemoveTagged = false;
        //[Tooltip("If target offsets should be added to previous offsets instead of overriding coords")] [HideInInspector] public bool AdditiveOffset = false;
        //[HideInInspector] [Tooltip("If you work with rotations with local position offset and you notice something moves like in old rotation space translation, then toggling it will force node to be executed before events and may sync better in some situations")]
        //public bool RunBeforeEvents = false;

        [PGG_SingleLineTwoProperties("RunOnRepetition")]
        [Tooltip("If found few spawns with desired tag in current cell, then coords will be getted from random spawn with desired tag")]
        [HideInInspector] public bool GetRandomIfMulti = false;
        [HideInInspector]
        [Tooltip("If found few spawns in cell which met requirements then spawner will be triggered for each correct spawn (needs to use tag/name find to be triggered)")]
        public bool RunOnRepetition = false;

        public enum ETranslateOnly { Default, OnlyPositie, OnlyNegarive, AllPositive, AllNegative }
        [Tooltip("(Only with directional offset) If you need all spawns move in one direction not matter how it's rotated")]
        [HideInInspector] public ETranslateOnly HelperMode = ETranslateOnly.Default;

        //[PGG_SingleLineSwitch("IgnoreCheckMode", 68, "Select if you want to use Tags, SpawnStigma or CellData", 105)]
        //public string IgnoreCoordsOf = "";
        //[HideInInspector] public ESR_Details IgnoreCheckMode = ESR_Details.Tag;

        private List<SpawnData> getted = new List<SpawnData>();

        [HideInInspector] public bool _DrawOffsets = true;
        [HideInInspector] public bool _DrawRandomization = false;
        [HideInInspector] public bool _DrawAdditionals = false;


        #region Drawing Inspector Window

#if UNITY_EDITOR

        private void OnEnable()
        {
            if (GUIIgnore.Count != 1) GUIIgnore.Add("GetFromTagged");
        }

        public override void NodeBody(SerializedObject so)
        {
            GUILayout.Space(2);
            SerializedProperty sp = so.FindProperty("GetFromTagged");
            EditorGUILayout.PropertyField(sp);
            GUILayout.Space(6);

            FGUI_Inspector.FoldHeaderStart(ref _DrawOffsets, " Offset Settings", FGUI_Resources.BGInBoxStyle, FGUI_Resources.Tex_Movement, 19);

            if (_DrawOffsets)
            {
                GUILayout.Space(3);
                sp.Next(false); sp.Next(false); EditorGUILayout.PropertyField(sp);
                sp.Next(false); sp.Next(false); EditorGUILayout.PropertyField(sp);
                sp.Next(false); EditorGUILayout.PropertyField(sp);

                sp.Next(false); EditorGUILayout.PropertyField(sp);
                sp.Next(false); EditorGUILayout.PropertyField(sp);
                sp.Next(false); EditorGUILayout.PropertyField(sp);
                GUILayout.Space(5);
            }
            else { sp.Next(false); sp.Next(false); sp.Next(false); sp.Next(false); sp.Next(false); sp.Next(false); sp.Next(false); sp.Next(false); }

            EditorGUILayout.EndVertical();
            GUILayout.Space(6);


            FGUI_Inspector.FoldHeaderStart(ref _DrawRandomization, " Randomization Settings", FGUI_Resources.BGInBoxStyle, FGUI_Resources.Tex_Random, 19);

            if (_DrawRandomization)
            {
                GUILayout.Space(3);
                sp.Next(false); EditorGUILayout.PropertyField(sp);
                sp.Next(false); EditorGUILayout.PropertyField(sp);
                sp.Next(false); EditorGUILayout.PropertyField(sp);

                GUILayout.Space(2);

                if (WorldOffset.sqrMagnitude >= DirectionalOffset.sqrMagnitude)
                    EditorGUILayout.LabelField("World offset bigger - so random world offset", EditorStyles.centeredGreyMiniLabel);
                else
                    EditorGUILayout.LabelField("Directional offset bigger - so random directional offset", EditorStyles.centeredGreyMiniLabel);

                GUILayout.Space(3);
            }
            else
            {
                sp.Next(false); sp.Next(false); sp.Next(false);
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(6);


            FGUI_Inspector.FoldHeaderStart(ref _DrawAdditionals, " Additional Settings", FGUI_Resources.BGInBoxStyle, FGUI_Resources.Tex_Sliders, 19);

            if (_DrawAdditionals)
            {
                GUILayout.Space(3);
                sp.Next(false); EditorGUILayout.PropertyField(sp);
                sp.Next(false); EditorGUILayout.PropertyField(sp);

                sp.Next(false); EditorGUILayout.PropertyField(sp);
                sp.Next(false); EditorGUILayout.PropertyField(sp); /*sp.Next(false);*/
                sp.Next(false); if (!string.IsNullOrEmpty(GetFromTagged)) EditorGUILayout.PropertyField(sp); sp.Next(false);
                GUILayout.Space(3);
                sp.Next(false); EditorGUILayout.PropertyField(sp);
                GUILayout.Space(5);
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(3);

        }
#endif

        #endregion


        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);

            CellAllow = true;
            getted.Clear();

            if (string.IsNullOrEmpty(GetFromTagged))
            {
                getted.Add(spawn);
            }
            else
            {
                var gettedSpwn = CellSpawnsHaveSpecifics(cell, GetFromTagged, CheckMode, spawn, GetRandomIfMulti);

                if (FGenerators.CheckIfIsNull(gettedSpwn))
                {
                    if (DontSpawnIfNoTagged) if (string.IsNullOrEmpty(GetFromTagged) == false) CellAllow = false;
                    gettedSpwn = spawn;
                }
                else
                {
                    CellAllow = true;
                }

                // Getting just one spawn
                if (RunOnRepetition == false)
                {
                    if (FGenerators.CheckIfExist_NOTNULL(gettedSpwn))
                    {
                        spawn.TempRotationOffset = gettedSpwn.RotationOffset + gettedSpwn.OutsideRotationOffset + RotationEulerOffset;
                        spawn.TempPositionOffset = gettedSpwn.Offset + Quaternion.Euler(gettedSpwn.RotationOffset) * gettedSpwn.GetDirectionalOffsetWithMods();
                        //spawn.TempPositionOffset = GetUnitOffset(spawn.TempPositionOffset, OffsetMode, preset);
                        getted.Add(gettedSpwn);
                    }
                }
                else // Run on repetition
                {
                    if (FGenerators.CheckIfExist_NOTNULL(gettedSpwn))
                    {
                        spawn.TempRotationOffset = gettedSpwn.RotationOffset + gettedSpwn.OutsideRotationOffset + RotationEulerOffset;
                        spawn.TempPositionOffset = gettedSpwn.Offset + Quaternion.Euler(gettedSpwn.RotationOffset) * gettedSpwn.GetDirectionalOffsetWithMods();

                        AddTempData(gettedSpwn, null);
                        getted.Add(gettedSpwn);
                    }

                    var allSpawns = cell.CollectSpawns(OwnerSpawner.ScaleAccess);

                    for (int a = 0; a < allSpawns.Count; a++)
                    {
                        var aSpawn = allSpawns[a];
                        if (FGenerators.CheckIfIsNull(aSpawn)) continue;
                        if (aSpawn == gettedSpwn) continue;

                        if (SpawnHaveSpecifics(aSpawn, GetFromTagged, CheckMode))
                        {
                            CellAllow = true;
                            //aSpawn.TempRotationOffset = aSpawn.RotationOffset + aSpawn.OutsideRotationOffset + RotationEulerOffset;
                            //aSpawn.TempPositionOffset = aSpawn.Offset + Quaternion.Euler(aSpawn.RotationOffset) * aSpawn.GetDirectionalOffsetWithMods(); //aSpawn.TempPositionOffset = GetUnitOffset(spawn.TempPositionOffset, OffsetMode, preset);
                            getted.Add(aSpawn);
                            //AddTempData(aSpawn, null);
                        }
                    }

                }

            }

        }

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            Vector3 directionalOffset = GetUnitOffset(DirectionalOffset, OffsetMode, preset);
            Vector3 worldOffset = GetUnitOffset(WorldOffset, OffsetMode, preset);

            SpawnData spawnBackup = spawn.Copy();
            ApplySpawn(getted[0], spawn, worldOffset, directionalOffset );


            #region Hard coded use of spawn count rule and spawn propability on repetition

            /* Hard Coded Part - Needs to be solved better way */
            Count.SR_LimitSpawnCount spawnCountRule = null;
            for (int s = 0; s < OwnerSpawner.Rules.Count; s++) { if (OwnerSpawner.Rules[s] is Count.SR_LimitSpawnCount) { spawnCountRule = OwnerSpawner.Rules[s] as Count.SR_LimitSpawnCount; if (spawnCountRule.Enabled == false) { spawnCountRule = null; } else break; } }
            if ( spawnCountRule != null) if ( spawnCountRule.CellAllow == false)
                {
                    //spawnCountRule.CellAllow = true;
                    //spawn.DontSpawnMainPrefab = true;
                    return;
                }

            Count.SR_SpawningPropability spawnPropabilityRule = null;
            for (int s = 0; s < OwnerSpawner.Rules.Count; s++) { if (OwnerSpawner.Rules[s] is Count.SR_SpawningPropability) { spawnPropabilityRule = OwnerSpawner.Rules[s] as Count.SR_SpawningPropability; if (spawnPropabilityRule.Enabled == false) { spawnPropabilityRule = null; } else break; } }
            if (spawnPropabilityRule != null) if (spawnPropabilityRule.CellAllow == false)
                {

                    //spawnPropabilityRule.CellAllow = true;
                    //spawn.DontSpawnMainPrefab = true;
                    return;
                }

            /* Hard Coded Part End */


            if (RunOnRepetition)
            {
                for (int i = getted.Count - 1; i > 0; i--)
                {
                    /* Hard Coded Part - Needs to be solved better way */
                    var sp = getted[i];

                    if (spawnPropabilityRule != null)
                    {
                        float mul = spawnPropabilityRule.PropabilityMulVariable.GetValue(1f);

                        if (FGenerators.GetRandom(0f, 1f) > spawnPropabilityRule.Propability * mul)
                        {
                            getted.RemoveAt(i);
                            continue;
                        }
                    }

                    if (spawnCountRule != null)
                    {
                        spawnCountRule.CheckRuleOn(mod, ref sp, preset, cell, grid, restrictDirection);

                        if (spawnCountRule.CellAllow == false || spawnCountRule.created + 1 >= spawnCountRule.Count.Max)
                        {
                            getted.RemoveAt(i);
                            continue;
                        }

                        spawnCountRule.OnAddSpawnUsingRule(mod, sp, cell, grid);
                    }

                    /* Hard Coded Part End */
                }
            }

            #endregion


            for (int i = 1; i < getted.Count; i++)
            {
                SpawnData getSpawn = getted[i]; // CellSpawnsHaveTag(cell, GetFromTagged, spawn, GetRandomIfMulti);
                SpawnData thisSpawn = spawnBackup.Copy();

                thisSpawn.TempPositionOffset = getSpawn.RotationOffset + getSpawn.OutsideRotationOffset + RotationEulerOffset;
                thisSpawn.TempRotationOffset = getSpawn.Offset + Quaternion.Euler(getSpawn.RotationOffset) * getSpawn.GetDirectionalOffsetWithMods(); 
                
                cell.AddSpawnToCell(thisSpawn);
                ApplySpawn(getSpawn, thisSpawn, worldOffset, directionalOffset);
                AddTempData(thisSpawn, null);
            }

        }

        private void ApplySpawn(SpawnData getSpawn, SpawnData thisSpawn, Vector3 worldOffset, Vector3 directionalOffset)
        {

            if (getSpawn != null)
            {
                Vector3 getOff = getSpawn.Offset;
                Vector3 getDirOff = getSpawn.GetDirectionalOffsetWithMods();
                //if (getOff == Vector3.zero) getOff = getSpawn.TempPositionOffset;

                if (MultiplyGetted != Vector3.one) getOff = Vector3.Scale(getOff, MultiplyGetted);
                if (MultiplyGetted != Vector3.one) getDirOff = Vector3.Scale(getDirOff, MultiplyGetted);

                thisSpawn.Offset = getOff + worldOffset;

                if (StackOffset)
                {
                    thisSpawn.DirectionalOffset = getDirOff;
                    thisSpawn.Offset += Quaternion.Euler(thisSpawn.RotationOffset) * directionalOffset;
                }
                else
                    thisSpawn.DirectionalOffset = getDirOff + directionalOffset;

                thisSpawn.RotationOffset = getSpawn.RotationOffset + getSpawn.OutsideRotationOffset + RotationEulerOffset;

                if (GetScale)
                    thisSpawn.LocalScaleMul = Vector3.Scale(getSpawn.LocalScaleMul, ScaleMultiplier);
                else
                    thisSpawn.LocalScaleMul = ScaleMultiplier;
            }


            if (WorldOffset.sqrMagnitude >= DirectionalOffset.sqrMagnitude)
            {
                thisSpawn.Offset += new Vector3(
                    FGenerators.GetRandom(-RandomOffsets.x, RandomOffsets.x),
                    FGenerators.GetRandom(-RandomOffsets.y, RandomOffsets.y),
                    FGenerators.GetRandom(-RandomOffsets.z, RandomOffsets.z)
                    );
            }
            else
            {
                thisSpawn.DirectionalOffset += new Vector3(
                    FGenerators.GetRandom(-RandomOffsets.x, RandomOffsets.x),
                    FGenerators.GetRandom(-RandomOffsets.y, RandomOffsets.y),
                    FGenerators.GetRandom(-RandomOffsets.z, RandomOffsets.z)
                    );
            }


            thisSpawn.LocalRotationOffset += new Vector3(
                FGenerators.GetRandom(-RandomLocalRotation.x, RandomLocalRotation.x),
                FGenerators.GetRandom(-RandomLocalRotation.y, RandomLocalRotation.y),
                FGenerators.GetRandom(-RandomLocalRotation.z, RandomLocalRotation.z)
                );

            thisSpawn.LocalScaleMul += new Vector3(
                FGenerators.GetRandom(-RandomScale.x, RandomScale.x),
                FGenerators.GetRandom(-RandomScale.y, RandomScale.y),
                FGenerators.GetRandom(-RandomScale.z, RandomScale.z)
                );


            if (DirectionalOffset.sqrMagnitude > 0f)
                thisSpawn.DirectionalOffset += Vector3.Scale(PivotOffset, ScaleMultiplier);


            #region Directional helper modes

            if (HelperMode != ETranslateOnly.Default)
            {
                Vector3 translated = thisSpawn.GetRotationOffset() * thisSpawn.DirectionalOffset;
                if (HelperMode == ETranslateOnly.OnlyPositie)
                {
                    if (translated.x < 0f) thisSpawn.DirectionalOffset.x = 0f;
                    if (translated.y < 0f) thisSpawn.DirectionalOffset.y = 0f;
                    if (translated.z < 0f) thisSpawn.DirectionalOffset.z = 0f;
                }
                else if (HelperMode == ETranslateOnly.OnlyNegarive)
                {
                    if (translated.x > 0f) thisSpawn.DirectionalOffset.x = 0f;
                    if (translated.y > 0f) thisSpawn.DirectionalOffset.y = 0f;
                    if (translated.z > 0f) thisSpawn.DirectionalOffset.z = 0f;
                }
                else if (HelperMode == ETranslateOnly.AllPositive)
                {
                    if (translated.x < 0f) thisSpawn.DirectionalOffset.x = -thisSpawn.DirectionalOffset.x;
                    if (translated.y < 0f) thisSpawn.DirectionalOffset.y = -thisSpawn.DirectionalOffset.y;
                    if (translated.z < 0f) thisSpawn.DirectionalOffset.z = -thisSpawn.DirectionalOffset.z;
                }
                else if (HelperMode == ETranslateOnly.AllNegative)
                {
                    if (translated.x > 0f) thisSpawn.DirectionalOffset.x = -thisSpawn.DirectionalOffset.x;
                    if (translated.y > 0f) thisSpawn.DirectionalOffset.y = -thisSpawn.DirectionalOffset.y;
                    if (translated.z > 0f) thisSpawn.DirectionalOffset.z = -thisSpawn.DirectionalOffset.z;
                }
            }

            #endregion

        }


        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            if (getted != null)
            {
                if (RemoveTagged) 
                    if (string.IsNullOrEmpty(GetFromTagged) == false)
                    {
                        for (int i = 0; i < getted.Count; i++)
                        {
                            cell.GetSpawnsJustInsideCell().Remove(getted[i]);
                        }
                    }
            }
        }

    }
}