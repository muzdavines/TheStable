
//#if UNITY_EDITOR
//using UnityEditor;
//#endif
//using UnityEngine;

//namespace FIMSpace.Generating.Rules.Transforming
//{
//    public class SR_AlignWith : SpawnRuleBase, ISpawnProcedureType
//    {
//        public override string TitleName() { return "Align With"; }
//        public override string Tooltip() { return ""; }
//        public EProcedureType Type => EProcedureType.Event;

//        public string AlignWithTagged = "";
//        public bool GetRandomIfMulti = false;
//        [HideInInspector] public bool Offset = true;
//        [HideInInspector] public bool DirOffset = true;
//        [HideInInspector] public bool Rotation = false;
//        [HideInInspector] public bool Scale = false;

//#if UNITY_EDITOR
//        public override void NodeBody(SerializedObject so)
//        {
//            SerializedProperty p = so.FindProperty("AlignWithTagged");

//            EditorGUILayout.BeginHorizontal();
//            p.Next(false); EditorGUILayout.PropertyField(p);
//            p.Next(false); EditorGUILayout.PropertyField(p);
//            EditorGUILayout.EndHorizontal();

//            EditorGUILayout.BeginHorizontal();
//            p.Next(false); EditorGUILayout.PropertyField(p);
//            p.Next(false); EditorGUILayout.PropertyField(p);
//            EditorGUILayout.EndHorizontal();
//        }
//#endif

//        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenVertex> grid)
//        {
//            SpawnData getSpawn = CellSpawnsHaveTag(cell, AlignWithTagged, spawn, GetRandomIfMulti);

//            if (getSpawn != null)
//            {
//                if (Offset) spawn.Offset = getSpawn.Offset;
//                if (DirOffset) spawn.DirectionalOffset = getSpawn.DirectionalOffset;
//                if (Rotation) spawn.RotationOffset = getSpawn.RotationOffset;
//                if (Scale) spawn.LocalScaleMul = getSpawn.LocalScaleMul;
//            }
//        }
//    }
//}