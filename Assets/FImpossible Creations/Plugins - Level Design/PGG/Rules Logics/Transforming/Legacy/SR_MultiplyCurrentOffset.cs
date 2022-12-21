//#if UNITY_EDITOR
//using UnityEditor;
//#endif
//using UnityEngine;

//namespace FIMSpace.Generating.Rules.Transforming.DirectOffset
//{
//    public class SR_MultiplyCurrentOffset : SpawnRuleBase, ISpawnProcedureType
//    {
//        public override string TitleName() { return "Multiply Direct Offset"; }
//        public override string Tooltip() { return "Multiplying current value of direct offset"; }
//        public EProcedureType Type => EProcedureType.Event;

//        public Vector3 Multiply = Vector3.one;

//        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenVertex> grid)
//        {
//            spawn.DirectionalOffset = Vector3.Scale(spawn.DirectionalOffset, Multiply);
//        }
//    }
//}