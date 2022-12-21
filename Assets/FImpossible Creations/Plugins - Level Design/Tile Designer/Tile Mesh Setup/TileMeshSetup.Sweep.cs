using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class TileMeshSetup
    {

        public List<CurvePoint> _sweep_path = new List<CurvePoint>();
        public List<CurvePoint> _sweep_shape = new List<CurvePoint>();
        public List<CurvePoint> _sweep_radius = new List<CurvePoint>();
        public float _sweep_radiusMul = 0.5f;
        public bool _sweep_Close = true;
        public float _sweep_shapeSubdivLimit = 30;
        public float _sweep_distribSubdivLimit = 30f;

        void SweepQuickUpdate()
        {

            splineLength = 0f;
            for (int i = 0; i < _sweep_path.Count - 1; i++)
            {
                _sweep_path[i].distanceInSpline = splineLength;
                splineLength += Vector2.Distance(_sweep_path[i].localPos, _sweep_path[i + 1].localPos);
            }

            if (_sweep_path.Count > 1) _sweep_path[_sweep_path.Count - 1].distanceInSpline = splineLength;
            if (splineLength == 0f) splineLength = 0.1f;

            GenerateMeshShape(_sweep_path, splineLength, _sweep_distribSubdivLimit, previewShape, SubdivMode);


            splineLength2 = 0f;
            for (int i = 0; i < _sweep_shape.Count - 1; i++)
            {
                _sweep_shape[i].distanceInSpline = splineLength2;
                splineLength2 += Vector2.Distance(_sweep_shape[i].localPos, _sweep_shape[i + 1].localPos);
            }

            if (_sweep_shape.Count > 1) _sweep_shape[_sweep_shape.Count - 1].distanceInSpline = splineLength2;
            if (splineLength2 == 0f) splineLength2 = 0.1f;


            GenerateMeshShape(_sweep_shape, splineLength2, _sweep_shapeSubdivLimit, previewShape2, SubdivMode);


            splineLength3 = 0f;
            for (int i = 0; i < _sweep_radius.Count - 1; i++)
            {
                _sweep_radius[i].distanceInSpline = splineLength3;
                splineLength3 += Vector2.Distance(_sweep_radius[i].localPos, _sweep_radius[i + 1].localPos);
            }

            if (_sweep_radius.Count > 1) _sweep_radius[_sweep_radius.Count - 1].distanceInSpline = splineLength3;
            if (splineLength3 == 0f) splineLength3 = 0.1f;


            GenerateMeshShape(_sweep_radius, splineLength3, _sweep_distribSubdivLimit, previewShape3, SubdivMode);
        }

    }
}