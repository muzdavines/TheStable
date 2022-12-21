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
        /// Snap align one checker to another without overlapping
        /// </summary>
        public void SnapToOther(CheckerField b, bool tryReAlign = false)
        {
            Vector2Int center = GetCenter();
            Vector2Int oCenter = b.GetCenter();
            Vector2Int dir = ((Vector2)oCenter - center).normalized.V2toV2Int();
            Vector2Int nearestOnOther = b.NearestAlignFor(center, dir.Negate());
            Vector2Int myNearest = NearestPoint(nearestOnOther);
            Position += nearestOnOther - myNearest;

            if (tryReAlign)
            {
                center = GetCenter();
                nearestOnOther = b.NearestAlignFor(center, dir.Negate());
                myNearest = NearestPoint(center);
                Vector2 fdir = nearestOnOther - myNearest;
                Vector2Int sDir;

                if (Mathf.Abs(fdir.y) > Mathf.Abs(fdir.x))
                {
                    dir = new Vector2Int(0, Mathf.RoundToInt(Mathf.Sign(fdir.y)));
                    sDir = new Vector2Int(Mathf.RoundToInt(Mathf.Sign(fdir.x)), 0);
                }
                else
                {
                    dir = new Vector2Int(Mathf.RoundToInt(Mathf.Sign(fdir.x)), 0);
                    sDir = new Vector2Int(0, Mathf.RoundToInt(Mathf.Sign(fdir.y)));
                }

                Vector2Int prePos = Position;
                Position += dir * Mathf.RoundToInt(fdir.magnitude);

                if (CollidesWith(b))
                {
                    Position = prePos + sDir * Mathf.RoundToInt(fdir.magnitude);
                }

                if (CollidesWith(b)) Position = prePos;
            }
        }


        public void SnapToOther(Vector2Int position)
        {
            Vector2Int myNearest = NearestPoint(position);
            Position += position - myNearest;
        }


        internal Vector2Int GetNearestEdge(Vector2Int localCheckerPos, bool getOutPos = false)
        {
            for (int o = 0; o < 300; o++)
            {
                Vector2Int off = new Vector2Int(o, 0);
                Vector2Int check = localCheckerPos + off;
                if (ChildPos.Contains(check) == false) return getOutPos ? check : (check - off);

                off = new Vector2Int(-o, 0);
                check = localCheckerPos + off;
                if (ChildPos.Contains(check) == false) return getOutPos ? check : (check - off);

                off = new Vector2Int(0, o);
                check = localCheckerPos + off;
                if (ChildPos.Contains(check) == false) return getOutPos ? check : (check - off);

                off = new Vector2Int(0, -o);
                check = localCheckerPos + off;
                if (ChildPos.Contains(check) == false) return getOutPos ? check : (check - off);
            }

            return localCheckerPos;
        }


        /// <summary>
        /// If other checker is aligning to this
        /// </summary>
        public bool IsAligning(CheckerField other, bool checkCollision = false)
        {

            if (UseBounds == false)
            {
                if (checkCollision)
                    if (CollidesWith(other))
                        return false;

                for (int o = 0; o < other.ChildPos.Count; o++)
                    if (IsAligning(other.WorldPos(o)))
                        return true;

                return false;
            }


            for (int o = 0; o < other.Bounding.Count; o++)
            {
                for (int b = 0; b < Bounding.Count; b++)
                {
                    if (Bounding[b].IsOnEdge(other.Bounding[o])) return true;
                }
            }


            return false;
        }


        /// <summary>
        /// If other checker is aligning to this in few points
        /// </summary>
        public List<Vector2Int> AlignPoints(CheckerField other, bool onlyNonColliding = false)
        {
            List<Vector2Int> points = new List<Vector2Int>();

            for (int o = 0; o < other.ChildPos.Count; o++)
            {
                Vector2Int pos = other.WorldPos(o);

                if (IsAligning(pos))
                {
                    if (onlyNonColliding)
                    {
                        if (CollidesWith(pos) == false)
                            points.Add(pos);
                    }
                    else
                        points.Add(pos);
                }
            }

            return points;
        }


        public Vector2Int GetBorderPosition(Vector2Int start, Vector2Int dir, int maxDistance = 30, bool edge = false)
        {
            if (ContainsWorldPos(start) == false) start = NearestPoint(start);
            Vector2Int borderPos = start;

            for (int i = 0; i < maxDistance; i++)
            {
                Vector2Int checkPos = start + dir * i;
                if (ContainsWorldPos(checkPos) == false) { if (edge == false) borderPos = checkPos; else borderPos = checkPos - dir; break; }
            }

            return borderPos;
        }


        /// <summary>
        /// Trying aligning this checker to another, starting checking collisions in start towards dir until finding enough space
        /// More precise than SnapTo()
        /// </summary>
        internal void SetAlignNextToPosition(CheckerField other, Vector2Int start, Vector2Int dir, int maxDistance = 30)
        {
            if (other.ContainsWorldPos(start) == false) start = other.NearestPoint(start);

            // Finding first align position
            Vector2Int otherAlignPos = other.GetBorderPosition(start, dir, maxDistance);

            // Finding this checker's most counter sided cell for this direction setup
            Vector2Int snapPos = NearestPoint(otherAlignPos);
            snapPos = GetBorderPosition(snapPos, dir.Negate(), maxDistance, true);

            MoveToPosition(Position + (otherAlignPos - snapPos));
            //Position += (otherAlignPos - snapPos);
        }


        /// <summary>
        /// Finding nearest position for choosed position in non-overlapping position
        /// </summary>
        public Vector2Int NearestAlignFor(Vector2Int worldPos, Vector2Int desiredOutDirection, int maxDistance = 100)
        {
            if (ChildPos.Count == 0) return Position;
            Vector2Int align = worldPos;
            Vector2Int nearest = NearestPoint(worldPos);

            for (int i = 0; i < maxDistance; i++)
            {
                Vector2Int check = nearest + desiredOutDirection * i;
                if (ContainsWorldPos(check) == false)
                    return check;
            }

            return align;
        }


        /// <summary>
        /// Finding nearest point of this checker to choosed world position
        /// </summary>
        public Vector2Int NearestPoint(Vector2Int worldPos)
        {
            float nearestDist = float.MaxValue;
            Vector2Int nearest = WorldPos(0);

            //for (int i = 0; i < Bounding.Count; i++)
            //{
            //    Vector2Int n = Bounding[i].GetNearestTo(worldPos);
            //    float dist = Vector2.Distance(n, worldPos);
            //    if (dist < nearestDist) { var nn = GetWorldPos(n); if (nn is null == false) { nearestDist = dist; nearest = n; } }
            //}

            for (int i = 0; i < ChildPos.Count; i++)
            {
                float dist = Vector2.Distance(WorldPos(i), worldPos);
                if (dist < nearestDist) { nearestDist = dist; nearest = WorldPos(i); }
            }

            return nearest;
        }


        /// <summary>
        /// Finding nearest point of this checker to other checker
        /// </summary>
        public Vector2Int NearestPoint(CheckerField other)
        {
            float nearestDist = float.MaxValue;
            Vector2Int nearest = Position;

            for (int i = 0; i < other.ChildPos.Count; i++)
            {
                Vector2Int n = NearestPoint(other.WorldPos(i));
                float dist = Vector2.Distance(n, other.WorldPos(i));
                if (dist < nearestDist) { nearestDist = dist; nearest = n; }
            }

            return nearest;
        }


        internal List<Vector2Int> GetEdgePositions()
        {
            List<Vector2Int> edges = new List<Vector2Int>();

            for (int i = 0; i < ChildPos.Count; i++)
            {
                if (ChildPos.Contains(ChildPosition(i) + new Vector2Int(1, 0)) == false) { edges.Add(Position + ChildPosition(i)); continue; }
                if (ChildPos.Contains(ChildPosition(i) + new Vector2Int(-1, 0)) == false) { edges.Add(Position + ChildPosition(i)); continue; }
                if (ChildPos.Contains(ChildPosition(i) + new Vector2Int(0, 1)) == false) { edges.Add(Position + ChildPosition(i)); continue; }
                if (ChildPos.Contains(ChildPosition(i) + new Vector2Int(0, -1)) == false) { edges.Add(Position + ChildPosition(i)); continue; }

                if (ChildPos.Contains(ChildPosition(i) + new Vector2Int(1, 1)) == false) { edges.Add(Position + ChildPosition(i)); continue; }
                if (ChildPos.Contains(ChildPosition(i) + new Vector2Int(1, -1)) == false) { edges.Add(Position + ChildPosition(i)); continue; }
                if (ChildPos.Contains(ChildPosition(i) + new Vector2Int(-1, 1)) == false) { edges.Add(Position + ChildPosition(i)); continue; }
                if (ChildPos.Contains(ChildPosition(i) + new Vector2Int(-1, -1)) == false) { edges.Add(Position + ChildPosition(i)); continue; }
            }

            return edges;
        }


        internal List<Vector2Int> FindConnectedShapeOfSize(int size, int radius = 0, bool local = false)
        {
            List<Vector2Int> shape = new List<Vector2Int>();
            shape.Add(ChildPosition(FGenerators.GetRandom(0, ChildPos.Count)));

            for (int i = 0; i < size * 5; i++)
            {
                for (int s = 0; s < shape.Count; s++)
                {
                    var squre = GetConnected(shape[s], radius, shape);
                    for (int q = 0; q < squre.Count; q++) shape.Add(squre[q]);
                    if (shape.Count - 1 >= size) break;
                }
            }

            if (!local) for (int s = 0; s < shape.Count; s++) shape[s] += Position;

            return shape;
        }


        private List<Vector2Int> GetConnected(Vector2Int local, int radius = 0, List<Vector2Int> ignores = null)
        {
            List<List<Vector2Int>> getted = new List<List<Vector2Int>>();

            Vector2Int p = local + new Vector2Int(1, 0);
            if (ChildPos.Contains(p)) if (ignores.Contains(p) == false)
                {
                    List<Vector2Int> sq = new List<Vector2Int>();
                    getted.Add(sq);
                    sq.Add(p);

                    p = local + new Vector2Int(1, 1);
                    if (ignores.Contains(p) == false) if (ChildPos.Contains(p)) sq.Add(p);
                    p = local + new Vector2Int(1, -1);
                    if (ignores.Contains(p) == false) if (ChildPos.Contains(p)) sq.Add(p);
                }


            p = local + new Vector2Int(-1, 0);
            if (ChildPos.Contains(p)) if (ignores.Contains(p) == false)
                {
                    List<Vector2Int> sq = new List<Vector2Int>();
                    getted.Add(sq);
                    sq.Add(p);

                    p = local + new Vector2Int(-1, 1);
                    if (ignores.Contains(p) == false) if (ChildPos.Contains(p)) sq.Add(p);
                    p = local + new Vector2Int(-1, -1);
                    if (ignores.Contains(p) == false) if (ChildPos.Contains(p)) sq.Add(p);
                }


            p = local + new Vector2Int(0, 1);
            if (ChildPos.Contains(p)) if (ignores.Contains(p) == false)
                {
                    List<Vector2Int> sq = new List<Vector2Int>();
                    getted.Add(sq);
                    sq.Add(p);

                    p = local + new Vector2Int(-1, 1);
                    if (ignores.Contains(p) == false) if (ChildPos.Contains(p)) sq.Add(p);
                    p = local + new Vector2Int(1, 1);
                    if (ignores.Contains(p) == false) if (ChildPos.Contains(p)) sq.Add(p);
                }

            p = local + new Vector2Int(0, -1);
            if (ChildPos.Contains(p)) if (ignores.Contains(p) == false)
                {
                    List<Vector2Int> sq = new List<Vector2Int>();
                    getted.Add(sq);
                    sq.Add(p);

                    p = local + new Vector2Int(-1, -1);
                    if (ignores.Contains(p) == false) if (ChildPos.Contains(p)) sq.Add(p);
                    p = local + new Vector2Int(1, -1);
                    if (ignores.Contains(p) == false) if (ChildPos.Contains(p)) sq.Add(p);
                }

            if (getted.Count == 0) return new List<Vector2Int>();
            return getted[FGenerators.GetRandom(0, getted.Count)];

        }


        public Vector2Int GetCenterOnEdge(Vector2Int edgedPos, Vector2Int emptyDir, int edgeLimit = 3, CheckerField mustCollideWith = null)
        {
            List<Vector2Int> edgeGroup = new List<Vector2Int>();
            edgeGroup.Add(edgedPos);

            Vector2Int rotDir = PGGUtils.GetRotatedFlatDirectionFrom(emptyDir);

            bool addPosit = true;
            bool addNeg = true;

            for (int i = 1; i <= edgeLimit; i++)
            {
                if (addPosit)
                {
                    Vector2Int positP = edgedPos + rotDir * i;

                    if (ContainsWorldPos(positP + emptyDir)) addPosit = false;

                    if (FGenerators.CheckIfExist_NOTNULL(mustCollideWith ))
                        if (mustCollideWith.ContainsWorldPos(positP + emptyDir) == false)
                            addPosit = false;

                    if (addPosit) if (ContainsWorldPos(positP)) edgeGroup.Add(positP); else addPosit = false;
                }

                if (addNeg)
                {
                    Vector2Int negP = edgedPos - rotDir * i;
                    if (ContainsWorldPos(negP + emptyDir)) addNeg = false;

                    if (FGenerators.CheckIfExist_NOTNULL(mustCollideWith ))
                        if (mustCollideWith.ContainsWorldPos(negP + emptyDir) == false)
                            addNeg = false;

                    if (addNeg) if (ContainsWorldPos(negP)) edgeGroup.Add(negP); else addNeg = false;
                }

                if (!addPosit && !addNeg) break;
            }

            // Choosing most middle
            // Defining middle
            Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
            Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);
            for (int i = 0; i < edgeGroup.Count; i++)
            {
                if (edgeGroup[i].x < min.x) min.x = edgeGroup[i].x;
                if (edgeGroup[i].x > max.x) max.x = edgeGroup[i].x;
                if (edgeGroup[i].y < min.y) min.y = edgeGroup[i].y;
                if (edgeGroup[i].y > max.y) max.y = edgeGroup[i].y;
            }

            Vector2Int middle = new Vector2Int();
            middle.x = Mathf.RoundToInt(Mathf.Lerp(min.x, max.x, 0.5f));
            middle.y = Mathf.RoundToInt(Mathf.Lerp(min.y, max.y, 0.5f));

            if (rotDir.x != 0)
            {
                Vector2Int tgt = new Vector2Int(middle.x, edgedPos.y);
                if (ContainsWorldPos(tgt)) return tgt;
            }
            else
            {
                Vector2Int tgt = new Vector2Int(edgedPos.x, middle.y);
                if (ContainsWorldPos(tgt)) return tgt;
            }

            return edgedPos;
        }


        /// <summary>
        /// If other checker is aligning to this in few points
        /// </summary>
        public int AlignPointsCount(CheckerField other, bool onlyNonColliding = false)
        {
            int points = 0;

            for (int o = 0; o < other.ChildPos.Count; o++)
            {
                Vector2Int pos = other.WorldPos(o);
                if (IsAligning(pos))
                {
                    if (onlyNonColliding) { if (CollidesWith(pos) == false) points++; }
                    else points++;
                }
            }

            return points;
        }


        /// <summary>
        /// If position is touching with it's side this checker
        /// </summary>
        public bool IsAligning(Vector2Int pos)
        {
            if (UseBounds == false)
            {
                for (int p = 0; p < ChildPos.Count; p++)
                {
                    if (WorldPos(p) == pos) return false;
                }

                for (int p = 0; p < ChildPos.Count; p++)
                {
                    Vector2Int chPos = WorldPos(p);
                    if (chPos + new Vector2Int(1, 0) == pos) return true;
                    if (chPos + new Vector2Int(0, 1) == pos) return true;
                    if (chPos + new Vector2Int(-1, 0) == pos) return true;
                    if (chPos + new Vector2Int(0, -1) == pos) return true;
                }

                return false;
            }

            for (int i = 0; i < Bounding.Count; i++)
            {
                if (Bounding[i].IsOnEdge(pos)) return true;
            }

            //for (int p = 0; p < ChildPos.Count; p++)
            //{
            //    if (WorldPos(p) == pos) return false;
            //}

            //for (int p = 0; p < ChildPos.Count; p++)
            //{
            //    Vector2Int chPos = WorldPos(p);
            //    if (chPos + new Vector2Int(1, 0) == pos) return true;
            //    if (chPos + new Vector2Int(0, 1) == pos) return true;
            //    if (chPos + new Vector2Int(-1, 0) == pos) return true;
            //    if (chPos + new Vector2Int(0, -1) == pos) return true;
            //}

            return false;
        }


        internal Vector2Int FindEdgeSquareInDirection(Vector2Int start, Vector2Int direction)
        {
            if (ContainsWorldPos(start) == false) return NearestPoint(start);

            for (int i = 0; i < 100; i++)
            {
                Vector2Int pos = start + direction * i;
                if (ContainsWorldPos(pos) == false)
                    return start + direction * (i - 1);
            }

            return start;
        }

    }
}