using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Rules.Transforming
{
    public class SR_GetPosRotScale : SpawnRuleBase, ISpawnProcedureType
    {
        // Base parameters implementation
        public override string TitleName() { return "Get Position-Rotation-Scale"; }
        public override string Tooltip() { return "Gathering position/rotation/scale out of choosed already spawned object in cell"; }

        public EProcedureType Type { get { return EProcedureType.Event; } }

        [PGG_SingleLineTwoProperties("GatherBy", 32, 62, 16, 80)]
        public ESR_Transforming Get = ESR_Transforming.Position;
        [PGG_SingleLineSwitch("CheckMode", 68, "Select if you want to use Tags, SpawnStigma or CellData", 80)]
        [HideInInspector] public string GatherBy = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        [Space(5)]
        [PGG_SingleLineTwoProperties("GetMode", 50, 70)]
        public ESP_OffsetSpace Space = ESP_OffsetSpace.LocalSpace;
        [Tooltip("Get just local/world position offset out of other spawn or sum all space into one")]
        [HideInInspector] public ESP_GetMode GetMode = ESP_GetMode.Sum;
        [Tooltip("You can add offset of other object to current one, but overriding by default")]
        public ESP_OffsetMode ApplyMode = ESP_OffsetMode.OverrideOffset;

        public Vector3Int GetFromOffsetCell = Vector3Int.zero;

        #region There you can do custom modifications for inspector view
#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            if (Get == ESR_Transforming.Scale)
            {
                if (GUIIgnore.Count != 2)
                {
                    GUIIgnore.Clear();
                    GUIIgnore.Add("Space");
                    GUIIgnore.Add("ApplyMode");
                }
            }
            else
                GUIIgnore.Clear();
        }
#endif
        #endregion


        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            #region Get Coordinates Features if used with "GetCoords"

            if (string.IsNullOrEmpty(GatherBy) == false)
            {
                var targetCell = cell;
                if (GetFromOffsetCell != Vector3Int.zero) targetCell = grid.GetCell(cell.Pos + GetFromOffsetCell);

                SpawnData sp = CellSpawnsHaveSpecifics(targetCell, GatherBy, CheckMode, spawn);
                if (FGenerators.CheckIfExist_NOTNULL(sp))
                {
                    if (Get == ESR_Transforming.Position)
                    {
                        if (GetMode == ESP_GetMode.Separate)
                        {
                            if (ApplyMode == ESP_OffsetMode.OverrideOffset)
                            {
                                if (Space == ESP_OffsetSpace.WorldSpace)
                                    spawn.Offset = sp.Offset/* + sp.GetOutsideDirectionalOffsetValue()*/;
                                else
                                    spawn.DirectionalOffset = sp.GetDirectionalOffsetWithMods();
                            }
                            else
                            {
                                if (Space == ESP_OffsetSpace.WorldSpace)
                                    spawn.Offset += sp.Offset/* + sp.GetOutsideDirectionalOffsetValue()*/;
                                else
                                    spawn.DirectionalOffset += sp.GetDirectionalOffsetWithMods();
                            }
                        }
                        else if (GetMode == ESP_GetMode.Sum)
                        {
                            if (ApplyMode == ESP_OffsetMode.OverrideOffset)
                            {
                                if (Space == ESP_OffsetSpace.WorldSpace)
                                    spawn.Offset = sp.GetFullOffset() + sp.GetOutsideDirectionalOffsetValue();
                                else
                                    spawn.DirectionalOffset = sp.GetDirectionalOffsetWithMods() + Quaternion.Inverse(Quaternion.Euler(sp.RotationOffset)) * sp.Offset;
                            }
                            else
                            {
                                if (Space == ESP_OffsetSpace.WorldSpace)
                                    spawn.Offset += sp.GetFullOffset() + sp.GetOutsideDirectionalOffsetValue();
                                else
                                    spawn.DirectionalOffset += sp.GetDirectionalOffsetWithMods() + Quaternion.Inverse(Quaternion.Euler(sp.RotationOffset)) * sp.Offset;
                            }
                        }
                    }
                    else if (Get == ESR_Transforming.Rotation)
                    {
                        if (GetMode == ESP_GetMode.Separate)
                        {
                            if (ApplyMode == ESP_OffsetMode.OverrideOffset)
                            {
                                if (Space == ESP_OffsetSpace.WorldSpace)
                                    spawn.RotationOffset = sp.RotationOffset + sp.OutsideRotationOffset;
                                else
                                    spawn.LocalRotationOffset = sp.LocalRotationOffset + sp.OutsideRotationOffset;
                            }
                            else
                            {
                                if (Space == ESP_OffsetSpace.WorldSpace)
                                    spawn.RotationOffset += sp.RotationOffset + sp.OutsideRotationOffset;
                                else
                                    spawn.LocalRotationOffset += sp.LocalRotationOffset + sp.OutsideRotationOffset;
                            }
                        }
                        else if (GetMode == ESP_GetMode.Sum)
                        {
                            if (ApplyMode == ESP_OffsetMode.OverrideOffset)
                            {
                                if (Space == ESP_OffsetSpace.WorldSpace)
                                    spawn.RotationOffset = sp.RotationOffset + sp.OutsideRotationOffset + sp.LocalRotationOffset;
                                else
                                    spawn.LocalRotationOffset = sp.LocalRotationOffset + sp.OutsideRotationOffset + sp.RotationOffset;
                            }
                            else
                            {
                                if (Space == ESP_OffsetSpace.WorldSpace)
                                    spawn.RotationOffset += sp.RotationOffset + sp.OutsideRotationOffset + sp.LocalRotationOffset;
                                else
                                    spawn.LocalRotationOffset += sp.LocalRotationOffset + sp.OutsideRotationOffset + sp.RotationOffset;
                            }
                        }
                    }
                    else if (Get == ESR_Transforming.Scale)
                    {
                        spawn.LocalScaleMul = sp.LocalScaleMul;
                    }
                }
            }

            #endregion
        }

    }
}