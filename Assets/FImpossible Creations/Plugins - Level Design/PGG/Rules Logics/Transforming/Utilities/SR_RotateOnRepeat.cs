using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Rules.Transforming.Utilities
{
    public class SR_RotateOnRepeat : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Rotate On Repeat"; }
        public override string Tooltip() { return "If you're using 'Repeat' feature for spawner (can be switched when toggling switch on the right of 'Cell Check Mode' field inside inspector window) "; }

        public EProcedureType Type { get { return EProcedureType.Event; } }

        [PGG_SingleLineTwoProperties("Apply", 90, 55)]
        public ESP_OffsetSpace OffsetSpace = ESP_OffsetSpace.WorldSpace;
        [HideInInspector] public ESP_OffsetMode Apply = ESP_OffsetMode.OverrideOffset;
        public Vector3 AnglePerRepeat = new Vector3(0, 90f, 0);


        #region There you can do custom modifications for inspector view
#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            EditorGUILayout.HelpBox("Switch toggle on the right to 'Cell Check Mode' to display 'Repeat' options", MessageType.None);
            base.NodeBody(so);
        }
#endif
        #endregion

        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (Apply == ESP_OffsetMode.AdditiveOffset)
                spawn.TempRotationOffset += AnglePerRepeat * OwnerSpawner._currentRepeat;
            else
                spawn.TempRotationOffset = AnglePerRepeat * OwnerSpawner._currentRepeat;

            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);
        }

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (OffsetSpace == ESP_OffsetSpace.WorldSpace)
            {
                //spawn.TempRotationOffset += AnglePerRepeat * OwnerSpawner._currentRepeat;
                if (Apply == ESP_OffsetMode.AdditiveOffset)
                    spawn.RotationOffset += AnglePerRepeat * OwnerSpawner._currentRepeat;
                else
                    spawn.RotationOffset = AnglePerRepeat * OwnerSpawner._currentRepeat;
            }
            else
            {
                //spawn.TempRotationOffset += AnglePerRepeat * OwnerSpawner._currentRepeat;
                if (Apply == ESP_OffsetMode.AdditiveOffset)
                    spawn.LocalRotationOffset += AnglePerRepeat * OwnerSpawner._currentRepeat;
                else
                    spawn.LocalRotationOffset = AnglePerRepeat * OwnerSpawner._currentRepeat;

            }
        }


    }
}