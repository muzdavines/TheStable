using FIMSpace.Generating;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Graph
{
    [System.Serializable]
    public class PGGModCellPort : NodePortBase
    {
        /// <summary> Container for few variables </summary>
        public struct Data
        {
            public FieldCell CellRef;
            public bool ContainsMultipleCells { get { return CellsList != null && CellsList.Count > 1; } }
            public List<FieldCell> CellsList;

            public Data(FieldCell cellRef)
            {
                CellRef = cellRef;
                CellsList = null;
            }

            public Data(List<FieldCell> cells, bool assignFirstAsMainRef)
            {
                CellRef = null;
                if (assignFirstAsMainRef) if (cells.Count > 0) CellRef = cells[0];
                CellsList = cells;
            }
        }

        public override System.Type GetPortValueType { get { return typeof(Data); } }
        public Data CellData { get; private set; }
        public FieldCell Cell { get { return CellData.CellRef; } }
        public bool ContainsMultipleCells { get { return CellData.ContainsMultipleCells; } }
        public List<FieldCell> CellsList { get { return CellData.CellsList; } }

        public void ProvideFullCellData(PGGModCellPort other)
        {
            CellData = new Data(other.Cell);
            ForcedNull = false;
        }

        public void ProvideFullCellData(FieldCell cellRef, bool clearList = true)
        {
            CellData = new Data(cellRef);
            ForcedNull = false;
            if (clearList) if (CellsList != null) CellsList.Clear();
        }

        public bool ForcedNull { get; private set; }
        internal void SetNull()
        {
            CellData = new Data(null);
            ForcedNull = true;
        }

        static List<FieldCell> _cellsContainer = new List<FieldCell>();
        internal List<FieldCell> GetAllConnectedCellsList(bool createListInstance = false, bool includeNulls = true)
        {
            List<FieldCell> cells = _cellsContainer;
            if (createListInstance) cells = new List<FieldCell>();
            else _cellsContainer.Clear();

            for (int c = 0; c < Connections.Count; c++)
            {
                var conn = Connections[c].PortReference;

                Data? d = GetDataFromPort(conn);
                if (d == null) continue; 

                var list = d.Value.CellsList;

                if (list != null)
                {
                    for (int l = 0; l < list.Count; l++)
                    {
                        if (includeNulls) cells.Add(list[l]);
                        else if (FGenerators.NotNull(list[l])) cells.Add(list[l]);
                    }
                }
                else
                {
                    if (includeNulls) cells.Add(d.Value.CellRef);
                    else if (FGenerators.NotNull(d.Value.CellRef)) cells.Add(d.Value.CellRef);
                }
            }

            return cells;
        }

        public override object DefaultValue { get { return CellData; } }

        public override Color GetColor()
        {
            return new Color(0.6f, 0.85f, .0f, 1f);
        }


        public FieldCell GetInputCellValue
        {
            get
            {
                if (PortState() == EPortPinState.Empty) { /*Debug.Log("empty");*/ return Generating.Rules.QuickSolutions.SR_ModGraph.Graph_Cell; }

                var conn = FirstNoSender().PortReference;

                Data? d = GetDataFromPort(conn);
                if (d == null) { /*Debug.Log("d null");*/ return CellData.CellRef; }

                return d.Value.CellRef;
            }
        }

        public bool ConnectedWithMultipleCells
        {
            get
            {
                int conns = 0;

                for (int c = 0; c < Connections.Count; c++)
                {
                    var conn = Connections[c].PortReference;

                    Data? d = GetDataFromPort(conn);
                    if (d == null) continue;

                    var list = d.Value.CellsList;
                    if (list != null)
                        conns += list.Count;
                    else
                        conns += 1;

                    if (conns > 1) return true;
                }

                return false;
            }
        }

        public int CountAllCellsInAllConnections
        {
            get
            {
                int conns = 0;

                for (int c = 0; c < Connections.Count; c++)
                {
                    var conn = Connections[c].PortReference;

                    Data? d = GetDataFromPort(conn);
                    if (d == null) continue;

                    var list = d.Value.CellsList;
                    if (list != null)
                        conns += list.Count;
                    else
                        conns += 1;
                }

                return conns;
            }
        }

        //public List<FieldCell> GetConnectedCellsList
        //{
        //    get
        //    {
        //        if (IsNotConnected) { return null; }

        //        var conn = FirstNoSender().PortReference;

        //        Data? d = GetDataFromPort(conn);
        //        if (d == null) { return null; }

        //        var list = d.Value.CellsList;
        //        if (list != null) if (list.Count == 0) return null;

        //        return d.Value.CellsList;
        //    }
        //}


        public Data? GetDataFromPort(IFGraphPort conn)
        {
            if (conn is PGGModCellPort)
            {
                PGGModCellPort prt = conn as PGGModCellPort;
                return prt.CellData;
            }

            if (conn is NodePortBase)
            {
                NodePortBase np = conn as NodePortBase;
                if (np.GetPortValueSafe is Data)
                {
                    return (Data)np.GetPortValueSafe;
                }
            }

            return null;
        }


        public override object GetPortValueCall(bool onReadPortCall = true)
        {
            var val = base.GetPortValueCall(onReadPortCall);

            if (val == null) return val;

            if (val.GetType() == typeof(Data)) CellData = (Data)val;
            else CellData = new Data();
            ForcedNull = false;

            if (FGenerators.CheckIfIsNull(CellData.CellRef)) return null; // If no data then return null value

            return val;
        }

        internal void ProvideCellsList(List<FieldCell> cells)
        {
            CellData = new Data(cells, true);
        }

        public override bool CanConnectWith(IFGraphPort toPort)
        {
            if (toPort is PGGModCellPort) return true;
            if (toPort is PGGVector3Port) return true;
            if (toPort.IsSender) return true;
            if (toPort.IsUniversal) return true;
            if (toPort.GetPortValueType == GetPortValueType) return true;

            return false;
        }

        public override bool AllowConnectionWithValueType(IFGraphPort other)
        {
            if (other is PGGModCellPort) return true;
            if (other.IsUniversal) return true;
            if (other.IsSender) return true;
            if (other.GetPortValueType == GetPortValueType) return true;

            return false;

        }

        internal void Clear()
        {
            CellData = new Data(null);
            ForcedNull = false;
        }


        public override bool OnClicked(Event e)
        {
            bool baseClick = base.OnClicked(e);
            if (baseClick) return true;

            FieldCell cl = GetInputCellValue;
            if (FGenerators.CheckIfExist_NOTNULL(cl))
            {
                //                if (FGenerators.CheckIfExist_NOTNULL(GetInputCheckerValue))
                //                {
                //                    GetInputCheckerValue.DebugLogDrawCellInWorldSpace(cl, Color.green, 0.25f);
                //#if UNITY_EDITOR
                //                    UnityEditor.SceneView.RepaintAll();
                //#endif
                //                }
            }

            return false;

        }

    }
}