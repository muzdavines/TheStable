using UnityEngine;

namespace FIMSpace.Generating.NotUsed
{
    //public class SR_TransformOffsets : SpawnRuleBase, ISpawnProcedureType
    //{
    //    public override string TitleName() { return "Offset Coordinates"; }
    //    public override string Tooltip() { return ""; }

    //    public EProcedureType Type => EProcedureType.Event;
    //    //public enum ETargetSpawnData { Last, First, All }
    //    //public ETargetSpawnData TargetSpawn = ETargetSpawnData.Last;
    //    public FieldModification OnlyOn;
    //    public bool Override = false;

    //    [Space(5)]
    //    public Vector3 WorldOffset = Vector3.zero;
    //    public Vector3 DirectionalOffset = Vector3.zero;
    //    [Space(5)]
    //    public Vector3 RotationEulerOffset = Vector3.zero;
    //    [Space(5)]
    //    public Vector3 ScaleMultiplier = Vector3.one;
    //    [Space(5)]
    //    public Vector3 RandomRotation = Vector3.zero;
    //    public Vector3 RandomOffsets = Vector3.zero;

    //    public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenVertex> grid)
    //    {
    //        if (OnlyOn != null) if (spawn.OwnerMod != OnlyOn) return;

    //        if (Override)
    //        {
    //            spawn.Offset = WorldOffset;
    //            spawn.DirectionalOffset = DirectionalOffset;
    //            spawn.RotationOffset = RotationEulerOffset;
    //            spawn.LocalScaleMul = ScaleMultiplier;
    //        }
    //        else
    //        {
    //            spawn.Offset += WorldOffset;
    //            spawn.DirectionalOffset += DirectionalOffset;
    //            spawn.RotationOffset += RotationEulerOffset;
    //            spawn.LocalScaleMul = Vector3.Scale(spawn.LocalScaleMul, ScaleMultiplier);
    //        }


    //        if (RandomOffsets.sqrMagnitude > 0)
    //        {
    //            spawn.Offset += new Vector3(
    //                FGenerators.GetRandom(-RandomOffsets.x, RandomOffsets.x),
    //                FGenerators.GetRandom(-RandomOffsets.y, RandomOffsets.y),
    //                FGenerators.GetRandom(-RandomOffsets.z, RandomOffsets.z)
    //                );
    //        }

    //        if (RandomRotation.sqrMagnitude > 0)
    //        {
    //            spawn.RotationOffset += new Vector3(
    //                FGenerators.GetRandom(-RandomRotation.x, RandomRotation.x),
    //                FGenerators.GetRandom(-RandomRotation.y, RandomRotation.y),
    //                FGenerators.GetRandom(-RandomRotation.z, RandomRotation.z)
    //                );
    //        }

    //    }
    //}
}