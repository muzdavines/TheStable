using UnityEngine;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Rules.PostEvents
{
    public class SR_AlignToGround : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Align To Ground"; }
        public override string Tooltip() { return "Align spawned object to ground with raycasting\n" + base.Tooltip(); }

        public EProcedureType Type { get { return EProcedureType.Event; } }

        [Header("Detecting Ground")]
        public LayerMask GroundRaycastMask = 1 << 0;
        [Tooltip("Most cases it will be 0,-1,0 so straight down")]
        public Vector3 RaycastDirection = Vector3.down;
        [Tooltip("How far collision raycast can go")]
        public float RaycastLength = 7f;
        [Tooltip("Casting ray from upper or lower position of the object")]
        public Vector3 OffsetRaycastOrigin = Vector3.up;

        [Header("Placing on Ground")]
        [Range(0f, 1f)]
        public float RotationAlignAmount = 1f;
        //public Vector3 ClampAngles = new Vector3(90f, 90f, 90f);
        [Tooltip("If origin of the object is not on the bottom you can move it higher/lower with this value")]
        public Vector3 OffsetOnGround = Vector3.zero;
        [Tooltip("Offset on slope, like when ground have very big angle then object will snap-offset to side of the slope")]
        [Range(0f,1f)] public float OffsetWithSlopeDirection = 1f;
        [Space(4)]
        [Tooltip("Adds component to the game object to align it when game starts")]
        public bool AlignOnlyInPlaymode = false;


        #region There you can do custom modifications for inspector view
#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            base.NodeBody(so);
        }
#endif
        #endregion


        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CellInfluence(preset, mod, cell, ref spawn, grid);

            if (AlignOnlyInPlaymode)
            {
                // Add start-align component to spawned object with node's stats
                Action<GameObject> alignOnGroundComponent =
                (o) =>
                {
                    PGGTool_AlignOnGround aligner = o.AddComponent<PGGTool_AlignOnGround>();
                    aligner.AllowPostGenerator = false;
                    aligner.AlignOnGameStart = true;

                    aligner.GroundRaycastMask = GroundRaycastMask;
                    aligner.RaycastDirection = RaycastDirection;
                    aligner.RaycastLength = RaycastLength;
                    aligner.OffsetRaycastOrigin = OffsetRaycastOrigin;
                    aligner.AlignRotationAmount = RotationAlignAmount;
                    aligner.OffsetOnGround = OffsetOnGround;
                };

                spawn.OnGeneratedEvents.Add(alignOnGroundComponent);
            }
            else
            {
                Action<GameObject> alignOnGround =
                (o) =>
                {
                    AlignObjectOnGround(o, GroundRaycastMask, RaycastDirection, RaycastLength, OffsetRaycastOrigin, RotationAlignAmount, OffsetOnGround, OffsetWithSlopeDirection);
                };

                spawn.OnGeneratedEvents.Add(alignOnGround);
            }
        }

        public static void AlignObjectOnGround(GameObject o, LayerMask mask, Vector3 rDir, float rDist, Vector3 offOrigin, float amount, Vector3 groundOff, float offsetWithSlope)
        {
            Vector3 dirN = rDir.normalized;
            Ray ray = new Ray(o.transform.position + (offOrigin), dirN);
            RaycastHit rHit;

            if (Physics.Raycast(ray, out rHit, rDist, mask, QueryTriggerInteraction.Ignore))
            {
                Quaternion slopeRotation = Quaternion.FromToRotation(-dirN, rHit.normal);
                o.transform.position = rHit.point + Vector3.LerpUnclamped(groundOff, Quaternion.FromToRotation(Vector3.up, rHit.normal) * groundOff, offsetWithSlope);

                Quaternion backupRotation = o.transform.rotation;
                o.transform.rotation = Quaternion.LerpUnclamped(backupRotation, backupRotation * slopeRotation, amount);
            }
        }

        public static void DrawGroundAligningGizmos(GameObject o, Vector3 raycastDirection, float raycastLength, Vector3 offsetRaycastOrigin)
        {
            Vector3 origin = o.transform.position + (offsetRaycastOrigin);

#if UNITY_EDITOR
            // Handles are editor only
            Handles.color = new Color(1f, 1f, 1f, 0.85f);
#endif

            Gizmos.color = new Color(1f, 1f, 1f, 0.75f);
            Gizmos.DrawRay(origin, raycastDirection.normalized * raycastLength);
            Gizmos.DrawSphere(origin, raycastLength * 0.03f);
            Gizmos.DrawSphere(origin + raycastDirection.normalized * raycastLength, raycastLength * 0.03f);
        }


    }
}