using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FIMSpace.Generating
{
    public static partial class FMeshUtils
    {


        #region Weld V2


        /// <summary>
        /// Vertex welding algorithm thanks to: ParadoxSolutions on the unity community forums
        /// https://forum.unity.com/threads/mesh-combine-that-preserves-shared-vertices.966650/#post-6294322
        /// </summary>
        public static Mesh Weld2(this Mesh mesh, float bucketStep)
        {
            Vector3[] oldVertices = mesh.vertices;
            Vector3[] newVertices = new Vector3[oldVertices.Length];
            Vector3[] newNormals = new Vector3[oldVertices.Length];
            Color[] newColors = new Color[oldVertices.Length];

            bool hasColors = mesh.colors.Length > 0;

            int[] old2new = new int[oldVertices.Length];
            int newSize = 0;

            // UV Preserve Required Operations
            Dictionary<Vector3, Vector2> uvCollect = new Dictionary<Vector3, Vector2>();

            for (int v = 0; v < mesh.vertices.Length; v++)
            {
                Vector3 vPos = mesh.vertices[v];
                if (uvCollect.ContainsKey(vPos) == false) uvCollect.Add(vPos, mesh.uv[v]);
            }

            // Find AABB
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for (int i = 0; i < oldVertices.Length; i++)
            {
                if (oldVertices[i].x < min.x) min.x = oldVertices[i].x;
                if (oldVertices[i].y < min.y) min.y = oldVertices[i].y;
                if (oldVertices[i].z < min.z) min.z = oldVertices[i].z;
                if (oldVertices[i].x > max.x) max.x = oldVertices[i].x;
                if (oldVertices[i].y > max.y) max.y = oldVertices[i].y;
                if (oldVertices[i].z > max.z) max.z = oldVertices[i].z;
            }

            min -= Vector3.one * 0.111111f;
            max += Vector3.one * 0.899999f;

            // Make cubic buckets, each with dimensions "bucketStep"
            int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStep) + 1;
            int bucketSizeY = Mathf.FloorToInt((max.y - min.y) / bucketStep) + 1;
            int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStep) + 1;
            List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];

            // Make new vertices
            for (int i = 0; i < oldVertices.Length; i++)
            {
                // Determine which bucket it belongs to
                int x = Mathf.FloorToInt((oldVertices[i].x - min.x) / bucketStep);
                int y = Mathf.FloorToInt((oldVertices[i].y - min.y) / bucketStep);
                int z = Mathf.FloorToInt((oldVertices[i].z - min.z) / bucketStep);

                // Check to see if it's already been added
                if (buckets[x, y, z] == null)
                    buckets[x, y, z] = new List<int>(); // Make buckets lazily

                for (int j = 0; j < buckets[x, y, z].Count; j++)
                {
                    Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                    if (Vector3.SqrMagnitude(to) < 0.001f)
                    {
                        old2new[i] = buckets[x, y, z][j];
                        goto skip; // Skip to next old vertex if this one is already there
                    }
                }

                // Add new vertex
                newVertices[newSize] = oldVertices[i];

                if (hasColors) newColors[newSize] = mesh.colors[i];
                newNormals[newSize] = mesh.normals[i];

                buckets[x, y, z].Add(newSize);
                old2new[i] = newSize;
                newSize++;

            skip:;
            }


            // Make new triangles
            int[] oldTris = mesh.triangles;
            int[] newTris = new int[oldTris.Length];

            for (int i = 0; i < oldTris.Length; i++)
            {
                newTris[i] = old2new[oldTris[i]];
            }

            Vector3[] finalVertices = new Vector3[newSize];
            Color[] finalCols = new Color[newSize];
            Vector3[] finalNorms = new Vector3[newSize];
            Vector2[] finalUvs = new Vector2[newSize];

            for (int i = 0; i < newSize; i++)
            {
                finalVertices[i] = newVertices[i];
                finalNorms[i] = newNormals[i];
                finalCols[i] = newColors[i];
            }

            // UV Preserve Operation
            for (int i = 0; i < newSize; i++)
            {
                finalUvs[i] = uvCollect[finalVertices[i]];
            }

            mesh.Clear();
            FMeshUtils.SetVerticesUnity2018(mesh, finalVertices);
            FMeshUtils.SetTrianglesUnity2018(mesh, newTris);
            FMeshUtils.SetNormalsUnity2018(mesh, finalNorms);
            if (hasColors) mesh.SetColorsUnity2018(finalCols.ToList());
            mesh.SetUVUnity2018(finalUvs);
            mesh.RecalculateTangents();

            return mesh;
        }



        #endregion










        /// <summary>
        /// Thanks to Tolin Simpson!
        /// https://github.com/TolinSimpson
        /// For great vertex velding algorithm.
        /// I did few changes to support UV transport and Vertex Color
        /// </summary>

        #region UV Packer


        public static class UVPacker
        {
            internal class Node
            {
                public Node Child1;
                public Node Child2;
                public Node Child3;
                public float X;
                public float Y;
                public float Width;
                public float Height;
                public bool Used;
            }

            public class Box
            {
                public float X;
                public float Y;
                public float Width;
                public float Height;
                public float ShiftX;
                public float ShiftY;
                public float Side;
                public MeshExtended Extended;
                public Vector2[] PackedUVs;
            }


            static Box UVBox(MeshExtended extended, float gap)
            {

                // find the minimum and maximum values for X and Y.
                Vector2[] uvs = extended.UVs;
                float xMin = Mathf.Infinity;
                float xMax = -Mathf.Infinity;
                float yMin = Mathf.Infinity;
                float yMax = -Mathf.Infinity;

                foreach (Vector2 v2 in uvs)
                {
                    if (v2.x < xMin)
                    {
                        xMin = v2.x;
                    }
                    if (v2.x > xMax)
                    {
                        xMax = v2.x;
                    }
                    if (v2.y < yMin)
                    {
                        yMin = v2.y;
                    }
                    if (v2.y > yMax)
                    {
                        yMax = v2.y;
                    }
                }

                // calculate the with and height plus a small gap definded by the user.
                float wid = xMax - xMin + gap;
                float hgt = yMax - yMin + gap;

                // calculate the box size base on the 3D size.
                Vector3 size = extended.Size;
                float sid = size.x * size.y * size.z;

                return new Box { Height = hgt, Width = wid, Side = sid, Extended = extended, ShiftX = xMin, ShiftY = yMin };

            }

            static Node FindNode(Node node, float width, float height)
            {

                // Find an empty node with enough space to fit the box.
                if (node.Used)
                {
                    Node next = node.Child1 == null ? null : FindNode(node.Child1, width, height);
                    if (next != null)
                    {
                        return next;
                    }
                    next = node.Child2 == null ? null : FindNode(node.Child2, width, height);
                    if (next != null)
                    {
                        return next;
                    }
                    return node.Child3 == null ? null : FindNode(node.Child3, width, height);
                }
                if (width <= node.Width && height <= node.Height)
                {
                    return node;
                }
                return null;

            }

            static void SplitNode(Node node, Box box)
            {

                float x = node.X;
                float y = node.Y;
                box.X = x;
                box.Y = y;

                // calculate the space left to the right and upwards.
                float dw = node.Width - box.Side;
                float dl = node.Height - box.Side;

                // adjust the node size to the box size.
                node.Used = true;
                node.Width = box.Side;
                node.Height = box.Side;
                if (dw > 0)
                {
                    // add a node to the right is there's unused horizontal space.
                    node.Child1 = new Node { X = x + node.Width, Y = y, Width = dw, Height = node.Height };
                }
                if (dl > 0)
                {
                    // add a node above if there's unused vertical space.
                    node.Child2 = new Node { X = x, Y = y + node.Height, Width = node.Width + dw, Height = dl };
                }

            }

            static Node AttachNode(Node root, Box box)
            {

                // Mo empty space was found, so create some.

                Node used, empty, parent;
                float x, y;
                // chose where to add the new space, in such a way that over time the resulting container becomes closer to a square.
                if (root.Width > root.Height)
                {
                    // calulate the space that wil be left unused if the node is added to the right.
                    float dw = Mathf.Abs(root.Width - box.Side);
                    x = 0;
                    y = root.Height;
                    // Create a node for the empty space.
                    if (root.Width < box.Side)
                    {
                        empty = new Node { X = root.Width, Y = 0, Width = dw, Height = root.Height };
                    }
                    else
                    {
                        empty = new Node { X = box.Side, Y = y, Width = dw, Height = box.Side };
                    }
                    // Create a node for the box.
                    used = new Node { X = 0, Y = y, Width = box.Side, Height = box.Side, Used = true };
                    // and add a new parent.
                    parent = new Node { X = 0, Y = 0, Width = root.Width, Height = root.Height + box.Side, Used = true, Child1 = root, Child2 = used, Child3 = empty };
                }
                else
                {
                    // calulate the space that wil be left unused if the node is added above.
                    float dl = Mathf.Abs(root.Height - box.Side);
                    x = root.Width;
                    y = 0;
                    // Create a node for the empty space.
                    if (root.Height < box.Side)
                    {
                        empty = new Node { X = 0, Y = root.Height, Width = root.Width, Height = dl };
                    }
                    else
                    {
                        empty = new Node { X = x, Y = box.Side, Width = box.Side, Height = dl };
                    }
                    // Create a node for the box.
                    used = new Node { X = x, Y = 0, Width = box.Side, Height = box.Side, Used = true };
                    // and add a new parent.
                    parent = new Node { X = 0, Y = 0, Width = root.Width + box.Side, Height = root.Height, Used = true, Child1 = root, Child2 = used, Child3 = empty };
                }
                box.X = x;
                box.Y = y;
                return parent;

            }

            static void AdjustUVs(Box box)
            {

                // make sure the UV fit in the 0-1 space (inside the box)
                Vector2[] uv = box.Extended.UVs;
                int count = uv.Length;
                float sqx = box.Side / box.Width;
                float sqy = box.Side / box.Height;
                float x = box.X;
                float y = box.Y;
                float shiftx = box.ShiftX;
                float shifty = box.ShiftY;

                Vector2[] packed = new Vector2[count];
                for (int u = 0; u < count; u++)
                {
                    packed[u].x = x + (uv[u].x - shiftx) * sqx;
                    packed[u].y = y + (uv[u].y - shifty) * sqy;
                }
                box.PackedUVs = packed;

            }

            static public Box Pack(MeshExtended extended, float gap)
            {
                Box boxs = UVBox(extended, gap);
                boxs.Side = 1f;

                // make a root node based on the first (biggest) box.
                Node root = new Node { Height = boxs.Side, Width = boxs.Side };
                Node node = FindNode(root, boxs.Side, boxs.Side);

                if (node == null)
                {
                    // if there's not enough space then add extra space.
                    root = AttachNode(root, boxs);
                }
                else
                {
                    // split the node to take only the necesary space.
                    SplitNode(node, boxs);
                }

                // once we have all the boxes at their correspinding positions and scales, let's ajust the UVs to fit each box.
                AdjustUVs(boxs);
                return boxs;

            }
        }

        #endregion




        #region Combine Welded

        struct WeldHelperVert
        {
            public Color c;
            public Vector2 u;
            public WeldHelperVert(Color col, Vector2 uv)
            {
                c = col;
                u = uv;
            }
        }


        public static Mesh Weld(Mesh mesh, float gap)
        {
            MeshExtended extended = new MeshExtended();
            extended.Prepare(mesh);
            UVPacker.Box boxes = UVPacker.Pack(extended, gap);

            #region Prepare a new mesh

            Mesh combined = new Mesh();
            List<Vector3> combinedVertices = new List<Vector3>();
            List<Vector2> combinedUVs = new List<Vector2>();
            List<Vector3> combinedNormals = new List<Vector3>();
            List<Color> combinedColors = new List<Color>();
            List<int> combinedTris = new List<int>();
            int triOffset = 0;

            #endregion

            // once welded and with the UVs packed we can just add items to lists.
            MeshExtended extmesh = boxes.Extended;
            Vector3[] vertices = extmesh.Vertices;
            Vector2[] uvs = boxes.PackedUVs;
            Vector3[] normals = extmesh.Normals;

            Dictionary<Vector3, WeldHelperVert> welds = new Dictionary<Vector3, WeldHelperVert>();
            for (int i = 0; i < extmesh.Vertices.Length; i++)
            {
                Vector3 vertPos = vertices[i];
                if (!welds.ContainsKey(vertPos))
                {
                    Color col = Color.white;
                    if (i < extmesh.Colors.Length) col = extmesh.Colors[i];
                    welds.Add(vertPos, new WeldHelperVert(col, extmesh.UVs[i]));
                }
            }

            int[] triangles = extmesh.Triangles;
            int vertexCount = vertices.Length;
            int triCount = triangles.Length;

            for (int v = 0; v < vertexCount; v++)
            {
                combinedVertices.Add(vertices[v]);
                combinedUVs.Add(uvs[v]);
                combinedNormals.Add(normals[v]);

                var weldHelp = welds[vertices[v]];
                combinedColors.Add(weldHelp.c);
            }

            for (int t = 0; t < triCount; t++) combinedTris.Add(triangles[t] + triOffset);

            // Create the new mesh
            combined.SetVertices(combinedVertices);
            combined.SetUVs(0, combinedUVs);
            combined.SetNormals(combinedNormals);
            combined.SetColors(combinedColors);
            combined.SetTriangles(combinedTris, 0);
            combined.RecalculateTangents();

            return combined;
        }

        #endregion


        #region Mesh Extended

        /// <summary>
        /// Extended Mesh class for use in Mesh combining.
        /// </summary>
        public class MeshExtended
        {
            public enum SizeMethod { Bounds, WorldScale }

            class VertexUV
            {
                public Vector3 Position;
                public Vector2 UV;
            }

            public Vector3[] Vertices;
            public Color[] Colors;
            public Vector2[] UVs;
            public Vector3[] Normals;
            public int[] Triangles;
            public Vector3 Size;

            /// <summary>
            /// Finds vertices to weld.
            /// </summary>
            static int FindWeld(List<VertexUV> list, Vector3 vertex, Vector2 uv)
            {
                // Find out if there's is a suitable vertext to weld.
                return list.FindIndex(e =>
                {
                    Vector3 p = e.Position;
                    Vector3 u = e.UV;
                    return p.x == vertex.x && p.y == vertex.y && p.z == vertex.z && u.x == uv.x && u.y == uv.y;
                    // Notice we take into account the UVs, otherwise we will lose texture info.
                });
            }

            /// <summary>
            /// Prepares a mesh to be combined.
            /// </summary>
            public void Prepare(Mesh mesh)
            {

                #region Get data from the mesh

                Vector3[] vertices = mesh.vertices;
                Vector2[] uvs = mesh.uv;
                Vector3[] normals = mesh.normals;
                Color[] colors = mesh.colors;
                int[] triangles = mesh.triangles;
                int vertexCount = vertices.Length;
                int triCount = triangles.Length;
                List<VertexUV> wVertices = new List<VertexUV>();
                List<Vector3> wNormals = new List<Vector3>();
                List<Color> wColor = new List<Color>();
                List<int> wTriangles = new List<int>();

                #endregion

                for (int v = 0; v < vertexCount; v++)
                {
                    Vector3 currentVertex = vertices[v];
                    Vector2 currentUV = uvs[v];
                    int index = FindWeld(wVertices, currentVertex, currentUV);

                    if (index == -1)
                    {
                        // no weld point found, so add the vertex and UV to the list.
                        wVertices.Add(new VertexUV { Position = currentVertex, UV = currentUV });
                        wNormals.Add(normals[v]);
                        if (colors.Length > 0) wColor.Add(colors[v]);
                        index = wVertices.Count - 1;
                    }
                    // the value of v and index will be always equal unless a weld point was found.
                    if (v != index)
                        for (int t = 0; t < triCount; t++)
                        {
                            // if there's a weld, we need to update the triangles.
                            if (triangles[t] == v)
                            {
                                triangles[t] = index;
                            }
                        }
                }

                // wait untill all the vertex are processed to update the triangle's list.
                for (int t = 0; t < triCount; t++)
                {
                    wTriangles.Add(triangles[t]);
                }


                // Save mesh data.
                Normals = wNormals.ToArray();
                Colors = wColor.ToArray();
                Triangles = wTriangles.ToArray();

                // Calculate the UV scale.
                Size = mesh.bounds.size;
                //Size = Vector3.one;

                // Save vertices and UVs
                int pcount = wVertices.Count;
                Vertices = new Vector3[pcount];
                UVs = new Vector2[pcount];

                for (int v = 0; v < pcount; v++)
                {
                    // convert local mesh coordinates to world space.
                    Vector3 worldPos = wVertices[v].Position;
                    // convert world space to the root's local space.
                    Vertices[v] = worldPos;
                    UVs[v] = wVertices[v].UV;
                }

            }


        }

        #endregion


    }
}