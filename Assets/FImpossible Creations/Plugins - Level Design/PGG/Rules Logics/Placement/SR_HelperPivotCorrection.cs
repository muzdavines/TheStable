#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace FIMSpace.Generating.Rules.Placement
{
    public class SR_HelperPivotCorrection : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Helper Pivot Correction"; }
        public override string Tooltip() { return "If you want to change coordinates for other objects which you want align with this model.\nUseful when your walls have pivot coordinates in corners!\n" + base.Tooltip(); }
        public override bool CanBeNegated() { return false; }

        public EProcedureType Type { get { return EProcedureType.Procedure; } }

        [PGG_SingleLineSwitch("OffsetMode", 58, "Select if you want to offset postion with cell size or world units", 140)]
        public Vector3 PositionCorrection = Vector3.zero;
        [HideInInspector] public ESR_Measuring OffsetMode = ESR_Measuring.Cells;
       
        public Vector3 RotationCorrection = Vector3.zero;
        
        [Space(4)]
        public bool Debug = false;


        #region Editor Inspector Window

#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            EditorGUILayout.HelpBox("Useful when your walls have pivot coordinates in corners.\nThis node will change coordinates just for other objects trying to get coordinates of this object, but not changing any coordinates for this object on the scene.", MessageType.None);
            GUILayout.Space(4);
            base.NodeBody(so);
        }
#endif

        #endregion


        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            _EditorDebug = Debug;
            CellAllow = true;
            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);
            Vector3 offset = GetUnitOffset(PositionCorrection, OffsetMode, preset);
            spawn.OutsidePositionOffset = offset;
            spawn.OutsideRotationOffset = RotationCorrection;

            if (spawn.ChildSpawns != null)
                for (int s = 0; s < spawn.ChildSpawns.Count; s++)
                {
                    spawn.ChildSpawns[s].OutsidePositionOffset = offset; 
                    spawn.ChildSpawns[s].OutsideRotationOffset = RotationCorrection;
                }
        }

#if UNITY_EDITOR
        public override void OnDrawDebugGizmos(FieldSetup preset, SpawnData spawn, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            base.OnDrawDebugGizmos(preset, spawn, cell, grid);

            Color c = Gizmos.color;
            Gizmos.color = new Color(0.1f, 0.9f, 0.15f, 0.8f);

            Vector3 size = preset.GetCellUnitSize();

            Vector3 pos = cell.WorldPos(preset);
            Quaternion rot = Quaternion.Euler(spawn.GetFullRotationOffset() );
          
            Vector3 nodeOffset = GetUnitOffset(PositionCorrection, OffsetMode, preset);
            Vector3 ppos = spawn.GetWorldPositionWithFullOffset() + rot * nodeOffset;

            Gizmos.DrawLine(pos, ppos);
            Gizmos.DrawWireSphere(ppos, size.x * 0.05f);

            Gizmos.color = new Color(0f, 0f, 1f, 0.9f);
            Gizmos.DrawRay(ppos + new Vector3(0f, size.x * 0.1f), Quaternion.Euler(RotationCorrection+ spawn.GetFullRotationOffset()) * Vector3.forward * size.x * 0.5f);

            Gizmos.color = c;
        }
#endif

    }
}