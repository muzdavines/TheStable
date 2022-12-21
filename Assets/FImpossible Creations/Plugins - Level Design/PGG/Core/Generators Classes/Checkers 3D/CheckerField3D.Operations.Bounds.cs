using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Checker
{
    public partial class CheckerField3D
    {
   

        private Vector3[] _tBoundsDiag = new Vector3[2];
        private Vector3[] _tBounds = new Vector3[4];
        public Vector3[] TransformBounds(Bounds b)
        {
            Matrix4x4 mx = Matrix;
            //Vector3 c = TransformBoundsCenter(b);
            _tBounds[0] = mx.MultiplyPoint3x4(new Vector3(b.min.x, b.center.y, b.min.z));
            _tBounds[1] = mx.MultiplyPoint3x4(new Vector3(b.min.x, b.center.y, b.max.z));
            _tBounds[2] = mx.MultiplyPoint3x4(new Vector3(b.max.x, b.center.y, b.max.z));
            _tBounds[3] = mx.MultiplyPoint3x4(new Vector3(b.max.x, b.center.y, b.min.z));

            //_tBounds[0].y = c.y;
            //_tBounds[1].y = c.y;
            //_tBounds[2].y = c.y;
            //_tBounds[3].y = c.y;

            return _tBounds;
        }


        public Vector3[] TransformBoundsDiag(Bounds b)
        {
            //Vector3 c = TransformBoundsCenter(b);
            _tBoundsDiag[0] = Matrix.MultiplyPoint3x4(new Vector3(b.min.x, b.center.y, b.min.z));
            _tBoundsDiag[1] = Matrix.MultiplyPoint3x4(new Vector3(b.max.x, b.center.y, b.max.z));

            //_tBoundsDiag[0].y = c.y;
            //_tBoundsDiag[1].y = c.y;

            return _tBoundsDiag;
        }


        public Vector3 TransformBoundsCenter(Bounds b)
        {
            return Matrix.MultiplyPoint3x4(b.center);
        }


    }
}