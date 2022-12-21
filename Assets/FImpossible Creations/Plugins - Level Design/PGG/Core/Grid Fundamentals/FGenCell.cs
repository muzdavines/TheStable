using System.Collections.Generic;
using UnityEngine;
// NO EXPORT

namespace FIMSpace.Generating
{
    public class FGenCell
    {
        /// <summary>
        /// !!! DON'T CHANGE IT MANUALLY IF CELL IS ALREADY ADDED TO SOME GRID, USE Grid.MoveCell  INSTEAD !!!
        /// Grid space position (it's not world position)
        /// </summary>
        public Vector3Int Pos;

        public bool InTargetGridArea = false;
        public int Scaler = 1;

        [System.NonSerialized] public Vector3 HelperVector = Vector3.zero;
        public Vector3Int HelperDirection { get { return HelperVector.V3toV3Int(); } }

        #region Helper dirs

        private readonly Vector3Int[] _helperDirsNull = new Vector3Int[0];
        private readonly Vector3Int[] _helperDirs1 = new Vector3Int[1];
        private readonly Vector3Int[] _helperDirs2 = new Vector3Int[2];
        private readonly Vector3Int[] _helperDirs3 = new Vector3Int[3];
        private readonly Vector3Int[] _helperDirs4 = new Vector3Int[4] { new Vector3Int(1, 0, 0), new Vector3Int(0, 0, 1), new Vector3Int(-1, 0, 0), new Vector3Int(0, 0, -1) };
        public Vector3Int[] GetAvailableOutDirs()
        {
            if (HelperDirection == Vector3Int.zero) return _helperDirsNull;

            if (HelperDirection.x != 0) // Not Zero X
            {
                if (HelperDirection.x == 2) // X Two Dir 
                {
                    if (HelperDirection.z == 2) // X and Z two dir
                    {
                        return _helperDirs4;
                    }
                    else // X two dir, Z single dir
                    {
                        if (HelperDirection.z != 0)
                        {
                            _helperDirs3[0] = new Vector3Int(1, 0, 0);
                            _helperDirs3[1] = new Vector3Int(-1, 0, 0);
                            _helperDirs3[2] = new Vector3Int(0, 0, HelperDirection.z);
                            return _helperDirs3;
                        }
                        else // Just X two dir
                        {
                            _helperDirs2[0] = new Vector3Int(1, 0, 0);
                            _helperDirs2[1] = new Vector3Int(-1, 0, 0);
                            return _helperDirs2;
                        }
                    }
                }
                else // X single dir
                {
                    if (HelperDirection.z == 2) // single dir X and Z two dir
                    {
                        _helperDirs3[0] = new Vector3Int(HelperDirection.x, 0, 0);
                        _helperDirs3[1] = new Vector3Int(0, 0, 1);
                        _helperDirs3[2] = new Vector3Int(0, 0, -1);
                        return _helperDirs3;
                    }
                    else // X single dir , Z single dir
                    {
                        if (HelperDirection.z != 0)
                        {
                            _helperDirs2[0] = new Vector3Int(HelperDirection.x, 0, 0);
                            _helperDirs2[1] = new Vector3Int(0, 0, HelperDirection.z);
                            return _helperDirs2;
                        }
                        else // Just X single dir
                        {
                            _helperDirs1[0] = HelperDirection;
                            return _helperDirs1;
                        }
                    }
                }
            }
            else if (HelperDirection.z != 0) // X = 0
            {
                if (HelperDirection.z == 2) // Z Two Dir 
                {
                    _helperDirs2[0] = new Vector3Int(0, 0, 1);
                    _helperDirs2[1] = new Vector3Int(0, 0, -1);
                    return _helperDirs2;
                }
                else // Z single dir
                {
                    _helperDirs1[0] = HelperDirection;
                    return _helperDirs1;
                }
            }

            return _helperDirsNull;
        }

        #endregion

        [System.NonSerialized] private Vector3Int _helperVector2 = Vector3Int.zero;
        public int LastSearchDistance { get { return _helperVector2.x; } set { _helperVector2.x = value; } }
        public int LastSearchTeleport { get { return _helperVector2.y; } set { _helperVector2.y = value; } }


        public Vector3 WorldPos(float cellSize = 2f, float ySize = 1f) { Vector3 pos = (Vector3)Pos * cellSize * Scaler; pos.y *= ySize; return pos; }
        public Vector3 WorldPos(float xSize, float ySize, float zSize) { return new Vector3(Pos.x * xSize * Scaler, Pos.y * ySize * Scaler, Pos.z * zSize * Scaler); }
        public Vector3 WorldPos(Vector3 cellSize) { return new Vector3(Pos.x * cellSize.x * Scaler, Pos.y * cellSize.y * Scaler, Pos.z * cellSize.z * Scaler); }
        public Vector3 WorldPos(FieldSetup preset) { Vector3 cellSize = preset.GetCellUnitSize(); return new Vector3(Pos.x * cellSize.x * Scaler, Pos.y * cellSize.y * Scaler, Pos.z * cellSize.z * Scaler); }


        #region Cells Hierarchy handling

        private List<FGenCell> biggerCells;
        private List<FGenCell> subCells;

        public bool HaveScaleParentCells()
        {
            if (biggerCells == null) return false;
            if (biggerCells.Count == 0) return false;
            return true;
        }

        public List<FGenCell> GetScaleParentCells()
        {
            return biggerCells;
        }

        public void AddScaleParentCell(FGenCell cellParent)
        {
            if (biggerCells == null) biggerCells = new List<FGenCell>();
            if (biggerCells.Contains(cellParent) == false) biggerCells.Add(cellParent);
        }

        public bool HaveScaleChildCells()
        {
            if (subCells == null) return false;
            if (subCells.Count == 0) return false;
            return true;
        }

        public List<FGenCell> GetScaleChildCells()
        {
            return subCells;
        }

        public void AddScaleChildCell(FGenCell childCell)
        {
            if (subCells == null) subCells = new List<FGenCell>();
            if (subCells.Contains(childCell) == false) subCells.Add(childCell);
        }

        public void ResetCellsHierarchy()
        {
            if (biggerCells != null) biggerCells.Clear();
            biggerCells = null;
            if (subCells != null) subCells.Clear();
            subCells = null;
        }

        #endregion

    }
}
