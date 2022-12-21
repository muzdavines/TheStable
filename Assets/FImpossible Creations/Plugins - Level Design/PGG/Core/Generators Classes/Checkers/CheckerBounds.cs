using System;
using UnityEngine;

namespace FIMSpace.Generating.Checker
{
    public partial class CheckerField
    {
        /// <summary> Speeding up significantly collision checking and some other operations 
        /// But requires updating bounds scale when changing shape of CheckerField </summary>
        public bool UseBounds = true;

        // Building bounds on add/remove
        private void AddLocalPos(Vector2Int pos)
        {
            ChildPos.AddCell(pos);

            if (UseBounds == false) return;

            if (Bounding.Count == 0) Bounding.Add(new CheckerBounds(this, pos));
            else
                Bounding[0].EncapsulateLocal(pos.x, pos.y);
        }

        private void RemoveLocalPos(Vector2Int pos, bool rebuild = true)
        {
            ChildPos.Remove(pos);
        }


        public void PushPositionX(int offsetX)
        {
            MoveToPosition(new Vector2Int(parentPosition.x + offsetX, parentPosition.y));
        }

        public void PushPositionY(int offsetY)
        {
            MoveToPosition(new Vector2Int(parentPosition.x, parentPosition.y + offsetY));
        }

        public void MoveToPosition(Vector2Int newPos)
        {
            parentPosition = newPos;
            RefreshBounds();
        }

        public CheckerPos GetLocalPos(Vector2Int pos)
        {
            return ChildPos.GetCell(pos, false, true);
        }


        /// <summary> Recalculating bounds, clearing and generating all from zero, it generates multiple bounds if chekcer field shape is unregular </summary>
        public void RecalculateMultiBounds()
        {
            Bounding.Clear();
            if (UseBounds == false) return;

            CheckerField graphCopy = Copy();
            for (int i = 0; i <= 1000; i++)
            {
                if (graphCopy.ChildPos.AllApprovedCells.Count == 0) break;

                var startCell = graphCopy.ChildPos.AllApprovedCells[0];
                CheckerBounds iBounds = new CheckerBounds(this, startCell.ToV2());

                // Nearest margins cells variables
                CheckerPos negX, negY, posX, posY;
                negX = new CheckerPos(); negY = new CheckerPos(); posX = new CheckerPos(); posY = new CheckerPos();

                CheckGraphForNearestMargins(graphCopy, ChildPos.AllApprovedCells.Count, startCell, ref posX, ref negX, ref posY, ref negY);
                graphCopy.RemoveLocal(startCell);

                if ((negX != null || negY != null) && (posX != null || posY != null))
                {
                    for (int x = negX.x; x <= posX.x; x++)
                        for (int y = negY.y; y <= posY.y; y++)
                        {

                            if (FGenerators.CheckIfIsNull(ChildPos.GetCell(x, y, false, true) )) continue;
                            iBounds.EncapsulateLocal(x, y);
                            graphCopy.RemoveLocal(x, y);
                        }

                    Bounding.Add(iBounds);
                }

                if (i == 1000) UnityEngine.Debug.Log("Safety end (1000 iterations, bounds created: " + Bounding.Count);
            }

            //UnityEngine.Debug.Log(", bounds created: " + Bounding.Count);
        }


