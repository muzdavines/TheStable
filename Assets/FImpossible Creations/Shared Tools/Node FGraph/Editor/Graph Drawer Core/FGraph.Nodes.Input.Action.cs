using FIMSpace.Generating;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    public abstract partial class FGraphDrawerBase
    {
        FGraph_NodeBase nodeInput_clickedNode = null;

        double _editorDoubleClickHelper = -100f;
        double _editorDoubleClickMMBHelper = -100f;

        NodePortBase GetPortUnderTheCursor(Event e, List<IFGraphPort> ports)
        {
            if (ports != null)
                for (int i = 0; i < ports.Count; i++)
                {
                    Rect outPortRect = ports[i].PortClickAreaRect;
                    if (outPortRect.Contains(inGraphMousePos))
                    {
                        NodePortBase port = ports[i] as NodePortBase;

                        if (FGenerators.CheckIfExist_NOTNULL(port))
                        {
                            return ports[i] as NodePortBase;
                        }
                    }
                }

            return null;
        }


        bool IsTriggerPortUnderCursor(FGraph_NodeBase node)
        {
            bool contains = false;

            if (node.OutputConnectorsCount > 1)
            {
                for (int i = 0; i < node.OutputConnectorsCount; i++)
                {
                    contains = node.Drawer(this).GetBaseConnectorOutputRect(node._E_LatestRect, i).Contains(inGraphMousePos);
                    if (contains) break;
                }
            }
            else
                contains = node.Drawer(this).GetBaseConnectorOutputRect(node._E_LatestRect).Contains(inGraphMousePos);

            return contains;
        }



        void NodeInput_MouseUp(Event e, FGraph_NodeBase node)
        {
            isConnectingNodes = false;

            // check for port click
            bool portClick = false;


            #region Port click check

            NodePortBase portUnderCur = GetPortUnderTheCursor(e, node.GetOutputPorts());
            if (portUnderCur != null)
            {
                portClick = portUnderCur.OnClicked(e);
            }
            else
            {
                portUnderCur = GetPortUnderTheCursor(e, node.GetInputPorts());
                if (portUnderCur != null) portClick = portUnderCur.OnClicked(e);
            }

            #endregion


            #region Trigger click check

            if (!portClick)
                if (node.DrawOutputConnector || node.OutputConnectorsCount > 1)
                {
                    if (IsTriggerPortUnderCursor(node)) portClick = node.OnConnectorClicked(e);
                }

            #endregion


            if (portClick)
            {
                e.Use();
                return;
            }


            if (e.button == 1) // Modify Node Menu
            {
                NodeModifyMenu(Event.current, node);
                e.Use();
            }
            else if (e.button == 0) // Click on node - select
            {
                if (dragMultiSelected.Count == 0)
                {
                    if (isMouseCursorInGraph)
                    {

                            nodeInput_clickedNode = node;

                        if (EditorApplication.timeSinceStartup - _editorDoubleClickHelper < 0.2)
                        {
                            DoubleClickNode(e, node);
                        }
                    }

                    _editorDoubleClickHelper = EditorApplication.timeSinceStartup;

                    if (!isMouseCursorInGraph)
                    {
                        Event.current.Use();
                    }
                }
            }
        }

        protected virtual void DoubleMMBClick(Event e)
        {

        }

        protected virtual void DoubleClickNode(Event e, FGraph_NodeBase node)
        {

        }

        void NodeInput_MouseDrag(Event e, FGraph_NodeBase node)
        {
            // Detecting click-dragging on outputs
            if (!canDragNode)
            {
                bool connecting = false;
                var ports = node.GetOutputPorts();
                if (ports != null)
                {
                    for (int i = 0; i < ports.Count; i++)
                    {
                        Rect outPortRect = ports[i].PortClickAreaRect;
                        if (outPortRect.Contains(inGraphMousePos))
                        {
                            StartConnecting(node, true, ports[i]);
                            connecting = true;
                        }
                    }
                }

                // Output ports click check - only disconnecting
                if (!connecting)
                {
                    var inPorts = node.GetInputPorts();
                    if (inPorts != null)
                        for (int i = 0; i < inPorts.Count; i++)
                        {
                            Rect inPortRect = inPorts[i].PortClickAreaRect;
                            if (inPortRect.Contains(inGraphMousePos))
                            {
                                StartConnecting(node, false, inPorts[i]);
                                connecting = true;
                            }
                        }
                }

                if (!connecting) // No action on ports -> so let's use trigger connectors
                {
                    bool onOutputArea = false;

                    if (node.DrawOutputConnector)
                    {
                        if (node.OutputConnectorsCount < 2)
                        {
                            if (node.Drawer(this).GetBaseConnectorOutputClickAreaRect().Contains(inGraphMousePos))
                            {
                                StartConnecting(node, true);
                                onOutputArea = true;
                            }
                        }
                        #region Multiple Connectors
                        else
                        {
                            for (int i = 0; i < node.OutputConnectorsCount; i++)
                            {
                                if (node.Drawer(this).GetBaseConnectorOutputClickAreaRect(i).Contains(inGraphMousePos))
                                {
                                    StartConnecting(node, true, null, i);
                                    onOutputArea = true;
                                }
                            }
                        }
                        #endregion
                    }

                    if (!onOutputArea)
                        if (node.DrawInputConnector)
                        {
                            if (node.Drawer(this).GetBaseConnectorInputClickAreaRect().Contains(inGraphMousePos))
                                StartConnecting(node, false);
                        }
                }

            }
        }

        void NodeInput_MouseDown(Event e, FGraph_NodeBase node)
        {
            if (!node.Drawer(this).InteractionAreaContainsCursor(inGraphMousePos))
            {
                if (dragMultiSelected.Count == 0)
                {
                        nodeInput_clickedNode = node;

                    //FGraph_NodeBase.SelectNode(node);
                    refreshAfterSelecting = 5;
                }

                //GUI.FocusWindow(w);

                    nodeInputToUseAfterDraw = e;

                canDragNode = node.Drawer(this).GetDragRect().Contains(inGraphMousePos);

            }
            else
            {
                canDragNode = false;
            }
        }


    }
}