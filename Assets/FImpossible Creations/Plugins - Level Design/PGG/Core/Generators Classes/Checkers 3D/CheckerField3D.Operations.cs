using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Checker
{
    public partial class CheckerField3D
    {
        public CheckerField3D Copy(bool copyCellContests = true)
        {
            CheckerField3D copy = (CheckerField3D)MemberwiseClone();
            copy.CopyParamsFrom(this);

            if (copyCellContests)
            {
                copy.Grid = new FGenGraph<FieldCell, FGenPoint>();
                copy.Grid.ReferenceScale = Grid.ReferenceScale;

                for (int i = 0; i < Grid.AllApprovedCells.Count; i++)
                {
                    copy.Grid.AddCell(Grid.AllApprovedCells[i].Copy());
                }
            }
            else
            {
                copy.Grid = Grid.Copy();
            }

            return copy;
        }

        public List<Vector3> CopyCellWorldPositionList()
        {
            List<Vector3> copy = new List<Vector3>();
            for (int i = 0; i < ChildPositionsCount; i++) copy.Add(GetWorldPos(i));
            return copy;
        }

        public List<FieldCell> CopyGridCellsList()
        {
            List<FieldCell> copy = new List<FieldCell>();
            for (int i = 0; i < ChildPositionsCount; i++) copy.Add(GetCell(i));
            return copy;
        }

        public void ChangeOrigin(Vector3 localOrigin)
        {
            Vector3 pos = RootPosition;
            Vector3 center = localOrigin;
            pos.y = 0f; center.y = 0f;

            Vector3Int off = (center - pos).V3toV3Int();
            for (int i = 0; i < AllCells.Count; i++)
            {
                Grid.MoveCell(AllCells[i], AllCells[i].Pos - off);
            }
        }

        public void CenterizeOrigin()
        {
            ChangeOrigin(GetFullBoundsLocalSpace().center);
        }

        public void CopyParamsFrom(CheckerField3D from)
        {
            _rootPosition = from._rootPosition;
            _rootRotation = from._rootRotation;
            RootScale = from.RootScale;
            UseBounds = from.UseBounds;
            AttachRootTo = from.AttachRootTo;
        }

        public static bool DebugHelper = false;
        public FieldCell _CheckCollisionOnSideCell { get; private set; }
        public FieldCell _CheckCollisionOnSideCellOther { get; private set; }

        internal void AddCellsOfOther(CheckerField3D oChecker)
        {
            for (int i = 0; i < oChecker.ChildPositionsCount; i++)
            {
                Vector3 wPos = oChecker.GetWorldPos(i);
                AddWorld(wPos);
            }
        }

        /// <summary> Found collision cell will be stored in _CheckCollisionOnSideCell </summary>
        public bool CheckCollisionOnSide(Vector3Int dir, float distance, List<CheckerField3D> collideWith)
        {
            for (int i = 0; i < collideWith.Count; i++)
            {
                if (collideWith[i] == this) continue;

                if (CheckCollisionOnSide(dir, distance, collideWith[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CheckCollisionOnSide(Vector3Int dir, float distance, List<Planning.FieldPlanner> collideWith)
        {
            for (int i = 0; i < collideWith.Count; i++)
            {
                if (collideWith[i].LatestChecker == this) continue;

                if (CheckCollisionOnSide(dir, distance, collideWith[i].LatestChecker))
                {
                    return true;
                }
            }

            return false;
        }

        public Vector3 GetScaleConversionRootOffset(Vector3 targetScale)
        {
            if (targetScale.x == 0) return Vector3.zero;

            float myScale = RootScale.x;
            float newScale = targetScale.x;
            float divV;

            //if (myScale < newScale)
            //{
            //    divV = myScale / 2f - newScale / 2f;
            //    return new Vector3(divV, 0f, divV);
            //}

            divV = myScale / 2f - newScale / 2f;
            return -new Vector3(divV, 0f, divV);
        }

        public CheckerField3D GenerateCheckerConvertedToNewScale(Vector3 targetScale, CheckerField3D willApplyTo)
        {
            float myScale = RootScale.x;
            float newScale = targetScale.x;
            CheckerField3D newField = new CheckerField3D();

            if (targetScale.x == 0) return newField;

            newField.CopyParamsFrom(this);
            newField.RootScale = targetScale;

            if (willApplyTo != null)
            {
                willApplyTo.RootPosition = RootPosition;// + GetScaleConversionRootOffset(targetScale);
                willApplyTo.RootRotation = RootRotation;
                newField.RootPosition = RootPosition;
                newField.RootRotation = RootRotation;
            }

            if (targetScale == RootScale) // Same scale, just copy
            {
                newField = Copy();
            }
            else if (myScale > newScale) // Converting bigger field onto smaller cells
            {
                bool ceil = myScale % newScale != 0f;
                int rescaleIters;

                if (ceil)
                    rescaleIters = Mathf.CeilToInt(myScale / newScale);
                else
                    rescaleIters = Mathf.RoundToInt(myScale / newScale);

                Matrix4x4 myMx = Matrix;
                Matrix4x4 oInvMx = newField.Matrix.inverse;

                for (int i = 0; i < ChildPositionsCount; i++)
                {
                    Vector3 wPos = GetWorldPos(i, myMx);

                    for (int x = 0; x < rescaleIters; x++)
                        for (int z = 0; z < rescaleIters; z++)
                        {
                            //if (ceil)
                            //    newField.AddLocal(newField.WorldToLocal(wPos + new Vector3(x * newScale, 0, z * newScale), oInvMx).V3toV3IntC());
                            //else
                            newField.AddLocal(newField.WorldToLocal(wPos + new Vector3(x * newScale, 0, z * newScale), oInvMx).V3toV3IntF());
                        }
                }
            }
            else // Converting group of smaller cells onto bigger ones
            {

                //float mod = (newScale % myScale);
                //int iterSkip = Mathf.RoundToInt(newScale / myScale);
                float iterSkipF = Mathf.Round(newScale / myScale);

                for (int i = 0; i < ChildPositionsCount; i++)
                {
                    Vector3 locOrigin = GetLocalPos(i).V3IntToV3();

                    Vector3Int nPos = new Vector3Int();
                    nPos.x = Mathf.FloorToInt(locOrigin.x / iterSkipF);
                    nPos.y = Mathf.FloorToInt(locOrigin.y / iterSkipF);
                    nPos.z = Mathf.FloorToInt(locOrigin.z / iterSkipF);

                    newField.AddLocal(nPos);
                }

            }

            if (willApplyTo != null)
            {
                willApplyTo.RootPosition += RootRotation * GetScaleConversionRootOffset(targetScale);
            }

            return newField;
        }

        public bool CheckCollisionOnSide(Vector3Int dir, float distance, CheckerField3D collideWith)
        {
            // collide with will be checked in world space
            Vector3 wOff = ScaleV3(dir.V3IntToV3()) * (0.5f + distance);

            // Transpone world direction onto current checker local space 
            Vector3Int thisWorldwOff = (Quaternion.Inverse(RootRotation) * dir).V3toV3Int();
            Matrix4x4 otherMx = collideWith.Matrix.inverse;

            for (int i = 0; i < ChildPositionsCount; i++)
            {
                _CheckCollisionOnSideCell = GetCell(AllCells[i].Pos + thisWorldwOff);

                if (FGenerators.CheckIfExist_NOTNULL(_CheckCollisionOnSideCell))
                {
                    //if (DebugHelper)
                    //{
                    //    UnityEngine.Debug.DrawLine(GetWorldPos(i), GetWorldPos(_CheckCollisionOnSideCell.Pos), Color.red, 1.01f);
                    //    DebugLogDrawCellInWorldSpace(_CheckCollisionOnSideCell.Pos, Color.gray);
                    //}

                    continue; // cell in direction is owned by this field so ignore it
                }


                // Check collision with other cell in this position
                Vector3 worldPos = GetWorldPos(i) + wOff;
                //if ( DebugHelper) UnityEngine.Debug.DrawLine(GetWorldPos(i), worldPos, Color.green, 1.01f);

                _CheckCollisionOnSideCellOther = collideWith.GetCell(otherMx.MultiplyPoint3x4(worldPos).V3toV3Int());
                //_CheckCollisionOnSideCellOther = collideWith.GetCellInWorldPos(worldPos, otherMx);
                //UnityEngine.Debug.Log("Local like: " + otherMx.MultiplyPoint3x4(worldPos).V3toV3IntF() + " to " + otherMx.MultiplyPoint3x4(worldPos).V3toV3IntC());
                //collideWith.DebugLogDrawCellInWorldSpace(otherMx.MultiplyPoint3x4(worldPos).V3toV3IntC(), Color.red);
                //if (checkRounded)
                //{
                //if (FGenerators.CheckIfIsNull(_CheckCollisionOnSideCellOther))
                //{
                //    Vector3 locPos = otherMx.MultiplyPoint3x4(worldPos);
                //    Vector3Int origPos = locPos.V3toV3Int();

                //    Vector3Int nCheck = new Vector3Int();
                //    if (locPos.x < origPos.x) nCheck.x = Mathf.FloorToInt(locPos.x); else nCheck.x = Mathf.CeilToInt(locPos.x);
                //    if (locPos.y < origPos.y) nCheck.y = Mathf.FloorToInt(locPos.y); else nCheck.y = Mathf.CeilToInt(locPos.y);
                //    if (locPos.z < origPos.z) nCheck.z = Mathf.FloorToInt(locPos.z); else nCheck.z = Mathf.CeilToInt(locPos.z);

                //    collideWith.DebugLogDrawCellInWorldSpace(new Vector3Int(nCheck.x, nCheck.y, origPos.z), Color.cyan);
                //    collideWith.DebugLogDrawCellInWorldSpace(new Vector3Int(nCheck.x, origPos.y, nCheck.z), Color.magenta);
                //    collideWith.DebugLogDrawCellInWorldSpace(new Vector3Int(origPos.x, nCheck.y, nCheck.z), Color.blue);

                //    _CheckCollisionOnSideCellOther = Grid.GetCell(new Vector3Int(nCheck.x, nCheck.y, origPos.z), false);
                //    if (FGenerators.CheckIfIsNull(_CheckCollisionOnSideCellOther))
                //    {
                //        _CheckCollisionOnSideCellOther = Grid.GetCell(new Vector3Int(nCheck.x, origPos.y, nCheck.z), false);
                //        if (FGenerators.CheckIfIsNull(_CheckCollisionOnSideCellOther))
                //        {
                //            _CheckCollisionOnSideCellOther = Grid.GetCell(new Vector3Int(origPos.x, nCheck.y, nCheck.z), false);
                //        }
                //    }
                //}
                //}


                if (FGenerators.CheckIfExist_NOTNULL(_CheckCollisionOnSideCellOther))
                {
                    // Collision occured!
                    //if ( DebugHelper) collideWith.DebugLogDrawCellInWorldSpace(_CheckCollisionOnSideCellOther, Color.red);
                    return true;
                }
                else
                {
                    //if ( DebugHelper) collideWith.DebugLogDrawCellInWorldSpace(collideWith.WorldToGridPos(worldPos), Color.yellow);
                }

            }

            return false;
        }

        internal Vector3 DirectionToLocal(Vector3Int off)
        {
            return Matrix_NoScale.inverse.MultiplyVector(off);
        }

        public float BoundsDistanceTo(CheckerField3D relationTo)
        {
            Bounds myBounds = GetFullBoundsWorldSpace();
            Bounds otherBounds = relationTo.GetFullBoundsWorldSpace();

            Vector3 myCentToOth = otherBounds.ClosestPoint(myBounds.center);
            Vector3 othCentToMy = myBounds.ClosestPoint(otherBounds.center);
            Vector3 mid = Vector3.LerpUnclamped(myCentToOth, othCentToMy, 0.5f);

            myCentToOth = myBounds.ClosestPoint(mid);
            othCentToMy = otherBounds.ClosestPoint(mid);

            return Vector3.Distance(myCentToOth, othCentToMy);
        }

        public void SetSize(int width, int yLevels, int depth)
        {
            for (int x = 1; x <= width; x++)
                for (int y = 1; y <= yLevels; y++)
                    for (int z = 1; z <= depth; z++)
                        Grid.AddCell(x - 1, y - 1, z - 1);
        }

        public void RemoveCellsCollidingWith(CheckerField3D other/*, bool getRounded = false*/)
        {
            //for (int o = 0; o < ChildPositionsCount; o++) UnityEngine.Debug.DrawRay(GetWorldPos(o).V3toV3Int() + Vector3.one * 0.05f, Vector3.up, Color.green, 1.01f);

            // Checking smaller grid world positions
            if (other.RootScale.x < RootScale.x)
            {
                Matrix4x4 mx = Matrix.inverse;
                Matrix4x4 oMx = other.Matrix;

                for (int i = 0; i < other.ChildPositionsCount; i++)
                {
                    Vector3 wPos = other.GetWorldPos(i, oMx);
                    RemoveWorld(wPos, mx);
                }
            }
            else if (other.RootScale.x > RootScale.x) // if this root scale is smaller than other cell size
            {
                Matrix4x4 mx = Matrix;
                Matrix4x4 oMx = other.Matrix.inverse;

                for (int i = ChildPositionsCount - 1; i >= 0; i--) // other cell can be much bigger so check multiple own cells
                {
                    Vector3 tPos = GetWorldPos(i, mx);
                    if (other.ContainsWorld(tPos, oMx, false)) RemoveLocal(i); // Remove own cells in the same position as other field cells
                }
            }
            else // Removing cells of the same cell size grids
            {
                if (ChildPositionsCount < other.ChildPositionsCount)
                {
                    Matrix4x4 mx = Matrix;
                    Matrix4x4 oMx = other.Matrix.inverse;

                    for (int i = ChildPositionsCount - 1; i >= 0; i--)
                    {
                        Vector3 tPos = GetWorldPos(i, mx);
                        if (other.ContainsWorld(tPos, oMx, false))
                        {
                            if (DebugHelper) UnityEngine.Debug.DrawRay(tPos, Vector3.up, Color.green, 1.01f);
                            RemoveLocal(i);
                        }
                    }
                }
                else
                {
                    Matrix4x4 mx = Matrix.inverse;
                    Matrix4x4 oMx = other.Matrix;

                    for (int i = other.ChildPositionsCount - 1; i >= 0; i--)
                    {
                        Vector3 tPos = other.GetWorldPos(i, oMx);
                        RemoveWorld(tPos, mx);
                    }
                }
            }
        }

        public FieldCell _nearestCellOtherField = null;

        /// <summary> Returns nearest found cell of this field towards other field, nearest cell of other field is stored in _nearestCellOtherField </summary>
        public FieldCell GetNearestCellTo(CheckerField3D other, bool fast = true)
        {
            if (AllCells.Count == 0) return null;
            if (other.AllCells.Count == 0) return null;

            FieldCell myNearest;

            if (fast)
            {
                Bounds myBounds = GetFullBoundsWorldSpace();
                Bounds otherBounds = other.GetFullBoundsWorldSpace();

                Vector3 myCentToOth = otherBounds.ClosestPoint(myBounds.center);
                Vector3 othCentToMy = myBounds.ClosestPoint(otherBounds.center);
                Vector3 mid = Vector3.LerpUnclamped(myCentToOth, othCentToMy, 0.5f);

                myCentToOth = myBounds.ClosestPoint(mid);
                othCentToMy = otherBounds.ClosestPoint(mid);

                myNearest = GetNearestCellInWorldPos(myCentToOth);
                FieldCell otherNearest = other.GetNearestCellInWorldPos(othCentToMy);

                _nearestCellOtherField = otherNearest;
            }
            else // Check every cell
            {
                myNearest = AllCells[0];
                FieldCell otherNearest = other.AllCells[0];

                float mNrst = float.MaxValue;

                Matrix4x4 mx = Matrix;
                Matrix4x4 oMx = other.Matrix;

                for (int m = 0; m < AllCells.Count; m++)
                {
                    Vector3 myPos = GetWorldPos(m, mx);

                    for (int o = 0; o < other.AllCells.Count; o++)
                    {
                        Vector3 oPos = other.GetWorldPos(o, oMx);

                        float dist = (myPos - oPos).sqrMagnitude;
                        if (dist < mNrst)
                        {
                            mNrst = dist;
                            myNearest = AllCells[m];
                            otherNearest = other.AllCells[o];
                        }

                    }
                }

                _nearestCellOtherField = otherNearest;
            }

            #region Debugging

            //if (FGenerators.CheckIfExist_NOTNULL(myNearest)) UnityEngine.Debug.DrawRay(GetWorldPos(myNearest), Vector3.up, Color.red, 1.01f);
            //if (FGenerators.CheckIfExist_NOTNULL(otherNearest)) UnityEngine.Debug.DrawRay(other.GetWorldPos(otherNearest), Vector3.up, Color.red, 1.01f);
            //if (FGenerators.CheckIfExist_NOTNULL(myNearest))
            //if (FGenerators.CheckIfExist_NOTNULL(otherNearest))
            //        UnityEngine.Debug.DrawLine(GetWorldPos(myNearest), other.GetWorldPos(otherNearest), Color.red, 1.01f);


            #endregion

            return myNearest;
        }

        /*[NonSerialized]*/
        public bool FailedToSet { get; private set; }
        public void MarkAsFailed()
        {
            FailedToSet = true;
        }

        //int _lineSearchDistance = -1;
        public FieldCell LineSearch(FieldCell start, Vector3Int dir, CheckerField3D searchFor, int maxCellsDist = 64)
        {
            Vector3 startWorld = GetWorldPos(start);
            Vector3Int startOtherLocal = searchFor.WorldToGridPos(startWorld);

            // If start cell position is contained by target field
            if (searchFor.ContainsLocal(startOtherLocal))
            {
                _nearestCellOtherField = searchFor.GetCell(startOtherLocal);
                return start;
            }

            // Help to define search ranges with full bounds
            Bounds oFull = searchFor.GetFullBoundsLocalSpace();
            int teleportDistanceForDir = -1;

            // If point is not already in bounds, let's teleport it towards bounds
            if (!oFull.Contains(startOtherLocal))
            {

                bool reachable = true;

                // Define if search will newer hit target field
                if (dir.x != 0 && !LineCheckReachableOnXAxis(startOtherLocal, dir.x, oFull)) reachable = false;
                if (dir.y != 0 && !LineCheckReachableOnYAxis(startOtherLocal, dir.y, oFull)) reachable = false;
                if (dir.z != 0 && !LineCheckReachableOnZAxis(startOtherLocal, dir.z, oFull)) reachable = false;

                #region Check if there is possibility to reach on other axis

                if (reachable)
                {
                    if (dir.x != 0) // check if Y and Z axis are contained
                    {
                        if (!IsYContainedIn(startOtherLocal.y, oFull)
                           || !IsZContainedIn(startOtherLocal.z, oFull))
                        {
                            reachable = false;
                        }

                        if (reachable)
                        {
                            teleportDistanceForDir = LineCheckDistanceOnXAxis(startOtherLocal, dir.x, oFull);
                        }
                    }

                    if (dir.y != 0) // check if X and Z axis are contained
                    {
                        if (!IsXContainedIn(startOtherLocal.x, oFull)
                                || !IsZContainedIn(startOtherLocal.z, oFull))
                        {
                            reachable = false;
                        }

                        if (reachable)
                        {
                            teleportDistanceForDir = LineCheckDistanceOnYAxis(startOtherLocal, dir.y, oFull);
                        }
                    }

                    if (dir.z != 0) // check if Y and X axis are contained
                    {
                        if (!IsYContainedIn(startOtherLocal.y, oFull)
                                || !IsXContainedIn(startOtherLocal.x, oFull))
                        {
                            reachable = false;
                        }

                        if (reachable)
                        {
                            teleportDistanceForDir = LineCheckDistanceOnZAxis(startOtherLocal, dir.z, oFull);
                        }
                    }
                }

                #endregion

                // Teleport start point and define additional distance if needed
            }

            if (teleportDistanceForDir > 0)
            { }
            else teleportDistanceForDir = 0; // reset -1 to zero



            for (int i = 0; i < maxCellsDist - teleportDistanceForDir; i++)
            {
                Vector3Int checkPos = startOtherLocal;
                checkPos += dir * (i + teleportDistanceForDir);

                FieldCell c = searchFor.GetCell(checkPos);
                if (FGenerators.CheckIfExist_NOTNULL(c))
                {
                    c.LastSearchDistance = teleportDistanceForDir + i;
                    c.LastSearchTeleport = teleportDistanceForDir;
                    return c;
                }
            }


            return null;
        }

        /// <summary> Returning cell of "other" field, self contact cell stored in _FindCellOfInDir_MyCell</summary>
        public FieldCell FindCellOfInDir(CheckerField3D other, Vector3 dir, int maxDistance = 1)
        {
            _FindCellOfInDir_MyCell = null;
            Vector3 wDir = ScaleV3(dir);
            Matrix4x4 oInvMx = other.Matrix.inverse;

            for (int i = 0; i < ChildPositionsCount; i++)
            {
                for (int d = 1; d <= maxDistance; d++)
                {
                    Vector3 wPos = GetWorldPos(i) + wDir * d;
                    Vector3Int oLocal = other.WorldToGridPos(wPos, oInvMx);

                    if (other.ContainsLocal(oLocal))
                    {
                        _FindCellOfInDir_MyCell = GetCell(i);
                        return other.GetCell(oLocal);
                    }
                }
            }

            return null;
        }

        [NonSerialized] public FieldCell _GetMostCenteredCellInAxis_MyCell = null;
        /// <summary> Returns most centered cell of other field, self cell is stored in _GetMostCenteredCellInAxis_MyCell variable </summary>
        public FieldCell GetMostCenteredCellInAxis(CheckerField3D other, FieldCell myCell, FieldCell oCell, Vector3Int toOtherCell)
        {
            Vector2Int minMaxStep = Vector2Int.zero;

            Vector3 myWorld = GetWorldPos(myCell);
            Vector3 stepForw = toOtherCell.V3IntToV3() * RootScale.x;

            Vector3 othWorld = other.GetWorldPos(oCell);

            for (int i = 0; i < 10; i++)
            {
                FieldCell myStep = GetCellInWorldPos(myWorld + stepForw * i);
                if (FGenerators.CheckIfIsNull(myStep)) break;

                FieldCell othStep = other.GetCellInWorldPos(othWorld + stepForw * i);
                if (FGenerators.CheckIfIsNull(othStep)) break;

                minMaxStep.x += 1;
            }

            for (int i = 0; i < 10; i++)
            {
                FieldCell myStep = GetCellInWorldPos(myWorld - stepForw * i);
                if (FGenerators.CheckIfIsNull(myStep)) break;

                FieldCell othStep = other.GetCellInWorldPos(othWorld - stepForw * i);
                if (FGenerators.CheckIfIsNull(othStep)) break;

                minMaxStep.y -= 1;
            }

            if (minMaxStep == Vector2Int.zero)
            {
                _GetMostCenteredCellInAxis_MyCell = myCell;
                return oCell;
            }

            int mid = Mathf.RoundToInt(Mathf.Lerp(minMaxStep.x, minMaxStep.y, 0.5f));

            if (mid == 0)
            {
                _GetMostCenteredCellInAxis_MyCell = myCell;
                return oCell;
            }
            else
            {
                _GetMostCenteredCellInAxis_MyCell = GetCellInWorldPos(myWorld + stepForw * mid);
                return other.GetCellInWorldPos(othWorld + stepForw * mid);
            }
        }

        /// <summary> Returns most centered cell on the edge in provided direction </summary>
        public FieldCell GetMostCenteredCellInAxis(FieldCell myCell, Vector3Int checkAxis)
        {
            Vector2Int minMaxStep = Vector2Int.zero;

            Vector3 myWorld = GetWorldPos(myCell);
            Vector3 stepForw = checkAxis.V3IntToV3() * RootScale.x;

            for (int i = 0; i < 10; i++)
            {
                FieldCell myStep = GetCellInWorldPos(myWorld + stepForw * i);
                if (FGenerators.CheckIfIsNull(myStep)) break;

                minMaxStep.x += 1;
            }

            for (int i = 0; i < 10; i++)
            {
                FieldCell myStep = GetCellInWorldPos(myWorld - stepForw * i);
                if (FGenerators.CheckIfIsNull(myStep)) break;

                minMaxStep.y -= 1;
            }

            if (minMaxStep == Vector2Int.zero)
            {
                return myCell;
            }

            int mid = Mathf.RoundToInt(Mathf.Lerp(minMaxStep.x, minMaxStep.y, 0.5f));

            if (mid == 0)
            {
                return myCell;
            }
            else
            {
                return GetCellInWorldPos(myWorld + stepForw * mid);
            }
        }

        [NonSerialized] public FieldCell _FindCellOfInDir_MyCell = null;


        #region Bounds Utils

        /// <summary> Returns -1 if no collision available </summary>
        private int LineCheckDistanceOnXAxis(Vector3Int startPos, int sign, Bounds collisionB)
        {
            if (sign > 0) // Cast towards right
            { if (startPos.x < collisionB.max.x) return ((int)collisionB.min.x - startPos.x); }
            else
            { if (startPos.x > collisionB.min.x) return (startPos.x - (int)collisionB.max.x); }
            return -1;
        }

        private int LineCheckDistanceOnYAxis(Vector3Int startPos, int sign, Bounds collisionB)
        {
            if (sign > 0) // Cast towards right
            { if (startPos.y < collisionB.max.y) return ((int)collisionB.min.y - startPos.y); }
            else
            { if (startPos.y > collisionB.min.y) return (startPos.y - (int)collisionB.max.y); }
            return -1;
        }

        private int LineCheckDistanceOnZAxis(Vector3Int startPos, int sign, Bounds collisionB)
        {
            if (sign > 0) // Cast towards right
            { if (startPos.z < collisionB.max.z) return ((int)collisionB.min.z - startPos.z); }
            else
            { if (startPos.z > collisionB.min.z) return (startPos.z - (int)collisionB.max.z); }
            return -1;
        }

        //private bool IsXContainedInBound(float x, Bounds b)
        //{ return x >= b.min.x && x <= b.max.x; }

        //private bool IsYContainedInBound(float y, Bounds b)
        //{ return y >= b.min.y && y <= b.max.y; }

        //private bool IsZContainedInBound(float z, Bounds b)
        //{ return z >= b.min.z && z <= b.max.z; }

        private bool LineCheckReachableOnXAxis(Vector3Int startPos, int sign, Bounds collisionB)
        {
            if (sign > 0) // Cast towards right
            { if (startPos.x < collisionB.max.x) return true; }
            else
            { if (startPos.x > collisionB.min.x) return true; }
            return false;
        }

        private bool LineCheckReachableOnYAxis(Vector3Int startPos, int sign, Bounds collisionB)
        {
            if (sign > 0) // Cast towards up
            { if (startPos.y < collisionB.max.y) return true; }
            else
            { if (startPos.y > collisionB.min.y) return true; }
            return false;
        }

        private bool LineCheckReachableOnZAxis(Vector3Int startPos, int sign, Bounds collisionB)
        {
            if (sign > 0) // Cast towards forward
            { if (startPos.z < collisionB.max.z) return true; }
            else
            { if (startPos.z > collisionB.min.z) return true; }
            return false;
        }

        private bool IsXContainedIn(int x, Bounds b)
        { return (x >= b.min.x && x <= b.max.x); }
        private bool IsYContainedIn(int y, Bounds b)
        { return (y >= b.min.y && y <= b.max.y); }
        private bool IsZContainedIn(int z, Bounds b)
        { return (z >= b.min.z && z <= b.max.z); }

        #endregion


        /// <summary>
        /// Getting nearest cell which is empty or first before being empty
        /// </summary>
        /// <param name="getOutPos"> to get first empty cell instead of before empty </param>
        public Vector3Int GetNearestEdge(Vector3Int localCheckerPos, bool getOutPos = false)
        {
            var dirs = GetRandomFlatDirections();

            for (int o = 0; o < 300; o++)
            {
                for (int d = 0; d < dirs.Length; d++)
                {
                    Vector3Int check = localCheckerPos + dirs[d];
                    if (ContainsLocal(check) == false) return getOutPos ? check : (check - dirs[d]);
                }
            }

            return localCheckerPos;
        }


        /// <summary>
        /// Returns -1 if not found collision
        /// </summary>
        /// <param name="toOther"></param>
        /// <param name="direction"></param>
        /// <param name="maxDistance"></param>
        //public int CheckCollisionDistanceInDirection(CheckerField3D toOther, Vector3Int direction, int maxDistance = 25, bool fromLocalDirToWorld = false)
        //{
        //    if (fromLocalDirToWorld) direction = (RootRotation * direction).V3toV3Int();

        //    for (int i = 0; i < ChildPositionsCount; i++)
        //    {
        //        Vector3 start = GetWorldPos(i);

        //        for (int c = 0; c < maxDistance; c++)
        //        {
        //            _CheckCollisionDistanceInDirection_OtherCell = toOther.GetCellInWorldPos(start + direction * c);

        //            if (FGenerators.CheckIfExist_NOTNULL(_CheckCollisionDistanceInDirection_OtherCell))
        //            {
        //                return c;
        //            }
        //        }
        //    }

        //    return -1;
        //}

        //[NonSerialized] public FieldCell _CheckCollisionDistanceInDirection_OtherCell = null;

        /// <summary> Returns null if no collision in range, if collision detected then returning length of the ray </summary>
        public float? CheckIfCollisionPossible(FieldCell originCell, Vector3 direction, CheckerField3D other, bool fromLocalDirToWorld = true)
        {
            return CheckIfCollisionPossible(GetWorldPos(originCell), direction, other, fromLocalDirToWorld);
        }


        /// <summary> Returns null if no collision in range, if collision detected then returning length of the ray </summary>
        public float? CheckIfCollisionPossible(Vector3 originPos, Vector3 direction, CheckerField3D other, bool fromLocalDirToWorld = true)
        {
            if (fromLocalDirToWorld) direction = (RootRotation * direction).V3toV3Int();

            Vector3 worldPos = originPos;
            Bounds otherBounds = other.GetFullBoundsWorldSpace();

            float distance;
            if (otherBounds.IntersectRay(new Ray(worldPos, direction), out distance))
            {
                return distance;
            }
            else
            {
                return null;
            }
        }

        public FieldCell _CheckCollisionInDirection_OtherCell { get; private set; }
        public bool CheckCollisionInDirection(FieldCell originCell, Vector3 direction, CheckerField3D other, int maxIterations = 32, bool fromLocalDirToWorld = true)
        {
            _CheckCollisionInDirection_OtherCell = null;
            if (fromLocalDirToWorld) direction = (RootRotation * direction).V3toV3Int();

            Vector3 worldPos = GetWorldPos(originCell);
            Bounds otherBounds = other.GetFullBoundsWorldSpace();

            Vector3 scaledDir = ScaleV3(direction);

            float distance = RootScale.x;

            bool search = false;
            if (otherBounds.Contains(worldPos)) search = true;
            else
            {
                if (otherBounds.IntersectRay(new Ray(worldPos, direction), out distance))
                {
                    //UnityEngine.Debug.DrawRay(worldPos, scaledDir * 0.5f, Color.green, 1.01f);
                    //UnityEngine.Debug.DrawLine(worldPos, worldPos + direction * distance, Color.green, 1.01f);
                    search = true;
                    distance -= RootScale.x * 0.5f;
                    if (distance <= 0f) distance = RootScale.x;
                }
            }

            if (search)
            {
                Vector3 wPos = worldPos + direction * distance;

                for (int i = 0; i < maxIterations; i++)
                {
                    FieldCell oCell = other.GetCellInWorldPos(wPos + scaledDir * i);

                    if (FGenerators.CheckIfExist_NOTNULL(oCell))
                    {
                        _CheckCollisionInDirection_OtherCell = oCell;
                        //other.DebugLogDrawCellInWorldSpace(oCell, Color.red);
                        return true;
                    }
                    //else
                    //    other.DebugLogDrawCellIn(wPos + scaledDir * i, Color.green);

                }
            }

            return false;
        }

        public int CheckCollisionDistanceInDirectionLocal(CheckerField3D toOther, Vector3Int direction, int maxDistance = 25)
        {
            for (int i = 0; i < ChildPositionsCount; i++)
            {
                Vector3 start = GetLocalPos(i);

                for (int c = 0; c < maxDistance; c++)
                {
                    if (toOther.ContainsLocal(start + direction * c)) return c;
                }
            }

            return -1;
        }

        /// <summary>
        /// Move cells by 90 degrees rotation by origin - this is changing cells!
        /// If you need then RootRotation is not changing cells!
        /// </summary>
        public void Rotate(int clockwise90)
        {
            if (clockwise90 % 4 == 0) return;

            Matrix4x4 rotM = Matrix4x4.Rotate(Quaternion.Euler(0, clockwise90 * 90, 0));

            List<Vector3Int> newPos = new List<Vector3Int>();
            for (int c = 0; c < ChildPositionsCount; c++)
            {
                Vector3 transposed = rotM.MultiplyPoint(GetLocalPos(c));
                newPos.Add(transposed.V3toV3Int());
            }

            Grid.Clear();
            for (int i = 0; i < newPos.Count; i++) Grid.AddCell(newPos[i]);
        }

    }
}