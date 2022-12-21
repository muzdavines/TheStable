using FIMSpace.Generating;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Graph
{
    public abstract partial class FGraph_NodeBase
    {

        #region Get Ports

        public List<IFGraphPort> GetPorts(bool input) { if (input) return GetInputPorts(); else return GetOutputPorts(); }
        public IFGraphPort GetPort(bool input, int index) { if (input) return GetInputPort(index); else return GetOutputPort(index); }
        /// <summary> !!! Override only if you really need to !!! </summary>
        public virtual List<IFGraphPort> GetInputPorts() { return inputPorts; }
        public IFGraphPort GetInputPort(int index) { return FGenerators.GetListElementOrNull(GetInputPorts(), index); }

        [SerializeField, HideInInspector/*, SerializeReference*/]
        protected List<IFGraphPort> outputPorts = new List<IFGraphPort>();
        [SerializeField, HideInInspector/*, SerializeReference*/]
        protected List<IFGraphPort> inputPorts = new List<IFGraphPort>();

        /// <summary> !!! Override only if you really need to !!! </summary>
        public virtual List<IFGraphPort> GetOutputPorts() { return outputPorts; }
        public IFGraphPort GetOutputPort(int index) { return FGenerators.GetListElementOrNull(GetOutputPorts(), index); }

        #endregion

        /// <summary> [Base is empty] Called once, when some port starts to be read </summary>
        public virtual void OnStartReadingNode() { }
        /// <summary> Called when any port starts to be read. If you want to call node operations, use 'OnStartReadingNode' instead </summary>
        public virtual void DONT_USE_IT_YET_OnReadPort(IFGraphPort port) { }
        public int GetPortIndex(IFGraphPort toPort, bool inputPort = true)
        {
            List<IFGraphPort> ports;

            int outAdd = 0;
            //if (GetInputPorts().Count == 0 && GetOutputPorts().Count == 0) { UnityEngine.Debug.Log("no ports!"); }
            //if (GetInputPorts().Count == 0 && GetOutputPorts().Count == 0) { UnityEngine.Debug.Log("no ports2!"); }
            if (inputPort) ports = GetInputPorts(); else { ports = GetOutputPorts();/* outAdd = 1000;*/ }

            if (ports == null) return -1;

            for (int i = 0; i < ports.Count; i++) if (ports[i] == toPort) return i + outAdd;

            return -1;
        }


        public void ClearPorts()
        {
            var inputPorts = GetInputPorts();
            if (inputPorts != null)
            {
                for (int i = 0; i < inputPorts.Count; i++)
                {
                    if (inputPorts[i] == null) continue;
                    for (int c = 0; c < inputPorts[i].Connections.Count; c++)
                        inputPorts[i].Connections[c].Clear();
                    inputPorts[i].Connections.Clear();
                }

                inputPorts.Clear();
            }

            var outputPorts = GetOutputPorts();
            if (outputPorts != null)
            {
                for (int i = 0; i < outputPorts.Count; i++)
                {
                    if (outputPorts[i] == null) continue;
                    for (int c = 0; c < outputPorts[i].Connections.Count; c++)
                        outputPorts[i].Connections[c].Clear();
                    outputPorts[i].Connections.Clear();
                }

                outputPorts.Clear();
            }
        }

        public void RefreshPort(IFGraphPort port)
        {
            NodePortBase portB = port as NodePortBase;
            if (FGenerators.CheckIfExist_NOTNULL(portB))
                portB.CallFromParentNode(this);
        }

        public static bool RequestsConnectionsRefresh = false;
        public void PortConnectionRequestsRefresh(NodePortBase port)
        {
            port.Refresh(this);
            RequestsConnectionsRefresh = true;
        }

        public virtual bool IsConnectedWith(NodePortBase otherPort)
        {
            if (otherPort.IsOutput)
            {
                return IsAnyInputConnectedWith(otherPort);
            }
            else
            {
                return IsAnyOutputConnectedWith(otherPort);
            }
        }

        public virtual bool IsAnyOutputConnectedWith(NodePortBase otherPort)
        {
            var outPorts = GetOutputPorts();
            if (outPorts == null) return false;
            for (int i = 0; i < outPorts.Count; i++)
            {
                NodePortBase port = outPorts[i] as NodePortBase;
                if (port == null) continue;
                if (port.IsConnectedWith(otherPort)) return true;
            }
            return false;
        }

        public virtual bool IsAnyInputConnectedWith(NodePortBase otherPort)
        {
            var inPorts = GetInputPorts();
            if (inPorts == null) return false;
            for (int i = 0; i < inPorts.Count; i++)
            {
                NodePortBase port = inPorts[i] as NodePortBase;
                if (port == null) continue;
                if (port.IsConnectedWith(otherPort)) return true;
            }
            return false;
        }
    }
}
