using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Checker
{
    public partial class CheckerField3D
    {

        private int _nearestMyBoundsIndex = -1;
        private int _nearestOtherBoundsIndex = -1;
        private Vector3 _nearestMyBoundsPos = Vector3.zero;

        /// <summary>
        /// After calling this method, access '_nearestMyBoundsIndex'  '_nearestMyBoundsPos' for more variables computed in the method
        /// </summary>
        public Vector3 GetNearestWorldPosToBoundsDiagonal(CheckerField3D other)
        {
            float nearest = float.MaxValue;
            _nearestMyBoundsIndex = -1;
            _nearestOtherBoundsIndex = -1;
            _nearestMyBoundsPos = Vector3.zero;

            if (Bounding.Count < 1 || UseBounds == false)
            {
                Bounds a = GetFullBoundsWorldSpace();
                Bounds b = other.GetFullBoundsWorldSpace();

                Vector3[] diag = other.TransformBoundsDiag(b);
                Vector3 nearP = GetNearestPointToLine(diag[0], diag[1], a.center);

                UnityEngine.Debug.DrawRay(nearP, Vector3.up * 5f, Color.red, 1.01f);

                return RootPosition;
            }

            if (other.Bounding.Count < 1) return RootPosition;

            Vector3 nearestOtherP = Vector3.zero;

            for (int i = 0; i < Bounding.Count; i++)
            {
                Vector3 ibCenter = TransformBoundsCenter(Bounding[i]);

                for (int o = 0; o < other.Bounding.Count; o++)
                {
                    Vector3[] diag = other.TransformBoundsDiag(other.Bounding[o]);
                    Vector3 nearP = GetNearestPointToLine(diag[0], diag[1], ibCenter);

                    float dist = Vector3.SqrMagnitude(nearP - ibCenter);

                    #region Debugging

                    //UnityEngine.Debug.Log("Dist between " + (nearP - ibCenter).magnitude);
                    //UnityEngine.Debug.DrawRay(nearP, Vector3.up, Color.yellow, 1.01f);
                    //UnityEngine.Debug.DrawRay(ibCenter, Vector3.up, Color.yellow, 1.01f);
                    //UnityEngine.Debug.DrawLine(nearP, ibCenter, Color.red, 1.01f);
                    //UnityEngine.Debug.DrawLine(other.TransformBoundsCenter(other.Bounding[o]), ibCenter, Color.magenta, 1.01f);

                    #endregion

                    if (dist < nearest)
                    {
                        _nearestMyBoundsIndex = i;
                        _nearestOtherBoundsIndex = o;
                        _nearestMyBoundsPos = ibCenter;
                        nearestOtherP = nearP;
                        nearest = dist;
                    }
                }
            }

            #region Debugging

            //UnityEngine.Debug.Log("nearest " + nearest + " ** " + (nearest * nearest));
            //UnityEngine.Debug.DrawLine(nearestIP, nearestOP, Color.green, 1.01f);

            #endregion

            return nearestOtherP;
        }


        /// <summary>
        /// After calling this method, access '_nearestMyBoundsIndex'  '_nearestMyBoundsPos' for more variables computed in the method
        /// </summary>
        public Vector3 GetNearestWorldPosToBounds(CheckerField3D other)
        {
            float nearest = float.MaxValue;
            _nearestMyBoundsIndex = -1;
            _nearestOtherBoundsIndex = -1;
            _nearestMyBoundsPos = Vector3.zero;

            if (Bounding.Count < 1) return RootPosition;
            if (other.Bounding.Count < 1) return RootPosition;

            Vector3 nearestOtherP = Vector3.zero;

            for (int i = 0; i < Bounding.Count; i++)
            {
                Bounds bound = LocalToWorldBounds(Bounding[i]);

                for (int o = 0; o < other.Bounding.Count; o++)
                {
                    Bounds oBound = other.LocalToWorldBounds(other.Bounding[o]);
                    Vector3 closeP = bound.ClosestPoint(oBound.center);
                    Vector3 nearP = oBound.ClosestPoint(closeP);

                    float dist = Vector3.SqrMagnitude(nearP - closeP);

                    //if (dist <= 0f) continue;

                    if (dist < nearest)
                    {
                        _nearestMyBoundsIndex = i;
                        _nearestOtherBoundsIndex = o;
                        _nearestMyBoundsPos = closeP;
                        nearestOtherP = nearP;
                        nearest = dist;
                    }
                }
            }

            return nearestOtherP;
        }


    }
}