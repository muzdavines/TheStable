using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public static partial class FMeshUtils
    {

        #region Helper Vertex Class


        public class PolyShapeHelpPoint
        {
            /// <summary> For generating vertices </summary>
            public int index;
            /// <summary> If casting verticies from some other reversed list </summary>
            public int helpIndex;

            public Vector3 vxPos;
            public Vector3 vxNorm;
            public float vxDot;

            public PolyShapeHelpPoint previous;
            public PolyShapeHelpPoint next;
            public bool isReflex;
            public bool isConvex;

            public PolyShapeHelpPoint(Vector3 pos)
            {
                vxPos = pos;
            }

            public Vector2 XZ()
            {
                return new Vector2(vxPos.x, vxPos.z);
            }

            public void ValidateReflexOrConvex()
            {
                isReflex = false;
                isConvex = false;

                Vector2 a = previous.XZ();
                Vector2 b = XZ();
                Vector2 c = next.XZ();

                if (IsTriangleOrientedClockwise(a, b, c)) isReflex = true; else isConvex = true;
            }


            public void ValidateVertexEar(List<PolyShapeHelpPoint> vertices, List<PolyShapeHelpPoint> earVertices)
            {
                if (isReflex) { return; }

                Vector2 a = previous.XZ();
                Vector2 b = XZ();
                Vector2 c = next.XZ();

                bool hasPointInside = false;

                for (int i = 0; i < vertices.Count; i++)
                {
                    if (vertices[i].isReflex)
                    {
                        Vector2 p = vertices[i].XZ();

                        if (IsPointInTriangle(a, b, c, p))
                        {
                            hasPointInside = true;
                            break;
                        }
                    }
                }

                if (!hasPointInside) earVertices.Add(this);
            }

            public static bool IsPointInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
            {
                bool isWithinTriangle = false;
                float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

                float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
                float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
                float c = 1 - a - b;

                if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f) isWithinTriangle = true;

                return isWithinTriangle;
            }

            public static bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
            {
                bool isClockWise = true;
                float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;
                if (determinant > 0f) isClockWise = false;
                return isClockWise;
            }


        }


        #endregion


        /// <summary>
        /// "vertexPoints" must be arranged counter-clockwise
        /// Thanks to Erik Nordeus!
        /// https://www.habrador.com/
        /// For awesome 3D math articles, without them this feature maybe would not exist.
        /// </summary>
        public static List<int> TriangulateConcavePolygon(List<PolyShapeHelpPoint> vertexPoints)
        {
            List<int> triangles = new List<int>();
            if (vertexPoints.Count < 3) return triangles; // No Shape!

            // One triangle shape!
            if (vertexPoints.Count == 3) { triangles.Add(vertexPoints[0].index); triangles.Add(vertexPoints[1].index); triangles.Add(vertexPoints[2].index); return triangles; }

            // Dynamic list since logic will take one by one element from list with removing it from list
            List<PolyShapeHelpPoint> vertices = new List<PolyShapeHelpPoint>();
            for (int i = 0; i < vertexPoints.Count; i++) vertices.Add(vertexPoints[i]);

            // Define vertices relation
            for (int i = 0; i < vertices.Count; i++)
            {
                int nextPos = (i + 1); if (nextPos >= vertices.Count) nextPos = 0;
                vertices[i].next = vertices[nextPos];

                int prevPos = (i - 1); if (prevPos < 0) prevPos = vertices.Count - 1;
                vertices[i].previous = vertices[prevPos];
            }

            for (int i = 0; i < vertices.Count; i++) vertices[i].ValidateReflexOrConvex();

            List<PolyShapeHelpPoint> earVertices = new List<PolyShapeHelpPoint>();
            for (int i = 0; i < vertices.Count; i++) vertices[i].ValidateVertexEar(vertices, earVertices);

            while (true)
            {
                // Final triangle
                if (vertices.Count == 3)
                {
                    triangles.Add(vertices[0].index);
                    triangles.Add(vertices[0].previous.index);
                    triangles.Add(vertices[0].next.index);
                    break;
                }

                if (earVertices.Count == 0)
                {
                    UnityEngine.Debug.Log("[Triangulation] Some exception happened when triangulating. Are vertices positions set correctly (if all vertices are Z=0 it also can cause this error) and in counter-clockwise order?");
                    break; // Exception error
                }

                PolyShapeHelpPoint earVertex = earVertices[0];
                PolyShapeHelpPoint earVertexPrev = earVertex.previous;
                PolyShapeHelpPoint earVertexNext = earVertex.next;

                triangles.Add(earVertex.index);
                triangles.Add(earVertexPrev.index);
                triangles.Add(earVertexNext.index);

                earVertices.Remove(earVertex);
                vertices.Remove(earVertex);

                earVertexPrev.next = earVertexNext;
                earVertexNext.previous = earVertexPrev;

                earVertexPrev.ValidateReflexOrConvex();
                earVertexNext.ValidateReflexOrConvex();

                earVertices.Remove(earVertexPrev);
                earVertices.Remove(earVertexNext);

                earVertexPrev.ValidateVertexEar(vertices, earVertices);
                earVertexNext.ValidateVertexEar(vertices, earVertices);
            }

            return triangles;
        }


    }
}