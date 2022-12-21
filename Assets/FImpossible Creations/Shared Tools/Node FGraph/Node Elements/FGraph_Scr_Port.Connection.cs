using FIMSpace.Graph;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Graph
{

    /// <summary>
    /// Helper variables port connection class
    /// </summary>
    [System.Serializable]
    public class PortConnection
    {
        /// <summary> Individual node ID </summary>
        public int ConnectedNodeID = -1;
        /// <summary> Index of port in connected parent ID </summary>
        public int ConnectedNodePortID = -1;
        /// <summary> Reference refreshed usign ConnectedNodeID value </summary>
        public FGraph_NodeBase NodeReference = null;
        /// <summary> Reference refreshed usign ConnectedNodePortID value </summary>
        public IFGraphPort PortReference = null;

        public bool ConnectedWithSomething { get { if (ConnectedNodeID == -1) return false; if (ConnectedNodePortID == -1) return false; return true; } }
        [SerializeField] bool isInput = false;
        public bool IsInput { get { return isInput; } private set { isInput = value; } }

        public bool WasReloaded { get; private set; } = false;

        public PortConnection(FGraph_NodeBase to, IFGraphPort toPort, bool isInput)
        {
            NodeReference = to;
            ConnectedNodeID = to.IndividualID;
            PortReference = toPort;
            IsInput = isInput;
            ConnectedNodePortID = to.GetPortIndex(toPort, isInput);
            WasReloaded = false;
        }


        public void RefreshReferences<T>(List<T> allGraphNodes) where T : FGraph_NodeBase
        {
            WasReloaded = true;
            NodeReference = null; // Reset values before refreshing
            PortReference = null;

            if (ConnectedNodeID == -1) { return; } // Not connected number
            if (ConnectedNodePortID == -1) { return; } // Not connected number

            // Refresh for outputs
            for (int i = 0; i < allGraphNodes.Count; i++)
            {
                var node = allGraphNodes[i];
                if (node.IndividualID != ConnectedNodeID) continue; // Searching for node with desired id
                NodeReference = node;
                var oPorts = node.GetPorts(IsInput);
                //if (oPorts == null) oPorts = node.GetOutputPorts();
                if (oPorts == null) { return; } // We trying connect to node which is not supporting input ports?
                if (ConnectedNodePortID >= oPorts.Count) { /*UnityEngine.Debug.Log("count null");*/ return; }// Connection port index is out of target ports list bounds
                PortReference = oPorts[ConnectedNodePortID];
            }

        }

        public void Clear()
        {
            NodeReference = null; ConnectedNodeID = -1;
            PortReference = null; ConnectedNodePortID = -1;
        }
    }


}
