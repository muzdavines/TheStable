#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using static FIMSpace.Generating.TileMeshSetup;

namespace FIMSpace.Generating
{
    public partial class TileDesignerWindow
    {

        void AddNewPointInCanvasPos(Rect displayArea, List<CurvePoint> list, Event e)
        {
            Vector2 pos = e.mousePosition - displayArea.position;
            CurvePoint near = FindNearestInCanvasPoint(pos, list);

            if (near == null)
            {
                list.Add(new CurvePoint(pos));
            }
            else
            {
                CurvePoint nPoint = new CurvePoint(pos);
                nPoint.Mode = near.Mode;
                nPoint.VertexColorFalloff = near.VertexColorFalloff;
                nPoint.VertexColor = near.VertexColor;
                nPoint.AutoFactor = near.AutoFactor;
                int ind = CurvePoint.GetCurvePointIndex(near, list);


                float dotNext = 0f, dotPre = 0f;

                #region Dot directions compute

                if (ind > 0)
                {
                    dotPre = Vector2.Dot((pos - list[ind].DisplayPos).normalized,
                        (list[ind - 1].DisplayPos - list[ind].DisplayPos).normalized
                        );
                }

                if (ind < list.Count - 1)
                {
                    dotNext = Vector2.Dot((pos - list[ind].DisplayPos).normalized,
                        (list[ind + 1].DisplayPos - list[ind].DisplayPos).normalized
                        );
                }

                #endregion

                //UnityEngine.Debug.Log("dotNext = " + dotNext + " dotPre = " + dotPre);

                if (ind == list.Count - 1)
                {
                    if (dotNext == 0f && dotPre < 0.0f)
                        list.Add(nPoint);
                    else
                        list.Insert(ind, nPoint);
                }
                else if (ind == 0)
                {
                    if (dotPre == 0f && dotNext < 0.0f)
                        list.Insert(0, nPoint);
                    else
                        list.Insert(ind + 1, nPoint);
                }
                else
                {
                    if (dotNext > 0f)
                        list.Insert(CurvePoint.GetCurvePointIndex(near, list) + 1, nPoint);
                    else
                        list.Insert(CurvePoint.GetCurvePointIndex(near, list), nPoint);
                }
            }

            repaintRequest = true;
        }


        CurvePoint FindNearestInCanvasPoint(Vector2 pos, List<CurvePoint> list)
        {
            if (list.Count == 0) return null;

            CurvePoint nearest = list[0];
            float nDist = float.MaxValue;

            for (int i = 0; i < list.Count; i++)
            {
                var p = list[i];
                float dist = Vector2.Distance(pos, p.DisplayPos);

                if (dist < nDist)
                {
                    nDist = dist;
                    nearest = p;
                }
            }

            return nearest;
        }


        void DrawCurves(Rect r, List<CurvePoint> curvePoints, bool mirror = false, bool secondary = false, bool loop = false)
        {
            if (!secondary)
                _latestEditorDisplayRect = _editorDisplayRect1;
            else
                _latestEditorDisplayRect = _editorDisplayRect2;

            for (int i = 0; i < curvePoints.Count; i++)
            {
                var p = curvePoints[i];
                if (p.next != null) DrawCurve(r, p, p.next, mirror);

                if (mirror) continue;

                if (p.Mode != CurvePoint.EPointMode.Linear)
                    if (selectedPoint == p)
                    {
                        DrawTanHandle(r, p, GUIDragInfo.EDrag.InTan);
                        DrawTanHandle(r, p.pre, GUIDragInfo.EDrag.NextTan); // self back

                        if (p.pre == null)
                        {
                            DrawTanHandle(r, p.pre, GUIDragInfo.EDrag.InTan);
                            DrawTanHandle(r, p, GUIDragInfo.EDrag.NextTan);
                        }

                        if (p.next == null)
                        {
                            DrawTanHandle(r, p.pre, GUIDragInfo.EDrag.InTan);
                        }
                    }
            }


            if ( loop)
            {
                if ( curvePoints.Count > 2)
                DrawCurve(r, curvePoints[0], curvePoints[curvePoints.Count-1], false);
            }
        }


        void DrawCurve(Rect r, CurvePoint p1, CurvePoint p2, bool mirror = false)
        {
            Vector2 start, end;
            Vector2 s = p1.rect.size / 2f;
            Color col = _curveCol;

            Vector2 inT = p1.inTan;
            Vector2 nxtT = p1.nextTan;

            if (!mirror)
            {
                start = GetRPos(r, p1.rect.x, p1.rect.y, false);
                end = GetRPos(r, p2.rect.x, p2.rect.y, false);
            }
            else
            {
                start = GetRPos(r, (r.x + r.width) - p1.rect.x, p1.rect.y, false);
                end = GetRPos(r, (r.x + r.width) - p2.rect.x, p2.rect.y, false);
                s.x = -s.x;
                col *= 0.8f;

                inT.x = -(inT.x);
                nxtT.x = -(nxtT.x);
            }

            s -= r.position;

            start += s;
            end += s;

            Handles.DrawBezier(start, end, start + inT, end + nxtT, col, null, 4);
        }

    }
}
#endif