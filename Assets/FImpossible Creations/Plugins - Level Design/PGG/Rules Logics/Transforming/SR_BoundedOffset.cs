using UnityEngine;

namespace FIMSpace.Generating.Rules.Transforming
{
    public class SR_BoundedOffset : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Bounded Offset"; }
        public override string Tooltip() { return "Offsetting position on grid using prefab's mesh or collider scale"; }
        public EProcedureType Type { get { return EProcedureType.Event; } }

        public Vector3 WorldOffset = Vector3.zero;
        public Vector3 DirectionalOffset = Vector3.zero;

        [PGG_SingleLineSwitch("CheckMode", 68, "Select if you want to use Tags, SpawnStigma or CellData", 80)]
        public string DirectFrom = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (spawn.PreviewMesh == null) return;
            if (spawn.Prefab == null) return;

            Bounds b = spawn.GetMeshFilterOrColliderBounds();
            Vector3 sz = Vector3.Scale(b.size, spawn.LocalScaleMul);
            //Vector3 sz = Vector3.Scale(Vector3.Scale(spawn.PreviewMesh.bounds.size, spawn.Prefab.transform.lossyScale) - spawn.GetRotationOffset() * spawn.PreviewMesh.bounds.center, spawn.LocalScaleMul);

            //SpawnData getSpawn = CellSpawnsHaveTag(cell, DirectFrom, spawn);

            if (WorldOffset != Vector3.zero)
            {
                spawn.Offset += Vector3.Scale(WorldOffset, sz);
            }

            if (DirectionalOffset != Vector3.zero)
            {
                SpawnData getSpawn = CellSpawnsHaveSpecifics(cell, DirectFrom, CheckMode, spawn);
               
                if (getSpawn != null)
                    spawn.DirectionalOffset += Quaternion.Euler(getSpawn.RotationOffset) * Vector3.Scale(DirectionalOffset, sz);
                else
                    spawn.DirectionalOffset += Vector3.Scale(DirectionalOffset, sz);
            }
        }
    }
}