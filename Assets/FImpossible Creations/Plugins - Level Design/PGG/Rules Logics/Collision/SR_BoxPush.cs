#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

//namespace FIMSpace.Generating.Rules.Collision
//{
//    public class SR_BoxPush : SpawnRuleBase, ISpawnProcedureType
//    {
//        public override string TitleName() { return "Box Push"; }
//        public override string Tooltip() { return ""; }
//        public EProcedureType Type => EProcedureType.Event;

//        public float BoxScale = 0.2f;
//        public Vector3 Direction = Vector3.forward;
//        public string GetDirectionFromTagged = "";

//        [Space(6)]
//        public bool Debug = false;

//#if UNITY_EDITOR
//        public override void NodeFooter(SerializedObject so, FieldModification mod)
//        {
//            base.NodeFooter(so, mod);
//        }
//#endif

//        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenVertex> grid)
//        {
//            if (Debug) _EditorDebug = true;
//        }

//        public override void OnDrawDebugGizmos(SpawnData spawn, FieldCell cell, FGenGraph<FieldCell, FGenVertex> grid)
//        {
//            base.OnDrawDebugGizmos(spawn, cell, grid);

//            Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.6f);
//            Gizmos.DrawCube(cell.WorldPos(), Vector3.one * BoxScale);
//            Gizmos.DrawWireCube(cell.WorldPos(), Vector3.one * BoxScale);


//            Gizmos.color = _DbPreCol;
//        }

//    }
//}