using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public static class TileCableGenerator
    {

        #region Mesh Variables

        [System.Serializable]
        public class CableMeshSettings
        {
            [Range(1, 36)]
            public int LengthSubdivs = 12;
            [Range(3, 20)]
            public int CircleSubdivs = 6;
            [Range(0, 360)]
            public float RollOffset = 0;

            [Space(3)]
            [Tooltip("Joininig end vertices of trail points when you use more than two cable points")]
            public bool JoinEnds = false;
            //public bool SmoothNormals = false;
        }

        #endregion


        #region Texturing Variables

        [System.Serializable]
        public class CableTexturingSettings
        {
            public float LengthTiling = 4f;
            public float VerticalTiling = 1f;
            [Range(0, 360)] public float UVRotate = 0;
        }

        #endregion


        #region Cloner Variables

        [System.Serializable]
        public class CableClonerSettings
        {
            [Tooltip("Cable clones count in X Y and Z axis")]
            public Vector3Int InstancesCount = new Vector3Int(1, 1, 1);
            [Tooltip("Distance between each clone")]
            public Vector3 ClonesOffsets = Vector3.one;
            [Tooltip("Quick adjust clones offset value")]
            [Range(0f, 2f)] public float ScaleOffsets = 1f;

            [Space(2)]
            [Tooltip("Generating XY clones in circular shape instead of rect shape")]
            public bool CircularGrid = false;

            [Space(5)]
            [Tooltip("If you're creating cable curve using multiple path points it can help getting desired shape of the cloned cables")]
            public bool PathReGenerate = false;
            [Tooltip("If using multiple path points, it will use dominant axis for clone direction at path start and at end")]
            public bool FlattenEnds = false;
        }

        #endregion


        #region Randomization Variables

        [System.Serializable]
        public class CableRandomizationSettings
        {
            [Tooltip("When X = 0.2 it will offset trail to the right/left with random position offset from -0.2 o 0.2")]
            public Vector2 RandomizeTrails = Vector2.zero;
            [Tooltip("How frequent should be random disturbances on the cable shape")]
            public float NoiseScale = 0.5f;
            [Tooltip("When X = 0.8 and Y = 1.2 it will multiply 'Loose' parameter with randomized values from 0.8 o 1.2")]
            public Vector2 RandomizeLoose = Vector2.one;

            [Space(4)]
            [Tooltip("Randomizing position of target path points")]
            public Vector3 RandomizePathPoints = Vector3.zero;

            [Tooltip("Cut out some random clones")]
            public Vector2Int CutOutClones = Vector2Int.zero;
        }

        #endregion


        #region Attachement Variables

        [System.Serializable]
        public class CableAttachementSettings
        {
            public Mesh Mesh;
            public Material Material;
            [Space(3)]
            public Vector3 Offset = Vector3.zero;
            public Vector3 Rotation = Vector3.zero;
            public bool FlatRotation = true;
            public Vector3 Scale = Vector3.one;
            public float ScaleMultiplier = 1f;
            [Space(3)]
            public bool AddOnStart = true;
            public bool AddInTheMiddle = true;
            public bool AddOnTheEnd = true;
            [Space(3)]
            public bool ApplyToAllClones = false;
        }

        #endregion



        static Mesh cablesMesh = null;

        static Vector3[] _vertices = null;
        static int[] _tris = null;
        static int[] _trisRev = null;
        static int[] _trisCircleHelperCurrent = null;
        static int[] _trisCircleHelperPre = null;


        #region Sweep Elements Preparation Methods

        /// <summary> Looped X-Y cable ring shape with +-1.0 scale offset </summary>
        static List<Vector3> cableCircle = new List<Vector3>();

        public static void GenerateCableCircle(List<Vector3> cableRing, int ringSubdivs)
        {
            if (ringSubdivs < 2) ringSubdivs = 2;
            if (cableRing.Count == ringSubdivs + 1) return; // Already generated

            cableRing.Clear();
            float stepAngle = 360f / (float)(ringSubdivs);

            for (int i = 0; i < ringSubdivs; i++)
            {
                float step = (i * stepAngle) * Mathf.Deg2Rad;
                cableRing.Add(new Vector3(Mathf.Sin(step), Mathf.Cos(step), 0f));
            }

            // Loop ring
            cableRing.Add(cableRing[0]);
        }


        /// <summary> Cable trail path using just positions </summary>
        static List<Vector3> cableTrail = new List<Vector3>();

        public static void GenerateCableTrailPoints(List<Vector3> trail, Vector3 a, Vector3 b, Vector3 stretchPoint, int lengthSubdivs, out float trailLength, float hanging = 0f)
        {
            bool addPoints = false;

            float tlength = 0f;
            Vector3 prePos = a;

            if (trail.Count != lengthSubdivs + 1) { addPoints = true; trail.Clear(); }

            if (hanging <= 0f)
            {
                for (int i = 0; i < lengthSubdivs + 1; i++) // +1 For end subdiv
                {
                    float t = i / ((float)lengthSubdivs);
                    Vector3 targetPos = GetBezierQuad(a, b, stretchPoint, t);

                    tlength += Vector3.Distance(prePos, targetPos);
                    prePos = targetPos;

                    if (addPoints) trail.Add(targetPos); else trail[i] = targetPos;
                }
            }
            else
            {
                Vector3 towardsB = b - a;
                Vector3 towardsBFlat = Vector3.ProjectOnPlane(towardsB, Vector3.up);
                float length = towardsB.magnitude;

                int totalLenSubdivs = lengthSubdivs + 1; // +1 For end subdiv
                int hangPartLength = (lengthSubdivs) / 4;
                float rHang = 1f - hanging;

                Vector3 aEnd = a + towardsB * (0.1f + 0.05f * hanging);
                aEnd.y = Mathf.LerpUnclamped(aEnd.y, stretchPoint.y, 0.1f + rHang * 0.05f);
                Vector3 aStretchTo = a + towardsBFlat * (0.025f + 0.1f * hanging);
                aStretchTo.y += (0.015f * hanging) * 0.05f * (a.y - stretchPoint.y);

                Vector3 bContinueStart = b - towardsB * (0.1f + 0.05f * hanging);
                bContinueStart.y = Mathf.LerpUnclamped(bContinueStart.y, stretchPoint.y, 0.1f + rHang * 0.05f);
                Vector3 bStretchTo = b - towardsBFlat * (0.025f + 0.1f * hanging);
                bStretchTo.y += (0.015f * hanging) * 0.05f * (b.y - stretchPoint.y);

                for (int i = 0; i < hangPartLength; i++) // Left cable part
                {
                    float t = (float)i * (1f / ((float)hangPartLength));
                    Vector3 targetPos = GetBezierQuad(a, aEnd, aStretchTo, t);
                    if (addPoints) trail.Add(targetPos); else trail[i] = targetPos;

                    tlength += Vector3.Distance(prePos, targetPos);
                    prePos = targetPos;
                }
                for (int i = hangPartLength; i < lengthSubdivs - hangPartLength + 1; i++) // Middle cable part
                {
                    float t = (i - hangPartLength) / ((float)totalLenSubdivs - hangPartLength - hangPartLength - 1);
                    Vector3 targetPos = GetBezierQuad(aEnd, bContinueStart, stretchPoint, t);
                    if (addPoints) trail.Add(targetPos); else trail[i] = targetPos;

                    tlength += Vector3.Distance(prePos, targetPos);
                    prePos = targetPos;
                }

                for (int i = totalLenSubdivs - hangPartLength; i < totalLenSubdivs; i++) // Right - end cable part
                {
                    float t = (i - (totalLenSubdivs - hangPartLength - 1)) / ((float)hangPartLength);
                    Vector3 targetPos = GetBezierQuad(bContinueStart, b, bStretchTo, t);
                    if (addPoints) trail.Add(targetPos); else trail[i] = targetPos;

                    tlength += Vector3.Distance(prePos, targetPos);
                    prePos = targetPos;
                }

            }

            //if(trail.Count > 1) tlength += Vector3.Distance(trail[trail.Count - 2], trail[trail.Count - 1]);
            trailLength = tlength;
        }


        public static Vector3 GetBezierQuad(Vector3 a, Vector3 b, Vector3 mid, float t)
        {
            float revT = 1 - t;
            return (revT * revT) * a + 2 * revT * t * mid + (t * t) * b;
        }


        #endregion


        static List<Vector3> pointsBackup = null;
        static List<List<Vector3>> attachementSources = null;
        static List<CombineInstance> toCombineSingle = new List<CombineInstance>();
        static List<CombineInstance> toCombine = new List<CombineInstance>();

        static bool resetLengthReference = true;
        /// <summary> For equal uv tiling </summary>
        static float lengthReferenceScale = 1f;


        static void RandomizaPoints(List<Vector3> points, List<Vector3> backupPoints, CableRandomizationSettings settings)
        {
            if (settings.RandomizePathPoints != Vector3.zero)
            {
                if (points.Count < 2) return;

                points.Clear();
                //Quaternion dir = Quaternion.identity;
                for (int b = 0; b < backupPoints.Count; b++)
                {
                    Vector3 p = backupPoints[b];
                    //if (b < backupPoints.Count - 1) dir = Quaternion.LookRotation(backupPoints[b + 1] - p);
                    points.Add(p + FGenerators.GetRandom(settings.RandomizePathPoints));
                    //points.Add(p + dir * FGenerators.GetRandom(settings.RandomizePathPoints));
                }
            }
        }


        /// <summary> Generating cable using multiple points, with randomization, cloning and generating attachements </summary>
        public static Mesh GenerateFullCablesMesh(List<Vector3> points, float cableLoose, float hanging, float radius, TileCableGenerator.CableMeshSettings meshSettings, TileCableGenerator.CableTexturingSettings texturingSettings, TileCableGenerator.CableClonerSettings clonerSettings, TileCableGenerator.CableRandomizationSettings randomSettings, TileCableGenerator.CableAttachementSettings attachementSettings)
        {

            #region Prepare trail randomization

            if (pointsBackup != null) pointsBackup.Clear();
            if (randomSettings.RandomizePathPoints != Vector3.zero)
            {
                if (pointsBackup == null) pointsBackup = new List<Vector3>();
                for (int p = 0; p < points.Count; p++) pointsBackup.Add(points[p]);
                RandomizaPoints(points, pointsBackup, randomSettings);
            }

            #endregion


            Mesh fullCableTrail = GenerateCablesMesh(points, cableLoose, hanging, radius, meshSettings, texturingSettings, randomSettings);
            Mesh fullMesh = fullCableTrail;

            bool usingRandomization = false;
            toCombine.Clear();


            #region Prepare Attachement Support

            bool useAttachements = false;

            if (attachementSettings != null)
                if (attachementSettings.Mesh != null)
                {
                    useAttachements = true;

                    if (attachementSources == null) attachementSources = new List<List<Vector3>>();
                    else attachementSources.Clear();
                }

            #endregion


            List<Vector3Int> clonesToSkip = null;
            List<Vector3> targetPoints = points; // reference for ReGenerate feature is used

            if (clonerSettings != null)
            {
                if (randomSettings != null)
                {
                    if (randomSettings.RandomizeLoose != Vector2.one) usingRandomization = true;
                    if (!usingRandomization) if (randomSettings.RandomizeTrails != Vector2.zero) usingRandomization = true;

                    // Supporting clone cut-out
                    if (randomSettings.CutOutClones != Vector2.zero)
                    {
                        List<Vector3Int> clonesIdx = new List<Vector3Int>();

                        #region Sum all possible clones count

                        for (int x = 0; x < clonerSettings.InstancesCount.x; x++)
                            for (int y = 0; y < clonerSettings.InstancesCount.y; y++)
                                for (int z = 0; z < clonerSettings.InstancesCount.z; z++)
                                {
                                    if (clonerSettings.CircularGrid)
                                    {
                                        Vector3 circleRef = new Vector3(0f, 0f, 0f);
                                        circleRef.x = x - clonerSettings.InstancesCount.x / 2;
                                        circleRef.y = y - clonerSettings.InstancesCount.y / 2;

                                        if (x > y)
                                        { if (circleRef.magnitude > clonerSettings.InstancesCount.x / 2) continue; }
                                        else
                                        { if (circleRef.magnitude > clonerSettings.InstancesCount.y / 2) continue; }
                                    }

                                    clonesIdx.Add(new Vector3Int(x, y, z));
                                }

                        #endregion

                        //UnityEngine.Debug.Log("allclones " + allClones);

                        if (clonesIdx.Count > 0)
                        {
                            if (randomSettings.CutOutClones.x < 0) randomSettings.CutOutClones.x = 0;
                            if (randomSettings.CutOutClones.y < 0) randomSettings.CutOutClones.y = 0;
                            int toCut = FGenerators.GetRandom(randomSettings.CutOutClones.x, randomSettings.CutOutClones.y + 1);

                            if (toCut > 0)
                            {
                                if (toCut >= clonesIdx.Count) return new Mesh();

                                clonesToSkip = new List<Vector3Int>();
                                for (int c = 0; c < toCut; c++)
                                {
                                    if (clonesIdx.Count == 0) break;
                                    int i = FGenerators.GetRandom(0, clonesIdx.Count);
                                    clonesToSkip.Add(clonesIdx[i]);
                                    clonesIdx.RemoveAt(i);
                                }
                            }
                        }
                    }
                }


                for (int x = 0; x < clonerSettings.InstancesCount.x; x++)
                {
                    for (int y = 0; y < clonerSettings.InstancesCount.y; y++)
                    {
                        List<Vector3> attachements = null;
                        if (useAttachements) attachements = new List<Vector3>();

                        for (int z = 0; z < clonerSettings.InstancesCount.z; z++)
                        {
                            if (clonerSettings.CircularGrid)
                            {
                                Vector3 circleRef = new Vector3(0f, 0f, 0f);
                                circleRef.x = x - clonerSettings.InstancesCount.x / 2;
                                circleRef.y = y - clonerSettings.InstancesCount.y / 2;

                                if (x > y)
                                {
                                    if (circleRef.magnitude > clonerSettings.InstancesCount.x / 2) continue;
                                }
                                else
                                {
                                    if (circleRef.magnitude > clonerSettings.InstancesCount.y / 2) continue;
                                }
                            }

                            if (clonesToSkip != null)
                            {
                                if (clonesToSkip.Contains(new Vector3Int(x, y, z))) continue;
                            }

                            Vector3 translationValue = new Vector3();
                            translationValue.x = x;
                            translationValue.y = y;
                            translationValue.z = z;

                            Vector3 centerOffset = new Vector3();
                            centerOffset.x = -(clonerSettings.InstancesCount.x - 1) / 2;
                            if (clonerSettings.InstancesCount.x % 2 == 0) centerOffset.x -= 0.5f;

                            centerOffset.y = -(clonerSettings.InstancesCount.y - 1) / 2;
                            if (clonerSettings.InstancesCount.y % 2 == 0) centerOffset.y -= 0.5f;

                            centerOffset.z = -(clonerSettings.InstancesCount.z - 1) / 2;
                            if (clonerSettings.InstancesCount.z % 2 == 0) centerOffset.z -= 0.5f;

                            translationValue += centerOffset;

                            translationValue.x *= clonerSettings.ClonesOffsets.x * clonerSettings.ScaleOffsets;
                            translationValue.y *= clonerSettings.ClonesOffsets.y * clonerSettings.ScaleOffsets;
                            translationValue.z *= clonerSettings.ClonesOffsets.z;

                            CombineInstance comb = new CombineInstance();

                            if (clonerSettings.PathReGenerate)
                            {
                                targetPoints = new List<Vector3>();

                                Vector3 lDir = Vector3.zero;

                                for (int p = 0; p < points.Count; p++)
                                {
                                    Vector3 clonerPathPoint = points[p];
                                    Quaternion dir;

                                    if (p == points.Count - 1 && clonerSettings.FlattenEnds)
                                    {
                                        lDir = FVectorMethods.ChooseDominantAxis(points[p] - points[p - 1]);
                                    }
                                    else if (p == 0 && clonerSettings.FlattenEnds)
                                    {
                                        lDir = FVectorMethods.ChooseDominantAxis(points[p + 1] - points[p]);
                                    }
                                    else if (p < points.Count - 1) lDir = points[p + 1] - points[p];

                                    if (lDir == Vector3.zero) dir = Quaternion.identity;
                                    else dir = Quaternion.LookRotation(lDir);

                                    clonerPathPoint += dir * translationValue;
                                    targetPoints.Add(clonerPathPoint);
                                }
                            }

                            if (usingRandomization || clonerSettings.PathReGenerate)
                            {
                                RandomizaPoints(targetPoints, pointsBackup, randomSettings);
                                comb.mesh = GenerateCablesMesh(targetPoints, cableLoose, hanging, radius, meshSettings, texturingSettings, randomSettings);
                            }
                            else comb.mesh = fullCableTrail;

                            if (useAttachements)
                            {
                                if (attachementSettings.ApplyToAllClones)
                                {
                                    for (int p = 0; p < targetPoints.Count; p++)
                                    {
                                        Vector3 pointPos = targetPoints[p] + translationValue;
                                        if (!attachements.Contains(pointPos)) attachements.Add(pointPos);
                                    }
                                }
                                else
                                {
                                    if (attachementSources.Count == 0)
                                        for (int p = 0; p < targetPoints.Count; p++)
                                        {
                                            Vector3 off = new Vector3(0f, 0f, translationValue.z);
                                            Vector3 pointPos = targetPoints[p] + off;
                                            if (!attachements.Contains(pointPos)) attachements.Add(pointPos);
                                        }
                                }
                            }


                            if (clonerSettings.PathReGenerate) translationValue = Vector3.zero;

                            comb.transform = Matrix4x4.Translate(translationValue);
                            toCombine.Add(comb);
                            targetPoints = points;
                        }

                        if (attachements != null) if (attachements.Count > 0) attachementSources.Add(attachements);
                    }
                }

                if (toCombine.Count > 0)
                {
                    Mesh clonerCombination = new Mesh();
                    clonerCombination.name = "CablesClonerCombination";
                    clonerCombination.CombineMeshes(toCombine.ToArray());
                    fullMesh = clonerCombination;
                }
            }




            #region Generate attachements and restore


            if (attachementSources != null)
                if (toCombine.Count == 0)
                {
                    List<Vector3> attachements = new List<Vector3>();
                    for (int p = 0; p < points.Count; p++) attachements.Add(points[p]);
                    attachementSources.Add(attachements);
                }


            if (useAttachements)
                if (attachementSources.Count > 0)
                {
                    Mesh attachementsMesh;

                    if (attachementSettings.ApplyToAllClones)
                    {
                        toCombineSingle.Clear();

                        for (int a = 0; a < attachementSources.Count; a++)
                        {
                            Mesh m = GenerateAttachements(attachementSources[a], attachementSettings);

                            CombineInstance comb = new CombineInstance();
                            comb.mesh = m;
                            comb.transform = Matrix4x4.identity;
                            toCombineSingle.Add(comb);
                        }

                        attachementsMesh = new Mesh();
                        attachementsMesh.CombineMeshes(toCombineSingle.ToArray());
                    }
                    else
                    {
                        attachementsMesh = GenerateAttachements(attachementSources[0], attachementSettings);
                    }

                    if (attachementsMesh != null)
                    {
                        toCombineSingle.Clear();

                        CombineInstance comb = new CombineInstance();
                        comb.mesh = fullMesh;
                        comb.subMeshIndex = 0;
                        comb.transform = Matrix4x4.identity;
                        toCombineSingle.Add(comb);

                        comb = new CombineInstance();
                        comb.mesh = attachementsMesh;
                        comb.subMeshIndex = 0;
                        comb.transform = Matrix4x4.identity;
                        toCombineSingle.Add(comb);

                        Mesh subMeshed = new Mesh();
                        subMeshed.name = "CablesMesh";
                        subMeshed.CombineMeshes(toCombineSingle.ToArray(), false);

                        fullMesh = subMeshed;
                    }
                }


            if (pointsBackup != null)
                if (pointsBackup.Count > 0)
                {
                    points.Clear();
                    for (int p = 0; p < pointsBackup.Count; p++) points.Add(pointsBackup[p]);
                }


            #endregion


            return fullMesh;
        }

        static Quaternion GetAxisRotation(Vector3 angles)
        {
            Quaternion rot = Quaternion.identity;
            rot *= Quaternion.AngleAxis(angles.x, Vector3.right);
            rot *= Quaternion.AngleAxis(angles.y, Vector3.up);
            rot *= Quaternion.AngleAxis(angles.z, Vector3.forward);
            return rot;
        }

        static Mesh GenerateAttachements(List<Vector3> attachementsTrail, TileCableGenerator.CableAttachementSettings settings)
        {
            Vector3 towards = Vector3.forward;
            if (attachementsTrail.Count > 1) towards = attachementsTrail[1] - attachementsTrail[0];

            toCombine.Clear();

            Quaternion rotOffset = GetAxisRotation(settings.Rotation);

            for (int t = 0; t < attachementsTrail.Count; t++)
            {
                if (t == 0) if (settings.AddOnStart == false) continue;
                if (t == attachementsTrail.Count - 1) if (settings.AddOnTheEnd == false) continue;
                if (t > 0 && t < attachementsTrail.Count - 1) if (settings.AddInTheMiddle == false) continue;

                Vector3 pos = attachementsTrail[t];

                if (t < attachementsTrail.Count - 1) towards = attachementsTrail[t + 1] - attachementsTrail[t];

                Quaternion rot;
                if (settings.FlatRotation) rot = Quaternion.LookRotation(Vector3.ProjectOnPlane(towards, Vector3.up));
                else rot = Quaternion.LookRotation(towards);

                pos += rot * settings.Offset;

                CombineInstance comb = new CombineInstance();
                comb.mesh = settings.Mesh;
                comb.transform = Matrix4x4.TRS(pos, rot * rotOffset, settings.Scale * settings.ScaleMultiplier);
                toCombine.Add(comb);
            }

            Mesh attachements = new Mesh();
            attachements.name = "Attachments";
            attachements.CombineMeshes(toCombine.ToArray());
            return attachements;
        }


        /// <summary> Generating cable trail out of multiple points, without cloning but with use of randomization </summary>
        public static Mesh GenerateCablesMesh(List<Vector3> points, float Loose, float Hanging, float Radius, TileCableGenerator.CableMeshSettings MeshSettings, TileCableGenerator.CableTexturingSettings TexturingSettings, TileCableGenerator.CableRandomizationSettings randomSettings)
        {
            Mesh fullCableTrail = new Mesh();
            toCombineSingle.Clear();
            resetLengthReference = true;

            for (int p = 0; p < points.Count - 1; p++)
            {
                CombineInstance comb = new CombineInstance();
                comb.mesh = GetSingleCableMesh(points[p], points[p + 1], Loose, Hanging, Radius, MeshSettings, TexturingSettings, randomSettings);
                comb.transform = Matrix4x4.identity;
                toCombineSingle.Add(comb);

                if (MeshSettings.JoinEnds) if (p > 0) JoinCableSegmentsEnds(toCombineSingle[p - 1].mesh, toCombineSingle[p].mesh, MeshSettings.CircleSubdivs + 1);
            }

            fullCableTrail.CombineMeshes(toCombineSingle.ToArray());
            return fullCableTrail;
        }


        public static void JoinCableSegmentsEnds(Mesh start, Mesh end, int circlePoints)
        {
            Vector3[] verts = start.vertices;
            Vector3[] vertse = end.vertices;

            for (int c = 1; c <= circlePoints; c++)
            {
                Vector3 nVal = Vector3.LerpUnclamped(verts[start.vertexCount - c], vertse[circlePoints - c], 0.5f);
                verts[start.vertexCount - c] = nVal;
                vertse[circlePoints - c] = nVal;
                //verts[start.vertexCount - c] = end.vertices[cableCircle.Count - c];
            }

            start.vertices = verts;
            end.vertices = vertse;
        }


        /// <summary> Single cable trail from point a to point b with applied randomization </summary>
        public static Mesh GetSingleCableMesh(Vector3 a, Vector3 b, float Loose, float Hanging, float Radius, TileCableGenerator.CableMeshSettings MeshSettings, TileCableGenerator.CableTexturingSettings TexturingSettings, TileCableGenerator.CableRandomizationSettings randomSettings = null)
        {
            Mesh mesh = new Mesh();
            mesh.name = "CABLE";

            Vector3 diff = b - a;

            float uvAngleByDiff = 0f;
            //if (diff != Vector3.zero) uvAngleByDiff = Quaternion.LookRotation(-diff).eulerAngles.x;

            if (randomSettings != null) if (randomSettings.RandomizeLoose != Vector2.one)
                {
                    Loose *= FGenerators.GetRandomSwap(randomSettings.RandomizeLoose.x, randomSettings.RandomizeLoose.y);
                }

            float looseFactor = .1f;
            if (Loose > 0) looseFactor = Loose * diff.sqrMagnitude;

            Vector3 stretchTowardsPos = (a + b) / 2f;

            float verticalFactor = 1f;
            verticalFactor = 1f - (Mathf.Abs(Vector3.Dot(diff.normalized, Vector3.up)));
            stretchTowardsPos += new Vector3(0, -looseFactor * verticalFactor);

            GenerateCableCircle(cableCircle, MeshSettings.CircleSubdivs);

            float calculatedLength;
            GenerateCableTrailPoints(cableTrail, a, b, stretchTowardsPos, MeshSettings.LengthSubdivs, out calculatedLength, Hanging);

            if (resetLengthReference)
            {
                lengthReferenceScale = calculatedLength;
                resetLengthReference = false;
            }
            else
            {
                calculatedLength = lengthReferenceScale;
            }

            int targetVertsCount = (MeshSettings.LengthSubdivs + 1) * (MeshSettings.CircleSubdivs + 1);

            if (_vertices == null || _vertices.Length != targetVertsCount) _vertices = new Vector3[targetVertsCount];

            int totalTrisCount = (cableCircle.Count * 6) * (MeshSettings.LengthSubdivs + 1);
            if (_tris == null || _tris.Length != totalTrisCount) _tris = new int[totalTrisCount];
            if (_trisRev == null || _tris.Length != _trisRev.Length) _trisRev = new int[totalTrisCount];

            int circlePointsCount = cableCircle.Count;

            if (_trisCircleHelperCurrent == null || _trisCircleHelperCurrent.Length != circlePointsCount)
                _trisCircleHelperCurrent = new int[circlePointsCount];

            if (_trisCircleHelperPre == null || _trisCircleHelperPre.Length != circlePointsCount)
                _trisCircleHelperPre = new int[circlePointsCount];

            List<Vector2> uvs = new List<Vector2>();
            Vector2 uvStep = new Vector2(1f / (float)(cableTrail.Count - 1), 1f / (float)(circlePointsCount - 1));
            float uvElapsed = 0f;

            Quaternion circleRotation = Quaternion.identity;
            Vector3 prePos = cableTrail[0];
            Vector3 trailDiff;
            Vector3 trailDirection;

            #region Randomize trails code

            // Randomize trail shape on middle subdivisions and keep start/end intact
            if (randomSettings != null) if (randomSettings.RandomizeTrails != Vector2.zero)
                {
                    float halfLen = (cableTrail.Count / 2f);
                    if (halfLen <= 0f) halfLen = 1f;

                    Vector3 rOffsets = FGenerators.GetRandom(new Vector3(100f, 100f, 100f));

                    for (int i = 1; i < cableTrail.Count - 1; i++)
                    {
                        trailDiff = cableTrail[i + 1] - cableTrail[i];
                        trailDirection = trailDiff.normalized;

                        if (trailDirection != Vector3.zero)
                        {
                            circleRotation = Quaternion.LookRotation(trailDirection, Vector3.up);
                            if (MeshSettings.RollOffset > 0f) circleRotation = Quaternion.AngleAxis(MeshSettings.RollOffset, trailDirection) * circleRotation;
                        }

                        float stepMul;
                        float iM = i * randomSettings.NoiseScale * (lengthReferenceScale * 0.1f);

                        if (i < halfLen) stepMul = Mathf.Lerp(0.0f, 1f, i / (halfLen));
                        else stepMul = Mathf.Lerp(1f, 0.0f, (i - halfLen) / (halfLen));
                        stepMul *= 0.1f;

                        Vector3 nTrail = cableTrail[i];

                        Vector3 offset = new Vector3();
                        offset.x = (-0.5f + Mathf.PerlinNoise(rOffsets.x + iM, rOffsets.y + iM)) * stepMul * randomSettings.RandomizeTrails.x;
                        offset.y = (-0.5f + Mathf.PerlinNoise(rOffsets.y + iM - 80f, rOffsets.z + iM + 80f)) * stepMul * randomSettings.RandomizeTrails.y;
                        cableTrail[i] = nTrail + circleRotation * offset;
                    }
                }

            #endregion

            circleRotation = Quaternion.identity;

            for (int t = 0; t < cableTrail.Count; t++) // Step through cable trail point
            {
                //UnityEngine.Debug.Log("pos["+t+" = " + cableTrail[t]);
                //if( t > 0) UnityEngine.Debug.DrawLine(cableTrail[t], cableTrail[t-1], Color.green, 1.01f);

                // Define circle look rotation towards next trail point
                if (t < cableTrail.Count - 1)
                {
                    trailDiff = cableTrail[t + 1] - cableTrail[t];
                    trailDirection = trailDiff.normalized;

                    if (trailDirection != Vector3.zero)
                    {
                        circleRotation = Quaternion.LookRotation(trailDirection, Vector3.up);
                        if (MeshSettings.RollOffset > 0f) circleRotation = Quaternion.AngleAxis(MeshSettings.RollOffset, trailDirection) * circleRotation;
                    }
                }

                uvElapsed += Vector3.Distance(cableTrail[t], prePos);
                prePos = cableTrail[t];


                // Populate circle vertices
                for (int c = 0; c < circlePointsCount; c++)
                {
                    // Calculate vertice position
                    Vector3 cPos = cableTrail[t];
                    cPos += circleRotation * (cableCircle[c] * Radius);

                    _vertices[t * circlePointsCount + c] = cPos;

                    // Set up UV
                    Vector2 uv = new Vector2();
                    uv.x = uvElapsed / calculatedLength * TexturingSettings.LengthTiling;

                    if (c > (circlePointsCount) / 2)
                        uv.y = ((float)(c) * uvStep.y) * TexturingSettings.VerticalTiling;
                    else
                        uv.y = ((float)(-c + circlePointsCount + 1) * uvStep.y) * TexturingSettings.VerticalTiling;

                    uvs.Add(uv);

                    // Prepare tris helpers
                    _trisCircleHelperPre[c] = _trisCircleHelperCurrent[c];
                    _trisCircleHelperCurrent[c] = t * circlePointsCount + c;
                }

                // Store mesh tris -> Two 3-point triangles
                for (int c = 0; c < circlePointsCount; c++)
                {
                    if (t == 0 || c >= cableCircle.Count - 1) continue;

                    int start = (t * circlePointsCount + c) * 6;

                    _tris[start] = _trisCircleHelperPre[c];
                    _tris[start + 1] = _trisCircleHelperPre[(c + 1) % circlePointsCount];
                    _tris[start + 2] = _trisCircleHelperCurrent[c];

                    _tris[start + 3] = _tris[start + 2];
                    _tris[start + 4] = _tris[start + 1];
                    _tris[start + 5] = _trisCircleHelperCurrent[(c + 1) % circlePointsCount];
                }
            }

            mesh.vertices = _vertices;

            for (int t = 0; t < _tris.Length; t++)
            {
                _trisRev[t] = _tris[_tris.Length - 1 - t];
            }

            mesh.triangles = _trisRev;

            mesh.RecalculateNormals();

            mesh.uv = uvs.ToArray();

            float uvAngle = uvAngleByDiff + TexturingSettings.UVRotate;
            if (uvAngle > 0f) FMeshUtils.RotateUV(mesh, uvAngle);

            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            cablesMesh = mesh;
            return cablesMesh;
        }

    }
}