        public static void CheckGraphForNearestMargins(CheckerField grid, int maxCells, CheckerPos root, ref CheckerPos px, ref CheckerPos nx, ref CheckerPos pz, ref CheckerPos nz)
        {
            CheckerPos preCell = root;
            Vector2Int startPos = root.ToV2();
            pz = null;

            // Going with x from -max to +max
            // Going with z from zero to +max
            for (int x = 0; x <= maxCells; ++x)
            {
                CheckerPos xCell = grid.GetLocalPos(startPos + new Vector2Int(x, 0));
                if (FGenerators.CheckIfIsNull(xCell)) break;

                for (int z = 0; z <= maxCells; ++z)
                {
                    if (x == 0 && z == 0) continue;
                    CheckerPos zCell = grid.GetLocalPos(startPos + new Vector2Int(x, z));
                    if (FGenerators.CheckIfExist_NOTNULL(zCell)) preCell = zCell;
                    else // No further cells
                    {
                        if (pz == null) pz = preCell; // Getting minimum positive z value cell
                        else if (preCell.y < pz.y) pz = preCell;
                        break;
                    }
                }
            }
            for (int x = 1; x <= maxCells; ++x)
            {
                CheckerPos xCell = grid.GetLocalPos(startPos + new Vector2Int(-x, 0));
                if (FGenerators.CheckIfIsNull(xCell)) break;

                for (int z = 0; z <= maxCells; ++z)
                {
                    if (x == 0 && z == 0) continue;
                    CheckerPos zCell = grid.GetLocalPos(startPos + new Vector2Int(-x, z));
                    if (FGenerators.CheckIfExist_NOTNULL(zCell)) preCell = zCell;
                    else // No further cells
                    {
                        if (pz == null) pz = preCell;
                        else if (preCell.y < pz.y) pz = preCell;
                        break;
                    }
                }
            }



            preCell = root;
            px = null;
            // Going with z from -max to +max
            // Going with x from zero to +max
            for (int zz = 0; zz <= maxCells; ++zz)
            {
                CheckerPos zzCell = grid.GetLocalPos(startPos + new Vector2Int(0, zz));
                if (FGenerators.CheckIfIsNull(zzCell)) break;

                for (int xx = 0; xx <= maxCells; ++xx)
                {
                    if (zz == 0 && xx == 0) continue;
                    CheckerPos xCell = grid.GetLocalPos(startPos + new Vector2Int(xx, zz));
                    if (FGenerators.CheckIfExist_NOTNULL(xCell)) preCell = xCell;
                    else // No further cells
                    {
                        if (px == null) px = preCell;
                        else if (preCell.x < px.x) px = preCell;
                        break;
                    }
                }
            }
            for (int zz = 1; zz <= maxCells; ++zz)
            {
                CheckerPos zzCell = grid.GetLocalPos(startPos + new Vector2Int(0, -zz));
                if (FGenerators.CheckIfIsNull(zzCell )) break;

                for (int xx = 0; xx <= maxCells; ++xx)
                {
                    if (zz == 0 && xx == 0) continue;
                    CheckerPos xCell = grid.GetLocalPos(startPos + new Vector2Int(xx, -zz));
                    if (FGenerators.CheckIfExist_NOTNULL(xCell)) preCell = xCell;
                    else // No further cells
                    {
                        if (px == null) px = preCell;
                        else if (preCell.x < px.x) px = preCell;
                        break;
                    }
                }
            }


            preCell = root;
            nz = null;
            // Going with x from -max to +max
            // Going with z from zero to -max
            for (int x = 0; x <= maxCells; ++x)
            {
                CheckerPos xCell = grid.GetLocalPos(startPos + new Vector2Int(x, 0));
                if (FGenerators.CheckIfIsNull(xCell)) break;
                for (int z = 0; z <= maxCells; ++z)
                {
                    if (x == 0 && z == 0) continue;
                    CheckerPos zCell = grid.GetLocalPos(startPos + new Vector2Int(x, -z));
                    if (FGenerators.CheckIfExist_NOTNULL(zCell)) preCell = zCell;
                    else // No further cells
                    {
                        if (nz == null) nz = preCell; // Getting maximum negative z value cell
                        else if (preCell.y > nz.y) nz = preCell;
                        break;
                    }
                }
            }
            for (int x = 1; x <= maxCells; ++x)
            {
                CheckerPos xCell = grid.GetLocalPos(startPos + new Vector2Int(-x, 0));
                if (FGenerators.CheckIfIsNull(xCell)) break;
                for (int z = 0; z <= maxCells; z++)
                {
                    if (x == 0 && z == 0) continue;

                    CheckerPos zCell = grid.GetLocalPos(startPos + new Vector2Int(-x, -z));
                    if (FGenerators.CheckIfExist_NOTNULL(zCell)) preCell = zCell;
                    else // No further cells
                    {
                        if (nz == null) nz = preCell;
                        else if (preCell.y > nz.y) nz = preCell;
                        break;
                    }
                }
            }


            preCell = root;
            nx = null;
            // Going with z from -max to +max
            // Going with x from zero to -max
            for (int zz = 0; zz <= maxCells; ++zz) //var xCell = grid.GetLocalPos(root + new Vector2Int(x, 0, 0));
            {
                CheckerPos xCell = grid.GetLocalPos(startPos + new Vector2Int(0, zz));
                if (FGenerators.CheckIfIsNull(xCell)) break;

                for (int xx = 0; xx <= maxCells; ++xx)
                {
                    if (zz == 0 && xx == 0) continue;

                    CheckerPos zCell = grid.GetLocalPos(startPos + new Vector2Int(-xx, zz));
                    if (FGenerators.CheckIfExist_NOTNULL(zCell)) preCell = zCell;
                    else // No further cells
                    {
                        if (nx == null) nx = preCell; // Getting minimum positive z value cell
                        else if (preCell.x > nx.x) nx = preCell;
                        break;
                    }
                }
            }
            for (int zz = 1; zz <= maxCells; ++zz) // going with x negatively -> GetLocalPos pos - x
            {
                CheckerPos xCell = grid.GetLocalPos(startPos + new Vector2Int(0, -zz));
                if (FGenerators.CheckIfIsNull(xCell)) break;

                for (int xx = 0; xx <= maxCells; ++xx)
                {
                    if (zz == 0 && xx == 0) continue;
                    CheckerPos zCell = grid.GetLocalPos(startPos + new Vector2Int(-xx, -zz));
                    if (FGenerators.CheckIfExist_NOTNULL(zCell)) preCell = zCell;
                    else // No further cells
                    {
                        if (nx == null) nx = preCell; // Getting minimum positive z value cell
                        else if (preCell.x > nx.x) nx = preCell;
                        break;
                    }
                }
            }

        }




