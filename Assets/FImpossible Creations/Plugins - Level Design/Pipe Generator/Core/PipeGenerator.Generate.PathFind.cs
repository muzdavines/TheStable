using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class PipeGenerator
    {
        /// <summary> To ignore some joints when could not find da way, walkaround for A* like behaviour of path search </summary>
        readonly List<PipeSpawnData> pathFindIgnoreSpecific = new List<PipeSpawnData>();

        void PreparePathFind(Vector3 startPos, Quaternion startRot)
        {
            pathSpawns.Clear();
            rootSpawn = new PipeSpawnData();
            AssignSpawnDataTo(rootSpawn, null, new PipeSegmentSetup.JoinPoint(), startPos, startRot);
            latestSpawn = rootSpawn;
        }

        /// <summary>
        /// Finding way towards target point with main restrictions like collision, hold distance etc.
        /// </summary>
        public void PathFindFromTo(Vector3 startPos, Quaternion startRot, Vector3 finishPosition)
        {
            PreparePathFind(startPos, startRot);



            for (int i = 0; i < MaxTries; i++)
            {
                if (breakGenerating) return;
                //if (latestSpawn == null || latestSpawn.Join == null)
                //{
                //    UnityEngine.Debug.Log("[Pipe Generator] No Start Point!");
                //    return;
                //}

                var spawn = PathNextSegmentTowards(latestSpawn, finishPosition, pathSpawns.Count >= FirstSegmentsWithoutCollision);

                if (spawn == null) // If could not found step forward segment let's try different step on latest segment and replace it
                {
                    if (AddToIgnore(latestSpawn))
                    {
                        if (pathSpawns.Count > 0)
                        {
                            allSpawns.Remove(pathSpawns[pathSpawns.Count - 1]);
                            pathSpawns.RemoveAt(pathSpawns.Count - 1);
                        }

                        latestSpawn = latestSpawn.Parent;
                    }
                }
                else // Join segment found in first try
                {
                    pathSpawns.Add(spawn);
                    allSpawns.Add(spawn);

                    latestSpawn = spawn;

                    if (Vector3.Distance(spawn.OutJoinPoint, EndPosition) < MaxDistanceToEnding)
                    {
                        ended = true;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Checking all possibilities and finding point which is available to proceed and is nearest to desired point
        /// </summary>
        public PipeSpawnData PathNextSegmentTowards(PipeSpawnData parent, Vector3 target, bool useCollision = true)
        {
            if (parent == null || parent.Join == null)
            {
#if UNITY_EDITOR
                if (UnityEditor.Selection.Contains(gameObject.GetInstanceID()))
                    Debug.Log("[Pipe Generator] No Start Point!");
#endif
                breakGenerating = true;
                return null;
            }

            // Prepare helper variables
            Vector3 startNormal = (parent.Rotation * parent.Join.outAxis).normalized;
            Vector3 startOrigin = parent.OutJoinPoint; //GetSpawnStartPosition(parent);// parent.Position + parent.Rotation * parent.Join.origin;

            PipeSpawnData spawn = new PipeSpawnData();
            spawn.Parent = latestSpawn;

            int iteration = 0;
            float nearest = float.MaxValue;
            latestComputedDistance = float.MaxValue;

            // Checking all prepared segments prefabs
            for (int s = 0; s < PresetData.Segments.Count; s++)
            {
                PipeSegmentSetup seg = PresetData.Segments[s];
                if (pathSpawns.Count < seg.CanBeUsedSinceIteration) continue;
                if (seg.Enabled == false) continue;

                // Checking all join points of segment
                for (int j = 0; j < seg.JoinPoints.Length; j++)
                {
                    PipeSegmentSetup.JoinPoint join = seg.JoinPoints[j];

                    // Check all available rotations for segment on this join point
                    #region Rotation Fors
                    for (int rotY = 0; rotY < 360; rotY += seg.AllowRotationYAxisCheckPer)
                    {
                        for (int rotZ = 0; rotZ < 360; rotZ += seg.AllowRotationZAxisCheckPer)
                        {
                            iteration++;
                            #endregion

                            Quaternion rotation = parent.Rotation * Quaternion.Euler(0f, rotY, rotZ);
                            Vector3 joinSnapPosition = startOrigin - rotation * join.origin;


                            #region Ignoring unwanted joins

                            // Ignoring unwanted joins -> helpful when could not found pipe way in latest segment
                            bool ignore = false;
                            for (int ign = 0; ign < pathFindIgnoreSpecific.Count; ign++)
                            {
                                ///if (join == ignoreSpecific[ign].Join)
                                if (Quaternion.Angle(rotation, pathFindIgnoreSpecific[ign].Rotation) < 1f)// rotation == ignoreSpecific[ign].Rotation)
                                    if (joinSnapPosition == pathFindIgnoreSpecific[ign].Position)
                                    { /*UnityEngine.Debug.Log("ignore");*/ ignore = true; break; }
                            }

                            #endregion


                            if (!ignore)
                                if (AllowWithExtraConditions(parent, joinSnapPosition, seg, join, startNormal, rotation))
                                    if (CheckSegmentAvailableOnJoinPoint(join, startNormal, rotation))
                                    {
                                        // Checking all other join points to find how big distance will remain to target with this snap
                                        // This join will be free join to continue chain
                                        var nearestJ = GetSegmentNearestJoinTo(seg, join, joinSnapPosition, rotation, target);

                                        if (nearestJ != null)
                                        {
                                            // Checking if choosed join don't collide with anything
                                            Vector3 outJoinPosition = joinSnapPosition + rotation * nearestJ.origin;

                                            if (latestComputedDistance + seg.UseCost < nearest)
                                                if (CheckSegmentJoinCollision(startOrigin, outJoinPosition, useCollision) == false)
                                                    if (CheckIfCollidesWithSelfSegment(startOrigin, outJoinPosition) == false)
                                                    {
                                                        AssignSpawnDataTo(spawn, seg, nearestJ, joinSnapPosition, rotation);
                                                        nearest = latestComputedDistance + seg.UseCost;

                                                        if (latestSpawn != null) latestSpawn.Child = spawn;
                                                    }
                                        }
                                    }

                            if (seg.AllowRotationZAxisCheckPer == 0) break; // if zero then just one rotation
                        }

                        if (seg.AllowRotationYAxisCheckPer == 0) break; // if zero then just one rotation
                    }
                }
            }

            if (spawn.ToCreate != null) return spawn; else return null;
        }




        float latestComputedDistance;
        PipeSegmentSetup.JoinPoint GetSegmentNearestJoinTo(PipeSegmentSetup seg, PipeSegmentSetup.JoinPoint joinToIgnore, Vector3 joinSnappedPos, Quaternion rot, Vector3 target)
        {
            float nearest = float.MaxValue;
            PipeSegmentSetup.JoinPoint nearestJoin = null;

            for (int j = 0; j < seg.JoinPoints.Length; j++)
            {
                var oJoin = seg.JoinPoints[j];
                if (oJoin == joinToIgnore) continue;

                Vector3 origin = joinSnappedPos + rot * oJoin.origin;

                float distance = Vector3.Distance(origin, target);
                if (distance < nearest)
                {
                    nearest = distance;
                    latestComputedDistance = distance;
                    nearestJoin = oJoin;
                }
            }

            return nearestJoin;
        }

        PipeSegmentSetup.JoinPoint GetSegmentNearestFittingJoinTo(PipeSegmentSetup seg, PipeSegmentSetup.JoinPoint joinToIgnore, Vector3 joinSnappedPos, Quaternion rot, Vector3 target, Vector3 targetNormal, float tolerance = 1f)
        {
            float nearest = float.MaxValue;
            PipeSegmentSetup.JoinPoint nearestJoin = null;

            for (int j = 0; j < seg.JoinPoints.Length; j++)
            {
                var oJoin = seg.JoinPoints[j];
                if (oJoin == joinToIgnore) continue;

                Vector3 outDir = rot * oJoin.outAxis;
                if (tolerance >= 1f)
                {
                    if (outDir != targetNormal) continue;
                }
                else
                {
                    float dot = Vector3.Dot(targetNormal, outDir);
                    if (dot < tolerance) continue;
                }

                Vector3 origin = joinSnappedPos + rot * oJoin.origin;

                float distance = Vector3.Distance(origin, target);
                if (distance < nearest)
                {
                    nearest = distance;
                    latestComputedDistance = distance;
                    nearestJoin = oJoin;
                }
            }

            return nearestJoin;
        }


        PipeSegmentSetup.JoinPoint GetSegmentFittingJoinTo(PipeSegmentSetup seg, PipeSegmentSetup.JoinPoint joinToIgnore, Quaternion rot, Vector3 targetNormal, float tolerance = 1f, bool reverse = false)
        {
            float nearest = float.MinValue;
            PipeSegmentSetup.JoinPoint nearestJoin = null;

            for (int j = 0; j < seg.JoinPoints.Length; j++)
            {
                var oJoin = seg.JoinPoints[j];
                if (oJoin == joinToIgnore) continue;
                if (latestSpawn == null) return null;

                Vector3 outDir = rot * (oJoin.outAxis * (reverse ? -1f : 1f));

                Vector3 db = latestSpawn.OutJoinPoint;
                //Debug.DrawRay(db, outDir, Color.red, 1f);
                //Debug.DrawRay(db + Vector3.up * 0.1f, targetNormal, Color.green, 1f);

                float dot;
                if (tolerance >= 1f)
                {
                    if (outDir != targetNormal) continue;
                    dot = 1f;
                }
                else
                {
                    dot = Vector3.Dot(targetNormal, outDir);
                    if (dot < tolerance) continue;
                }

                if (dot >= 1f)
                {
                    return oJoin;
                }

                if (dot > nearest)
                {
                    nearest = dot;
                    nearestJoin = oJoin;
                }
            }

            return nearestJoin;
        }

        PipeSpawnData FindMostAccurateSegmentFor(PipeSpawnData parent, Vector3 targetPosition, Vector3 targetOutAxis)
        {
            if (parent == null) return null;

            // Choose most accurate segment to fit with ending point
            //float mostAccurateVal = float.MaxValue;
            PipeSpawnData mostAccurate = null;
            Vector3 parentOutPos = parent.OutJoinPoint;
            float mostAccurateDiff = float.MaxValue;

            // Checking all possible segment setups
            for (int i = 0; i < PresetData.Segments.Count; i++)
            {
                var seg = PresetData.Segments[i];
                // Checking all possible join points for this segment setup
                for (int j = 0; j < seg.JoinPoints.Length; j++)
                {
                    var join = seg.JoinPoints[j];

                    #region Rotation Fors
                    for (int rotY = 0; rotY < 360; rotY += seg.AllowRotationYAxisCheckPer)
                    {
                        for (int rotZ = 0; rotZ < 360; rotZ += seg.AllowRotationZAxisCheckPer)
                        {
                            #endregion

                            Quaternion rotation = Quaternion.Euler(0f, rotY, rotZ) * parent.Rotation;

                            // If this rotation fits to parent joint
                            if (CheckSegmentAvailableOnJoinPoint(join, parent.JoinOutDir, rotation))
                            {
                                // Then find join which rotation fits also to target desired normal
                                for (int n = 0; n < seg.JoinPoints.Length; n++)
                                {
                                    var oJoin = seg.JoinPoints[n];
                                    if (oJoin == join) continue;

                                    if ((rotation * oJoin.outAxis).normalized == targetOutAxis)
                                    {
                                        Vector3 joinEndPos = parentOutPos - rotation * join.origin;

                                        // Check if this is the most accurate in distance option
                                        float toTarget = Vector3.Distance(joinEndPos, targetPosition);

                                        if (toTarget < mostAccurateDiff)
                                        {
                                            mostAccurateDiff = toTarget;
                                            if (mostAccurate == null) mostAccurate = new PipeSpawnData();
                                            AssignSpawnDataTo(mostAccurate, seg, join, joinEndPos, rotation);
                                        }
                                    }
                                }

                            }

                            if (seg.AllowRotationZAxisCheckPer <= 0) break; // if zero then just one rotation
                        }

                        if (seg.AllowRotationYAxisCheckPer <= 0) break; // if zero then just one rotation
                    }
                }
            }

            //UnityEngine.Debug.Log("ToTarget = " + distanceToTarget + " vs " + );
            //Debug.DrawRay(snapOutPoint, current.JoinOutDir, Color.yellow, 0.1f);

            if (mostAccurate != null)
            {
                mostAccurate.Parent = parent;
                if (latestSpawn != null) latestSpawn.Child = mostAccurate;
            }

            return mostAccurate;
        }


        PipeSpawnData GetWithOriginFittingTo(PipeSpawnData parent, Vector3 desiredPosition, Vector3 targetOutAxis)
        {
            if (parent == null) return null;
            if (parent.Parent == null) return null;

            PipeSpawnData spawn = new PipeSpawnData();
            Vector3 parentOut = parent.JoinOutDir;
            Vector3 parentOrigin = parent.OutJoinPoint; //GetSpawnStartPosition(parent.Parent);
            targetOutAxis.Normalize();

            //Debug.DrawRay(parentOrigin, parentOut, Color.red, 1f);
            //Debug.DrawRay(originData.Position, originData.Rotation * originData.Join.outAxis, Color.red, 1f);

            float mostAccurate = float.MaxValue;

            // Checking all possible segment setups
            for (int i = 0; i < PresetData.Segments.Count; i++)
            {
                var seg = PresetData.Segments[i];

                // Checking all possible join points for this segment setup
                for (int j = 0; j < seg.JoinPoints.Length; j++)
                {
                    var join = seg.JoinPoints[j];
                    if (join == parent.Join) continue;

                    #region Rotation Fors
                    for (int rotY = 0; rotY < 360; rotY += seg.AllowRotationYAxisCheckPer)
                    {
                        for (int rotZ = 0; rotZ < 360; rotZ += seg.AllowRotationZAxisCheckPer)
                        {
                            #endregion

                            Quaternion rotation = Quaternion.Euler(0f, rotY, rotZ) * parent.Parent.Rotation;
                            float tolerance = 1f;

                            // If this rotation fits to parent joint
                            if (CheckSegmentAvailableOnJoinPoint(join, parentOut, rotation, tolerance))
                            {
                                // Then find join which rotation fits also to target desired normal
                                for (int n = 0; n < seg.JoinPoints.Length; n++)
                                {
                                    var oJoin = seg.JoinPoints[n];
                                    if (oJoin == join) continue;


                                    Vector3 dir = (rotation * oJoin.outAxis).normalized;
                                    float dot = Vector3.Dot(dir, targetOutAxis);

                                    if (dot >= tolerance)
                                    {
                                        Vector3 joinEndPos = parentOrigin - rotation * (join.origin);

                                        //Debug.DrawRay(joinEndPos, Vector3.up, Color.cyan, 5f);
                                        //Debug.DrawRay(parent.OutJoinPointReverse + rotation * (join.origin), Vector3.up, Color.red, 5f);
                                        //Debug.DrawRay(parent.OutJoinPoint + rotation * (join.origin), Vector3.up, Color.yellow, 5f);

                                        // Check if this is the most accurate in distance option
                                        float toTarget = Vector3.Distance(joinEndPos, desiredPosition);
                                        if (toTarget < mostAccurate)
                                        {
                                            mostAccurate = toTarget;
                                            AssignSpawnDataTo(spawn, seg, oJoin, joinEndPos, rotation);
                                        }
                                    }
                                }

                            }

                            if (seg.AllowRotationZAxisCheckPer <= 0) break; // if zero then just one rotation
                        }

                        if (seg.AllowRotationYAxisCheckPer <= 0) break; // if zero then just one rotation
                    }
                }
            }

            if (mostAccurate == float.MaxValue)
            {
                UnityEngine.Debug.Log("none");
                return null;
            }

            spawn.Parent = parent;

            return spawn;
        }


    }
}
