using FIMSpace.Generating.Rules.Helpers;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Collision.Legacy
{
    public class SR_BoundCollisionOffset : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Bound Collision Offset"; }
        public override string Tooltip() { return "Offsetting position with collision check basing on bounds box shape of prefab's mesh or collider"; }
        public EProcedureType Type { get { return EProcedureType.Event; } }

        [HideInInspector] public bool OverrideOffset = false;
        [HideInInspector] public bool Randomize = false;


        [PGG_SingleLineSwitch("CheckMode", 68, "Select if you want to use Tags, SpawnStigma or CellData", 100)]
        public string IgnoreTagged = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        [Range(0f, 2f)] public float Amount = 1f;
        [HideInInspector] public float ToSides = 0f;
        [HideInInspector] public float ToSideMin = 0f;
        public float LimitToCellMargins = 2f;

#if UNITY_EDITOR
        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            base.NodeFooter(so, mod);

            SerializedProperty sp = so.FindProperty("ToSides");
            SerializedProperty spn = sp.Copy(); spn.Next(false);

            float min = sp.floatValue, max = spn.floatValue;
            EditorGUILayout.MinMaxSlider(new GUIContent("To Sides"), ref min, ref max, 0f, 1f);
            sp.floatValue = min;
            spn.floatValue = max;
        }
#endif

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            CollisionOffsetData thisOffset = new CollisionOffsetData(spawn);
            thisOffset.prbounds = CollisionOffsetData.PRBounds(thisOffset.bounds, thisOffset.scale * 0.95f, thisOffset.positionOffset + Vector3.up * thisOffset.bounds.extents.y * 0.1f);

            float sideVal = FGenerators.GetRandom(ToSideMin, ToSides);
            sideVal *= FGenerators.GetRandom() > 0.5f ? -1f : 1f;
            Vector3 preOff = spawn.Offset;
            Vector3 predOff = spawn.DirectionalOffset;

            #region Debugging

            //if (cell.FlatPos == new Vector2Int(2, 0))
            //{
            //    Vector3 cOff = new Vector3(cell.FlatPos.x, 0, cell.FlatPos.y) * 2f;

            //    for (int i = 0; i < spawns.Count; i++)
            //    {
            //        if (spawns[i] == null) continue;
            //        if (spawns[i].Prefab == null) continue;
            //        if (spawns[i].Prefab.GetComponentInChildren<Collider>() == null)
            //            if (FTransformMethods.FindComponentInAllChildren<Collider>(spawns[i].Prefab.transform) == null) continue;
            //        if (spawns[i].PreviewMesh == null) continue;

            //        Bounds obo = spawns[i].PreviewMesh.bounds;
            //        obo.center += cOff + spawns[i].Offset + Quaternion.Euler(spawns[i].RotationOffset) * spawns[i].DirectionalOffset;
            //        PlanHelper.DebugBounds3D(obo, Color.red);
            //    }

            //    Bounds ob = spawn.PreviewMesh.bounds;
            //    ob.center += cOff + spawn.Offset + Quaternion.Euler(spawn.RotationOffset) * spawn.DirectionalOffset;
            //    PlanHelper.DebugBounds3D(ob, Color.green);

            //}

            #endregion Debugging

            var spawns = cell.CollectSpawns(OwnerSpawner.ScaleAccess);

            // Getting child collision datas
            for (int i = 0; i < spawns.Count; i++)
            {
                if (spawns[i] == null) continue;
                if (spawns[i].Prefab == null) continue;
                if (spawns[i].Prefab.GetComponentInChildren<Collider>() == null)
                    if (FTransformMethods.FindComponentInAllChildren<Collider>(spawns[i].Prefab.transform) == null) continue;
                if (spawns[i].PreviewMesh == null) continue;
                
                if ( !string.IsNullOrEmpty(IgnoreTagged) )
                {
                    if (SpawnHaveSpecifics(spawns[i], IgnoreTagged, CheckMode)) continue;
                }
  
                if (! thisOffset.OffsetOn(new CollisionOffsetData(spawns[i]), ref spawn, Amount, cell, sideVal, LimitToCellMargins) )
                {
                    spawn.Offset = preOff;
                    spawn.Offset = predOff;
                    return;
                }
            }

        }

    }
}