#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Transforming.Utilities
{
    public class SR_ConditionalPushPosition : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Conditional Push Position"; }
        public override string Tooltip() { return "Offsetting spawn position like DirectOffset and with different values if spawn is rotated"; }
        public EProcedureType Type { get { return EProcedureType.Event; } }

        [PGG_SingleLineTwoProperties("ApplyMode",0, 76, 10)]
        [Tooltip("Using units or cell size (multiplied units) for translation measurement")]
        public ESR_Measuring OffsetMode = ESR_Measuring.Units;
        [Tooltip("Add this offset to current spawn position offset or override previous applied translations with this values")]
        [HideInInspector] public ESP_OffsetMode ApplyMode = ESP_OffsetMode.AdditiveOffset;
        [Tooltip("Offset for being applied when spawn rotation is between -45 and 45 degrees")]
        public Vector3 OffsetOnZero = new Vector3(0f, 0f, 0.5f);
        [Tooltip("Offset for being applied when spawn rotation is between 45 and 135 degrees")]
        public Vector3 OffsetOn90 = Vector3.zero;
        [Tooltip("Offset for being applied when spawn rotation is between 135 and 225 degrees")]
        public Vector3 OffsetOn180 = Vector3.zero;
        [Tooltip("Offset for being applied when spawn rotation is between 225 and 315 degrees")]
        public Vector3 OffsetOn270 = Vector3.zero;

#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            EditorGUILayout.HelpBox("Some of rotations can be not used by node, 'GetCoordinates' can not work in every case", MessageType.None);
            base.NodeBody(so);
        }
#endif

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            Vector3 offset;

            float yRotOff = spawn.RotationOffset.y;
            if (yRotOff == 0f) yRotOff = spawn.TempRotationOffset.y;

            if (yRotOff >= -45 && yRotOff < 45)
                offset = spawn.GetRotationOffset() * OffsetOnZero;
            else
            if (yRotOff >= 45 && yRotOff < 135)
                offset = spawn.GetRotationOffset() * OffsetOn90;
            else
            if (yRotOff >= 135 && yRotOff < 225)
                offset = spawn.GetRotationOffset() * OffsetOn180;
            else
            if (yRotOff >= 225 && yRotOff < 315)
                offset = spawn.GetRotationOffset() * OffsetOn270;
            else
                offset = spawn.GetRotationOffset() * OffsetOnZero;

            offset = GetUnitOffset(offset, OffsetMode, preset);

            if (ApplyMode == ESP_OffsetMode.AdditiveOffset)
            {
                spawn.Offset += offset;
                spawn.TempPositionOffset += offset;
            }
            else
            {
                spawn.Offset = offset;
                spawn.TempPositionOffset = offset;
            }

        }
    }
}