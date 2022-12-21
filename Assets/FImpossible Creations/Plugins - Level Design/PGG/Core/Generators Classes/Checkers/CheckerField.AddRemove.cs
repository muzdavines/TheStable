using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Checker
{
    public partial class CheckerField
    {

        public void Add(Vector2Int pos)
        {
            if (ContainsWorldPos(pos) == false) AddLocalPos(pos - Position); // Conversion from to world to local child pos
        }

        public void AddLocal(Vector2Int pos)
        {
            if (FGenerators.CheckIfIsNull(ChildPos.GetCell(pos, false, true)))
            {
                AddLocalPos(pos);
            }
        }


        public void Remove(Vector2Int pos)
        {
            if (ContainsWorldPos(pos)) RemoveLocalPos(pos - Position);
        }

        public void RemoveLocal(Vector2Int pos)
        {
            ChildPos.Remove(pos);
        }

        public void RemoveLocal(CheckerPos pos)
        {
            ChildPos.RemoveCell(pos);
        }

        public void RemoveLocal(int x, int y)
        {
            ChildPos.RemoveCell(x, y);
        }

        internal Vector2Int WorldPos(int i)
        {
            if (ChildPos.AllApprovedCells.Count == 0) return Position;
            return ChildPos.AllApprovedCells[i].ToV2() + Position;
        }

        internal Vector2Int GridPos(int i)
        {
            if (ChildPos.AllApprovedCells.Count == 0) return Position + Vector2Int.one;
            return ChildPos.AllApprovedCells[i].ToV2() + Position + Vector2Int.one;
        }

        public bool ContainsWorldPos(Vector2Int worldPos)
        {
            return FGenerators.CheckIfExist_NOTNULL(GetWorldPos(worldPos));
        }

        public CheckerPos GetWorldPos(Vector2Int worldPos)
        {
            return ChildPos.GetCell(worldPos - Position, false, true);
        }

        internal void Join(CheckerField other)
        {
            for (int i = 0; i < other.ChildPos.AllApprovedCells.Count; i++)
            {
                ChildPos.AddCell(other.ChildPos.AllApprovedCells[i]);
                //if (ContainsWorldPos(other.WorldPos(i)) == false) Add(other.WorldPos(i));
            }
        }

        internal int CountSize()
        {
            return ChildPos.AllApprovedCells.Count;
        }


        internal void InjectToGrid(FGenGraph<FieldCell, FGenPoint> mainCorridorsGrid)
        {
            for (int i = 0; i < ChildPos.AllApprovedCells.Count; i++)
                mainCorridorsGrid.AddCell(GridPos(i));
        }

        internal void AddPositions(List<Vector2Int> nRect, bool local = false)
        {
            if (local)
                for (int i = 0; i < nRect.Count; i++) AddLocal(nRect[i]);
            else
                for (int i = 0; i < nRect.Count; i++) Add(nRect[i]);
        }

    }
}