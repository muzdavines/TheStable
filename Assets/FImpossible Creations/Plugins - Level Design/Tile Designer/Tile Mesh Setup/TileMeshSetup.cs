using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    [System.Serializable]
    public partial class TileMeshSetup
    {
        public string Name = "Tile Mesh";

        public TileMeshSetup(string name = "")
        {
            if (name != "") Name = name;
            _lathe_fillAngle = 360;
        }


        public static TileMeshSetup _CopyRef = null;
        public static bool _CopyInstances = false;

        public void PasteMainTo(TileMeshSetup to)
        {
            to.Name = Name;
            to.Material = Material;
        }

        public void PasteParametersTo(TileMeshSetup to)
        {
            to.Origin = Origin;
            to.GenTechnique = GenTechnique;
            to.UVFit = UVFit;
            to.UVMul = UVMul;
            to.HardNormals = HardNormals;
            to.SubdivMode = SubdivMode;

            to.width = width;
            to.height = height;
            to.depth = depth;

            if (GenTechnique == EMeshGenerator.Loft)
            {
                to._loftDepthCurveWidener = _loftDepthCurveWidener;
                to._loft_DepthSubdivLimit = _loft_DepthSubdivLimit;
                to._loft_DistribSubdivLimit = _loft_DistribSubdivLimit;
            }
            else if (GenTechnique == EMeshGenerator.Lathe)
            {
                to._lathe_fillAngle = _lathe_fillAngle;
                to._lathe_xSubdivCount = _lathe_xSubdivCount;
                to._lathe_ySubdivLimit = _lathe_ySubdivLimit;
            }
            else if (GenTechnique == EMeshGenerator.Extrude)
            {
                to._extrude_SubdivLimit = _extrude_SubdivLimit;
                to._extrudeMirror = _extrudeMirror;
                to._extrudeFrontCap = _extrudeFrontCap;
                to._extrudeBackCap = _extrudeBackCap;
            }
            else if (GenTechnique == EMeshGenerator.Sweep)
            {
                to._sweep_Close = _sweep_Close;
                to._sweep_distribSubdivLimit = _sweep_distribSubdivLimit;
                to._sweep_radiusMul = _sweep_radiusMul;
                to._sweep_shapeSubdivLimit = _sweep_shapeSubdivLimit;
            }
            else if (GenTechnique == EMeshGenerator.Primitive)
            {
                to._primitive_cube_backFace = _primitive_cube_backFace;
                to._primitive_cube_bevel = _primitive_cube_bevel;
                to._primitive_cube_bevelSubdivs = _primitive_cube_bevelSubdivs;
                to._primitive_cube_bottomFace = _primitive_cube_bottomFace;
                to._primitive_cube_frontFace = _primitive_cube_frontFace;
                to._primitive_cube_leftFace = _primitive_cube_leftFace;
                to._primitive_cube_rightFace = _primitive_cube_rightFace;
                to._primitive_cube_topFace = _primitive_cube_topFace;
                to._primitive_plane_subdivs = _primitive_plane_subdivs;
                to._primitive_scale = _primitive_scale;
                to._primitive_Type = _primitive_Type;
            }
            else if (GenTechnique == EMeshGenerator.CustomMeshAndExtras)
            {
                to._customMeshOverwriteVertexColor = _customMeshOverwriteVertexColor;
                to._customMeshOverwriteVertexColorValues = _customMeshOverwriteVertexColorValues;
            }
        }


        public void PasteCurvesTo(TileMeshSetup to)
        {
            CurvePoint.CopyListFromTo(_loft_depth, to._loft_depth);
            CurvePoint.CopyListFromTo(_loft_distribute, to._loft_distribute);
            CurvePoint.CopyListFromTo(_lathe_points, to._lathe_points);
            CurvePoint.CopyListFromTo(_extrude_curve, to._extrude_curve);
            CurvePoint.CopyListFromTo(_sweep_path, to._sweep_path);
            CurvePoint.CopyListFromTo(_sweep_radius, to._sweep_radius);
            CurvePoint.CopyListFromTo(_sweep_shape, to._sweep_shape);
        }

        public void PasteAllSetupTo(TileMeshSetup to, bool copyInstances = false)
        {
            PasteMainTo(to);
            PasteParametersTo(to);
            PasteCurvesTo(to);

            if (copyInstances)
            {
                to._instances.Clear();

                for (int i = 0; i < _instances.Count; i++)
                {
                    TileMeshCombineInstance inst = _instances[i].Copy();
                    to._instances.Add(inst);
                }

                to.Copies = Copies;
            }

            _CopyInstances = false;
        }


        public bool DrawSnappingPX()
        {
            return GenTechnique != EMeshGenerator.CustomMeshAndExtras && GenTechnique != EMeshGenerator.Primitive;
        }

        public bool DrawMeshOptions()
        {
            return GenTechnique != EMeshGenerator.CustomMeshAndExtras;
        }


    }
}