#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using FIMSpace.FEditor;

using static FIMSpace.Generating.TileMeshSetup;

namespace FIMSpace.Generating
{
    public partial class TileDesignerWindow
    {
        TileMeshSetup.CurvePoint enteredPoint = null;


        void EndDraggingTangent(Event e)
        {
            if (e.type == EventType.MouseUp)
            {
                if (e.button == 0)
                    shapeEndChanging = true;
            }
        }

        bool _displayVertexColor = false;
        void DrawCurveHandle(int id)
        {
            _displayVertexColor = true;
            _latestEditorDisplayRect = _editorDisplayRect1;

            if (FLogicMethods.ContainsIndex(_editorPoints, id))
            {
                HandleDraw(_editorPoints[id]);
            }

            GUI.DragWindow();
        }

        void DrawCurveHandle2(int id)
        {
            _displayVertexColor = false;
            _latestEditorDisplayRect = _editorDisplayRect2;

            if (FLogicMethods.ContainsIndex(_editorPoints2, id - (_editorPoints.Count)))
            {
                HandleDraw(_editorPoints2[id - (_editorPoints.Count)]);
            }

            GUI.DragWindow();
        }


        void HandleDraw(TileMeshSetup.CurvePoint p)
        {
            p.RefreshRectPos();

            Event e = Event.current;
            if (e != null)
            {
                //if (p.index == 0) UnityEngine.Debug.Log("rect " + p.rect.position + " mouse " + windowMousePos + " edRect " + _latestEditorDisplayRect.position + " editorRect " + _editorRect.position);

                Rect trRect = p.rect; //_latestEditorDisplayRect;
                trRect.position += _editorRect.position;

                if (isSelectingMultiple == false)
                {
                    if (trRect.Contains(windowMousePos))
                    {
                        enteredPoint = p;
                    }
                    else
                    {
                        if (enteredPoint == p) enteredPoint = null;
                    }
                }

                if (e.type == EventType.MouseDown)
                {
                    if (e.button == 0)
                    {
                        selectedPoint = p;
                        if (_displayVertexColor) selectedPointListOwner = 1; else selectedPointListOwner = -1;
                        _latestEditorDisplayRectSelected = _latestEditorDisplayRect;
                    }
                    else if (e.button == 1)
                    {
                        if (selectedPoint == p)
                        {
                            if (p.Mode == TileMeshSetup.CurvePoint.EPointMode.Auto) p.Mode = TileMeshSetup.CurvePoint.EPointMode.Manual;
                            else p.Mode = TileMeshSetup.CurvePoint.EPointMode.Auto;

                            e.Use();
                        }
                    }
                }
                else
                {
                    if (e.type == EventType.MouseDrag)
                    {
                        lastestDraggedNode = p;
                    }
                }

                if (e.type == EventType.MouseUp)
                {
                    if (e.button == 0)
                        shapeEndChanging = true;
                }
            }

        }


        void DrawTanHandle(Rect r, CurvePoint p, GUIDragInfo.EDrag type)
        {
            if (p == null) return;
            if (p.Mode != CurvePoint.EPointMode.Manual) return;

            Color preH = Handles.color;
            Handles.color = new Color(1f, 1f, 1f, 0.5f);

            if (EditorGUIUtility.isProSkin == false) Handles.color = new Color(.3f, .3f, .3f, 0.5f);

            Vector2 scale = new Vector2(8f, 8f);
            Vector2 s = p.rect.size / 2f;

            if (type == GUIDragInfo.EDrag.InTan)
            {
                if (p.next == null) return;

                Vector2 nPos = p.rPos + p.inTan;
                Handles.DrawAAPolyLine(GetRPos(r, p.rPos + s, true), GetRPos(r, nPos + s, true));

                Rect tanR = new Rect(GetRPos(r, nPos + s - scale / 2f, true), scale);
                HandleTangentDrag(r, p, tanR, GUIDragInfo.EDrag.InTan);

                GUI.Box(tanR, GUIContent.none, FGUI_Resources.BGInBoxStyleH);
            }
            else if (type == GUIDragInfo.EDrag.NextTan)
            {
                if (p.next == null) return;

                Vector2 nPos = p.next.rPos + p.nextTan;
                Handles.DrawAAPolyLine(GetRPos(r, p.next.rPos + s, true), GetRPos(r, nPos + s, true));

                Rect tanR = new Rect(GetRPos(r, nPos + s - scale / 2f, true), scale);
                HandleTangentDrag(r, p, tanR, GUIDragInfo.EDrag.NextTan);

                GUI.Box(tanR, GUIContent.none, FGUI_Resources.BGInBoxStyleH);
            }

            Handles.color = preH;
        }



        void HandleTangentDrag(Rect r, CurvePoint p, Rect tanR, GUIDragInfo.EDrag type)
        {

            Event e = Event.current;
            if (e != null)
            {
                if (e.type == EventType.MouseDown)
                {
                    Rect dragR = tanR;
                    dragR.size *= 1.65f;
                    if (tanR.Contains(e.mousePosition))
                    {
                        _dragInfo = new GUIDragInfo(p, type, GetRPos(r, e.mousePosition, true));
                    }
                }
                else if (e.type == EventType.MouseDrag)
                {
                    if (_dragInfo != null)
                        if (_dragInfo.owner == p)
                        {
                            if (_dragInfo.UpdateDragPos(r, GetRPos(r, e.mousePosition, true))) shapeChanged = true;
                            repaintRequest = true;
                            e.Use();
                        }
                }
                else if (e.type == EventType.MouseUp)
                {
                    if (_dragInfo != null)
                        if (_dragInfo.owner == p)
                        {
                            _dragInfo = null;
                            EndDraggingTangent(e);
                            repaintRequest = true;
                        }
                }
            }
        }


    }
}
#endif
