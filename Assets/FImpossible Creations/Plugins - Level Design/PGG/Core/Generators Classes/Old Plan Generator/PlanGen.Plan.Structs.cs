using FIMSpace.Generating.PathFind;
using FIMSpace.Generating.Planning;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{

    public partial class PlanHelper
    {
        public struct HelperRect
        {
            public Vector2 pos;
            //public Vector2Int cellPos;
            public Vector2 size;
            public Vector3 separationOffset;
            public Vector3 posV3 { get { return new Vector3(pos.x, 0, pos.y); } }
            public Vector2 separationOffsetV2 { get { return new Vector2(separationOffset.x, separationOffset.z); } }
            public Vector2 totalSepOffsetV2 { get { return new Vector2(totalSepOffset.x, totalSepOffset.z); } }

            public Vector3 totalSepOffset;
            public bool rotated;

            /// <summary> -1 is corridor </summary>
            public int TypeID;
            public int IndividualID;
            public int DuplicateID;
            public bool HelperBool;

            public List<ConnectionRect> Connections;
            public SingleInteriorSettings SettingsRef;

            public int ChoosedConnections;

            //public List<PlanRoomRestrictions> InteriorRestrictions;
            //public List<SpawnInstructionGuide> nearGuides;
            //public List<SpawnInstructionGuide> counterGuides;
            //public List<SpawnInstructionGuide> outsideGuides;
            public SpawnRestrictionsGroup nears;
            public SpawnRestrictionsGroup counters;
            public SpawnRestrictionsGroup outsides;

            public HelperRect(SingleInteriorSettings settingsRef, int id, int duplicateId)
            {
                SettingsRef = settingsRef;

                TypeID = settingsRef.ID;
                IndividualID = id;
                DuplicateID = duplicateId;
                pos = Vector2.zero;
                //cellPos = Vector2Int.zero;
                //size = settingsRef.Size;
                size = new Vector2(settingsRef.InternalSetup.RectSetup.Width.Max, settingsRef.InternalSetup.RectSetup.Height.Max);
                separationOffset = Vector3.zero;
                totalSepOffset = Vector3.zero;

                rotated = false;
                HelperBool = false;
                Connections = new List<ConnectionRect>();

                nears = new SpawnRestrictionsGroup();
                counters = new SpawnRestrictionsGroup();
                outsides = new SpawnRestrictionsGroup();

                //nearGuides = new List<SpawnInstructionGuide>();
                //counterGuides = new List<SpawnInstructionGuide>();
                //outsideGuides= new List<SpawnInstructionGuide>();

                ChoosedConnections = SettingsRef.DoorConnectionsCount.GetRandom();
            }

            public void EncapsateInto(Bounds b)
            {
                //FDebug.DrawBounds3D(b, Color.red);

                pos = new Vector2(b.center.x, b.center.z);
                if (b.size.x % 2 != 0) pos = new Vector2(pos.x + 0.5f, pos.y);
                if (b.size.z % 2 != 0) pos = new Vector2(pos.x, pos.y + 0.5f);
                size = new Vector2(b.size.x, b.size.z);
            }

            public void ResetSeparationOffset()
            {
                separationOffset = Vector3.zero;
                totalSepOffset = Vector3.zero;
            }

            public Vector2 RotatedSize
            {
                get
                {
                    return rotated ? (new Vector2(size.y, size.x)) : (new Vector2(size.x, size.y));
                }
            }

            public Bounds Bound
            {
                get
                {
                    Vector3 p = new Vector3(pos.x, 0, pos.y);
                    Vector3 s = rotated ? (new Vector3(size.y, 1, size.x)) : (new Vector3(size.x, 1, size.y));

                    if (s.x % 2 != 0) p = new Vector3(p.x - 0.5f, 0, p.z);
                    if (s.z % 2 != 0) p = new Vector3(p.x, 0, p.z - 0.5f);

                    return new Bounds(p, s);
                }
            }

            public Bounds GridBound
            {
                get
                {
                    Vector3 p = new Vector3(pos.x, 0, pos.y);
                    //p -= new Vector3(totalSepOffset.x, 0, totalSepOffset.z);
                    p -= new Vector3(totalSepOffset.x, 0, totalSepOffset.z);
                    //p -= new Vector3(separationOffset.x, 0, separationOffset.z);

                    Vector3 s = rotated ? (new Vector3(size.y, 1, size.x)) : (new Vector3(size.x, 1, size.y));
                    //Vector3Int sint = new Vector3Int(Mathf.RoundToInt(s.x), (int)s.y, Mathf.RoundToInt(s.z));

                    if (s.x % 2 != 0) p = new Vector3(p.x - 0.5f, 0, p.z);
                    if (s.z % 2 != 0) p = new Vector3(p.x, 0, p.z - 0.5f);

                    //p = new Vector3(Mathf.RoundToInt(p.x), p.y, Mathf.RoundToInt(p.z));

                    //UnityEngine.Debug.Log("min = " + (new Bounds(p,s).min) + " inR.GridBound.center " + p + " totaloff = " + totalSepOffset + " off = " + separationOffset);

                    return new Bounds(p, s);
                }
            }


            internal bool HaveConnectionWithCorridor()
            {
                if (Connections == null) return false;

                for (int i = 0; i < Connections.Count; i++)
                {
                    var c = Connections[i];
                    if (c.Connection1.TypeID == -1 || c.Connection2.TypeID == -1) return true;
                }

                return false;
            }

            public List<SpawnRestrictionsGroup> GetRestrictionsList()
            {
                List<SpawnRestrictionsGroup> list = new List<SpawnRestrictionsGroup>();

                if (nears.Restriction.IsRestricting())
                    if (nears.Cells.Count > 0)
                        list.Add(nears);

                if (counters.Restriction.IsRestricting())
                    if (counters.Cells.Count > 0)
                        list.Add(counters);

                if (outsides.Restriction.IsRestricting())
                    if (outsides.Cells.Count > 0)
                        list.Add(outsides);

                return list;
            }

            internal bool HaveConnectionWith(HelperRect tgtRect)
            {
                if (Connections == null) return false;

                for (int i = 0; i < Connections.Count; i++)
                {
                    var c = Connections[i];
                    if (IsEqual(c.Connection1, tgtRect) || IsEqual(c.Connection2, tgtRect)) return true;
                }

                return false;
            }

            public static bool IsEqual(HelperRect a, HelperRect b)
            {
                if (a.pos == b.pos && a.TypeID == b.TypeID && a.rotated == b.rotated && a.size == b.size) return true; else return false;
            }

            public bool CanConnectTo()
            {
                // Ignoring one-door limited rooms 
                if (TypeID != -1)
                    if (SettingsRef.JustOneDoor || SettingsRef.DoorConnectionsCount.Max == 0)
                    {
                        return false;
                    }
                    else
                    {
                        if (Connections.Count > ChoosedConnections) return false;
                    }

                return true;
            }


            /// <summary>
            /// Converting bounds to grid cells
            /// </summary>
            internal void GenerateGraphCells(FGenGraph<FieldCell, FGenPoint> corridorsGraph)
            {
                Vector3Int cellsStart = IGeneration.ConvertBoundsStartPosition(GridBound);

                Vector3Int iterations = new Vector3Int((int)GridBound.size.x, 0, (int)GridBound.size.z);
                for (int x = 0; x < iterations.x; x++)
                    for (int z = 0; z < iterations.z; z++)
                        corridorsGraph.AddCell(cellsStart.x + x, 0, cellsStart.z + z);
            }

            /// <summary>
            /// Converting bounds to grid cells
            /// </summary>
            internal List<FieldCell> GenerateGraphCells(bool additionalNeightbourData = false)
            {
                List<FieldCell> cells = new List<FieldCell>();

                Vector3Int cellsStart = IGeneration.ConvertBoundsStartPosition(GridBound);
                Vector3Int iterations = new Vector3Int((int)GridBound.size.x, 0, (int)GridBound.size.z);

                if (!additionalNeightbourData)
                {
                    for (int x = 0; x < iterations.x; x++)
                        for (int z = 0; z < iterations.z; z++)
                            cells.Add(new FieldCell() { Pos = new Vector3Int(cellsStart.x + x, 0, cellsStart.z + z) });
                }
                else
                {
                    FGenGraph<FieldCell, FGenPoint> tempGraph = new FGenGraph<FieldCell, FGenPoint>();

                    for (int x = 0; x < iterations.x; x++)
                        for (int z = 0; z < iterations.z; z++)
                        {
                            Vector3Int pos = new Vector3Int(cellsStart.x + x, 0, cellsStart.z + z);
                            var cell = tempGraph.AddCell(pos.x, 0, pos.z);
                            cells.Add(cell);
                        }

                    for (int i = 0; i < cells.Count; i++)
                        cells[i].CheckNeightboursRelation(tempGraph);
                }

                return cells;
            }
        }


        public struct ConnectionRect
        {
            public HelperRect Connection1;
            public HelperRect Connection2;
            public Vector2 pos;
            public Vector3 directOffset;
            public EAlignDir direction;
            public bool Found;
            public int Id;

            public Bounds Bound
            {
                get
                {
                    Vector3 p = new Vector3(pos.x, 0, pos.y);
                    Vector3 s = new Vector3(1, 1, 1);
                    return new Bounds(p, s);
                }
            }


            public Vector3Int V3Dir
            {
                get
                {
                    return GetDirection(direction);
                }
            }

            public Vector3 FlatDir
            {
                get
                {
                    Vector3Int v2 = V3Dir;
                    return new Vector3(v2.x, 0, v2.z);
                }
            }

            public static Vector3Int GetDirection(EAlignDir dir)
            {
                switch (dir)
                {
                    case EAlignDir.Up: return new Vector3Int(0, 0, 1);
                    case EAlignDir.Down: return new Vector3Int(0, 0, -1);
                    case EAlignDir.Left: return new Vector3Int(-1, 0, 0);
                    case EAlignDir.Right: return new Vector3Int(1, 0, 0);
                    default: return Vector3Int.zero;
                }
            }

            internal SpawnInstruction GenerateGuide(FieldSetup parentSetup, EHelperGuideType type, bool reverse = false, float mul = 1f)
            {
                SpawnInstruction guide = new SpawnInstruction
                {
                    helperConnection = this,
                    helperType = type
                };

                Bounds fromBound = Bound;

                fromBound.center -= FlatDir * (reverse ? -.5f * mul : .5f * mul); // Offset bound center by 0.5 to be aligned with grid

                guide.gridPosition = IGeneration.ConvertBoundsStartPosition(fromBound);

                if (type == EHelperGuideType.Doors)
                {
                    if (guide.definition == null) guide.definition = parentSetup.FindCellInstruction("Door Hole");
                    if (guide.definition == null) guide.definition = parentSetup.FindCellInstruction(InstructionDefinition.EInstruction.DoorHole);
                }
                else if (type == EHelperGuideType.ClearWall)
                {
                    if (guide.definition == null) guide.definition = parentSetup.FindCellInstruction("Counter Door");
                }

                guide.desiredDirection = GetDirection(direction) * (reverse ? -1 : 1);
                guide.useDirection = true;
                //UnityEngine.Debug.Log("guide.gridPosition " + guide.gridPosition+ " guide.desiredDirection " + guide.desiredDirection + " guideDefTitle = " + guide.definition.Title + " direction " + direction);
                //Debug.DrawRay(fromBound.center * 2f, guide.desiredDirection, type == EHelperGuideType.ClearWall ? Color.red : Color.yellow, 1.1f);
                //Debug.DrawRay(fromBound.center * 2f + guide.desiredDirection,  Vector3.up, type == EHelperGuideType.ClearWall ? Color.red : Color.yellow, 1.1f);

                return guide;
            }



            internal SpawnInstruction GenerateGuide(FieldSetup parentSetup, SingleInteriorSettings settings, bool reverse = false, float mul = 1f)
            {
                SpawnInstruction guide = new SpawnInstruction
                {
                    helperConnection = this
                };

                Bounds fromBound = Bound;

                fromBound.center -= FlatDir * (reverse ? -.5f * mul : .5f * mul); // Offset bound center by 0.5 to be aligned with grid

                guide.gridPosition = IGeneration.ConvertBoundsStartPosition(fromBound);

                InstructionDefinition definition = null;
                if (settings.DoorHoleCommandID < parentSetup.CellsCommands.Count)
                definition = parentSetup.CellsCommands[settings.DoorHoleCommandID];

                guide.definition = definition;

                guide.desiredDirection = GetDirection(direction) * (reverse ? -1 : 1);
                guide.useDirection = true;

                return guide;
            }


        }


    }

}