        /// <summary> Refreshing positioning of bounds with parent ChekcerField position </summary>
        public void RefreshBounds()
        {
            if (UseBounds == false) return;
            for (int i = 0; i < Bounding.Count; ++i) Bounding[i].RefreshWorldPos();
        }

        public void ClearAll()
        {
            Bounding.Clear();
            ChildPos = new FCheckerGraph<CheckerPos>(true);
        }

        /// <summary> Auto generated bounds when changing checker field squares positions for much faster collision checking etc. </summary>
        public class CheckerBounds
        {
            public CheckerField parent;

            public Vector2 localMin;
            public Vector2 localMax;

            public Vector2 min;
            public Vector2 max;

            public CheckerBounds(CheckerField owner, Vector2 localPos)
            {
                parent = owner;
                localMin = localPos;
                localMax = localPos;
                RefreshWorldPos();
            }

            public CheckerBounds(CheckerField owner, Vector2 locMin, Vector2 locMax)
            {
                parent = owner;
                localMin = locMin;
                localMax = locMax;
                RefreshWorldPos();
            }

            static readonly Vector2 staticOffsetMax = new Vector2(1.5f, 1.5f);
            static readonly Vector2 staticOffsetMin = new Vector2(0.5f, 0.5f);
            public void RefreshWorldPos()
            {
                max = localMax + parent.Position + staticOffsetMax + new Vector2(parent.FloatingOffset.x, parent.FloatingOffset.z);
                min = localMin + parent.Position + staticOffsetMin + new Vector2(parent.FloatingOffset.x, parent.FloatingOffset.z);
            }

            public void EncapsulateLocal(float x, float y)
            {
                if (x > localMax.x) localMax.x = x;
                else if (x < localMin.x) localMin.x = x;

                if (y > localMax.y) localMax.y = y;
                else if (y < localMin.y) localMin.y = y;

                RefreshWorldPos();
            }

            public void EncapsulateLocal(Vector2 pos)
            {
                EncapsulateLocal(pos.x, pos.y);
            }

            public void Encapsulate(CheckerPos pos)
            {
                if (pos.x > max.x) max.x = pos.x;
                else if (pos.x < min.x) min.x = pos.x;

                if (pos.y > max.y) max.y = pos.y;
                else if (pos.y < min.y) min.y = pos.y;
            }

            public void Encapsulate(CheckerBounds other)
            {
                if (other.max.x > max.x) max.x = other.max.x;
                else if (other.min.x < min.x) min.x = other.min.x;

                if (other.max.y > max.y) max.y = other.max.y;
                else if (other.min.y < min.y) min.y = other.min.y;
            }

            public bool Intersects(CheckerBounds o)
            {
                return (max.x > o.min.x && min.x < o.max.x &&
                 max.y > o.min.y && min.y < o.max.y);
            }

            public bool IsInside(Vector2 pos)
            {
                return (pos.x > min.x && pos.x < max.x && pos.y > min.y && pos.y < max.y);
            }

            /// <summary>
            /// If other bound is fully inside
            /// </summary>
            public bool IsInside(CheckerBounds o)
            {
                return (o.min.x <= min.x && o.max.x >= max.x && o.min.y <= min.y && o.max.y >= max.y);
            }

            public bool IsInsideOrEdge(Vector2 pos)
            {
                return (pos.x >= min.x && pos.x <= max.x && pos.y >= min.y && pos.y <= max.y);
            }

            public bool IsOnEdge(Vector2 pos)
            {
                Vector2 posmax = pos + staticOffsetMax;
                Vector2 posmin = pos + staticOffsetMin;

                return
                 ((posmin.x == min.x || posmax.x == max.x) && posmax.y >= min.y && posmin.y <= max.y)
                 || ((posmin.y == min.y || posmax.y == max.y) && posmax.x >= min.x && posmin.x <= max.x);
            }

