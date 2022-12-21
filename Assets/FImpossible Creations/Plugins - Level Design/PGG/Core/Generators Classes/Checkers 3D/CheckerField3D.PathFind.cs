using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FIMSpace.Generating.Checker
{
    public partial class CheckerField3D
    {
        public CheckerField3D GenerateMaskOutOfCheckers(List<CheckerField3D> ch, bool ignoreSelf, CheckerField3D additionalIgnore)
        {
            CheckerField3D mask = new CheckerField3D();

            for (int c = 0; c < ch.Count; c++)
            {
                var checker = ch[c];
                if (ignoreSelf) if (checker == this) continue;
                if (checker == additionalIgnore) continue;
                mask.Join(checker);
            }

            mask.RecalculateMultiBounds();

            return mask;
        }

        //private List<PathFindNode> _pathFind_openList = new List<PathFindNode>();
        private List<FieldCell> _pathFind_openListC = new List<FieldCell>();
        //private readonly List<PathFindNode> _pathFind_allNodes = new List<PathFindNode>();
        private List<CheckerField3D> _pathFindListHelper = new List<CheckerField3D>();

        public CheckerField3D GeneratePathFindTowards(CheckerField3D target, CheckerField3D worldSpaceCollision, PathFindParams findParams, bool removeOverlappedCells = true)
        {
            _pathFindListHelper.Clear();
            if (worldSpaceCollision != null) _pathFindListHelper.Add(worldSpaceCollision);
            return GeneratePathFindTowards(target, _pathFindListHelper, findParams, removeOverlappedCells);
        }

        public FieldCell _GeneratePathFindTowards_OtherTargetCell;

        /// <summary>
        /// Generating line towards target with collision detection
        /// </summary>
        public CheckerField3D GeneratePathFindTowards(CheckerField3D target, List<CheckerField3D> worldSpaceCollision, PathFindParams findParams, bool removeOverlappedCells = true)
        {
            //FDebug.DrawBounds3D(GetFullBoundsWorldSpace(), Color.cyan);
            //UnityEngine.Debug.DrawRay(GetFullBoundsWorldSpace().center, Vector3.up + Vector3.one * 0.05f, Color.green, 1.01f);

            FieldCell startCell = GetNearestCellTowardsWorldPos3x3(GetFullBoundsWorldSpace().center);
            FieldCell targetCell = target.GetNearestCellTo(this);
            //FieldCell targetCell = target.GetNearestCellTowardsWorldPos3x3(target.GetFullBoundsWorldSpace().center);

            Vector3 startCellWorldPos = GetWorldPos(startCell);
            Vector3 targetCellWorldPos = target.GetWorldPos(targetCell);
            _GeneratePathFindTowards_OtherTargetCell = targetCell;
            //worldSpaceCollision.RoundRootPositionAccordingly(this);
            //worldSpaceCollision.DebugLogDrawBoundings(Color.yellow);
            //UnityEngine.Debug.DrawLine(startCellWorldPos, targetCellWorldPos, Color.green, 1.01f);
            //UnityEngine.Debug.DrawLine(_nearestMyBoundsPos, targetCellWorldPos, Color.green, 1.01f);

            CheckerField3D path = new CheckerField3D();
            path.CopyParamsFrom(this);

            startCell = path.AddLocal(startCell.Pos);
            startCell._PathFind_CalculateTotalDistance3D(targetCellWorldPos);
            startCell._PathFind_movementCost = 0;
            startCell.ParentCell = null;

            _pathFind_openListC.Clear();
            _pathFind_openListC.Add(startCell);


            _pathFind_cheapestCost = float.MaxValue;
            _pathFind_cheapestNodeC = null;

            Vector3Int targetPosInPathLocal = path.WorldToGridPos(targetCellWorldPos);
            FieldCell targetCellInPathGraph = path.AddLocal(targetPosInPathLocal);

            //UnityEngine.Debug.DrawLine(path.GetWorldPos(startCell), path.GetWorldPos(targetPosInPathLocal), Color.magenta, 1.01f);

            int limitIter = 81500 + 4 * Mathf.RoundToInt(Vector3Int.Distance(targetPosInPathLocal, startCell.Pos));
            int l = 0; // Counter which defines number of iterations during searching, if searching is too long we can stop it and create path based on created data
            FieldCell cheapest = null;
            _pathFind_endCellOther = null;

            while (_pathFind_openListC.Count > 0) /* If there are nodes in queue to check */
            {
                if (l > limitIter) /* If there is too much iterations, let's stop and return path to nearest point to player */
                {
                    Debug.Log(">>>>>>>>>>>> Searching path was too long, stopped (openList.Count=" + _pathFind_openListC.Count + ") <<<<<<<<<<<<");
                    break;
                }


                cheapest = _pathFind_openListC[0];
                _pathFind_cheapestNodeC = _pathFind_openListC[0];

                // Opening node - creating child nodes around in the step
                PathFind_OpenNode(cheapest, path, target, worldSpaceCollision, targetCellWorldPos, targetPosInPathLocal, targetCellInPathGraph, findParams);

                if (_pathFind_cheapestCost == -1f || _pathFind_cheapestNodeC == targetCellInPathGraph)
                {
                    cheapest = _pathFind_cheapestNodeC;
                    //path.DebugLogDrawCellInWorldSpace(cheapest, Color.red);
                    if (_pathFind_cheapestCost == -1f)
                    {
                        _pathFind_endCellOther = _pathFind_cheapestNodeC;
                    }
                    else
                    {
                        var contactC = PathFind_TraceFirstCellOfTarget(cheapest, target, path);
                        _pathFind_endCellOther = contactC;
                        //path.DebugLogDrawCellInWorldSpace(contactC.Pos, Color.red);
                    }

                    break;
                }

                // Remove current node to check new ones with _pathFind_openListC[0]
                if (cheapest != null) _pathFind_openListC.Remove(cheapest);

                l++;
            }

            //UnityEngine.Debug.Log("sizes " + path.RootScale + " start scale: " + RootScale + " tgt: " + target.RootScale);
            ReverseTracePath(cheapest, path);

            if (removeOverlappedCells)
            {
                path.RemoveCellsCollidingWith(this);
                path.RemoveCellsCollidingWith(target);
            }

            //path.RecalculateMultiBounds();
            //path.DebugLogDrawBoundings(Color.magenta);

            return path;
        }

        int None { get { return int.MaxValue; } }
        public FieldCell GetNearestCellInWorldPos(Vector3 worldPos, int maxDist = 32)
        {
            Vector3Int local = WorldToGridPos(worldPos);

            // Check if in choosed position there is already cell
            FieldCell inExact = GetCell(local);

            if (FGenerators.CheckIfExist_NOTNULL(inExact)) if (inExact.InTargetGridArea)
                {
                    return inExact;
                }

            // If origin point is too far, reposition it towards nearest available grid area
            Bounds localB = GetFullBoundsLocalSpace();

            if (localB.Contains(local) == false)
            {
                Vector3 nrst = localB.ClosestPoint(local);
                local = nrst.V3toV3Int();
            }

            return CubicSearchForFirstCell(local, localB, maxDist);
        }

        public Vector3 GetNearestContainedWorldPosTo(Vector3 worldPos, int maxDist = 32)
        {
            FieldCell cell = GetNearestCellInWorldPos(worldPos, maxDist);
            if (FGenerators.CheckIfExist_NOTNULL(cell)) return GetWorldPos(cell);
            return Vector3.zero;
        }

        FieldCell _cubSearchRes = null;
        Vector3Int _cubSearchOrig = Vector3Int.zero;
        private bool _CubicSearchCheck(int x = 0, int y = 0, int z = 0)
        {
            _cubSearchRes = GetCell(_cubSearchOrig + new Vector3Int(x, y, z));
            if (FGenerators.CheckIfExist_NOTNULL(_cubSearchRes)) if (_cubSearchRes.InTargetGridArea) { return true; } else { _cubSearchRes = null; }

            return false;
        }

        /// <summary> Search in flat expanding space X Z. Y is checked only on the 'local.y' level </summary>
        public FieldCell CubicSearchForFirstCell(Vector3Int local, Bounds localFullBounds, int maxDist = 32)
        {
            _cubSearchRes = null;
            _cubSearchOrig = local;

            if (!_CubicSearchCheck(0, 0, 0))
            {
                bool xPStop = false;
                bool xNStop = false;
                bool zPStop = false;
                bool zNStop = false;

                for (int d = 1; d < maxDist; d++)
                {
                    bool refillBreak = false;

                    if (xPStop == false) xPStop = !IsXContainedIn(_cubSearchOrig.x + d, localFullBounds);
                    if (xNStop == false) xNStop = !IsXContainedIn(_cubSearchOrig.x - d, localFullBounds);
                    if (zPStop == false) zPStop = !IsZContainedIn(_cubSearchOrig.z + d, localFullBounds);
                    if (zNStop == false) zNStop = !IsZContainedIn(_cubSearchOrig.z - d, localFullBounds);

                    for (int r = 0; r <= d; r++)
                    {
                        if (r > 0)
                        {
                            if (xPStop == false) xPStop = !IsXContainedIn(_cubSearchOrig.x + r, localFullBounds);
                            if (xNStop == false) xNStop = !IsXContainedIn(_cubSearchOrig.x - r, localFullBounds);
                            if (zPStop == false) zPStop = !IsZContainedIn(_cubSearchOrig.z + r, localFullBounds);
                            if (zNStop == false) zNStop = !IsZContainedIn(_cubSearchOrig.z - r, localFullBounds);
                        }

                        if (zPStop == false)
                        {
                            if (xPStop == false) if (_CubicSearchCheck(d, 0, r)) { refillBreak = true; break; }
                            if (xNStop == false) if (_CubicSearchCheck(-d, 0, r)) { refillBreak = true; break; }
                        }

                        if (zNStop == false)
                        {
                            if (xPStop == false) if (_CubicSearchCheck(d, 0, -r)) { refillBreak = true; break; }
                            if (xNStop == false) if (_CubicSearchCheck(-d, 0, -r)) { refillBreak = true; break; }
                        }

                        if (xPStop == false)
                        {
                            if (zPStop == false) if (_CubicSearchCheck(r, 0, d)) { refillBreak = true; break; }
                            if (zNStop == false) if (_CubicSearchCheck(r, 0, -d)) { refillBreak = true; break; }
                        }

                        if (xNStop == false)
                        {
                            if (zPStop == false) if (_CubicSearchCheck(-r, 0, d)) { refillBreak = true; break; }
                            if (zNStop == false) if (_CubicSearchCheck(-r, 0, -d)) { refillBreak = true; break; }
                        }
                    }

                    if (refillBreak) break;
                }
            }

            return _cubSearchRes;
        }

        public FieldCell GetNearestCellTowardsWorldPos3x3(Vector3 worldPos)
        {
            FieldCell inExact = GetCellInWorldPos(worldPos);
            if (FGenerators.CheckIfExist_NOTNULL(inExact)) if (inExact.InTargetGridArea) return inExact;

            Vector3 targetPoint = GetNearestContainedWorldPosTo(worldPos); // Bounds based world position
            Vector3Int startPoint = WorldToGridPos(targetPoint); // Convert to grid position

            FieldCell startCell = Grid.GetEmptyCell(startPoint); // Get reference cell

            FieldCell[] cells = Grid.Get3x3Square(startCell, false); // Get surroundings
            //UnityEngine.Debug.Log("cells = " + cells.Length);
            FieldCell nrst = Grid.GetNearestFrom(startCell, cells);
            //UnityEngine.Debug.DrawLine(targetPoint, GetWorldPos(nrst), Color.green, 1.01f);
            //UnityEngine.Debug.DrawRay(GetWorldPos(nrst), Vector3.up, Color.green, 1.01f);

            return nrst;
        }


        /// <summary>
        /// Recurent tracing path from end to start
        /// </summary>
        private void ReverseTracePath(FieldCell cheapest, CheckerField3D owner)
        {
            if (cheapest == null) return;
            if (cheapest.ParentCell == null) return;
            //UnityEngine.Debug.DrawLine(owner.GetWorldPos(cheapest), owner.GetWorldPos(cheapest.ParentCell), Color.yellow, 1.01f);
            ReverseTracePath(cheapest.ParentCell, owner);
            owner.AddLocal(cheapest.Pos);
        }

        private FieldCell PathFind_TraceFirstCellOfTarget(FieldCell cheapest, CheckerField3D target, CheckerField3D owner)
        {
            if (cheapest.ParentCell == null) return cheapest;

            FieldCell tCell = target.GetCellInWorldPos(owner.GetWorldPos(cheapest));
            Vector3 wPos2 = owner.GetWorldPos(cheapest.ParentCell);

            if (FGenerators.NotNull(tCell)
                && FGenerators.IsNull(target.GetCellInWorldPos(wPos2)))
            {
                return tCell;
            }

            return PathFind_TraceFirstCellOfTarget(cheapest.ParentCell, target, owner);
        }

        float _pathFind_cheapestCost = float.MaxValue;
        //PathFindNode _pathFind_cheapestNode = null;
        [NonSerialized] public FieldCell _pathFind_cheapestNodeC = null;
        [NonSerialized] public FieldCell _pathFind_endCellOther = null;
        void PathFind_OpenNode(FieldCell originNode, CheckerField3D pathChecker, CheckerField3D targetChecker, List<CheckerField3D> collisionChecker, Vector3 targetWorldPos, Vector3Int targetPathEndLocalPos, FieldCell targetCell, PathFindParams findParams)
        {
            originNode._PathFind_status = -1; // Lock cell
            Vector3Int nodeOriginInGridLocal = originNode.Pos;

            #region Open Neightbours

            // Searching for nearest point towards target
            for (int i = 0; i < findParams.directions.Count; i++)
            {
                Vector3Int offsettedPosGridLocal = nodeOriginInGridLocal + findParams.directions[i].Dir;

                if (findParams.WorldSpace == false)
                {
                    if (findParams.IsOutOfLimitsLocalSpace(offsettedPosGridLocal)) continue;
                }

                FieldCell checkedPathCell = pathChecker.Grid.GetEmptyCell(offsettedPosGridLocal);
                Vector3 checkedWorldPos = pathChecker.GetWorldPos(checkedPathCell);

                if (findParams.WorldSpace)
                {
                    if (findParams.IsOutOfLimitsWorldSpace(checkedWorldPos)) continue;
                }

                if (targetChecker.ContainsWorld(checkedWorldPos)) // Reaching target checker field
                {
                    _pathFind_cheapestCost = -1f;
                    _pathFind_cheapestNodeC = pathChecker.AddWorld(checkedWorldPos);
                    _PathFindValidateNode(originNode, checkedPathCell, targetPathEndLocalPos, findParams.directions[i]);
                    return;
                }

                if (checkedPathCell._PathFind_status != 0) // Checked before
                {
                    continue;
                }

                Vector3 offsettedWorldPos = pathChecker.GetWorldPos(offsettedPosGridLocal);

                bool collision = false;
                for (int c = 0; c < collisionChecker.Count; c++)
                {
                    var cCheck = collisionChecker[c];
                    FieldCell collisionMaskCell = cCheck.GetCellInWorldPos(offsettedWorldPos);
                    if (FGenerators.CheckIfExist_NOTNULL(collisionMaskCell)) if (collisionMaskCell.InTargetGridArea) collision = true;
                    if (collision) break;
                }

                if (collision)
                {
                    checkedPathCell._PathFind_status = -2;
                    continue;
                }
                else
                {
                    _PathFindValidateNode(originNode, checkedPathCell, targetPathEndLocalPos, findParams.directions[i]);
                }
            }

            #endregion

            _pathFind_openListC = _pathFind_openListC.OrderBy(o => o._PathFind_distAndCost).ToList();
        }

        void _PathFindValidateNode(FieldCell originNode, FieldCell checkedPathCell, Vector3Int targetPathEndLocalPos, LineFindHelper direction)
        {
            checkedPathCell._PathFind_status = 1;
            checkedPathCell.ParentCell = originNode;
            checkedPathCell._PathFind_movementCost = originNode._PathFind_movementCost + direction.Cost;
            checkedPathCell._PathFind_CalculateTotalDistance3D_Local(targetPathEndLocalPos);
            checkedPathCell._PathFind_CalculateDistAndCost();
            //UnityEngine.Debug.Log("distAndCost = " + checkedPathCell._PathFind_distAndCost + " dist = " + checkedPathCell._PathFind_distance + " cost = " + checkedPathCell._PathFind_movementCost);

            if (checkedPathCell._PathFind_distAndCost < _pathFind_cheapestCost)
            {
                _pathFind_cheapestCost = checkedPathCell._PathFind_distAndCost;
                _pathFind_cheapestNodeC = checkedPathCell;
                //UnityEngine.Debug.Log("cheapest = " + _pathFind_cheapestCost);
            }

            _pathFind_openListC.Add(checkedPathCell);
        }


        public struct PathFindParams
        {
            /// <summary> World space limits or grid space limits </summary>
            public bool WorldSpace;
            public float LimitHighestY;
            public float LimitLowestY;
            public float LimitMaxX;
            public float LimitMinX;
            public float LimitMaxZ;
            public float LimitMinZ;

            public bool NoLimits;

            public List<LineFindHelper> directions;
            public int AllowChangeDirectionEvery;
            public float PrioritizeYLevel;

            public PathFindParams(List<LineFindHelper> movementDirections, float limitLowYTo = float.MaxValue, bool worldSpace = false)
            {
                directions = movementDirections;
                WorldSpace = worldSpace;
                LimitHighestY = float.MaxValue;
                LimitLowestY = limitLowYTo;
                LimitMaxX = float.MaxValue;
                LimitMinX = -float.MaxValue;
                LimitMaxZ = float.MaxValue;
                LimitMinZ = -float.MaxValue;
                NoLimits = limitLowYTo == float.MaxValue;

                AllowChangeDirectionEvery = 1;
                PrioritizeYLevel = float.NaN;
            }

            public bool IsOutOfLimitsLocalSpace(Vector3Int gridPos)
            {
                if (NoLimits) return false;

                if (gridPos.y < LimitLowestY) return true;
                if (gridPos.y > LimitHighestY) return true;

                if (gridPos.x < LimitMinX) return true;
                if (gridPos.x > LimitMaxX) return true;

                if (gridPos.z < LimitMinZ) return true;
                if (gridPos.z > LimitMaxZ) return true;

                return false;
            }

            public bool IsOutOfLimitsWorldSpace(Vector3 worldPos)
            {
                if (NoLimits) return false;

                if (worldPos.y < LimitLowestY) return true;
                if (worldPos.y > LimitHighestY) return true;

                if (worldPos.x < LimitMinX) return true;
                if (worldPos.x > LimitMaxX) return true;

                if (worldPos.z < LimitMinZ) return true;
                if (worldPos.z > LimitMaxZ) return true;

                return false;
            }

        }


    }
}