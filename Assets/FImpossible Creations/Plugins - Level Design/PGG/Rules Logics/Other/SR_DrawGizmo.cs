#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Other
{
    public class SR_DrawGizmo : SpawnRuleBase, ISpawnProcedureType
    {
        public EProcedureType Type { get { return EProcedureType.Event; } }

        public override string TitleName() { return "Draw Gizmo"; }
        public override string Tooltip() { return "Just drawing sphere gizmo to display current position of spawn"; }

        public float Radius = 1f;
        public Color color = new Color(0.1f, 1f, 0.1f, 0.7f);

#if UNITY_EDITOR

        public override void NodeBody(SerializedObject so)
        {
            EditorGUILayout.HelpBox("Gizmo will draw if spawner conditions are met", MessageType.None);
            base.NodeBody(so);
        }

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            _EditorDebug = true;
        }

        public override void OnDrawDebugGizmos(FieldSetup preset, SpawnData spawn, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            base.OnDrawDebugGizmos(preset, spawn, cell, grid);

            Gizmos.color = color;
            Gizmos.DrawWireSphere(spawn.GetWorldPositionWithFullOffset(preset, true), Radius);
            Gizmos.color = _DbPreCol;
        }
#endif

    }
}