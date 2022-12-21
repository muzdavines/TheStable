using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class OStamperSet
    {

        // Raycasting origin position -------------------------------------------------------------
        /// <summary>
        /// Raycast origin without additional cast offset
        /// </summary>
        public Vector3 GetRawRayOrigin(ObjectStamperEmittedInfo info, Transform parent)
        {
            return GetCastVector(info, parent, RaycastWorldSpace == false);
        }

        /// <summary>
        /// Raycast origin without additional cast offset
        /// </summary>
        public Vector3 GetRawRayOrigin(ObjectStamperEmittedInfo info, Transform parent, bool local)
        {
            //UnityEngine.Debug.Log("info pr = " + info.PrefabReference);

            Vector3 l = info.PrefabReference.ReferenceBounds.center + info.GetPositionOffsetLocal();

            if (local) return l; // Local Space
            return info.GetMatrixFor(parent).MultiplyPoint(l);
        }

        /// <summary>
        /// Raycast origin with additional cast offset
        /// </summary>
        public Vector3 GetRayOrigin(ObjectStamperEmittedInfo info, Transform parent, bool local)
        {
            return GetRawRayOrigin(info, parent, local);// - GetRaycastingOffset(info, local, parent);
        }


        // Raycasting direction -------------------------------------------------------------
        public Vector3 GetCastVector(ObjectStamperEmittedInfo info, Transform parent, bool local = true)
        {
            Vector3 l;

            //if ( info.SetReference.ObjectShape == EOSType.Object)
            //{
            if (info.PrefabReference.FlatBounds)
            {
                l = (RayCheckDirection.normalized * info.SetReference.ReferenceBounds.size.magnitude) * RayDistanceMul;
            }
            else
            {
                Vector3 localBoundedSpaceDirection = GetBoundedDirection(RayCheckDirection.normalized);
                l = RayCheckDirection * localBoundedSpaceDirection.magnitude * RayDistanceMul;
            }
            //}
            //else
            //{
            //    l = (RayCheckDirection.normalized * info.SetReference.ReferenceBounds.size.magnitude) * RayDistanceMul;
            //}

            if (local) return l; // Local ray direction
            if (RaycastWorldSpace) return Vector3.Scale(l, parent.lossyScale); // If world space direction then scalling with emitter
            return info.GetMatrixFor(parent).MultiplyVector(l); // Ray direction and scale according to emitter
        }

        /// <summary>
        /// Raycasting with box shape of reference bounds of the prefab from emitted info
        /// </summary>
        public RaycastHit[] BoundsCast(ObjectStamperEmittedInfo info, Vector3 origin, Quaternion boundsRotation, Vector3 directionNormalized, float targetDistance)
        {
            Vector3 boundsOffset = Vector3.Scale(info.PrefabReference.ReferenceBounds.extents, directionNormalized);
            float boxCastDist = targetDistance - boundsOffset.magnitude;

            #region Debugging

            //Debug.DrawRay( origin + Vector3.left * 0.2f, directionNormalized * boxCastDist, Color.white, 0.5f);

            #endregion

            RaycastHit[] boundsHit = Physics.BoxCastAll(origin, info.PrefabReference.ReferenceBounds.extents, directionNormalized, boundsRotation, boxCastDist, RayCheckLayer, QueryTriggerInteraction.Ignore);

            if (info.SetReference != null)
            {
                if (info.SetReference.IgnoreCheckOnTags.Count > 0)
                {
                    List<RaycastHit> notIgnore = new List<RaycastHit>();
                    for (int i = 0; i < boundsHit.Length; i++)
                    {
                        OStampStigma stigm = boundsHit[i].transform.GetComponent<OStampStigma>();

                        if (stigm != null)
                            if (stigm.ReferenceSet != null)
                                if (info.SetReference.IgnoreCheckOnTags.Contains(stigm.ReferenceSet.StampersetTag))
                                    continue;

                        if (info.SetReference.IgnoreCheckOnTags.Contains(boundsHit[i].transform.tag))
                            continue;

                        notIgnore.Add(boundsHit[i]);
                    }

                    return notIgnore.ToArray();
                }
            }

            return boundsHit;
        }

    }
}