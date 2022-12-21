using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class TileMeshSetup
    {
        [System.Serializable]
        public class CurvePoint
        {
            public static readonly float _size = 14f;
            public float distanceInSpline = 0f;

            public enum EPointMode { Linear, Auto, Manual }
            public EPointMode Mode = EPointMode.Linear;
            public Color VertexColor = Color.white;
            public float VertexColorFalloff = 1f;
            //public bool AlwaysInclude = true;


            public Vector2 overrideNormal = Vector2.zero;
            [NonSerialized] public bool wasDrag = false;
            public int index = 0;
            public Rect rect;

            [SerializeField] public Vector2 localPos = Vector2.zero;
            [SerializeField] public Vector2 localInTan = Vector2.zero;
            [SerializeField] public Vector2 localNextTan = Vector2.zero;


            [NonSerialized] public CurvePoint next = null;
            [NonSerialized] public CurvePoint pre = null;


            public float _extra_z = 0f;
            public bool _Loft_Height_ShiftWhole = false;

            public Vector2 inTan
            {
                get { return ToCanv(_latestEditorDisplayRect, localInTan) - _latestEditorDisplayRect.position; }
                set { localInTan = FromCanv(_latestEditorDisplayRect, value + _latestEditorDisplayRect.position); }
            }

            public Vector2 nextTan
            {
                get { return ToCanv(_latestEditorDisplayRect, localNextTan) - _latestEditorDisplayRect.position; }
                set { localNextTan = FromCanv(_latestEditorDisplayRect, value + _latestEditorDisplayRect.position); }
            }

            public Vector2 rPos
            {
                get { RefreshRectPos(); return rect.position; }
                set { rect.position = value; SyncLocalPos(); }
            }

            public static void CopyListFromTo(List<CurvePoint> from, List<CurvePoint> to)
            {
                to.Clear();
                for (int i = 0; i < from.Count; i++)
                {
                    to.Add(from[i].Copy());
                }
            }

            public CurvePoint Copy()
            {
                return (CurvePoint)MemberwiseClone();
            }

            /// <summary> Rect pos without refreshing </summary>
            public Vector2 DisplayPos
            {
                get { return rect.position; }
                //set { rect.position = value; SyncLocalPos(); }
            }

            public float AutoFactor = .7f;

            public Rect GetRect(Rect display)
            {
                return new Rect(ToCanv(display, localPos), rect.size);
            }

            public void SetPos(Rect display, Vector2 canvasPos)
            {
                if (display.width < 5 || display.height < 5) return;
                localPos = FromCanv(display, canvasPos);
            }

            /// <summary> From 01 pos to world pos </summary>
            public Vector2 ToCanv(Rect displayRect, Vector2 pos)
            {
                return (displayRect.position - Vector2.one * 7) + Vector2.Scale(displayRect.size, pos);
            }

            /// <summary> From world pos to 01 pos </summary>
            public Vector2 FromCanv(Rect displayRect, Vector2 pos)
            {
                Vector3 ps = pos - (displayRect.position - Vector2.one * 7);
                return new Vector2((ps.x) / (displayRect.width), (ps.y) / (displayRect.height));
            }


            public CurvePoint(float x, float y)
            {
                localPos = FromCanv(_latestEditorDisplayRect, new Vector2(x, y));
                localInTan = Vector2.zero;
                localNextTan = Vector2.zero;
                overrideNormal = Vector2.zero;

                rect = new Rect(x, y, _size, _size);
            }

            void ClampLocalPos()
            {
                localPos.x = Mathf.Clamp(localPos.x, 0f, 1f);
                localPos.y = Mathf.Clamp(localPos.y, 0f, 1f);
            }

            public CurvePoint(float x, float y, bool local)
            {
                if (local)
                    localPos = new Vector2(x, y);
                else
                    ToCanv(_latestEditorDisplayRect, new Vector2(x, y));

                rect = new Rect(x, y, _size, _size);
            }

            public CurvePoint(Vector2 pos) : this(pos.x, pos.y) { }
            public CurvePoint(Vector2 pos, bool local) : this(pos.x, pos.y, local) { }


            public void Update(int index, CurvePoint prePoint, CurvePoint nextPoint)
            {
                this.index = index;
                pre = prePoint;
                next = nextPoint;

                if (Mode == EPointMode.Linear)
                {
                    if (nextPoint != null)
                    {
                        Vector2 dir = nextPoint.rPos - rect.position;
                        Vector2 dirN = dir.normalized;
                        inTan = dirN;
                        nextTan = -dirN;
                    }
                }
                else if (Mode == EPointMode.Auto)
                {
                    if (pre == null)
                    {
                        Vector2 dir = nextPoint.rPos - rect.position;
                        Vector2 dirN = dir.normalized;
                        inTan = -dirN * AutoFactor;
                        nextTan = -nextPoint.inTan * AutoFactor;
                    }
                    else if (next == null)
                    {
                        Vector2 dir = prePoint.rPos - rect.position;
                        Vector2 dirN = dir.normalized;
                        inTan = -prePoint.nextTan * AutoFactor;
                        nextTan = dirN * AutoFactor;
                    }
                    else
                    {
                        Vector2 preDir = prePoint.rPos - rect.position;
                        //Vector2 preDirN = preDir.normalized;
                        Vector2 nxtDir = nextPoint.rPos - rect.position;
                        Vector2 nxtDirN = nxtDir.normalized;
                        Vector2 fromTo = prePoint.rPos - nextPoint.rPos;
                        //Vector2 fromToN = fromTo.normalized;

                        Vector2 nextTanVal = nxtDirN * (fromTo.magnitude * 1f * AutoFactor);
                        inTan = Vector2.LerpUnclamped(pre.nextTan, nextTanVal, 0.5f);
                        nextTan = -nextPoint.inTan.normalized * nxtDir.magnitude * 0.5f * AutoFactor;
                    }
                }
                else if (Mode == EPointMode.Manual)
                {

                }
            }

            public void SyncLocalPos(Vector2 rectPos)
            {
                SetPos(_latestEditorDisplayRect, rectPos);
            }
            public void SyncLocalPos()
            {
                SetPos(_latestEditorDisplayRect, rect.position);
                ClampLocalPos();
            }

            public void RefreshRectPos()
            {
                rect.position = ToCanv(_latestEditorDisplayRect, localPos);
            }

            internal Vector2 GetPositionTowards(CurvePoint p2, float t)
            {
                float omT = 1f - t;

                if (Mode == EPointMode.Linear && p2.Mode == EPointMode.Linear)
                {
                    return Vector3.LerpUnclamped(localPos, p2.localPos, t);
                }

                Vector2 inTan = localPos + localInTan;
                Vector2 nextTan = p2.localPos + localNextTan;

                // Layer 1
                Vector3 Q = omT * localPos + t * inTan;
                Vector3 R = omT * inTan + t * nextTan;
                Vector3 S = omT * nextTan + t * p2.localPos;

                // Layer 2
                Vector3 P = omT * Q + t * R;
                Vector3 T = omT * R + t * S;

                // Final interpolated position
                return omT * P + t * T;
            }

            internal static CurvePoint GetPointAt(List<CurvePoint> curve, float time, float splineLength, bool getPre = false)
            {
                return curve[GetIndexAt(curve, time, splineLength, getPre)];
            }

            internal static int GetIndexAt(List<CurvePoint> curve, float time, float splineLength, bool getPre = false)
            {
                if (curve.Count == 0) return 0;

                float distInTime = Mathf.Lerp(0f, splineLength, time);
                int p2i = 0;

                for (int i = 0; i < curve.Count; i++)
                {
                    p2i = i;
                    var p2 = curve[i];

                    if (p2.distanceInSpline >= distInTime)
                    {
                        break;
                    }
                }

                if (getPre) return Mathf.Max(0, p2i - 1);
                return p2i;
            }

            public static int GetCurvePointIndex(CurvePoint p, List<CurvePoint> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (p == list[i])
                    {
                        p.index = i;
                        return i;
                    }
                }

                return 0;
            }


            public static Rect _latestEditorDisplayRect = new Rect(0, 0, 100, 100);
            public static CurvePoint _GetPosAt_Origin;
            public static CurvePoint _GetPosAt_Other;

            public static Vector2 GetPosAt(List<CurvePoint> points, float time, float splineLength)
            {
                if (points.Count == 0) return Vector2.zero;

                float distInTime = Mathf.Lerp(0f, splineLength, time);

                CurvePoint p1 = points[0];
                CurvePoint p2 = points[0];

                for (int i = 0; i < points.Count; i++)
                {
                    p2 = points[i];

                    if (p2.distanceInSpline >= distInTime)
                    {
                        if (p2.distanceInSpline > distInTime)
                        {
                            if (i - 1 > 0) p1 = points[i - 1];
                            break;
                        }
                    }
                }

                float time1 = p1.distanceInSpline / splineLength;
                float time2 = p2.distanceInSpline / splineLength;

                float abProgr = Mathf.InverseLerp(time1, time2, time);

                if (time > 0.5f)
                {
                    _GetPosAt_Origin = p2;
                    _GetPosAt_Other = p1;
                }
                else
                {
                    _GetPosAt_Origin = p1;
                    _GetPosAt_Other = p2;
                }

                return p1.GetPositionTowards(p2, abProgr);
            }


            public static float GetZAt(List<CurvePoint> points, float time, float splineLength)
            {
                if (points.Count == 0) return 0f;

                float distInTime = Mathf.Lerp(0f, splineLength, time);

                CurvePoint p1 = points[0];
                CurvePoint p2 = points[0];

                for (int i = 0; i < points.Count; i++)
                {
                    p2 = points[i];

                    if (p2.distanceInSpline >= distInTime)
                    {
                        if (p2.distanceInSpline > distInTime)
                        {
                            if (i - 1 > 0) p1 = points[i - 1];
                            break;
                        }
                    }
                }

                float time1 = p1.distanceInSpline / splineLength;
                float time2 = p2.distanceInSpline / splineLength;

                float abProgr = Mathf.InverseLerp(time1, time2, time);

                if (time > 0.5f)
                {
                    _GetPosAt_Origin = p2;
                    _GetPosAt_Other = p1;
                }
                else
                {
                    _GetPosAt_Origin = p1;
                    _GetPosAt_Other = p2;
                }

                return Mathf.Lerp(p1._extra_z, p2._extra_z, abProgr);
            }


        }


    }
}