using FIMSpace.FEditor;
using FIMSpace.Generating;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    public abstract partial class FGraphDrawerBase
    {
        bool isConnectingNodes = false;
        bool connectingFromOut = true;
        FGraph_NodeBase connectingFrom = null;
        int connectingFromHelperID = -1;
        IFGraphPort connectingFromPort = null;
        bool reloadConnections = false;

        protected virtual void DrawConnections()
        {
            Event e = Event.current;

            #region Active Connection Drag Drawing

            if (e != null && FGenerators.CheckIfExist_NOTNULL(connectingFrom))
            {

                var from = connectingFrom;
                var to = enteredOnNode;
                if (to == from) to = null;
                if (FGenerators.CheckIfExist_NOTNULL(to)) if (connectingFromPort == null) if (to.DrawInputConnector == false) to = null;

                if (FGenerators.CheckIfExist_NOTNULL(to))
                {
                    if (connectingFromPort == null) // Not from port - so from connector
                    {
                        if (connectingFrom.OutputConnectorsCount < 2)
                        {
                            bool display = true;
                            if (connectingFromOut == false) if (to.OutputConnectorsCount > 1) display = false;
                            if (display)
                            {
                                DrawConnection(from, to, connectingFromOut);
                            }
                        }
                        else
                        {
                            DrawConnection(
                                connectingFrom.Drawer(this).GetBaseConnectorOutputClickAreaRect(connectingFromHelperID).center,
                                to.Drawer(this).GetInputConnectorPinPosition(), FGraph_NodeBase_Drawer.EConnectorsWireMode.Up_Down, connectingFromOut);
                        }
                    }
                    else // Connection from port
                    {
                        // Nearest fitting port
                        var otherInputs = to.GetInputPorts();
                        if (otherInputs != null)
                        {
                            IFGraphPort nearestPort = GetNearestFittingPort(otherInputs);

                            if (nearestPort != null)
                            {
                                //GUI.Box(nearestPort.PortClickAreaRect, GUIContent.none, FGUI_Resources.BGInBoxStyleH);
                                //UnityEngine.Debug.Log("nearestPort.GetPortRect " + nearestPort.GetPortRect + " " + (nearestPort as NodePortBase).DisplayName);
                                Vector2 ap = connectingFromPort.PortClickAreaRect.center;
                                Vector2 topos = nearestPort.PortClickAreaRect.center;

                                DrawConnectionWithTangents(ap, topos, connectingFromOut, connectingFromPort.PortColor);
                            }
                        }
                    }
                }
                else // To mouse position
                {
                    if (connectingFromPort == null) // Not from port - so from connector
                    {
                        Vector2 ap = Vector2.zero;

                        if (connectingFromOut)
                        {
                            if (connectingFrom.OutputConnectorsCount < 2)
                                ap = connectingFrom.Drawer(this)._E_LatestOutputRect.center;
                            else
                                ap = connectingFrom.Drawer(this).GetBaseConnectorOutputClickAreaRect(connectingFromHelperID).center;
                        }

                        if (ap == Vector2.zero) ap = connectingFrom.Drawer(this)._E_LatestInputRect.center;
                        DrawConnectionWithTangents(ap, e.mousePosition, connectingFromOut);
                    }
                    else
                    {
                        Vector2 ap = connectingFromPort.PortClickAreaRect.center;
                        DrawConnectionWithTangents(ap, e.mousePosition, connectingFromOut, connectingFromPort.PortColor);
                    }

                }

            }

            #endregion

            if (drawingNodes != null)
                for (int n = 0; n < drawingNodes.Count; n++)
                {
                    var node = drawingNodes[n];
                    if (node is null) continue;


                    #region Trigger Connections Draw

                    for (int o = node.OutputConnections.Count - 1; o >= 0; o--)
                    {
                        var conn = node.OutputConnections[o];
                        if (conn is null) { node.OutputConnections.RemoveAt(o); reloadConnections = true; continue; }
                        if (conn.From is null) { reloadConnections = true; continue; }
                        if (conn.To is null) { reloadConnections = true; continue; }

                        if (node.isCulled && node.OutputConnections[o].To.isCulled) continue;
                        if (node.IsContainedBy != null) if (node.IsContainedBy._EditorFoldout == false) continue;

                        Color? col = null;
                        if (node.OutputConnections[o] == latestClickedConnection)
                        {
                            col = new Color(1f, .7f, 0.2f, 1f);
                        }

                        DrawNodeTriggersConnection(node, node.OutputConnections[o].To, col, conn);
                    }

                    #endregion


                    #region Output Ports Connections Draw

                    var outputPorts = node.GetOutputPorts();

                    if (outputPorts != null)
                    {
                        for (int i = 0; i < outputPorts.Count; i++)
                        {
                            var port = outputPorts[i];
                            NodePortBase portB = port as NodePortBase;

                            if (portB != null)
                            {

                                for (int p = port.Connections.Count - 1; p >= 0; p--)
                                {
                                    var conn = port.Connections[p];

                                    if (conn.ConnectedNodeID == -1) { continue; }
                                    if (conn.ConnectedNodePortID == -1) { continue; }
                                    if (FGenerators.CheckIfIsNull(conn.PortReference)) port.RefreshPortConnections(GetAllNodes());
                                    if (conn.PortReference == null) { continue; }
                                    if (node.RefreshedPorts) if (conn.NodeReference == null) { port.Connections.RemoveAt(p); continue; }

                                    if (node.isCulled && conn.NodeReference.isCulled) continue;
                                    if (node.IsContainedBy != null) if (node.IsContainedBy._EditorFoldout == false) continue;
                                    DrawNodePortsConnection(port, conn);
                                }
                            }
                        }
                    }

                    #endregion

                }



            if (reloadConnections) return;



            if (wasCheckingEnter)
            {
                SwitchEnteredOnNode(null);
            }


            if (isSelectingMultiple)
            {
                dragMultiSelected.Clear();

                if (drawingNodes != null)
                    for (int i = 0; i < drawingNodes.Count; i++)
                        if (selectingMultipleRect.Overlaps(drawingNodes[i]._E_LatestRect))
                            dragMultiSelected.Add(drawingNodes[i]);
            }

        }


        public virtual void DrawNodeTriggersConnection(FGraph_NodeBase output, FGraph_NodeBase input, Color? col, FGraph_TriggerNodeConnection conn)
        {
            if (output.TriggerConnectionDrawMode == FGraph_NodeBase.ETriggerConnectionDrawMode.Default || output._editorForceDrawDefaultTriggerWires)
            {
                DrawConnection(output, input, true, col, 5, conn);
            }
            else
            {
                Color nCol = Color.gray;
                if (col != null) nCol = col.Value;

                var from = output;
                Vector2 a = output.Drawer(this).GetOutputConnectorPinPosition();

                int fromId = -1;
                if (conn != null) fromId = conn.ConnectionFrom_AlternativeID;
                if (fromId != -1)
                {
                    a = output.Drawer(this).GetOutputConnectorPinPosition(fromId);
                }

                Vector2 b = Vector2.zero;
                if ((input is null) == false) if (input != from) b = input.Drawer(this).GetInputConnectorPinPosition();

                if (output.TriggerConnectionDrawMode == FGraph_NodeBase.ETriggerConnectionDrawMode.Ghosting)
                {
                    Color ghCol = nCol * 0.8f;
                    ghCol.a *= 0.325f;
                    DrawConnectionWithTangents(a, b, true, ghCol, 6, false, FGraphStyles.TEX_Gradient3, FGraph_NodeBase_Drawer.EConnectorsWireMode.Up_Down, .8f, .8f, .6f, .6f, 0.2f);
                }
                else if (output.TriggerConnectionDrawMode == FGraph_NodeBase.ETriggerConnectionDrawMode.Curled)
                {
                    Vector2 aTob = (b - a).normalized;

                    float curlOff = 18f;
                    DrawConnectionWithTangents(b, b - aTob * curlOff, true, nCol, 4, false, FGraphStyles.TEX_Gradient3, FGraph_NodeBase_Drawer.EConnectorsWireMode.Up_Down, 0f, 0f, 0f, 0f, 0.0f);
                    DrawConnectionWithTangents(a, a + aTob * curlOff, true, nCol, 4, false, FGraphStyles.TEX_Gradient3, FGraph_NodeBase_Drawer.EConnectorsWireMode.Up_Down, 0f, 0f, 0f, 0f, 0.0f);
                }

            }
        }

        public virtual void DrawNodePortsConnection(IFGraphPort thisPort, PortConnection conn)
        {
            NodePortBase port = thisPort as NodePortBase;

            if (FGenerators.NotNull(port))
            {
                bool drawDef = false;
                if (conn != null) if (conn.NodeReference != null) drawDef = conn.NodeReference._editorForceDrawDefaultTriggerWires;


                if (port._EditorForceDrawDefaultWires == false || drawDef)
                {
                    if (port.ConnectionDisplay == NodePortBase.EPortConnectionDisplayMode.Hidden) return;

                    if (port.ConnectionDisplay == NodePortBase.EPortConnectionDisplayMode.Ghosting)
                    {
                        Vector2 a = thisPort.PortClickAreaRect.center;
                        Vector2 b = conn.PortReference.PortClickAreaRect.center;
                        //Vector2 aTob = (b - a).normalized;
                        //float drawLen = 18f;
                        if (conn.NodeReference) if (conn.NodeReference._EditorWasDrawn == false) if (conn.NodeReference.IsContainedBy != null) b = conn.NodeReference.IsContainedBy._E_LatestRect.center - new Vector2(0, conn.NodeReference.IsContainedBy._E_LatestRect.height * 0.36f);

                        //DrawConnectionWithTangents(a, a + aTob * drawLen, true, thisPort.PortColor, 5, false, FGraphStyles.TEX_Gradient3);

                        Color ghCol = thisPort.PortColor * 0.7f;
                        ghCol.a *= 0.325f;
                        DrawConnectionWithTangents(a, b, true, ghCol, 6, false, FGraphStyles.TEX_Gradient3);
                        //DrawConnectionWithTangents(a + aTob * drawLen, b, true, ghCol, 6, false, FGraphStyles.TEX_Gradient3);
                        //DrawConnectionWithTangents(a + aTob * drawLen, b - aTob * drawLen, true, ghCol, 6, false, FGraphStyles.TEX_Gradient3);
                        //DrawConnectionWithTangents(b, b - aTob * drawLen, true, thisPort.PortColor, 5, false, FGraphStyles.TEX_Gradient3, FGraph_NodeBase_Drawer.EConnectorsWireMode.Left_Right);

                        return;
                    }
                    else if (port.ConnectionDisplay == NodePortBase.EPortConnectionDisplayMode.Curled)
                    {
                        Vector2 a = thisPort.PortClickAreaRect.center;
                        Vector2 b = conn.PortReference.PortClickAreaRect.center;
                        if (conn.NodeReference) if (conn.NodeReference._EditorWasDrawn == false) if (conn.NodeReference.IsContainedBy != null) b = conn.NodeReference.IsContainedBy._E_LatestRect.center - new Vector2(0, conn.NodeReference.IsContainedBy._E_LatestRect.height * 0.36f);
                        Vector2 aTob = (b - a).normalized;

                        float curlOff = 15f;
                        DrawConnectionWithTangents(b, b - aTob * curlOff, true, thisPort.PortColor, 4, false, FGraphStyles.TEX_Gradient3, FGraph_NodeBase_Drawer.EConnectorsWireMode.Left_Right, 1f, 1f, 1f, 1f);
                        DrawConnectionWithTangents(a, a + aTob * curlOff, true, thisPort.PortColor, 4, false, FGraphStyles.TEX_Gradient3, FGraph_NodeBase_Drawer.EConnectorsWireMode.Left_Right, 1f, 1f, 1f, 1f);

                        return;
                    }
                }
            }

            Vector3 bPos = conn.PortReference.PortClickAreaRect.center;
            if (conn.NodeReference) if (conn.NodeReference._EditorWasDrawn == false) if (conn.NodeReference.IsContainedBy != null) bPos = conn.NodeReference.IsContainedBy._E_LatestRect.center - new Vector2(0, conn.NodeReference.IsContainedBy._E_LatestRect.height * 0.36f);
            DrawConnectionWithTangents(thisPort.PortClickAreaRect.center, bPos, true, thisPort.PortColor, 6, false, FGraphStyles.TEX_Gradient3);

        }



        /// <summary>
        /// Finding Right + Nearest to cursor Port
        /// </summary>
        protected virtual IFGraphPort GetNearestFittingPort(List<IFGraphPort> otherInputs)
        {
            if (otherInputs == null) return null;
            if (otherInputs.Count <= 0) { return null; }

            IFGraphPort nearestPort = null;

            float nearest = float.MaxValue;
            for (int i = 0; i < otherInputs.Count; i++)
            {
                var inp = otherInputs[i];

                if (inp.GetPortValueType == null)
                {
                    UnityEngine.Debug.Log("null");
                    continue;
                    //NodePortBase port = inp as NodePortBase;
                    //if (port != null) port.ParentNode.PortConnectionRequestsRefresh(port);
                    //if (inp.GetPortValue == null) continue;
                }

                if (connectingFromPort.CanConnectWith(inp))
                //if (inp.GetPortValue.GetType() == connectingFromPort.GetPortValue.GetType())
                {
                    float dist = Vector2.Distance(inGraphMousePos, inp.PortClickAreaRect.center);
                    if (dist < nearest) { nearestPort = inp; nearest = dist; }
                }
                else
                {
                    //UnityEngine.Debug.Log("cannot with " + i);
                }

            }

            return nearestPort;
        }


        protected virtual void DrawConnection(Vector2 a, Vector2 b, FGraph_NodeBase_Drawer.EConnectorsWireMode wireMode, bool fromOutToInput = true, Color? color = null, float thickness = 5f, Vector2? offsets = null, Texture2D lineTex = null)
        {
            if (b != Vector2.zero) DrawConnectionWithTangents(a, b, fromOutToInput, color, thickness, false, lineTex, wireMode);
        }

        protected FGraph_NodeBase _DrawConnection_output = null;
        protected FGraph_NodeBase _DrawConnection_input = null;
        protected virtual void DrawConnection(FGraph_NodeBase output, FGraph_NodeBase input, bool fromOutToInput = true, Color? color = null, float thickness = 5f, FGraph_TriggerNodeConnection optionalRefToConn = null, Vector2? offsets = null, Texture2D lineTex = null)
        {
            if ((output is null) || (input is null)) return;
            //if (output._EditorWasDrawn == false) return;

            var from = output;
            var to = input;

            _DrawConnection_input = input;
            _DrawConnection_output = output;

            Vector2 off = Vector2.zero;
            if (offsets != null) off = offsets.Value;

            int fromId = -1;
            if (optionalRefToConn != null) fromId = optionalRefToConn.ConnectionFrom_AlternativeID;

            Vector2 a = output.Drawer(this).GetOutputConnectorPinPosition();


            if (fromOutToInput == false)
            {
                a = output.Drawer(this)._E_LatestInputRect.center;
            }
            else
            {
                if (fromId != -1)
                {
                    a = output.Drawer(this).GetOutputConnectorPinPosition(fromId);
                }
            }

            Vector2 b = Vector2.zero;

            if ((to is null) == false) if (to != from)
                {
                    b = to.Drawer(this).GetInputConnectorPinPosition();
                    if (fromOutToInput == false)
                    {
                        b = to.Drawer(this)._E_LatestOutputRect.center;
                    }
                }

            if (input._EditorWasDrawn == false) if (input.IsContainedBy != null) b = input.IsContainedBy._E_LatestRect.center - new Vector2(0, input.IsContainedBy._E_LatestRect.height * 0.36f);

            if (b != Vector2.zero)
                DrawConnectionWithTangents(a + off, b + off, fromOutToInput, color, thickness, false, lineTex, output.Drawer(this).ConnectorsWiresMode);

            _DrawConnection_input = null;
            _DrawConnection_output = null;
        }

        protected virtual Texture2D DefaultConnectionTex { get { return null; } }
        protected virtual Color DefaultConnectionColor { get { return new Color(0.6f, 0.6f, 0.6f, 1f); } }

        /// <summary> Returns middle point of bezier curve if getMiddlePoint = true </summary>
        protected virtual Vector3 DrawConnectionWithTangents(Vector2 start, Vector2 end, bool outToInput = true, Color? color = null, float thickness = 5f, bool getMiddlePoint = false, Texture2D lineTex = null, FGraph_NodeBase_Drawer.EConnectorsWireMode wireMode = FGraph_NodeBase_Drawer.EConnectorsWireMode.Left_Right, float oXTan = 0.8f, float iXTan = 0.8f, float oYTan = 0.6f, float iYTan = 0.6f, float tanMul = 1f)
        {
            if (start == Vector2.zero)
                start = end + new Vector2(-10, 0);
            else if (end == Vector2.zero)
                end = start + new Vector2(-10, 0);

            Vector2 a = start;
            Vector2 b = end;

            Vector2 toTarget = b - a;
            //Vector2 toSign = new Vector2(Mathf.Sign(toTarget.x), Mathf.Sign(toTarget.y));

            Color lCol = DefaultConnectionColor;
            if (color != null) lCol = color.Value;

            Vector2 inTan, outTan;

            if (wireMode == FGraph_NodeBase_Drawer.EConnectorsWireMode.Left_Right)
            {
                bool rShift;
                if (outToInput) rShift = toTarget.x > 0f;
                else rShift = toTarget.x < 0f;

                if (rShift)
                {
                    if (iYTan == 0.6f || iYTan == 0.6f) iYTan = 0.125f;
                    if (oYTan == 0.6f || oYTan == 1.05f) oYTan = 0.125f;
                    if (iXTan == 0.8f || iXTan == 0.5f) iXTan = 0.65f;
                    if (oXTan == 0.8f || oXTan == 0.9f) oXTan = 0.65f;
                    //inTan = a; outTan = b; // Linear
                    inTan = a + new Vector2(toTarget.x * iXTan, toTarget.y * iYTan) * tanMul;
                    outTan = b + new Vector2(toTarget.x * -oXTan, toTarget.y * oYTan) * tanMul;
                }
                else
                {
                    inTan = a + new Vector2(Mathf.Min(128, toTarget.x * -iXTan), toTarget.y * iYTan) * tanMul;
                    outTan = b + new Vector2(Mathf.Max(-128, toTarget.x * oXTan), -toTarget.y * oYTan) * tanMul;
                }
            }
            else //if (wireMode == FGraph_NodeBase_Drawer.EConnectorsWireMode.Up_Down)
            {
                bool aboveShift;
                if (outToInput) aboveShift = toTarget.y < 0f;
                else aboveShift = toTarget.y > 0f;

                if (!aboveShift)
                {
                    if (iYTan == 0.6f) iYTan = 0.5f;
                    if (oYTan == 0.6f) oYTan = 0.5f;
                    if (iXTan == 0.8f) iXTan = 0.25f;
                    if (oXTan == 0.8f) oXTan = 0.25f;
                    inTan = a + new Vector2(toTarget.x * iXTan, toTarget.y * iYTan) * tanMul;
                    outTan = b + new Vector2(toTarget.x * -oXTan, -toTarget.y * oYTan) * tanMul;
                }
                else
                {
                    if (iYTan == 0.6f) iYTan = 1f;
                    if (oYTan == 0.6f) oYTan = 1f;
                    if (iXTan == 0.8f) iXTan = 0.5f;
                    if (oXTan == 0.8f) oXTan = 0.5f;
                    inTan = a + new Vector2(toTarget.x * iXTan + 32, -toTarget.y * iYTan + 20) * tanMul;
                    outTan = b + new Vector2(toTarget.x * -oXTan - 32, toTarget.y * oYTan - 20) * tanMul;
                }
            }


            if (getMiddlePoint)
                connBezier = Handles.MakeBezierPoints(a, b, inTan, outTan, 3);

            if (lineTex == null) lineTex = DefaultConnectionTex;
            Handles.DrawBezier(a, b, inTan, outTan, lCol, lineTex, thickness);

            return connBezier[1];
        }

        protected Vector3[] connBezier = new Vector3[3];
        readonly Vector3[] connMidRect = new Vector3[5];

        void StopConnectingNodes()
        {
            if (connectingFrom == enteredOnNode)
            {
                SwitchEnteredOnNode(null);
            }

            if ((connectingFrom is null == false) && (enteredOnNode is null) == false)
            {
                // Trigger connection
                if (connectingFromPort == null)
                {
                    if (connectingFrom.IsConnectedWith(enteredOnNode) == false)
                    {
                        bool canConnect = true;
                        if (connectingFromOut == false) if (enteredOnNode.OutputConnectorsCount > 1) canConnect = false;
                        if (canConnect) CreateConnectionWith(connectingFrom, enteredOnNode, connectingFromOut);
                    }
                }
                else // Port connection
                {
                    var targetPort = GetNearestFittingPort(enteredOnNode.GetInputPorts());
                    if (targetPort != null)
                    {
                        NodePortBase portB = connectingFromPort as NodePortBase;
                        if (portB != null)
                        {
                            portB.ConnectWith(connectingFrom, connectingFromPort, enteredOnNode, targetPort);
                            OnGraphStructureChange();
                        }
                    }

                }
            }

            isConnectingNodes = false;
            connectingFrom = null;
        }


        protected virtual void CreateConnectionWith(FGraph_NodeBase from, FGraph_NodeBase to, bool connectingFromOut)
        {
            if (from.DrawOutputConnector == false) return;
            if (to.DrawInputConnector == false) return;
            var conn = from.CreateConnectionWith(to, connectingFromOut, connectingFromHelperID);
            OnAddConnection(conn);
            SetDirty();
        }

        void StartConnecting(FGraph_NodeBase node, bool dragFromOutput, IFGraphPort port = null, int multipleConnectorsHelperID = -1)
        {
            if (isConnectingNodes) return;

            connectingFrom = node;
            connectingFromHelperID = multipleConnectorsHelperID;
            connectingFromPort = port;
            connectingFromOut = dragFromOutput;
            isConnectingNodes = true;

            // Supporting unpinning connections
            if (dragFromOutput == false)
            {
                if (port == null)
                {
                    if (node.InputConnections.Count > 0)
                        if (node.InputConnections[0] != null)
                        {
                            connectingFrom = node.InputConnections[0].GetOther(node);
                            node.RemoveConnectionWith(connectingFrom);
                            connectingFromOut = true;
                            OnRemoveConnection(null);
                        }
                }
                else
                {
                    port.RefreshPortConnections(GetAllNodes());
                    // Can disconnect?
                    if (port.Connections == null || port.Connections.Count == 0)
                    {
                        StopConnectingNodes();
                    }
                    else // Disconnection
                    {
                        port.RefreshPortConnections(GetAllNodes());

                        if (port.BaseConnection.PortReference != null)
                        {
                            // Remember port on which drag start was performed
                            NodePortBase portB = connectingFromPort as NodePortBase;
                            NodePortBase oPortB = port.BaseConnection.PortReference as NodePortBase;

                            oPortB.DisconnectWith(connectingFrom, connectingFromPort);

                            // Swap connection wire start point
                            connectingFromPort = port.BaseConnection.PortReference;
                            connectingFrom = port.BaseConnection.NodeReference;

                            portB.DisconnectWith(connectingFrom, connectingFromPort);

                            connectingFromOut = true;
                            OnRemoveConnection(null);
                        }
                    }

                    return;
                }
            }

            if ((node is null) == false) node.CheckForNulls();
            if ((connectingFrom is null) == false) connectingFrom.CheckForNulls();
        }

    }
}