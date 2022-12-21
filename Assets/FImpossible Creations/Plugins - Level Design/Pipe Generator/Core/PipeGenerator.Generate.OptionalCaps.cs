using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class PipeGenerator
    {

        void OptionalUnfinishedCapsApply()
        {

            // Adding unfinished prefab caps if provided -----
            if (PresetData.OptionalUnended.Enabled)
                if (PresetData.OptionalUnended.Prefab != null)
                {
                    if (!ended || !AlignFinish || endAlignSpawns.Count == 0)
                    {
                        if (pathSpawns.Count > 1)
                        {
                            PipeSpawnData spawn = pathSpawns[pathSpawns.Count - 1];
                            PipeSpawnData capSpawn = GetSpawnFittingTo(spawn, PresetData.OptionalUnended, false);
                            if (capSpawn != null) allSpawns.Add(capSpawn);
                        }
                    }

                    if (PresetData.AllowUseUnendedOnStartAlign)
                    {
                        if (startAlignFinderHit.transform == null || startAlignSpawns.Count == 0)
                            if (pathSpawns.Count > 1)
                            {
                                PipeSpawnData spawn = new PipeSpawnData();
                                AssignSpawnDataTo(spawn, null, new PipeSegmentSetup.JoinPoint(), transform.position, Quaternion.LookRotation(-transform.forward, transform.up));
                                latestSpawn = spawn;
                                PipeSpawnData capSpawn = GetSpawnFittingTo(spawn, PresetData.OptionalUnended, false);
                                if (capSpawn != null) allSpawns.Add(capSpawn);
                            }
                    }
                }
        }


        void OptionalFinishedCapsApply()
        {

            // Adding finished prefab caps if provided ----
            if (PresetData.OptionalEndCap.Enabled)
                if (PresetData.OptionalEndCap.Prefab != null)
                {
                    if (startAlignHelperHit.transform)
                    {
                        PipeSpawnData spawn = new PipeSpawnData();
                        AssignSpawnDataTo(spawn, null, new PipeSegmentSetup.JoinPoint(), startAlignHelperHit.point, PresetData.AlignOnHitNormal ? Quaternion.FromToRotation(Vector3.forward, startAlignHelperHit.normal ) : startAlignHelperHitSpawn.Rotation);
                        PipeSpawnData capSpawn = GetSpawnFittingTo(spawn, PresetData.OptionalEndCap, true);
                        if (capSpawn != null) allSpawns.Add(capSpawn); //else UnityEngine.Debug.Log("Cap not fitting");
                    }

                    if (endAlignHelperHit.transform)
                    {
                        PipeSpawnData spawn = new PipeSpawnData();
                        AssignSpawnDataTo(spawn, null, new PipeSegmentSetup.JoinPoint(), endAlignHelperHit.point, PresetData.AlignOnHitNormal ? Quaternion.FromToRotation(Vector3.forward, endAlignHelperHit.normal) : endAlignHelperHitSpawn.Rotation);
                        //AssignSpawnDataTo(spawn, null, new PipeSegmentSetup.JoinPoint(), endAlignHelperHit.point, endAlignHelperHitSpawn.Rotation);
                        PipeSpawnData capSpawn = GetSpawnFittingTo(spawn, PresetData.OptionalEndCap, true);
                        if (capSpawn != null) allSpawns.Add(capSpawn);
                    }
                }
        }


        PipeSpawnData GetSpawnFittingTo(PipeSpawnData parent, PipeSegmentSetup seg, bool reverse = false)
        {
            Vector3 startNormal = (parent.Rotation * parent.Join.outAxis).normalized;
            Vector3 startOrigin = parent.OutJoinPoint;
            Vector3 endNormal = reverse ? -parent.JoinOutDir : parent.JoinOutDir;
            PipeSpawnData spawn = new PipeSpawnData();

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
                            PipeSegmentSetup.JoinPoint nearestJ;
                            // Reversed check
                            nearestJ = GetSegmentFittingJoinTo(seg, null, rotation, endNormal, 1f, true);

                            if (nearestJ != null)
                            {
                                AssignSpawnDataTo(spawn, seg, nearestJ, joinSnapPosition, rotation);
                            }
                        }

                        if (seg.AllowRotationZAxisCheckPer == 0) break; // if zero then just one rotation
                    }

                    if (seg.AllowRotationYAxisCheckPer == 0) break; // if zero then just one rotation
                }
            }

            if (spawn.ToCreate == null) return null;

            return spawn;
        }

    }
}
