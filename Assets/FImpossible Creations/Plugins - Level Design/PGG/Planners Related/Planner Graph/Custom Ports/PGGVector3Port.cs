using FIMSpace.Generating;
using System;
using UnityEngine;

namespace FIMSpace.Graph
{
    [System.Serializable]
    public class PGGVector3Port : NodePortBase
    {
        public override System.Type GetPortValueType { get { return typeof(Vector3); } }
        /// <summary> Default value of port -> if output then it's output value </summary>
        public Vector3 Value = Vector3.zero;

        /// <summary>
        /// If null then returning zero
        /// </summary>
        public Vector3 GetInputValue
        {
            get
            {
                object val = GetPortValueSafe;

                if (val is PGGCellPort.Data)
                {
                    PGGCellPort.Data data = (PGGCellPort.Data)val;
                    if (FGenerators.CheckIfExist_NOTNULL(data.CellRef))
                        if (FGenerators.CheckIfExist_NOTNULL(data.ParentChecker))
                            return data.ParentChecker.GetWorldPos(data.CellRef);
                }

                if (FGenerators.CheckIfIsNull(val)) return Vector3.zero;
                if (val is Vector3) return (Vector3)val;
                if (val is Vector3Int) return (Vector3Int)val;
                if (val is Vector2) { Vector2 v2 = (Vector2)(val); return new Vector3(v2.x, v2.y, 0); }
                if (val is Vector2Int) { Vector2Int v2 = (Vector2Int)(val); return new Vector3(v2.x, v2.y, 0); }
                if (val is Single) return new Vector3((float)val, 0, 0);
                return Vector3.zero;
            }
        }

        public override object DefaultValue { get { return Value; } }

        public bool ConnectingWithPlannerPort { get { if (Connections.Count > 0) if (BaseConnection.PortReference is PGGPlannerPort) return true; return false; } }
        public Generating.Planning.FieldPlanner GetConnectingPlanner
        {
            get
            {
                if (ConnectingWithPlannerPort == false) return null;
                Generating.Planning.FieldPlanner planner;
                planner = Generating.Planning.PlannerNodes.PlannerRuleBase.GetPlannerFromPortS(BaseConnection.PortReference as PGGPlannerPort, false);
                return planner;
            }
        }


        public override bool AllowConnectionWithValueType(IFGraphPort other)
        {
            bool allow = base.AllowConnectionWithValueType(other);

            if (!allow)
            {
                if (AdditionalAllows != null)
                {
                    if (AdditionalAllows.Length == 1 && AdditionalAllows[0] == typeof(int))
                    {
                        object ov = other.GetPortValue;
                        System.Type ot = ov.GetType();
                        if (ot == typeof(int)) return true;
                        if (ot == typeof(float)) return true;
                        if (ot == typeof(Vector2)) return true;
                    }
                }
            }

            return allow;
        }

        public override Color GetColor()
        {
            return new Color(0.2f, 0.8f, .5f, 1f);
        }

        public override void InitialValueRefresh(object initialValue)
        {
            if (initValueSet) return;
            base.InitialValueRefresh(initialValue);
            if (initialValue is Vector3) Value = (Vector3)initialValue;
        }
    }
}