
using UnityEngine;

namespace FIMSpace.Generating.Rules.Collision.Legacy
{
    public class SR_OffsetFromBounds : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Offset From Bounds"; }
        public override string Tooltip() { return "Offsetting spawn's position basing on other object's bounds, placing this spawn next to other object if possible"; }
        public EProcedureType Type { get { return EProcedureType.Event; } }

        [HideInInspector] public bool OverrideOffset = false;
        [HideInInspector] public bool Randomize = false;

        public string OffseOnlyOnTag = "";
        public string NotOffsetTag = "";
        [Range(0f, 2f)] public float Amount = 1f;
        [Range(0f, 2f)] public float PushOther= 0f;

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            SpawnData sp = null;
            if (string.IsNullOrEmpty(OffseOnlyOnTag) == false)
                sp = CellSpawnsHaveTag(cell, OffseOnlyOnTag);

            Vector3 pos = preset.GetCellWorldPosition(cell);

            if (sp != null && sp != spawn) 
                OffsetOn(ref spawn, ref sp, pos);
            else
                for (int i = 0; i < cell.CollectSpawns().Count; i++)
                {
                    SpawnData spd = cell.CollectSpawns()[i];
                    if (spd == spawn) continue;
                    if (SpawnHaveTag(spd, NotOffsetTag) ) continue;

                    OffsetOn(ref spawn, ref spd, pos);
                }
        }

        public void OffsetOn(ref SpawnData source, ref SpawnData other, Vector3 pos)
        {
            if (source.PreviewMesh == null) return;
            if (source.Prefab == null) return;

            if (other.PreviewMesh == null) return;
            if (other.Prefab == null) return;

            Bounds sBounds = source.PreviewMesh.bounds;
            sBounds.size = Vector3.Scale(sBounds.size, Vector3.Scale(source.Prefab.transform.lossyScale, source.LocalScaleMul));
            sBounds.center = pos + source.Offset + Quaternion.Euler(source.RotationOffset) * source.DirectionalOffset;

            Bounds oBounds = other.PreviewMesh.bounds;
            oBounds.size = Vector3.Scale(oBounds.size, Vector3.Scale(other.Prefab.transform.lossyScale, other.LocalScaleMul));
            oBounds.center = pos + other.Offset + Quaternion.Euler(other.RotationOffset) * other.DirectionalOffset;

            if ( sBounds.Intersects(oBounds))
            {
                Vector3 pushOutDir = (sBounds.center) - (oBounds.center);
                source.Offset += pushOutDir * Amount;
                other.Offset += -pushOutDir * PushOther;
            }

        }
    }
}