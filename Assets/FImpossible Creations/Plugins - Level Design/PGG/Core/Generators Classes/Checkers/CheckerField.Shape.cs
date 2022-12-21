using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Checker
{
    public partial class CheckerField
    {

        public void SetSize(Vector2Int size, Vector2Int? origin = null)
        {
            ClearAll();
            LastSettedSize = size;
            Vector2Int originPush = Vector2Int.zero;
            if (origin != null) originPush = origin.Value;

            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                    AddLocalPos((new Vector2Int(x, y) - originPush));
        }


        internal bool ContainsFully(CheckerField pathCheck)
        {
            for (int i = 0; i < Bounding.Count; i++)
                for (int o = 0; o < pathCheck.Bounding.Count; o++)
                {
                    if (pathCheck.Bounding[o].IsInside(Bounding[i]) == false)
                        return false;
                }

            return true;

            //for (int i = 0; i < pathCheck.ChildPos.Count; i++)
            //{
            //    if (!ContainsWorldPos(pathCheck.WorldPos(i))) return false;
            //}

            //return true;
        }


        public Vector2Int GetCenter()
        {
            return GetBoundingBox().center.V3toV2Int();
        }


        public void SetSize(int x, int y, Vector2Int? origin = null)
        {
            SetSize(new Vector2Int(x, y), origin);
        }


        /// <summary>
        /// Default origin in 0,0 and x going right, y going up
        /// </summary>
        public void SetSize(int x, int y, bool center)
        {
            if (center)
                SetSize(new Vector2Int(x, y), new Vector2Int(x / 2, y / 2));
            else
                SetSize(new Vector2Int(x, y), null);
        }


        public void PushAllChildPositions(Vector2Int offset)
        {

            List<Vector2Int> newPos = new List<Vector2Int>();
            for (int c = 0; c < ChildPos.Count; c++)
            {
                Vector2Int transposed = ChildPosition(c) + offset;
                newPos.Add(transposed);
            }

            ChildPos.Clear();
            for (int i = 0; i < newPos.Count; i++)
                ChildPos.Add(newPos[i]);

            //for (int i = ChildPos.Count-1; i >= 0; i--) OffsetChildCell(i, offset);
            //for (int i = 0; i < ChildPos.Count; i++) OffsetChildCell(i, offset);
        }


        internal int GetSizeOnAxis(Vector2 direction)
        {
            if (direction.x != 0) return GetBoundsSize().x;
            else return GetBoundsSize().y;
        }


        public Vector2Int GetBoundsSize()
        {
            Vector4 minMax = GetBoundsMinMax();
            return new Vector2Int(Mathf.Abs(minMax.x - minMax.y).ToInt(), Mathf.Abs(minMax.z - minMax.w).ToInt());
        }


        /// <summary> Returns (minX, maxX, minY, maxY) </summary>
        public Vector4 GetBoundsMinMax(bool local = false)
        {
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;

            for (int i = 0; i < ChildPos.Count; i++)
            {
                if (ChildPosition(i).x < minX) minX = ChildPosition(i).x;
                if (ChildPosition(i).x > maxX) maxX = ChildPosition(i).x;

                if (ChildPosition(i).y < minY) minY = ChildPosition(i).y;
                if (ChildPosition(i).y > maxY) maxY = ChildPosition(i).y;
            }

            if (local)
                return new Vector4(minX, maxX, minY, maxY);
            else
                return new Vector4(Position.x + minX, Position.x + maxX, Position.y + minY, Position.y + maxY);
        }


        public Vector2Int GetBoundsMin()
        {
            Vector4 minMax = GetBoundsMinMax();
            return new Vector2Int(Mathf.RoundToInt(minMax.x), Mathf.RoundToInt(minMax.z));
        }


        public Vector2Int GetBoundsMax()
        {
            Vector4 minMax = GetBoundsMinMax();
            return new Vector2Int(Mathf.RoundToInt(minMax.y), Mathf.RoundToInt(minMax.w));
        }


        internal Bounds GetBoundingBox(float scale = 1f)
        {
            Bounds b = new Bounds(GridPos(0).V2toV3Bound() * scale, Vector3.one * scale);

            for (int i = 1; i < ChildPos.Count; i++)
                b.Encapsulate(new Bounds(GridPos(i).V2toV3Bound() * scale, Vector3.one * scale));

            b.center += new Vector3(1, 0, 1) * scale * 0.5f;

            return b;
        }


        internal Vector2Int FromWorldToGridPos(Vector2Int nearestOwn)
        {
            return nearestOwn + Vector2Int.one;
        }


        public void FillSquareInDirection(Vector2Int startPos, Vector2Int dir, int thickness, int off = 0)
        {
            dir = ((Vector2)dir).normalized.V2toV2Int();
            Vector2Int sideDir = PGGUtils.GetRotatedFlatDirectionFrom(dir);

            for (int s = 1; s <= thickness / 2 + thickness % 2; s++)
                for (int t = -thickness / 2 + off; t <= thickness / 2 - off; t++)
                    Add(startPos + dir * s + sideDir * t);
        }


        public void ClearSquareInDirection(Vector2Int startPos, Vector2Int dir, int thickness)
        {
            dir = ((Vector2)dir).normalized.V2toV2Int();
            Vector2Int sideDir = PGGUtils.GetRotatedFlatDirectionFrom(dir);

            for (int s = 1; s <= thickness + thickness % 2; s++)
                for (int t = -thickness / 2; t <= thickness / 2; t++)
                    Remove(startPos + dir * s + sideDir * t);
        }


        public void Rotate(int clockwise90)
        {
            if (clockwise90 % 4 == 0) return;

            Matrix4x4 rotM = Matrix4x4.Rotate(Quaternion.Euler(0, clockwise90 * 90, 0));

            List<Vector2Int> newPos = new List<Vector2Int>();
            for (int c = 0; c < ChildPos.Count; c++)
            {
                Vector3 transposed = rotM.MultiplyPoint(ChildPosition(c).V2toV3(1));
                newPos.Add(transposed.V3toV2Int());
            }

            ChildPos.Clear();
            for (int i = 0; i < newPos.Count; i++)
                ChildPos.Add(newPos[i]);

            if (UseBounds == false) return;

            for (int i = 0; i < Bounding.Count; i++)
            {
                Vector2 preMn = Bounding[i].localMin;
                Vector2 preMx = Bounding[i].localMax;

                Bounding[i] = new CheckerBounds(this, rotM.MultiplyPoint(preMn.V2toV3(1)).V3toV2());
                Bounding[i].EncapsulateLocal(rotM.MultiplyPoint(preMx.V2toV3(1)).V3toV2());
            }
        }


        public enum ECheckerMeasureMode { Rectangular, Spherical }
        public CheckerField GetOutlineChecker(int thickness = 1, ECheckerMeasureMode edgesShape = ECheckerMeasureMode.Rectangular)
        {
            CheckerField outline = Copy();

            if (thickness > 0)
                for (int i = 0; i < ChildPos.Count; i++)
                {
                    var pos = ChildPos.AllApprovedCells[i];

                    for (int x = -thickness; x <= thickness; x++)
                    {
                        for (int z = -thickness; z <= thickness; z++)
                        {
                            if (x == 0 && z == 0) continue;
                            
                            if ( edgesShape == ECheckerMeasureMode.Spherical)
                            {
                                if (Mathf.Abs(x) == thickness)
                                if (Mathf.Abs(z) == thickness) continue;
                            }

                            outline.AddLocalPos(pos.ToV2() + new Vector2Int(x, z));
                        }
                    }

                    #region Old Version
                    //for (int t = 1; t <= thickness; t++)
                    //{
                    //    var pos = ChildPos.AllApprovedCells[i];

                    //    outline.AddLocalPos(pos.ToV2() + new Vector2Int(t, 0));
                    //    outline.AddLocalPos(pos.ToV2() + new Vector2Int(-t, 0));
                    //    outline.AddLocalPos(pos.ToV2() + new Vector2Int(0, t));
                    //    outline.AddLocalPos(pos.ToV2() + new Vector2Int(0, -t));

                    //    if (edgesShape == ECheckerMeasureMode.Rectangular || t < thickness)
                    //    {
                    //        outline.AddLocalPos(pos.ToV2() + new Vector2Int(-t, -t));
                    //        outline.AddLocalPos(pos.ToV2() + new Vector2Int(t, t));
                    //        outline.AddLocalPos(pos.ToV2() + new Vector2Int(-t, t));
                    //        outline.AddLocalPos(pos.ToV2() + new Vector2Int(t, -t));
                    //    }
                    //}
                    #endregion

                }

            outline.RemoveOnesCollidingWith(this, false);

            return outline;
        }

    }
}