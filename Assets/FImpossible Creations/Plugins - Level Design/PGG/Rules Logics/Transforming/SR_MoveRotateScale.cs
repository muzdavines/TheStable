using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Rules.Transforming
{
    public partial class SR_MoveRotateScale : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Move-Rotate-Scale"; }
        public override string Tooltip() { return "Applying basic position / rotation or scale offset to spawned object"; }

        public EProcedureType Type { get { return EProcedureType.Event; } }

        [Space(4)]
        [PGG_SingleLineTwoProperties("ApplyMode", 76, 80, 14, 20)]
        public ESR_Transforming EffectOn = ESR_Transforming.Position;
        [HideInInspector] public ESP_OffsetMode ApplyMode = ESP_OffsetMode.OverrideOffset;

        [Space(4)]
        [PGG_SingleLineTwoProperties("GetCoords", 52, 84, 16, 50)]
        [Tooltip("Local space will offset object with ingerention of current rotation of the spawn")]
        public ESP_OffsetSpace Space = ESP_OffsetSpace.WorldSpace;
        [PGG_SingleLineSwitch("CheckMode", 70, "Select if you want to use Tags, SpawnStigma or CellData", 84)]
        [Tooltip("Optional feature to gather coordinates from object which is already on the same cell.\nIt sums local and world position by default, if you need to sepaerate this use 'Get Position-Rotate-Scale' node.\nGathered coords are applied as override for current coords as start point for the rest of the offset logics.")]
        [HideInInspector] public string GetCoords = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        [Space(4)]

        [PGG_SingleLineSwitch("OffsetMode", 58, "Select if you want to offset postion with cell size or world units", 140, 4)]
        public Vector3 PositionOffset = new Vector3(0f, 0f, 0f);
        [HideInInspector] public ESR_Measuring OffsetMode = ESR_Measuring.Cells;
        public SpawnerVariableHelper OffsetPosVariable = new SpawnerVariableHelper(FieldVariable.EVarType.Vector3);

        [Space(4)]
        public Vector3 RotationOffset = new Vector3(0f, 90f, 0f);
        public Vector3 ScaleMultiplier = new Vector3(1f, 1f, 1f);

        [Space(4)]
        [Tooltip("When randomize offset is 0,0,0 then it's not used")]
        public Vector3 RandomizeOffset = Vector3.zero;
        [Space(4)]
        public Vector3 MaxDegreesSteps = Vector3.one;

        [HideInInspector] public bool InheritScaleOffset = false;

        #region There you can do custom modifications for inspector view
#if UNITY_EDITOR

        UnityEngine.Texture _Tex_Scale = null;
        public override void NodeBody(SerializedObject so)
        {
            if (EffectOn == ESR_Transforming.Position)
            {
                if (GUIIgnore.Count != 3) { GUIIgnore.Clear(); GUIIgnore.Add(""); GUIIgnore.Add(""); GUIIgnore.Add(""); }
                GUIIgnore[0] = "RotationOffset";
                GUIIgnore[1] = "MaxDegreesSteps";
                GUIIgnore[2] = "ScaleMultiplier";
            }
            else if (EffectOn == ESR_Transforming.Rotation)
            {
                if (GUIIgnore.Count != 3) { GUIIgnore.Clear(); GUIIgnore.Add(""); GUIIgnore.Add(""); GUIIgnore.Add(""); }
                GUIIgnore[0] = "PositionOffset";
                GUIIgnore[1] = "ScaleMultiplier";
                GUIIgnore[2] = "OffsetPosVariable";
            }
            else if (EffectOn == ESR_Transforming.Scale)
            {
                if (GUIIgnore.Count != 5) { GUIIgnore.Clear(); GUIIgnore.Add(""); GUIIgnore.Add(""); GUIIgnore.Add(""); GUIIgnore.Add(""); GUIIgnore.Add(""); }
                GUIIgnore[0] = "PositionOffset";
                GUIIgnore[1] = "RotationOffset";
                GUIIgnore[2] = "Space";
                GUIIgnore[3] = "MaxDegreesSteps";
                GUIIgnore[4] = "OffsetPosVariable";
            }

            OffsetPosVariable.Tooltip = "Set Additive Vector3 Field Variable value to this property";
        }

        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {

            if (EffectOn == ESR_Transforming.Position)
                if (Space == ESP_OffsetSpace.LocalSpace)
                {
                    EditorGUIUtility.labelWidth = 220;
                    if (_Tex_Scale == null) _Tex_Scale = EditorGUIUtility.IconContent("ScaleTool").image;
                    EditorGUILayout.PropertyField(so.FindProperty("InheritScaleOffset"), new GUIContent("  Inherit spawn scale for offset", _Tex_Scale));
                    EditorGUIUtility.labelWidth = 0;
                }

            base.NodeFooter(so, mod);
        }
#endif
        #endregion


        public override System.Collections.Generic.List<SpawnerVariableHelper> GetVariables() { return OffsetPosVariable.GetListedVariable(); }

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {

            #region Get Coordinates Features if used with "GetCoords"

            if (string.IsNullOrEmpty(GetCoords) == false)
            {
                SpawnData sp = CellSpawnsHaveSpecifics(cell, GetCoords, CheckMode, spawn);
                if (FGenerators.CheckIfExist_NOTNULL(sp))
                {
                    if (EffectOn == ESR_Transforming.Position)
                    {
                        if (Space == ESP_OffsetSpace.WorldSpace)
                            spawn.Offset = sp.GetFullOffset() + sp.GetOutsideDirectionalOffsetValue();
                        else
                            spawn.DirectionalOffset = sp.GetDirectionalOffsetWithMods() + Quaternion.Inverse(Quaternion.Euler(sp.RotationOffset)) * sp.Offset;
                    }
                    else if (EffectOn == ESR_Transforming.Rotation)
                    {
                        if (Space == ESP_OffsetSpace.WorldSpace)
                            spawn.RotationOffset = sp.RotationOffset + sp.OutsideRotationOffset + sp.LocalRotationOffset;
                        else
                            spawn.LocalRotationOffset = sp.LocalRotationOffset + sp.OutsideRotationOffset + sp.RotationOffset;
                    }
                    else if (EffectOn == ESR_Transforming.Scale)
                    {
                        spawn.LocalScaleMul = sp.LocalScaleMul;
                    }
                }
            }

            #endregion



            #region Random offset prepare

            Vector3 randomOffset = Vector3.zero;
            if (RandomizeOffset != Vector3.zero)
            {
                randomOffset = new Vector3
                (
                FGenerators.GetRandom(-RandomizeOffset.x, RandomizeOffset.x),
                FGenerators.GetRandom(-RandomizeOffset.y, RandomizeOffset.y),
                FGenerators.GetRandom(-RandomizeOffset.z, RandomizeOffset.z)
                );
            }

            #endregion


            if (EffectOn == ESR_Transforming.Position)
            {
                Vector3 off = PositionOffset + OffsetPosVariable.GetVector3(Vector3.zero);
                Vector3 tgtOffset = GetUnitOffset(off + randomOffset, OffsetMode, preset);

                if (ApplyMode == ESP_OffsetMode.AdditiveOffset)
                {
                    if (Space == ESP_OffsetSpace.LocalSpace)
                        spawn.DirectionalOffset += InheritScaleOffset ? Vector3.Scale(tgtOffset, spawn.TempScaleMul) : tgtOffset;
                    else
                        spawn.Offset += tgtOffset;
                }
                else if (ApplyMode == ESP_OffsetMode.OverrideOffset)
                {
                    if (Space == ESP_OffsetSpace.LocalSpace)
                        spawn.DirectionalOffset = InheritScaleOffset ? Vector3.Scale(tgtOffset, spawn.TempScaleMul) : tgtOffset;
                    else
                        spawn.Offset = tgtOffset;
                }
            }
            else if (EffectOn == ESR_Transforming.Rotation)
            {
                Vector3 tgtOffset = RotationOffset;

                if (MaxDegreesSteps.x != 0f) randomOffset.x = OStamperSet.GetAngleFor(MaxDegreesSteps.x, 1f, randomOffset.x / MaxDegreesSteps.x);
                if (MaxDegreesSteps.y != 0f) randomOffset.y = OStamperSet.GetAngleFor(MaxDegreesSteps.y, 1f, randomOffset.y / MaxDegreesSteps.y);
                if (MaxDegreesSteps.z != 0f) randomOffset.z = OStamperSet.GetAngleFor(MaxDegreesSteps.z, 1f, randomOffset.z / MaxDegreesSteps.z);
                tgtOffset += randomOffset;

                if (ApplyMode == ESP_OffsetMode.AdditiveOffset)
                {
                    if (Space == ESP_OffsetSpace.LocalSpace)
                        spawn.LocalRotationOffset += tgtOffset;
                    else
                        spawn.RotationOffset += tgtOffset;
                }
                else if (ApplyMode == ESP_OffsetMode.OverrideOffset)
                {
                    if (Space == ESP_OffsetSpace.LocalSpace)
                        spawn.LocalRotationOffset = tgtOffset;
                    else
                        spawn.RotationOffset = tgtOffset;
                }

            }
            else if (EffectOn == ESR_Transforming.Scale)
            {
                spawn.LocalScaleMul = Vector3.Scale(ScaleMultiplier, (Vector3.one + randomOffset));
            }

        }

    }
}