using FIMSpace.FEditor;
using FIMSpace.Generating;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    public abstract partial class FGraphDrawerBase
    {
        protected FGraph_NodeBase selectedNode = null;
        protected FGraph_NodeBase lastestSelectedNode = null;
        protected FGraph_NodeBase lastestDraggedNode = null;
        protected FGraph_NodeBase enteredOnNode = null;
        protected FGraph_TriggerNodeConnection latestClickedConnection = null;

        bool wasCheckingEnter = true;
        bool canDragNode = true;
        bool wasDraggingNode = false;
        bool wasDragging = false;

        protected FGraph_NodeBase lastResizedNode = null;
        bool wasResizing = false;
        Vector2 startResizeScale = Vector2.zero;
        Vector2 startResizePos = Vector2.zero;

        Event graphAfterDrawToUse = null;
        protected bool isMouseCursorInGraph;

        protected void SwitchEnteredOnNode(FGraph_NodeBase newNode)
        {
            if (FGenerators.NotNull(enteredOnNode)) enteredOnNode.OnCursorExitNode();
            enteredOnNode = newNode;
            if (FGenerators.NotNull(newNode)) newNode.OnCursorEnterOnNode();
        }

        /// <summary> Initial draw procedures </summary>
        void GraphBeginDraw()
        {
            isMouseCursorInGraph = IsCursorInGraph();
            graphAfterDrawToUse = null;
            BeginDrawInputs();
        }


        /// <summary>
        /// Finishing drawing graph + executing finish input events
        /// </summary>
        void GraphEndDraw()
        {
            EndDrawInputs();
        }


        Rect TransposePortRect(FGraph_NodeBase node, IFGraphPort port)
        {

            NodePortBase portB = port as NodePortBase;
            if (FGenerators.CheckIfIsNull(portB))
            {
                return port.PortClickAreaRect;
            }

            Vector2 drawerPos = node.Drawer(this).GetGuiBodyRect().position;

            if (port.GetPortRect.position == Vector2.zero)
                return portB._E_LatestCorrectPortRect;

            if (drawerPos == Vector2.zero)
                return portB._E_LatestCorrectPortRect;

            portB._E_LatestCorrectPortRect = port.GetPortRect;

            portB._E_LatestCorrectPortRect.position += node._E_LatestRect.position + drawerPos;

            portB._E_LatestCorrectPortRect.size += new Vector2(8, 8);
            portB._E_LatestCorrectPortRect.position -= new Vector2(4, 4);

            return portB._E_LatestCorrectPortRect;
        }


        /// <summary> List of nodes displayed on the graph </summary>
        List<FGraph_NodeBase> drawingNodes = new List<FGraph_NodeBase>();
        List<FGraph_NodeBase> toRemoveOnDraw = new List<FGraph_NodeBase>();
        public bool UseNodesCulling = true;
        protected virtual void DrawNodes()
        {
            DrawGraphSelectionFrame();
            HighlightSelectedNodes();

            preNodeDragPos = Vector2.zero;

            NodesBounds = new Bounds(); // Refresh bounds before nodes draw

            drawingNodes = GetAllNodes(); // Refresh nodes to draw

            if (drawingNodes != null)
            {
                for (int i = 0; i < drawingNodes.Count; i++)
                {
                    FGraph_NodeBase node = drawingNodes[i];
                    if (node is null) continue;

                    NodeDrawStack(i, node);
                }
            }


            if (isConnectingNodes)
            { InputEnd_MouseUp(Event.current); }
            else
            { if (!(lastestDraggedNode is null)) TranslateSelectedNodes(); }
        }

        void NodeDrawStack(int i, FGraph_NodeBase node)
        {
            node._EditorWasDrawn = false;

            #region Containable Hide Implementation

            if ( node.IsContainable == false)
            {
                if ( node.IsContainedBy != null)
                {
                    if ( node.IsContainedBy.IsFoldable)
                        if (node.IsContainedBy._EditorFoldout == false)
                        {
                            return;
                        }
                }
            }

            #endregion


            if (lastestDraggedNode != null)
                if (node == lastestDraggedNode) preNodeDragPos = lastestDraggedNode._E_LatestRect.position;

            Rect nodeRect = new Rect(node.NodePosition, node.NodeSize);

            node.isCulled = false;
            if (UseNodesCulling) if (drawingNodes.Count > 12) if (viewRect.Overlaps(nodeRect) == false) node.isCulled = true;
            //else

            var drawer = node.Drawer(this);
            if (drawer != null)
            {
                nodeRect = drawer.DrawFullNode(graphAreaRect);

                node._E_LatestRect = nodeRect;
                if (node.isCulled == false) NodePostFrameDraw(node);

                Vector2 initRectPos = node._E_LatestRect.position; // To change check

                if (node.IsContainable)
                {
                    // GUI.Window mouse button press event swallowing is uncontrollable! 
                    // As for now no other solution than disabling it at certain mouse positions 

                    bool can = true;

                    Rect containableRect = new Rect(nodeRect);
                    containableRect.height -= 100;
                    containableRect.position += new Vector2(0,60);

                    if ( containableRect.Contains(inGraphMousePos))
                    {
                        can = false;
                    }

                    if (can) nodeRect = GUI.Window(i, nodeRect, OnNodeDraw, "", EditorStyles.label);
                }
                else // Default node
                {
                    nodeRect = GUI.Window(i, nodeRect, OnNodeDraw, "", EditorStyles.label);
                }

                node.NodePosition = nodeRect.position;
                node._E_LatestRect = nodeRect;

                //node._E_latestGuiDrawerBodyRectPos = node.Drawer().GetGuiBodyRect().position;


                #region Debug Boxes
                //var ports = node.GetOutputPorts();
                //if (ports != null)
                //{
                //    //UnityEngine.Debug.Log("ports " + node.GetOutputPorts().Count);
                //    for (int o = 0; o < ports.Count; o++)
                //    {
                //        Rect outPortRect = ports[o].PortClickAreaRect; // Debug draw cursor interaction areas
                //        GUI.Box(outPortRect, GUIContent.none, FGUI_Resources.BGInBoxStyleH);
                //    }
                //}
                // ports = node.GetInputPorts();
                //if (ports != null)
                //{
                //    for (int o = 0; o < ports.Count; o++)
                //    {
                //        Rect outPortRect = ports[o].PortClickAreaRect; // Debug draw cursor interaction areas
                //        GUI.Box(outPortRect, GUIContent.none, FGUI_Resources.BGInBoxStyleH);
                //    }
                //}

                #endregion


                if (initRectPos != node.NodePosition)
                {
                    node._E_SetDirty();
                    node.baseSerializedObject.ApplyModifiedProperties();

                    //if (node.IsFoldableFix) 
                    //if (!requireNodeSerializeUpdateNodes.Contains(node)) requireNodeSerializeUpdateNodes.Add(node);
                }

            }
            else { toRemoveOnDraw.Add(node); }

        }

        /// <summary>
        /// Can be used to display debugging progress bar
        /// </summary>
        protected virtual void NodePostFrameDraw(FGraph_NodeBase node)
        {
        }


        void OnNodeDraw(int windowID)
        {
            if (windowID >= drawingNodes.Count) return;
            DrawNode(drawingNodes[windowID]);
        }


        protected bool repaintRequest = false;
        protected bool refreshRequest = false;
        bool wasRemovingNodes = false;


        public virtual void OnInspectorUpdate()
        {
            #region Draw GUI Area for node apply changes after inputs

            if (requireNodeSerializeUpdateNodes.Count > 0)
            {
                for (int i = 0; i < requireNodeSerializeUpdateNodes.Count; i++)
                {
                    var node = requireNodeSerializeUpdateNodes[i];
                    if (node == null) continue;
                    //new SerializedObject(node).ApplyModifiedProperties();

                    if (node.baseSerializedObject == null) continue;
                    node.baseSerializedObject.ApplyModifiedProperties(); UnityEngine.Debug.Log("up " + node.GetDisplayName());
                    if (node.baseSerializedObject.targetObject == null) continue;
                    node.baseSerializedObject.Update();
                }

                requireNodeSerializeUpdateNodes.Clear();
            }

            for (int i = 0; i < nodesToDraw.Count; i++)
            {
                if (nodesToDraw[i] == null) continue;
                if (nodesToDraw[i].baseSerializedObject == null) continue;
                if (nodesToDraw[i].baseSerializedObject.targetObject == null) continue;
                nodesToDraw[i].baseSerializedObject.Update();
            }

            #endregion

            //RefreshNodePortsRects();
        }


        void RefreshNodePortsRects()
        {
            GetAllNodes();

            for (int n = 0; n < nodesToDraw.Count; n++)
            {
                FGraph_NodeBase node = nodesToDraw[n];
                if (node is null) continue;

                #region Refresh Ports

                for (int i = 0; i < node.GetInputPorts().Count; i++)
                {
                    var port = node.GetInputPorts()[i];
                    port.PortClickAreaRect = TransposePortRect(node, port);
                }

                for (int i = 0; i < node.GetOutputPorts().Count; i++)
                {
                    var port = node.GetOutputPorts()[i];
                    port.PortClickAreaRect = TransposePortRect(node, port);
                }

                #endregion
            }
        }

    }
}