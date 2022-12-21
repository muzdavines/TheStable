//using UnityEngine;

//namespace FIMSpace.Generating.Rules.FieldAndGrid
//{
//    public class SR_HeightOffset : SpawnRuleBase, ISpawnProcedureType
//    {
//        public override string TitleName() { return "Preset Offsets"; }
//        public override string Tooltip() { return "Using FieldSetup parameters to change object shape or position"; }

//        public EProcedureType Type => EProcedureType.Event;
//        public float MultiplyOffset = 1f;
//        public float MultiplyScale = 0f;
//        public float MultiplyYOffset = 0f;
//        public float MultiplyYOffsetScale = 1f;


//        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenVertex> grid)
//        {
//            spawn.Offset += Vector3.up * (preset.HeightOffset * (MultiplyOffset * (MultiplyYOffset + preset.YScaleOffset * MultiplyYOffsetScale)));
//            if (MultiplyScale > 0f) if (MultiplyScale > 0f) spawn.LocalScaleMul += Vector3.up * MultiplyScale * preset.YScaleOffset * MultiplyOffset;
//            //if (MultiplyYOffset > 0f) spawn.LocalScaleMul += Vector3.up * preset.YScaleOffset * MultiplyYOffset;
//            //UnityEngine.Debug.Log("Offsetting by " + preset.HeightOffset * MultiplyOffset);
//        }
//    }
//}