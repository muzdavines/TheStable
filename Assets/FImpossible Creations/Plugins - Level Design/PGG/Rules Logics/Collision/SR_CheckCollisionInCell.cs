using FIMSpace.Generating.Rules.Helpers;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Collision
{
    public class SR_CheckCollisionInCell : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Check If Collides"; }
        public override string Tooltip() { return "Checking collision in cell with model's bounding box"; }
        public EProcedureType Type { get { return EProcedureType.Event; } }


        [PGG_SingleLineSwitch("CheckMode", 68, "Select if you want to use Tags, SpawnStigma or CellData", 110)]
        public string StopOnTagged = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        [Space(5)]
        public Vector3 OffsetColliderOrigin = Vector3.zero;
        [PGG_SingleLineTwoProperties("AllMeshesBounds", 130, 120, 18)]
        public float CollisionBoxSize = 0.9f;
        [Tooltip("Build bounding box out of all found mesh renderers")]
        [HideInInspector] public bool AllMeshesBounds = false;
        [Tooltip("If there is rist of colliding with models which size is bigger than single cell or this object can go out of cell and should have checked collision with neightbour cells spawns")]
        public bool CheckAlsoNeightbourCells = false;
        [Tooltip("(distance measured in cells!) How many neightbour cells should be checked for collision (if more then it costs more time)")]
        public int CheckNeightboursDistance = 1;

        [Space(5)]
        public Vector3 NonUniformScale = Vector3.one;

        [Space(4)]
        public bool Debug = false;

#if UNITY_EDITOR

        public override void NodeBody(SerializedObject so)
        {
            if (CheckAlsoNeightbourCells == true)
            {
                if (GUIIgnore.Count > 0) GUIIgnore.Clear();
                if (CheckNeightboursDistance < 1) CheckNeightboursDistance = 1;
            }
            else
            {
                if (GUIIgnore.Count == 0) GUIIgnore.Add("CheckNeightboursDistance");
            }

            EditorGUILayout.HelpBox("Collision is checked just with spawns already in same cell using bounding boxes of meshes", MessageType.None);
            if (string.IsNullOrEmpty(StopOnTagged)) EditorGUILayout.HelpBox("When 'StopOnTagged' is empty then collision is checked on every spawn in cell", MessageType.None);

            base.NodeBody(so);
        }
#endif

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            _EditorDebug = Debug;

            // Preparing parameters for mocing on grid and checking collisions
            CollisionOffsetData thisOffset = new CollisionOffsetData(spawn, null, AllMeshesBounds);
            Vector3 checkPos = cell.WorldPos(preset); checkPos += thisOffset.boundsWithSpawnOff.center + spawn.GetRotationOffset() * OffsetColliderOrigin;
            //Vector3 checkPos = spawn.GetWorldPositionWithFullOffset(preset);
            //Vector3 off = spawn.Offset;
            //if (off == Vector3.zero) off = spawn.TempPositionOffset;
            //else if (spawn.DirectionalOffset.sqrMagnitude > 0f) off = Quaternion.Euler(spawn.RotationOffset) * spawn.DirectionalOffset;
            //checkPos += off;

            List<SpawnData> checkSpawns = cell.CollectSpawns(OwnerSpawner.ScaleAccess, true);

            if (CheckAlsoNeightbourCells)
            {
                if (CheckNeightboursDistance < 1) CheckNeightboursDistance = 1;

                //var neight = grid.Get3x3Square(cell, false);
                var neight = grid.GetDistanceSquare2DList(cell, CheckNeightboursDistance);

                for (int n = 0; n < neight.Count; n++)
                {
                    if (FGenerators.CheckIfIsNull(neight[n])) continue;
                    if (neight[n] == cell) continue;
                    PGGUtils.TransferFromListToList<SpawnData>(neight[n].CollectSpawns(OwnerSpawner.ScaleAccess), checkSpawns);
                }
            }

            Bounds currentCollCheck =
            new Bounds(checkPos,
                          Vector3.Scale(thisOffset.bounds.size * CollisionBoxSize, NonUniformScale));

            //FDebug.DrawBounds3D(currentCollCheck, Color.yellow, 1f);

            // Check box bounded collision on spawns
            for (int s = 0; s < checkSpawns.Count; s++)
            {
                var sp = checkSpawns[s];
                if (sp == spawn) continue;
                if (sp.Enabled == false) continue;
                if (sp.Spawner == OwnerSpawner) continue;

                if (string.IsNullOrEmpty(StopOnTagged) == false)
                    if (SpawnHaveSpecifics(sp, StopOnTagged, CheckMode) == false) continue; // Checking collision only on wanted tags

                CollisionOffsetData checkCol = new CollisionOffsetData(sp);
                Bounds oBounds = checkCol.boundsWithSpawnOff;

                if (sp.OwnerCell != null)
                    oBounds.center += sp.OwnerCell.WorldPos(preset);
                else
                    oBounds.center += cell.WorldPos(preset);

                if (Debug) FDebug.DrawBounds3D(oBounds, Color.blue, 1f);

                if (currentCollCheck.Intersects(oBounds))
                //if (oBounds.Intersects(currentCollCheck))
                {
                    if (Debug) FDebug.DrawBounds3D(oBounds, Color.red, 1f);
                    // Colllision occured, hard stop
                    //CellAllow = false;
                    spawn.Enabled = false;
                    return;
                }
            }
        }



#if UNITY_EDITOR    
        public override void OnDrawDebugGizmos(FieldSetup preset, SpawnData spawn, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            base.OnDrawDebugGizmos(preset, spawn, cell, grid);

            if (spawn.isTemp)
                Gizmos.color = new Color(0.1f, .7f, 0.1f, 0.3f);
            else
                Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.6f);

            CollisionOffsetData thisOffset = new CollisionOffsetData(spawn, null, AllMeshesBounds);
            Vector3 pos = cell.WorldPos(preset); pos += thisOffset.boundsWithSpawnOff.center + spawn.GetRotationOffset() * OffsetColliderOrigin;
            Vector3 size = Vector3.Scale(thisOffset.boundsWithSpawnOff.size * CollisionBoxSize, NonUniformScale);
            if (spawn.isTemp == false)
            {
                Gizmos.DrawCube(pos, size);
            }

            Gizmos.DrawWireCube(pos, size);
            Gizmos.color = _DbPreCol;
        }
#endif

    }
}