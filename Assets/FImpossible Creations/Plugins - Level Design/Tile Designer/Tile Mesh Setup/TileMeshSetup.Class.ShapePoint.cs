using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class TileMeshSetup
    {

        public struct MeshShapePoint
        {
            public Vector3 p;
            public Vector2 normal;
            public Color c;



            /// <summary>
            /// If using override normal
            /// </summary>
            internal bool QuickNormalCompute(CurvePoint sampleFocus)
            {
                if (sampleFocus.overrideNormal != Vector2.zero)
                {
                    normal = sampleFocus.overrideNormal;
                    normal.y = -normal.y;

                    if (sampleFocus.overrideNormal.magnitude < 1f)
                    {
                        normal *= sampleFocus.overrideNormal.magnitude;
                    }

                    return true;
                }

                return false;
            }

            internal void ComputeNormal(int i, float maxDivs, CurvePoint sampleFocus, CurvePoint sampleTarget, float progr, List<CurvePoint> points, float stepProgr, float splineLength)
            {
                bool quick = QuickNormalCompute(sampleFocus);
                if (quick) return;

                if (points.Count <= 1) return;

                stepProgr *= 0.1f;

                if (i == 0 || i == maxDivs - 1)
                {
                    if (i == 0)
                    {
                        Vector2 next = CurvePoint.GetPosAt(points, progr + stepProgr, splineLength);
                        Vector2 dir = sampleFocus.localPos - next;
                        normal = Vector2.Perpendicular(dir.normalized);
                    }
                    else
                    {
                        Vector2 pre = CurvePoint.GetPosAt(points, 1f - stepProgr, splineLength); //sampleTarget.localPos;
                        Vector2 dir = pre - CurvePoint.GetPosAt(points, 1f, splineLength);
                        normal = Vector2.Perpendicular(dir.normalized);
                    }
                }
                else
                {
                    Vector2 pre = CurvePoint.GetPosAt(points, progr - stepProgr, splineLength);
                    Vector2 next = CurvePoint.GetPosAt(points, progr + stepProgr, splineLength);

                    Vector2 dir = pre - next;
                    normal = Vector2.Perpendicular(dir.normalized);
                }
            }

        }



    }
}