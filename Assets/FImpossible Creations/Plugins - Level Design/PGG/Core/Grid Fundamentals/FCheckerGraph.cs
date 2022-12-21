using FIMSpace.Generating.Checker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class FCheckerGraph<T> where T : CheckerPos, new()
    {
        public List<T> AllCells = new List<T>();
        public List<T> AllApprovedCells = new List<T>();
        public FGenGrid<T> Cells = new FGenGrid<T>();

        public int Count { get { return AllApprovedCells.Count; } }

        public FCheckerGraph(bool reset = false)
        {
            if (reset)
            {
                AllCells = new List<T>();
                AllApprovedCells = new List<T>();
                Cells = new FGenGrid<T>();
            }
        }

        public T Add(Vector2Int position)
        {
            return AddCell(position);
        }

        public T AddCell(Vector2Int position)
        {
            return AddCell(position.x, position.y);
        }

        public T AddCell(CheckerPos position)
        {
            return AddCell(position.x, position.y);
        }

        public bool Contains(Vector2Int position)
        {
            return FGenerators.CheckIfExist_NOTNULL(GetCell(position, false, true) );
        }

        public T AddCell(int x, int y)
        {
            T cell = GetCell(x, y, false);

            if (FGenerators.CheckIfIsNull(cell) || cell.approved == false )
            {
                cell = GetCell(x, y, true);
                cell.x = x; cell.y = y;
                cell.approved = true;
                AllApprovedCells.Add(cell);
                CheckForMinMax(cell);
            }

            return cell;
        }


        public T GetCell(Vector2Int pos, bool generateIfOut = true, bool nullIfNotApproved = false)
        {
            return GetCell(pos.x, pos.y, generateIfOut, nullIfNotApproved);
        }

        public bool Remove(Vector2Int pos)
        {
            T cell = GetCell(pos, false);

            if (FGenerators.CheckIfExist_NOTNULL(cell) /*&& cell.approved*/)
            {
                cell.approved = false;
                AllApprovedCells.Remove(cell);
                AllCells.Remove(cell);
                return true;
            }

            return false;
        }
        public bool RemoveCell(int x, int y)
        {
            return Remove(new Vector2Int(x, y));
        }

        public void RemoveCell(T cell)
        {
            if (FGenerators.CheckIfIsNull(cell )) return;
            cell.approved = false;
            AllApprovedCells.Remove(cell);
            AllCells.Remove(cell);
        }

        public T GetCell(int x, int y, bool generateIfOut = true, bool nullIfNotApproved = false)
        {
            bool wasGenerated = false;

            T cell = Cells.GetCell(x, 0, y,
                (c, gy) =>
                {
                    c.x = x; c.y = y;
                    AllCells.Add(c);
                    wasGenerated = true;
                },
                generateIfOut);

            if (FGenerators.CheckIfExist_NOTNULL(cell ))
            {
                if (nullIfNotApproved) if (cell.approved == false) return null;
                return cell;
            }

            if (wasGenerated)
            {
                cell = Cells.GetCell(x, y, null, false);
                if (nullIfNotApproved) if (cell.approved == false) return null;
                return cell;
            }
            else
            {
                return null;
            }
        }


        public T MinX { get; private set; }
        public T MinY { get; private set; }
        public T MaxX { get; private set; }
        public T MaxY { get; private set; }

        
        private void CheckForMinMax(T cell)
        {
            
            if (FGenerators.CheckIfExist_NOTNULL(cell))
            {
                if (FGenerators.CheckIfIsNull(MinX )) MinX = cell;
                if (FGenerators.CheckIfIsNull(MinY )) MinY = cell;
                if (FGenerators.CheckIfIsNull(MaxX )) MaxX = cell;
                if (FGenerators.CheckIfIsNull(MaxY )) MaxY = cell;

                if (cell.x < MinX.x) MinX = cell;
                if (cell.y < MinY.y) MinY = cell;

                if (cell.x > MaxX.x) MaxX = cell;
                if (cell.y > MaxY.y) MaxY = cell;
            }
        }

        public Vector2Int GetMin()
        {
            return new Vector2Int(MinX.x, MinY.y);
        }

        public Vector2Int GetMax()
        {
            return new Vector2Int(MaxX.x, MaxY.y);
        }

        public FCheckerGraph<T> Copy()
        {
            FCheckerGraph<T> copy = new FCheckerGraph<T>(true);
            copy.MaxX = MaxX; copy.MinX = MinX;
            copy.MaxY = MaxY; copy.MinY = MinY;

            for (int i = 0; i < AllApprovedCells.Count; i++)
            {
                copy.AddCell(AllApprovedCells[i]);
            }

            return copy;
        }

        internal void Clear()
        {
            AllApprovedCells.Clear();
            AllCells.Clear();
            Cells.Clear();
        }
    }
}
