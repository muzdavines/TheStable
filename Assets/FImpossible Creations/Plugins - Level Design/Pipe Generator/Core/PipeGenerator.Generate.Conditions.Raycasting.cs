using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class PipeGenerator
    {
        Vector3 boxCastHalfScale;
        bool CheckSegmentJoinCollision(Vector3 rootPoint, Vector3 snapPoint, bool useCollision = true)
        {
            if (useCollision == false) return false;
            if (ObstaclesMask == 0) return false;

            RaycastHit hit;
            Vector3 dir = (snapPoint - rootPoint);

            if (BoxcastScale <= 0f)
            {
                Vector3 startPos = rootPoint;
                Vector3 endPos = snapPoint;

                if (Physics.Linecast(startPos, endPos, out hit, ObstaclesMask, QueryTriggerInteraction.Ignore))
                {
                    return true;
                }
            }
            else
            {
                Vector3 dirN = dir.normalized;
                Vector3 start = rootPoint - dirN * (boxCastHalfScale.x * 1.5f);
                float castDistance = dir.magnitude + boxCastHalfScale.x * 1.0f;// - BoxcastScale * 0.25f;

                #region Debugging
#if UNITY_EDITOR
                //if (Application.isPlaying == false) FDebug.DrawBounds3D(new Bounds(start, boxCastHalfScale * 2f), Color.green, 1f);
                //if (Application.isPlaying == false) Debug.DrawLine(start + Vector3.up * 0.02f, start +  dirN * castDistance - Vector3.up * 0.02f, Color.yellow, 1f);
                //if (Application.isPlaying == false) FDebug.DrawBounds3D(new Bounds(start + dirN * castDistance, boxCastHalfScale * 2f), Color.cyan, 1f);
#endif
                #endregion


                if (Physics.BoxCast(start, boxCastHalfScale, dirN, out hit, Quaternion.LookRotation(dirN), castDistance, ObstaclesMask, QueryTriggerInteraction.Ignore))
                {
                    //Debug.DrawRay(hit.point, hit.normal, Color.red, 3.1f);
                    return true;
                }
                else
                {
                    //Debug.DrawRay(start, dirN * castDistance, Color.green, 5f);
                    //Debug.DrawRay(start, dirN * (castDistance + BoxcastScale * 0.5f), Color.green, 4f);
                }
            }

            return false; // No Collision
        }


        bool CheckIfCollidesWithSelfSegment(Vector3 rootPoint, Vector3 snapPoint)
        {
            if (allSpawns.Count < 2) return false;
            if (SelfCollisionScale <= 0f) return false;

            Vector3 dir = snapPoint - rootPoint;
            float magn = dir.magnitude;
            Ray r = new Ray(rootPoint, dir.normalized);

            for (int i = 0; i < allSpawns.Count; i++)
            {
                if (allSpawns[i].PreviewMesh == null) continue;

                float boundedDistance = Vector3.Distance(rootPoint, allSpawns[i].Position);
                if (boundedDistance > allSpawns[i].PreviewMesh.bounds.size.magnitude * 2f) continue; // Don't waste calculation time on far objects

                Bounds b = allSpawns[i].RotatedBounds;
                b.size *= SelfCollisionScale;
                b.center = allSpawns[i].Position + allSpawns[i].TransformVector(allSpawns[i].PreviewMesh.bounds.center);

                //PlanHelper.DrawBounds3D(b, Color.yellow);

                float dist;
                bool intersect = b.IntersectRay(r, out dist);

                if (intersect)
                    if (dist < magn)
                    {
                        //UnityEngine.Debug.Log("dist = " + dist);
                        //Debug.DrawRay(r.origin, dir, Color.red, 1f);
                        return true;
                    }
            }

            return false;
        }

        bool CheckIfCollidesWithSelfSegment(PipeSegmentSetup seg, PipeSegmentSetup.JoinPoint join, Vector3 snapPos, Quaternion rotation)
        {
            if (SelfCollisionScale <= 0f) return false;

            Bounds thisBounds = FEngineering.RotateBoundsByMatrix(seg.PreviewMesh.bounds, rotation);
            thisBounds.size *= SelfCollisionScale;
            thisBounds.center = snapPos + (rotation * (seg.PreviewMesh.bounds.center * SelfCollisionScale));

            float boundsMinDistance = thisBounds.size.magnitude * 2.5f;

            for (int i = 0; i < allSpawns.Count; i++)
            {
                float dist = Vector3.Distance(thisBounds.center, allSpawns[i].Position);

                if (dist < boundsMinDistance)
                {
                    Bounds oBounds = allSpawns[i].RotatedBounds;
                    oBounds.size *= SelfCollisionScale;
                    oBounds.center = allSpawns[i].Position + allSpawns[i].TransformVector(allSpawns[i].PreviewMesh.bounds.center * SelfCollisionScale);

                    if (thisBounds.Intersects(oBounds))
                    {
                        #region Debugging
#if UNITY_EDITOR
                        if (Application.isPlaying == false) FDebug.DrawBounds3D(oBounds, new Color(1f, 1f, 0f, 0.25f));
#endif
                        #endregion

                        return true;
                    }
                }
            }

            return false;
        }


        RaycastHit RaycastGetHit(Vector3 start, Vector3 end, LayerMask mask)
        {
            RaycastHit hit;
            Physics.Linecast(start, end, out hit, mask, QueryTriggerInteraction.Ignore);
            return hit;
        }

        RaycastHit RaycastGetHit(Vector3 start, Vector3 dir, float distance, LayerMask mask)
        {
            RaycastHit hit;
            Physics.Raycast(start, dir, out hit, distance, mask, QueryTriggerInteraction.Ignore);
            return hit;
        }

        public Vector3 FlattenNormal(Quaternion orientation, Vector3? forward = null)
        {
            Vector3 f = forward == null ? Vector3.forward : forward.Value;
            var vec = orientation.eulerAngles;
            vec.x = Mathf.Round(vec.x / 90) * 90;
            vec.y = Mathf.Round(vec.y / 90) * 90;
            vec.z = Mathf.Round(vec.z / 90) * 90;
            return Quaternion.Euler(vec) * f;
        }

    }
}
