using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class TileMeshSetup
    {

        public float _loftDepthCurveWidener = 0.5f;
        public float _loft_DepthSubdivLimit = 30;
        public float _loft_DistribSubdivLimit = 30f;
        public float _loft_Collapse = 0f;

        public List<CurvePoint> _loft_depth = new List<CurvePoint>();
        public List<CurvePoint> _loft_distribute = new List<CurvePoint>();
        public List<CurvePoint> _loft_height = new List<CurvePoint>();


        void LoftQuickUpdate()
        {

            splineLength = 0f;
            for (int i = 0; i < _loft_depth.Count - 1; i++)
            {
                _loft_depth[i].distanceInSpline = splineLength;
                splineLength += Vector2.Distance(_loft_depth[i].localPos, _loft_depth[i + 1].localPos);
            }

            if (_loft_depth.Count > 1) _loft_depth[_loft_depth.Count - 1].distanceInSpline = splineLength;
            if (splineLength == 0f) splineLength = 0.1f;

            GenerateMeshShape(_loft_depth, splineLength, _loft_DepthSubdivLimit, previewShape, SubdivMode);




            splineLength2 = 0f;
            for (int i = 0; i < _loft_distribute.Count - 1; i++)
            {
                _loft_distribute[i].distanceInSpline = splineLength2;
                splineLength2 += Vector2.Distance(_loft_distribute[i].localPos, _loft_distribute[i + 1].localPos);
            }

            if (_loft_distribute.Count > 1) _loft_distribute[_loft_distribute.Count - 1].distanceInSpline = splineLength2;
            if (splineLength2 == 0f) splineLength2 = 0.1f;


            GenerateMeshShape(_loft_distribute, splineLength2, _loft_DistribSubdivLimit, previewShape2, SubdivMode);




            splineLength3 = 0f;
            for (int i = 0; i < _loft_height.Count - 1; i++)
            {
                _loft_height[i].distanceInSpline = splineLength3;
                splineLength3 += Vector2.Distance(_loft_height[i].localPos, _loft_height[i + 1].localPos);
            }

            if (_loft_height.Count > 1) _loft_height[_loft_height.Count - 1].distanceInSpline = splineLength3;
            if (splineLength3 == 0f) splineLength3 = 0.1f;


            GenerateMeshShape(_loft_height, splineLength3, _loft_DistribSubdivLimit, previewShape3, SubdivMode);

        }



    }
}