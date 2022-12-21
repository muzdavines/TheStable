using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Checker
{
    public partial class CheckerField3D
    {
        public FieldCell CopyCellRefAndAdd( FieldCell otherCell)
        {
            return Grid.AddCell(otherCell);
        }
        public FieldCell AddLocal(Vector3 position)
        {
            return AddLocal(position.V3toV3Int());
        }
        public FieldCell AddLocal(Vector3Int position)
        {
            FieldCell added = Grid.AddCell(position);
            return added;
        }
        public FieldCell AddWorld(Vector3 worldPos)
        {
            return AddWorld(worldPos.V3toV3Int());
        }
        public FieldCell AddWorld(Vector3Int worldPos)
        {
            Vector3Int pos = Matrix.inverse.MultiplyPoint3x4(worldPos).V3toV3Int(); // RootPosition.V3toV3Int();
            return AddLocal(pos);
            //return AddLocal(worldPos - pos);
        }
        public void AddLocal(List<Vector3> positions)
        {
            for (int i = 0; i < positions.Count; i++) AddLocal(positions[i]);
        }
        public void AddLocal(List<Vector3Int> positions)
        {
            for (int i = 0; i < positions.Count; i++) AddLocal(positions[i]);
        }
        public void AddWorld(List<Vector3> worldPos)
        {
            Vector3Int pos = RootPosition.V3toV3Int();
            for (int i = 0; i < worldPos.Count; i++)
            {
                AddWorld(worldPos[i] - pos);
            }
        }
        public void AddWorld(List<Vector3Int> worldPos)
        {
            Vector3Int pos = RootPosition.V3toV3Int();
            for (int i = 0; i < worldPos.Count; i++)
            {
                AddWorld(worldPos[i] - pos);
            }
        }

        public void RemoveLocal(Vector3 pos)
        {
            Grid.RemoveCell(pos.V3toV3Int());
        }

        public void RemoveLocal(int index)
        {
            Grid.RemoveCell(AllCells[index]);
        }

        /// <summary>
        /// Fast check, no rotation support
        /// </summary>
        public bool ContainsWorld(Vector3 pos)
        {
            //Vector3 locPos = Matrix.inverse.MultiplyPoint3x4(pos);
            //Vector3Int origPos = locPos.V3toV3Int();
            //var cell = Grid.GetCell(origPos, false);
            //var cell = Grid.GetCell(  WorldToLocal(pos).V3toV3Int(), false);
            var cell = Grid.GetCell((pos - RootPosition).V3toV3Int(), false);

            if (FGenerators.CheckIfIsNull(cell)) return false;

            if (cell.InTargetGridArea)
            {
                //UnityEngine.Debug.DrawRay(GetWorldPos(cell), Vector3.up, Color.green, 1.01f);
                return true;
            }

            return false;
        }

        public bool ContainsWorld(Vector3 pos, Matrix4x4 myInvMatrix, bool checkRounded = false)
        {
            Vector3 locPos = myInvMatrix.MultiplyPoint3x4(pos);
            Vector3Int origPos = locPos.V3toV3Int();

            var cell = Grid.GetCell(origPos, false );

            if (FGenerators.CheckIfIsNull(cell))
            {
                if (checkRounded)
                {
                    Vector3Int nCheck = new Vector3Int();
                    if (locPos.x < origPos.x) nCheck.x = Mathf.FloorToInt(locPos.x); else nCheck.x = Mathf.CeilToInt(locPos.x);
                    if (locPos.y < origPos.y) nCheck.y = Mathf.FloorToInt(locPos.y); else nCheck.y = Mathf.CeilToInt(locPos.y);
                    if (locPos.z < origPos.z) nCheck.z = Mathf.FloorToInt(locPos.z); else nCheck.z = Mathf.CeilToInt(locPos.z);

                    #region Debugging

                    if (DebugHelper)
                    {

                        //DebugLogDrawCellInWorldSpace(new Vector3Int(nCheck.x, nCheck.y, origPos.z), Color.red * 0.8f);
                        //DebugLogDrawCellInWorldSpace(new Vector3Int(nCheck.x, origPos.y, nCheck.z), Color.green * 0.8f);
                        //DebugLogDrawCellInWorldSpace(new Vector3Int(origPos.x, nCheck.y, nCheck.z), Color.blue * 0.8f);
                        //DebugLogDrawCellInWorldSpace(new Vector3Int(origPos.x, origPos.y, origPos.z), Color.magenta * 0.8f);
                        //DebugLogDrawCellInWorldSpace(new Vector3Int(nCheck.x, nCheck.y, nCheck.z), Color.cyan * 0.9f);
                    }

                    #endregion

                    cell = Grid.GetCell(new Vector3Int(nCheck.x, nCheck.y, origPos.z), false);
                    if (FGenerators.CheckIfExist_NOTNULL(cell)) if (cell.InTargetGridArea) return true;

                    cell = Grid.GetCell(new Vector3Int(nCheck.x, origPos.y, nCheck.z), false);
                    if (FGenerators.CheckIfExist_NOTNULL(cell)) if (cell.InTargetGridArea) return true;

                    cell = Grid.GetCell(new Vector3Int(origPos.x, nCheck.y, nCheck.z), false);
                    if (FGenerators.CheckIfExist_NOTNULL(cell)) if (cell.InTargetGridArea) return true;
                }

                return false;
            }

            if (cell.InTargetGridArea) return true;
            return false;
        }

        public bool ContainsLocal(Vector3 pos)
        {
            return ContainsLocal(pos.V3toV3Int());
        }

        public bool ContainsLocal(Vector3Int pos)
        {
            var cell = Grid.GetCell((pos), false);
            if (FGenerators.CheckIfIsNull(cell)) return false;
            if (cell.InTargetGridArea) return true;
            return false;
        }

        public void RemoveWorld(Vector3 pos)
        {
            RemoveWorld(pos, Matrix.inverse);
            //Grid.RemoveCell((RootPosition - pos).V3toV3Int() );
        }

        public void RemoveWorld(Vector3 pos, Matrix4x4 invMx)
        {
            Grid.RemoveCell(invMx.MultiplyPoint3x4(pos).V3toV3Int());
            //Grid.RemoveCell((RootPosition - pos).V3toV3Int() );
        }

        public void ClearAllCells()
        {
            Grid.Clear();
        }

        public void Join(CheckerField3D other)
        {
            Matrix4x4 mx = Matrix.inverse;

            for (int i = 0; i < other.ChildPositionsCount; i++)
            {
                AddLocal(mx.MultiplyPoint3x4(other.GetWorldPos(i)));
                //AddWorld(other.GetWorldPos(i));
            }
        }

        /// <summary> Auto generated bounds when changing squares for much faster collision checking etc. </summary>
        [HideInInspector] public List<Bounds> Bounding = new List<Bounds>();
        public void RecalculateMultiBounds()
        {
            Bounding.Clear();
            if (UseBounds == false) return;

            var yLevels = CollectYLevels();

            for (int y = 0; y < yLevels.Count; y++)
            {
                RecalculateMultiBounds(yLevels[y]);
            }
        }

        public Bounds GetBasicBoundsLocal(bool set = false)
        {
            Bounds b = new Bounds();
            b.min = Grid.GetMin() - new Vector3(0.5f, 0, 0.5f);
            b.max = Grid.GetMax() + new Vector3(0.5f, 0, 0.5f);
            //b.size *= Scale;

            //b.center += RootPosition;

            if (set) { Bounding.Clear(); Bounding.Add(b); }

            return b;
        }

        private List<int> CollectYLevels()
        {
            List<int> yLevels = new List<int>();

            for (int i = 0; i < Grid.AllApprovedCells.Count; i++)
            {
                int y = Grid.AllApprovedCells[i].Pos.y; // Da sie szybciej! grid[0][y][0]
                if (yLevels.Contains(y) == false) yLevels.Add(y);
            }

            return yLevels;
        }

        public void RecalculateMultiBounds(int yLevel)
        {
            FGenGraph<FieldCell, FGenPoint> graphCopy = new FGenGraph<FieldCell, FGenPoint>();

            for (int i = 0; i < Grid.AllApprovedCells.Count; i++)
            {
                if (Grid.AllApprovedCells[i].Pos.y == yLevel) graphCopy.AddCell(Grid.AllApprovedCells[i].Pos);
            }

            if (graphCopy.AllApprovedCells.Count == 0) return;

            Vector3 s = new Vector3(1f, 1f, 1f);
            for (int i = 0; i <= 1000; i++)
            {
                if (graphCopy.AllApprovedCells.Count == 0) break;

                var startCell = graphCopy.AllApprovedCells[0];
                Bounds iBounds = new Bounds(startCell.Pos.V3IntToV3(), s);

                // Nearest margins cells variables
                FieldCell negX, negY, posX, posY;
                negX = new FieldCell(); negY = new FieldCell(); posX = new FieldCell(); posY = new FieldCell();
                negX.OverrideYPos(yLevel); negY.OverrideYPos(yLevel); posX.OverrideYPos(yLevel); posY.OverrideYPos(yLevel);

                CheckGraphForNearestMargins(graphCopy, Grid.AllApprovedCells.Count, startCell, ref posX, ref negX, ref posY, ref negY);
                graphCopy.RemoveCell(startCell.Pos);

                if ((negX != null || negY != null) && (posX != null || posY != null))
                {
                    for (int x = negX.Pos.x; x <= posX.Pos.x; x++)
                        for (int y = negY.Pos.z; y <= posY.Pos.z; y++)
                        {
                            if (FGenerators.CheckIfIsNull(Grid.GetCell(x, yLevel, y, false))) continue;
                            iBounds.Encapsulate(new Bounds(new Vector3(x, yLevel, y), s));
                            graphCopy.RemoveCell(new Vector3Int(x, yLevel, y));
                        }

                    Bounding.Add(iBounds);
                }

                if (i == 1000) UnityEngine.Debug.Log("Safety end (1000 iterations, bounds created: " + Bounding.Count);
            }
        }

    }
}