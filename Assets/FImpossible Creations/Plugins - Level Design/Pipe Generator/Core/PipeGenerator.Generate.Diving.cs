using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class PipeGenerator
    {
        int diveReach = 0;
        bool diveFinishing = false;
        bool diveFinished = false;
        PipeSpawnData diveSpawn = null;

        private void ResetDiving()
        {
            diveReach = 0;
            diveFinishing = false;
            diveFinished = false;
        }

        Vector3 diveReachStartPos;
        Vector3 diveReachStartDir;
        private void StartFinishingDiving(Vector3 pos, Vector3 dir)
        {
            diveReach = 1;
            diveReachStartPos = pos;
            diveReachStartDir = dir;
        }


        /// <summary>
        /// Finding way towards target point without main restrictions
        /// </summary>
        PipeSpawnData DiveNextSegmentTowards(PipeSpawnData parent, Vector3 targetPoint, Vector3 endNormal, bool useCollision)
        {
            //if (diveFinish) { /*UnityEngine.Debug.Log("Diving Ended");*/ return null; } // Diving ended

            // Prepare helper variables
            Vector3 startNormal = (parent.Rotation * parent.Join.outAxis).normalized;
            Vector3 startOrigin = parent.OutJoinPoint;
            endNormal.Normalize();

            PipeSpawnData spawn = new PipeSpawnData();
            diveSpawn = null;
            spawn.Parent = parent;

            float nearest = float.MaxValue;
            latestComputedDistance = float.MaxValue;

            // Checking all prepared segments prefabs
            for (int s = 0; s < PresetData.Segments.Count; s++)
            {
                PipeSegmentSetup seg = PresetData.Segments[s];
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
                            #endregion

                            Quaternion rotation = parent.Rotation * Quaternion.Euler(0f, rotY, rotZ);
                            Vector3 jDir = rotation * join.origin;
                            Vector3 joinSnapPosition = startOrigin - jDir;

                            // Check if join and rotation fits to start attachement point
                            if (IsFittingTo(rotation, startNormal, join.outAxis))
                            {
                                // Checking all other join points on this segment rotation to find 
                                // how big distance will remain to target with this snap 
                                // This join will be free join to continue chain

                                PipeSegmentSetup.JoinPoint nearestJ;

                                bool diver = false;
                                if (diveReach == 0)
                                {
                                    float distanceTo = Vector3.Distance(joinSnapPosition, targetPoint);
                                    if (distanceTo < MaxDistanceToEnding) diver = true;
                                }
                                else diver = true;

                                if (diver)
                                {
                                    nearestJ = GetSegmentNearestFittingJoinTo(seg, join, joinSnapPosition, rotation, targetPoint, endNormal, 1f);
                                }
                                else
                                    nearestJ = GetSegmentNearestJoinTo(seg, join, joinSnapPosition, rotation, targetPoint);

                                if (nearestJ != null)
                                {
                                    if (latestComputedDistance + seg.UseCost < nearest)
                                    // Checking if choosed join don't collide with other segments
                                    //if (CheckIfCollidesWithSelfSegment(seg, join, joinSnapPosition, rotation) == false || useCollision == false)
                                    {
                                        AssignSpawnDataTo(spawn, seg, nearestJ, joinSnapPosition, rotation);
                                        nearest = latestComputedDistance + seg.UseCost;

                                        if (parent != null) parent.Child = spawn;
                                        if (diver) diveSpawn = spawn;
                                    }
                                }
                            }

                            if (seg.AllowRotationZAxisCheckPer == 0) break; // if zero then just one rotation
                        }

                        if (seg.AllowRotationYAxisCheckPer == 0) break; // if zero then just one rotation
                    }
                }
            }

            if (spawn.ToCreate != null)
            {
                if (diveSpawn == spawn)
                {
                    if (diveReach == 0)
                        StartFinishingDiving(spawn.Position, spawn.Rotation * spawn.Join.outAxis);
                    else
                    {
                        Vector3 joinToTarget = (targetPoint - spawn.Position).normalized;

                        float dot = Vector3.Dot(diveReachStartDir, joinToTarget);
                        if (dot <= 0.1f)
                        {
                            diveFinishing = true;
                        }
                        else diveReach++;
                    }
                }

                return spawn;
            }
            else return null;
        }


        RaycastHit endAlignHelperHit;
        PipeSpawnData endAlignHelperHitSpawn;
        void DiveEndAlign()
        {
            ResetDiving();
            endAlignHelperHit = new RaycastHit();
            endAlignHelperHitSpawn = null;
            float nearestEndingHit = float.MaxValue;

            // Proceed diving procedure towards finishing desired target point
            for (int i = 0; i < MaxTries; i++)
            {
                #region Check if object is going too far against start point

                if (AlignScaleForFinishingSegments)
                {
                    if (endAlignHelperHit.transform)
                    {
                        Vector3 outToAlign = latestSpawn.OutJoinPoint - endAlignHelperHit.point;
                        float dot = Vector3.Dot((outToAlign).normalized, -EndDirection);

                        if (dot <= -0.5f)
                        {
                            // Scaling finish segment to not go through wall
                            float diff = outToAlign.magnitude;

                            float distanceRev = Vector3.Distance(latestSpawn.OutJoinPointReverse, endAlignHelperHit.point);
                            if (distanceRev < diff)
                            //if (distanceRev > latestSpawn.PreviewMesh.bounds.size.magnitude * 0.1f)
                            {
                                Vector3 outDir = latestSpawn.Join.outAxis.normalized;
                                latestSpawn.Scale = Vector3.one + outDir * (diff * latestSpawn.ParentSegment.BoundsSizeOnAxis(outDir));
                                latestSpawn.Position += 0.5f * -diff * latestSpawn.JoinOutDir;
                                //if (startAlignSpawns.Count == 1) latestSpawn.Enabled = false;
                            }
                            else
                            {
                                latestSpawn.Enabled = false;
                            }

                            // Tag finish
                            diveFinishing = true;
                            diveFinished = true;
                            break;
                        }
                    }
                }

                #endregion



                var spawn = DiveNextSegmentTowards(latestSpawn, EndPosition, EndDirection, endAlignSpawns.Count > 0);

                if (spawn != null)
                {
                    endAlignSpawns.Add(spawn);
                    allSpawns.Add(spawn);

                    #region Helper Start-End Hit raycasting

                    // Detecting end align hit for caps creation etc.
                    if (spawn.Parent != null)
                    {
                        if (AlignFinishOptionalsOn != 0)
                        {
                            //Vector3 dir = spawn.OutJoinPoint - latestSpawn.OutJoinPoint;
                            //Vector3 start = Vector3.Lerp(latestSpawn.OutJoinPoint, spawn.OutJoinPoint, 0.5f);
                            //RaycastHit hit = RaycastGetHit(start, spawn.OutJoinPoint + dir * 0.5f, AlignFinishOptionalsOn);
                            RaycastHit hit = RaycastGetHit(latestSpawn.OutJoinPoint, spawn.OutJoinPoint, AlignFinishOptionalsOn);
                            if (hit.transform)
                            {
                                if (ChooseNearestSegmentChainHit(ref endAlignHelperHit, ref nearestEndingHit, hit, EndPosition))
                                    endAlignHelperHitSpawn = spawn;
                            }
                            else
                            {
                                //UnityEngine.Debug.Log("No Hit " );
                            }
                        }
                    }

                    #endregion

                    latestSpawn = spawn;
                }
                else break;

                if (diveFinishing && diveFinished) break;
                if (diveFinishing) diveFinished = true;
            }
        }

        RaycastHit startAlignFinderHit;
        RaycastHit startAlignHelperHit;
        PipeSpawnData startAlignHelperHitSpawn;
        void DiveStartAlign()
        {
            #region Prepare raycast point for start alignment

            // Getting nearest hit position for initial aligning
            RaycastHit shortest = new RaycastHit();
            float shortVal = float.MaxValue;

            for (int i = 0; i < AlignStartDirections.Length; i++)
            {
                RaycastHit hit = RaycastGetHit(transform.position, transform.TransformDirection(AlignStartDirections[i]), AlignStartMaxDistance, AlignStartOn);
                if (hit.transform)
                {
                    if (hit.distance < shortVal)
                    {
                        shortVal = hit.distance;
                        shortest = hit;
                    }
                }
            }

            startAlignFinderHit = shortest;

            #endregion

            if (startAlignFinderHit.transform == null) return;

            // Initialize Diving
            ResetDiving();
            startAlignHelperHitSpawn = null;
            float nearestStartingHit = float.MaxValue;
            latestSpawn = new PipeSpawnData();
            AssignSpawnDataTo(latestSpawn, null, new PipeSegmentSetup.JoinPoint(), transform.position, Quaternion.LookRotation(-transform.forward, transform.up));
            Vector3 alignNormal = FlattenNormal(Quaternion.FromToRotation(Vector3.forward, -startAlignFinderHit.normal));

            // Proceed segments spawning
            for (int i = 0; i < MaxTries; i++)
            {
                #region Check if object is going too far against start point

                Vector3 outToAlign = latestSpawn.OutJoinPoint - startAlignFinderHit.point;
                float dot = Vector3.Dot((outToAlign).normalized, -alignNormal);

                if (dot <= -0.5f)
                {
                    // Scaling finish segment to not go through wall
                    float diff = outToAlign.magnitude;

                    Vector3 outDir = latestSpawn.Join.outAxis.normalized;
                    float distanceRev = Vector3.Distance(latestSpawn.OutJoinPointReverse, startAlignFinderHit.point);

                    if (distanceRev < diff)
                    //if (distanceRev > latestSpawn.PreviewMesh.bounds.size.magnitude * 0.1f)
                    {
                        latestSpawn.Scale = Vector3.one - outDir * (diff * latestSpawn.ParentSegment.BoundsSizeOnAxis(outDir));
                        latestSpawn.Position += 0.5f * -diff * latestSpawn.JoinOutDir;
                        //if (startAlignSpawns.Count == 1) latestSpawn.Enabled = false;
                    }
                    else
                    {
                        latestSpawn.Enabled = false;
                    }

                    //float measureDist = Mathf.Min(distanceRev, diff);

                    //latestSpawn.Scale = Vector3.one + outDir * (measureDist * latestSpawn.ParentSegment.BoundsSizeOnAxis(outDir));
                    //UnityEngine.Debug.Log("dist = " + measureDist + " scale = " + latestSpawn.Scale);
                    //latestSpawn.Position += 0.5f * -measureDist * latestSpawn.JoinOutDir;

                    //latestSpawn.Position += 0.5f * Vector3.up;

                    //if (distanceRev < measureDist)
                    //    if (startAlignSpawns.Count >= 1) latestSpawn.Enabled = false;

                    // Tag finish
                    diveFinishing = true;
                    diveFinished = true;
                    break;
                }

                #endregion

                // Else go on start align diving

                var spawn = DiveNextSegmentTowards(latestSpawn, startAlignFinderHit.point, alignNormal, startAlignSpawns.Count > 0);

                if (spawn != null)
                {
                    startAlignSpawns.Add(spawn);
                    allSpawns.Add(spawn);

                    #region Helper Start-End Hit raycasting

                    if (spawn.Parent != null) if (AlignFinishOptionalsOn != 0)
                        {
                            //Debug.DrawLine(latestSpawn.OutJoinPoint, spawn.OutJoinPoint, Color.red, 1f);
                            RaycastHit hit = RaycastGetHit(latestSpawn.OutJoinPoint, spawn.OutJoinPoint, AlignFinishOptionalsOn);
                            if (hit.transform)
                                if (ChooseNearestSegmentChainHit(ref startAlignHelperHit, ref nearestStartingHit, hit, EndPosition))
                                    startAlignHelperHitSpawn = spawn;
                        }

                    #endregion

                    latestSpawn = spawn;
                }
                else break;

                if (diveFinishing && diveFinished) break;
                if (diveFinishing) diveFinished = true;
            }
        }

        bool ChooseNearestSegmentChainHit(ref RaycastHit varHit, ref float nearestDist, RaycastHit compareHit, Vector3 targetPoint)
        {
            float dist = Vector3.Distance(compareHit.point, targetPoint);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                varHit = compareHit;
                return true;
            }

            return false;
        }

    }
}
