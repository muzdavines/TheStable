using FIMSpace.Generating.Checker;
using FIMSpace.Generating.PathFind;
using FIMSpace.Hidden;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace FIMSpace.Generating
{
    /// <summary>
    /// This class will become deprecated and removed in future versions!
    /// </summary>
    [CreateAssetMenu(fileName = "GenShape_", menuName = "FImpossible Creations/Procedural Generation/Legacy/Generation Shape (Deprecated)", order = 4201)]
    public class GenerationShape : ScriptableObject
    {
        public enum EGenerationMode { StaticSizeRectangle, RandomRectangle, RandomTunnels, ManualRectangles, RulesSet_NotYetInBeta }
        public enum EGenerationType { Default, Room, Tunnel, Other }
        public GenerationSetup Setup;

        #region Classes for painted cells

        [System.Serializable]
        public class ShapeCells
        {
            public List<Vector3Int> Cells = new List<Vector3Int>();
            public List<ShapeCell> Datas = new List<ShapeCell>();
            public bool ContainsPosition(Vector3Int p) { return Cells.Contains(p); }

            public void SwitchOnPosition(Vector3Int pos)
            {
                if (Cells == null) Cells = new List<Vector3Int>();
                if (!Cells.Contains(pos)) Cells.Add(pos); else Cells.Remove(pos);
            }

            public CheckerField GetChecker()
            {
                CheckerField f = new CheckerField();

                for (int i = 0; i < Cells.Count; i++)
                {
                    f.Add(new Vector2Int(Cells[i].x, Cells[i].z));
                }

                f.RecalculateMultiBounds();

                return f;
            }
        }

        [System.Serializable]
        public class ShapeCell
        {
            public Vector3Int pos = Vector3Int.zero;
            public string data = "";
        }

        #endregion

        [System.Serializable]
        public class GenerationSetup
        {
            public EGenerationMode GenerationMode = EGenerationMode.StaticSizeRectangle;

            // Rects
            public RectSet RectSetup;

            // Tunnels
            public MinMax BranchLength = new MinMax(3, 5);
            public MinMax TargetBranches = new MinMax(1, 2);
            public MinMax BranchThickness = new MinMax(1, 1);

            // Manual and rule cells
            public List<ShapeCells> CellSets = new List<ShapeCells>();

            #region Tunnel Generating

            public static CheckerField GetTunnel(CheckerField alignTo, int size, int thickness, int separation = 3, CheckerField limitChecker = null, List<SimplePathGuide> guidesUsed = null)
            {
                CheckerField checker = new CheckerField();

                if (alignTo == null)
                {
                    if (FGenerators.GetRandom(0f, 1f) < 0.5f)
                        checker.AddPathTowards(Vector2Int.down * (size / 2), Vector2Int.up * (size / 2), 0.3f, thickness);
                    else
                        checker.AddPathTowards(Vector2Int.left * (size / 2), Vector2Int.right * (size / 2), 0.3f, thickness);
                }
                else
                {
                    // Checking every point on checker for alignment
                    var positionsToCheck = alignTo.GetRandomizedPositionsCopy();

                    for (int pos = 0; pos < positionsToCheck.Count; pos++)
                    {
                        Vector2Int getRandom = positionsToCheck[pos];

                        #region Only points far from guides
                        if (guidesUsed != null)
                        {
                            bool isOk = true;

                            for (int g = 0; g < guidesUsed.Count; g++)
                            {
                                int refDist = separation;

                                if (Vector2Int.Distance(getRandom, guidesUsed[g].Start) < refDist) isOk = false;
                                if (isOk == false) break;
                                if (Vector2Int.Distance(getRandom, guidesUsed[g].End) < refDist) isOk = false;
                                if (isOk == false) break;
                            }

                            if (isOk == false) continue;
                        }
                        #endregion

                        Vector2Int edged = alignTo.GetNearestEdge(getRandom, true);
                        Vector2Int dir = ((Vector2)(edged - getRandom)).normalized.V2toV2Int();

                        bool done = false;
                        for (int side = 0; side < 2; side++)
                        {
                            CheckerField pathCheck = new CheckerField();
                            int sign = side == 0 ? 1 : -1;
                            pathCheck.AddPathTowards(edged, edged + dir * sign * size, 0.3f, thickness);

                            // If out of limit square then ignore
                            if (limitChecker != null) if (!limitChecker.ContainsFully(pathCheck)) continue;

                            if (pathCheck.CollidesWith(alignTo)) continue;

                            int distanceLSide = pathCheck.CheckCollisionDistanceInDirection(alignTo, PGGUtils.GetRotatedFlatDirectionFrom(dir));
                            int distanceRSide = pathCheck.CheckCollisionDistanceInDirection(alignTo, PGGUtils.GetRotatedFlatDirectionFrom(dir).Negate());

                            if (distanceLSide == 1 || distanceRSide == 1) continue; // too near

                            checker = pathCheck;
                            if (separation <= 1) { done = true; break; }

                            // Checking separation from other corridors, if bigger then overriding other options
                            if (distanceLSide == -1)
                            {
                                if (distanceRSide >= separation) { done = true; break; }
                            }
                            else if (distanceRSide == -1)
                            {
                                if (distanceLSide >= separation) { done = true; break; }
                            }
                            else
                            {
                                if ((distanceLSide >= separation && distanceRSide >= separation)) { done = true; break; }
                            }
                        }

                        if (done) break;
                    }

                }

                return checker;
            }


            #endregion


            #region Generating Checker Field

            public CheckerField GetChecker(CheckerField alignTo = null)
            {
                if (GenerationMode == EGenerationMode.StaticSizeRectangle)
                {
                    CheckerField checker = new CheckerField();
                    checker.SetSize(RectSetup.Width.Min, RectSetup.Height.Min, Vector2Int.zero);
                    return checker;

                }
                //else if (GenerationMode == EGenerationMode.MultipleRectangles)
                //{
                //    CheckerField checker = new CheckerField();

                //    for (int i = 0; i < Rects.Count; i++)
                //        checker.Join(Rects[i].GetChecker());

                //    return checker;
                //}
                else if (GenerationMode == EGenerationMode.RandomTunnels)
                {
                    CheckerField checker = GenerationSetup.GetTunnel(alignTo, BranchLength.GetRandom(), BranchThickness.GetRandom());

                    for (int i = 0; i < TargetBranches.GetRandom(); i++)
                    {
                        CheckerField tunnel = GenerationSetup.GetTunnel(checker, BranchLength.GetRandom(), BranchThickness.GetRandom());
                        checker.Join(tunnel);
                    }

                    return checker;
                }
                else if (GenerationMode == EGenerationMode.ManualRectangles)
                {
                    if (CellSets == null) return RectSetup.GetChecker();
                    if (CellSets.Count == 0) return RectSetup.GetChecker();
                    int choose = FGenerators.GetRandom(0,CellSets.Count);
                    return CellSets[choose].GetChecker();
                }
                else
                {
                    return RectSetup.GetChecker();
                }
            }

            #endregion

        }

        [System.Serializable]
        public class RectSet
        {
            public Vector2Int StartPos = Vector2Int.zero;
            public MinMax Width = new MinMax(4, 6);
            public MinMax Height = new MinMax(3, 4);

            public CheckerField GetChecker()
            {
                CheckerField checker = new CheckerField();
                checker.SetSize(Width.GetRandom(), Height.GetRandom(), false);
                return checker;
            }
        }

        public CheckerField GetChecker(CheckerField alignTo = null)
        {
            return Setup.GetChecker(alignTo);
        }

        /// <summary>
        /// Centering is not working yet
        /// </summary>
        public CheckerField GetChecker(bool center)
        {
            return GetChecker(null);
        }

        #region Not Used

        //        public class BoundGroup
        //        {
        //            public List<Bounds> Bounds = new List<Bounds>();

        //            /// <summary>
        //            /// Creating long rectangular bounds out of array of single squares of rects
        //            /// </summary>
        //            public void SetOptimizedBounds(List<Bounds> bounds)
        //            {

        //            }

        //            /// <param name="quart"> from 0 to 4 -> right, up, left, down </param>
        //            public void Rotate(int quart)
        //            {

        //            }

        //            bool CollidesWith(Bounds otherBounds)
        //            {
        //                return false;
        //            }

        //            bool CollidesWith(BoundGroup other)
        //            {
        //                return false;
        //            }

        //            void FindFreePlace()
        //            {

        //            }

        //            void AlignVerticallyWith()
        //            {

        //            }

        //            void AlignHorizontallyWith()
        //            {

        //            }


        //            #region Editor

        //#if UNITY_EDITOR


        //            public void OnDrawGizmos()
        //            {

        //            }

        //#endif

        //            #endregion


        //        }


        #endregion

    }
}