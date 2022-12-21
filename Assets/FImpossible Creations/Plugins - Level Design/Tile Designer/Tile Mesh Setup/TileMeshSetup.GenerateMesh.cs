using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class TileMeshSetup
    {
        public bool _tryWeldVertices = false;
        public bool _tryWeldVerticesV2 = false;
        public Vector3 _randomizeVerticesOffset = Vector3.zero;
        public Vector2 _randomizeVerticesNoiseScale = Vector2.one;

        public bool _customMeshOverwriteVertexColor = false;
        public Color _customMeshOverwriteVertexColorValues = Color.white;

        public List<Vector3> _CablePoints = new List<Vector3>() { new Vector3(0f, 0f, -0.5f), new Vector3(0f, 0f, 0.5f) };
        public float _CableRadius = 0.02f;
        public float _CableLoose = 1.5f;
        public float _CableHanging = 0f;
        public TileCableGenerator.CableMeshSettings _CableMeshSettings;
        public TileCableGenerator.CableTexturingSettings _CableTexturingSettings;
        public TileCableGenerator.CableClonerSettings _CableClonerSettings;
        public TileCableGenerator.CableRandomizationSettings _CableRandomizationSettings;

        public enum ECableView { Mesh, Texturing, Cloner, Randomization, CablePoints }
        public ECableView _CableView = ECableView.Mesh;


        public Mesh LatestGeneratedMesh { get; private set; }

        public Mesh FullGenerateMesh()
        {
            if (GenTechnique == EMeshGenerator.CustomMeshAndExtras)
            {
                if (ExtraMesh == EExtraMesh.CustomMesh)
                {
                    if (CustomMesh == null) return null;
                }

                LatestGeneratedMesh = GenerateCustomMesh();
                return LatestGeneratedMesh;
            }

            CheckInstances();
            PrepareCurves();
            QuickUpdate();
            GenerateMesh();

            return LatestGeneratedMesh;
        }


        private Mesh GenerateMesh()
        {
            if (GenTechnique == EMeshGenerator.Lathe)
                LatestGeneratedMesh = GenerateLathe(previewShape, new Vector2(width, height), Mathf.RoundToInt(_lathe_xSubdivCount), _lathe_fillAngle, HardNormals, UVMul, UVFit, NormalsMode);
            else if (GenTechnique == EMeshGenerator.Loft)
                LatestGeneratedMesh = GenerateLoft(previewShape, previewShape2, new Vector3(width, height, (width + height) * _loftDepthCurveWidener), HardNormals, UVMul, UVFit, NormalsMode, _loft_depthDim, _loft_height);
            else if (GenTechnique == EMeshGenerator.Extrude)
                LatestGeneratedMesh = GenerateExtrude(previewShape, new Vector3(width, height, depth), _extrudeFrontCap, _extrudeBackCap, HardNormals, UVMul, UVFit, NormalsMode, _extrudeMirror);
            else if (GenTechnique == EMeshGenerator.CustomMeshAndExtras)
                LatestGeneratedMesh = GenerateCustomMesh();
            else if (GenTechnique == EMeshGenerator.Primitive)
                LatestGeneratedMesh = GeneratePrimitiveMesh();
            else if (GenTechnique == EMeshGenerator.Sweep)
                LatestGeneratedMesh = GenerateSweep(previewShape, previewShape2, new Vector3(width, height, (width + height)), HardNormals, UVMul, UVFit, NormalsMode, _sweep_radiusMul, _sweep_radius, _sweep_Close);

            return LatestGeneratedMesh;
        }


        #region Generate with Loft

        //Vector3 CollapseNormal(Vector3 norm, float collapseAmount)
        //{
        //    norm.x = CollapseVal(norm.x, collapseAmount);
        //    norm.y = CollapseVal(norm.y, collapseAmount);
        //    norm.z = CollapseVal(norm.z, collapseAmount);
        //    return norm;
        //}

        //float CollapseVal(float val, float collapseAmount)
        //{
        //    if (val == -0.7071068f) val = Mathf.LerpUnclamped(val, -1f, collapseAmount);
        //    else if (val == 0.7071068f) val = Mathf.LerpUnclamped(val, 1f, collapseAmount);
        //    return val;
        //}




        public Mesh GenerateLoft(List<MeshShapePoint> shape, List<MeshShapePoint> shapeDistrib, Vector3 dimensions, float hardNormals, Vector2 uvMul, EUVFit uvFit, ENormalsMode normalsCompute, float distribDepth, List<CurvePoint> shapeHeight)
        {
            if (uvMul.x == 0f) uvMul.x = 1f;
            if (uvMul.y == 0f) uvMul.y = 1f;

            List<MeshVertPoint> shapeZY = new List<MeshVertPoint>();
            float zyLength = 0f;
            #region Shape ZY

            // Reference 2D shape for distributed mesh creation
            float lowestY = float.MaxValue, highestY = float.MinValue;

            for (int s = 0; s < shape.Count; s++)
            {
                MeshShapePoint sh = shape[s];
                MeshVertPoint m = new MeshVertPoint();

                m.pos = Vector3.zero;
                m.pos.y = (1f - sh.p.y) * dimensions.y;
                m.pos.z = (1f - sh.p.x - 1f) * dimensions.z * -0.5f;

                m.vCol = sh.c;
                m.norm = new Vector3(0, m.norm.y, -m.norm.x);

                if (m.pos.y < lowestY) lowestY = m.pos.y;
                if (m.pos.y > highestY) highestY = m.pos.y;

                shapeZY.Add(m);
                if (s > 0) zyLength += Vector3.Distance(m.pos, shapeZY[s - 1].pos);
            }


            #endregion

            float distrLength = 0f;
            List<MeshVertPoint> distr = new List<MeshVertPoint>();
            #region Distrib

            float lowestX = float.MaxValue, mostX = float.MinValue;

            for (int s = 0; s < shapeDistrib.Count; s++)
            {
                MeshShapePoint sh = shapeDistrib[s];
                MeshVertPoint m = new MeshVertPoint();

                m.pos = sh.p;
                m.pos.x = (sh.p.x - 0.5f) * dimensions.x;
                m.pos.y = 0f;
                m.pos.z = (-sh.p.y + 0.5f) * distribDepth;

                m.vCol = sh.c;
                m.norm = new Vector3(sh.normal.x, 0f, -sh.normal.y);

                if (m.pos.x < lowestX) lowestX = m.pos.x;
                if (m.pos.x > mostX) mostX = m.pos.x;

                distr.Add(m);
                if (s > 0) distrLength += Vector3.Distance(m.pos, distr[s - 1].pos);
            }


            #endregion

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();
            List<Color> vCol = new List<Color>();

            int subDivsMul = shapeZY.Count;

            if (distrLength == 0f) distrLength = 1f;
            if (zyLength == 0f) zyLength = 1f;

            float ratio = 1f / Mathf.Abs(highestY - lowestY);
            float xElapsed = 0f, yElapsed = 0f;


            #region Prepare height

            bool useHeight = false;
            float heightSplineLength = 0f;

            if (shapeHeight != null)
                if (shapeHeight.Count > 1)
                {
                    bool allZero = false;

                    for (int d = 0; d < shapeHeight.Count; d++)
                    {
                        if (d < shapeHeight.Count - 1)
                        {
                            heightSplineLength += Vector2.Distance(shapeHeight[d].localPos, shapeHeight[d + 1].localPos);
                        }

                        if (allZero == false) if (shapeHeight[d].localPos.y > 0.0001f) { allZero = false; }
                    }

                    if (!allZero) useHeight = true;
                }

            #endregion

            float collapsRev = 1f - _loft_Collapse;

            // Setup vertices
            for (int x = 0; x < distr.Count; x += 1)
            {
                var xStep = distr[x];
                float distrProgr = (float)((float)x / (float)(distr.Count - 1));

                for (int y = 0; y < shapeZY.Count; y += 1)
                {
                    var yStep = shapeZY[y];

                    if (_loft_Collapse > 0.001f)
                    {
                        //xStep.norm = CollapseNormal(xStep.norm, _loft_Collapse);
                    }

                    Vector3 dir = new Vector3(xStep.norm.x, 0f, xStep.norm.z);
                    Quaternion populateDir = Quaternion.identity;
                    if (dir.sqrMagnitude > Mathf.Epsilon) { populateDir = Quaternion.LookRotation(dir); }

                    float heightMul = 1f;
                    bool shiftDown = false;

                    if (useHeight)
                    {
                        heightMul = 1f - CurvePoint.GetPosAt(shapeHeight, distrProgr, heightSplineLength).y;
                        var hP = CurvePoint.GetPointAt(shapeHeight, distrProgr, heightSplineLength);
                        shiftDown = hP._Loft_Height_ShiftWhole;
                    }

                    // Collapse feature
                    if (_loft_Collapse > 0.001f)
                    {
                        xStep.pos.x *= collapsRev;
                        xStep.pos.z *= collapsRev;
                    }

                    // Vertex pos
                    Vector3 vPos = xStep.pos;

                    if (!shiftDown)
                        vPos.y = yStep.pos.y * heightMul;
                    else
                        vPos.y = yStep.pos.y - highestY * (1f - heightMul);

                    vPos += (populateDir * new Vector3(0f, 0f, yStep.pos.z * xStep.norm.magnitude));
                    verts.Add(vPos);
                    vCol.Add(yStep.vCol);

                    Vector3 normal = (xStep.norm + yStep.norm).normalized;
                    normals.Add(normal);


                    #region Prepare UV

                    Vector2 uv = new Vector2();

                    // Fit X 
                    if (uvFit == EUVFit.FitX)
                    {
                        uv.x = (1f - (xElapsed / distrLength)) * uvMul.x;
                        uv.y = (1f - (yElapsed / zyLength)) * heightMul * uvMul.y / ratio;
                    }
                    else if (uvFit == EUVFit.FitY)
                    {
                        uv.x = (1f - (xElapsed / distrLength)) * uvMul.x * ratio;
                        uv.y = (1f - (yElapsed / zyLength)) * heightMul * uvMul.y;
                    }
                    else if (uvFit == EUVFit.FitXY)
                    {
                        uv.x = (1f - (xElapsed / distrLength)) * uvMul.x;
                        uv.y = (1f - (yElapsed / zyLength)) * heightMul * uvMul.y;
                    }

                    uvs.Add(uv);

                    #endregion


                    #region Prepare tris

                    if (y < shapeZY.Count - 1 && x < distr.Count - 1)
                    {
                        int ls = x * (subDivsMul);
                        int uls = (x + 1) * (subDivsMul);

                        tris.Add(ls + y); // ld
                        tris.Add(ls + y + 1); // rd
                        tris.Add(uls + y); // lup

                        tris.Add(ls + y + 1); // rd
                        tris.Add(uls + y + 1); // rup
                        tris.Add(uls + y); // lup
                    }

                    #endregion


                    if (y < shapeZY.Count - 1) yElapsed += Vector3.Distance(shapeZY[y].pos, shapeZY[y + 1].pos);

                }

                yElapsed = 0f;
                if (x < distr.Count - 1) xElapsed += Vector3.Distance(distr[x].pos, distr[x + 1].pos);
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetColors(vCol);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);

            if (normalsCompute == ENormalsMode.NormalsAsSubdivView)
            {
                mesh.SetNormals(normals);
            }
            else
            {
                if (hardNormals <= 0f)
                {
                    mesh.RecalculateNormals();
                    mesh.RecalculateTangents();
                }
                else
                {
                    FMeshUtils.SmoothMeshNormals(mesh, hardNormals);
                }
            }

            mesh = FMeshUtils.AdjustOrigin(mesh, Origin);

            //mesh.Optimize();
            mesh.RecalculateBounds();

            return mesh;
        }



        #endregion


        #region Generate with Lathe


        public Mesh GenerateLathe(List<MeshShapePoint> shape, Vector2 dimensions, int subdivs, int fillTo, float hardNormals, Vector2 uvMul, EUVFit uvFit, ENormalsMode normalsCompute)
        {
            subdivs += 1;
            int subDivsMul = subdivs + 1;
            dimensions.x *= 0.5f;

            if (uvMul.x == 0f) uvMul.x = 1f;
            if (uvMul.y == 0f) uvMul.y = 1f;

            // Reference 2D shape for rounded mesh creation
            List<MeshVertPoint> latheShape = new List<MeshVertPoint>();
            float lowestY = float.MaxValue, highestY = float.MinValue, farthestOff = float.MinValue;

            for (int s = 0; s < shape.Count; s++)
            {
                MeshShapePoint sh = shape[s];
                MeshVertPoint m = new MeshVertPoint();

                Vector3 refPos = sh.p;
                refPos.x = 1f - refPos.x;
                refPos.y = -refPos.y + 1f;

                Vector3 newPos = Vector2.Scale(refPos, dimensions);
                m.pos = newPos;
                m.vCol = sh.c;
                m.norm = new Vector3(sh.normal.x, sh.normal.y, 0);

                if (newPos.y < lowestY) lowestY = newPos.y;
                if (newPos.y > highestY) highestY = newPos.y;
                if (newPos.x > farthestOff) farthestOff = newPos.x;

                latheShape.Add(m);
            }

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            //List<Vector4> tangents = new List<Vector4>();
            List<Vector2> uvs = new List<Vector2>();
            List<Color> vCol = new List<Color>();
            List<int> tris = new List<int>();

            float stepAngle = (float)fillTo / (float)(subdivs - 1f); // 360 or less fill step
            stepAngle *= Mathf.Deg2Rad; // when quat then comment this line

            float ratio = 1f / Mathf.Abs(highestY - lowestY);
            float xStep = 1f / (float)(subdivs);


            for (int y = 0; y < latheShape.Count; y += 1)
            {
                var lth = latheShape[y];

                for (int x = 0; x <= subdivs; x += 1)
                {
                    float xOff = Mathf.Sin(x * stepAngle);
                    float zOff = Mathf.Cos(x * stepAngle);

                    float yPos = lth.pos.y;


                    Vector3 angleOff = new Vector3(xOff * lth.pos.x, 0f, zOff * lth.pos.x);

                    // Vertex pos
                    Vector3 vPos = new Vector3(0, yPos, 0) + angleOff;
                    verts.Add(vPos);


                    #region Prepare normals and tangents

                    Vector3 norm = ((angleOff.normalized) + new Vector3(0f, lth.norm.y, 0f)).normalized;
                    normals.Add(norm);

                    //Vector3 tanHelp = Vector3.Cross(Vector3.forward, norm);
                    //Vector4 tan = tanHelp; tan.w = 1f;
                    //tangents.Add(tan);

                    #endregion


                    #region Prepare UV

                    Vector2 uv = new Vector2();

                    // Fit X 
                    if (uvFit == EUVFit.FitX)
                    {
                        uv.x = (1f - (x * xStep)) * uvMul.x;
                        uv.y = Mathf.InverseLerp(lowestY, highestY, yPos) * uvMul.y / ratio;
                    }
                    else if (uvFit == EUVFit.FitY)
                    {
                        uv.x = (1f - (x * xStep)) * uvMul.x * ratio;
                        uv.y = Mathf.InverseLerp(lowestY, highestY, yPos) * uvMul.y;
                    }
                    else if (uvFit == EUVFit.FitXY)
                    {
                        uv.x = (1f - (x * xStep)) * uvMul.x;
                        uv.y = Mathf.InverseLerp(lowestY, highestY, yPos) * uvMul.y;
                    }

                    uvs.Add(uv);

                    #endregion


                    #region Prepare tris

                    if (x < subdivs - 1 && y < latheShape.Count - 1)
                    {
                        int ls = y * (subDivsMul);
                        int uls = (y + 1) * (subDivsMul);

                        int xn = x + 1;
                        if (fillTo >= 360f) if (x == subdivs - 2) xn = 0;

                        tris.Add(ls + x); // ld
                        tris.Add(ls + xn); // rd
                        tris.Add(uls + x); // lup

                        tris.Add(ls + xn); // rd
                        tris.Add(uls + xn); // rup
                        tris.Add(uls + x); // lup
                    }

                    #endregion

                    vCol.Add(lth.vCol);
                }
            }


            verts.Reverse();
            tris.Reverse();
            normals.Reverse();
            //tangents.Reverse();
            uvs.Reverse();
            vCol.Reverse();

            Mesh mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetColors(vCol);
            mesh.SetTriangles(tris, 0);

            if (normalsCompute == ENormalsMode.NormalsAsSubdivView)
            {
                mesh.SetNormals(normals);
            }
            else
            {
                if (hardNormals <= 0f)
                {
                    //mesh.SetNormals(normals);
                    //mesh.SetTangents(tangents);

                    mesh.RecalculateNormals();
                    mesh.RecalculateTangents();
                }
                else
                {
                    FMeshUtils.SmoothMeshNormals(mesh, hardNormals);
                }
            }

            mesh.SetUVs(0, uvs);

            mesh = FMeshUtils.AdjustOrigin(mesh, Origin);

            ;
            mesh.RecalculateBounds();

            return mesh;
        }


        #endregion



        #region Generate with Sweep

        public Mesh GenerateSweep(List<MeshShapePoint> splineShape, List<MeshShapePoint> shapeCirc, Vector3 dimensions, float hardNormals, Vector2 uvMul, EUVFit uvFit, ENormalsMode normalsCompute, float radius, List<CurvePoint> sweepRadius, bool close)
        {
            if (uvMul.x == 0f) uvMul.x = 1f;
            if (uvMul.y == 0f) uvMul.y = 1f;

            if (radius < 0.01f) radius = 0.01f;

            List<MeshVertPoint> splineShapeXY = new List<MeshVertPoint>();
            float zyLength = 0f;

            #region Shape XY

            float lowestY = float.MaxValue, highestY = float.MinValue;

            for (int s = 0; s < splineShape.Count; s++)
            {
                MeshShapePoint sh = splineShape[s];
                MeshVertPoint m = new MeshVertPoint();

                m.pos = Vector3.zero;
                m.pos.y = (1f - sh.p.y) * dimensions.y;
                m.pos.x = (sh.p.x - 0.5f) * dimensions.x;
                m.pos.z = sh.p.z;

                m.vCol = sh.c;
                m.norm = sh.normal;

                if (m.pos.y < lowestY) lowestY = m.pos.y;
                if (m.pos.y > highestY) highestY = m.pos.y;

                splineShapeXY.Add(m);
                if (s > 0) zyLength += Vector3.Distance(m.pos, splineShapeXY[s - 1].pos);
            }

            #endregion

            float distrLength = 0f;
            List<MeshVertPoint> sweepCircShape = new List<MeshVertPoint>();
            #region Distrib

            float lowestX = float.MaxValue, mostX = float.MinValue;

            for (int s = 0; s < shapeCirc.Count; s++)
            {
                MeshShapePoint sh = shapeCirc[s];
                MeshVertPoint m = new MeshVertPoint();

                m.pos = sh.p;
                m.pos.x = (sh.p.x - 0.5f) * 2f;
                m.pos.z = (-sh.p.y + 0.5f) * 2f;
                m.pos.y = 0f;

                m.vCol = sh.c;

                //m.norm = new Vector3(sh.normal.x, 0f, -sh.normal.y);
                m.norm = sh.normal;

                if (m.pos.x < lowestX) lowestX = m.pos.x;
                if (m.pos.x > mostX) mostX = m.pos.x;

                sweepCircShape.Add(m);
                if (s > 0) distrLength += Vector3.Distance(m.pos, sweepCircShape[s - 1].pos);
            }

            if (close) sweepCircShape.Add(sweepCircShape[0]);

            #endregion

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();
            List<Color> vCol = new List<Color>();

            int subDivsMul = splineShapeXY.Count;

            if (distrLength == 0f) distrLength = 1f;
            if (zyLength == 0f) zyLength = 1f;

            float ratio = 1f / Mathf.Abs(highestY - lowestY);
            float xElapsed = 0f, yElapsed = 0f;


            #region Prepare height

            bool useRadius = false;
            float heightSplineLength = 0f;

            if (sweepRadius != null)
                if (sweepRadius.Count > 1)
                {
                    bool allZero = false;

                    for (int d = 0; d < sweepRadius.Count; d++)
                    {
                        if (d < sweepRadius.Count - 1)
                        {
                            heightSplineLength += Vector2.Distance(sweepRadius[d].localPos, sweepRadius[d + 1].localPos);
                        }

                        if (allZero == false) if (sweepRadius[d].localPos.y > 0.0001f) { allZero = false; }
                    }

                    if (!allZero) useRadius = true;
                }

            #endregion

            // Setup vertices
            for (int c = 0; c < sweepCircShape.Count; c += 1) // circle shape step
            {
                var circStepXZ = sweepCircShape[c];

                for (int s = 0; s < splineShapeXY.Count; s += 1) // move along path length
                {

                    #region Radius over path length calcs

                    float radiusMul = 1f;
                    if (useRadius)
                    {
                        float distrProgr = 1f - ((float)((float)s / (float)(splineShapeXY.Count - 1)));
                        radiusMul *= CurvePoint.GetPosAt(sweepRadius, distrProgr, heightSplineLength).x;
                    }

                    #endregion

                    // vert vPos Calcs
                    var yStep = splineShapeXY[s];
                    Vector3 vPos = yStep.pos; // origin position on path - ref for circle shape

                    #region Commented but can be helpful later

                    ////Quaternion populateDir = Quaternion.identity;

                    //float yAngle = Mathf.Atan2(circStepXZ.pos.x, -circStepXZ.pos.z) * Mathf.Rad2Deg;
                    //float xAngle = 40f;

                    //Quaternion shapeRot = Quaternion.Euler(xAngle, yAngle,0f);

                    //if (s == splineShapeXY.Count - 1)
                    //{
                    //    xAngle = Mathf.Atan2(yStep.norm.x, -yStep.norm.y) * Mathf.Rad2Deg;
                    //}
                    //else if (s == 0 )
                    //{
                    //    xAngle = Mathf.Atan2(yStep.norm.y, -yStep.norm.x) * Mathf.Rad2Deg;
                    //}
                    //else
                    //{
                    //    //populateDir = Quaternion.LookRotation(dir);
                    //    //if ( y == 0 ) UnityEngine.Debug.Log(yStep.norm.x +", " + yStep.norm.y + "  angle = " + (Mathf.Atan2(-yStep.norm.x, yStep.norm.y) * Mathf.Rad2Deg));
                    //    //angl = Mathf.Atan2(-yStep.norm.x, yStep.norm.y) * Mathf.Rad2Deg;
                    //}

                    //// Vertex pos
                    //Vector3 vPos = yStep.pos;

                    ////shapeRot = shapeRot * Quaternion.Euler(angl, 0f, 0);
                    //vPos += (shapeRot) * (Vector3.forward) * (radiusMul * radius);

                    ////float step = (float)x / ((float)sweepCircShape.Count);
                    ////vPos.y += step * 0.5f;

                    #endregion

                    float zAngle = Mathf.Atan2(yStep.norm.x, yStep.norm.y) * Mathf.Rad2Deg - 90;
                    float xAngle = 0f; // Z axis offset to implement
                    Quaternion pathRot = Quaternion.Euler(xAngle, 0f, zAngle);
                    Vector3 off = pathRot * circStepXZ.pos;
                    vPos += off * radius * radiusMul;


                    verts.Add(vPos);
                    vCol.Add(yStep.vCol);

                    Vector3 normal = (off.normalized).normalized;
                    normals.Add(normal);


                    #region Prepare UV

                    Vector2 uv = new Vector2();

                    // Fit X 
                    if (uvFit == EUVFit.FitX)
                    {
                        uv.x = (1f - (xElapsed / distrLength)) * uvMul.x;
                        uv.y = (1f - (yElapsed / zyLength)) * uvMul.y / ratio;
                    }
                    else if (uvFit == EUVFit.FitY)
                    {
                        uv.x = (1f - (xElapsed / distrLength)) * uvMul.x * ratio;
                        uv.y = (1f - (yElapsed / zyLength)) * uvMul.y;
                    }
                    else if (uvFit == EUVFit.FitXY)
                    {
                        uv.x = (1f - (xElapsed / distrLength)) * uvMul.x;
                        uv.y = (1f - (yElapsed / zyLength)) * uvMul.y;
                    }

                    uvs.Add(uv);

                    #endregion


                    #region Prepare tris

                    if (s < splineShapeXY.Count - 1 && c < sweepCircShape.Count - 1)
                    {
                        int ls = c * (subDivsMul);
                        int uls = (c + 1) * (subDivsMul);

                        tris.Add(ls + s); // ld
                        tris.Add(ls + s + 1); // rd
                        tris.Add(uls + s); // lup

                        tris.Add(ls + s + 1); // rd
                        tris.Add(uls + s + 1); // rup
                        tris.Add(uls + s); // lup
                    }

                    #endregion


                    if (s < splineShapeXY.Count - 1) yElapsed += Vector3.Distance(splineShapeXY[s].pos, splineShapeXY[s + 1].pos);

                }

                yElapsed = 0f;
                if (c < sweepCircShape.Count - 1) xElapsed += Vector3.Distance(sweepCircShape[c].pos, sweepCircShape[c + 1].pos);
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetColors(vCol);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);

            if (normalsCompute == ENormalsMode.NormalsAsSubdivView)
            {
                mesh.SetNormals(normals);
            }
            else
            {
                if (hardNormals <= 0f)
                {
                    mesh.RecalculateNormals();
                    mesh.RecalculateTangents();
                }
                else
                {
                    FMeshUtils.SmoothMeshNormals(mesh, hardNormals);
                }
            }

            mesh = FMeshUtils.AdjustOrigin(mesh, Origin);

            //mesh.Optimize();
            mesh.RecalculateBounds();

            return mesh;
        }



        #endregion



        #region Generate with Extrude



        public Mesh GenerateExtrude(List<MeshShapePoint> shape, Vector3 dimensions, bool extrudeFrontCap, bool extrudeBackCap, float hardNormals, Vector2 uvMul, EUVFit uvFit, ENormalsMode normalsCompute, bool extrudeMirror)
        {
            if (uvMul.x == 0f) uvMul.x = 1f;
            if (uvMul.y == 0f) uvMul.y = 1f;

            float lowestY = float.MaxValue, highestY = float.MinValue;
            float mostLeft = float.MaxValue, mostRight = float.MinValue;


            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();
            List<Color> vCol = new List<Color>();

            if (extrudeMirror)
            {

                List<MeshVertPoint> fullSymmShape = new List<MeshVertPoint>();
                float shpLength = 0f;

                #region Shape 

                // Reference 2D shape for distributed mesh creation
                for (int s = 1; s < shape.Count - 1; s++) // Remove extra symmetry points
                {
                    MeshShapePoint sh = shape[s];
                    MeshVertPoint m = new MeshVertPoint();

                    m.pos = sh.p;
                    m.pos.y = 1f - m.pos.y;
                    m.pos.x = (1f - m.pos.x);
                    m.vCol = sh.c;

                    m.norm = sh.normal;

                    fullSymmShape.Add(m);
                    if (s > 0) shpLength += Vector3.Distance(m.pos, fullSymmShape[s - 1].pos);

                    float yRev = 1f - m.pos.y;
                    float xRev = (m.pos.x) * -1f;
                    if (yRev < lowestY) lowestY = yRev;
                    if (yRev > highestY) highestY = yRev;
                    if (xRev < mostLeft) mostLeft = xRev;
                }

                //UnityEngine.Debug.Log(" b " + lowestY + " u " + highestY + " l " + mostLeft);

                int symOff = shape.Count - 2;

                // Prepare symmetry points
                for (int s = 1; s < shape.Count - 1; s++)
                {
                    MeshShapePoint sh = shape[s];
                    MeshVertPoint m = new MeshVertPoint();

                    m.pos = sh.p;
                    m.pos.y = 1f - m.pos.y;
                    m.pos.x = (1f - m.pos.x) * -1f;
                    m.vCol = sh.c;
                    m.norm = sh.normal;

                    fullSymmShape.Add(m);
                }


                //string repoty = "";
                //for (int i = 0; i < fullSymmShape.Count; i++)
                //{
                //    repoty += "[" + i + "] " + fullSymmShape[i].pos + "   ";
                //}
                //UnityEngine.Debug.Log(repoty);

                #endregion



                // First cap shape
                for (int v = 0; v < fullSymmShape.Count; v += 1)
                {
                    var yStep = fullSymmShape[v];

                    // Vertex pos
                    Vector3 vPos = yStep.pos;
                    vPos.x *= width * 0.5f; // Symmetry mul
                    vPos.y *= height;
                    vCol.Add(yStep.vCol);
                    verts.Add(vPos);
                }


                // Flat cap in z depth
                for (int v = 0; v < fullSymmShape.Count; v += 1)
                {
                    var yStep = fullSymmShape[v];

                    // Vertex pos
                    Vector3 vPos = yStep.pos;
                    vPos.x *= width * 0.5f; // Symmetry mul
                    vPos.y *= height;
                    vPos.z = -dimensions.z;
                    vCol.Add(yStep.vCol);
                    verts.Add(vPos);
                }


                if (extrudeFrontCap)
                {
                    // Front cap
                    for (int v = 0; v < symOff - 1; v += 1)
                    {
                        tris.Add(symOff + v);
                        tris.Add(v);
                        tris.Add(v + 1);

                        tris.Add(v + 1);
                        tris.Add(symOff + v + 1);
                        tris.Add(symOff + v);
                    }
                }

                if (extrudeBackCap)
                {
                    int start = symOff * 2;
                    // Back cap
                    for (int v = start; v < start + symOff - 1; v += 1)
                    {
                        tris.Add(v + 1);
                        tris.Add(v);
                        tris.Add(symOff + v);

                        tris.Add(symOff + v);
                        tris.Add(symOff + v + 1);
                        tris.Add(v + 1);
                    }
                }


                #region Compute UVs


                //if (false)
                {

                    // From down to up points of one cap
                    for (int v = 0; v < fullSymmShape.Count; v += 1)
                    {
                        var p = fullSymmShape[v];

                        #region Prepare UV

                        Vector2 uv = new Vector2();

                        float xPos = p.pos.x;
                        float yPos = 1f - p.pos.y;

                        float yUv = FLogicMethods.InverseLerpUnclamped(lowestY, highestY, yPos);
                        float xUv = FLogicMethods.InverseLerpUnclamped(mostLeft, -mostLeft, xPos);

                        //if (v == fullSymmShape.Count - 1)
                        //{
                        //    yUv = FLogicMethods.InverseLerpUnclamped(lowestY, highestY, yPos + dimensions.z);
                        //    xUv = FLogicMethods.InverseLerpUnclamped(mostLeft, -mostLeft, xPos);
                        //}

                        uv.x = ((1f - xUv)) * uvMul.x;
                        uv.y = (1f - (yUv)) * uvMul.y;

                        uvs.Add(uv);

                        #endregion
                    }


                    // From down to up points with extrude depth cap
                    for (int v = symOff; v < fullSymmShape.Count + symOff; v += 1)
                    {
                        var p = fullSymmShape[v - symOff];

                        #region Prepare UV

                        Vector2 uv = new Vector2();

                        float xPos = p.pos.x;
                        float yPos = 1f - p.pos.y;

                        float yUv = FLogicMethods.InverseLerpUnclamped(lowestY, highestY, yPos);
                        float xUv = FLogicMethods.InverseLerpUnclamped(mostLeft, -mostLeft, xPos);

                        //if ( v == symOff || v == fullSymmShape.Count + symOff-1 )
                        //if ( v < fullSymmShape.Count + symOff - 2)
                        //{
                        //    yUv = FLogicMethods.InverseLerpUnclamped(lowestY, highestY, yPos + dimensions.z);
                        //}

                        uv.x = ((xUv)) * uvMul.x;
                        uv.y = (1f - (yUv)) * uvMul.y;

                        uvs.Add(uv);

                        #endregion
                    }

                    //FGenerators.SwapElements(uvs, 0, uvs.Count/2);
                }


                // From down to up points of one cap
                //for (int v = 0; v < fullSymmShape.Count; v += 1)
                //{
                //    var p = fullSymmShape[v];

                //    #region Prepare UV

                //    Vector2 uv = new Vector2();

                //    float xPos = p.pos.x;
                //    float yPos = 1f - p.pos.y;

                //    float yUv = Mathf.InverseLerp(lowestY, highestY, yPos);
                //    float xUv = Mathf.InverseLerp(mostLeft, -mostLeft, xPos);

                //    uv.x = ((1f - xUv));
                //    uv.y = (1f - (yUv));

                //    uv = Vector2.Scale(UVMul, uv);
                //    uvs.Add(uv);

                //    #endregion
                //}

                //// From down to up points of one cap
                //for (int v = 0; v < fullSymmShape.Count; v += 1)
                //{
                //    var p = fullSymmShape[v];

                //    #region Prepare UV

                //    Vector2 uv = new Vector2();

                //    float xPos = p.pos.x;
                //    float yPos = 1f - p.pos.y;

                //    float yUv = Mathf.InverseLerp(lowestY, highestY, yPos);
                //    float xUv = Mathf.InverseLerp(mostLeft, -mostLeft, xPos);

                //    uv.x = ((1f - xUv));
                //    uv.y = (1f - (yUv));

                //    uv.x = 1f - uv.x;
                //    uv.y = 1f - uv.y;

                //    uv = Vector2.Scale(UVMul, uv);
                //    uvs.Add(uv);

                //    #endregion
                //}


                #endregion


                #region Sides Extrude

                if (dimensions.z > -0.000001f && dimensions.z < 0.000001f)
                {

                }
                else
                {

                    // Left Side
                    for (int v = symOff; v < symOff + symOff - 1; v += 1)
                    {
                        tris.Add(symOff * 2 + v);
                        tris.Add(v);
                        tris.Add(v + 1);

                        tris.Add(v + 1);
                        tris.Add(symOff * 2 + v + 1);
                        tris.Add(symOff * 2 + v);
                    }

                    // Right Side
                    for (int v = 0; v < symOff - 1; v += 1)
                    {
                        tris.Add(v + 1);
                        tris.Add(v);
                        tris.Add(symOff * 2 + v);

                        tris.Add(symOff * 2 + v);
                        tris.Add(symOff * 2 + v + 1);
                        tris.Add(v + 1);
                    }


                    // Fill top and bottom joining

                    // Bottom
                    tris.Add(0);
                    tris.Add(symOff);
                    tris.Add(symOff * 2);

                    tris.Add(symOff * 3);
                    tris.Add(symOff * 2);
                    tris.Add(symOff);

                    // Top
                    tris.Add(symOff * 3 - 1);
                    tris.Add(symOff * 2 - 1);
                    tris.Add(symOff - 1);

                    tris.Add(symOff * 3 - 1);
                    tris.Add(symOff * 4 - 1);
                    tris.Add(symOff * 2 - 1);

                }

                #endregion


            }
            else // Extrude not symmetrical
            {

                List<FMeshUtils.PolyShapeHelpPoint> vGenPoints = new List<FMeshUtils.PolyShapeHelpPoint>();

                #region Triangulating poly shape preparation

                for (int s = 0; s < shape.Count; s++)
                {
                    var vGen = new FMeshUtils.PolyShapeHelpPoint(shape[s].p);
                    vGen.vxPos.z = 1f - vGen.vxPos.y;
                    vGen.vxPos.y = 0f;
                    vGen.vxPos.x *= dimensions.x;
                    vGen.vxPos.z *= dimensions.y;

                    vGen.helpIndex = s;

                    vGenPoints.Add(vGen);

                    if (vGen.vxPos.x < mostLeft) mostLeft = vGen.vxPos.x;
                    if (vGen.vxPos.x > mostRight) mostRight = vGen.vxPos.x;

                    if (vGen.vxPos.z < lowestY) lowestY = vGen.vxPos.z;
                    if (vGen.vxPos.z > highestY) highestY = vGen.vxPos.z;
                }

                #endregion

                vGenPoints.Reverse();

                for (int p = 0; p < vGenPoints.Count; p++)
                {
                    var vPoint = vGenPoints[p];
                    vPoint.index = p;
                    verts.Add(new Vector3(vPoint.vxPos.x, vPoint.vxPos.z, 0f));
                }

                bool twoCaps = false;
                if (dimensions.z != 0f || (extrudeFrontCap || extrudeBackCap))
                {
                    twoCaps = true;
                    for (int p = 0; p < vGenPoints.Count; p++)
                    {
                        var vPoint = vGenPoints[p];
                        verts.Add(new Vector3(vPoint.vxPos.x, vPoint.vxPos.z, -dimensions.z));
                    }
                }


                List<int> frontCapTris = null;

                if (extrudeFrontCap)
                {
                    frontCapTris = FMeshUtils.TriangulateConcavePolygon(vGenPoints);

                    #region Applying triangulated cap

                    if (extrudeFrontCap)
                    {
                        for (int i = frontCapTris.Count - 1; i >= 0; i--)
                        //for (int i = 0; i < frontCapTris.Count; i++)
                        {
                            tris.Add(frontCapTris[i]);
                        }
                    }

                    #endregion
                }


                #region Applying sides poly

                if (dimensions.z != 0f)
                {
                    int depthOff = vGenPoints.Count;
                    for (int i = 0; i < vGenPoints.Count - 1; i += 1)
                    {
                        // generating triangle bridge
                        // u -> uf   uf -> d   d -> df
                        tris.Add(vGenPoints[i].index);
                        tris.Add(vGenPoints[i].index + depthOff + 1);
                        tris.Add(vGenPoints[i].index + 1);

                        tris.Add(vGenPoints[i].index + depthOff + 1);
                        tris.Add(vGenPoints[i].index);
                        tris.Add(vGenPoints[i].index + depthOff);
                    }

                    // Loop last poly
                    tris.Add(vGenPoints[vGenPoints.Count - 1].index);
                    tris.Add(depthOff);
                    tris.Add(0);

                    tris.Add(depthOff);
                    tris.Add(vGenPoints[vGenPoints.Count - 1].index);
                    tris.Add(vGenPoints[vGenPoints.Count - 1].index + depthOff);
                }

                #endregion


                if (extrudeBackCap)
                {
                    if (frontCapTris == null) frontCapTris = FMeshUtils.TriangulateConcavePolygon(vGenPoints);

                    #region Applying triangulated cap

                    for (int i = 0; i < frontCapTris.Count; i++)
                    //for (int i = frontCapTris.Count - 1; i >= 0; i--)
                    {
                        tris.Add(frontCapTris[i] + vGenPoints.Count);
                    }

                    #endregion
                }


                #region Prepare UV for Caps

                for (int v = 0; v < vGenPoints.Count; v++)
                {
                    vCol.Add(shape[vGenPoints[v].helpIndex].c);
                    Vector2 uvVal = new Vector2();
                    uvVal.x = FLogicMethods.InverseLerpUnclamped(mostLeft, mostRight, shape[vGenPoints[v].helpIndex].p.x);
                    uvVal.y = FLogicMethods.InverseLerpUnclamped(lowestY, highestY, shape[vGenPoints[v].helpIndex].p.y);
                    uvs.Add(uvVal);
                }

                if (twoCaps)
                {
                    float uvDepth = dimensions.z;
                    for (int v = 0; v < vGenPoints.Count; v++)
                    {
                        vCol.Add(shape[vGenPoints[v].helpIndex].c);
                        Vector2 uvVal = new Vector2();
                        uvVal.x = FLogicMethods.InverseLerpUnclamped(mostLeft, mostRight, shape[vGenPoints[v].helpIndex].p.x);
                        uvVal.y = FLogicMethods.InverseLerpUnclamped(lowestY + uvDepth, highestY + uvDepth, shape[vGenPoints[v].helpIndex].p.y);
                        uvs.Add(uvVal);
                    }
                }

                for (int u = 0; u < uvs.Count; u++)
                {
                    uvs[u] = Vector2.Scale(uvs[u], UVMul);
                }

                #endregion

            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(vCol);

            if (hardNormals <= 0f)
            {
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();
            }
            else
            {
                FMeshUtils.SmoothMeshNormals(mesh, hardNormals);
            }

            mesh = FMeshUtils.AdjustOrigin(mesh, Origin);

            mesh.RecalculateBounds();

            return mesh;
        }


        #endregion


        #region Generate Custom Mesh

        Mesh GenerateCustomMesh()
        {

            if (ExtraMesh == EExtraMesh.CustomMesh)
            {
                if (CustomMesh == null) return null;

                Mesh m = GameObject.Instantiate(CustomMesh);

                if (_customMeshOverwriteVertexColor)
                {
                    List<Color> cols = new List<Color>();
                    for (int c = 0; c < m.vertexCount; c++) cols.Add(_customMeshOverwriteVertexColorValues);
                    FMeshUtils.SetColorsUnity2018(m, cols);
                }

                if (Origin != EOrigin.Unchanged) FMeshUtils.AdjustOrigin(m, Origin);

                return m;
            }
            else if (ExtraMesh == EExtraMesh.CableGenerator)
            {
                Mesh m = TileCableGenerator.GenerateFullCablesMesh(_CablePoints, _CableLoose, _CableHanging, _CableRadius, _CableMeshSettings, _CableTexturingSettings, _CableClonerSettings, _CableRandomizationSettings, null);
                if (Origin != EOrigin.Unchanged) FMeshUtils.AdjustOrigin(m, Origin);

                return m;
            }


            return null;
        }

        #endregion


        #region Generate Primitive Mesh

        Mesh GeneratePrimitiveMesh()
        {
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            if (_primitive_Type == EPrimitiveType.Cube)
            {
                _Primitive_Cube_ClampParams();

                CubeGenerator cube = new CubeGenerator();

                #region Adjust Generator

                cube.Scale = _primitive_scale;
                cube.Subdivisions = _primitive_plane_subdivs;
                cube.BevelSize = _primitive_cube_bevel;
                cube.BevelSubdivs = _primitive_cube_bevelSubdivs;

                cube.FaceTop = _primitive_cube_topFace;
                cube.FaceBottom = _primitive_cube_bottomFace;
                cube.FaceLeft = _primitive_cube_leftFace;
                cube.FaceRight = _primitive_cube_rightFace;
                cube.FaceFront = _primitive_cube_frontFace;
                cube.FaceBack = _primitive_cube_backFace;

                #endregion

                Mesh cubeMesh = cube.GenerateMesh();
                cubeMesh.GetVertices(verts);
                cubeMesh.GetNormals(normals);
                cubeMesh.GetUVs(0, uvs);
                cubeMesh.GetTriangles(tris, 0);
            }
            else if (_primitive_Type == EPrimitiveType.Plane)
            {
                _Primitive_Plane_ClampParams();

                Mesh plane = _GeneratePlane(_primitive_plane_subdivs.x, _primitive_plane_subdivs.y, Vector2.one);
                plane.GetVertices(verts);
                plane.GetNormals(normals);
                plane.GetUVs(0, uvs);
                plane.GetTriangles(tris, 0);
            }
            else if (_primitive_Type == EPrimitiveType.Sphere)
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Mesh sphere = GameObject.Instantiate(obj.GetComponent<MeshFilter>().sharedMesh);
                sphere.GetVertices(verts);
                sphere.GetNormals(normals);
                sphere.GetUVs(0, uvs);
                sphere.GetTriangles(tris, 0);
                FGenerators.DestroyObject(obj);
            }
            else if (_primitive_Type == EPrimitiveType.Cylinder)
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Mesh cylinder = GameObject.Instantiate(obj.GetComponent<MeshFilter>().sharedMesh);
                cylinder.GetVertices(verts);
                cylinder.GetNormals(normals);
                cylinder.GetUVs(0, uvs);
                cylinder.GetTriangles(tris, 0);
                FGenerators.DestroyObject(obj);
            }

            Mesh mesh = new Mesh();

            if (_primitive_Type != EPrimitiveType.Cube)
                if (_primitive_scale != Vector3.one)
                    for (int v = 0; v < verts.Count; v++)
                    {
                        verts[v] = Vector3.Scale(verts[v], _primitive_scale);
                    }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);

            if (NormalsMode == ENormalsMode.NormalsAsSubdivView)
            {
                mesh.SetNormals(normals);
            }
            else
            {
                if (HardNormals <= 0f)
                {
                    mesh.RecalculateNormals();
                    mesh.RecalculateTangents();
                }
                else
                {
                    FMeshUtils.SmoothMeshNormals(mesh, HardNormals);
                }
            }


            if (_customMeshOverwriteVertexColor)
            {
                List<Color> cols = new List<Color>();
                for (int c = 0; c < mesh.vertexCount; c++) cols.Add(_customMeshOverwriteVertexColorValues);
                FMeshUtils.SetColorsUnity2018(mesh, cols);
            }


            if (_tryWeldVertices) // Adjustable subdivs cube
            {
                if (_tryWeldVerticesV2)
                    mesh = FMeshUtils.Weld2(mesh, mesh.bounds.max.magnitude * 0.01f);
                else
                    mesh = FMeshUtils.Weld(mesh, mesh.bounds.max.magnitude * 0.01f);

                mesh.Optimize();
            }

            mesh.GetVertices(verts);

            if (_randomizeVerticesOffset != Vector3.zero)
            {
                Vector2 rand = new Vector2(FGenerators.GetRandom(0f, 1000f), FGenerators.GetRandom(0f, 1000f));
                Vector2 rand2 = new Vector2(FGenerators.GetRandom(0f, 1000f), FGenerators.GetRandom(0f, 1000f));
                Vector2 rand3 = new Vector2(FGenerators.GetRandom(0f, 1000f), FGenerators.GetRandom(0f, 1000f));

                for (int v = 0; v < verts.Count; v++)
                    verts[v] += new Vector3
                        (
                            Mathf.PerlinNoise((rand.x + v) * _randomizeVerticesNoiseScale.x,
                            (rand.y + v) * _randomizeVerticesNoiseScale.y) * _randomizeVerticesOffset.x,

                            Mathf.PerlinNoise((rand2.x + v) * _randomizeVerticesNoiseScale.y,
                            (rand2.y + v) * _randomizeVerticesNoiseScale.y) * _randomizeVerticesOffset.y,

                            Mathf.PerlinNoise((rand3.x + v) * _randomizeVerticesNoiseScale.x,
                            (rand3.y + v) * _randomizeVerticesNoiseScale.y) * _randomizeVerticesOffset.z
                        );
            }

            mesh.SetVertices(verts);

            if (NormalsMode != ENormalsMode.NormalsAsSubdivView)
            {
                if (HardNormals <= 0f)
                {
                    mesh.RecalculateNormals();
                    mesh.RecalculateTangents();
                }
                else
                {
                    FMeshUtils.SmoothMeshNormals(mesh, HardNormals);
                }
            }

            mesh = FMeshUtils.AdjustOrigin(mesh, Origin);
            mesh.RecalculateBounds();
            //mesh.Optimize();

            return mesh;
        }


        internal static Mesh _GeneratePlane(int xSub, int ySub, Vector2 size)
        {
            if (xSub <= 0) xSub = 1;
            if (ySub <= 0) ySub = 1;

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            ySub += 1;
            xSub += 1;

            for (int y = 0; y < ySub; y++)
                for (int x = 0; x < xSub; x++)
                {
                    float tx = (float)x / (float)(xSub - 1);
                    float ty = (float)y / (float)(ySub - 1);

                    float pX = (-0.5f + tx) * size.x;
                    float pZ = (-0.5f + ty) * size.y;

                    vertices.Add(new Vector3(pX, 0, pZ));
                    normals.Add(Vector3.up);
                    uvs.Add(new Vector2(tx, ty));
                }

            List<int> tris = new List<int>();

            for (int y = 0; y < (ySub - 1); y++)
                for (int x = 0; x < (xSub - 1); x++)
                {
                    int quad = y * xSub + x;

                    tris.Add(quad);
                    tris.Add(quad + xSub);
                    tris.Add(quad + xSub + 1);

                    tris.Add(quad);
                    tris.Add(quad + xSub + 1);
                    tris.Add(quad + 1);
                }


            var mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.normals = normals.ToArray();
            mesh.RecalculateBounds();

            return mesh;
        }


        internal void _Primitive_Plane_ClampParams()
        {
            if (_primitive_plane_subdivs.x < 0) _primitive_plane_subdivs.x = 1;
            if (_primitive_plane_subdivs.x > 250) _primitive_plane_subdivs.x = 250;
            if (_primitive_plane_subdivs.y < 0) _primitive_plane_subdivs.y = 1;
            if (_primitive_plane_subdivs.y > 250) _primitive_plane_subdivs.y = 250;

            if (_primitive_cube_backFace == false && _primitive_cube_frontFace == false && _primitive_cube_leftFace == false && _primitive_cube_rightFace == false && _primitive_cube_topFace == false && _primitive_cube_bottomFace == false)
            {
                _primitive_scale = Vector3.one;
                _primitive_cube_topFace = true;
                _primitive_cube_bottomFace = true;
                _primitive_cube_leftFace = true;
                _primitive_cube_rightFace = true;
                _primitive_cube_frontFace = true;
                _primitive_cube_backFace = true;
                _primitive_cube_bevel = 0f;
                _primitive_cube_bevelSubdivs = 1;
                _primitive_plane_subdivs = new Vector3Int(1, 1, 1);
            }
        }

        internal void _Primitive_Cube_ClampParams()
        {
            _Primitive_Plane_ClampParams();
            if (_primitive_plane_subdivs.z < 0) _primitive_plane_subdivs.z = 1;
            if (_primitive_plane_subdivs.z > 250) _primitive_plane_subdivs.z = 250;
            if (_primitive_cube_bevel < 0) _primitive_cube_bevel = 0f;
            if (_primitive_cube_bevelSubdivs < 1) _primitive_cube_bevelSubdivs = 1;
        }

        #endregion


        #region Helpers

        struct MeshVertPoint
        {
            public Vector3 pos;
            public Color vCol;
            public Vector3 norm;
            public Vector2 UV;

            public MeshVertPoint(Vector3 pos)
            {
                this.pos = pos;
                vCol = Color.white;
                norm = Vector3.zero;
                UV = Vector3.zero;
            }
        }



        #endregion

    }
}