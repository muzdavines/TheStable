using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    public abstract partial class FGraphDrawerBase
    {
        bool holdingLMB = false;

        int refreshAfterSelecting = 0;
        Vector2 preNodeDragPos = Vector2.zero;
        bool isSelectingMultiple = false;
        bool isSelectingMultipleFinished = false;
        int isSelectingStarted = 0;
        Vector2 selectingMultipleStart = Vector2.zero;
        Rect selectingMultipleRect = new Rect();
        Vector3[] selectionFrame = new Vector3[5];

        protected List<FGraph_NodeBase> dragMultiSelected = new List<FGraph_NodeBase>();

        void GraphUnclickDrag()
        {
            if (wasResizing) EndResizing();
            isSelectingMultiple = false;
            selectingMultipleRect = new Rect();
        }

        void GraphStartSelectionFrame()
        {
            if (isSelectingMultiple == false)
            {
                isSelectingStarted = 0;
                selectingMultipleStart = inGraphMousePos;

                dragMultiSelected.Clear();
                selectingMultipleRect = new Rect();
                isSelectingMultipleFinished = false;
            }

            isSelectingMultiple = true;
        }

        void ClearMultiSelectFrame()
        {
            for (int i = 0; i < selectionFrame.Length; i++) selectionFrame[i] = Vector2.zero;
        }

        void GraphPerformSelectionFrame()
        {
            if (isSelectingStarted < 7)
            {
                ClearMultiSelectFrame();
                return;
            }

            Vector2 mStart = selectingMultipleStart;
            Vector2 p = inGraphMousePos - selectingMultipleStart;

            selectionFrame[0] = mStart;
            selectionFrame[1] = mStart + new Vector2(p.x, 0);
            selectionFrame[2] = mStart + new Vector2(p.x, p.y);
            selectionFrame[3] = mStart + new Vector2(0, p.y);
            selectionFrame[4] = mStart;

            selectingMultipleRect = new Rect();
            Vector2 toMouse = inGraphMousePos - selectingMultipleStart;

            if (toMouse.x > 0)
                selectingMultipleRect.position = new Vector2(mStart.x, selectingMultipleRect.y);
            else
                selectingMultipleRect.position = new Vector2(inGraphMousePos.x, selectingMultipleRect.y);

            if (toMouse.y > 0)
                selectingMultipleRect.position = new Vector2(selectingMultipleRect.x, mStart.y);
            else
                selectingMultipleRect.position = new Vector2(selectingMultipleRect.x, inGraphMousePos.y);

            selectingMultipleRect.size = new Vector2(Mathf.Abs(toMouse.x), Mathf.Abs(toMouse.y));
        }


        void DrawGraphSelectionFrame()
        {
            if (isSelectingMultiple)
            {
                if (isSelectingStarted < 8) // To prevent frame flicker on drag start
                {
                    isSelectingStarted += 1;
                    return;
                }

                Color c = new Color(1f, .8f, .1f, 0.65f);
                Handles.color = c;
                Handles.DrawAAPolyLine(4, 5, selectionFrame);

                GUI.Box(selectingMultipleRect, GUIContent.none, EditorStyles.helpBox);
                Handles.color = Color.white;
            }
        }


        private void HighlightSelectedNodes()
        {
            GUI.color = new Color(1f, 0.7f, 0.1f, 0.5f);

            try
            {

                for (int d = dragMultiSelected.Count - 1; d >= 0; d--)
                {
                    if (dragMultiSelected[d] == null)
                    {
                        dragMultiSelected.RemoveAt(d);
                        continue;
                    }

                    Rect r = dragMultiSelected[d].Drawer(this).GetFrameBodyHighlightMultiSelectedRect(dragMultiSelected[d]._E_LatestRect);
                    GUI.Box(r, GUIContent.none, dragMultiSelected[d].Drawer(this).GetFrameBodyHighlightStyle);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                dragMultiSelected.Clear();
            }

            GUI.color = Color.white;
        }


        private void TranslateSelectedNodes()
        {
            if (lastestDraggedNode != null)
            {
                if (preNodeDragPos != Vector2.zero)
                {
                    if (dragMultiSelected.Contains(lastestDraggedNode))
                    {
                        Vector2 offset = lastestDraggedNode._E_LatestRect.position - preNodeDragPos;

                        for (int i = 0; i < dragMultiSelected.Count; i++)
                        {
                            if (dragMultiSelected[i] == lastestDraggedNode) continue;
                            dragMultiSelected[i].NodePosition += offset;
                        }

                        return;
                    }
                }

                // Containable movement
                if (lastestDraggedNode.IsContainable)
                {
                    if (preNodeDragPos != Vector2.zero)
                    {
                        Vector2 offset = lastestDraggedNode._E_LatestRect.position - preNodeDragPos;

                        if (offset != Vector2.zero)
                        {
                            if (lastestDraggedNode._EditorFoldout)
                            {
                                for (int i = 0; i < lastestDraggedNode._EditorInContainedRange.Count; i++)
                                {
                                    if (lastestDraggedNode._EditorInContainedRange[i] == lastestDraggedNode) continue;
                                    lastestDraggedNode._EditorInContainedRange[i].NodePosition += offset;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < lastestDraggedNode._EditorInContainedRange.Count; i++)
                                {
                                    if (lastestDraggedNode._EditorInContainedRange[i] == lastestDraggedNode) continue;

                                    lastestDraggedNode._EditorInContainedRange[i].NodePosition += offset;
                                    //lastestDraggedNode._EditorInContainedRange[i].baseSerializedObject.ApplyModifiedProperties();
                                    EditorUtility.SetDirty(lastestDraggedNode._EditorInContainedRange[i]);
                                }
                            }
                        }

                    }
                }

            }
        }

        void EndResizing()
        {
            wasResizing = false;
        }

        void UpdateResizingProcess()
        {
            Vector2 diff = inGraphMousePos - startResizePos;

            if (lastResizedNode != null)
            {
                lastResizedNode.ResizedScale = startResizeScale + diff;
                if (lastResizedNode.ResizedScale.x < 140) lastResizedNode.ResizedScale.x = 140;
                if (lastResizedNode.ResizedScale.y < 60) lastResizedNode.ResizedScale.y = 60;
            }
        }

    }
}