using UnityEngine;

namespace FIMSpace.Generating.Checker
{
    public partial class CheckerField3D
    {
        public enum ECheckerMeasureMode { Rectangular, Spherical }

        /// <summary> After generating outline, you can access cell.HelperDirection which points towards outside direction in grid local space </summary>
        public CheckerField3D GetOutlineChecker(int thickness = 1, ECheckerMeasureMode edgesShape = ECheckerMeasureMode.Rectangular, bool recalculate = false, bool copyCellRefs = false)
        {
            CheckerField3D outline = new CheckerField3D();
            outline.CopyParamsFrom(this);

            if (thickness > 0)
                for (int i = 0; i < ChildPositionsCount; i++)
                {
                    var pos = ChildPos(i);

                    for (int x = -thickness; x <= thickness; x++)
                    {
                        for (int z = -thickness; z <= thickness; z++)
                        {
                            if (x == 0 && z == 0) continue;

                            if (edgesShape == ECheckerMeasureMode.Spherical)
                            {
                                if (Mathf.Abs(x) == thickness)
                                    if (Mathf.Abs(z) == thickness) continue;
                            }

                            Vector3 target = pos + new Vector3(x, 0, z);
                            FieldCell cell = null;

                            if (copyCellRefs)
                            {
                                FieldCell refCell = GetCell(target.V3toV3Int());
                                if (FGenerators.NotNull(refCell))
                                {
                                    cell = outline.CopyCellRefAndAdd(refCell);
                                }
                            }
                            
                            if (FGenerators.IsNull(cell)) cell = outline.AddLocal(target);

                            Vector3 dir = new Vector3(x, 0, z);

                            if (ContainsLocal(target - dir)) cell.HelperVector = dir;
                            else cell.HelperVector = Vector3.zero;

                        }
                    }
                }

            outline.RemoveCellsCollidingWith(this);

            if (recalculate) outline.RecalculateMultiBounds();

            return outline;
        }

        /// <summary> After generating inline, you can access cell.HelperDirection which points towards outside direction in grid local space </summary>
        public CheckerField3D GetInlineChecker(bool invert = false, bool includeDiagonalCheck = true, bool detectOutDirections = false, bool recalculate = false, bool copyCellRefs = false)
        {
            CheckerField3D inline = new CheckerField3D();
            inline.CopyParamsFrom(this);

            bool leftEmpty;
            bool rightEmpty;
            bool frontEmpty;
            bool backEmpty;
            bool surrounded;

            for (int i = 0; i < ChildPositionsCount; i++)
            {

                #region Surrounded check

                FieldCell scell = GetCell(i);

                surrounded = false;
                leftEmpty = !ContainsLocal(scell.Pos + new Vector3Int(-1, 0, 0));
                rightEmpty = !ContainsLocal(scell.Pos + new Vector3Int(1, 0, 0));
                frontEmpty = !ContainsLocal(scell.Pos + new Vector3Int(0, 0, 1));
                backEmpty = !ContainsLocal(scell.Pos + new Vector3Int(0, 0, -1));

                if (!leftEmpty && !rightEmpty && !frontEmpty && !backEmpty)
                {
                    surrounded = true;

                    if (includeDiagonalCheck)
                    {
                        if (!Grid.CellIsSurroundedOnlyDiag(scell.Pos))
                            surrounded = false;
                    }
                }

                #endregion


                if (surrounded == invert)
                {
                    FieldCell cell;

                    if (copyCellRefs)
                    {
                        cell = GetCell(i);
                        inline.CopyCellRefAndAdd( cell);
                    }
                    else
                    {
                        cell = inline.AddLocal(ChildPos(i));
                    }

                    if (detectOutDirections)
                    {
                        cell.HelperVector = Vector3.zero;

                        if (leftEmpty && rightEmpty)
                        {
                            cell.HelperVector.x = 2;
                            //UnityEngine.Debug.DrawRay(GetWorldPos(cell), new Vector3(0.8f, 0f, 0f), Color.green, 1.01f);
                            //UnityEngine.Debug.DrawRay(GetWorldPos(cell), new Vector3(-0.8f, 0f, 0f), Color.green, 1.01f);
                        }
                        else
                        {
                            if (leftEmpty)
                            {
                                cell.HelperVector.x = -1;
                                //UnityEngine.Debug.DrawRay(GetWorldPos(cell), new Vector3(-0.8f, 0f, 0f), Color.green, 1.01f);
                            }
                            else
                            {
                                if (rightEmpty)
                                {
                                    //UnityEngine.Debug.DrawRay(GetWorldPos(cell), new Vector3(0.8f, 0f, 0f), Color.green, 1.01f);
                                    cell.HelperVector.x = 1;
                                }
                            }
                        }


                        if (frontEmpty && backEmpty)
                        {
                            cell.HelperVector.z = 2;
                            //UnityEngine.Debug.DrawRay(GetWorldPos(cell), new Vector3(0f, 0f, 0.8f), Color.green, 1.01f);
                            //UnityEngine.Debug.DrawRay(GetWorldPos(cell), new Vector3(0f, 0f, -0.8f), Color.green, 1.01f);
                        }
                        else
                        {
                            if (backEmpty)
                            {
                                cell.HelperVector.z = -1;
                                //UnityEngine.Debug.DrawRay(GetWorldPos(cell), new Vector3(0f, 0f, -0.8f), Color.green, 1.01f);
                            }
                            else
                            {
                                if (frontEmpty)
                                {
                                    //UnityEngine.Debug.DrawRay(GetWorldPos(cell), new Vector3(0f, 0f, 0.8f), Color.green, 1.01f);
                                    cell.HelperVector.z = 1;
                                }
                            }
                        }

                    }
                }
            }

            if (recalculate) inline.RecalculateMultiBounds();

            return inline;
        }

        public CheckerField3D GetOutlineNonDiagonal()
        {
            CheckerField3D outline = GetOutlineChecker();
            CheckerField3D nonDiags = new CheckerField3D();
            nonDiags.CopyParamsFrom(this);

            for (int i = 0; i < outline.ChildPositionsCount; i++)
            {
                var cell = outline.GetCell(i);
                var pos = outline.GetWorldPos(cell);

                if (!outline.Grid.CellIsDiagonalOut(cell))
                {
                    nonDiags.AddWorld(pos);
                }
            }

            return nonDiags;
        }

        public CheckerField3D GetDiagonals()
        {
            CheckerField3D outline = GetOutlineChecker();
            CheckerField3D diags = new CheckerField3D();
            diags.CopyParamsFrom(this);

            for (int i = 0; i < outline.ChildPositionsCount; i++)
            {
                var cell = outline.GetCell(i);
                var pos = outline.GetWorldPos(cell);

                if (outline.Grid.CellIsDiagonalOut(cell))
                {
                    diags.AddWorld(pos);
                }
            }

            return diags;
        }



        public void EraseCells()
        {
            Grid.Clear();
        }
    }
}