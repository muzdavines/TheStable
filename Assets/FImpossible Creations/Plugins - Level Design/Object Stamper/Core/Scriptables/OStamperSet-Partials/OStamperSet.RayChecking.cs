using System.Linq;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class OStamperSet
    {

        /// <summary>
        /// Container for box cast and raycast checking area for spawning emitted objects on top of other objects or just in choosed placement
        /// </summary>
        public struct PlacementVolumeRaycastingData
        {
            public Transform emitter;
            public ObjectStamperEmittedInfo emittedInfo;
            public RaycastHit mainHit;
            public RaycastHit backupFullLineCast;
            public Vector3 raycastingDirection;
            public Vector3 lineCastPoint;
            public Vector3 customPoint;
            public Vector3 customNormal;
            public RaycastHit[] raycastHits;
            public string OverrideInfo;

            public PlacementVolumeRaycastingData(Transform emitter, ObjectStamperEmittedInfo emittedInfo, RaycastHit mainHit, RaycastHit[] raycastHits, Vector3 lineCastPoint, Vector3 raycastingDirection, Vector3 customPoint, Vector3 customNormal, RaycastHit backupFullLineCast)
            {
                this.emitter = emitter;
                this.emittedInfo = emittedInfo;
                this.mainHit = mainHit;
                this.raycastingDirection = raycastingDirection;
                this.raycastHits = raycastHits;
                this.lineCastPoint = lineCastPoint;
                this.customPoint = customPoint;
                this.customNormal = customNormal;
                this.backupFullLineCast = backupFullLineCast;
                OverrideInfo = string.Empty;
            }

            public static readonly PlacementVolumeRaycastingData Empty = new PlacementVolumeRaycastingData();
        }


        /// <summary>
        /// Container for raycasting restriction check containing 'Allow' spawn variable and info about cause if spawn not allowed
        /// </summary>
        public struct RaycastingRestrictionsCheckResult
        {
            public RaycastHit originHit;
            public Vector3 targetPosition;
            public bool allow;
            public string info;

            public RaycastingRestrictionsCheckResult(bool allow, string info, RaycastHit hit, Vector3? targetPosition = null)
            {
                this.allow = allow;
                this.info = info;
                if (targetPosition == null) this.targetPosition = hit.point; else this.targetPosition = (Vector3)targetPosition;
                originHit = hit;
            }

            public static readonly RaycastingRestrictionsCheckResult Empty = new RaycastingRestrictionsCheckResult();
        }


        /// <summary>
        /// Short: Checking if stamper info can be emitted in target raycasted placement volume.
        /// Checking StamperSet restrictions for spawner, returning RaycastCheckResult with 'Allow' setted to false with cause at 'Info' if can't spawn in this raycasting area
        /// </summary>
        public PlacementVolumeRaycastingData GetRaycastingVolumeFor(ObjectStamperEmittedInfo info, Transform parent)
        {
            Vector3 origin = GetRayOrigin(info, parent, false);
            Vector3 castRay = GetCastVector(info, parent, false);

            Vector3 direction = castRay.normalized;
            float castDistance = castRay.magnitude;

            if (info.PrefabReference.ReferenceBounds.extents == Vector3.zero) { info.PrefabReference.RefreshBounds(); }

            RaycastHit[] boundsHit = BoundsCast(info, origin, parent.rotation * info.RotOffset, direction, castDistance);
            //UnityEngine.Debug.DrawRay(origin, direction * castDistance, Color.white, 1.01f);

            #region Debugging

            //Debug.Log("BoundHits: " + boundsHit.Length + " from " + parent.name);
            //Debug.DrawRay(origin + Vector3.right * .1f, direction * castDistance, Color.blue, 1.5f);

            #endregion

            if (boundsHit.Length > 0)
            {
                // Sorting hits from first hit to last hit
                if (boundsHit.Length > 1) boundsHit = boundsHit.OrderBy(x => x.distance).ToArray();

                Vector3 originToParent = (parent.position - origin);

                // Defining root hit point
                RaycastHit originHit;
                float lineAddDistance = /*Vector3.Scale(*/Vector3.Scale(info.PrefabReference.ReferenceBounds.extents, parent.lossyScale).y/*, dirNorm)*/;
                Physics.Raycast(new Ray(origin, direction), out originHit, boundsHit[0].distance + ReferenceBounds.extents.magnitude + lineAddDistance, RayCheckLayer, QueryTriggerInteraction.Ignore);
                //UnityEngine.Debug.DrawRay(origin-Vector3.right * 0.02f, direction * (boundsHit[0].distance + ReferenceBounds.extents.magnitude + lineAddDistance), Color.green, 1.01f);

                Vector3 placementNormal = boundsHit[0].normal;
                Vector3 lineHit = originHit.point;
                RaycastHit fullLineHit = new RaycastHit();

                float dist = boundsHit[0].distance;
                if (originHit.transform)
                {
                    Vector3 boundsOffset = Vector3.Scale(info.PrefabReference.GameObject.transform.lossyScale, Vector3.Scale(info.PrefabReference.ReferenceBounds.extents, direction));
                    dist = originHit.distance - lineAddDistance;// boundsOffset.magnitude;
                }

                Vector3 placementOrigin = parent.position + (direction * dist) + parent.transform.TransformVector(info.GetPositionOffsetLocal());

                if (originHit.transform == null)
                {
                    originHit = boundsHit[0];
                    Physics.Raycast(new Ray(origin, direction), out fullLineHit, castDistance, RayCheckLayer, QueryTriggerInteraction.Ignore);
                }
                else
                {
                    Physics.Raycast(new Ray(origin, direction), out fullLineHit, originHit.distance * 1.001f, RayCheckLayer, QueryTriggerInteraction.Ignore);

                    if (fullLineHit.transform != null) originHit = fullLineHit;
                    placementNormal = originHit.normal;
                }

                // Placement position detected but let's check if nothing overlaps here
                if (fullLineHit.transform)
                {
                    if (CheckOverlapping(info, parent, fullLineHit))
                    {
                        var rData = new PlacementVolumeRaycastingData();
                        rData.OverrideInfo = "Overlapping! Consider disabling overlapping check.";
                        return rData;
                    }
                }

                return new PlacementVolumeRaycastingData(parent, info, originHit, boundsHit, lineHit, direction, placementOrigin, placementNormal, fullLineHit);
            }

            return new PlacementVolumeRaycastingData();
        }

        static Collider[] overlapsBuffer = new Collider[2];
        bool CheckOverlapping(ObjectStamperEmittedInfo info, Transform parent, RaycastHit originHit)
        {
            if (OverlapCheckMask != 0)
                if (OverlapCheckScale > 0f)
                {
                    Vector3 center = originHit.point + (originHit.normal * info.PrefabReference.ReferenceBounds.extents.y * 1.01f);
                    Physics.SyncTransforms();

                    Quaternion overlapBoxRot;// = parent.rotation * info.RotOffset;
                    overlapBoxRot = Quaternion.FromToRotation(Vector3.up, originHit.normal);
                    overlapBoxRot *= parent.rotation * info.RotOffset;

                    int overlaps = Physics.OverlapBoxNonAlloc(center, info.PrefabReference.ReferenceBounds.extents * OverlapCheckScale, overlapsBuffer,
                        overlapBoxRot);//, ~(0<<0), QueryTriggerInteraction.Ignore);

                    //Debug.DrawRay(center, (parent.rotation * info.RotOffset) * Vector3.forward, Color.blue, 0.01f);
                    //Debug.DrawRay(center, overlapBoxRot * Vector3.forward, Color.green, 0.01f);
                    //Bounds bCheck = new Bounds(center, info.PrefabReference.ReferenceBounds.extents * OverlapCheckScale * 2f);
                    //FDebug.DrawBounds3D(bCheck, Color.yellow, 1f);

                    if (overlaps > 0)
                    {
                        //Bounds bCheck = new Bounds(center, info.PrefabReference.ReferenceBounds.extents * OverlapCheckScale * 2f);
                        //UnityEngine.Debug.DrawRay(center, (parent.rotation * info.RotOffset) * Vector3.forward, Color.yellow, 1.1f);
                        //FDebug.DrawBounds3D(bCheck, Color.yellow, 1f);
                        //FDebug.DrawBounds3D(overlapsBuffer[0].bounds, Color.red, 1f);
                        return true;
                    }
                }

            return false;
        }


        /// <summary>
        /// Short: Checking if stamper info can be emitted in target raycasted placement volume.
        /// Checking StamperSet restrictions for spawner, returning RaycastCheckResult with 'Allow' setted to false with cause at 'Info' if can't spawn in this raycasting area
        /// </summary>
        public RaycastingRestrictionsCheckResult CheckRestrictionsOn(PlacementVolumeRaycastingData volumeData, bool checkStandPlace = true)
        {
            if (volumeData.mainHit.transform == null)
            {
                string info = "No Hits";
                if (string.IsNullOrEmpty(volumeData.OverrideInfo) == false) info = volumeData.OverrideInfo;
                return new RaycastingRestrictionsCheckResult(false, info, volumeData.mainHit);
            }

            #region Tagging check ------------------


            for (int i = 0; i < AllowJustOnTags.Count; i++)
                if (volumeData.mainHit.transform.gameObject.CompareTag(AllowJustOnTags[i]) == false) return new RaycastingRestrictionsCheckResult(false, "Hitted tag which is not in allowed list (" + volumeData.mainHit.transform.gameObject.tag + ")", volumeData.mainHit);

            for (int i = 0; i < DisallowOnTags.Count; i++)
                if (volumeData.mainHit.transform.gameObject.CompareTag(DisallowOnTags[i]) == true) return new RaycastingRestrictionsCheckResult(false, "Hitted tag which is not allowed (" + volumeData.mainHit.transform.gameObject.tag + ")", volumeData.mainHit);


            #endregion


            #region Stamp restriction check ------------------


            if (StampRestriction != EOSRaystriction.None)
            {
                OStampStigma stigma = volumeData.mainHit.transform.GetComponentInChildren<OStampStigma>();
                if (stigma == null) stigma = volumeData.mainHit.transform.GetComponentInParent<OStampStigma>();

                switch (StampRestriction)
                {
                    case EOSRaystriction.AvoidAnyOtherStamper:
                        if (stigma != null) return new RaycastingRestrictionsCheckResult(false, "Hitted other stamper and it's not allowed", volumeData.mainHit);
                        break;

                    case EOSRaystriction.AllowStackOnSelected:

                        bool anyFound = false;
                        if (stigma != null)
                            for (int i = 0; i < RestrictionSets.Count; i++)
                            {
                                if (stigma.ReferenceSet == RestrictionSets[i]) { anyFound = true; break; }
                            }

                        if (anyFound)
                            if (volumeData.raycastHits != null) // Checking stacking count
                            {
                                int stacked = 0;
                                for (int i = 0; i < volumeData.raycastHits.Length; i++)
                                {
                                    OStampStigma othersStigma = volumeData.raycastHits[i].transform.GetComponentInChildren<OStampStigma>();
                                    if (othersStigma == null) continue;

                                    if (othersStigma.ReferenceSet == this) { stacked++; }
                                    if (stacked > PlacementLimitCount) { return new RaycastingRestrictionsCheckResult(false, "Too much objects stacked one on another", volumeData.mainHit); }
                                }

                                //if (anyFound == false) return new RaycastCheckResult(false, "Not found stack to place this object on");
                            }

                        break;

                    case EOSRaystriction.DisallowOnlyOnSelected:

                        if (stigma != null)
                            for (int i = 0; i < RestrictionSets.Count; i++)
                            {
                                if (stigma.ReferenceSet == RestrictionSets[i])
                                    return new RaycastingRestrictionsCheckResult(false, "Trying to stack on not allowed stamper", volumeData.mainHit);
                            }

                        break;
                }

            }


            #endregion


            #region Placement Check ------------------


            if (MaxSlopeAngle < 90)
            {
                float placementAngle = Vector3.Angle(volumeData.customNormal, -volumeData.raycastingDirection);
                //Debug.Log("Hit angle = " + placementAngle);
                if (placementAngle > MaxSlopeAngle) return new RaycastingRestrictionsCheckResult(false, "Placement angle too big (" + placementAngle + ")", volumeData.mainHit);
            }

            //bool cSetted = false;

            if (checkStandPlace)
                if (MinimumStandSpace > 0f)
                {

                    bool centered = false;
                    if (volumeData.lineCastPoint != Vector3.zero)
                    {
                        if (Vector3.Distance(volumeData.lineCastPoint, volumeData.mainHit.point) < volumeData.emittedInfo.PrefabReference.ReferenceBoundsFull.extents.magnitude * 0.05f)
                            centered = true;
                    }

                    if (centered) { } // If centered that means boxcast hit is in target position center correct
                    else // else we have to find hit point on ground nearest to centered raycast position
                    {
                        // Checking raycasts from center point towards target hit until hit to define start ground position with better precision
                        Vector3 fromCenterToHoldPoint = volumeData.raycastHits[0].point - volumeData.customPoint;
                        float fromCtoHDist = fromCenterToHoldPoint.magnitude;
                        float step = 1f / 10f;
                        float refDist = fromCtoHDist * 0.1f;
                        RaycastHit groundTestHit = new RaycastHit();

                        for (int i = 1; i < 9; i++)
                        {
                            Vector3 origin = Vector3.Lerp(volumeData.customPoint, volumeData.raycastHits[0].point, step * i);
                            //Debug.DrawLine(origin + Vector3.up, origin);
                            if (Physics.Raycast(origin - volumeData.raycastingDirection * refDist, volumeData.raycastingDirection, out groundTestHit, refDist * 1.05f, RayCheckLayer, QueryTriggerInteraction.Ignore))
                            {
                                //Debug.DrawLine(origin + Vector3.up, origin, Color.red);
                                break;
                            }
                        }

                        Vector3 targetPlacementPoint;

                        if (groundTestHit.transform)
                        {
                            targetPlacementPoint = groundTestHit.point;
                            volumeData.customNormal = groundTestHit.normal;
                            volumeData.mainHit = groundTestHit;
                        }
                        else
                            targetPlacementPoint = volumeData.raycastHits[0].point;

                        //Debug.DrawRay(volumeData.mainHit.point, Vector3.up);

                        Matrix4x4 spaceCheckMatrix = Matrix4x4.TRS(volumeData.customPoint /*+ volumeData.emitter.TransformVector(volumeData.emittedInfo.PrefabReference.ReferenceBoundsFull.center)*/, volumeData.emitter.transform.rotation * volumeData.emittedInfo.RotOffset, volumeData.emittedInfo.PrefabReference.ReferenceBoundsFull.extents);
                        Vector3 inBoundsLocal = spaceCheckMatrix.inverse.MultiplyPoint(targetPlacementPoint);

                        if (Mathf.Abs(inBoundsLocal.x) > 1f - MinimumStandSpace)
                            return new RaycastingRestrictionsCheckResult(false, "Not enough stand space in x (local:" + (inBoundsLocal) + ")", volumeData.mainHit);
                        else
                        if (Mathf.Abs(inBoundsLocal.z) > 1f - MinimumStandSpace)
                            return new RaycastingRestrictionsCheckResult(false, "Not enough stand space in z (local:" + (inBoundsLocal) + ")", volumeData.mainHit);
                        else
                        if (Mathf.Abs(inBoundsLocal.y) > 1f - MinimumStandSpace)
                            return new RaycastingRestrictionsCheckResult(false, "Not enough stand space in y (local:" + (inBoundsLocal) + ")", volumeData.mainHit);

                        if (groundTestHit.transform) { volumeData.customPoint = groundTestHit.point; /*cSetted = true;*/ }
                    }

                }

            #endregion


            return new RaycastingRestrictionsCheckResult(true, string.Empty, volumeData.mainHit, volumeData.customPoint);

        }

        public RaycastingRestrictionsCheckResult CheckOverlapOnFullLineCast(ObjectStamperEmittedInfo info, PlacementVolumeRaycastingData volumeData)
        {
            if (volumeData.backupFullLineCast.transform == null) return new RaycastingRestrictionsCheckResult(false, "No Full Ray Hit", volumeData.backupFullLineCast);

            volumeData.mainHit = volumeData.backupFullLineCast;
            volumeData.customPoint = volumeData.backupFullLineCast.point;
            volumeData.lineCastPoint = volumeData.backupFullLineCast.point;
            volumeData.customNormal = volumeData.backupFullLineCast.normal;
            volumeData.raycastHits = null;

            RaycastingRestrictionsCheckResult restrictionsOnBackup = CheckRestrictionsOn(volumeData, false);
            Vector3 center = volumeData.backupFullLineCast.point - volumeData.raycastingDirection * info.PrefabReference.GetScaledBoundsExt(volumeData.emitter.lossyScale, volumeData.raycastingDirection).magnitude;
            Vector3 boxHalfDimensions = info.PrefabReference.GetScaledBoundsExt(volumeData.emitter.lossyScale);

            Quaternion checkRotation = volumeData.emittedInfo.GetRotationOn(volumeData.emitter, volumeData.backupFullLineCast.normal);
            Vector3 checkPosition = volumeData.emittedInfo.GetWorldOffsetOnHit(volumeData.emitter, volumeData.backupFullLineCast, volumeData.backupFullLineCast.point);

            Collider[] overlaps = Physics.OverlapBox(center + checkPosition, boxHalfDimensions * 0.999f, checkRotation, RayCheckLayer, QueryTriggerInteraction.Ignore);

            //Bounds ovBox = new Bounds(center + checkPosition, boxHalfDimensions * 2f);
            //FDebug.DrawBounds3D(ovBox, Color.red, 1f);
            //Debug.DrawRay(center + checkPosition, boxHalfDimensions, Color.cyan, 1.1f);
            //Debug.DrawRay(center + checkPosition, -boxHalfDimensions, Color.cyan, 1.1f);

            bool mustBreak = false;
            OStamperSet setRef = volumeData.emittedInfo.SetReference;

            if (setRef.IgnoreCheckOnTags.Count > 0)
                for (int i = 0; i < overlaps.Length; i++)
                {
                    if (setRef.IgnoreCheckOnTags.Contains(overlaps[i].transform.tag)) continue;
                    mustBreak = true;
                    break;
                }

            //#region Debugging

            //if (overlaps.Length > 0) Debug.DrawRay(overlaps[0].transform.position, Vector3.one, Color.yellow);
            //for (int i = 0; i < overlaps.Length; i++)
            //{
            //    Debug.Log("overl " + overlaps[i].name);
            //}

            //#endregion

            if (mustBreak)
                return new RaycastingRestrictionsCheckResult(false, "No free space in raycast area", volumeData.backupFullLineCast);
            else
                return CheckRestrictionsOn(volumeData, false); // Checking modified volume
        }


    }
}