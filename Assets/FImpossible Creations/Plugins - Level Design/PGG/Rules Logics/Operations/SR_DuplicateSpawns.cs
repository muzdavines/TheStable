using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Rules.Operations
{
    public class SR_DuplicateSpawns : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Duplicate Spawns"; }
        public override string Tooltip() { return "Duplicating target prefab to spawn few times"; }

        public EProcedureType Type { get { return EProcedureType.OnConditionsMet; } }

        public Vector3Int Iterations = new Vector3Int(2, 1, 2);
        public Vector3 OffsetPerIteration = new Vector3(1f, 0f, 1f);
        [Space(5)]
        public ESP_OffsetSpace PositionOffsetSpace = ESP_OffsetSpace.WorldSpace;
        public Vector3 RandomizeOffset = new Vector3(0.2f, 0f, 0.2f);
        public Vector3 RandomizeRotation = new Vector3(0.0f, 45f, 0.0f);
        [Space(5)]
        public bool AddOneOffset = false;


        #region There you can do custom modifications for inspector view
#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            // GUIIgnore.Clear(); GUIIgnore.Add("Tag"); // Custom ignores drawing properties
            base.NodeBody(so);
        }
#endif
        #endregion


        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {

            for (int x = 0; x < Iterations.x; x++)
            {
                for (int y = 0; y < Iterations.y; y++)
                {
                    for (int z = 0; z < Iterations.z; z++)
                    {
                        SpawnData clone = thisSpawn.Copy(true);

                        Vector3 randOffset = new Vector3();
                        randOffset.x = FGenerators.GetRandom(-randOffset.x, RandomizeOffset.x);
                        randOffset.y = FGenerators.GetRandom(-randOffset.y, RandomizeOffset.y);
                        randOffset.z = FGenerators.GetRandom(-randOffset.z, RandomizeOffset.z);

                        Vector3 newOffset = randOffset;

                        if (PositionOffsetSpace == ESP_OffsetSpace.WorldSpace)
                            newOffset += clone.Offset;
                        else
                            newOffset += clone.DirectionalOffset;

                        int off = 0;
                        if (AddOneOffset) off = 1;
                        newOffset.x += OffsetPerIteration.x * (x+off);
                        newOffset.y += OffsetPerIteration.y * (y);
                        newOffset.z += OffsetPerIteration.z * (z+off);

                        Vector3 rotOffset = new Vector3();
                        rotOffset.x = FGenerators.GetRandom(-RandomizeRotation.x, RandomizeRotation.x);
                        rotOffset.y = FGenerators.GetRandom(-RandomizeRotation.y, RandomizeRotation.y);
                        rotOffset.z = FGenerators.GetRandom(-RandomizeRotation.z, RandomizeRotation.z);
                        
                        clone.RotationOffset += rotOffset;

                        if (PositionOffsetSpace == ESP_OffsetSpace.WorldSpace)
                            clone.Offset = newOffset;
                        else
                            clone.DirectionalOffset = newOffset;

                        cell.AddSpawnToCell(clone);
                    }
                }
            }

        }

    }
}