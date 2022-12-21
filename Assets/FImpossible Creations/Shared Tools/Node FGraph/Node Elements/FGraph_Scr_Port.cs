using FIMSpace.Generating;
using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Graph
{

    [System.Serializable]
    public class NodePortBase : IFGraphPort
    {
        public int ParentNodeID = -1;
        public int PortID = -1;
        public FGraph_NodeBase ParentNode;// { get; private set; } its scriptable object - can be public
        public Rect _E_LatestCorrectPortRect;

        public EPortPinType PortType = EPortPinType.Output;
        public EPortNameDisplay NameDisplayMode = EPortNameDisplay.Default;
        public EPortValueDisplay ValueDisplayMode = EPortValueDisplay.Default;
        public EPortSlotDisplay SlotMode = EPortSlotDisplay.Default;

        public string DisplayName = "Port";
        public string OverwriteName = "";
        [System.NonSerialized] public bool AllowDragWire = true;
        [System.NonSerialized] public Vector2 _EditorCustomOffset = Vector2.zero;
        [System.NonSerialized] public bool _EditorForceDrawDefaultWires = false;
        public int _HelperFunctionsID = -1;

        public enum EPortConnectionDisplayMode
        {
            Default, Ghosting, Curled, Hidden
        }

        public EPortConnectionDisplayMode ConnectionDisplay = EPortConnectionDisplayMode.Default;


        /// <summary> MUST BE OVERRIDED </summary>
        public virtual Type GetPortValueType { get { return null; } }

        public void RemoveConnectionAt(int index)
        {
            //UnityEngine.Debug.Log("r");
            portConnections.RemoveAt(index);

            if (ParentNode)
            {
                ParentNode._E_SetDirty();
                ParentNode.OnPortConnectionsChanged();
            }
        }

        public virtual void TriggerReadPort(bool callRead = false)
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                NodePortBase port = Connections[i].PortReference as NodePortBase;
                if (port != null)
                {
                    // Only connected output can trigger further reading
                    if (port.IsOutput) port.ParentNode.OnStartReadingNode();
                }
            }

            if (callRead)
            {
                // Only output can trigger further reading
                if (IsOutput) GetPortValueCall();
            }
        }

        #region Interface

        public bool IsOutput { get { return PortType == EPortPinType.Output; } }
        public bool IsInput { get { return PortType == EPortPinType.Input; } }
        public bool IsSendingSignals = false;
        public bool IsSender { get { return IsSendingSignals; } }
        public Rect GetPortRect { get { return _E_LatestPortRect; } }

        public virtual EPortPinState PortState()
        {
            if (BaseConnection == null)
            {
                return EPortPinState.Empty;
            }

            if (BaseConnection.ConnectedNodeID != -1 && BaseConnection.ConnectedNodeID != -1)
            {
                return EPortPinState.Connected;
            }

            return EPortPinState.Empty;
        }

        public bool IsConnected { get { return PortState() == EPortPinState.Connected; } }
        public bool IsNotConnected { get { return PortState() != EPortPinState.Connected; } }

        public virtual void InitialValueRefresh(object initialValue)
        {
            if (initValueSet) return;
            // Set initialvalue to the port value
            initValueSet = true;
        }


        /// <summary> For custom implementation of connections allow, from output to input check </summary>
        public virtual bool AllowConnectionWithValueType(IFGraphPort other)
        {
            if (other.IsUniversal || IsUniversal) return true;

            if (AdditionalAllows != null)
            {
                #region Additional allows provided by [Port]

                for (int a = 0; a < AdditionalAllows.Length; a++)
                {
                    Type allow = AdditionalAllows[a];

                    if (allow.IsSubclassOf(typeof(NodePortBase))) // Port type based
                    {
                        if (allow == other.GetType()) return true;
                    }
                    else // Port value type
                    {
                        if (allow == other.GetPortValueType) return true;
                    }
                }

                #endregion
            }

            return false;
        }

        [NonSerialized] public System.Type[] AdditionalAllows = null;

        internal bool IsConnectedWith(FGraph_NodeBase node)
        {
            for (int c = 0; c < Connections.Count; c++)
            {
                var conn = Connections[c];
                if (conn == null) continue;
                if (conn.NodeReference == node) return true;
            }

            return false;
        }

        internal void RemoveConnectionsWith(FGraph_NodeBase node)
        {
            for (int i = Connections.Count - 1; i >= 0; i--)
            {
                var conn = Connections[i];
                if (conn == null) continue;
                if (conn.ConnectedNodeID == node.IndividualID) RemoveConnection(i);
            }
        }

        /// <summary> Returns null if no NodePortBase </summary>
        public static NodePortBase ToNodePortBase(IFGraphPort iFGraphPort)
        {
            if (FGenerators.CheckIfIsNull(iFGraphPort)) return null;
            if (iFGraphPort is NodePortBase) return iFGraphPort as NodePortBase;
            return null;
        }

        /// <summary> 0 is unlimited</summary>
        [NonSerialized] public int LimitInConnectionsCount = 0;

        /// <summary> From output to input connection check </summary>
        public virtual bool CanConnectWith(IFGraphPort toPort)
        {
            NodePortBase other = toPort as NodePortBase;

            if (other == null)
            {
                return false; // Trying connect with null 
            }
            //object ov = other.GetPortValue;
            //object v = GetPortValue;

            //if (ov == null) return false; // Null ref getted
            //if (v == null)
            //{
            //    return false; // Null ref getted
            //}
            System.Type mType = GetPortValueType;
            System.Type oType = other.GetPortValueType;

            if (other.GetPortValueType == GetPortValueType) return true; // Same type

            if (other.AllowConnectionWithValueType(this)) return true; // Custom allow

            if ((oType == typeof(float)) || (oType == typeof(int)))
            {
                if ((mType == typeof(float)) || (mType == typeof(int))) return true; // Int and float value types
            }

            return false;
        }


        /// <summary>
        /// DON'T use it inside  OnReadPort(), use GetPortValueCall(false) there
        /// Returning value of connected node or if not connected then returning constant assigned default value
        /// </summary>
        public object GetPortValue
        {
            get { return GetPortValueCall(true); }
        }

        /// <summary>
        /// Same as GetPortValue but without calling additional operations for triggering chain reaction in connections
        /// You can use it inside  OnReadPort()
        /// </summary>
        public object GetPortValueSafe
        {
            get { return GetPortValueCall(false); }
        }


        public virtual object GetPortValueCall(bool onReadPortCall = true)
        {
            if (IsOutput)
            {
                if (ParentNode != null) if (onReadPortCall) { ParentNode.DONT_USE_IT_YET_OnReadPort(this); }
                return DefaultValue;
            }
            else // Input
            {
                if (BaseConnection is null)
                {
                    if (ParentNode != null) if (onReadPortCall) ParentNode.DONT_USE_IT_YET_OnReadPort(this);
                    return DefaultValue;
                }
                else // Something is connecting into this input port!
                {
                    if (ParentNode != null)
                    {
                        if (onReadPortCall) ParentNode.DONT_USE_IT_YET_OnReadPort(this);
                    }

                    if (BaseConnection.PortReference == this)
                    {
                        //UnityEngine.Debug.Log("nyll");
                        return null;
                    }

                    if (Connections.Count == 1)
                    {
                        if (Connections[0].PortReference != null)
                            if (Connections[0].PortReference.IsSender)
                            {
                                return DefaultValue;
                            }
                    }

                    NodePortBase connPort = BaseConnection.PortReference as NodePortBase;

                    if (connPort != null)
                    {
                        if (connPort.ParentNode != null)
                        {
                            if (onReadPortCall) connPort.ParentNode.DONT_USE_IT_YET_OnReadPort(connPort);
                        }
                        else
                        { /*UnityEngine.Debug.Log("conn null!");*/ }
                    }

                    if (BaseConnection.PortReference == null)
                    {
                        ParentNode.PortConnectionRequestsRefresh(this);
                        if (BaseConnection.PortReference == null)
                        {
                            //UnityEngine.Debug.Log(" null3 " + DisplayName + "  :  " + ParentNode._E_GetDisplayName());
                            return null;
                        }
                    }

                    return BaseConnection.PortReference.GetPortValue;
                }
            }
        }

        /// <summary>
        /// Default port value used if port is not connected with any other port </summary>
        /// If it's output port then this is output value
        /// if it's input port then it's just default port value when not connected with other ports
        /// </summary>
        public virtual object DefaultValue { get; }


        public Color PortColor => GetColor();

        //[SerializeReference]
        [SerializeField] private List<PortConnection> portConnections = new List<PortConnection>();

        public PortConnection BaseConnection { get { if (portConnections.Count == 0) return null; else { return FirstNoSender(); } } }
        public PortConnection FirstNoSender()
        {
            if (Connections.Count == 1) return Connections[0];

            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i] == null) continue;
                if (Connections[i].PortReference == null) continue;
                if (Connections[i].PortReference.IsSender == false) return Connections[i];
            }

            if (Connections.Count == 0) return null;
            return Connections[0];
        }

        public IFGraphPort FirstConnectedPortOfType(System.Type t)
        {
            for (int c = 0; c < Connections.Count; c++)
            {
                if (Connections[c] == null) continue;
                if (Connections[c].PortReference == null) continue;
                if (Connections[c].PortReference.GetType() == t) return Connections[c].PortReference;
            }

            return null;
        }

        /// <summary> If nothing connected then returning self </summary>
        public NodePortBase TargetConnectionPort { get { if (portConnections.Count == 0) return this; else return portConnections[0].PortReference as NodePortBase; } }
        public List<PortConnection> Connections => portConnections;


        #endregion

        [SerializeField] protected bool initValueSet = false;
        [SerializeField] protected bool initialized = false;
        public bool WasInitialized { get { return initialized; } }

        Rect IFGraphPort.PortClickAreaRect
        {
            get { return AllowDragWire ? _E_LatestPortInterationRect : Rect.zero; }
            set { _E_LatestPortInterationRect = value; }
        }

        public bool IsUniversal => setAsUniversal;
        protected virtual bool setAsUniversal { get { return false; } }


        public virtual void Refresh(FGraph_NodeBase parentNode)
        {
            //parentNode.baseSerializedObject.Update();
            ParentNode = parentNode;
            ParentNodeID = parentNode.IndividualID;
            PortID = parentNode.GetPortIndex(this, !IsOutput);
            //parentNode.baseSerializedObject.ApplyModifiedProperties();
        }


        #region Serialization Utilities

        static void UpdateSerializedObject(NodePortBase myPort, NodePortBase otherPort = null)
        {
#if UNITY_EDITOR
            if (myPort != null) if (myPort.ParentNode) myPort.ParentNode.baseSerializedObject.Update();
            if (otherPort != null) if (otherPort.ParentNode != null) otherPort.ParentNode.baseSerializedObject.Update();
#endif
        }

        static void ApplySerializedObject(NodePortBase myPort, NodePortBase otherPort = null)
        {
#if UNITY_EDITOR
            if (myPort != null) if (myPort.ParentNode != null)
                {
                    myPort.ParentNode.baseSerializedObject.ApplyModifiedProperties();
                    myPort.ParentNode._E_SetDirty();
                }

            if (otherPort != null) if (otherPort.ParentNode != null)
                {
                    otherPort.ParentNode.baseSerializedObject.ApplyModifiedProperties();
                    otherPort.ParentNode._E_SetDirty();
                }
#endif
        }

        static void UpdateSerializedObject(FGraph_NodeBase a, FGraph_NodeBase b = null)
        {
#if UNITY_EDITOR
            if (a) a.baseSerializedObject.Update();
            if (b) b.baseSerializedObject.Update();
#endif
        }

        static void ApplySerializedObject(FGraph_NodeBase a, FGraph_NodeBase b = null)
        {
#if UNITY_EDITOR
            if (a)
            {
                a.baseSerializedObject.ApplyModifiedProperties();
                a._E_SetDirty();
            }

            if (b)
            {
                b.baseSerializedObject.ApplyModifiedProperties();
                b._E_SetDirty();
            }
#endif
        }

        #endregion



        #region Port Connections

        public void ConnectWith(FGraph_NodeBase from, IFGraphPort fromPort, FGraph_NodeBase to, IFGraphPort toPort)
        {
            if (from == to) return;
            if (fromPort == toPort) return;
            if (to == null) return;
            if (toPort == null) return;

            UpdateSerializedObject(from, to);

            if (IsConnectedWith(toPort))
            {
                NodePortBase toPortBase = toPort as NodePortBase;
                if (FGenerators.CheckIfExist_NOTNULL(toPortBase))
                {
                    if (toPortBase.IsConnectedWith(this) == false)
                    {
                        if (RemoveConnectionWith(toPort)) return;
                    }
                }
            }

            if (fromPort.CanConnectWith(toPort) == false) return;

            NodePortBase toPortB = toPort as NodePortBase;

            // Remove connections if limiting count
            if (toPortB.LimitInConnectionsCount > 0)
                if (toPortB.Connections.Count >= toPortB.LimitInConnectionsCount)
                {
                    //UnityEngine.Debug.Log("ke lim = " + toPortB.LimitInConnectionsCount + " >= to cnt " + toPortB.Connections.Count);
                    //UnityEngine.Debug.Log("GO from " + ParentNode._E_GetDisplayName() + " to " + toPortB.ParentNode._E_GetDisplayName());

                    for (int i = toPortB.Connections.Count - 1; i >= 0; i--)
                    {
                        if (toPortB.Connections[i].PortReference.IsSender) continue;
                        toPortB.RemoveConnection(i);
                    }
                }

            // Connect two ports with each other
            Connections.Add(new PortConnection(to, toPort, true));
            toPortB.Connections.Add(new PortConnection(from, fromPort, false));


            ApplySerializedObject(from, to);
            //UnityEngine.Debug.Log("pconn");
            from.OnStartReadingNode();
            to.OnStartReadingNode();

            if (ParentNode)
            {
                ParentNode._E_SetDirty();
                ParentNode.OnPortConnectionsChanged();
            }
        }


        public void DisconnectWith(FGraph_NodeBase otherParent, IFGraphPort disconnectWithPort)
        {
            if (disconnectWithPort == null) { return; }
            if (IsConnectedWith(disconnectWithPort) == false) { return; }

            NodePortBase port = disconnectWithPort as NodePortBase;
            if (port == null) { return; }

            for (int i = 0; i < Connections.Count; i++)
                if (Connections[i].ConnectedNodeID == otherParent.IndividualID)
                    if (Connections[i].PortReference == disconnectWithPort)
                    {
                        Connections[i].Clear();
                        RemoveConnectionAt(i);
                        break;
                    }
        }


        public void RemoveConnection(int index)
        {
            var conn = Connections[index];
            if (conn == null) return;

            UpdateSerializedObject(ParentNode);

            var other = conn.PortReference;
            for (int c = 0; c < other.Connections.Count; c++)
            {
                var oConn = other.Connections[c];

                if (oConn.PortReference == this)
                {
                    oConn.Clear();
                    NodePortBase oPort = other as NodePortBase;

                    if (oPort != null) oPort.RemoveConnectionAt(c);
                    else other.Connections.RemoveAt(c);

                    break;
                }
            }

            conn.Clear();
            RemoveConnectionAt(index);

            ApplySerializedObject(ParentNode, null);
        }


        //public void DisconnectWith(FGraph_NodeBase otherParent, int otherIndex, bool isInput)
        //{
        //    IFGraphPort disconnectWithPort = otherParent.GetPort(isInput, otherIndex);
        //    DisconnectWith(otherParent, disconnectWithPort);
        //}

        //internal void ClearUnconnected()
        //{
        //    for (int i = Connections.Count - 1; i >= 0; i--)
        //    {
        //        if (Connections[i] == null)
        //        {
        //            RemoveConnectionAt(i);
        //            continue;
        //        }

        //        if (!Connections[i].ConnectedWithSomething)
        //        {
        //            RemoveConnectionAt(i);
        //        }
        //    }
        //}

        public bool IsConnectedWith(IFGraphPort otherPort)
        {
            NodePortBase oprt = otherPort as NodePortBase;

            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].PortReference == otherPort)
                {
                    return true;
                }
                else
                {
                    if (oprt != null)
                    {
                        if (Connections[i].ConnectedNodePortID == oprt.PortID)
                            return true;
                    }
                }
            }

            return false;
        }

        public bool RemoveConnectionWith(IFGraphPort otherPort)
        {
#if UNITY_EDITOR
            NodePortBase oprt = otherPort as NodePortBase;

            if (ParentNode) ParentNode.baseSerializedObject.Update();
            if (oprt != null) oprt.ParentNode.baseSerializedObject.Update();

            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].PortReference == otherPort)
                {
                    RemoveConnection(i);
                    return true;
                }
                else
                {
                    if (oprt != null)
                        if (Connections[i].ConnectedNodeID == oprt.ParentNodeID)
                            if (Connections[i].ConnectedNodePortID == oprt.PortID)
                            {
                                RemoveConnection(i);
                                return true;
                            }
                }
            }
