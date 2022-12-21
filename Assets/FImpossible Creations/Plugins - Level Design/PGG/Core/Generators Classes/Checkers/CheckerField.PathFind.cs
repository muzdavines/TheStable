using FIMSpace.Generating.Planning;
using FIMSpace.Generating.PathFind;
using System;
using System.Collections.Generic;
using UnityEngine;
using FIMSpace.Hidden;

namespace FIMSpace.Generating.Checker
{
    public partial class CheckerField
    {
        /// <summary>
        /// Generates box positions from one point to another like encapsulating
        /// </summary>
        public void AddFieldTowards(Vector2Int from, Vector2Int to)
        {
            int xDiff = to.x - from.x;
            int yDiff = to.y - from.y;
            int xSign = Mathf.Sign(xDiff).ToInt();
            int ySign = Mathf.Sign(yDiff).ToInt();

            for (int x = 0; x < Mathf.Abs(xDiff); x++)
                for (int y = 0; y < Mathf.Abs(yDiff); y++)
                {
                    Vector2Int newPos = new Vector2Int();
                    newPos.x = from.x + x * xSign;
                    newPos.y = from.y + y * ySign;
                    Add(newPos);
                }
        }

        /// <summary>
        /// Generates box positions from one point to another
        /// </summary>
        /// <param name="dirChangeCost"> from 0 to 1, 1 -> just two lines, 0 -> diagonal lines which you probably don't want to have </param>
        /// <param name="pathThickness"> How many checkers around path line </param>
        public void AddPathTowards(Vector2Int from, Vector2Int to, float dirChangeCost = 0.3f, int pathThickness = 1, bool recalculateBounds = true)
        {
            List<Vector2Int> pathPoints = GenerateLinePoints(from, to, dirChangeCost);
            AddPathTowards(pathPoints, pathThickness, true, true, recalculateBounds);
        }

        public void AddPathTowards(List<Vector2Int> path, int pathThickness = 1, bool clearStartAndEnd = true, bool fillCurves = true, bool recalculateBounds = true)
        {
            if (path.Count <= 1) return;

            for (int i = 0; i < path.Count; i++)
            {
                Add(path[i]);

                    if (pathThickness > 1)
                    {
                        Vector2Int dir;
                        if (i + 1 < path.Count) dir = (path[i + 1] - path[i]); else dir = path[i] - path[i - 1];
                        dir = ((Vector2)dir).normalized.V2toV2Int();
                        Vector2Int sideDir = PGGUtils.GetRotatedFlatDirectionFrom(dir);

                        if (pathThickness % 2 == 0) // 2, 4, 6
                        {
                            for (int t = -pathThickness / 2; t <= pathThickness / 3; t++)
                            {
                                if (t == 0) continue;
                                Add(path[i] + sideDir * t);
                            }
                        }
                        else // 3 ,5 ,7
                        {
                            for (int t = -pathThickness / 2; t <= pathThickness / 2; t++)
                            {
                                if (t == 0) continue;
                                Add(path[i] + sideDir * t);
                            }
                        }
                    }
            }

            // Filling curves and correcting start/end point
            if (path.Count > 3)
                if (pathThickness > 1)
                {
                    Vector2Int dir;

                    if (clearStartAndEnd)
                    {
                        // Finish Point Clearing
                        //dir = (path[path.Count - 1] - path[path.Count - 2]);
                        //dir = ((Vector2)dir).normalized.V2toV2Int();
                        //ClearSquareInDirection(path[path.Count - 1], dir, pathThickness);
                        //int even = pathThickness % 2 == 0 ? 1 : 0;
                        //FillSquareInDirection(path[path.Count - 1], -dir, pathThickness, even);

                        //// Correcting start
                        //dir = (path[1] - path[0]);
                        //dir = ((Vector2)dir).normalized.V2toV2Int();

                        //ClearSquareInDirection(path[0], -dir, pathThickness);
                        //FillSquareInDirection(path[0], dir, pathThickness, even);
                    }

                    if (fillCurves)
                    {
                        // Finding direction change points and filling them
                        for (int i = 1; i < path.Count - 1; i++)
                        {
                            Vector2Int preDir = (path[i] - path[i - 1]);
                            preDir = ((Vector2)preDir).normalized.V2toV2Int();

                            dir = (path[i + 1] - path[i]);
                            dir = ((Vector2)dir).normalized.V2toV2Int();

                            if (dir != preDir)
                            {
                                if (pathThickness % 2 == 0)
                                {
                                    if (pathThickness < 4)
                                    {
                                        if ((preDir.x > 0 && dir.y > 0) ||
                                            (preDir.y > 0 && dir.x > 0) ||
                                            (preDir.y < 0 && dir.x < 0) ||
                                            (preDir.x < 0 && dir.y < 0)
                                            )
                                        {
                                            FillSquareInDirection(path[i - 1], preDir, pathThickness);
                                        }
                                    }
                                    else
                                    {

                                        if ((preDir.x > 0 && dir.y > 0) ||
                                            (preDir.y > 0 && dir.x > 0)
                                            )
                                            FillSquareInDirection(path[i - 1], preDir, pathThickness);

                                        else if ((preDir.x > 0 && dir.y < 0) ||
                                            (preDir.y < 0 && dir.x > 0)
                                            )
                                            FillSquareInDirection(path[i - 1] + dir, preDir, pathThickness + 1);
                                        else

                                        if ((preDir.x < 0 && dir.y < 0) ||
                                            (preDir.y < 0 && dir.x < 0)
                                            )
                                            FillSquareInDirection(path[i - 1], preDir, pathThickness);
                                        else if ((preDir.y > 0 && dir.x < 0) ||
                                                 (preDir.x < 0 && dir.y > 0)
                                                )
                                            FillSquareInDirection(path[i - 1] + dir, preDir, pathThickness + 1);

                                    }
                                }
                                else
                                {
                                    FillSquareInDirection(path[i - 1], preDir, pathThickness);
                                }
                            }
                            else
                            {
                                //FillSquareInDirection(path[i - 1], preDir, pathThickness, 1);
                            }
                        }
                    }

                }

            if ( recalculateBounds) RecalculateMultiBounds();
        }



