using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Rules.Transforming.Noise
{
    public class SR_PerlinNoiseOffset : SpawnRuleBase, ISpawnProcedureType
    {
        // Base parameters implementation
        public override string TitleName() { return "Perlin Noise Offset"; }
        public override string Tooltip() { return "Transforming position of the spawned object with perlin noise offset, can provide landscape effect"; }

        // Define what your script will do
        public EProcedureType Type { get { return EProcedureType.Event; } }

        [PGG_SingleLineSwitch("OffsetMode", 58, "Select if you want to offset postion with cell size or world units", 140)]
        public Vector3 PosOffset = new Vector3(0f,2f,0f);
        [HideInInspector] public ESR_Measuring OffsetMode = ESR_Measuring.Cells;

        [PGG_SingleLineTwoProperties("Space", 88, 60, 14)]
        public ESP_OffsetMode ApplyMode = ESP_OffsetMode.OverrideOffset; 
        [HideInInspector] public ESP_OffsetSpace Space = ESP_OffsetSpace.LocalSpace; 

        [Space(6)]
        public Vector2 PerlinNoiseScale = new Vector2(16, 16);
        public Vector2 OffsetNoise = Vector2.zero;
        [Space(2)]
        [Tooltip("Making objects transform max by this units, setted to 2 offset will start at two units")]
        public float SquareSteps = 0f;

        #region There you can do custom modifications for inspector view
#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            // GUIIgnore.Clear(); GUIIgnore.Add("Tag"); // Custom ignores drawing properties
            base.NodeBody(so);
        }
#endif
        #endregion


        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            Vector3 tgtOffset = PosOffset;

            float xNoise = Mathf.PerlinNoise(cell.PosXZ.x * PerlinNoiseScale.x * 0.01f + OffsetNoise.x, cell.PosXZ.y * PerlinNoiseScale.y * 0.01f + +OffsetNoise.y);

            tgtOffset.x *= xNoise;
            tgtOffset.y *= xNoise;
            tgtOffset.z *= xNoise;

            tgtOffset = GetUnitOffset(tgtOffset, OffsetMode, preset);

            if (SquareSteps > 0f)
            {
                tgtOffset.x = Mathf.Round(tgtOffset.x * SquareSteps) * SquareSteps;
                tgtOffset.y = Mathf.Round(tgtOffset.y * SquareSteps) * SquareSteps;
                tgtOffset.z = Mathf.Round(tgtOffset.z * SquareSteps) * SquareSteps;
            }
            else
                SquareSteps = 0f;

            if (ApplyMode == ESP_OffsetMode.OverrideOffset)
            {
                if (Space == ESP_OffsetSpace.LocalSpace) spawn.DirectionalOffset = tgtOffset;
                else spawn.Offset = tgtOffset;
            }
            else
            {
                if (Space == ESP_OffsetSpace.LocalSpace) spawn.DirectionalOffset += tgtOffset;
                else spawn.Offset += tgtOffset;
            }
        }

    }
}