            public bool IsOnEdge(CheckerBounds o)
            {
                if (max.x == o.min.x)
                {
                    if (min.y < o.max.y && max.y > o.min.y) return true;
                }
                else if (min.x == o.max.x)
                {
                    if (min.y < o.max.y && max.y > o.min.y) return true;
                }
                else if (max.y == o.min.y)
                {
                    if (min.x < o.max.x && max.x > o.min.x) return true;
                }
                else if (min.y == o.max.y)
                {
                    if (min.x < o.max.x && max.x > o.min.x) return true;
                }

                return false;
            }

            public Vector2 GetSize()
            {
                Vector2 size = new Vector2(min.x - max.x, min.y - max.y);
                if (size.x < 0) size.x = -size.x;
                if (size.y < 0) size.y = -size.y;
                return size;
            }

            public void DrawGizmo(float scaleUp, bool fill = false)
            {
                Vector3 center = Vector3.LerpUnclamped(min.V2toV3() * scaleUp, max.V2toV3() * scaleUp, 0.5f);

                if (!fill)
                    Gizmos.DrawWireCube(center, GetSize().V2toV3() * 1.05f * scaleUp);
                else
                    Gizmos.DrawCube(center, GetSize().V2toV3() * scaleUp);

                //Gizmos.DrawSphere(parent.Position.V2toV3() * scaleUp, 0.125f);
                //Gizmos.DrawSphere(min.V2toV3() * scaleUp, 0.2f);
                //Gizmos.DrawSphere(max.V2toV3() * scaleUp, 0.2f);
            }

            public void LogBounds(float scaleUp, Color col)
            {
                //Vector3 center = Vector3.LerpUnclamped(min.V2toV3() * scaleUp, max.V2toV3() * scaleUp, 0.5f);
                Vector2 min = this.min * scaleUp;
                Vector2 max = this.max * scaleUp;

                Vector3 p1 = min.V2toV3();
                Vector3 p2 = new Vector3(p1.x, 0, max.y);
                Vector3 p3 = new Vector3(max.x, 0, max.y);
                Vector3 p4 = new Vector3(max.x, 0, min.y);

                Debug.DrawLine(p1, p2, col, 1.1f);
                Debug.DrawLine(p2, p3, col, 1.1f);
                Debug.DrawLine(p3, p4, col, 1.1f);
                Debug.DrawLine(p4, p1, col, 1.1f);
            }

            internal CheckerBounds Copy(CheckerField newParent)
            {
                CheckerBounds b = (CheckerBounds)MemberwiseClone();
                b.localMin = localMin;
                b.localMax = localMax;
                b.min = min;
                b.max = max;
                b.parent = newParent;
                return b;
            }


            /// <summary> Getting nearest point on bounds margins</summary>
            internal Vector2Int GetNearestTo(Vector2Int worldPos)
            {
                Vector2 wp = new Vector2(worldPos.x, worldPos.y);
                Vector2 leftM = GetNearestPositionToLine(new Vector2(min.x, min.y), new Vector2(min.x, max.y), wp);
                float ld = Vector2.Distance(leftM, wp);

                Vector2 rM = GetNearestPositionToLine(new Vector2(max.x, min.y), new Vector2(max.x, max.y), wp);
                float rd = Vector2.Distance(rM, wp);

                Vector2 uM = GetNearestPositionToLine(new Vector2(min.x, max.y), new Vector2(max.x, max.y), wp);
                float ud = Vector2.Distance(uM, wp);

                Vector2 dM = GetNearestPositionToLine(new Vector2(min.x, min.y), new Vector2(max.x, min.y), wp);
                float dd = Vector2.Distance(dM, wp);

                float nearest = ld;
                Vector2 nearestV = leftM;

                if (rd < nearest) { nearestV = rM; }
                if (ud < nearest) { nearestV = uM; }
                if (dd < nearest) { nearestV = dM; }

                return nearestV.V2toV2Int();
            }

            private Vector2 GetNearestPositionToLine(Vector2 lineStart, Vector2 lineEnd, Vector2 from)
            {
                Vector2 dirVector1 = from - lineStart;
                Vector2 dirVector2 = (lineEnd - lineStart).normalized;

                float distance = Vector2.Distance(lineStart, lineEnd);
                float dot = Vector2.Dot(dirVector2, dirVector1);

                if (dot <= 0) return lineStart;
                if (dot >= distance) return lineEnd;

                Vector2 dotVector = dirVector2 * dot;
                Vector2 closestPoint = lineStart + dotVector;

                return closestPoint;
            }


        }

        internal void LogBounds(float scaleUp, Color color)
        {
            for (int i = 0; i < Bounding.Count; i++)
            {
                Bounding[i].LogBounds(scaleUp, color);
            }
        }
    }
}