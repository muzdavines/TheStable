using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Checker
{
    public partial class CheckerField3D
    {
        public FieldCell GetCell(int index)
        {
            if (index >= AllCells.Count) return null;
            return AllCells[index];
        }

        public FieldCell GetCell(Vector3Int localPos, bool generateIfOut = false)
        {
            FieldCell getted =  Grid.GetCell(localPos, generateIfOut);
            //if (FGenerators.CheckIfExist_NOTNULL(getted)) getted.ParentChecker = this;
            return getted;
        }

        Vector3Int WorldToGridPos(Vector3 world, Matrix4x4? invertMatrix = null)
        {
            if (invertMatrix == null) invertMatrix = Matrix.inverse;
            return (invertMatrix.Value.MultiplyPoint3x4(world).V3toV3Int());
        }

        public FieldCell GetCellInWorldPos(Vector3 worldPos, Matrix4x4? invertMatrix = null)
        {
            return Grid.GetCell( WorldToGridPos(worldPos, invertMatrix), false );
        }

        public FieldCell GetCellInWorldPos(Vector3 worldPos, bool checkRounded, Matrix4x4? invertMatrix = null)
        {
            Vector3 locPos;

            if (invertMatrix != null)
                locPos = invertMatrix.Value.MultiplyPoint3x4(worldPos);
            else
                locPos = Matrix.inverse.MultiplyPoint3x4(worldPos);

            Vector3Int origPos = locPos.V3toV3Int();
            var cell = Grid.GetCell(origPos, false);

            if (FGenerators.CheckIfIsNull(cell))
            {
                if (checkRounded)
                {
                    Vector3Int nCheck = new Vector3Int();
                    if (locPos.x < origPos.x) nCheck.x = Mathf.FloorToInt(locPos.x); else nCheck.x = Mathf.CeilToInt(locPos.x);
                    if (locPos.y < origPos.y) nCheck.y = Mathf.FloorToInt(locPos.y); else nCheck.y = Mathf.CeilToInt(locPos.y);
                    if (locPos.z < origPos.z) nCheck.z = Mathf.FloorToInt(locPos.z); else nCheck.z = Mathf.CeilToInt(locPos.z);

                    cell = Grid.GetCell(new Vector3Int(nCheck.x, nCheck.y, origPos.z), false);
                    if (FGenerators.CheckIfExist_NOTNULL(cell)) if (cell.InTargetGridArea) return cell;

                    cell = Grid.GetCell(new Vector3Int(nCheck.x, origPos.y, nCheck.z), false);
                    if (FGenerators.CheckIfExist_NOTNULL(cell)) if (cell.InTargetGridArea) return cell;

                    cell = Grid.GetCell(new Vector3Int(origPos.x, nCheck.y, nCheck.z), false);
                    if (FGenerators.CheckIfExist_NOTNULL(cell)) if (cell.InTargetGridArea) return cell;
                }
            }

            return cell;
        }


        public Vector3 GetWorldPos(FieldCell cell)
        {
            if (cell == null) return RootPosition;
            return Matrix.MultiplyPoint3x4(cell.Pos);
        }

        public Vector3 GetWorldPos(Vector3Int gridLocalPos)
        {
            return Matrix.MultiplyPoint3x4(gridLocalPos);
        }

        public Vector3 GetWorldPos(FieldCell cell, Matrix4x4 mx)
        {
            return mx.MultiplyPoint3x4(cell.Pos);
        }
        public Vector3 GetWorldPos(int index, Matrix4x4 mx)
        {
            return mx.MultiplyPoint3x4(GetCell(index).Pos); 
        }
        public Vector3 GetWorldPos(int index)
        {
            return GetWorldPos(GetCell(index));
        }

        public Vector3 GetLocalPos(FieldCell cell)
        {
            return cell.Pos;
        }

        public Vector3Int GetLocalPos(int index)
        {
            return GetCell(index).Pos;
        }

        /// <summary>
        /// Just multiply position by field maxtrix
        /// </summary>
        public Vector3 CheckerPos(Vector3 pos)
        {
            return Matrix.MultiplyPoint3x4(pos);
        }

        public void RoundRootPositionAccordingly(CheckerField3D accordingTo)
        {
            RootPosition = RoundPositionAccordingly(accordingTo, RootPosition, RootScale.x);
        }

        public Vector3 RoundPositionAccordingly(CheckerField3D accordingTo, Vector3 position, float scale = 1f)
        {
            Matrix4x4 mx = accordingTo.Matrix_NoScale;
            Vector3 pos = mx.inverse.MultiplyPoint3x4(position);
            pos = FVectorMethods.FlattenVector(pos, scale);
            return mx.MultiplyPoint3x4(pos);
        }

        public void RoundRootPosition(float scale = 1f)
        {
            RootPosition = FVectorMethods.FlattenVector(RootPosition, scale);
        }

        public FieldCell GetNearestContainedCellInBoundsRange(Vector3 worldPos, int boundsIndex)
        {
            List<FieldCell> cells = BoundsToCells(Bounding[boundsIndex]);
            float nearest = float.MaxValue;
            int n = -1;

            Matrix4x4 mx = Matrix;

            for (int i = 0; i < cells.Count; i++)
            {
                float dist = (worldPos - mx.MultiplyPoint3x4(cells[i].Pos)).sqrMagnitude;

                if (dist < nearest)
                {
                    n = i;
                    nearest = dist;
                }

                #region Debug
                //UnityEngine.Debug.Log(i + " Cells Pos " + cells[i].Pos + " mx " + Matrix.MultiplyPoint(cells[i].Pos));
                //UnityEngine.Debug.DrawRay(Matrix.MultiplyPoint(cells[i].Pos), Vector3.up, Color.red, 1.01f);
                #endregion
            }


            if (n > -1)
            {
                //UnityEngine.Debug.DrawRay(Matrix.MultiplyPoint(cells[n].Pos), Vector3.up, Color.yellow, 1.01f);
                return cells[n];
            }

            return null;
        }

    }
}