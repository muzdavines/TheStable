using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Graph
{
    public abstract partial class FGraph_NodeBase
    {


        public virtual void CheckForNulls()
        {
            CheckConnectionsListForNulls(InputConnections);
            CheckConnectionsListForNulls(OutputConnections);
        }

        private void CheckConnectionsListForNulls(List<FGraph_TriggerNodeConnection> connections)
        {
            for (int i = connections.Count - 1; i >= 0; i--)
            {
                if (connections[i] == null)
                {
                    RemoveAtIndex(connections, i);
                    continue;
                }
            }
        }


        public virtual bool IsConnectedWith(FGraph_NodeBase otherNode)
        {
            for (int i = 0; i < OutputConnections.Count; i++)
                if (OutputConnections[i].To == otherNode) return true;

            for (int i = 0; i < InputConnections.Count; i++)
                if (InputConnections[i].From == otherNode) return true;

            return false;
        }


        public virtual FGraph_TriggerNodeConnection CreateConnectionWith(FGraph_NodeBase otherNode, bool connectingFromOut, int fromAltID = -1, int toAltID = -1)
        {
#if UNITY_EDITOR
            baseSerializedObject.Update();
            otherNode.baseSerializedObject.Update();

            FGraph_TriggerNodeConnection c = new FGraph_TriggerNodeConnection();

            if (connectingFromOut)
            {
                c.From = this;
                c.To = otherNode;

                if (OutputConnectorsCount > 1)
                {
                    if (fromAltID == -1) fromAltID = AllowedOutputConnectionIndex;
                }
            }
            else
            {
                c.From = otherNode;
                c.To = this;

                if (otherNode.OutputConnectorsCount > 1)
                {
                    if (fromAltID == -1) fromAltID = otherNode.AllowedOutputConnectionIndex;
                }
            }

            c.To.InputConnections.Add(c);
            c.From.OutputConnections.Add(c);

            c.ConnectionFrom_AlternativeID = fromAltID;
            c.ConnectionTo_AlternativeID = toAltID;

            //UnityEngine.Debug.Log("conn");
            baseSerializedObject.ApplyModifiedProperties();
            otherNode.baseSerializedObject.ApplyModifiedProperties();
            _E_SetDirty();
            otherNode._E_SetDirty();
            return c;
#else
            return null;
#endif
        }

        public virtual void RemoveConnectionWith(FGraph_NodeBase otherNode)
        {
            if (otherNode == null) return;

            for (int i = 0; i < OutputConnections.Count; i++)
            {
                if (OutputConnections[i].To == otherNode)
                {
                    otherNode.RemoveInConnectionWith(this);
                    RemoveAtIndex(OutputConnections, i);
                    return;
                }
            }

            for (int i = 0; i < InputConnections.Count; i++)
            {
                if (InputConnections[i].From == otherNode)
                {
                    otherNode.RemoveOutConnectionWith(this);
                    RemoveAtIndex(InputConnections, i);
                    return;
                }
            }

            otherNode._E_SetDirty();
            _E_SetDirty();
        }

        protected virtual void RemoveInConnectionWith(FGraph_NodeBase otherNode)
        {
            //UnityEngine.Debug.Log("INremove  " + _E_GetDisplayName() + " conn with " + otherNode._E_GetDisplayName());
            for (int i = 0; i < InputConnections.Count; i++)
                if (InputConnections[i].IsConnectedWith(otherNode))
                {
                    RemoveAtIndex(InputConnections, i);
                    return;
                }

            otherNode._E_SetDirty();
            _E_SetDirty();
        }

        public void RemoveAtIndex(List<FGraph_TriggerNodeConnection> connections, int index)
        {
            //UnityEngine.Debug.Log("remove ");
            connections.RemoveAt(index);
            _E_SetDirty();
        }

        protected virtual void RemoveOutConnectionWith(FGraph_NodeBase otherNode)
        {
            //UnityEngine.Debug.Log("OUTremove  " + _E_GetDisplayName() + " conn with " + otherNode._E_GetDisplayName());
            for (int i = 0; i < OutputConnections.Count; i++)
                if (OutputConnections[i].IsConnectedWith(otherNode))
                {
                    RemoveAtIndex(OutputConnections, i);
                    return;
                }

            otherNode._E_SetDirty();
            _E_SetDirty();
        }


        protected void SetMultiOutputID(ref int outputId, int targetId)
        {
            for (int o = 0; o < OutputConnections.Count; o++)
            {
                if (OutputConnections[o].ConnectionFrom_AlternativeID == targetId)
                {
                    outputId = o;
                }
            }
        }


        public void RefreshConnections(List<FGraph_NodeBase> drawingNodes)
        {
            FGraph_RunHandler.RefreshConnectorsConnections(this, drawingNodes);
        }


        public void CheckPortsForNullConnections()
        {
            CheckPortsForNullConnections(inputPorts);
            CheckPortsForNullConnections(outputPorts);
        }


        public void CheckPortsForNullConnections(List<IFGraphPort> ports)
        {
            //UnityEngine.Debug.Log( _E_GetDisplayName() + " ch " + ports.Count);


            for (int i = ports.Count - 1; i >= 0; i--)
            //for (int i = 0; i < ports.Count; i++)
            {
                NodePortBase port = NodePortBase.ToNodePortBase(ports[i]);

                if (port == null) { ports.RemoveAt(i); _E_SetDirty(); continue; }
                //UnityEngine.Debug.Log("checking " + port.DisplayName + " " + port.Connections.Count);

                for (int c = port.Connections.Count - 1; c >= 0; c--)
                {
                    var conn = port.Connections[c];

                    if (conn.NodeReference == null || conn.PortReference == null)
                    {
                        port.RemoveConnectionAt(c);
                    }
                    else
                    {
                        var oPort = port.Connections[c].PortReference;

                        if (oPort == null)
                        {
                            port.RemoveConnectionAt(c);
                            continue;
                        }

                        if (oPort.PortState() != EPortPinState.Connected)
                        {
                            port.RemoveConnectionAt(c);
                            continue;
                        }

                        if (conn.PortReference.PortState() != EPortPinState.Connected)
                        {
                            port.RemoveConnectionAt(c);
                        }
                        else
                        {
                            // If other node is not connected with this one - something is wrong
                            if (port.IsOutput)
                            {
                                if (conn.NodeReference.IsConnectedWith(port) == false)
                                {
                                    port.RemoveConnectionAt(c);
                                }
                            }
                            else // Connection of Input port check
                            {
                                // If connecting to some port which tells that it
                                // is not connecting to this input port
                                if (conn.NodeReference.IsConnectedWith(port) == false)
                                {
                                    port.RemoveConnectionAt(c);
                                }
                            }
                        }

                    }
                }

            }
        }


        bool _editor_wasPortsConnectionsChanged = false;
        /// <summary> Used for editor graph window refresh on port connection changes </summary>
        public virtual void OnPortConnectionsChanged()
        {
            _editor_wasPortsConnectionsChanged = true;
        }

        /// <summary> Used for editor graph window refresh on port connection changes </summary>
        public bool Ports_CheckIfConnectionsChanged()
        {
            if (_editor_wasPortsConnectionsChanged)
            {
                _editor_wasPortsConnectionsChanged = false;
                return true;
            }

            return false;
        }


        public enum ETriggerConnectionDrawMode
        {
            Default, Ghosting, Curled
        }

        [HideInInspector] public ETriggerConnectionDrawMode TriggerConnectionDrawMode = ETriggerConnectionDrawMode.Default;

        public bool OnConnectorClicked(Event e)
        {
            if (e.type == UnityEngine.EventType.MouseUp)
                if (e.button == 1)
                {
#if UNITY_EDITOR
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("--- Connection Display Mode ---"), false, () => { });
                    menu.AddItem(GUIContent.none, false, () => { });
                    menu.AddItem(new GUIContent("Default"), TriggerConnectionDrawMode == ETriggerConnectionDrawMode.Default, () => { TriggerConnectionDrawMode = ETriggerConnectionDrawMode.Default; });
                    menu.AddItem(new GUIContent("Wire Ghosting"), TriggerConnectionDrawMode == ETriggerConnectionDrawMode.Ghosting, () => { TriggerConnectionDrawMode = ETriggerConnectionDrawMode.Ghosting; });
                    menu.AddItem(new GUIContent("Curled Wire"), TriggerConnectionDrawMode == ETriggerConnectionDrawMode.Curled, () => { TriggerConnectionDrawMode = ETriggerConnectionDrawMode.Curled; });
                    menu.ShowAsContext();
                    return true;
#endif
                }

            return false;
        }

    }
}
