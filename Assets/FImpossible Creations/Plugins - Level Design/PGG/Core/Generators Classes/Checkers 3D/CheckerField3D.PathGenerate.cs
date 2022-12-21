using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Checker
{
    public partial class CheckerField3D
    {

        private static readonly List<LineFindHelper> defaultLineFindDirections = new List<LineFindHelper>();
        private static readonly List<LineFindHelper> defaultLineFindDirectionsDiag = new List<LineFindHelper>();
        private static readonly List<LineFindHelper> defaultLineFindDirections3D = new List<LineFindHelper>();
        private static readonly List<LineFindHelper> defaultLineFindDirections3DDiag = new List<LineFindHelper>();

        /// <summary>
        /// Fast path find but without collision detection
        /// </summary>
        public void AddLinesTowards(Vector3Int start, Vector3Int end, float nonDiagonal = 0.3f, int thickness = 1, List<LineFindHelper> directions = null, LineFindHelper.ERadiusType radiusType = LineFindHelper.ERadiusType.RectangleRadius, bool yRadius = false, bool clearOverpaint = false, bool eraseFinishCell = false)
        {
            var line = GenerateLinesTowards(start, end, nonDiagonal, thickness, directions, radiusType, yRadius, clearOverpaint, eraseFinishCell);
            AddLocal(line);
        }

        public Vector3 WorldToLocal(Vector3 pos)
        {
            return Matrix.inverse.MultiplyPoint3x4(pos);
        }

        public Vector3 WorldToLocal(Vector3 pos, Matrix4x4 inversMatrix)
        {
            return inversMatrix.MultiplyPoint3x4(pos);
        }

        public Vector3 LocalToWorld(Vector3 pos)
        {
            return Matrix.MultiplyPoint3x4(pos);
        }

        public void AddLinesTowardsUsingWorldPos(Vector3 start, Vector3 end, float nonDiagonal = 0.3f, int thickness = 1, List<LineFindHelper> directions = null, LineFindHelper.ERadiusType radiusType = LineFindHelper.ERadiusType.RectangleRadius, bool yRadius = false, bool clearOverpaint = false, bool eraseFinishCell = false)
        {
            start = WorldToLocal(start);
            end = WorldToLocal(end);
            AddLinesTowards(start.V3toV3Int(), end.V3toV3Int(), nonDiagonal, thickness, directions, radiusType, yRadius, clearOverpaint, eraseFinishCell);

        }

        public void AddStripeInDirection(Vector3Int start, Vector3Int dir, int length, int thickness = 1)
        {
            for (int i = 0; i < length; i++)
            {
                AddLocal(start + dir * i);
            }

            if (thickness > 1)
            {
                Vector3Int rotDir = PGGUtils.GetRotatedFlatDirectionFrom(dir);

                if (thickness % 2 != 0)
                {
                    for (int i = 0; i < length; i++)
                    {
                        for (int t = 1; t <= Mathf.FloorToInt(thickness / 2f); t++)
                        {
                            AddLocal(start + dir * i + rotDir * t);
                            AddLocal(start + dir * i + rotDir * -t);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        for (int t = 1; t <= thickness / 2; t += 1)
                        {
                            AddLocal(start + dir * i + rotDir * t);
                        }

                        for (int t = 2; t <= thickness / 2; t += 1)
                        {
                            AddLocal(start + dir * i - rotDir * (t - 1));
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Fast path find but without collision detection
        /// </summary>
        public List<Vector3Int> GenerateLinesTowards(Vector3Int start, Vector3Int end, float nonDiagonal = 0.3f, int thickness = 1, List<LineFindHelper> directions = null, LineFindHelper.ERadiusType radiusType = LineFindHelper.ERadiusType.RectangleRadius, bool yRadius = false, bool clearOverpaint = false, bool eraseFinishCell = false)
        {
            if (directions == null || directions.Count < 2)
            {
                RefreshDefaultDirections();
                directions = defaultLineFindDirections;
            }

            if (end.y - start.y != 0f)
            {
                if (directions == defaultLineFindDirections) directions = defaultLineFindDirections3D;
                else
                if (directions == defaultLineFindDirectionsDiag) directions = defaultLineFindDirections3DDiag;
                else
                {
                    bool containsYDir = false;
                    for (int d = 0; d < directions.Count; d++)
                        if (directions[d].Dir.y != 0) { containsYDir = true; break; }

                    if (!containsYDir)
                    {
                        directions.Add(new LineFindHelper(new Vector3Int(0, 1, 0), 1));
                        directions.Add(new LineFindHelper(new Vector3Int(0, -1, 0), 1));
                    }
                }
            }

            List<Vector3Int> positions = new List<Vector3Int>();

            Vector3Int currentPos = start;
            Vector3Int startDir = Vector3Int.zero;
            Vector3Int lastDir = Vector3Int.zero;

            //float initDistance = (start- end).sqrMagnitude;
            positions.Add(currentPos);

            int iters = 0;
            int fIters = 0;

            while (currentPos != end)
            {
                float nearest = float.MaxValue;
                Vector3Int nearestPos = currentPos;
                Vector3Int targetDir = Vector3Int.zero;

                // Searching for nearest point towards target
                for (int i = 0; i < directions.Count; i++)
                {
                    Vector3Int targetPos = currentPos + directions[i].Dir; // One unit offset in different directions check

                    float distance = (targetPos - end).magnitude + directions[i].Cost; // initial distance towards target from new position

                    if (nonDiagonal > 0f) if (fIters == 0) { if (directions[i].Dir.magnitude >= 1f) distance += nonDiagonal; }

                    if (directions[i].Dir != lastDir)
                    {
                        distance += nonDiagonal;
                    }

                    if (distance < nearest)
                    {
                        targetDir = directions[i].Dir;
                        nearest = distance;
                        nearestPos = targetPos;
                    }

                    #region Freeze preventers

                    //if (distance > initDistance * 8f)
                    //{
                    //    UnityEngine.Debug.Log("[PGG - Shape Generator] Path exceeded available distance - breaking to prevent freeze");
                    //    return new List<Vector3Int>() { Vector3Int.zero };
                    //}

                    iters += 1;
                    if (iters > 1000000)
                    {
                        UnityEngine.Debug.Log("[PGG - Shape Generator] Too many line generation iterations - breaking to prevent freeze");
                    }

                    #endregion
                }

                lastDir = targetDir;

                if (fIters == 0) startDir = lastDir;
                currentPos = nearestPos;
                positions.Add(currentPos);

                fIters += 1;
            }

            // Applying radius
            if (thickness > 1)
            {
                int range = thickness - 1;
                List<Vector3Int> toAdd = new List<Vector3Int>();

                if (radiusType == LineFindHelper.ERadiusType.RectangleRadius)
                {
                    for (int i = 0; i < positions.Count; i++)
                    {
                        for (int x = -range; x <= range; x++)
                        {
                            int yReps = 0;
                            if (yRadius) yReps = range;
                            for (int y = -yReps; y <= yReps; y++)
                            {
                                for (int z = -range; z <= range; z++)
                                {
                                    Vector3Int nPos = positions[i] + new Vector3Int(x, y, z);
                                    if (toAdd.Contains(nPos) == false) toAdd.Add(nPos);
                                }
                            }
                        }
                    }
                }
                else if (radiusType == LineFindHelper.ERadiusType.CircularRadius)
                {
                    float falloffFactor = (float)thickness * 0.7f;
                    for (int i = 0; i < positions.Count; i++)
                    {
                        for (int x = -range; x <= range; x++)
                        {
                            int yReps = 0;
                            if (yRadius) yReps = range;
                            for (int y = -yReps; y <= yReps; y++)
                            {
                                for (int z = -range; z <= range; z++)
                                {
                                    Vector3Int offset = new Vector3Int(x, y, z);
                                    if (offset.magnitude > falloffFactor) continue;
                                    Vector3Int nPos = positions[i] + offset;
                                    if (toAdd.Contains(nPos) == false) toAdd.Add(nPos);
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < toAdd.Count; i++)
                {
                    if (!positions.Contains(toAdd[i]))
                        positions.Add(toAdd[i]);
                }

                if (clearOverpaint)
                {
                    Vector3Int back = startDir.InverseV3Int();
                    Vector3 cross = Vector3.Cross(startDir.V3IntToV3(), Vector3.up);
                    Vector3Int side = cross.V3toV3Int();

                    for (int b = 1; b <= thickness; b++)
                    {
                        for (int s = -thickness; s <= thickness; s++)
                        {
                            Vector3Int pos = back * b;
                            pos += side * s;
                            pos += start;

                            if (positions.Contains(pos)) positions.Remove(pos);
                        }
                    }

                    back = lastDir;
                    cross = Vector3.Cross(lastDir.V3IntToV3(), Vector3.up);
                    side = cross.V3toV3Int();

                    for (int b = 1; b <= thickness; b++)
                    {
                        for (int s = -thickness; s <= thickness; s++)
                        {
                            Vector3Int pos = back * b;
                            pos += side * s;
                            pos += end;

                            if (positions.Contains(pos)) positions.Remove(pos);
                        }
                    }
                }

            }

            if (eraseFinishCell) if (positions.Count > 1) positions.RemoveAt(positions.Count - 1);

            return positions;
        }

        //[System.Serializable]
        public struct LineFindHelper
        {
            public enum ERadiusType { CircularRadius, RectangleRadius }

            public Vector3Int Dir;
            /// <summary> Path distance helper value or direction cost </summary>
            public float Cost;

            public LineFindHelper(Vector3Int dir, float val)
            {
                Dir = dir;
                Cost = val;
            }
        }

        public static List<LineFindHelper> GetDefaultDirections { get { RefreshDefaultDirections(); return defaultLineFindDirections; } }
        public static List<LineFindHelper> GetDefaultDirections3D { get { RefreshDefaultDirections(); return defaultLineFindDirections3D; } }
        public static List<LineFindHelper> GetDefaultDirectionsDiag { get { RefreshDefaultDirections(); return defaultLineFindDirectionsDiag; } }

        private static void RefreshDefaultDirections()
        {
            if (defaultLineFindDirections.Count == 0)
            {
                defaultLineFindDirections.Add(new LineFindHelper(new Vector3Int(1, 0, 0), 1f));
                defaultLineFindDirections.Add(new LineFindHelper(new Vector3Int(-1, 0, 0), 1f));
                defaultLineFindDirections.Add(new LineFindHelper(new Vector3Int(0, 0, 1), 1f));
                defaultLineFindDirections.Add(new LineFindHelper(new Vector3Int(0, 0, -1), 1f));

                PGGUtils.TransferFromListToList(defaultLineFindDirections, defaultLineFindDirectionsDiag);
                defaultLineFindDirectionsDiag.Add(new LineFindHelper(new Vector3Int(1, 0, 1), 1.33333f));
                defaultLineFindDirectionsDiag.Add(new LineFindHelper(new Vector3Int(1, 0, -1), 1.33333f));
                defaultLineFindDirectionsDiag.Add(new LineFindHelper(new Vector3Int(-1, 0, 1), 1.33333f));
                defaultLineFindDirectionsDiag.Add(new LineFindHelper(new Vector3Int(-1, 0, -1), 1.33333f));

                PGGUtils.TransferFromListToList(defaultLineFindDirections, defaultLineFindDirections3D);
                defaultLineFindDirections3D.Add(new LineFindHelper(new Vector3Int(0, 1, 0), 1f));
                defaultLineFindDirections3D.Add(new LineFindHelper(new Vector3Int(0, -1, 0), 1f));

                PGGUtils.TransferFromListToList(defaultLineFindDirectionsDiag, defaultLineFindDirections3DDiag);
                defaultLineFindDirections3DDiag.Add(new LineFindHelper(new Vector3Int(0, 1, 0), 1f));
                defaultLineFindDirections3DDiag.Add(new LineFindHelper(new Vector3Int(0, -1, 0), 1f));
            }
        }

    }
}