#endif

            return false;
        }

        private void CheckForEmptyConnections()
        {
            for (int i = portConnections.Count - 1; i >= 0; i--)
            {
                var port = portConnections[i];

                if (port == null)
                {
                    RemoveConnectionAt(i);
                    continue;
                }

                #region Debugging Backup

                //if (port.ConnectedNodeID != -1)
                //    if (port.ConnectedNodePortID != -1)
                //        if (port.PortReference != null)
                //        {
                //            NodePortBase nPort = port.PortReference as NodePortBase;
                //            if (FGenerators.CheckIfExist_NOTNULL(nPort))
                //            {
                //                if (nPort.ParentNode)
                //                {
                //                    if (nPort.ParentNode.IsConnectedWith(this) == false)
                //                    {
                //                        portConnections.RemoveAt(i);
                //                        UnityEngine.Debug.Log("re,ov");//
                //                        continue;
                //                    }
                //                }
                //            }
                //        }

                #endregion

                //if (port.ConnectedNodeID == -1 || port.ConnectedNodePortID == -1)
                if (port.WasReloaded)
                {
                    //if (port.NodeReference == null || port.ConnectedNodeID == -1 || port.ConnectedNodePortID == -1)
                    if (port.NodeReference == null && port.ConnectedNodeID == -1 && port.ConnectedNodePortID == -1)
                    {
                        RemoveConnectionAt(i);
                        continue;
                    }
                }
            }
        }

        //public bool SomePortIsConnectedWith(NodePortBase nodePortBase)
        //{
        //    for (int i = 0; i < portConnections.Count; i++)
        //    {
        //        if (FGenerators.CheckIfIsNull(portConnections[i])) continue;
        //        if (FGenerators.CheckIfIsNull(portConnections[i].PortReference)) continue;
        //        if (portConnections[i].PortReference == nodePortBase) return true;
        //    }

        //    return false;
        //}

        public void RefreshPortConnections<T>(List<T> allNodes) where T : FGraph_NodeBase
        {
            //bool found = false;
            for (int i = 0; i < allNodes.Count; i++)
            {
                if (allNodes[i].IndividualID == ParentNodeID)
                {
                    ParentNode = allNodes[i];
                    //found = true;
                }
            }

            //if (found == false) UnityEngine.Debug.Log("notfound");

            for (int i = 0; i < portConnections.Count; i++)
            {
                portConnections[i].RefreshReferences(allNodes);
            }

            CheckForEmptyConnections();
        }

        #endregion


        internal void CallFromParentNode(FGraph_NodeBase nodeBase)
        {
            ParentNode = nodeBase;
            ParentNodeID = nodeBase.IndividualID;
            PortID = nodeBase.GetPortIndex(this, !IsOutput);
        }

        /// <summary> Base for Editor code use </summary>
        [NonSerialized] public Rect _E_LatestPortRect = new Rect(0, 0, 1, 1);
        private Rect _E_LatestPortInterationRect = new Rect(0, 0, 1, 1);
        /// <summary> Base for Editor code use </summary>
        public virtual Color GetColor() { return new Color(0.2f, 0.2f, .9f, 1f); }

        /// <summary> Return true if something executed to prevent displaying generic menu for example on right mouse click </summary>
        public virtual bool OnClicked(Event e)
        {
            if (e == null) return false;

            if (e.button == 1)
            {
#if UNITY_EDITOR

                NodePortBase oPort = this;

                if (IsOutput == false)
                {
                    if (BaseConnection == null) return false;
                    else
                    {
                        if (BaseConnection.PortReference != null)
                        {
                            oPort = BaseConnection.PortReference as NodePortBase;
                            if (oPort == this) return false;
                        }
                        else
                            return false;
                    }
                }

                if (oPort != null)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("--- Connection Display Mode ---"), false, () => { });
                    menu.AddItem(GUIContent.none, false, () => { });
                    menu.AddItem(new GUIContent("Default"), oPort.ConnectionDisplay == EPortConnectionDisplayMode.Default, () => { oPort.ConnectionDisplay = EPortConnectionDisplayMode.Default; });
                    menu.AddItem(new GUIContent("Wire Ghosting"), oPort.ConnectionDisplay == EPortConnectionDisplayMode.Ghosting, () => { oPort.ConnectionDisplay = EPortConnectionDisplayMode.Ghosting; });
                    menu.AddItem(new GUIContent("Curled Wire"), oPort.ConnectionDisplay == EPortConnectionDisplayMode.Curled, () => { oPort.ConnectionDisplay = EPortConnectionDisplayMode.Curled; });
                    //menu.AddItem(new GUIContent("Hide Wire"), oPort.ConnectionDisplay == EPortConnectionDisplayMode.Hidden, () => { oPort.ConnectionDisplay = EPortConnectionDisplayMode.Hidden; });
                    menu.ShowAsContext();
                }

#endif
                return true;
            }

            return false;
        }

    }

}
