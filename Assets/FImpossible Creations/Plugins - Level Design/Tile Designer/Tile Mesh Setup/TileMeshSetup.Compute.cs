using System;
using System.Collections.Generic;

namespace FIMSpace.Generating
{
    public partial class TileMeshSetup
    {
        //[NonSerialized] public List<CurvePoint> _editorPoints = new List<CurvePoint>();
        //[NonSerialized] public List<CurvePoint> _editorPoints2 = new List<CurvePoint>();

        [NonSerialized] public List<MeshShapePoint> previewShape = new List<MeshShapePoint>();
        [NonSerialized] public List<MeshShapePoint> previewShape2 = new List<MeshShapePoint>();
        [NonSerialized] public List<MeshShapePoint> previewShape3 = new List<MeshShapePoint>();

        public float splineLength = 1f;
        public float splineLength2 = 1f;
        public float splineLength3 = 1f;


        public void QuickUpdate()
        {
            if (GenTechnique == EMeshGenerator.Lathe) LatheQuickUpdate();
            else if (GenTechnique == EMeshGenerator.Loft) LoftQuickUpdate();
            else if (GenTechnique == EMeshGenerator.Extrude) ExtrudeQuickUpdate();
            else if (GenTechnique == EMeshGenerator.Sweep) SweepQuickUpdate();
            else if (GenTechnique == EMeshGenerator.CustomMeshAndExtras) CustomAndExtraQuickUpdate();
        }


        internal void PrepareCurves()
        {
            if (previewShape == null) previewShape = new List<MeshShapePoint>();
            if (previewShape2 == null) previewShape2 = new List<MeshShapePoint>();
            if (previewShape3 == null) previewShape3 = new List<MeshShapePoint>();

            if (_extrudePreview == null) _extrudePreview = new List<CurvePoint>();

            if (_loft_depth == null) _loft_depth = new List<CurvePoint>();
            if (_loft_distribute == null) _loft_distribute = new List<CurvePoint>();
            if (_loft_height == null) _loft_height = new List<CurvePoint>();

            if (_lathe_points == null) _lathe_points = new List<CurvePoint>();

            if (_extrude_curve == null) _extrude_curve = new List<CurvePoint>();

            if (_sweep_path == null) _sweep_path = new List<CurvePoint>();
            if (_sweep_shape == null) _sweep_shape = new List<CurvePoint>();
            if (_sweep_radius == null) _sweep_radius = new List<CurvePoint>();
        }

    }
}