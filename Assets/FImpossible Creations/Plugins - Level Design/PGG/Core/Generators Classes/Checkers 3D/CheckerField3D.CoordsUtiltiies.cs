using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Checker
{
    public partial class CheckerField3D
    {
        public Transform AttachRootTo = null;

        /// <summary> Computing field matrix - if needed to use multiple times, it's better to keep it as local variable </summary>
        public Matrix4x4 Matrix
        {
            get
            {
                return Matrix4x4.TRS(RootPosition, RootRotation, RootScale);
            }
        }

        public Matrix4x4 Matrix_NoScale
        {
            get
            {
                return Matrix4x4.TRS(RootPosition, RootRotation, Vector3.one);
            }
        }

        public Vector3 GetGridWorldMin()
        {
            return (Grid.GetMin() + RootPosition);
        }

        public Vector3 GetGridWorldMax()
        {
            return (Grid.GetMax() + RootPosition);
        }

        public Bounds LimitLocalBoundsToGrid(Bounds localBounds)
        {
            Vector3 nMin = localBounds.min;
            Vector3 nMax = localBounds.max;

            if (Grid.MinX.Pos.x > nMin.x) nMin.x = Grid.MinX.Pos.x - 0.5f;
            if (nMax.x > Grid.MaxX.Pos.x) nMax.x = Grid.MaxX.Pos.x + 0.5f;

            if (Grid.MinY.Pos.y > nMin.y) nMin.y = Grid.MinY.Pos.y - 0.5f;
            if (nMax.y > Grid.MaxY.Pos.y) nMax.y = Grid.MaxY.Pos.y + 0.5f;

            if (Grid.MinZ.Pos.z > nMin.z) nMin.z = Grid.MinZ.Pos.z - 0.5f;
            if (nMax.z > Grid.MaxZ.Pos.z) nMax.z = Grid.MaxZ.Pos.z + 0.5f;


            localBounds.SetMinMax(nMin, nMax);
            FDebug.DrawBounds2D(LocalToWorldBounds(localBounds), Color.red, 0, 1, 0.01f);

            return localBounds;
        }

        private List<FieldCell> _tempHelpCellsList = new List<FieldCell>();
        public List<FieldCell> BoundsToCells(Bounds localBounds, bool getRounded = true/*, bool worldMatrix = true*/)
        {
            _tempHelpCellsList.Clear();
            List<FieldCell> cells = _tempHelpCellsList;

            //localBounds = LimitLocalBoundsToGrid(localBounds);

            int lX, rX, uZ, dZ, uY, dY;

            if (getRounded)
            {

                float xDiff = localBounds.max.x - localBounds.min.x;
                if (xDiff >= 0.5f)
                {
                    lX = Mathf.FloorToInt(localBounds.min.x + 0.5f);
                    rX = Mathf.CeilToInt(localBounds.max.x - 0.5f);
                }
                else
                {
                    lX = Mathf.FloorToInt(localBounds.min.x);
                    rX = Mathf.CeilToInt(localBounds.max.x);
                }

                float yDiff = localBounds.max.y - localBounds.min.y;
                if (yDiff > 0.5f)
                {
                    dY = Mathf.FloorToInt(localBounds.min.y + 0.5f);
                    uY = Mathf.CeilToInt(localBounds.max.y - 0.5f);
                }
                else
                {
                    uY = Mathf.RoundToInt(localBounds.min.y);
                    dY = Mathf.RoundToInt(localBounds.max.y);
                }

                float zDiff = localBounds.max.z - localBounds.min.z;
                if (zDiff >= 0.5f)
                {
                    uZ = Mathf.CeilToInt(localBounds.max.z - 0.5f);
                    dZ = Mathf.FloorToInt(localBounds.min.z + 0.5f);
                }
                else
                {
                    uZ = Mathf.CeilToInt(localBounds.max.z);
                    dZ = Mathf.FloorToInt(localBounds.min.z);
                }

            }
            else
            {

                lX = Mathf.RoundToInt(localBounds.min.x + 0.5f);
                rX = Mathf.RoundToInt(localBounds.max.x - 0.5f);
                uZ = Mathf.RoundToInt(localBounds.max.z - 0.5f);
                dZ = Mathf.RoundToInt(localBounds.min.z + 0.5f);
                uY = Mathf.RoundToInt(localBounds.max.y - 0.5f);
                dY = Mathf.RoundToInt(localBounds.min.y + 0.5f);
            }

            for (int x = lX; x <= rX; x++)
            {
                for (int y = dY; y <= uY; y++)
                {
                    for (int z = dZ; z <= uZ; z++)
                    {
                        FieldCell c = Grid.GetCell(new Vector3Int(x, y, z), false);
                        if (FGenerators.CheckIfExist_NOTNULL(c)) cells.Add(c);
                    }
                }
            }

            return cells;
        }


        public Bounds? GetCollidingBound(CheckerField3D other)
        {
            for (int i = 0; i < Bounding.Count; i++)
            {
                for (int o = 0; o < other.Bounding.Count; o++)
                {
                    Bounds iWorld = Bounding[i];
                    iWorld.center += RootPosition;
                    iWorld.size *= 0.999f;

                    Bounds oWorld = other.Bounding[o];
                    oWorld.center += other.RootPosition;
                    oWorld.size *= 0.999f;

                    if (iWorld.Intersects(oWorld))
                    {
                        return Bounding[i];
                    }
                }
            }

            return null;
        }

        /// <summary> Empty bounds if no intersection </summary>
        public Bounds GetBoundsPenetration(Bounds a, Bounds b)
        {
            Bounds p = new Bounds();
            //if (a.Intersects(b) == false) return p;

            Vector3 nMax = Vector3.zero;
            Vector3 nMin = Vector3.zero;

            // If bounds intersects each on X axis
            if ((a.min.x <= b.max.x) && (a.max.x >= b.min.x))
                if ((a.min.y <= b.max.y) && (a.max.y >= b.min.y))
                    if ((a.min.z <= b.max.z) && (a.max.z >= b.min.z))
                    {
                        //LimitBoundsAxis(ref nMin.x, ref nMax.x, a.min.x, a.max.x, b.min.x, b.max.x);
                        if (b.max.x > a.max.x) nMax.x = a.max.x;
                        if (b.max.x > a.min.x) nMin.x = a.min.x;
                        if (b.min.x > a.min.x) nMin.x = b.min.x;
                        if (b.max.x < a.max.x) nMax.x = b.max.x;
                        if (a.min.x < b.min.x) nMin.x = b.min.x;
                        //}

                        //if ((a.min.y <= b.max.y) && (a.max.y >= b.min.y))
                        {
                            if (b.max.y > a.max.y) nMax.y = a.max.y;
                            if (b.max.y > a.min.y) nMin.y = a.min.y;
                            if (b.min.y > a.min.y) nMin.y = b.min.y;
                            if (b.max.y < a.max.y) nMax.y = b.max.y;
                            if (a.min.y < b.min.y) nMin.y = b.min.y;
                        }

                        if ((a.min.z <= b.max.z) && (a.max.z >= b.min.z))
                        {
                            if (b.max.z > a.max.z) nMax.z = a.max.z;
                            if (b.max.z > a.min.z) nMin.z = a.min.z;
                            if (b.min.z > a.min.z) nMin.z = b.min.z;
                            if (b.max.z < a.max.z) nMax.z = b.max.z;
                            if (a.min.z < b.min.z) nMin.z = b.min.z;
                        }
                    }

            p.SetMinMax(nMin, nMax);

            return p;
        }

        //void LimitBoundsAxis(ref float min, ref float max, float aMin, float aMax, float bMin, float bMax)
        //{
        //    if (bMax > aMax) max = aMax;
        //    if (bMax > aMin) min = aMin;
        //    if (bMin > aMin) min = bMin;
        //    if (bMax < aMax) max = bMax;
        //    if (aMin < bMin) min = bMin;
        //}

        [NonSerialized] public FieldCell _IsCollidingWith_MyFirstCollisionCell = null;
        public bool IsCollidingWith(List<CheckerField3D> others)
        {
            for (int i = 0; i < others.Count; i++)
            {
                if (others[i] == this) continue;

                if (IsCollidingWith(others[i], true))
                {
                    return true;
                }
            }

            return false;
        }

        public Planning.FieldPlanner IsCollidingWith(List<Planning.FieldPlanner> others)
        {
            for (int i = 0; i < others.Count; i++)
            {
                if (others[i].LatestChecker == this) continue;

                if (IsCollidingWith(others[i].LatestChecker, true))
                {
                    return others[i];
                }
            }

            return null;
        }

        public bool IsCollidingWith(CheckerField3D other, bool checkRounded = false)
        {
            if (!UseBounds)
            {
                Bounds mInt = GetCollisionBounds(other);

                // Quick check if collision is far out
                if (mInt.size == Vector3.zero) return false;

                Bounds boundForCells = GetTransponedBounding(mInt, true);

                // Checking cells in bounds intersect area
                var intersAreaCells = BoundsToCells(boundForCells);
                Matrix4x4 tMx = Matrix;
                Matrix4x4 otMx = other.Matrix.inverse;

                for (int i = 0; i < intersAreaCells.Count; i++)
                {
                    if (other.ContainsWorld(tMx.MultiplyPoint3x4(intersAreaCells[i].Pos), otMx, checkRounded))
                    {
                        _IsCollidingWith_MyFirstCollisionCell = intersAreaCells[i];
                        return true;
                    }
                }

                //if (DebugHelper) FDebug.DrawBounds2D((mInt), Color.red);

                //Bounds boundForCells = other.GetTransponedBounding(mInt, true);
                //return other.IntersectsWorldBounds(mInt);
                //var intersAreaCells = other.BoundsToCells(boundForCells);

            }
            else
            {
                for (int i = 0; i < Bounding.Count; i++)
                {
                    Bounds iWorld = Bounding[i];
                    iWorld.center += RootPosition;
                    iWorld.size *= 0.999f;

                    for (int o = 0; o < other.Bounding.Count; o++)
                    {
                        Bounds oWorld = other.Bounding[o];
                        oWorld.center += other.RootPosition;
                        oWorld.size *= 0.999f;

                        if (iWorld.Intersects(oWorld)) return true;
                    }
                }
            }

            return false;
        }

        private bool IntersectsWorldBounds(Bounds mInt)
        {


            return false;
        }

        public Bounds GetCollisionBounds(CheckerField3D other)
        {
            Bounds oB = other.GetFullBoundsWorldSpace();
            oB.size *= 0.999f;
            Bounds mB = GetFullBoundsWorldSpace();

            return GetBoundsPenetration(mB, oB);
        }

        private static readonly List<FieldCell> _emptyList = new List<FieldCell>();
        private static readonly List<FieldCell> _collisionList = new List<FieldCell>();
        /// <summary> For shared use, cells list must be copied, if count is zero then no collision detected </summary>
        public List<FieldCell> GetCollisionCells(CheckerField3D other)
        {
            Bounds cBound = GetCollisionBounds(other);
            if (cBound.size == Vector3.zero) return _emptyList;

            Bounds localBounds = GetTransponedBounding(cBound, true);

            // Checking cells in bounds intersect area
            var intersAreaCells = BoundsToCells(localBounds);
            Matrix4x4 tMx = Matrix;
            Matrix4x4 otMx = other.Matrix.inverse;
            _collisionList.Clear();

            for (int i = 0; i < intersAreaCells.Count; i++)
            {
                if (other.ContainsWorld(tMx.MultiplyPoint3x4(intersAreaCells[i].Pos), otMx))
                {
                    _collisionList.Add(intersAreaCells[i]);
                }
            }

            return _collisionList;
        }

        /// <summary>
        /// Supporting world space rotation bounding box
        /// </summary>
        public Bounds GetTransponedBounding(Bounds b, bool toLocal = false)
        {
            //if (!toLocal) b.size = ScaleV3(b.size);

            if (RootRotation == Quaternion.identity && RootScale == Vector3.one) // No need for matrix support - faster compute
            {
                if (!toLocal)
                    b.center += RootPosition;
                else
                    b.center -= RootPosition;

                return b;
            }

            Matrix4x4 mx = Matrix;
            if (toLocal) mx = mx.inverse;

            Vector3 min = mx.MultiplyPoint3x4(b.min);
            Vector3 max = mx.MultiplyPoint3x4(b.max);

            Vector3 minB = mx.MultiplyPoint3x4(new Vector3(b.max.x, b.center.y, b.min.z));
            Vector3 maxB = mx.MultiplyPoint3x4(new Vector3(b.min.x, b.center.y, b.max.z));

            b = new Bounds(min, Vector3.zero);

            b.Encapsulate(min);
            b.Encapsulate(max);
            b.Encapsulate(minB);
            b.Encapsulate(maxB);

            return b;
        }

        public Bounds GetFullBoundsLocalSpace()
        {
            Bounds b = new Bounds();
            Vector3 half = new Vector3(0.5f, 0.5f, 0.5f);
            b.SetMinMax(Grid.GetMin() - half, Grid.GetMax() + half);
            return b;
        }

        public Bounds GetFullBoundsWorldSpace()
        {
            Bounds fullB;

            if (Bounding.Count > 0)
                fullB = GetTransponedBounding(Bounding[0]);
            else
            {
                fullB = new Bounds();
                Vector3 half = new Vector3(0.5f, 0.5f, 0.5f);
                fullB.SetMinMax(Grid.GetMin() - half, Grid.GetMax() + half);
                fullB = GetTransponedBounding(fullB);
            }

            for (int i = 1; i < Bounding.Count; i++)
            {
                fullB.Encapsulate(GetTransponedBounding(Bounding[i]));
            }

            return fullB;
        }


        public static void CheckGraphForNearestMargins(FGenGraph<FieldCell, FGenPoint> grid, int maxCells, FieldCell root, ref FieldCell px, ref FieldCell nx, ref FieldCell pz, ref FieldCell nz, bool limitMinMax = true)
        {
            FieldCell preCell = root;
            pz = null;

            // Going with x from -max to +max
            // Going with z from zero to +max

            int maxX = maxCells;
            int minX = maxCells;
            int maxZ = maxCells;
            int minZ = maxCells;

            if (limitMinMax)
            {
                maxX = Mathf.Min(maxCells, grid.MaxX.Pos.x - root.Pos.x + 1);
                minX = Mathf.Min(maxCells, root.Pos.x - grid.MinX.Pos.x - 1);
                maxZ = Mathf.Min(maxCells, grid.MaxZ.Pos.z - root.Pos.z + 1);
                minZ = Mathf.Min(maxCells, root.Pos.z - grid.MinZ.Pos.z - 1);
            }

            for (int x = 0; x <= maxX; x++)
            {
                var xCell = grid.GetCell(root.Pos + new Vector3Int(x, 0, 0));
                if ((FGenerators.CheckIfIsNull(xCell)) || xCell.InTargetGridArea == false) break;

                for (int z = 0; z <= maxZ; z++)
                {
                    if (x == 0 && z == 0) continue;
                    var zCell = grid.GetCell(root.Pos + new Vector3Int(x, 0, z));
                    if ((FGenerators.CheckIfExist_NOTNULL(zCell)) && zCell.InTargetGridArea) preCell = zCell;
                    else // No further cells
                    {
                        if (pz == null) pz = preCell; // Getting minimum positive z value cell
                        else if (preCell.Pos.z < pz.Pos.z) pz = preCell;
                    }
                }
            }


            for (int x = 1; x <= minX; x++)
            {
                var xCell = grid.GetCell(root.Pos + new Vector3Int(-x, 0, 0));
                if ((FGenerators.CheckIfIsNull(xCell)) || !xCell.InTargetGridArea) break;

                for (int z = 0; z <= maxZ; z++)
                {
                    if (x == 0 && z == 0) continue;
                    var zCell = grid.GetCell(root.Pos + new Vector3Int(-x, 0, z));
                    if ((FGenerators.CheckIfExist_NOTNULL(zCell)) && zCell.InTargetGridArea) preCell = zCell;
                    else // No further cells
                    {
                        if (pz == null) pz = preCell;
                        else if (preCell.Pos.z < pz.Pos.z) pz = preCell;
                    }
                }
            }



            preCell = root;
            px = null;
            // Going with z from -max to +max
            // Going with x from zero to +max
            for (int zz = 0; zz <= maxZ; zz++)
            {
                var zzCell = grid.GetCell(root.Pos + new Vector3Int(0, 0, zz));
                if ((FGenerators.CheckIfIsNull(zzCell)) || !zzCell.InTargetGridArea) break;

                for (int xx = 0; xx <= maxX; xx++)
                {
                    if (zz == 0 && xx == 0) continue;
                    var xCell = grid.GetCell(root.Pos + new Vector3Int(xx, 0, zz));
                    if ((FGenerators.CheckIfExist_NOTNULL(xCell)) && xCell.InTargetGridArea) preCell = xCell;
                    else // No further cells
                    {
                        if (px == null) px = preCell;
                        else if (preCell.Pos.x < px.Pos.x) px = preCell;
                    }
                }
            }
            for (int zz = 1; zz <= minZ; zz++)
            {
                var zzCell = grid.GetCell(root.Pos + new Vector3Int(0, 0, -zz));
                if ((FGenerators.CheckIfIsNull(zzCell)) || !zzCell.InTargetGridArea) break;

                for (int xx = 0; xx <= maxX; xx++)
                {
                    if (zz == 0 && xx == 0) continue;
                    var xCell = grid.GetCell(root.Pos + new Vector3Int(xx, 0, -zz));
                    if ((FGenerators.CheckIfExist_NOTNULL(xCell)) && xCell.InTargetGridArea) preCell = xCell;
                    else // No further cells
                    {
                        if (px == null) px = preCell;
                        else if (preCell.Pos.x < px.Pos.x) px = preCell;
                    }
                }
            }


            preCell = root;
            nz = null;
            // Going with x from -max to +max
            // Going with z from zero to -max
            for (int x = 0; x <= maxX; x++)
            {
                var xCell = grid.GetCell(root.Pos + new Vector3Int(x, 0, 0));
                if ((FGenerators.CheckIfIsNull(xCell)) || !xCell.InTargetGridArea) break;
                for (int z = 0; z <= maxZ; z++)
                {
                    if (x == 0 && z == 0) continue;
                    var zCell = grid.GetCell(root.Pos + new Vector3Int(x, 0, -z));
                    if ((FGenerators.CheckIfExist_NOTNULL(zCell)) && zCell.InTargetGridArea) preCell = zCell;
                    else // No further cells
                    {
                        if (nz == null) nz = preCell; // Getting maximum negative z value cell
                        else if (preCell.Pos.z > nz.Pos.z) nz = preCell;
                    }
                }
            }
            for (int x = 1; x <= minX; x++)
            {
                var xCell = grid.GetCell(root.Pos + new Vector3Int(-x, 0, 0));
                if ((FGenerators.CheckIfIsNull(xCell)) || !xCell.InTargetGridArea) break;
                for (int z = 0; z <= maxZ; z++)
                {
                    if (x == 0 && z == 0) continue;

                    var zCell = grid.GetCell(root.Pos + new Vector3Int(-x, 0, -z));
                    if ((FGenerators.CheckIfExist_NOTNULL(zCell)) && zCell.InTargetGridArea) preCell = zCell;
                    else // No further cells
                    {
                        if (nz == null) nz = preCell;
                        else if (preCell.Pos.z > nz.Pos.z) nz = preCell;
                    }
                }
            }


            preCell = root;
            nx = null;
            // Going with z from -max to +max
            // Going with x from zero to -max
            for (int zz = 0; zz <= maxZ; zz++) //var xCell = grid.GetCell(root.Pos + new Vector3Int(x, 0, 0));
            {
                var xCell = grid.GetCell(root.Pos + new Vector3Int(0, 0, zz));
                if ((FGenerators.CheckIfIsNull(xCell)) || !xCell.InTargetGridArea) break;

                for (int xx = 0; xx <= maxX; xx++)
                {
                    if (zz == 0 && xx == 0) continue;

                    var zCell = grid.GetCell(root.Pos + new Vector3Int(-xx, 0, zz));
                    if ((FGenerators.CheckIfExist_NOTNULL(zCell)) && zCell.InTargetGridArea) preCell = zCell;
                    else // No further cells
                    {
                        if (nx == null) nx = preCell; // Getting minimum positive z value cell
                        else if (preCell.Pos.x > nx.Pos.x) nx = preCell;
                    }
                }
            }
            for (int zz = 1; zz <= minZ; zz++) // going with x negatively -> getcell pos - x
            {
                var xCell = grid.GetCell(root.Pos + new Vector3Int(0, 0, -zz));
                if ((FGenerators.CheckIfIsNull(xCell)) || !xCell.InTargetGridArea) break;

                for (int xx = 0; xx <= maxX; xx++)
                {
                    if (zz == 0 && xx == 0) continue;
                    var zCell = grid.GetCell(root.Pos + new Vector3Int(-xx, 0, -zz));
                    if ((FGenerators.CheckIfExist_NOTNULL(zCell)) && zCell.InTargetGridArea) preCell = zCell;
                    else // No further cells
                    {
                        if (nx == null) nx = preCell; // Getting minimum positive z value cell
                        else if (preCell.Pos.x > nx.Pos.x) nx = preCell;
                    }
                }
            }

            if (px == null) px = root;
            if (nx == null) nx = root;
            if (pz == null) pz = root;
            if (nz == null) nz = root;
        }

        public static Vector3 GetNearestPointToLine(Vector3 lineStart, Vector3 lineEnd, Vector3 from)
        {
            Vector3 dirVector1 = from - lineStart;
            Vector3 dirVector2 = (lineEnd - lineStart).normalized;

            float distance = Vector3.Distance(lineStart, lineEnd);
            float dot = Vector3.Dot(dirVector2, dirVector1);

            if (dot <= 0) return lineStart;

            if (dot >= distance) return lineEnd;

            Vector3 dotVector = dirVector2 * dot;

            Vector3 closestPoint = lineStart + dotVector;

            return closestPoint;
        }

    }
}