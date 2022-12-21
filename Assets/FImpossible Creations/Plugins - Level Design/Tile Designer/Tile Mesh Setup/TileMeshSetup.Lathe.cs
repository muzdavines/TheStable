using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class TileMeshSetup
    {
        public int _lathe_fillAngle = 360;
        public int _lathe_xSubdivCount = 8;
        public float _lathe_ySubdivLimit = 30;

        public List<CurvePoint> _lathe_points = new List<CurvePoint>();


        void LatheQuickUpdate()
        {
            splineLength = 0f;

            for (int i = 0; i < _lathe_points.Count - 1; i++)
            {
                _lathe_points[i].distanceInSpline = splineLength;
                splineLength += Vector2.Distance(_lathe_points[i].localPos, _lathe_points[i + 1].localPos);
            }

            if (_lathe_points.Count > 1) _lathe_points[_lathe_points.Count - 1].distanceInSpline = splineLength;
            if (splineLength == 0f) splineLength = 0.1f;


            GenerateMeshShape(_lathe_points, splineLength, _lathe_ySubdivLimit, previewShape, SubdivMode);
        }



    }
}