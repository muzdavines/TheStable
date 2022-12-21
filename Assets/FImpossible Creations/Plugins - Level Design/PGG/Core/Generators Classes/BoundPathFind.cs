using FIMSpace.Generating.Checker;
using FIMSpace.Hidden;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.PathFind
{
    [System.Serializable]
    public class SimplePathGuide
    {
        [Header("Placement")]
        public Vector2Int Start = new Vector2Int(-8, -8);
        public EPlanGuideDirecion StartDir = EPlanGuideDirecion.Back;

        [Space(4)]
        public Vector2Int End = new Vector2Int(6, 8);
        public EPlanGuideDirecion EndDir = EPlanGuideDirecion.Forward;

        [Header("Configure Path")]
        [Range(1, 5)] public int PathThickness = 1;
        [Range(0f, 1f)] public float ChangeDirCost = .35f;

        [Header("Optionals")]
        [Tooltip("FieldSetup's command index to generate doorway (like level start special gate)")]
        public int StartGuideDoorInstruction = 0;
        [Tooltip("Generating door not in exact end point position but on center of wall which will be encountered in desired direction")]
        public bool StartGuideDoorCenterFit = false;
        public int GetStartCenterRange() { if (StartGuideDoorCenterFit) return 5; else return 0; }

        [Space(4)]
        [Tooltip("FieldSetup's command index to generate doorway (like level end gate)")]
        public int EndGuideDoorInstruction = 0;
        [Tooltip("Generating door not in exact end point position but on center of wall which will be encountered in desired direction")]
        public bool EndGuideDoorCenterFit = false;


        [Header("Inject Cell Datas")]
        [Tooltip("IF greater than zero then guide will spread data string into cells around start path position")]
        public int StartGuideSpreadDistance = 0;
        [Tooltip("String which will be putted inside generated grid cells data and can be used by FieldSetups and generators")]
        public string StartGuideCellDataToInject = "Start Area";

        [Space(4)]
        [Tooltip("IF greater than zero then guide will spread data string into cells around end path position")]
        public int EndGuideSpreadDistance = 0;
        [Tooltip("String which will be putted inside generated grid cells data and can be used by FieldSetups and generators")]
        public string EndGuideCellDataToInject = "End Area";

        public int GetEndCenterRange() { if (EndGuideDoorCenterFit) return 5; else return 0; }

        /// <summary>
        /// Generating path checker with current settings
        /// </summary>
        public CheckerField GenerateChecker(bool spreadCheckerData = true)
        {
            CheckerField checker = new CheckerField();
            checker.AddPathTowards(Start, End, ChangeDirCost, PathThickness);

            if (spreadCheckerData) SpreadCheckerDataOn(checker);

            return checker;
        }

        public void SpreadCheckerDataOn(CheckerField checker)
        {
            if (string.IsNullOrEmpty(StartGuideCellDataToInject) == false)
                checker.SpreadData(Start, StartGuideSpreadDistance, StartGuideCellDataToInject);

            if (string.IsNullOrEmpty(EndGuideCellDataToInject) == false)
                checker.SpreadData(End, EndGuideSpreadDistance, EndGuideCellDataToInject);
        }

        /// <summary>
        /// Generating bounds basing on BuildPlanPreset parameters
        /// </summary>
        public static List<Bounds> GeneratePathFindBounds(Vector2Int start, Vector2Int end, Vector2Int startDir, Vector2Int endDir, List<Vector2> pathPoints, int cellSize = 1, float changeDirectionCost = 0.1f)
        {
            if (pathPoints == null) return null;
            if (pathPoints.Count == 0) return null;

            List<Bounds> dirChangeBounds = new List<Bounds>();

            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                Vector2 dir = (pathPoints[i + 1] - pathPoints[i]).normalized;
                Vector3 size = PGGUtils.GetDirectionalSize(dir.V2toV2Int(), cellSize).V2toV3();

                Bounds cBounds = new Bounds(pathPoints[i].V2toV3() - new Vector3(0.5f, 0f, 0.5f), size);
                Bounds eBounds = new Bounds(pathPoints[i + 1].V2toV3() - new Vector3(0.5f, 0f, 0.5f), size);

                cBounds.Encapsulate(eBounds);
                //FDebug.DrawBounds3D(cBounds, Color.green);

                dirChangeBounds.Add(cBounds);
            }

            return dirChangeBounds;
        }

        public static List<Bounds> GeneratePathFindBounds(Vector2Int start, Vector2Int end, Vector2Int startDir, Vector2Int endDir, int cellSize = 1, float changeDirectionCost = 0.1f)
        {
            return GeneratePathFindBounds(start, end, startDir, endDir,
                GeneratePathFindPoints(start, end, startDir, endDir, changeDirectionCost),
                 cellSize, changeDirectionCost);
        }


        public static List<Vector2> GeneratePathFindPoints(Vector2Int start, Vector2Int end, Vector2Int startDir, Vector2Int endDir, float changeDirectionCost = 0.1f)
        {
            List<Vector2> dirChangePoints = new List<Vector2>();

            int maxIters = Mathf.RoundToInt(Vector2Int.Distance(start, end) * 3);
            PathFindHelper[] steps = new PathFindHelper[4];
            Vector2Int position = start;
            Vector2Int currentDir = startDir.Negate();
            dirChangePoints.Add(position);

            // Generating point path from one point to another
            for (int i = 0; i < maxIters; i++)
            {
                int nearestD = 0;
                float nearestDist = float.MaxValue;

                // Get step resulting in nearest position to target point
                for (int d = 0; d < 4; d++)
                {
                    steps[d] = new PathFindHelper();
                    steps[d].Dir = PathFindHelper.GetStepDirection(d);
                    steps[d].Distance = Vector2.Distance(position + steps[d].Dir, end);

                    if (steps[d].Dir != currentDir)
                    {
                        steps[d].Distance += changeDirectionCost;
                    }

                    if (steps[d].Distance < nearestDist)
                    {
                        nearestDist = steps[d].Distance;
                        nearestD = d;
                    }
                }

                PathFindHelper pfNearest = steps[nearestD];

                if (currentDir != pfNearest.Dir) // Direction change occured
                {
                    if (dirChangePoints.Contains(position) == false)
                        dirChangePoints.Add(position);
                }

                position += pfNearest.Dir;
                currentDir = pfNearest.Dir;

                if (position == end)
                {
                    if (dirChangePoints.Contains(position) == false)
                        dirChangePoints.Add(position);
                    break;
                }
            }

            return dirChangePoints;
        }


        public void DrawGizmos(float size, Vector3 cellSize, float offset = 1f)
        {
            Vector3 off = new Vector3(1, 0, 1) * size * offset;

            Gizmos.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            Vector3 pos = new Vector3(Start.x, 0, Start.y) * size - cellSize + off;
            Vector3 d = StartDir.GetDirection() * size;
            Gizmos.DrawRay(pos, d);
            Gizmos.DrawLine(pos + d, Vector3.Lerp(pos, pos + d, 0.7f) + Vector3.right * 0.12f * size);
            Gizmos.DrawLine(pos + d, Vector3.Lerp(pos, pos + d, 0.7f) - Vector3.right * 0.12f * size);
            Gizmos.DrawCube(pos, cellSize);

            pos = new Vector3(End.x, 0, End.y) * size - cellSize + off;
            d = EndDir.GetDirection() * size;
            Gizmos.DrawRay(pos, d);
            Gizmos.DrawLine(pos + d, Vector3.Lerp(pos, pos + d, 0.7f) + Vector3.right * 0.12f * size);
            Gizmos.DrawLine(pos + d, Vector3.Lerp(pos, pos + d, 0.7f) - Vector3.right * 0.12f * size);
            Gizmos.DrawCube(pos, cellSize);
        }


        struct PathFindHelper
        {
            public Vector2Int Dir;
            public float Distance;

            public static Vector2Int GetStepDirection(int iter)
            {
                if (iter == 0) return new Vector2Int(1, 0);
                else if (iter == 1) return new Vector2Int(0, 1);
                else if (iter == 2) return new Vector2Int(-1, 0);
                else return new Vector2Int(0, -1);
            }
        }

        public void InjectStartDataIntoGrid(FGenGraph<FieldCell, FGenPoint> grid)
        {
            if (StartGuideSpreadDistance > 0)
            {
                for (int x = -StartGuideSpreadDistance; x <= StartGuideSpreadDistance; x++)
                    for (int y = -StartGuideSpreadDistance; y <= StartGuideSpreadDistance; y++)
                    {
                        var cell = grid.GetCell(Start + new Vector2Int(x, y), false);
                        if (FGenerators.CheckIfExist_NOTNULL(cell )) cell.AddCustomData(StartGuideCellDataToInject);
                    }
            }
        }

        public void InjectEndDataIntoGrid(FGenGraph<FieldCell, FGenPoint> grid)
        {
            if (EndGuideSpreadDistance > 0)
            {
                for (int x = -EndGuideSpreadDistance; x <= EndGuideSpreadDistance; x++)
                    for (int y = -EndGuideSpreadDistance; y <= EndGuideSpreadDistance; y++)
                    {
                        var cell = grid.GetCell(End + new Vector2Int(x, y), false);
                        if (FGenerators.CheckIfExist_NOTNULL(cell )) cell.AddCustomData(EndGuideCellDataToInject);
                    }
            }
        }

        public SpawnInstruction GenerateStartDoorHoleInstructionOn(CheckerField checker, FieldSetup addDefinition = null)
        {
            SpawnInstruction ins = PGGUtils.GenerateInstructionTowards(checker, Start - Vector2Int.one, StartDir.GetDirection().V3toV3Int(), GetStartCenterRange());
            if (addDefinition) ins.definition = addDefinition.CellsInstructions[StartGuideDoorInstruction];
            return ins;
        }

        public SpawnInstruction GenerateEndDoorHoleInstructionOn(CheckerField checker, FieldSetup addDefinition = null)
        {
            SpawnInstruction ins = PGGUtils.GenerateInstructionTowards(checker, End, EndDir.GetDirection().V3toV3Int(), GetEndCenterRange());
            if (addDefinition) ins.definition = addDefinition.CellsInstructions[EndGuideDoorInstruction];
            return ins;
        }

        public void SetDefaultSettings()
        {
            Start = new Vector2Int(-8, -8);
            StartDir = EPlanGuideDirecion.Back;
            End = new Vector2Int(6, 8);
            EndDir = EPlanGuideDirecion.Forward;
            PathThickness = 1;
            ChangeDirCost = .35f;
            StartGuideDoorInstruction = 0;
            StartGuideDoorCenterFit = false;
            EndGuideDoorInstruction = 0;
            EndGuideDoorCenterFit = false;

            StartGuideSpreadDistance = 0;
            StartGuideCellDataToInject = "Start Area";
            EndGuideSpreadDistance = 0;
            EndGuideCellDataToInject = "End Area";
        }

    }

}