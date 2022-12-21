using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class TileMeshSetup
    {
        public bool _extrudeMirror = true;
        public bool _extrudeFrontCap = false;
        public bool _extrudeBackCap = false;

        public float _extrude_SubdivLimit = 30f;

        public List<CurvePoint> _extrude_curve = new List<CurvePoint>();
        public List<CurvePoint> _extrudePreview = new List<CurvePoint>();


        void ExtrudeQuickUpdate()
        {
            if (_extrude_curve.Count <= 1) return;

            splineLength = 0f;
            _extrudePreview.Clear();

            Vector3 p;

            if (_extrudeMirror)
            {
                p = _extrude_curve[0].localPos;
                p.x = 1f;
                _extrudePreview.Add(new CurvePoint(p, true));
                _extrudePreview[0].VertexColor = _extrude_curve[0].VertexColor;
            }

            for (int i = 0; i < _extrude_curve.Count; i++)
            {
                CurvePoint np = new CurvePoint(_extrude_curve[i].localPos, true);
                np.localInTan = _extrude_curve[i].localInTan;
                np.localNextTan = _extrude_curve[i].localNextTan;
                np.VertexColor = _extrude_curve[i].VertexColor;
                _extrudePreview.Add(np);
            }

            if (_extrudeMirror)
            {
                p = _extrude_curve[_extrude_curve.Count - 1].localPos;
                p.x = 1f;
                _extrudePreview.Add(new CurvePoint(p, true));
                _extrudePreview[_extrudePreview.Count - 1].VertexColor = _extrude_curve[_extrude_curve.Count - 1].VertexColor;
            }

            for (int i = 0; i < _extrudePreview.Count - 1; i++)
            {
                _extrudePreview[i].distanceInSpline = splineLength;
                splineLength += Vector2.Distance(_extrudePreview[i].localPos, _extrudePreview[i + 1].localPos);
            }

            if (_extrudePreview.Count > 1) _extrudePreview[_extrudePreview.Count - 1].distanceInSpline = splineLength;
            if (splineLength == 0f) splineLength = 0.1f;

            GenerateMeshShape(_extrudePreview, splineLength, _extrude_SubdivLimit, previewShape, SubdivMode);
        }


    }
}