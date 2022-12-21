using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Graph
{
    public enum EPortPinType { Input, Output }
    public enum EPortNameDisplay { Default, HideName, HideOnConnected }
    public enum EPortValueDisplay { Default, AlwaysEditable, NotEditable, HideValue, HideOnConnected }
    public enum EPortSlotDisplay { Default, HidePort }
    public enum EPortPinState { Unknown, Empty, Connected }

    /// <summary>
    /// Variable port interface for handling node ports
    /// </summary>
    public interface IFGraphPort
    {
        bool IsOutput { get; }
        bool IsSender { get; }
        Rect GetPortRect { get; }
        Rect PortClickAreaRect { get; set; }
        PortConnection BaseConnection { get; }
        List<PortConnection> Connections { get; }
        /// <summary> Port connection value if connected, if not connected then using constant assigned value </summary>
        object GetPortValue { get; }
        System.Type GetPortValueType { get; }
        Color PortColor { get; }
        void RefreshPortConnections<T>(List<T> allNodes) where T : FGraph_NodeBase;
        bool CanConnectWith(IFGraphPort toPort);
        bool IsUniversal { get; }
        EPortPinState PortState();
    }

    /// <summary> Thanks to this ports are working on the build </summary>
    [SerializeField]
    public class PortHandler
    {
        FGraph_NodeBase parentNode;
        public int ParentNodeID = -1;
        public int PortID = -1;
        public EPortPinType PortType;
        public int _HelperFunctionsID;
        //private string ClassType = "";

        public PortHandler(NodePortBase port, FGraph_NodeBase node)
        {
            parentNode = node;
            ParentNodeID = port.ParentNodeID;
            PortID = port.PortID;
            PortType = port.PortType;
            _HelperFunctionsID = port._HelperFunctionsID;
        }

    }


}
