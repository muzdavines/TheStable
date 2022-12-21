// Original CSG.JS library by Evan Wallace (http://madebyevan.com), under the MIT license.
// GitHub: https://github.com/evanw/csg.js/
//
// C++ port by Tomasz Dabrowski (http://28byteslater.com), under the MIT license.
// GitHub: https://github.com/dabroz/csgjs-cpp/
//
// C# port by Karl Henkel (parabox.co), under MIT license.
//
// Constructive Solid Geometry (CSG) is a modeling technique that uses Boolean
// operations like union and intersection to combine 3D solids. This library
// implements CSG operations on meshes elegantly and concisely using BSP trees,
// and is meant to serve as an easily understandable implementation of the
// algorithm. All edge cases involving overlapping coplanar polygons in both
// solids are correctly handled.

using UnityEngine;
using System.Collections.Generic;

namespace Parabox.CSG
{
    /// <summary>
    /// Base class for CSG operations. Contains GameObject level methods for Subtraction, Intersection, and Union
    /// operations. The GameObjects passed to these functions will not be modified.
    /// </summary>
    public static class CSG
    {
        public enum BooleanOp
        {
            None,
            Intersection,
            Union,
            Subtraction
        }

        const float k_DefaultEpsilon = 0.00001f;
        static float s_Epsilon = k_DefaultEpsilon;

        /// <summary>
        /// Tolerance used by <see cref="Plane.SplitPolygon"/> determine whether planes are coincident.
        /// </summary>
        public static float epsilon
        {
            get => s_Epsilon;
            set => s_Epsilon = value;
        }

        /// <summary>
        /// Performs a boolean operation on two GameObjects.
        /// </summary>
        /// <returns>A new mesh.</returns>
        public static Model Perform(BooleanOp op, Mesh s, Material sm, Matrix4x4 smx, Mesh o, Material om, Matrix4x4 omx, bool allPoly)
        {
            switch (op)
            {
                case BooleanOp.Intersection:
                    return Intersect(s, sm, smx, o, om, omx, allPoly);
                case BooleanOp.Union:
                    return Union(s, sm, smx, o, om, omx, allPoly);
                case BooleanOp.Subtraction:
                    return Subtract(s, sm, smx, o, om, omx, allPoly);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns a new mesh by merging @lhs with @rhs.
        /// </summary>
        /// <param name="lhs">The base mesh of the boolean operation.</param>
        /// <param name="rhs">The input mesh of the boolean operation.</param>
        /// <returns>A new mesh if the operation succeeds, or null if an error occurs.</returns>
        public static Model Union(Mesh s, Material sm, Matrix4x4 smx, Mesh o, Material om, Matrix4x4 omx, bool allPoly)
        {
            Model csg_model_a = new Model(s, sm, smx);
            Model csg_model_b = new Model(o, om, omx);

            Node a = new Node(csg_model_a.ToPolygons());
            Node b = new Node(csg_model_b.ToPolygons());

            List<Polygon> polygons;

            if (allPoly)
                polygons = Node.Union(a, b).AllPolygons();
            else
                polygons = Node.Union(a, b).ClipPolygons(csg_model_a.ToPolygons());

            return new Model(polygons);
        }

        /// <summary>
        /// Returns a new mesh by subtracting @lhs with @rhs.
        /// </summary>
        /// <param name="lhs">The base mesh of the boolean operation.</param>
        /// <param name="rhs">The input mesh of the boolean operation.</param>
        /// <returns>A new mesh if the operation succeeds, or null if an error occurs.</returns>
        public static Model Subtract(Mesh s, Material sm, Matrix4x4 smx, Mesh o, Material om, Matrix4x4 omx, bool allPoly)
        {
            Model csg_model_a = new Model(s, sm, smx);
            Model csg_model_b = new Model(o, om, omx);

            Node a = new Node(csg_model_a.ToPolygons());
            Node b = new Node(csg_model_b.ToPolygons());

            List<Polygon> polygons;

            if ( allPoly)
                polygons = Node.Subtract(a, b).AllPolygons();
            else
                polygons = Node.Subtract(a, b).ClipPolygons(a.polygons);

            return new Model(polygons);
        }

        /// <summary>
        /// Returns a new mesh by intersecting @lhs with @rhs.
        /// </summary>
        /// <param name="lhs">The base mesh of the boolean operation.</param>
        /// <param name="rhs">The input mesh of the boolean operation.</param>
        /// <returns>A new mesh if the operation succeeds, or null if an error occurs.</returns>
        public static Model Intersect(Mesh s, Material sm, Matrix4x4 smx, Mesh o, Material om, Matrix4x4 omx, bool allPoly)
        {
            Model csg_model_a = new Model(s, sm, smx);
            Model csg_model_b = new Model(o, om, omx);

            Node a = new Node(csg_model_a.ToPolygons());
            Node b = new Node(csg_model_b.ToPolygons());

            List<Polygon> polygons;

            if (allPoly)
                polygons = Node.Intersect(a, b).AllPolygons();
            else
                polygons = Node.Intersect(a, b).ClipPolygons(csg_model_a.ToPolygons());

            return new Model(polygons);
        }
    }
}
