#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace FIMSpace.Generating.Rules.Placement
{
    public class SR_IfRotated : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "If Rotated"; }
        public override string Tooltip() { return "Allowing to spawn when cell rotation is in desired range"; }

        public EProcedureType Type { get { return EProcedureType.Rule; } }
        public float RotationFrom = -90;
        [HideInInspector] public float RotationTo = 90;

        #region Editor Inspector Window

#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            base.NodeBody(so);

            if ( GUIIgnore.Count != 1) GUIIgnore.Add("RotationFrom");

            EditorGUILayout.MinMaxSlider("Rotation From To:", ref RotationFrom, ref RotationTo, -180f, 180f);
            EditorGUILayout.LabelField("From " + Mathf.Round(RotationFrom) + "°  to " + Mathf.Round(RotationTo) + "°", EditorStyles.centeredGreyMiniLabel);

            so.ApplyModifiedProperties();
        }
#endif

        #endregion

        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);

            float diff = FEngineering.WrapAngle(spawn.GetFullRotationOffset().y);
            if (diff >= RotationFrom && diff <= RotationTo) CellAllow = true;
        }
    }
}