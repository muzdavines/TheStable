using Parabox.CSG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FIMSpace.Generating
{
    public static partial class FMeshUtils
    {

        public static void SmoothMeshNormals(Mesh m, float hard)
        {
            var verts = m.vertices;
            var triangles = m.triangles;

            Vector3[] normals = new Vector3[verts.Length];

            List<Vector3>[] vertexNormals = new List<Vector3>[verts.Length];

            for (int i = 0; i < vertexNormals.Length; i++)
            {
                vertexNormals[i] = new List<Vector3>();
            }

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 currNormal = Vector3.Cross(
                    (verts[triangles[i + 1]] - verts[triangles[i]]).normalized,
                    (verts[triangles[i + 2]] - verts[triangles[i]]).normalized);

                vertexNormals[triangles[i]].Add(currNormal);
                vertexNormals[triangles[i + 1]].Add(currNormal);
                vertexNormals[triangles[i + 2]].Add(currNormal);
            }

            for (int i = 0; i < vertexNormals.Length; i++)
            {
                normals[i] = Vector3.zero;

                float numNormals = vertexNormals[i].Count;
                for (int j = 0; j < numNormals; j++)
                {
                    normals[i] += vertexNormals[i][j];
                }

                normals[i] /= numNormals;

                if (hard > 0.05f)
                {
                    if (normals[i].sqrMagnitude > Mathf.Epsilon)
                    {
                        Quaternion look = Quaternion.LookRotation(normals[i]);
                        Vector3 sm = look.eulerAngles;
                        sm = FVectorMethods.FlattenVector(sm, hard * 90f);
                        normals[i] = Quaternion.Euler(sm) * Vector3.forward;
                    }
                }
            }

            m.normals = normals;
        }


        public static Mesh MeshesOperation(Mesh combined, Mesh removeCombination, Parabox.CSG.CSG.BooleanOp operation)
        {
            if (operation == Parabox.CSG.CSG.BooleanOp.None) return combined;

            Material defMat = new Material(Shader.Find("Diffuse"));
            Model result;

            if (operation == CSG.BooleanOp.Intersection)
                result = CSG.Intersect(combined, defMat, Matrix4x4.identity, removeCombination, defMat, Matrix4x4.identity, true);
            else if (operation == CSG.BooleanOp.Subtraction)
                result = CSG.Subtract(combined, defMat, Matrix4x4.identity, removeCombination, defMat, Matrix4x4.identity, true);
            else //if (operation == CSG.BooleanOp.Union)
                result = CSG.Union(combined, defMat, Matrix4x4.identity, removeCombination, defMat, Matrix4x4.identity, true);


            return result.mesh;
        }


        public static Mesh AdjustOrigin(Mesh m, TileMeshSetup.EOrigin origin)
        {
            m.RecalculateBounds();

            if (origin == TileMeshSetup.EOrigin.Unchanged) return m;
            else if (origin == TileMeshSetup.EOrigin.Center)
            {
                Vector3 off = -m.bounds.center;
                var verts = m.vertices;

                // Center Offset
                for (int v = 0; v < verts.Length; v++) verts[v] += off;


                m.SetVerticesUnity2018(verts);
            }
            else if (origin == TileMeshSetup.EOrigin.BottomCenter)
            {
                Vector3 off = new Vector3(-m.bounds.center.x, -m.bounds.min.y, -m.bounds.center.z);

                var verts = m.vertices;
                for (int v = 0; v < verts.Length; v++) verts[v] += off;

                m.SetVerticesUnity2018(verts);
            }
            else if (origin == TileMeshSetup.EOrigin.TopCenter)
            {
                Vector3 off = new Vector3(-m.bounds.center.x, -m.bounds.max.y, -m.bounds.center.z);

                var verts = m.vertices;
                for (int v = 0; v < verts.Length; v++) verts[v] += off;

                m.SetVerticesUnity2018(verts);
            }
            else if (origin == TileMeshSetup.EOrigin.BottomLeft)
            {
                Vector3 off = new Vector3(-m.bounds.min.x, -m.bounds.min.y, -m.bounds.min.z);

                var verts = m.vertices;
                for (int v = 0; v < verts.Length; v++) verts[v] += off;

                m.SetVerticesUnity2018(verts);
            }
            else if (origin == TileMeshSetup.EOrigin.BottomCenterBack)
            {
                Vector3 off = new Vector3(-m.bounds.center.x, -m.bounds.min.y, -m.bounds.min.z);

                var verts = m.vertices;
                for (int v = 0; v < verts.Length; v++) verts[v] += off;

                m.SetVerticesUnity2018(verts);
            }
            else if (origin == TileMeshSetup.EOrigin.BottomCenterFront)
            {
                Vector3 off = new Vector3(-m.bounds.center.x, -m.bounds.min.y, -m.bounds.max.z);

                var verts = m.vertices;
                for (int v = 0; v < verts.Length; v++) verts[v] += off;

                m.SetVerticesUnity2018(verts);
            }

            return m;
        }


        #region Unity 2018 Support


        public static void SetVerticesUnity2018(this Mesh m, Vector3[] verts)
        {
#if UNITY_2019_4_OR_NEWER
            m.SetVertices(verts);
#else
            m.vertices = verts;
#endif
        }

        public static void SetUVUnity2018(this Mesh m, Vector2[] uv)
        {
#if UNITY_2019_4_OR_NEWER
            m.SetUVs(0, uv);
#else
            m.uv = uv;
#endif
        }

        public static void SetNormalsUnity2018(this Mesh m, Vector3[] norm)
        {
#if UNITY_2019_4_OR_NEWER
            m.SetNormals(norm);
#else
            m.normals = norm;
#endif
        }

        public static void SetTrianglesUnity2018(this Mesh m, int[] tris)
        {
#if UNITY_2019_4_OR_NEWER
            m.SetTriangles(tris, 0);
#else
            m.triangles = tris;
#endif
        }


        public static void SetColorsUnity2018(this Mesh m, List<Color> c)
        {
#if UNITY_2019_4_OR_NEWER
            m.SetColors(c);
#else
            m.colors = c.ToArray();
#endif
        }


        #endregion



        public static void OffsetUV(Mesh mesh, Vector2 uVOffset)
        {
            Vector2[] uvs = mesh.uv;
            for (int u = 0; u < uvs.Length; u++)
            {
                uvs[u] = new Vector2((uvs[u].x + uVOffset.x), (uvs[u].y + uVOffset.y));
            }

            mesh.SetUVUnity2018(uvs);
        }

        public static void RotateUV(Mesh mesh, float angle)
        {
            Vector2[] uvs = mesh.uv;

            float rad = angle * Mathf.Deg2Rad;

            float rotMatrix00 = Mathf.Cos(rad);
            float rotMatrix01 = -Mathf.Sin(rad);
            float rotMatrix10 = Mathf.Sin(rad);
            float rotMatrix11 = Mathf.Cos(rad);

            Vector2 halfV2 = new Vector2(0.5f, 0.5f);

            for (int j = 0; j < uvs.Length; j++)
            {
                uvs[j] = uvs[j] - halfV2;
                float u = rotMatrix00 * uvs[j].x + rotMatrix01 * uvs[j].y;
                float v = rotMatrix10 * uvs[j].x + rotMatrix11 * uvs[j].y;
                uvs[j].x = u; uvs[j].y = v;
                uvs[j] = uvs[j] + halfV2;
            }

            mesh.SetUVUnity2018(uvs);
        }

        public static void RescaleUV(Mesh mesh, Vector2 uVReScale)
        {
            Vector2[] uvs = mesh.uv;
            for (int u = 0; u < uvs.Length; u++)
            {
                uvs[u] = new Vector2((uvs[u].x * uVReScale.x), (uvs[u].y * uVReScale.y));
            }

            mesh.SetUVUnity2018(uvs);
        }

        public static void FlipNormals(Mesh mesh)
        {
            Vector3[] normals = mesh.normals;

            for (int i = 0; i < normals.Length; i++) normals[i] = -normals[i];
            mesh.SetNormalsUnity2018(normals);

            int[] triangles = mesh.GetTriangles(0);

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int temp = triangles[i + 0];
                triangles[i + 0] = triangles[i + 1];
                triangles[i + 1] = temp;
            }

            mesh.SetTrianglesUnity2018(triangles);
        }

        public static void SmoothNormals(Mesh mesh)
        {
            Vector3[] normals = mesh.normals;

            for (int i = 0; i < mesh.vertices.Length; i++)
                for (int j = i + 1; j < mesh.vertices.Length; j++)
                    if (mesh.vertices[i] == mesh.vertices[j])
                    {
                        Vector3 averagedNormal = (normals[i] + normals[j]) / 2;
                        normals[i] = averagedNormal;
                        normals[j] = averagedNormal;
                    }

            mesh.normals = normals;
        }

    }
}