using FIMSpace.Generating.Checker;
using FIMSpace.Hidden;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Planning.GeneratingLogics
{

    public class SG_RandomTunnels : ShapeGeneratorBase
    {
        public override string TitleName() { return "Complex/Random Tunnels (Facility Example)"; }

        public MinMax BranchLength = new MinMax(5, 7);
        public MinMax TargetBranches = new MinMax(4, 5);
        public MinMax BranchThickness = new MinMax(1, 1);
        [Range(1, 25)] public int SeparationFactor = 5;
        public bool AvoidOverlaps = true;
        //public bool ThicknessSnap = true;

#if UNITY_EDITOR
        public override void OnGUIModify() { }
#endif

        public override CheckerField3D GetChecker(FieldPlanner planner)
        {
            CheckerField3D checker = GetTunnel(null, null, BranchLength.GetRandom(), BranchThickness.GetRandom());
            CheckerField3D initTunnel = checker.Copy();

            List<CheckerField3D> tunnels = new List<CheckerField3D>();
            tunnels.Add(initTunnel);

            for (int i = 0; i < TargetBranches.GetRandom(); i++)
            {
                CheckerField3D tunnel = null;

                for (int tries = 0; tries < 8; tries++)
                {
                    tunnel = GetTunnel(tunnels[FGenerators.GetRandom(0, tunnels.Count)], checker, BranchLength.GetRandom(), BranchThickness.GetRandom(), SeparationFactor, AvoidOverlaps/*, ThicknessSnap*/);
                    if (tunnel != null) break;
                }

                if (tunnel == null) continue;

                if (tunnel.Bounding.Count > 0) checker.Bounding.Add(tunnel.Bounding[0]);

                checker.Join(tunnel);
                tunnels.Add(tunnel);

                RefreshPreview(checker);
            }

            checker.Bounding.Clear();

            return checker;
        }


        #region Tunnel Generating

        public static CheckerField3D GetTunnel(CheckerField3D alignTo, CheckerField3D full, int size, int thickness, int separation = 3, bool avoid = true, bool snap = false)
        {
            CheckerField3D checker = new CheckerField3D();

            if (alignTo == null)
            {
                if (FGenerators.GetRandom(0f, 1f) < 0.5f)
                    checker.AddStripeInDirection(new Vector3Int(0, 0, -1) * (size / 2), new Vector3Int(0, 0, 1), size, thickness);
                else
                    checker.AddStripeInDirection(Vector3Int.left * (size / 2), Vector3Int.right, size, thickness);

                checker.GetBasicBoundsLocal(true);
            }
            else
            {
                // Checking every point on checker for alignment
                bool done = false;
                var positionsToCheck = IGeneration.GetRandomizedCells(alignTo.Grid);

                for (int pos = 0; pos < positionsToCheck.Count; pos++)
                {
                    Vector3Int getRandom = positionsToCheck[pos].Pos;

                    Vector3Int edged = alignTo.GetNearestEdge(getRandom, true);

                    Vector3Int dir = ((Vector3)(edged - getRandom)).normalized.V3toV3Int();

                    for (int side = 0; side < 2; side++)
                    {
                        CheckerField3D pathCheck = new CheckerField3D();
                        int sign = side == 0 ? 1 : -1;
                        pathCheck.AddStripeInDirection(edged, dir * sign, size, thickness);

                        if (snap)
                        {

                        }

                        pathCheck.GetBasicBoundsLocal(true);

                        if (pathCheck.IsCollidingWith(alignTo)) continue;

                        if (avoid)
                            if (pathCheck.IsCollidingWith(full))
                            {
                                #region Debug

                                //pathCheck.DebugLogDrawBoundings(Color.red);
                                //alignTo.DebugLogDrawBoundings(Color.yellow);

                                //UnityEngine.Debug.DrawRay(alignTo.GetWorldPos(edged) + Vector3.up, Vector3.up + Vector3.one * 0.04f, Color.green, 1.01f);
                                //UnityEngine.Debug.DrawRay(alignTo.GetWorldPos(getRandom) + Vector3.up, Vector3.up + Vector3.one * 0.04f, Color.red, 1.01f);
                                //UnityEngine.Debug.DrawLine(alignTo.GetWorldPos(edged) + Vector3.up * 0.5f, alignTo.GetWorldPos(edged) + Vector3.up * 0.5f + dir * sign, Color.yellow, 1.01f);

                                #endregion

                                continue;
                            }

                        if (full != null)
                        {
                            if (separation <= 1) { checker = pathCheck; done = true; break; }


                            int distanceLSide = pathCheck.CheckCollisionDistanceInDirectionLocal(full, PGGUtils.GetRotatedFlatDirectionFrom(dir), thickness + 2 + separation);
                            int distanceRSide = pathCheck.CheckCollisionDistanceInDirectionLocal(full, PGGUtils.GetRotatedFlatDirectionFrom(dir).Negate(), thickness + 2 + separation);

                            //UnityEngine.Debug.Log("dist l and r " + distanceLSide + "  " + distanceRSide);
                            if (distanceLSide == 1 || distanceRSide == 1) continue; // too near

                            checker = pathCheck;

                            int sep = separation - 1;

                            // Checking separation from other corridors, if bigger then overriding other options
                            if (distanceLSide == -1)
                            {
                                if (distanceRSide == -1) { done = true; break; }
                                if (distanceRSide >= sep) { done = true; break; }
                            }
                            else if (distanceRSide == -1)
                            {
                                if (distanceLSide >= sep) {  done = true; break; }
                            }
                            else
                            {
                                if ((distanceLSide >= separation && distanceRSide >= sep)) { done = true; break; }
                            }
                        }
                        else
                        {
                            done = true; break;
                        }
                    }

                    if (done) break;
                }

                if (!done) checker = null;
            }

            return checker;
        }


        #endregion

    }
}