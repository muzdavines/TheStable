using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class FGenGraph<T1, T2> where T1 : FGenCell, new() where T2 : FGenPoint, new()
    {
        public List<T1> AllCells = new List<T1>();
        public List<T1> AllApprovedCells = new List<T1>();

        public FGenGrid<T1> Cells = new FGenGrid<T1>();

        /// <summary> Additional scaled grids </summary>
        public List<FGenGraph<T1, T2>> SubGraphs;

        public T1 MinX { get; private set; }
        public T1 MinY { get; private set; }
        public T1 MinZ { get; private set; }
        public T1 MaxX { get; private set; }
        public T1 MaxY { get; private set; }
        public T1 MaxZ { get; private set; }

        public int Width, Height, Depth;
        public int ReferenceScale = 1;
        public float YScale = 1f;

        public FGenGraph(bool reset = false)
        {
            if (reset)
            {
                AllCells = new List<T1>();
                AllApprovedCells = new List<T1>();
                Cells = new FGenGrid<T1>();
            }
        }

        public void Generate(int width, int depth, Vector3Int offset)
        {
            Width = width;
            Height = 1;
            Depth = depth;

            for (int x = offset.x; x < width + offset.x; x++)
                for (int z = offset.z; z < depth + offset.z; z++)
                    AddCell(x, offset.y, z);

        }

        public void Generate(int xWidth, int yHeight, int zDepth, Vector3Int offset)
        {
            Width = xWidth;
            Height = yHeight;
            Depth = zDepth;

            for (int x = offset.x; x < xWidth + offset.x; x++)
                for (int y = offset.y; y <= yHeight + offset.y; y++)
                    for (int z = offset.z; z < zDepth + offset.z; z++)
                        AddCell(x, y, z);
        }

        internal FieldCell GetNearestFrom(FieldCell startCell, FieldCell[] cells)
        {
            float nearest = float.MaxValue;
            FieldCell nr = startCell;

            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == startCell) continue;
                if (cells[i] == null) continue;
                if (cells[i].InTargetGridArea == false) continue;

                float dist = (startCell.Pos - cells[i].Pos).sqrMagnitude;
                if (dist < nearest)
                {
                    nearest = dist;
                    nr = cells[i];
                }
            }

            return nr;
        }


        /// <summary> If cell is diagonal end edge, like ┌ </summary>
        internal bool CellIsDiagonalOut(FieldCell cell)
        {
            Vector3Int myPos = cell.Pos;

            for (int i = 0; i < 4; i++)
                if (CellExists(myPos + QuartDir(i + 2)))
                    if (CellExists(myPos + QuartDir(i - 1)))
                        if (!CellExists(myPos + QuartDir(i)))
                            if (!CellExists(myPos + QuartDir(i + 1)))
                                return true;

            return false;
        }

        /// <summary> If cell is diagonal end edge, like ┌ </summary>
        internal bool CellIsSurrounded(FieldCell cell, bool includeDiagonal = false)
        {
            Vector3Int myPos = cell.Pos;

            if (CellExists(myPos + QuartDir(0)))
                if (CellExists(myPos + QuartDir(1)))
                    if (CellExists(myPos + QuartDir(2)))
                        if (CellExists(myPos + QuartDir(3)))
                        {
                            if (includeDiagonal)
                            {
                                return CellIsSurroundedOnlyDiag(myPos);
                            }
                            else
                                return true;
                        }

            return false;
        }

        internal bool CellIsSurroundedOnlyDiag(Vector3Int pos)
        {
            if (CellExists(pos + QuartDir(0) + QuartDir(1)))
                if (CellExists(pos + QuartDir(1) + QuartDir(2)))
                    if (CellExists(pos + QuartDir(2) + QuartDir(3)))
                        if (CellExists(pos + QuartDir(3) + QuartDir(0)))
                            return true;

            return false;
        }

        /// <summary>
        /// Returns per 90 degrees direction vector
        /// </summary>
        public Vector3Int QuartDir(int dir)
        {
            if (dir == 90) dir = 1;
            if (dir == -90) dir = 3;
            if (dir == 180) dir = 2;
            if (dir == 270) dir = 3;

            if (dir < 0) dir += 4;
            if (dir > 3) dir -= 4;

            switch (dir)
            {
                case 0: return new Vector3Int(1, 0, 0); // 0
                case 1: return new Vector3Int(0, 0, 1); // 90
                case 2: return new Vector3Int(-1, 0, 0); // 180
                case 3: return new Vector3Int(0, 0, -1); // 270
            }

            return Vector3Int.zero;
        }

        public T1 AddCell(Vector3Int position)
        {
            return AddCell(position.x, position.y, position.z);
        }

        public T1 AddCell(T1 oRef)
        {
            //Cells.GetCell(oRef.Pos.x, oRef.Pos.y, oRef.Pos.z, null, true);
            if (oRef.InTargetGridArea == false)
            {
                GetCell(oRef.Pos, true);
            }
            else
            {
                AddCell(oRef.Pos.x, oRef.Pos.y, oRef.Pos.z);
            }

            ReplaceCell(oRef.Pos, oRef);
            return GetCell(oRef.Pos);
        }

        public T1 AddCell(Vector2Int position, int yLevel = 0)
        {
            return AddCell(position.x, yLevel, position.y);
        }

        public T1 AddCell(int x, int y, int z)
        {
            T1 cell = GetCell(x, y, z, false);

            if (FGenerators.CheckIfIsNull((cell)) || cell.InTargetGridArea == false)
            {
                cell = GetCell(x, y, z, true);
                cell.InTargetGridArea = true;
                cell.Scaler = ReferenceScale;
                AllApprovedCells.Add(cell);
                CheckForMinMax(cell);
            }

            return cell;
        }

        public void Clear()
        {
            AllCells = new List<T1>();
            AllApprovedCells = new List<T1>();
            Cells = new FGenGrid<T1>();
        }

        private void CheckForMinMax(T1 cell, bool isRemoving = false)
        {
            if (FGenerators.CheckIfExist_NOTNULL(cell))
            {
                if (isRemoving == false)
                {
                    if (FGenerators.CheckIfIsNull(MinX)) MinX = cell;
                    if (FGenerators.CheckIfIsNull(MinY)) MinY = cell;
                    if (FGenerators.CheckIfIsNull(MinZ)) MinZ = cell;
                    if (FGenerators.CheckIfIsNull(MaxX)) MaxX = cell;
                    if (FGenerators.CheckIfIsNull(MaxY)) MaxY = cell;
                    if (FGenerators.CheckIfIsNull(MaxZ)) MaxZ = cell;

                    if (cell.Pos.x < MinX.Pos.x) MinX = cell;
                    if (cell.Pos.y < MinY.Pos.y) MinY = cell;
                    if (cell.Pos.z < MinZ.Pos.z) MinZ = cell;

                    if (cell.Pos.x > MaxX.Pos.x) MaxX = cell;
                    if (cell.Pos.y > MaxY.Pos.y) MaxY = cell;
                    if (cell.Pos.z > MaxZ.Pos.z) MaxZ = cell;
                }
                else // Updating min/max after removing cell from grid
                {
                    if (cell.Pos.x == MaxX.Pos.x)
                        /*if (cell == MaxX)*/ // Search for current max
                        MaxX = AnalyzeGridForMaxX();

                    if (cell.Pos.x == MinX.Pos.x)
                        /*if (cell == MinX)*/ // Search for current min
                        MinX = AnalyzeGridForMinX();

                    if (cell.Pos.y == MaxY.Pos.y) /*if (cell == MaxY)*/ MaxY = AnalyzeGridForMaxY();
                    if (cell.Pos.y == MinY.Pos.y) /*if (cell == MinY)*/ MaxY = AnalyzeGridForMinY();
                    /*                 */
                    if (cell.Pos.z == MaxZ.Pos.z) /*if (cell == MaxZ)*/ MaxZ = AnalyzeGridForMaxZ();
                    if (cell.Pos.z == MinZ.Pos.z) /*if (cell == MinZ)*/ MinZ = AnalyzeGridForMinZ();
                }
            }
        }


        public T1 AnalyzeGridForMaxX()
        {
            if (AllApprovedCells.Count == 0) return null;
            T1 max = AllApprovedCells[0];
            for (int a = 0; a < AllApprovedCells.Count; a++)
                if (AllApprovedCells[a].Pos.x > max.Pos.x) max = AllApprovedCells[a];
            return max;
        }

        public T1 AnalyzeGridForMinX()
        {
            if (AllApprovedCells.Count == 0) return null;
            T1 min = AllApprovedCells[0];
            for (int a = 0; a < AllApprovedCells.Count; a++)
                if (AllApprovedCells[a].Pos.x < min.Pos.x) min = AllApprovedCells[a];
            return min;
        }

        public T1 AnalyzeGridForMaxY()
        {
            if (AllApprovedCells.Count == 0) return null;
            T1 max = AllApprovedCells[0];
            for (int a = 0; a < AllApprovedCells.Count; a++)
                if (AllApprovedCells[a].Pos.y > max.Pos.y) max = AllApprovedCells[a];
            return max;
        }

        public T1 AnalyzeGridForMinY()
        {
            if (AllApprovedCells.Count == 0) return null;
            T1 min = AllApprovedCells[0];
            for (int a = 0; a < AllApprovedCells.Count; a++)
                if (AllApprovedCells[a].Pos.y < min.Pos.y) min = AllApprovedCells[a];
            return min;
        }

        public T1 AnalyzeGridForMaxZ()
        {
            if (AllApprovedCells.Count == 0) return null;
            T1 max = AllApprovedCells[0];
            for (int a = 0; a < AllApprovedCells.Count; a++)
                if (AllApprovedCells[a].Pos.z > max.Pos.z) max = AllApprovedCells[a];
            return max;
        }

        public T1 AnalyzeGridForMinZ()
        {
            if (AllApprovedCells.Count == 0) return null;
            T1 min = AllApprovedCells[0];
            for (int a = 0; a < AllApprovedCells.Count; a++)
                if (AllApprovedCells[a].Pos.z < min.Pos.z) min = AllApprovedCells[a];
            return min;
        }


        public Vector3Int GetMin()
        {
            if (FGenerators.CheckIfIsNull(MinX)) return Vector3Int.zero;
            return new Vector3Int(MinX.Pos.x, MinY.Pos.y, MinZ.Pos.z);
        }

        public Vector3Int GetMax()
        {
            if (FGenerators.CheckIfIsNull(MaxX)) return Vector3Int.zero;
            return new Vector3Int(MaxX.Pos.x, MaxY.Pos.y, MaxZ.Pos.z);
        }

        public Vector3Int GetMaxSizeInCells()
        {
            Vector3Int size = new Vector3Int();
            Vector3Int min = GetMin();
            Vector3Int max = GetMax();

            size.x = Mathf.Abs(max.x - min.x) + 1;
            size.y = Mathf.Abs(max.y - min.y) + 1;
            size.z = Mathf.Abs(max.z - min.z) + 1;

            return size;
        }

        internal void RestoreGrid(List<T1> gridCellsSave)
        {
            for (int i = 0; i < gridCellsSave.Count; i++)
            {
                GetCell(gridCellsSave[i].Pos, true);
                ReplaceCell(gridCellsSave[i].Pos, gridCellsSave[i]);
                if (gridCellsSave[i].InTargetGridArea) ApproveCell(gridCellsSave[i]);
            }
        }

        public void ReplaceCell(Vector3Int pos, T1 cell)
        {
            Cells.ReplaceCell(pos.x, pos.y, pos.z, ref cell);

            // !!! TODO: Need to be done better in the future -> to not lost refs ins AllCells lists when replacing cells
            // Probably AllCells List<T1> will change to AllCells List<Vector3Int> 
            for (int i = 0; i < AllCells.Count; i++) if (AllCells[i].Pos == cell.Pos) { AllCells[i] = cell; break; }
            for (int i = 0; i < AllApprovedCells.Count; i++) if (AllApprovedCells[i].Pos == cell.Pos) { AllApprovedCells[i] = cell; break; }
        }

        public bool CellExists(Vector3Int pos, bool mustBeContained = true)
        {
            if (mustBeContained)
            {
                var cell = GetCell(pos.x, pos.y, pos.z, false);
                if (FGenerators.CheckIfExist_NOTNULL(cell))
                    if (cell.InTargetGridArea) return true;

                return false;
            }
            else
                return FGenerators.CheckIfExist_NOTNULL(GetCell(pos.x, pos.y, pos.z, false));
        }

        public T1 GetCell(Vector3Int pos, bool generateIfOut = true)
        {
            return GetCell(pos.x, pos.y, pos.z, generateIfOut);
        }

        public bool IsEmpty(int x, int y, int z)
        {
            return FGenerators.IsNull(GetCell(x, y, z, false));
        }

        public bool IsEmpty(Vector3Int pos)
        {
            return FGenerators.IsNull(GetCell(pos, false));
        }

        public T1 GetEmptyCell(Vector3Int pos)
        {
            T1 get = GetCell(pos.x, pos.y, pos.z, false);

            if (FGenerators.CheckIfIsNull(get))
            {
                get = GetCell(pos.x, pos.y, pos.z, true);
                get.InTargetGridArea = false;
            }

            return get;
        }

        public T1 GetCell(Vector2Int pos, bool generateIfOut = true, int y = 0)
        {
            return GetCell(pos.x, y, pos.y, generateIfOut);
        }

        public T1 KickOutCell(Vector3Int pos)
        {
            T1 cell = GetCell(pos);
            if (FGenerators.CheckIfExist_NOTNULL(cell)) cell.InTargetGridArea = false;
            return cell;
        }

        public void MoveCell(T1 cell, Vector3Int newPos)
        {
            T1 newPosCell = null;

            if (Cells.Cells.TryGetValue(newPos, out newPosCell))
            {
                Vector3Int preCellPos = cell.Pos;

                cell.Pos = newPos;
                Cells.Cells[newPos] = cell;

                newPosCell.Pos = preCellPos;
                Cells.Cells[preCellPos] = newPosCell;
            }
            else
            {
                Cells.Cells.Remove(cell.Pos);
                cell.Pos = newPos;
                Cells.Cells.Add(newPos, cell);
            }
        }

        public bool RemoveCell(Vector3Int pos)
        {
            T1 cell = GetCell(pos, false); //!!! zmiana

            if (FGenerators.CheckIfExist_NOTNULL(cell))
            {
                cell.InTargetGridArea = false;
                AllApprovedCells.Remove(cell);
                CheckForMinMax(cell, true);
                return true;
            }

            return false;
        }

        public void RemoveCell(T1 cell)
        {
            if (FGenerators.CheckIfIsNull(cell)) return;
            if (cell.InTargetGridArea == false) return;

            cell.InTargetGridArea = false;
            AllApprovedCells.Remove(cell);
            CheckForMinMax(cell, true);
        }

        public T1 GetCell(int x, int y, int z, bool generateIfOut = true)
        {
            bool wasGenerated = false;

            T1 cell = Cells.GetCell(x, y, z,
                (c, gz) =>
                {
                    c.Pos = new Vector3Int(x, y, z);
                    AllCells.Add(c);
                    wasGenerated = true;
                },
                generateIfOut);

            if (cell != null) return cell;
            if (wasGenerated) return Cells.GetCell(x, y, z, null, false);
            else return null;
        }

        /// <summary> L R U D </summary>
        public T1[] GetNeightbours(T1 cell, bool generateIfOut = true)
        {
            T1[] verts = new T1[4];

            verts[0] = GetCell(cell.Pos.x - 1, cell.Pos.y, cell.Pos.z, generateIfOut);
            verts[1] = GetCell(cell.Pos.x + 1, cell.Pos.y, cell.Pos.z, generateIfOut);
            verts[2] = GetCell(cell.Pos.x, cell.Pos.y, cell.Pos.z + 1, generateIfOut);
            verts[3] = GetCell(cell.Pos.x, cell.Pos.y, cell.Pos.z - 1, generateIfOut);

            return verts;
        }

        public void ApproveCell(T1 cell)
        {
            if (AllApprovedCells.Contains(cell) == false)
            //if (cell.InTargetGridArea != true)
            {
                AllApprovedCells.Add(cell);
                cell.InTargetGridArea = true;
            }
            else
            {
                cell.InTargetGridArea = true;
            }
        }

        /// <summary> from lu, every x row </summary>
        public T1[] GetCustomSquare(T1 cell, int around)
        {
            T1[] cells = new T1[around * around];
            int iter = 0;

            for (int z = 0; z < around; z++)
            {
                for (int x = 0; x < around; x++)
                {
                    cells[iter] = GetCell(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z, false);
                }
            }

            return cells;
        }


        /// <summary> from lu, every x row </summary>
        public List<T1> GetCustomSquare(T1 cell, int around, bool ignoreEmpty, bool generateIfOut = false)
        {
            List<T1> cells = new List<T1>();

            if (ignoreEmpty)
            {
                for (int z = 0; z < around; z++)
                    for (int x = 0; x < around; x++)
                    {
                        var nCell = GetCell(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z, generateIfOut);
                        if (FGenerators.CheckIfIsNull(nCell)) continue;
                        if (nCell.InTargetGridArea == false) continue;
                        cells.Add(nCell);
                    }
            }
            else
                for (int z = 0; z < around; z++)
                    for (int x = 0; x < around; x++)
                        cells.Add(GetCell(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z, generateIfOut));

            return cells;
        }


        public int CountCellsAround(T1 cell, int size)
        {
            int counter = 0;
            for (int x = 0; x < size; x++)
                for (int z = 0; z < size; z++)
                {
                    var c = GetCell(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z, false);
                    if (FGenerators.CheckIfExist_NOTNULL(c)) if (c.InTargetGridArea) counter++;
                }

            return counter;
        }

        public bool AreAnyCellsAround(T1 cell, int size)
        {
            for (int x = 0; x < size; x++)
                for (int z = 0; z < size; z++)
                    if (FGenerators.CheckIfExist_NOTNULL(GetCell(cell.Pos.x, +x, cell.Pos.z + z, false))) return true;

            return false;
        }

        public bool AreAnyCellsLacking(T1 cell, int size)
        {
            for (int x = 0; x < size; x++)
                for (int z = 0; z < size; z++)
                    if (FGenerators.CheckIfIsNull(GetCell(cell.Pos.x, +x, cell.Pos.z + z, false))) return false;

            return true;
        }

        /// <summary>
        /// 3x3 square neightbours including initial cell ([4]th index)
        /// </summary>
        public T1[] Get3x3Square(T1 cell, bool generateIfOut = true)
        {
            T1[] verts = new T1[9];

            verts[0] = GetCell(cell.Pos.x - 1, cell.Pos.y, cell.Pos.z + 1, generateIfOut);
            verts[1] = GetCell(cell.Pos.x, cell.Pos.y, cell.Pos.z + 1, generateIfOut);
            verts[2] = GetCell(cell.Pos.x + 1, cell.Pos.y, cell.Pos.z + 1, generateIfOut);

            verts[3] = GetCell(cell.Pos.x - 1, cell.Pos.y, cell.Pos.z, generateIfOut);
            verts[4] = GetCell(cell.Pos.x, cell.Pos.y, cell.Pos.z, generateIfOut);
            verts[5] = GetCell(cell.Pos.x + 1, cell.Pos.y, cell.Pos.z, generateIfOut);

            verts[6] = GetCell(cell.Pos.x - 1, cell.Pos.y, cell.Pos.z - 1, generateIfOut);
            verts[7] = GetCell(cell.Pos.x, cell.Pos.y, cell.Pos.z - 1, generateIfOut);
            verts[8] = GetCell(cell.Pos.x + 1, cell.Pos.y, cell.Pos.z - 1, generateIfOut);

            return verts;
        }

        /// <summary> U L R D </summary>
        public T1[] GetPLUSSquare(T1 cell, bool generateIfOut = true)
        {
            T1[] verts = new T1[4];

            verts[0] = GetCell(cell.Pos.x, cell.Pos.y, cell.Pos.z + 1, generateIfOut);
            verts[1] = GetCell(cell.Pos.x - 1, cell.Pos.y, cell.Pos.z, generateIfOut);
            verts[2] = GetCell(cell.Pos.x + 1, cell.Pos.y, cell.Pos.z, generateIfOut);
            verts[3] = GetCell(cell.Pos.x, cell.Pos.y, cell.Pos.z - 1, generateIfOut);

            return verts;
        }


        /// <summary> LU RU LD RD </summary>
        public T1[] GetDiagonalCross(T1 cell, bool generateIfOut = true)
        {
            T1[] verts = new T1[4];

            verts[0] = GetCell(cell.Pos.x - 1, cell.Pos.y, cell.Pos.z + 1, generateIfOut);
            verts[1] = GetCell(cell.Pos.x + 1, cell.Pos.y, cell.Pos.z + 1, generateIfOut);
            verts[2] = GetCell(cell.Pos.x - 1, cell.Pos.y, cell.Pos.z - 1, generateIfOut);
            verts[3] = GetCell(cell.Pos.x + 1, cell.Pos.y, cell.Pos.z - 1, generateIfOut);

            return verts;
        }

        /// <summary> Getting squares around in cells distance </summary>
        public List<T1> GetDistanceSquare2DList(T1 from, int indexDistance, float cellSize = 0f, float worldDistance = 0f)
        {
            List<T1> cells = new List<T1>();
            Vector3 refPos = from.Pos;

            for (int x = -indexDistance; x <= indexDistance; x++)
            {
                for (int z = -indexDistance; z <= indexDistance; z++)
                {
                    Vector3 tgtPos = new Vector3(refPos.x + x, refPos.y, refPos.z + z);

                    T1 tgtCell = GetCell(Mathf.RoundToInt(tgtPos.x), Mathf.RoundToInt(tgtPos.y), Mathf.RoundToInt(tgtPos.z), false);

                    if (tgtCell != null)
                    {
                        if (worldDistance > 0f)
                        {
                            float cellsWorldDistance = Vector3.Distance(tgtPos * cellSize, refPos * cellSize);
                            if (cellsWorldDistance <= worldDistance) cells.Add(tgtCell);
                        }
                        else cells.Add(tgtCell);
                    }
                }
            }

            return cells;
        }


        public void GetDistanceSquare2DList(List<T1> to, T1 from, Vector3Int indexRanges, float cellSize = 0f, float worldDistance = 0f)
        {
            Vector3 refPos = from.Pos;

            for (int x = -indexRanges.x; x <= indexRanges.x; x++)
            {
                for (int y = -indexRanges.y; y <= indexRanges.y; y++)
                {
                    for (int z = -indexRanges.z; z <= indexRanges.z; z++)
                    {
                        Vector3 tgtPos = new Vector3(refPos.x + x, refPos.y + y, refPos.z + z);

                        T1 tgtCell = GetCell(Mathf.RoundToInt(tgtPos.x), Mathf.RoundToInt(tgtPos.y), Mathf.RoundToInt(tgtPos.z), false);

                        if (FGenerators.CheckIfExist_NOTNULL(tgtCell))
                        {
                            if (worldDistance > 0f)
                            {
                                if (cellSize > 0f)
                                {
                                    float cellsWorldDistance = Vector3.Distance(tgtPos * cellSize, refPos * cellSize);
                                    if (cellsWorldDistance <= worldDistance) to.Add(tgtCell);
                                }
                                else
                                {
                                    to.Add(tgtCell);
                                }
                            }
                            else
                            {
                                to.Add(tgtCell);
                            }
                        }

                    }
                }
            }
        }


        internal FGenGraph<T1, T2> GetCorrespondingSubGraph(int scale)
        {
            if (SubGraphs != null)
                for (int i = 0; i < SubGraphs.Count; i++)
                    if (SubGraphs[i].ReferenceScale == scale) return SubGraphs[i];

            return null;
        }

        internal T1 GetCorrespondingSubGraphCell(FieldCell cell, FGenGraph<T1, T2> rightGraph)
        {
            int scaleDiff = ReferenceScale - rightGraph.ReferenceScale;
            if (scaleDiff < 0) { scaleDiff = -scaleDiff + 1; } else scaleDiff += 1;

            Vector3Int transposedPosition = new Vector3Int();

            transposedPosition.x = Mathf.FloorToInt(cell.Pos.x / scaleDiff);
            transposedPosition.y = Mathf.FloorToInt(cell.Pos.y / scaleDiff);
            transposedPosition.z = Mathf.FloorToInt(cell.Pos.z / scaleDiff);

            T1 newCell = rightGraph.GetCell(transposedPosition, false);

            return newCell;
        }

        internal Vector3 GetCenterUnrounded()
        {
            float xx = MaxX.Pos.x - MinX.Pos.x;// Mathf.Lerp(MinX.Pos.x, MaxX.Pos.x, 0.5f);
            float yy = MaxY.Pos.y - MinY.Pos.y;//Mathf.Lerp(MinY.Pos.y, MaxY.Pos.y, 0.5f);
            float zz = MaxZ.Pos.z - MinZ.Pos.z;//Mathf.Lerp(MinZ.Pos.z, MaxZ.Pos.z, 0.5f);

            return new Vector3(xx, yy, zz);
        }

        internal Vector3Int GetCenter()
        {
            int xx = Mathf.FloorToInt(Mathf.Lerp(MinX.Pos.x, MaxX.Pos.x, 0.5f));
            int yy = Mathf.FloorToInt(Mathf.Lerp(MinY.Pos.y, MaxY.Pos.y, 0.5f));
            int zz = Mathf.FloorToInt(Mathf.Lerp(MinZ.Pos.z, MaxZ.Pos.z, 0.5f));
            //int y = Height / 2;
            return new Vector3Int(xx, yy, zz);
        }

        public FGenGraph<T1, T2> Copy(bool newRefs = true)
        {
            FGenGraph<T1, T2> copy = new FGenGraph<T1, T2>(true);
            copy.ReferenceScale = ReferenceScale;
            copy.MaxX = MaxX; copy.MinX = MinX;
            copy.MaxY = MaxY; copy.MinY = MinY;
            copy.MaxZ = MaxZ; copy.MinZ = MinZ;

            if (newRefs)
            {
                for (int i = 0; i < AllApprovedCells.Count; i++)
                {
                    copy.AddCell(AllApprovedCells[i].Pos);
                }
            }
            else
            {
                for (int i = 0; i < AllApprovedCells.Count; i++)
                {
                    copy.AddCell(AllApprovedCells[i]);
                }
            }

            return copy;
        }

        public FGenGraph<T1, T2> CopyEmpty()
        {
            FGenGraph<T1, T2> copy = new FGenGraph<T1, T2>(true);
            copy.ReferenceScale = ReferenceScale;
            return copy;
        }

        private Vector3Int DivideAndCeilMax(Vector3Int val, float div)
        {
            if (val.x < 0) val.x = -Mathf.FloorToInt((float)-(val.x + 1) / div); else val.x = Mathf.CeilToInt((float)(val.x + 1) / div);
            if (val.y < 0) val.y = -Mathf.FloorToInt((float)-(val.y + 1) / div); else val.y = Mathf.CeilToInt((float)(val.y + 1) / div);
            if (val.z < 0) val.z = -Mathf.FloorToInt((float)-(val.z + 1) / div); else val.z = Mathf.CeilToInt((float)(val.z + 1) / div);
            return val;
        }

        private Vector3Int DivideAndCeilMin(Vector3Int val, float div)
        {
            if (val.x < 0) val.x = -Mathf.CeilToInt((float)-(val.x - 1) / div); else val.x = Mathf.FloorToInt((float)(val.x + 1) / div);
            if (val.y < 0) val.y = -Mathf.CeilToInt((float)-(val.y - 1) / div); else val.y = Mathf.FloorToInt((float)(val.y + 1) / div);
            if (val.z < 0) val.z = -Mathf.CeilToInt((float)-(val.z - 1) / div); else val.z = Mathf.FloorToInt((float)(val.z + 1) / div);
            return val;
        }

        public FGenGraph<T1, T2> GenerateScaledGraph(int scale, bool inheritCells = true, bool oneCellIsEnough = true)
        {
            FGenGraph<T1, T2> newGraph = new FGenGraph<T1, T2>();
            newGraph.ReferenceScale = scale;
            if (FGenerators.CheckIfIsNull(MinX) || FGenerators.CheckIfIsNull(MaxX)) return null;

            Vector3Int maxXC, minXC, maxZC, minZC;

            if (oneCellIsEnough)
            {
                maxXC = DivideAndCeilMax(MaxX.Pos, scale) * scale;
                minXC = DivideAndCeilMin(MinX.Pos, scale) * scale;
                maxZC = DivideAndCeilMax(MaxZ.Pos, scale) * scale;
                minZC = DivideAndCeilMin(MinZ.Pos, scale) * scale;
            }
            else
            {
                maxXC = MaxX.Pos;
                minXC = MinX.Pos;
                maxZC = MaxZ.Pos;
                minZC = MinZ.Pos;
            }

            for (int x = minXC.x; x <= maxXC.x; x += 1)
            {
                if (x % scale != 0) continue;
                for (int y = MinY.Pos.y; y <= MaxY.Pos.y; y += 1)
                    for (int z = minZC.z; z <= maxZC.z; z += 1)
                    {
                        if (z % scale != 0) continue;

                        T1 cell = GetCell(x, y, z, oneCellIsEnough);
                        if (FGenerators.CheckIfIsNull(cell)) continue;

                        if (CountCellsAround(cell, scale) > 0)
                        {
                            T1 nCell = newGraph.AddCell((x / scale), y, (z / scale));
                            //T1 nCell = newGraph.AddCell(Mathf.FloorToInt( x / scale), y, Mathf.FloorToInt(z / scale));

                            if (inheritCells)
                            {
                                int ff = 0;
                                var gCells = GetCustomSquare(cell, scale, true);

                                foreach (var sCell in gCells)
                                {
                                    if (FGenerators.CheckIfIsNull(sCell)) continue;
                                    sCell.AddScaleParentCell(nCell);
                                    nCell.AddScaleChildCell(sCell);
                                    ff++;
                                }
                            }

                        }
                    }
            }

            RecalculateGridDimensions();
            newGraph.Width = Mathf.FloorToInt(Width / scale);
            newGraph.Height = Mathf.FloorToInt(Height / scale);
            newGraph.Depth = Mathf.FloorToInt(Depth / scale);

            return newGraph;
        }

        /// <summary>
        /// Recalculating width,height,depth variables
        /// </summary>
        public void RecalculateGridDimensions()
        {
            Width = Mathf.Abs((MaxX.Pos.x) - MinX.Pos.x) + 1;
            Height = Mathf.Abs((MaxY.Pos.y) - MinY.Pos.y) + 1;
            Depth = Mathf.Abs((MaxZ.Pos.z) - MinZ.Pos.z) + 1;
        }

        internal Vector3 GetWorldCenter(Vector3 cellSize, bool withOffset = false)
        {
            cellSize *= ReferenceScale;
            float x = Width / 2f;
            float y = Height / 2f;
            float z = Depth / 2f;

            if (withOffset)
                return Vector3.Scale(new Vector3(x, y, z), cellSize) + GetCenterOffset(cellSize);
            else
                return Vector3.Scale(new Vector3(x, y, z), cellSize);
        }

        internal Vector3 GetCenterOffset(Vector3 cellSize)
        {
            cellSize *= ReferenceScale;
            float x = 0f;
            float y = 0f;
            float z = 0f;

            Vector3 center = GetCenterUnrounded();

            if (Mathf.RoundToInt(center.x * 2) % 2 == 0)
                x += 1f / 2f;

            if (Mathf.RoundToInt(center.z * 2) % 2 == 0)
                z += 1f / 2f;

            return Vector3.Scale(new Vector3(x, y, z), cellSize);
        }


    }
}
