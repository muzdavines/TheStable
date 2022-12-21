#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Transforming.Legacy
{
    public class SR_GetRestrictedRotation : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Get Restricted Rotation"; }
        public override string Tooltip() { return "Getting rotation out of command placed in cell directly for call (only if using 'Post Run Modificator' or 'Pre Run Modificator')"; }
        public EProcedureType Type { get { return EProcedureType.Event; } }

        [Tooltip("Just for quick tweak rotation angle if needed")]
        public Vector3 AdditionalAngleOffset = Vector3.zero;

        private Vector3? restrict = null; 
        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            restrict = restrictDirection;
            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);
        }


#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            UnityEditor.EditorGUILayout.HelpBox("Only if using 'Post Run Modificator' or 'Pre Run Modificator'", MessageType.None);
            GUILayout.Space(4);
            base.NodeBody(so);
        }
#endif


        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if ( restrict != null)
            {
                if ( restrict.Value != Vector3.zero)
                {
                    Vector3 angles = AdditionalAngleOffset;
                    angles += Quaternion.LookRotation(restrict.Value).eulerAngles;
                    spawn.RotationOffset = angles;
                    return;
                }
            }
        }
    }
}