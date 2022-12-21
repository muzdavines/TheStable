#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;

namespace FIMSpace.Generating.Rules.Transforming.Utilities
{
    public class SR_PrefabOffset : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Prefab Offset"; }
        public override string Tooltip() { return "Offseting spawn with position/rotation setted inside prefab file"; }
        public EProcedureType Type { get { return EProcedureType.Event; } }

        public float Multiply = 1f;
        public ESP_OffsetSpace Space = ESP_OffsetSpace.LocalSpace;
        [Tooltip("Check 'Helper Pivot Correction' node, this toggle will use prefab offset in the same way as the node")]
        public bool UseAsHelperCorrection = false;

        #region Drawing Inspector Window

#if UNITY_EDITOR

        public override void NodeBody(SerializedObject so)
        {
            EditorGUILayout.HelpBox("Offsetting spawned object with position setted inside prefab file, can be used as custom pivot correction without need to changing model file/creating additional empty transforms", MessageType.None);
            base.NodeBody(so);
        }

#endif

        #endregion


        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            GameObject prefab = spawn.Prefab;
            if (preset == null) return;

            if (!UseAsHelperCorrection)
            {
                if (Space == ESP_OffsetSpace.WorldSpace)
                {
                    spawn.Offset += prefab.transform.position * Multiply;
                }
                else
                {
                    spawn.DirectionalOffset += prefab.transform.position * Multiply;
                }
            }
            else
            {
                if (Space == ESP_OffsetSpace.WorldSpace)
                {
                    spawn.OutsidePositionOffset = prefab.transform.position * Multiply;
                }
                else
                {
                    spawn.OutsidePositionOffset = prefab.transform.position * Multiply;
                }
            }
        }

    }
}