using FIMSpace.Generating;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{

    public abstract partial class FGraphDrawerBase
    {
        protected bool performingGraphDrag = false;
        /// <summary> Walkaround for event keys pressed detection </summary>
        protected List<KeyCode> pressedKeys = new List<KeyCode>();
        private void SetKeyPressed(KeyCode k) { if (k == KeyCode.None) return; if (pressedKeys.Contains(k) == false) pressedKeys.Add(k); }
        Event cursorOutOfGraphEventForward = null;

        /// <summary> Deselecting, stopping connecting nodes etc. </summary>
        protected virtual void BeginDrawInputs()
        {
            nodeInput_clickedNode = null;

            if (isMouseCursorInGraph == false)
            {
                return;
            }

            if (Event.current.type == EventType.MouseDown)
            {
                SetKeyPressed(Event.current.keyCode);
                if (Event.current.button == 0)
                {
                    InputBegin_ClickSelectWithLMB();
                }
            }

            else if (Event.current.type == EventType.MouseUp)
            {
                pressedKeys.Clear();
                if (Event.current.button == 0)
                {
                    InputBegin_UnclickLMB();
                }
            }

            else if (Event.current.type == EventType.MouseDrag)
            {
                pressedKeys.Clear();
                if (Event.current.button == 0)
                {
                    InputBegin_DragWithLMB();
                }
            }

        }


        protected virtual void UpdateGraphInput()
        {
            var e = Event.current;

            if (e.type == EventType.ScrollWheel) // Zoom in/out
            {
                pressedKeys.Clear();
                InputActive_ZoomInOut(e);
            }
            else
            {
                // Middle Mouse Button Events
                if (e.button == 2 ) // Third button
                {
                    if (e.type == EventType.MouseDrag)
                    {
                        pressedKeys.Clear();
                        InputActive_DragCanvasWithMiddleMouse(e);
                    }
                    else if (e.type == EventType.MouseUp)
                    {
                        pressedKeys.Clear();

                        if (e.button == 2)
                            InputActive_EndDragCanvas_MiddleMouseUp(e);
                    }
                }
                // R Mouse Button Events
                else if (e.button == 1)
                {
                    if (isMouseCursorInGraph == false) return;
                    if (e.type == EventType.MouseUp) InputActive_RightMouseUp(e);
                }
                else // Input arrows graph view movement
                {

                    if (pressedKeys.Contains(KeyCode.RightArrow))
                    {
                        graphDisplayOffset -= new Vector2(2, 0);
                        wasDragging = true;
                        //ClampGraphPosition(true);
                    }
                    else if (pressedKeys.Contains(KeyCode.LeftArrow))
                    {
                        graphDisplayOffset += new Vector2(2, 0);
                        wasDragging = true;
                        //ClampGraphPosition(true);
                    }

                    if (pressedKeys.Contains(KeyCode.UpArrow))
                    {
                        graphDisplayOffset += new Vector2(0, 2);
                        wasDragging = true;
                        //ClampGraphPosition(true);
                    }
                    else if (pressedKeys.Contains(KeyCode.DownArrow))
                    {
                        graphDisplayOffset -= new Vector2(0, 2);
                        wasDragging = true;
                        //ClampGraphPosition(true);
                    }

                }
            }

            // Center graph position at first draw complete
            if (drawingNodes != null)
                if (drawingNodes.Count > 0)
                    if (wasFirstDraw == 1)
                    {
                        wasFirstDraw = 2;
                        ResetGraphPosition();
                    }
                    else
                        wasFirstDraw += 1;

        }


        protected virtual void EndDrawInputs()
        {
            InputEnd_NodeSelectionTrigger();

            if (isMouseCursorInGraph == false) return;

            Event e = Event.current;
            if (e == null) return;

            if (e.type == EventType.MouseDrag)
            {
                pressedKeys.Clear();
                InputEnd_DragMouse(e);
            }
            else if (e.type == EventType.MouseUp)
            {
                pressedKeys.Clear();

                //if (!(wasDragging && e.button > 0))
                    InputEnd_MouseUp(e);
            }
            else if (e.type == EventType.KeyDown) // Key Downs
            {
                SetKeyPressed(Event.current.keyCode);

                if (Event.current.keyCode == KeyCode.Delete) // Delete nodes input 
                {
                    InputEnd_DeleteKey(e);
                }
                else if (Event.current.control)
                {
                    if (Event.current.keyCode == KeyCode.C)
                    {
                        Input_Copy(e);
                    }
                    else if (Event.current.keyCode == KeyCode.V)
                    {
                        Input_Paste(e);
                    }
                }

            }
            else if (e.type == EventType.MouseDown)
            {
                if (e.button == 0)
                {
                    if (FGenerators.CheckIfIsNull(enteredOnNode))
                    {
                        LMBFreeClick();
                    }
                }
                else if (e.button == 2)
                {
                    MMBClick();
                    Input_Utils_CheckDoubleMMB(e);
                }
            }
            else if (e.type == EventType.KeyUp)
            {
                if (e.isMouse == false) pressedKeys.Clear();
            }

            // Execute selecting / deselecting events
            if (Event.current != null) if (graphAfterDrawToUse != null) graphAfterDrawToUse.Use();
            if ((enteredOnNode is null) == false) wasCheckingEnter = true;

        }


        #region Utilities

        protected Event graphStartDrawEvent;
        protected Vector2 graphStartDrawCursorPos = Vector2.zero;

        public FGraph_TriggerNodeConnection CursorOnConnection()
        {
            if (drawingNodes == null) return null;

            for (int i = 0; i < drawingNodes.Count; i++)
            {
                for (int c = 0; c < drawingNodes[i].OutputConnections.Count; c++)
                {
                    var conn = drawingNodes[i].OutputConnections[c];
                    if (conn is null) continue;
                    if (conn.From is null) continue;
                    if (conn.To is null) continue;

                    Vector2 a = conn.From.Drawer(this)._E_LatestOutputRect.center;
                    Vector2 b = conn.To.Drawer(this)._E_LatestInputRect.center;
                    Vector2 atob = b - a;

                    Vector2 bounds = new Vector2(32, 24);
                    Vector2 boundsOff = -bounds * 0.5f;

                    float moveMagn = atob.magnitude * 0.1f;
                    if (moveMagn > 10) moveMagn = 10;
                    Vector2 toOff = atob.normalized * moveMagn;

                    Rect r = new Rect(a + boundsOff, bounds);
                    r.position += toOff;
                    if (r.Contains(inGraphMousePos)) return conn;

                    r = new Rect(b + boundsOff, bounds);
                    r.position -= toOff;
                    if (r.Contains(inGraphMousePos)) return conn;

                    bounds = new Vector2(6 + Mathf.Min(40, Mathf.Abs(atob.x)), 6 + Mathf.Min(32, Mathf.Abs(atob.y)));
                    boundsOff = -bounds * 0.5f;
                    r = new Rect(Vector2.LerpUnclamped(a, b, 0.5f) + boundsOff, bounds);

                    if (r.Contains(inGraphMousePos)) return conn;
                }
            }

            return null;
        }

        protected Vector3 GetNodeCreatePos()
        {
            Vector2 mPos = new Vector2(-graphDisplayOffset.x + 220, -graphDisplayOffset.y + 200);

#if UNITY_EDITOR

            if (graphStartDrawEvent != null)
            {
                mPos = graphStartDrawCursorPos / graphZoom;
                mPos -= graphDisplayOffset;
                mPos -= new Vector2(0f, TopMarginOffset) / graphZoom;
                mPos -= new Vector2(56, 30);
            }
#endif

            return mPos;
        }

        void Input_Utils_CheckDoubleMMB(Event e)
        {
            if (e.button != 2) return;

            if (EditorApplication.timeSinceStartup - _editorDoubleClickMMBHelper < 0.3) DoubleMMBClick(e);

            _editorDoubleClickMMBHelper = EditorApplication.timeSinceStartup;

            if (!isMouseCursorInGraph) Event.current.Use();
        }


        #endregion


    }

}