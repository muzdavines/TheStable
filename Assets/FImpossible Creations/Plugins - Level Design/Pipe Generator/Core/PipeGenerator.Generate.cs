using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class PipeGenerator
    {
        public bool AreGeneratedObjects { get { return generated.Count > 0; } }
        [SerializeField][HideInInspector] private List<GameObject> generated = new List<GameObject>();
        public List<PipeSpawnData> SpawnData { get { return allSpawns; } }

        readonly List<PipeSpawnData> allSpawns = new List<PipeSpawnData>();
        readonly List<PipeSpawnData> pathSpawns = new List<PipeSpawnData>();
        readonly List<PipeSpawnData> startAlignSpawns = new List<PipeSpawnData>();
        readonly List<PipeSpawnData> endAlignSpawns = new List<PipeSpawnData>();

        PipeSpawnData rootSpawn;
        PipeSpawnData latestSpawn;
        bool ended;
        bool breakGenerating = false;

        #region Debugging coroutine

        IEnumerator IEPreviewGenerationDebug()
        {
            ClearGenerated();
            PreparePathFind(transform.position, transform.rotation);

            for (int i = 0; i < MaxTries; i++)
            {
                while (true)
                {
                    yield return null;

                    if (Input.GetKeyDown(KeyCode.N))
                    {
                        yield return null;
                        break;
                    }
                }

                var spawn = PathNextSegmentTowards(latestSpawn, EndPosition, pathSpawns.Count >= FirstSegmentsWithoutCollision);

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
                        break;
                    }
                }
            }

            yield break;
        }

        #endregion


        public void PipePreviewGeneration()
        {
            ClearGenerated();

            if (AlignStartOn != 0) DiveStartAlign();

            PathFindFromTo(transform.position, transform.rotation, EndPosition);

            if (ended) if (AlignFinish) DiveEndAlign();

            OptionalUnfinishedCapsApply();

            OptionalFinishedCapsApply();

            CheckFirstSegmentOverlap();
        }

        bool CheckFirstSegmentOverlap(bool removeFirst = true)
        {
            if (allSpawns.Count < 2) return false;
            Bounds a = allSpawns[0].RotatedBounds;
            Bounds b = allSpawns[1].RotatedBounds;
            a.size *= 0.8f;

            if (a.Intersects(b))
            {
                allSpawns.RemoveAt(0);
                return true;
            }

            return false;
        }

        bool AddToIgnore(PipeSpawnData data)
        {
            if (data == null) return false;

            for (int i = 0; i < pathFindIgnoreSpecific.Count; i++)
            {
                var ign = pathFindIgnoreSpecific[i];

                if (//ign.Join == data.Join &&
                 ign.Position == data.Position &&
                Quaternion.Angle(ign.Rotation, data.Rotation) < 1f) //ign.Rotation == data.Rotation)
                {
                    return false; // Already contains it
                }

            }

            pathFindIgnoreSpecific.Add(data);
            return true;
        }


        #region Backup

        //private void StretchSegmentTowards(PipeSpawnData newFinishSpawn, Vector3 targetPos)
        //{
        //    if (newFinishSpawn == null) return;

        //    //Vector3 towards = targetPos - newFinishSpawn.OutJoinPointReverse;
        //    Matrix4x4 orientation = Matrix4x4.TRS(newFinishSpawn.OutJoinPointReverse, newFinishSpawn.Rotation, newFinishSpawn.Scale);
        //    Debug.DrawRay(newFinishSpawn.OutJoinPointReverse, Vector3.up, Color.yellow, 0.1f);

        //    float distance = orientation.inverse.MultiplyPoint(targetPos).z;
        //    float distanceABS = Mathf.Abs(distance);

        //    Vector3 outDir = newFinishSpawn.Join.outAxis.normalized;
        //    newFinishSpawn.Scale = Vector3.one + outDir * (distanceABS * newFinishSpawn.ParentSegment.BoundsSizeOnAxis(outDir));
        //    newFinishSpawn.Position += newFinishSpawn.JoinOutDir * -distanceABS * 0.5f;

        //    UnityEngine.Debug.Log("zDist = " + distance);
        //}

        #endregion


        public void GenerateObjects()
        {
            PipePreviewGeneration();

            if (DontGenerateIfNotEnded)
            {
                if (ended == false) return;
                if (AlignFinish) if (diveFinished == false) return;
            }

            for (int i = 0; i < allSpawns.Count; i++)
            {

                GameObject cr = FGenerators.InstantiateObject(allSpawns[i].ToCreate.Prefab);
                cr.transform.position = allSpawns[i].Position;
                cr.transform.rotation = allSpawns[i].Rotation;
                cr.transform.localScale = allSpawns[i].Scale;
                cr.transform.SetParent(transform, true);
                generated.Add(cr);
            }
        }


        public void ClearGenerated()
        {
            breakGenerating = false;
            for (int i = 0; i < generated.Count; i++) if (generated[i] != null) FGenerators.DestroyObject(generated[i]);
            generated.Clear();

            allSpawns.Clear();
            pathSpawns.Clear();
            startAlignSpawns.Clear();
            endAlignSpawns.Clear();

            pathFindIgnoreSpecific.Clear();

            startAlignHelperHit = new RaycastHit();
            startAlignHelperHitSpawn = null;
            startAlignFinderHit = new RaycastHit();

            endAlignHelperHit = new RaycastHit();
            endAlignHelperHitSpawn = null;

            ended = false;
            boxCastHalfScale = transform.lossyScale * BoxcastScale * 0.5f;
        }


        void AssignSpawnDataTo(PipeSpawnData data, PipeSegmentSetup seg, PipeSegmentSetup.JoinPoint join, Vector3 pos, Quaternion rot)
        {
            data.SetToCreate(seg);
            data.ParentSegment = seg;
            data.Join = join;
            data.Position = pos;
            data.Rotation = rot;
        }

    }
}
