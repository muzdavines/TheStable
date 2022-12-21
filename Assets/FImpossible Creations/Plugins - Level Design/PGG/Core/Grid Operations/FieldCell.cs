using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    [System.Serializable]
    public class FieldCell : FGenCell
    {
        public int GetCount = 0;
        public bool IsDirty = true;
        /// <summary> If cell have assigned some spawning datas </summary>
        [SerializeField] private List<SpawnData> Spawns;
        //[NonSerialized] public CheckerField3D ParentChecker = null;
        /// <summary> If cell is occupied also by other cell (on same grid scale level) </summary>
        [NonSerialized] public FieldCell ParentCell = null;
        public Vector3Int ParentCellPos;
        [SerializeField][HideInInspector] public bool haveParentCell = false;
        /// <summary> If cell is occupying other cells, they're saved here (on same grid scale level) </summary>
        [NonSerialized] public List<FieldCell> ChildCells;
        public List<Vector3Int> ChildCellsPos;

        private Vector3 _helperPos = Vector3.zero;
        private Vector4 _helperValue = Vector4.zero;


        #region Path find related

        public int _PathFind_Status
        {
            get { return ParentCellPos.x; }
            set { ParentCellPos.x = value; }
        }

        public float _PathFind_distAndCost
        {
            get { return _helperValue.x; }
            set { _helperValue.x = value; }
        }

        public FieldCell Copy()
        {
            FieldCell cpy = (FieldCell)MemberwiseClone();

            //if (GetCustomDatasCount() > 0)
            //{
            //    cpy.cellCustomData = new List<string>();
            //    PGGUtils.TransferFromListToList(cellCustomData, cpy.cellCustomData);
            //}

            //cpy.cellCustomObjects = cellCustomObjects;
            return cpy;
        }


        public void GridPainter_AssignDataTo(ref GridPainter.PainterCell pCell)
        {
            pCell.cellCustomData = cellCustomData;
            pCell.cellCustomObjects = cellCustomObjects;
            pCell.HelperVector = HelperVector;
            pCell.isGhost = IsGhostCell;
        }

        public void GridPainter_AssignDataFrom(GridPainter.PainterCell pCell)
        {
            cellCustomData = pCell.cellCustomData;
            cellCustomObjects = pCell.cellCustomObjects;
            HelperVector = pCell.HelperVector;
            IsGhostCell = pCell.isGhost;
        }


        public float _PathFind_movementCost
        {
            get { return _helperValue.y; }
            set { _helperValue.y = value; }
        }

        public float _PathFind_distance
        {
            get { return _helperValue.z; }
            set { _helperValue.z = value; }
        }

        public float _PathFind_status
        {
            get { return _helperValue.w; }
            set { _helperValue.w = value; }
        }

        public Vector3 _PathFind_WorldPos
        {
            get { return _helperPos; }
            set { _helperPos = value; }
        }

        public void _PathFind_CalculateTotalDistance3D_Local(Vector3Int end)
        {
            _PathFind_distance = (int)(Mathf.Abs(Pos.x - end.x) + Mathf.Abs(Pos.y - end.y) + Mathf.Abs(Pos.z - end.z));
        }

        public void _PathFind_CalculateTotalDistance3D(Vector3 end)
        {
            _PathFind_distance = (int)(Mathf.Abs(_PathFind_WorldPos.x - end.x) + Mathf.Abs(_PathFind_WorldPos.y - end.y) + Mathf.Abs(_PathFind_WorldPos.z - end.z));
        }

        public float _PathFind_CalculateDistAndCost()
        {
            _PathFind_distAndCost = _PathFind_distance + _PathFind_movementCost;
            return _PathFind_distAndCost;
        }


        #endregion


        /// <summary> Additional custom data within cell </summary>
        [SerializeField] public List<string> cellCustomData;
        [SerializeField] private List<SpawnInstruction> CellInstructions;

        [SerializeField] private List<UnityEngine.Object> cellCustomObjects;


        [NonSerialized] public SpawnDiffs SpawnDiffs;

        [NonSerialized] public NeightbourPlacement neightbours;
        [NonSerialized] public List<SpawnInstruction> GuidesIn = null;

        public Vector2Int PosXZ { get { return new Vector2Int(Pos.x, Pos.z); } }

        [NonSerialized] public bool IsGhostCell = false;

        public FieldCell()
        {
            Spawns = new List<SpawnData>();
        }

        public FieldCell(Vector3Int position) : base()
        {
            Pos = position;
        }

        /// <summary> Parent cell reference is not serialized to avoid stack serialization, if lost it can be easy gathered again </summary>
        public void RefreshParentCellRef(FGenGraph<FieldCell, FGenPoint> grid)
        {
            if (haveParentCell == false) return;
            if (FGenerators.CheckIfIsNull(ParentCell)) ParentCell = grid.GetCell(ParentCellPos);
        }

        public virtual void Clear()
        {
            ParentCell = null;
            haveParentCell = false;
            GetCount = 0;
            ResetCellsHierarchy();
            if (ChildCells != null) ChildCells.Clear();
            if (cellCustomData != null) { cellCustomData.Clear(); cellCustomData = null; }
            Spawns.Clear();
            if (CellInstructions != null) CellInstructions.Clear();
        }

        internal void OccupyOther(FieldCell child)
        {
            if (FGenerators.CheckIfIsNull(child)) return;

            if (ChildCells == null)
            {
                ChildCells = new List<FieldCell>();
                ChildCellsPos = new List<Vector3Int>();
            }

            if (ChildCells.Contains(child) == false)
            {
                ChildCells.Add(child);
                ChildCellsPos.Add(child.Pos);
            }

            child.haveParentCell = true;
            child.ParentCell = this;
        }

        internal void UnOccupyOther(FieldCell child)
        {
            if (ChildCells == null) return;
            if (ChildCells.Contains(child) == false) return;
            ChildCellsPos.Remove(child.Pos);
            ChildCells.Remove(child);
            child.haveParentCell = false;
            child.ParentCell = null;
        }

        public void AddSpawnToCell(SpawnData spawn)
        {
            if (Spawns.Contains(spawn) == false) Spawns.Add(spawn);
        }

        public void RemoveSpawnFromCell(SpawnData spawn)
        {
            if (Spawns.Contains(spawn)) Spawns.Remove(spawn);
        }

        public void RemoveAllSpawnsFromCell()
        {
            Spawns.Clear();
        }

        public int GetJustCellSpawnCount()
        {
            return Spawns.Count;
        }

        public List<SpawnData> GetSpawnsJustInsideCell(bool returnCopyOfList = false)
        {
            if (Spawns == null) Spawns = new List<SpawnData>();

            if (returnCopyOfList)
            {
                List<SpawnData> list = new List<SpawnData>();
                PGGUtils.TransferFromListToList<SpawnData>(Spawns, list);
                return list;
            }
            else
                return Spawns;
        }

        /// <summary>
        /// Including parent and child cells if occupied and also scaled grid cells if using access override
        /// </summary>
        public List<SpawnData> CollectSpawns(FieldSpawner.ESR_CellHierarchyAccess access = FieldSpawner.ESR_CellHierarchyAccess.SameScale, bool alwaysNewList = false)
        {
            if (FGenerators.CheckIfIsNull(ParentCell))
            {
                if (access == FieldSpawner.ESR_CellHierarchyAccess.SameScale)
                {
                    return GetSpawnsJustInsideCell(alwaysNewList);
                    //List<SpawnData> datas = new List<SpawnData>();
                    ////if (ChildCells != null) if (ChildCells.Count > 0)
                    ////        for (int i = 0; i < ChildCells.Count; i++) StreamSpawnListToOther(ChildCells[i].GetSpawnsJustInsideCell(), datas);

                    //StreamSpawnListToOther(Spawns, datas);
                    //return datas;
                }
                else
                {
                    // Handling accessing scaled graphs
                    List<SpawnData> datas = new List<SpawnData>();

                    if (access == FieldSpawner.ESR_CellHierarchyAccess.LowerAndSame || access == FieldSpawner.ESR_CellHierarchyAccess.HigherAndSame)
                    {
                        StreamSpawnListToOther(Spawns, datas);
                    }

                    CellAccessProcess(this, access, datas);

                    return datas;
                }
            }
            else
            {
                if (ParentCell.ChildCells == null)
                {
                    if (access == FieldSpawner.ESR_CellHierarchyAccess.SameScale)
                    {
                        return GetSpawnsJustInsideCell(alwaysNewList);
                        //List<SpawnData> datass = new List<SpawnData>();
                        //StreamSpawnListToOther(Spawns, datass);
                        //return datass;
                    }
                    else
                    {
                        List<SpawnData> datasacc = new List<SpawnData>();

                        if (access == FieldSpawner.ESR_CellHierarchyAccess.LowerAndSame || access == FieldSpawner.ESR_CellHierarchyAccess.HigherAndSame)
                            StreamSpawnListToOther(Spawns, datasacc);

                        CellAccessProcess(this, access, datasacc);

                        return datasacc;
                    }
                }

                List<SpawnData> datas = new List<SpawnData>();
                StreamSpawnListToOther(ParentCell.Spawns, datas);

                for (int i = 0; i < ParentCell.ChildCells.Count; i++)
                {
                    if (FGenerators.CheckIfIsNull(ParentCell.ChildCells[i])) continue;
                    FieldCell chld = ParentCell.ChildCells[i];

                    for (int s = 0; s < chld.Spawns.Count; s++)
                        if (FGenerators.CheckIfExist_NOTNULL(chld.Spawns[s]))
                            if (chld.Spawns[s].Enabled)
                                datas.Add(chld.Spawns[s]);
                }

                StreamSpawnListToOther(Spawns, datas, true);


                if (access != FieldSpawner.ESR_CellHierarchyAccess.SameScale)
                {
                    CellAccessProcess(ParentCell, access, datas);
                }

                return datas;
            }
        }

        internal void OverrideYPos(int yLevel)
        {
            Pos = new Vector3Int(Pos.x, yLevel, Pos.z);
        }

        static void CellAccessProcess(FieldCell startCell, FieldSpawner.ESR_CellHierarchyAccess access, List<SpawnData> datas)
        {
            if (/*access == FieldSpawner.ESR_CellHierarchyAccess.HigherScale ||*/ access == FieldSpawner.ESR_CellHierarchyAccess.HigherAndSame)
            {
                //UnityEngine.Debug.Log("HaveScaleParentCells = " + startCell.HaveScaleParentCells());
                if (startCell.HaveScaleParentCells()) foreach (FieldCell biggerCell in startCell.GetScaleParentCells()) { if (FGenerators.CheckIfExist_NOTNULL(biggerCell)) startCell.StreamSpawnListToOther(biggerCell.CollectSpawns(access), datas); }
            }
            else if (/*access == FieldSpawner.ESR_CellHierarchyAccess.LowerScale || */access == FieldSpawner.ESR_CellHierarchyAccess.LowerAndSame)
            {
                if (startCell.HaveScaleChildCells()) foreach (FieldCell lowerCell in startCell.GetScaleChildCells()) { if (FGenerators.CheckIfExist_NOTNULL(lowerCell)) startCell.StreamSpawnListToOther(lowerCell.CollectSpawns(access), datas); }
            }
        }

        void StreamSpawnListToOther(List<SpawnData> from, List<SpawnData> to, bool containsCheck = false)
        {
            if (from == null) return;

            if (!containsCheck) // Faster if contains check not needed
            {
                for (int i = 0; i < from.Count; i++) if (FGenerators.CheckIfExist_NOTNULL(from[i])) { if (from[i].Enabled) to.Add(from[i]); }
            }
            else
            {
                for (int i = 0; i < from.Count; i++) if (FGenerators.CheckIfExist_NOTNULL(from[i])) if (!to.Contains(from[i])) { if (from[i].Enabled) to.Add(from[i]); }
            }
        }

        internal Bounds ToBounds(float scale, Vector3 offset)
        {
            return new Bounds(new Vector3(Pos.x + 0.5f + offset.x, offset.y, Pos.z + 0.5f + offset.z) * scale, Vector3.one * scale);
        }


        public void CheckNeightboursRelation(FGenGraph<FieldCell, FGenPoint> onGraph)
        {
            neightbours = new NeightbourPlacement();

            for (int i = 0; i <= 8; i++)
            {
                NeightbourPlacement.ENeightbour n = (NeightbourPlacement.ENeightbour)i;
                if (n == NeightbourPlacement.ENeightbour.Middle) continue;

                Vector3 dir = NeightbourPlacement.GetDirection(n);
                Vector3Int pos = new Vector3Int(Pos.x + (int)dir.x, Pos.y, Pos.z + (int)dir.z);
                FieldCell tempCell = onGraph.GetCell(pos, false);

                if (tempCell == null || tempCell.InTargetGridArea == false)
                {
                    neightbours.Set(n, false);
                }
                else
                {
                    neightbours.Set(n, true);
                }
            }

        }

        internal int GetSpawnsWithModCount(FieldModification mod)
        {
            int count = 0;
            for (int i = 0; i < Spawns.Count; i++)
                if (Spawns[i].OwnerMod == mod) count++;
            return count;
        }

        public void AddCustomData(string dataString)
        {
            if (cellCustomData == null) cellCustomData = new List<string>();
            if (cellCustomData.Contains(dataString) == false)
            {
                cellCustomData.Add(dataString);
            }
        }

        public void AddCustomObject(UnityEngine.Object obj)
        {
            if (cellCustomObjects == null) cellCustomObjects = new List<UnityEngine.Object>();
            if (cellCustomObjects.Contains(obj) == false) cellCustomObjects.Add(obj);
        }

        public bool HaveCustomObject(UnityEngine.Object obj)
        {
            if (cellCustomObjects == null) return false;
            if (obj == null) return false;
            return cellCustomObjects.Contains(obj);
        }

        /// <summary> [!If there is no objects in cell, null object is returned!] Returns new list with unity objects previously added to the cell contests.</summary>
        public List<UnityEngine.Object> GetCustomObjectsInCell()
        {
            if (cellCustomObjects == null) return null;
            if (cellCustomObjects.Count == 0) return null;

            List<UnityEngine.Object> objs = new List<UnityEngine.Object>();

            for (int i = 0; i < cellCustomObjects.Count; i++)
            {
                if (cellCustomObjects[i] == null) continue;
                objs.Add(cellCustomObjects[i]);
            }

            return objs;
        }

        public int GetCustomDatasCount()
        {
            if (cellCustomData == null) return 0;
            return cellCustomData.Count;
        }

        /// <summary> For debugging purposes </summary>
        public string GetAllCustomDatasString()
        {
            if (GetCustomDatasCount() <= 0) return "No Data";

            string datas = "";

            for (int i = 0; i < cellCustomData.Count; i++)
            {
                datas += cellCustomData[i] + ((i == cellCustomData.Count - 1) ? "" : "  |  ");
            }

            return datas;
        }

        public bool HaveCustomData(string targetData)
        {
            if (targetData.Length > 0) if (targetData[0] == '!') if (cellCustomData == null) return true;

            if (cellCustomData == null) return false;

            if (targetData[0] == '!')
            {
                return !cellCustomData.Contains(targetData.Substring(1, targetData.Length - 1));
            }
            else
                return cellCustomData.Contains(targetData);
        }

        public void AddCellInstruction(SpawnInstruction instruction)
        {
            if (CellInstructions == null) CellInstructions = new List<SpawnInstruction>();

            for (int i = 0; i < CellInstructions.Count; i++)
                if (CellInstructions[i].definition.InstructionType == instruction.definition.InstructionType) return;

            if (CellInstructions.Contains(instruction) == false) CellInstructions.Add(instruction);
        }

        public void ReplaceInstructions(List<SpawnInstruction> instructions)
        {
            CellInstructions = instructions;
        }

        public bool HaveInstructions()
        {
            if (CellInstructions == null) return false;
            if (CellInstructions.Count == 0) return false;
            return true;
        }

        public List<SpawnInstruction> GetInstructions()
        {
            if (CellInstructions == null) CellInstructions = new List<SpawnInstruction>();
            return CellInstructions;
        }

    }
}
