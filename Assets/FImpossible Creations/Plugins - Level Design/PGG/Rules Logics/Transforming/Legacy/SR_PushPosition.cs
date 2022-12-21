#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Transforming.Legacy
{
    public class SR_PushPosition : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Push Position"; }
        public override string Tooltip() { return "Offsetting spawn position like DirectOffset but in additive way"; }
        public EProcedureType Type { get { return EProcedureType.Event; } }

        public Vector3 Offset = Vector3.zero;
        public Vector3 RandomOffset = Vector3.zero;
        [Tooltip("If you want to add random offset to the object by 0.1 units in x axis, set here 0.1 value in x")]
        [HideInInspector] public Vector3 RoundOffsetBy = Vector3.zero;

#if UNITY_EDITOR
        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            if (RandomOffset != Vector3.zero)
                EditorGUILayout.PropertyField(so.FindProperty("RoundOffsetBy"));

            so.ApplyModifiedProperties();

            base.NodeFooter(so, mod);
        }
#endif

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            Quaternion rot = spawn.GetRotationOffset();
            Vector3 offset = Offset;

            if (RandomOffset != Vector3.zero)
            {
                Vector3 rOffset = new Vector3
                (
                    FGenerators.GetRandom(-RandomOffset.x, RandomOffset.x),
                    FGenerators.GetRandom(-RandomOffset.y, RandomOffset.y),
                    FGenerators.GetRandom(-RandomOffset.z, RandomOffset.z)
                );

                if (RoundOffsetBy != Vector3.zero)
                {
                    if (RoundOffsetBy.x != 0f) rOffset.x = Mathf.Round(rOffset.x / RoundOffsetBy.x) * RoundOffsetBy.x;
                    if (RoundOffsetBy.y != 0f) rOffset.y = Mathf.Round(rOffset.y / RoundOffsetBy.y) * RoundOffsetBy.y;
                    if (RoundOffsetBy.z != 0f) rOffset.z = Mathf.Round(rOffset.z / RoundOffsetBy.z) * RoundOffsetBy.z;
                }

                offset += rOffset;
            }



            spawn.Offset += rot * offset;
            spawn.TempPositionOffset += offset;
        }
    }
}