using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Rules.Transforming.Noise
{
    public class SR_PerlinNoiseRotationOrScale : SpawnRuleBase, ISpawnProcedureType
    {
        // Base parameters implementation
        public override string TitleName() { return "Perlin Noise Rotation-Scale"; }
        public override string Tooltip() { return "Transforming rotation or scale of the spawned object with perlin noise offset, can provide landscape effect"; }

        // Define what your script will do
        public EProcedureType Type { get { return EProcedureType.Event; } }

        [PGG_SingleLineSwitch("EffectOn", 78, "", 140)]
        public Vector3 Offset = new Vector3(0f,1f,0f);
        [HideInInspector] public ESR_RotationOrScale EffectOn = ESR_RotationOrScale.Scale;
        public ESP_OffsetMode ApplyMode = ESP_OffsetMode.OverrideOffset; 

        [Space(6)]
        public Vector2 PerlinNoiseScale = new Vector2(16, 16);
        public Vector2 OffsetNoise = Vector2.zero;

        [HideInInspector] public Vector2 OptionalClamp = new Vector2(0f, 1f);

        #region There you can do custom modifications for inspector view
#if UNITY_EDITOR
        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            EditorGUILayout.MinMaxSlider("Optional Clamp", ref OptionalClamp.x, ref OptionalClamp.y, 0f, 1f);
            base.NodeFooter(so, mod);
        }
#endif
        #endregion


        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            Vector3 tgtOffset = Offset;

            float xNoise = Mathf.PerlinNoise(cell.PosXZ.x * PerlinNoiseScale.x * 0.01f + OffsetNoise.x, cell.PosXZ.y * PerlinNoiseScale.y * 0.01f + +OffsetNoise.y);

            if (xNoise < OptionalClamp.x) xNoise = OptionalClamp.x;
            if (xNoise > OptionalClamp.y) xNoise = OptionalClamp.y;

            tgtOffset.x *= xNoise;
            tgtOffset.y *= xNoise;
            tgtOffset.z *= xNoise;

            if (ApplyMode == ESP_OffsetMode.OverrideOffset)
            {
                if (EffectOn == ESR_RotationOrScale.Rotation) spawn.RotationOffset = tgtOffset;
                else spawn.LocalScaleMul = Vector3.one + tgtOffset;
            }
            else
            {
                if (EffectOn == ESR_RotationOrScale.Rotation) spawn.RotationOffset += tgtOffset;
                else spawn.LocalScaleMul += tgtOffset;
            }
        }

    }
}