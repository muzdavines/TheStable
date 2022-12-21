using FIMSpace.Generating;
using System;
using UnityEngine;

namespace FIMSpace.Graph
{
    [System.Serializable]
    public class PGGUniversalPort : NodePortBase
    {
        public FieldVariable Variable = new FieldVariable();

        public override System.Type GetPortValueType { get { return typeof(System.Single); } }
        public override object DefaultValue { get { return Variable.GetValue(); } }
        protected override bool setAsUniversal { get { return true; } }

#if UNITY_EDITOR
        public override bool AllowConnectionWithValueType(IFGraphPort other)
        {
            if (FieldVariable.SupportingType(other.GetPortValue))
            {
                return true;
            }
            else
            {
                return base.AllowConnectionWithValueType(other);
            }
        }
#endif

        public override void TriggerReadPort(bool callRead = false)
        {

            Variable.SetTemporaryReference(false);

            // Support for cell and planenr variables
            var conn = FirstNoSender();
            if (FGenerators.CheckIfExist_NOTNULL(conn))
            {
                if (FGenerators.CheckIfExist_NOTNULL(conn.PortReference))
                {
                    if (conn.PortReference is PGGCellPort || conn.PortReference is PGGPlannerPort)
                    {
                        Variable.SetTemporaryReference(true, conn.PortReference);
                    }
                }
            }

            base.TriggerReadPort(callRead);
        }

        internal IFGraphPort GetConnectedPortOfType(Type type)
        {
            if (Connections == null) return null;

            for (int c = 0; c < Connections.Count; c++)
            {
                var conn = Connections[c];
                if (conn == null) continue;
                if (conn.PortReference == null) continue;
                if (conn.PortReference.GetType() == type) return conn.PortReference;
            }

            return null;
        }

        public override Color GetColor()
        {
            switch (Variable.ValueType)
            {
                case FieldVariable.EVarType.Number:
                    if (Variable.FloatSwitch == FieldVariable.EVarFloatingSwitch.Float)
                        return new Color(0.4f, 0.4f, .9f, 1f);
                    else
                        return new Color(0.2f, 0.6f, .9f, 1f);

                case FieldVariable.EVarType.Bool:
                    return new Color(0.9f, 0.4f, .4f, 1f);

                case FieldVariable.EVarType.Vector3:
                case FieldVariable.EVarType.Vector2:
                    return new Color(0.2f, 0.8f, .5f, 1f);
            }

            return new Color(0.4f, 0.4f, .5f, 1f);
        }
    }
}