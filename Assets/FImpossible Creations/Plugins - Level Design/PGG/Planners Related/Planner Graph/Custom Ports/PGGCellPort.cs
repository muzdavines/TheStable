using FIMSpace.Generating;
using FIMSpace.Generating.Checker;
using FIMSpace.Generating.Planning;
using System;
using UnityEngine;

namespace FIMSpace.Graph
{
    [System.Serializable]
    public class PGGCellPort : NodePortBase
    {
        /// <summary> Container for few variables </summary>
        public struct Data
        {
            [NonSerialized] public FieldCell CellRef;
            [NonSerialized] public CheckerField3D ParentChecker;
            [NonSerialized] public PlannerResult ParentResult;

            public Data(FieldCell cellRef, CheckerField3D parentChecker, PlannerResult parentResult)
            {
                CellRef = cellRef;
                ParentChecker = parentChecker;
                ParentResult = parentResult;
            }
        }

        public override System.Type GetPortValueType { get { return typeof(Data); } }
        public Data CellData { get; private set; }
        public FieldCell Cell { get { return CellData.CellRef; } }
        public CheckerField3D Checker { get { return CellData.ParentChecker; } }
        public PlannerResult ParentResult { get { return CellData.ParentResult; } }

        public void ProvideFullCellData(PGGCellPort other)
        {
            CellData = new Data(other.Cell, other.Checker, other.ParentResult);
        }

        /// <summary> </summary>
        /// <param name="parentChecker">Usually .latestChecker</param>
        /// <param name="currentResult">Usually .latestResult</param>
        public void ProvideFullCellData(FieldCell cellRef, CheckerField3D parentChecker, PlannerResult currentResult)
        {
            CellData = new Data(cellRef, parentChecker, currentResult);
        }

        public override object DefaultValue { get { return CellData; } }

        public override Color GetColor()
        {
            return new Color(0.6f, 0.85f, .0f, 1f);
        }


        public Data GetAnyData()
        {
            var first = FirstNoSender();
            if (first != null)
            {
                var tPort = first.PortReference;
                if (tPort != null)
                {
                    if ( tPort is PGGPlannerPort)
                    {
                        PGGPlannerPort plPrt = tPort as PGGPlannerPort;
                        var shpe = plPrt.shape;
                        if( shpe != null)
                        {
                            Data nData = new Data(null, shpe, null);
                            return nData;
                        }

                    }

                    Data? dt = GetDataFromPort(FirstNoSender().PortReference);
                    if (dt != null) { return dt.Value; }
                }
            }

            return new Data(null, null, null);
        }

        public Data? GetDataFromPort(IFGraphPort conn)
        {
            if (conn is PGGCellPort)
            {
                PGGCellPort prt = conn as PGGCellPort;
                return prt.CellData;
            }

            if (conn is PGGPlannerPort)
            {
                PGGPlannerPort prt = conn as PGGPlannerPort;
                
                if (prt.HasShape)
                {
                    Data nData = new Data();
                    nData.ParentChecker = prt.shape;
                    return nData;
                }
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

        public FieldCell GetInputCellValue
        {
            get
            {
                if (PortState() == EPortPinState.Empty) { /*Debug.Log("empty");*/ return null; }

                var conn = FirstNoSender().PortReference;

                Data? d = GetDataFromPort(conn);
                if (d == null) { /*Debug.Log("d null");*/ return null; }

                return d.Value.CellRef;
            }
        }

        public CheckerField3D GetInputCheckerValue
        {
            get
            {
                if (PortState() == EPortPinState.Empty) return null;

                var conn = FirstNoSender().PortReference;

                Data? d = GetDataFromPort(conn);
                if (d == null) return null;
                return d.Value.ParentChecker;
            }
        }

        public PlannerResult GetInputResultValue
        {
            get
            {
                if (PortState() == EPortPinState.Empty) return null;
                var conn = FirstNoSender().PortReference;

                Data? d = GetDataFromPort(conn);
                if (d == null) return null;
                return d.Value.ParentResult;
            }
        }

        public override object GetPortValueCall(bool onReadPortCall = true)
        {
            var val = base.GetPortValueCall(onReadPortCall);

            if (val == null) return val;

            if (val.GetType() == typeof(Data)) CellData = (Data)val;
            else CellData = new Data();

            if ( FGenerators.CheckIfIsNull( CellData.CellRef) ) return null; // If no data then return null value

            return val;
        }

        public override bool CanConnectWith(IFGraphPort toPort)
        {
            if (toPort is PGGCellPort) return true;
            if (toPort is BoolPort) return true;
            if (toPort is PGGVector3Port) return true;
            if (toPort.IsSender) return true;
            if (toPort.IsUniversal) return true;
            if (toPort.GetPortValueType == GetPortValueType) return true;
            if (toPort.IsOutput == false)
            { if (toPort.GetType() == typeof(PGGPlannerPort)) return true; }

            return false;
        }

        public override bool AllowConnectionWithValueType(IFGraphPort other)
        {
            if (other is PGGCellPort) return true;
            if (other.IsUniversal) return true;
            if (other.IsSender) return true;
            if (other.GetPortValueType == GetPortValueType) return true;
            if (other.IsOutput == false)
            { if (other.GetType() == typeof(PGGPlannerPort)) return true; }

            return false;

        }

        internal void Clear()
        {
            CellData = new Data(null, null, null);
        }

        public override bool OnClicked(Event e)
        {
            bool baseClick = base.OnClicked(e);
            if (baseClick) return true;

            FieldCell cl = GetInputCellValue;
            if (FGenerators.CheckIfExist_NOTNULL(cl))
            {
                if (FGenerators.CheckIfExist_NOTNULL(GetInputCheckerValue))
                {
                    GetInputCheckerValue.DebugLogDrawCellInWorldSpace(cl, Color.green, 0.25f);
#if UNITY_EDITOR
                    UnityEditor.SceneView.RepaintAll();
#endif
                }
            }

            return false;

        }
    }
}