using FIMSpace.Generating;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{

    public abstract partial class FGraphDrawerBase
    {

        void InputBegin_ClickSelectWithLMB()
        {
            if (FGenerators.CheckIfExist_NOTNULL(selectedNode)) InputEnd_MouseUp(Event.current);

            if (enteredOnNode == selectedNode)
            { }
            else // Unclick
            {
                selectedNode = null;
                FGraph_NodeBase_Drawer.SelectNodeSignal(null);
                refreshAfterSelecting = 5;
            }

            lastestDraggedNode = null;
            GUI.FocusControl("");
            graphAfterDrawToUse = Event.current;
            holdingLMB = true;
            isSelectingStarted = -1;
        }

        protected virtual void MMBClick()
        {

        }

        protected virtual void LMBFreeClick()
        {

        }

        protected void LMBUnclickPrevent()
        {
            wasDragging = false;
            GraphUnclickDrag();
            StopConnectingNodes();
            graphAfterDrawToUse = Event.current;
            holdingLMB = false;
        }

        private void InputBegin_DragWithLMB()
        {
            wasDragging = true;
        }

        private void InputBegin_UnclickLMB()
        {
            if (holdingLMB == false) return;

            if (wasDraggingNode)
                if (lastestDraggedNode != null)
                {
                    lastestDraggedNode.OnEndDrag();

                    CheckContainables();
                }

            wasDraggingNode = false;

            FGraph_TriggerNodeConnection enterOnConnection = null;

            if (wasDraggingNode == false && wasDragging == false)
                enterOnConnection = CursorOnConnection();

            if (!(enterOnConnection is null))
            {
                if (wasDraggingNode == false && wasDragging == false)
                {
                    if (latestClickedConnection == enterOnConnection)
                        latestClickedConnection = null;
                    else
                        latestClickedConnection = enterOnConnection;
                }
            }
            else
            {
                if (enteredOnNode is null) latestClickedConnection = null;
                lastestSelectedNode = null;
            }

            wasDragging = false;
            GraphUnclickDrag();
            StopConnectingNodes();
            holdingLMB = false;
            graphAfterDrawToUse = Event.current;
        }



        void InputActive_ZoomInOut(Event e)
        {
            var zoomDelta = 0.1f;
            zoomDelta = e.delta.y < 0 ? zoomDelta : -zoomDelta;

            float preZoom = graphZoom;

            graphZoom += zoomDelta * 0.4f;
            graphZoom = Mathf.Clamp(graphZoom, 0.25f, 1.5f);

            preZoom = graphZoom - preZoom; // Zoom Diff 

            // Zoom translate towards cursor position
            Vector2 toCursorCentered = eventMousePosOffsetted - graphDisplayRect.size / 2f;

            graphDisplayOffset -= (graphDisplayRect.size / 2f) * preZoom / graphZoom; // Centered Zoom
            graphDisplayOffset -= toCursorCentered * 0.1f * graphDisplayRect.size.magnitude * 0.01f * zoomDelta * 0.4f; // Zoom with translation towards cursor measure from display center
            repaintRequest = true;

            ClampGraphPosition();
            e.Use();
        }

        void InputActive_DragCanvasWithMiddleMouse(Event e)
        {
            Vector2 moveDelta = e.delta;
            graphDisplayOffset += moveDelta / graphZoom;
            performingGraphDrag = true;
            ClampGraphPosition();
            e.Use();
        }

        void InputActive_EndDragCanvas_MiddleMouseUp(Event e)
        {
            performingGraphDrag = false;

        }

        void InputActive_RightMouseUp(Event e)
        {
            var enteredConn = CursorOnConnection();

            if (enteredConn is null)
            {
                if (enteredOnNode is null)
                {
                    NodeAddMenu(e);
                    e.Use();
                }
            }
            else
            {
                ConnectionModifyMenu(e, enteredConn);
            }
        }

        void InputEnd_DragMouse(Event e)
        {
            if (isSelectingMultiple) GraphPerformSelectionFrame();
            if (wasResizing) UpdateResizingProcess();
            if (!wasResizing) if (!wasDraggingNode) if (holdingLMB) GraphStartSelectionFrame();
        }

        void InputEnd_MouseUp(Event e)
        {
            if (e.button != 2)
                if (isSelectingMultipleFinished || wasResizing)
                {
                    if (wasResizing) EndResizing();
                    isSelectingMultiple = false;
                    dragMultiSelected.Clear();
                    selectingMultipleRect = new Rect();
                    refreshAfterSelecting = 5;
                    ClearMultiSelectFrame();
                }

            isSelectingMultipleFinished = true;
        }

        void InputEnd_DeleteKey(Event e)
        {
            if (dragMultiSelected.Count > 0)
            {
                if (dragMultiSelected.Count > 1)
                {
                    if (EditorUtility.DisplayDialog("Removing Multiple Nodes", "Are you sure you want to remove " + dragMultiSelected.Count + " nodes from the graph?", "Yes, remove " + dragMultiSelected.Count + " nodes", "No"))
                    {
                        Undo.RecordObject(displayedGraphSetup, "GraphRemoveNodes");

                        for (int i = 0; i < dragMultiSelected.Count; i++)
                        {
                            if ((dragMultiSelected[i] is null) == false)
                                RemoveNode(dragMultiSelected[i]);
                        }

                        dragMultiSelected.Clear();
                        e.Use();
                    }
                }
                else
                {
                    if ((dragMultiSelected[0] is null) == false)
                    {
                        Undo.RecordObject(displayedGraphSetup, "GraphRemoveNodes");
                        RemoveNode(dragMultiSelected[0]);
                        dragMultiSelected.Clear();
                        selectedNode = null;
                        e.Use();
                    }
                }
            }
            else
            {
                var node = selectedNode;
                if (FGenerators.CheckIfExist_NOTNULL(node))
                {
                    if ((selectedNode is null) == false)
                    {
                        Undo.RecordObject(displayedGraphSetup, "GraphRemoveNodes");
                        RemoveNode(node);
                        wasRemovingNodes = true;
                        dragMultiSelected.Clear();
                        selectedNode = null;
                        e.Use();
                    }
                }

            }


            if (wasRemovingNodes == false)
                if (!(latestClickedConnection is null))
                {
                    Undo.RecordObject(displayedGraphSetup, "GraphRemoveNodes");
                    latestClickedConnection.From.RemoveConnectionWith(latestClickedConnection.To);
                    e.Use();
                }

        }

        void InputEnd_NodeSelectionTrigger()
        {
            if (nodeInput_clickedNode != null)
            {
                FGraph_NodeBase_Drawer.SelectNodeSignal(nodeInput_clickedNode);
                refreshAfterSelecting = 5;
                lastestSelectedNode = nodeInput_clickedNode;
                selectedNode = nodeInput_clickedNode;

                //if (selectedNode.IsContainable)
                //{
                //    if (selectedNode._EditorInContainedRange != null)
                //        if (selectedNode._EditorInContainedRange.Count > 0)
                //        {
                //            dragMultiSelected.Clear();

                //            for (int i = selectedNode._EditorInContainedRange.Count - 1; i >= 0; i--)
                //            {
                //                if (selectedNode._EditorInContainedRange[i] == null) { selectedNode._EditorInContainedRange.RemoveAt(i); continue; }
                //                dragMultiSelected.Add(selectedNode._EditorInContainedRange[i]);
                //            }
                //        }
                //}

                nodeInput_clickedNode = null;
            }
        }


        protected static List<FGraph_NodeBase> _RememberedNodesRefs = new List<FGraph_NodeBase>();

        /// <summary>
        /// Saved in _RememberedNodesRefs list
        /// Useful for copy-paste implementation
        /// </summary>
        void SaveReferencesToSelectedNodes()
        {
            _RememberedNodesRefs.Clear();

            for (int i = 0; i < dragMultiSelected.Count; i++)
            {
                _RememberedNodesRefs.Add(dragMultiSelected[i]);
            }

            if (selectedNode != null)
                if (!_RememberedNodesRefs.Contains(selectedNode))
                {
                    _RememberedNodesRefs.Add(selectedNode);
                }
        }

        protected virtual void Input_Copy(Event e)
        {
            SaveReferencesToSelectedNodes();
            e.Use();
        }

        protected virtual void Input_Paste(Event e)
        {
            Vector2 originPos = GetNodeCreatePos();

            // Fint most top left saved node
            Vector2 mostTopL = new Vector2(float.MaxValue, float.MaxValue);

            FGenerators.CheckForNulls(_RememberedNodesRefs);

            for (int i = 0; i < _RememberedNodesRefs.Count; i++)
            {
                Vector2 pos = _RememberedNodesRefs[i].NodePosition;
                if (pos.x < mostTopL.x && pos.y < mostTopL.y)
                {
                    mostTopL = pos;
                }
            }

            if (mostTopL.x == float.MaxValue) mostTopL = new Vector2();

            for (int i = 0; i < _RememberedNodesRefs.Count; i++)
            {
                var inst = ScriptableObject.Instantiate(_RememberedNodesRefs[i]);
                inst.ClearAfterPaste();
                inst.NodePosition = originPos + inst.NodePosition - mostTopL;
                AddNewNodeToPreset(inst, false);
            }

            _RememberedNodesRefs.Clear();
            e.Use();
        }

        void CheckContainables()
        {
            if (!_containablesPresent) return;
            if (lastestDraggedNode == null) return;

            FGraph_NodeBase containable = lastestDraggedNode;
            FGraph_NodeBase toBeContained = null;

            if (lastestDraggedNode.IsContainable) // Check other nodeds to include into containable
            {
                for (int i = 0; i < nodesToDraw.Count; i++)
                {
                    toBeContained = nodesToDraw[i];
                    if (toBeContained.IsContainable) continue;

                    if (toBeContained.IsContainedBy == containable)
                    {
                        CheckIfStillContained(toBeContained);
                    }
                    else
                    {
                        if (CheckIfContained(toBeContained, containable))
                        {
                            if (containable._EditorInContainedRange.Contains(toBeContained) == false)
                            {
                                containable._EditorInContainedRange.Add(toBeContained);
                                toBeContained.IsContainedBy = containable;
                            }
                        }
                    }

                }
            }
            else // Check if isn't dragged onto containable node
            {
                toBeContained = lastestDraggedNode;
                bool containedDone = false;

                for (int i = 0; i < nodesToDraw.Count; i++)
                {
                    if (!nodesToDraw[i].IsContainable) continue;
                    containable = nodesToDraw[i];

                    if (CheckIfContained(toBeContained, containable))
                    {
                        if (containable._EditorInContainedRange.Contains(toBeContained) == false)
                        {
                            containable._EditorInContainedRange.Add(toBeContained);
                            toBeContained.IsContainedBy = containable;
                            containedDone = true;
                            break;
                        }
                    }
                }

                if (!containedDone)
                {
                    CheckIfStillContained(toBeContained);
                }
            }

        }

        bool CheckIfContained(FGraph_NodeBase node, FGraph_NodeBase by)
        {
            if (node == null || by == null) return false;

            if (by._E_LatestRect.Contains(node._E_LatestRect.min))
                if (by._E_LatestRect.Contains(node._E_LatestRect.max)) return true;

            return false;
        }

        bool CheckIfStillContained(FGraph_NodeBase node)
        {
            if (node.IsContainedBy == null) return false;
            if (node.IsContainedBy._EditorFoldout == false) return true;

            if (CheckIfContained(node, node.IsContainedBy) == false)
            {
                if (node.IsContainedBy._EditorInContainedRange.Contains(node)) node.IsContainedBy._EditorInContainedRange.Remove(node);
                node.IsContainedBy = null;
            }
            else
                return true;

            return false;
        }

    }

}