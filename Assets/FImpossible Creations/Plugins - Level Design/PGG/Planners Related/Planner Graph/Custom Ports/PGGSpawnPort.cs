using FIMSpace.Generating;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Graph
{
    [System.Serializable]
    public class PGGSpawnPort : NodePortBase
    {
        private List<SpawnData> containedSpawns = new List<SpawnData>();
        public override System.Type GetPortValueType { get { return typeof(SpawnData); } }

        public SpawnData FirstSpawnForOutputPort
        {
            get { if (containedSpawns.Count > 0) return containedSpawns[0]; else return null; }
            set { if (value == null) { containedSpawns.Clear(); return; } if (containedSpawns.Count > 0) containedSpawns[0] = value; else containedSpawns.Add(value); }
        }

        public override object DefaultValue { get { return FirstSpawnForOutputPort; } }

        public override Color GetColor()
        {
            return new Color(0.2f, 0.55f, .95f, 1f);
        }


        public SpawnData GetInputCellValue
        {
            get
            {
                return GetFirstConnectedSpawn;
            }
        }

        public bool ContainsMultipleSpawns { get { return containedSpawns.Count > 1; } }
        public List<SpawnData> GetLocalSpawnsList { get { return containedSpawns; } }
        public SpawnData GetFirstConnectedSpawn { get { PGGSpawnPort conn = GetConnectedSpawnPort; if (conn != null) return conn.FirstSpawnForOutputPort; return FirstSpawnForOutputPort; } }
        public List<SpawnData> GetConnectedSpawnsList { get { PGGSpawnPort conn = GetConnectedSpawnPort; if (conn != null) return conn.containedSpawns; return containedSpawns; } }

        public PGGSpawnPort GetConnectedSpawnPort
        {
            get
            {
                for (int c = 0; c < Connections.Count; c++)
                {
                    var conn = Connections[c].PortReference;
                    if (conn is PGGSpawnPort) return conn as PGGSpawnPort;
                }

                return null;
            }
        }

        public SpawnData GetDataFromPort(IFGraphPort conn)
        {
            if (conn is PGGSpawnPort)
            {
                PGGSpawnPort prt = conn as PGGSpawnPort;
                return prt.FirstSpawnForOutputPort;
            }

            if (conn is NodePortBase)
            {
                NodePortBase np = conn as NodePortBase;
                if (np.GetPortValueSafe is SpawnData)
                {
                    return (SpawnData)np.GetPortValueSafe;
                }
            }

            return null;
        }

        internal void ApplySpawnsGroup(List<SpawnData> spawns)
        {
            containedSpawns = spawns;
        }

        public override object GetPortValueCall(bool onReadPortCall = true)
        {
            var val = base.GetPortValueCall(onReadPortCall);

            if (val == null) return val;

            if (val.GetType() == typeof(SpawnData)) FirstSpawnForOutputPort = (SpawnData)val;
            else FirstSpawnForOutputPort = null;

            if (FGenerators.CheckIfIsNull(FirstSpawnForOutputPort)) return null; // If no data then return null value

            return val;
        }

        public override bool CanConnectWith(IFGraphPort toPort)
        {
            if (toPort is PGGSpawnPort) return true;
            if (toPort.IsSender) return true;
            if (toPort.IsUniversal) return true;
            if (toPort.GetPortValueType == GetPortValueType) return true;

            return false;
        }

        public override bool AllowConnectionWithValueType(IFGraphPort other)
        {
            if (other is PGGSpawnPort) return true;
            if (other.IsUniversal) return true;
            if (other.IsSender) return true;
            if (other.GetPortValueType == GetPortValueType) return true;

            return false;

        }

        internal void Clear()
        {
            containedSpawns.Clear();
        }
    }
}