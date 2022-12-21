using FIMSpace.Generating;
using FIMSpace.Generating.Checker;
using FIMSpace.Generating.Planning;
using FIMSpace.Generating.Planning.PlannerNodes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Graph
{
    [System.Serializable]
    public class PGGPlannerPort : NodePortBase
    {
        public int UniquePlannerID = -1;
        public int DuplicatePlannerID = -1;
        public bool DisplayVariableName = true;


        // Grouped planners
        public override System.Type GetPortValueType { get { return typeof(int); } }
        public List<Generating.Planning.FieldPlanner> PlannersList { get { return _plannersList; } }
        private List<Generating.Planning.FieldPlanner> _plannersList = null;
        public bool ContainsMultiplePlanners { get { if (_plannersList == null) return false; if (_plannersList.Count == 0) return false; return true; } }
        public void AssignPlannersList(List<Generating.Planning.FieldPlanner> plans) { _plannersList = plans; }

        // Checkers
        private CheckerField3D containedShape = null;
        public CheckerField3D shape
        {
            get
            {

                if (!IsOutput)
                    if (PortState() == EPortPinState.Connected)
                    {
                        var port = FirstConnectedPortOfType(typeof(PGGPlannerPort));
                        if (port != null)
                        {
                            var plPrt = port as PGGPlannerPort;
                            if (plPrt.HasShape) return plPrt.shape;
                        }
                        else
                        {
                            var cellPrt = FirstConnectedPortOfType(typeof(PGGCellPort));
                            if (cellPrt != null) return GetCheckerFromCellPort(cellPrt);
                        }
                    }

                return containedShape;
            }
        }

        public bool HasShape { get { return FGenerators.CheckIfExist_NOTNULL(shape); } }
        public bool JustCheckerContainer = false;

        internal void Clear()
        {
            containedShape = null;
            SetIDsOfPlanner(null);
        }


        /// <summary>
        /// Planner Index (-1 if use self) and Duplicate Index (in most cases just zero)
        /// </summary>
        public override object DefaultValue
        {
            get
            {
                if (JustCheckerContainer) return containedShape;
                //return GetPortInputValue();
                return new Vector2Int(UniquePlannerID, DuplicatePlannerID);
            }
        }

        /// <summary>
        /// Returning connected values or default value if nothing connected
        /// </summary>
        //public object GetPortInputValue()
        //{
        //    if ( PortState() != EPortPinState.Connected) return new Vector2Int(UniquePlannerID, DuplicatePlannerID);

        //}

        public void RefreshInputCall()
        {
            if (BaseConnection != null)
            {
                if (BaseConnection.PortReference != null)
                    ReadValue(BaseConnection.PortReference.GetPortValue);
            }
        }

        /// <summary>
        ///  -1 is use self else use index in field planner
        /// </summary>
        public int GetPlannerIndex()
        {
            if (UniquePlannerID < -1) return -1;
            if (PortState() != EPortPinState.Connected) return -1;


            //object pVal = GetPortValueSafe;
            //if ( pVal != null)
            //{
            //    if ( pVal is Vector2)
            //    {
            //        int plInd = Mathf.RoundToInt(((Vector2)pVal).x);
            //        if (plInd < 0) plInd = -1;
            //        return plInd;
            //    }
            //}


            return UniquePlannerID;
        }

        CheckerField3D GetCheckerFromCellPort(IFGraphPort cellPrt)
        {
            var plPrt = cellPrt as PGGCellPort;
            var cData = plPrt.CellData;

            if (FGenerators.NotNull(cData.CellRef))
            {
                if (cData.ParentChecker != null)
                {
                    CheckerField3D ch = new CheckerField3D();
                    ch.CopyParamsFrom(cData.ParentChecker);
                    ch.AddLocal(cData.CellRef.Pos);
                    return ch;
                }
            }

            return null;
        }

        internal void ProvideShape(CheckerField3D newChecker, Vector3? extraOffset = null)
        {
            containedShape = newChecker;
        }

        public object AcquireObjectReferenceFromInput()
        {
            for (int c = 0; c < Connections.Count; c++)
            {
                var conn = Connections[c];
                var value = conn.PortReference.GetPortValue;
                if (value != null) return value;
            }

            return null;
        }

        public CheckerField3D GetInputCheckerSafe
        {
            get
            {
                FieldPlanner planner;



                if (PortState() == EPortPinState.Empty)
                {
                    planner = GetPlannerFromPort(false);
                    if (planner) return planner.LatestChecker;
                }
                else
                {


                    if (BaseConnection != null) if (BaseConnection.PortReference != null)
                        {
                            if (BaseConnection.PortReference is PGGPlannerPort)
                            {
                                PGGPlannerPort pport = BaseConnection.PortReference as PGGPlannerPort;
                                if (pport.HasShape) return pport.shape;
                            }
                            else if (BaseConnection.PortReference is PGGCellPort)
                            {
                                PGGCellPort pport = BaseConnection.PortReference as PGGCellPort;
                                var cellChecker = GetCheckerFromCellPort(pport);
                                if (cellChecker != null) return cellChecker;
                            }
                        }

                    if (containedShape != null) return containedShape;
                }

                object val = GetPortValueSafe;

                if (val is PGGCellPort.Data)
                {
                    PGGCellPort.Data data = (PGGCellPort.Data)val;
                    if (FGenerators.CheckIfExist_NOTNULL(data.CellRef))
                        if (FGenerators.CheckIfExist_NOTNULL(data.ParentChecker))
                            return data.ParentChecker;
                }

                planner = GetPlannerFromPort(false);
                if (planner) return planner.LatestChecker;

                return null;
            }
        }

        public FieldPlanner GetPlannerFromPort(bool callRead = true)
        {
            if (callRead) GetPortValueCall();

            int plannerId = GetPlannerIndex();
            int duplicateId = GetPlannerDuplicateIndex();

            if (Connections.Count == 0)
            {
                if (UniquePlannerID > -1)
                {
                    return PlannerRuleBase.GetFieldPlannerByID(UniquePlannerID, DuplicatePlannerID);
                }
            }
            else
            {
                if (BaseConnection.PortReference != null)
                {
                    PGGCellPort cellPrt = BaseConnection.PortReference as PGGCellPort;

                    if (cellPrt != null)
                    {
                        if (cellPrt.GetInputResultValue != null)
                        {
                            return cellPrt.GetInputResultValue.ParentFieldPlanner;
                        }
                    }
                    else
                    {
                        PGGPlannerPort fPort = BaseConnection.PortReference as PGGPlannerPort;
                        if (fPort != null)
                        {
                            return PlannerRuleBase.GetFieldPlannerByID(fPort.UniquePlannerID, fPort.DuplicatePlannerID);
                        }
                    }
                }
            }

            return PlannerRuleBase.GetFieldPlannerByID(plannerId, duplicateId);
        }

        /// <summary>
        /// Index of duplicated planner field
        /// </summary>
        public int GetPlannerDuplicateIndex()
        {

            //object pVal = GetPortValueSafe;
            //if (pVal != null)
            //{
            //    if (pVal is Vector2)
            //    {
            //        int plInd = Mathf.RoundToInt(((Vector2)pVal).y);
            //        if (plInd < 0) plInd = -1;
            //        return plInd;
            //    }
            //}


            if (DuplicatePlannerID < 0) return -1;

            return DuplicatePlannerID;
        }

        public override Color GetColor()
        {
            return new Color(0.9f, 0.7f, .3f, 1f);
        }

        public override bool AllowConnectionWithValueType(IFGraphPort other)
        {
            if (FGenerators.CheckIfIsNull(other)) return false;
            if (FGenerators.CheckIfIsNull(other.GetPortValue)) return false;
            if (other.GetPortValue.GetType() == typeof(int)) return true;
            if (other.GetPortValue.GetType() == typeof(float)) return true;
            if (other.GetPortValue.GetType() == typeof(Vector2)) return true;
            if (other.GetPortValue.GetType() == typeof(Vector2Int)) return true;
            if (other.GetPortValue.GetType() == typeof(Vector3)) return true;
            if (other.GetPortValue.GetType() == typeof(Vector3Int)) return true;
            return base.AllowConnectionWithValueType(other);
        }

        public override bool CanConnectWith(IFGraphPort toPort)
        {
            if (toPort is PGGCellPort) return true;
            return base.CanConnectWith(toPort);
        }

        public override object GetPortValueCall(bool onReadPortCall = true)
        {
            var val = base.GetPortValueCall(onReadPortCall);

            if (val == null) return val;

            ReadValue(val);

            return val;
        }

        void ReadValue(object val)
        {
            if (val.GetType() == typeof(int)) UniquePlannerID = (int)val;
            else
            if (val.GetType() == typeof(float)) UniquePlannerID = Mathf.RoundToInt((float)val);
            else
            if (val.GetType() == typeof(Vector2))
            {
                Vector2 v2 = (Vector2)val;
                UniquePlannerID = Mathf.RoundToInt(v2.x);
                DuplicatePlannerID = Mathf.RoundToInt(v2.y);
            }
            else if (val.GetType() == typeof(Vector2Int))
            {
                Vector2Int v2 = (Vector2Int)val;
                UniquePlannerID = (v2.x);
                DuplicatePlannerID = (v2.y);
            }
            else if (val.GetType() == typeof(Vector3))
            {
                Vector3 v2 = (Vector3)val;
                UniquePlannerID = Mathf.RoundToInt(v2.x);
                DuplicatePlannerID = Mathf.RoundToInt(v2.y);
            }
            else if (val.GetType() == typeof(Vector3Int))
            {
                Vector3Int v2 = (Vector3Int)val;
                UniquePlannerID = (v2.x);
                DuplicatePlannerID = (v2.y);
            }
            else if (val.GetType() == typeof(PGGCellPort.Data))
            {
                PGGCellPort.Data dt = (PGGCellPort.Data)val;

                if (dt.ParentResult != null)
                {
                    if (dt.ParentResult.ParentFieldPlanner)
                    {
                        SetIDsOfPlanner(dt.ParentResult.ParentFieldPlanner);
                    }
                }
            }
        }

        internal void SetIDsOfPlanner(FieldPlanner planner)
        {
            if (planner == null)
            {
                UniquePlannerID = -1;
                DuplicatePlannerID = -1;
                return;
            }

            UniquePlannerID = planner.IndexOnPreset;
            DuplicatePlannerID = planner.IndexOfDuplicate;
        }
    }
}