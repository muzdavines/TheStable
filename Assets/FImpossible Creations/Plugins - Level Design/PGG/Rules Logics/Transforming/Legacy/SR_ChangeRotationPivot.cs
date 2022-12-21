using UnityEngine;

namespace FIMSpace.Generating.Rules.Transforming.Legacy
{
    public class SR_ChangeRotationPivot : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Change Rotation Pivot"; }
        public override string Tooltip() { return "Getting final rotation and offsetting model position to be positioned like it's rotated around different pivot/origin point"; }

        public EProcedureType Type { get { return EProcedureType.OnConditionsMet; } }

        public Vector3 PivotOffset = Vector3.zero;
        public ESR_Measuring OffsetMode = ESR_Measuring.Units;

        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            Vector3 pivotOffsetted = GetUnitOffset(PivotOffset, OffsetMode, preset);
            Vector3 finalEulerRot = spawn.GetFullRotationOffset();
            Quaternion finalRot = Quaternion.Euler(finalEulerRot);
            Matrix4x4 mx = Matrix4x4.TRS(pivotOffsetted, finalRot, Vector3.one);
            spawn.Offset += mx.MultiplyVector(pivotOffsetted); 
        }

    }
}