        #region Complex

        public static List<Vector2Int> GenerateLinePoints(Vector2Int start, Vector2Int end, float changeDirectionCost = 0.1f)
        {
            return GeneratePathFindPointsAllVersions(start, end, Vector2Int.zero, Vector2Int.zero, changeDirectionCost)[0];
        }
        public static List<List<Vector2Int>> GeneratePathFindPointsAllVersions(Vector2Int start, Vector2Int end, float changeDirectionCost = 0.1f)
        {
            return GeneratePathFindPointsAllVersions(start, end, Vector2Int.zero, Vector2Int.zero, changeDirectionCost);
        }

        /// <summary>
        /// Returns three elements array, [0] -> array with all points, [1] -> only change direction positions, [2] -> change direction position without start points
        /// </summary>
        public static List<List<Vector2Int>> GeneratePathFindPointsAllVersions(Vector2Int start, Vector2Int end, Vector2Int startDir, Vector2Int endDir, float changeDirectionCost = 0.1f)
        {
            List<List<Vector2Int>> lists = new List<List<Vector2Int>>();

            List<Vector2Int> allPoints = new List<Vector2Int>();
            List<Vector2Int> dirChangePoints = new List<Vector2Int>();
            List<Vector2Int> dirChangePointsNoStart = new List<Vector2Int>();

            int maxIters = Mathf.RoundToInt(Vector2Int.Distance(start, end) * 3);
            PathFindHelper[] steps = new PathFindHelper[4];
            Vector2Int position = start;
            Vector2Int currentDir = startDir.Negate();

            dirChangePointsNoStart.Add(position);
            dirChangePoints.Add(position);
            allPoints.Add(position);

            // Generating point path from one point to another
            for (int i = 0; i < maxIters; i++)
            {
                int nearestD = 0;
                float nearestDist = float.MaxValue;

                // Get step resulting in nearest position to target point
                for (int d = 0; d < 4; d++)
                {
                    steps[d] = new PathFindHelper();
                    steps[d].Dir = PathFindHelper.GetStepDirection(d);
                    steps[d].Distance = Vector2.Distance(position + steps[d].Dir, end);

                    if (steps[d].Dir != currentDir) steps[d].Distance += changeDirectionCost;

                    if (steps[d].Distance < nearestDist)
                    {
                        nearestDist = steps[d].Distance;
                        nearestD = d;
                    }
                }

                PathFindHelper pfNearest = steps[nearestD];

                //UnityEngine.Debug.DrawRay(position.V2toV3(), Vector3.up, Color.green, 1.01f);

                if (currentDir != pfNearest.Dir) // Direction change occured
                {
                    if (dirChangePoints.Contains(position) == false)
                    {
                        dirChangePoints.Add(position);
                        dirChangePointsNoStart.Add(position + currentDir);
                    }
                }

                position += pfNearest.Dir;
                currentDir = pfNearest.Dir;
                allPoints.Add(position);

                if (position == end)
                {
                    if (dirChangePoints.Contains(position) == false) dirChangePoints.Add(position);
                    if (dirChangePointsNoStart.Contains(position) == false) dirChangePointsNoStart.Add(position);
                    break;
                }
            }

            lists.Add(allPoints);
            lists.Add(dirChangePoints);
            lists.Add(dirChangePointsNoStart);
            return lists;
        }

        /// <summary>
        /// Returns three elements array, [0] -> array with all points, [1] -> only change direction positions, [2] -> change direction position without start points
        /// </summary>
        internal static List<List<Vector2Int>> GeneratePathFindPointsFromStartToEnd(SimplePathGuide guide)
        {
            return GeneratePathFindPointsAllVersions(guide.Start, guide.End, guide.StartDir.GetDirection2D(), guide.EndDir.GetDirection2D(), guide.ChangeDirCost);
        }

        struct PathFindHelper
        {
            public Vector2Int Dir;
            public float Distance;

            public static Vector2Int GetStepDirection(int iter)
            {
                if (iter == 0) return new Vector2Int(1, 0);
                else if (iter == 1) return new Vector2Int(0, 1);
                else if (iter == 2) return new Vector2Int(-1, 0);
                else return new Vector2Int(0, -1);
            }
        }


        #endregion


    }
}