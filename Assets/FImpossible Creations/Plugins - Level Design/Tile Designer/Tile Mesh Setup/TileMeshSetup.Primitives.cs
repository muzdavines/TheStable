using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class TileMeshSetup
    {
        public enum EPrimitiveType
        {
            Cube, Plane, Sphere, Cylinder //, Torus, Tube
        }

        public EPrimitiveType _primitive_Type = EPrimitiveType.Cube;
        public Vector3 _primitive_scale = Vector3.one;

        public bool _primitive_cube_topFace = true;
        public bool _primitive_cube_bottomFace = true;
        public bool _primitive_cube_leftFace = true;
        public bool _primitive_cube_rightFace = true;
        public bool _primitive_cube_frontFace = true;
        public bool _primitive_cube_backFace = true;

        public float _primitive_cube_bevel = 0f;
        public int _primitive_cube_bevelSubdivs = 1;

        public Vector3Int _primitive_plane_subdivs = new Vector3Int(1, 1, 1);

        void PrimitiveQuickUpdate()
        {

        }


    }
}