using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class TileMeshSetup
    {
        public class CubeGenerator
        {

            #region Params

            public Vector3 Scale;
            public Vector3Int Subdivisions;

            public float BevelSize;
            public int BevelSubdivs;

            public bool FaceFront;
            public bool FaceBack;
            public bool FaceTop;
            public bool FaceBottom;
            public bool FaceLeft;
            public bool FaceRight;

            public class CubePlane
            {
                public List<Vector3> verts;
                public List<int> vertsIndexes;
                public List<int> tris;

                public CubePlane(List<Vector3> baseVert, List<int> baseTris, List<int> vertIds)
                {
                    verts = baseVert;
                    tris = baseTris;
                    vertsIndexes = vertIds;
                }

                public void FindEdgesTopBottom(CubeGenerator g)
                {
                    float limitX = 0.1f * (g.Scale.x);
                    float limitZ = 0.1f * (g.Scale.z);

                    for (int v = 0; v < verts.Count; v++)
                    {
                        //UnityEngine.Debug.Log("vert["+v+"] = " + verts[v] + " limit = " + limitX);
                        if (verts[v].x > limitX) EdgeR.Add(vertsIndexes[v]);
                        else if (verts[v].x < -limitX) EdgeL.Add(vertsIndexes[v]);

                        if (verts[v].z > limitZ) EdgeF.Add(vertsIndexes[v]);
                        else if (verts[v].z < -limitZ) EdgeB.Add(vertsIndexes[v]);
                    }
                }

                public void FindEdgesLeftRight(CubeGenerator g)
                {
                    float limitZ = 0.1f * (g.Scale.z);
                    float limitY = 0.1f * (g.Scale.y);

                    for (int v = 0; v < verts.Count; v++)
                    {
                        if (verts[v].z > limitZ) EdgeR.Add(vertsIndexes[v]);
                        else if (verts[v].z < -limitZ) EdgeL.Add(vertsIndexes[v]);

                        if (verts[v].y > limitY) EdgeF.Add(vertsIndexes[v]);
                        else if (verts[v].y < -limitY) EdgeB.Add(vertsIndexes[v]);
                    }
                }

                public void FindEdgesFrontBack(CubeGenerator g)
                {
                    float limitX = 0.1f * (g.Scale.x);
                    float limitY = 0.1f * (g.Scale.y);

                    for (int v = 0; v < verts.Count; v++)
                    {
                        if (verts[v].x > limitX) EdgeR.Add(vertsIndexes[v]);
                        else if (verts[v].x < -limitX) EdgeL.Add(vertsIndexes[v]);

                        if (verts[v].y > limitY) EdgeF.Add(vertsIndexes[v]);
                        else if (verts[v].y < -limitY) EdgeB.Add(vertsIndexes[v]);
                    }
                }

                public List<int> EdgeL = new List<int>();
                public List<int> EdgeR = new List<int>();
                public List<int> EdgeF = new List<int>();
                public List<int> EdgeB = new List<int>();
            }

            CubePlane planeTop;
            CubePlane planeBottom;
            CubePlane planeLeft;
            CubePlane planeRight;
            CubePlane planeFront;
            CubePlane planeBack;

            internal Mesh GenerateMesh()
            {
                // Lists for the final cube mesh
                List<Vector3> verts = new List<Vector3>();
                List<Vector3> normals = new List<Vector3>();
                List<Vector2> uvs = new List<Vector2>();
                List<int> tris = new List<int>();


                planeTop = null;
                planeBottom = null;
                planeLeft = null;
                planeRight = null;
                planeFront = null;
                planeBack = null;

                // Non Beveled simple Cube
                // Lists for helper plane mesh

                float smallest = Scale.x;
                if (Scale.y < smallest) smallest = Scale.y;
                if (Scale.z < smallest) smallest = Scale.z;

                bool areSides = FaceRight || FaceLeft;
                bool isUpDown = FaceTop || FaceBottom;
                bool isFrontBack = FaceFront || FaceBack;

                float bevelV = smallest * BevelSize * 0.9f;
                Mesh plane = _GeneratePlane(Subdivisions.x, Subdivisions.z, new Vector2(Scale.x - bevelV * (areSides ? 1f : 0f), Scale.z - bevelV * (isFrontBack ? 1f : 0f)));
                List<Vector3> baseVert = new List<Vector3>();
                List<Vector2> baseUV = new List<Vector2>();
                List<int> baseTris = new List<int>();
                List<int> vertIds = new List<int>();

                plane.GetVertices(baseVert);
                plane.GetUVs(0, baseUV);
                plane.GetTriangles(baseTris, 0);
                int planeVerts = 0;


                if (FaceTop)
                {
                    plane.GetVertices(baseVert);
                    for (int t = 0; t < baseVert.Count; t++) baseVert[t] = new Vector3(baseVert[t].x, Scale.y / 2f, baseVert[t].z); // - S(baseVert[t].x ) * bevelV
                    for (int t = 0; t < baseVert.Count; t++) { vertIds.Add(verts.Count); verts.Add(baseVert[t]); normals.Add(Vector3.up); uvs.Add(baseUV[t]); }
                    for (int t = 0; t < baseTris.Count; t++) tris.Add(baseTris[t] + planeVerts);
                    planeVerts += baseVert.Count;
                    planeTop = new CubePlane(baseVert, baseTris, vertIds);
                    planeTop.FindEdgesTopBottom(this);
                }

                if (FaceBottom)
                {
                    vertIds.Clear();
                    plane.GetVertices(baseVert);
                    for (int t = 0; t < baseVert.Count; t++) baseVert[t] = new Vector3(baseVert[t].x, -Scale.y / 2f, baseVert[t].z);
                    for (int t = 0; t < baseVert.Count; t++) { vertIds.Add(verts.Count); verts.Add(baseVert[t]); normals.Add(Vector3.up); uvs.Add(baseUV[t]); }
                    baseTris.Reverse();
                    for (int t = 0; t < baseTris.Count; t++) tris.Add(baseTris[t] + planeVerts);
                    planeVerts += baseVert.Count;

                    planeBottom = new CubePlane(baseVert, baseTris, vertIds);
                    planeBottom.FindEdgesTopBottom(this);
                }

                plane = _GeneratePlane(Subdivisions.y, Subdivisions.z, new Vector2(Scale.y - bevelV, Scale.z - bevelV));
                plane.GetVertices(baseVert);
                plane.GetUVs(0, baseUV);
                plane.GetTriangles(baseTris, 0);


                if (FaceRight)
                {
                    vertIds.Clear();
                    plane.GetVertices(baseVert);
                    for (int t = 0; t < baseVert.Count; t++) baseVert[t] = (new Vector3(-Scale.x / 2f, baseVert[t].x, baseVert[t].z));
                    for (int t = 0; t < baseVert.Count; t++) { vertIds.Add(verts.Count); verts.Add(baseVert[t]); normals.Add(Vector3.right); uvs.Add(baseUV[t]); }
                    for (int t = 0; t < baseTris.Count; t++) tris.Add(baseTris[t] + planeVerts);
                    planeVerts += baseVert.Count;
                    planeRight = new CubePlane(baseVert, baseTris, vertIds);
                    planeRight.FindEdgesLeftRight(this);
                }

                if (FaceLeft)
                {
                    vertIds.Clear();
                    plane.GetVertices(baseVert);
                    for (int t = 0; t < baseVert.Count; t++) baseVert[t] = (new Vector3(Scale.x / 2f, baseVert[t].x, baseVert[t].z));
                    for (int t = 0; t < baseVert.Count; t++) { vertIds.Add(verts.Count); verts.Add(baseVert[t]); normals.Add(Vector3.left); uvs.Add(baseUV[t]); }
                    baseTris.Reverse();
                    for (int t = 0; t < baseTris.Count; t++) tris.Add(baseTris[t] + planeVerts);
                    planeVerts += baseVert.Count;
                    planeLeft = new CubePlane(baseVert, baseTris, vertIds);
                    planeLeft.FindEdgesLeftRight(this);
                }

                plane = _GeneratePlane(Subdivisions.x, Subdivisions.y, new Vector2(Scale.x - bevelV * (areSides ? 1f : 0f), Scale.y - bevelV * (areSides ? 1f : 0f)));
                plane.GetVertices(baseVert);
                plane.GetUVs(0, baseUV);
                plane.GetTriangles(baseTris, 0);

                if (FaceBack)
                {
                    vertIds.Clear();
                    plane.GetVertices(baseVert);
                    for (int t = 0; t < baseVert.Count; t++) baseVert[t] = (new Vector3(baseVert[t].x, baseVert[t].z, -Scale.z / 2f));
                    for (int t = 0; t < baseVert.Count; t++) { vertIds.Add(verts.Count); verts.Add(baseVert[t]); normals.Add(Vector3.forward); uvs.Add(baseUV[t]); }
                    for (int t = 0; t < baseTris.Count; t++) tris.Add(baseTris[t] + planeVerts);
                    planeVerts += baseVert.Count;
                    planeBack = new CubePlane(baseVert, baseTris, vertIds);
                    planeBack.FindEdgesFrontBack(this);
                }

                if (FaceFront)
                {
                    vertIds.Clear();
                    plane.GetVertices(baseVert);
                    for (int t = 0; t < baseVert.Count; t++) baseVert[t] = (new Vector3(baseVert[t].x, baseVert[t].z, Scale.z / 2f));
                    for (int t = 0; t < baseVert.Count; t++) { vertIds.Add(verts.Count); verts.Add(baseVert[t]); normals.Add(Vector3.back); uvs.Add(baseUV[t]); }
                    baseTris.Reverse();
                    for (int t = 0; t < baseTris.Count; t++) tris.Add(baseTris[t] + planeVerts);
                    planeFront = new CubePlane(baseVert, baseTris, vertIds);
                    planeFront.FindEdgesFrontBack(this);
                }


                if (BevelSize > 0f)
                {
                    #region Planes bevel connections

                    if (FaceTop)
                    {
                        if (FaceRight) ConnectAllSubdivsEdgePoly(planeTop.EdgeL, planeRight.EdgeF, tris, verts, false);
                        if (FaceLeft) ConnectAllSubdivsEdgePoly(planeTop.EdgeR, planeLeft.EdgeF, tris, verts, true);
                        if (FaceFront) ConnectAllSubdivsEdgePoly(planeTop.EdgeF, planeFront.EdgeF, tris, verts, false);
                        if (FaceBack) ConnectAllSubdivsEdgePoly(planeTop.EdgeB, planeBack.EdgeF, tris, verts, true);
                    }

                    if (FaceBottom)
                    {
                        if (FaceRight) ConnectAllSubdivsEdgePoly(planeBottom.EdgeL, planeRight.EdgeB, tris, verts, true);
                        if (FaceLeft) ConnectAllSubdivsEdgePoly(planeBottom.EdgeR, planeLeft.EdgeB, tris, verts, false);
                        if (FaceFront) ConnectAllSubdivsEdgePoly(planeBottom.EdgeF, planeFront.EdgeB, tris, verts, true);
                        if (FaceBack) ConnectAllSubdivsEdgePoly(planeBottom.EdgeB, planeBack.EdgeB, tris, verts, false);
                    }

                    if (FaceLeft && FaceBack) ConnectAllSubdivsEdgePoly(planeLeft.EdgeL, planeBack.EdgeR, tris, verts, false);
                    if (FaceRight && FaceFront) ConnectAllSubdivsEdgePoly(planeRight.EdgeR, planeFront.EdgeL, tris, verts, false);

                    if (FaceLeft && FaceFront) ConnectAllSubdivsEdgePoly(planeLeft.EdgeR, planeFront.EdgeR, tris, verts, true);
                    if (FaceRight && FaceBack) ConnectAllSubdivsEdgePoly(planeRight.EdgeL, planeBack.EdgeL, tris, verts, true);


                    // Fill 8 triangle gaps

                    #region Top Gaps

                    if (FaceBack && FaceTop && FaceRight) if (planeBack.EdgeF.Count > 0)
                        {
                            tris.Add(planeTop.EdgeL[0]);
                            tris.Add(planeBack.EdgeL[planeBack.EdgeL.Count - 1]);
                            tris.Add(planeRight.EdgeL[planeRight.EdgeL.Count - 1]);
                        }

                    if (FaceFront && FaceTop && FaceRight) if (planeRight.EdgeF.Count > 0)
                        {
                            tris.Add(planeRight.EdgeF[planeRight.EdgeF.Count - 1]);
                            tris.Add(planeFront.EdgeL[planeFront.EdgeL.Count - 1]);
                            tris.Add(planeTop.EdgeL[planeTop.EdgeL.Count - 1]);
                        }


                    if (FaceBack && FaceTop && FaceLeft) if (planeLeft.EdgeF.Count > 0)
                        {
                            tris.Add(planeLeft.EdgeL[planeLeft.EdgeL.Count - 1]);
                            tris.Add(planeBack.EdgeR[planeBack.EdgeR.Count - 1]);
                            tris.Add(planeTop.EdgeB[planeTop.EdgeB.Count - 1]);
                        }

                    if (FaceFront && FaceTop && FaceLeft) if (planeTop.EdgeF.Count > 0)
                        {
                            tris.Add(planeTop.EdgeR[planeTop.EdgeR.Count - 1]);
                            tris.Add(planeFront.EdgeR[planeFront.EdgeR.Count - 1]);
                            tris.Add(planeLeft.EdgeR[planeLeft.EdgeR.Count - 1]);
                        }

                    #endregion

                    #region Bottom Gaps

                    if (FaceBack && FaceBottom && FaceRight) if (planeRight.EdgeF.Count > 0)
                        {
                            tris.Add(planeRight.EdgeL[0]);
                            tris.Add(planeBack.EdgeL[0]);
                            tris.Add(planeBottom.EdgeL[0]);
                        }

                    if (FaceFront && FaceBottom && FaceRight) if (planeBottom.EdgeF.Count > 0)
                        {
                            tris.Add(planeBottom.EdgeL[planeBottom.EdgeL.Count - 1]);
                            tris.Add(planeFront.EdgeL[0]);
                            tris.Add(planeRight.EdgeR[0]);
                        }


                    if (FaceBack && FaceBottom && FaceLeft) if (planeBottom.EdgeB.Count > 0)
                        {
                            tris.Add(planeBottom.EdgeB[planeBottom.EdgeB.Count - 1]);
                            tris.Add(planeBack.EdgeR[0]);
                            tris.Add(planeLeft.EdgeL[0]);
                        }

                    if (FaceFront && FaceBottom && FaceLeft) if (planeLeft.EdgeR.Count > 0)
                        {
                            tris.Add(planeLeft.EdgeR[0]);
                            tris.Add(planeFront.EdgeR[0]);
                            tris.Add(planeBottom.EdgeR[planeBottom.EdgeR.Count - 1]);
                        }

                    #endregion


                    #endregion
                }


                // Generate Cube Mesh
                Mesh mesh = new Mesh();
                mesh.SetVertices(verts);
                mesh.SetTriangles(tris, 0);
                mesh.SetNormals(normals);
                mesh.SetUVs(0, uvs);
                mesh.RecalculateBounds();

                mesh.Optimize();


                return mesh;
            }

            void ConnectAllSubdivsEdgePoly(List<int> edgeA, List<int> edgeB, List<int> finalTris, List<Vector3> verts, bool reverse = false)
            {
                if (BevelSubdivs <= 1)
                {
                    for (int i = 0; i < edgeA.Count - 1; i++)
                        ConnectEdgePoly(i, edgeA, edgeB, finalTris, reverse);
                }
                else
                {
                    for (int i = 0; i < edgeA.Count - 1; i++)
                    {
                        if (i >= edgeB.Count) break;

                        Vector3 a1 = verts[edgeA[i]];
                        Vector3 a2 = verts[edgeA[i + 1]];
                        Vector3 b1 = verts[edgeB[i]];
                        Vector3 b2 = verts[edgeB[i + 1]];

                        Vector3 preV = a1;
                        Vector3 targetV = a2;


                        float step = 1f / (float)BevelSubdivs;

                        for (int b = 0; b < BevelSubdivs; b++)
                        {

                            if (b == 0) // Connect with start
                            {

                            }
                            else if (b == BevelSubdivs - 1) // Finalize
                            {
                                ConnectEdgePoly(i, edgeA, edgeB, finalTris, reverse);
                            }
                            else // Connect extra verts
                            {

                            }
                        }
                    }
                }
            }



            List<int> _toAddTris = new List<int>();
            void ConnectEdgePoly(int startIndex, List<int> edgeA, List<int> edgeB, List<int> finalTris, bool reverse = false)
            {
                _toAddTris.Clear();

                if (startIndex >= edgeB.Count) return;
                if (startIndex + 1 >= edgeB.Count) return;
                if (startIndex >= edgeA.Count) return;
                if (startIndex + 1 >= edgeA.Count) return;

                _toAddTris.Add(edgeA[startIndex]);
                _toAddTris.Add(edgeB[startIndex]);
                _toAddTris.Add(edgeA[startIndex + 1]);

                _toAddTris.Add(edgeB[startIndex]);
                _toAddTris.Add(edgeB[startIndex + 1]);
                _toAddTris.Add(edgeA[startIndex + 1]);

                if (reverse) _toAddTris.Reverse();

                for (int i = 0; i < _toAddTris.Count; i++) finalTris.Add(_toAddTris[i]);
            }

            #endregion


        }
    }
}