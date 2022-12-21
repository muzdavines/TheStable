using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Checker
{
    public partial class CheckerField3D
    {
        //private bool _alignZeroContactsCorrectionNeeded = false;

        public FieldCell _AlignTo_OtherCollisionCell = null;
        public FieldCell _AlignTo_MyCollisionCell = null;
        /// <summary>
        /// Warning! It can align one field to another just on diagonal edges, to fix it, call ShiftForAlignPoints() to prevent it
        /// </summary>
        public void AlignTo(CheckerField3D otherField, int shiftIfNoContact_MinimumContacts = 1, FieldCell targetCell = null)
        {
            // Find nearest points
            FieldCell nearest = targetCell;
            if (FGenerators.IsNull(targetCell)) nearest = GetNearestCellTo(otherField);

            _AlignTo_OtherCollisionCell = null;
            _AlignTo_MyCollisionCell = null;

            Vector3 myNrstWorld = GetWorldPos(nearest);
            Vector3 othNrstWorld = otherField.GetWorldPos(_nearestCellOtherField);
            Vector3 offset = othNrstWorld - myNrstWorld;

            RootPosition += offset;
            RoundRootPositionAccordingly(otherField);

            Vector3Int dir = offset.normalized.V3toV3Int();

            if (dir == Vector3Int.zero)
            {
                if (offset.x != 0)
                {
                    if (offset.x < 0) dir.x = -1; else dir.x = 1;
                }
                else if (offset.z != 0)
                {
                    if (offset.z < 0) dir.z = -1; else dir.z = 1;
                }
                else if (offset.y != 0)
                {
                    if (offset.y < 0) dir.y = -1; else dir.y = 1;
                }
            }

            StepPushOutOfCollision(otherField, dir.InverseV3Int());

            if (shiftIfNoContact_MinimumContacts > 1)
            {
                var dirs = GetDefaultDirections;

                int mostAlignPoints = -1;
                Vector3 mostAlignPos = RootPosition;

                for (int i = 0; i < dirs.Count; i++)
                {
                    int aligns = ShiftForAlignPoints(otherField, dirs[i].Dir, shiftIfNoContact_MinimumContacts);
                    if (aligns == 0) continue;
                    if (aligns > mostAlignPoints)
                    {
                        mostAlignPoints = aligns;
                        mostAlignPos = RootPosition;
                    }
                }

                RootPosition = mostAlignPos;
            }
        }


        public void StepPushOutOfCollision(CheckerField3D other, Vector3Int pushDir, int maxIters = 128)
        {
            bool pushed = false;
            bool checkRnd = RootScale != other.RootScale;
            //if (checkRnd == false) checkRnd = RootScale.x != 1f;

            for (int i = 0; i < maxIters; i++)
            {
                if (IsCollidingWith(other, checkRnd))
                {
                    RootPosition += ScaleV3(pushDir);
                    pushed = true;
                }
                else
                    break;
            }

            if (pushed)
            {
                RootPosition -= (other.ScaleV3(pushDir) - ScaleV3(pushDir)) / 2f;
            }
        }

        public void StepPushOutOfCollision(List<Planning.FieldPlanner> other, Vector3Int pushDir, int maxIters = 128)
        {
            bool pushed = false;
            CheckerField3D latestPush = null;

            for (int i = 0; i < maxIters; i++)
            {
                bool collided = false;

                for (int c = 0; c < other.Count; c++)
                {
                    bool checkRnd = RootScale != other[c].LatestChecker.RootScale;

                    if (IsCollidingWith(other[c].LatestChecker, checkRnd))
                    {
                        RootPosition += ScaleV3(pushDir);
                        latestPush = other[c].LatestChecker;
                        pushed = true;
                        collided = true;
                    }
                }

                if (!collided) break;
            }

            if (pushed)
            {
                if (latestPush != null)
                {
                    RootPosition -= (latestPush.ScaleV3(pushDir) - ScaleV3(pushDir)) / 2f;
                }
            }
        }

        public void StepPushToAlignCollision(List<Planning.FieldPlanner> other, Vector3Int pushDir, int maxIters = 128)
        {
            // First Check if collision with something is possible
            // if yes ten choose the nearest one to align with
            Planning.FieldPlanner nearest = null;
            float nearestD = float.MaxValue;
            Bounds mBounds = GetFullBoundsWorldSpace();

            for (int c = other.Count-1; c >= 0; c--)
            {
                if (other[c].LatestChecker == this) { other.RemoveAt(c); continue; }

                float? dist = CheckIfCollisionPossible(mBounds.center, pushDir, other[c].LatestChecker, true);
                if (dist != null)
                {
                    if (dist.Value < nearestD)
                    {
                        nearestD = dist.Value;
                        nearest = other[c];
                    }
                }
            }

            if (nearest == null) return;

            Vector3 backupRootPos = RootPosition;
            bool pushed = false;

            for (int i = 0; i < maxIters; i++)
            {
                bool collided = false;
                RootPosition += ScaleV3(pushDir);

                for (int c = 0; c < other.Count; c++)
                {
                    bool checkRnd = RootScale != other[c].LatestChecker.RootScale;

                    if (IsCollidingWith(other[c].LatestChecker, checkRnd))
                    {
                        //UnityEngine.Debug.Log("collision with = " + other[c].ArrayNameString);
                        RootPosition -= ScaleV3(pushDir);
                        collided = true;
                        pushed = true;
                    }

                    if (collided) break;
                }

                if (collided) break;
            }

            if (!pushed)
            {
                //UnityEngine.Debug.Log("not pueshed, end on " + RootPosition + " backing up " + backupRootPos);
                RootPosition = backupRootPos;
            }
            else
            {
                //UnityEngine.Debug.Log("pueshed");
            }
        }

        public int FindAlignmentsInDirection(CheckerField3D other, Vector3 dir, int desiredAlignments)
        {
            if (desiredAlignments <= 0) return 0;
            if (dir == Vector3.zero) return 0;

            Vector3 latestPos = RootPosition;
            int lastAlign = CountAlignmentsWith(other);
            for (int i = 0; i < desiredAlignments; i++)
            {
                RootPosition += ScaleV3(dir);

                if (!IsCollidingWith(other))
                {
                    int alignments = CountAlignmentsWith(other);

                    if (alignments > lastAlign)
                    {
                        lastAlign = alignments;
                        if (alignments == desiredAlignments) return alignments;
                    }
                    else
                    {
                        RootPosition = latestPos;
                        return lastAlign;
                    }
                }
                else
                {
                    RootPosition = latestPos;
                    return lastAlign;
                }
            }

            return lastAlign;
        }

        public int ShiftForAlignPoints(CheckerField3D other, Vector3 helperDirection, int minimumAlignPoints = 1)
        {
            Vector3 rootPosCopy = RootPosition;
            Vector3 normDir = helperDirection.normalized;
            Vector3 off = Vector3.zero;

            // Offset In X
            if (normDir.x != 0)
            {
                if (normDir.x > 0f) off = new Vector3(RootScale.x, 0, 0);
                else off = new Vector3(-RootScale.x, 0, 0);

                RootPosition += off;
                if (!IsCollidingWith(other))
                {
                    if (IsAnyAligning(other))
                    {
                        return FindAlignmentsInDirection(other, off, minimumAlignPoints);
                    }
                }

                // Restore for new axis
                RootPosition = rootPosCopy;
            }

            if (normDir.y != 0)
            {
                if (normDir.y > 0f) off = new Vector3(0, RootScale.y, 0);
                else off = new Vector3(0, -RootScale.y, 0);

                RootPosition += off;
                if (!IsCollidingWith(other))
                {
                    if (IsAnyAligning(other))
                    {
                        return FindAlignmentsInDirection(other, off, minimumAlignPoints);
                    }
                }

                // Restore for new axis
                RootPosition = rootPosCopy;
            }

            if (normDir.z != 0)
            {
                // Offset In Z
                if (normDir.z > 0f) off = new Vector3(0, 0, RootScale.x);
                else off = new Vector3(0, 0, -RootScale.x);

                RootPosition += off;
                if (!IsCollidingWith(other))
                {
                    if (IsAnyAligning(other))
                    {
                        return FindAlignmentsInDirection(other, off, minimumAlignPoints);
                    }
                }
            }

            // No direction - restore
            RootPosition = rootPosCopy;

            return 0;
        }

        public int CountAlignmentsWith(CheckerField3D other)
        {
            int alignments = 0;

            if (other.ChildPositionsCount > ChildPositionsCount)
            {
                for (int i = 0; i < AllCells.Count; i++)
                    alignments += CountCellAlignedTo(AllCells[i], other);
            }
            else
            {
                for (int i = 0; i < other.AllCells.Count; i++)
                    alignments += other.CountCellAlignedTo(other.AllCells[i], this);
            }

            return alignments;
        }

        public bool IsAnyAligning(CheckerField3D other)
        {
            for (int i = 0; i < AllCells.Count; i++)
            {
                Vector3 wPos = GetWorldPos(AllCells[i]);

                if (other.ContainsWorld(wPos + Vector3.right)) { return true; }
                if (other.ContainsWorld(wPos + Vector3.left)) { return true; }
                if (other.ContainsWorld(wPos + Vector3.forward)) { return true; }
                if (other.ContainsWorld(wPos + Vector3.back)) { return true; }
            }

            return false;
        }

        public int CountCellAlignedTo(FieldCell cell, CheckerField3D other)
        {
            Vector3 wPos = GetWorldPos(cell);
            int alignments = 0;

            var mx = other.Matrix.inverse;

            if (other.ContainsWorld(wPos, mx)) { return 0; } // Is inside

            if (other.ContainsWorld(wPos + Vector3.right * RootScale.x, mx, false)) { alignments += 1; }
            if (other.ContainsWorld(wPos + Vector3.left * RootScale.x, mx, false)) { alignments += 1; }
            if (other.ContainsWorld(wPos + Vector3.forward * RootScale.z, mx, false)) { alignments += 1; }
            if (other.ContainsWorld(wPos + Vector3.back * RootScale.z, mx, false)) { alignments += 1; }

            //if (other.ContainsWorld(wPos + Vector3.right)) { alignments += 1; UnityEngine.Debug.DrawRay(wPos + Vector3.right, Vector3.up, Color.green, 1.01f); }
            //if (other.ContainsWorld(wPos + Vector3.left)) { alignments += 1; UnityEngine.Debug.DrawRay(wPos + Vector3.left, Vector3.up, Color.green, 1.01f); }
            //if (other.ContainsWorld(wPos + Vector3.forward)) { alignments += 1; UnityEngine.Debug.DrawRay(wPos + Vector3.forward, Vector3.up, Color.green, 1.01f); }
            //if (other.ContainsWorld(wPos + Vector3.back)) { alignments += 1; UnityEngine.Debug.DrawRay(wPos + Vector3.back, Vector3.up, Color.green, 1.01f); }

            return alignments;
        }

        public void PushOutOfCollision(CheckerField3D otherField, bool roundAccordingly = false, CheckerField3D collisionChecker = null)
        {
            if (collisionChecker == null) collisionChecker = otherField;

            if (IsCollidingWith(collisionChecker))
            {
                PushOutAway(otherField, roundAccordingly);
                AlignTo(otherField);
            }
        }



        /// <summary>
        /// Pushing out of collision without snapping, just ensure push out of any collision area
        /// </summary>
        public void PushOutOfCollisionAway(CheckerField3D otherField, bool roundAccordingly = false)
        {
            if (roundAccordingly) RoundRootPositionAccordingly(otherField);

            if (IsCollidingWith(otherField))
            {
                PushOutAway(otherField, roundAccordingly);
            }
        }

        public void PushOutAway(CheckerField3D otherField, bool roundAccordingly = false)
        {
            if (roundAccordingly) RoundRootPositionAccordingly(otherField);

            var fullBounds = GetFullBoundsWorldSpace();
            var otherFullBounds = otherField.GetFullBoundsWorldSpace();

            Vector3 outVector = fullBounds.center - otherFullBounds.center;
            Vector3 dirN = outVector.normalized;

            if (dirN == Vector3.zero) dirN = GetRandomFlatDirection();
            dirN = FVectorMethods.ChooseDominantAxis(dirN);

            // Define how much to shift the checker to snap out of collision
            Vector3 targetPoint = otherFullBounds.ClosestPoint(fullBounds.center + dirN * 1000000);
            Vector3 counterPoint = otherFullBounds.ClosestPoint(otherFullBounds.center - dirN * 1000000);

            dirN = (targetPoint - counterPoint).normalized;

            int magn = Mathf.FloorToInt((targetPoint - counterPoint).magnitude);
            if (magn == 0) magn = 1;

            RootPosition += dirN.normalized * magn;
        }


        private List<FieldCell> _CollisionCells = new List<FieldCell>();
        /// <summary> Copy result if needed multiple list instances!  </summary>
        List<FieldCell> GetCollisionCellsWith(CheckerField3D other)
        {
            _CollisionCells.Clear();

            CheckerField3D toCheck;
            CheckerField3D counterCheck;

            if (other.ChildPositionsCount > ChildPositionsCount)
            {
                toCheck = this;
                counterCheck = other;
            }
            else
            {
                toCheck = other;
                counterCheck = this;
            }

            Vector3 wPos;
            Matrix4x4 mx = Matrix.inverse;

            for (int i = 0; i < toCheck.AllCells.Count; i++)
            {
                wPos = toCheck.GetWorldPos(i);
                if (counterCheck.ContainsWorld(wPos))
                {
                    _CollisionCells.Add(GetCellInWorldPos(wPos, mx));
                }
            }

            return _CollisionCells;
        }

        public void DebugLogDrawCellInWorldSpace(FieldCell cell, Color color, float drawDur = 1.1f)
        {
#if UNITY_EDITOR
            DebugLogDrawCellInWorldSpace(cell.Pos, color, drawDur);
#endif
        }

        public void DebugLogDrawCellInWorldSpace(Vector3Int localPos, Color color, float drawDur = 1.1f)
        {
#if UNITY_EDITOR
            DebugLogDrawCellIn(GetWorldPos(localPos), color, drawDur);
#endif
        }

        public void DebugLogDrawCellsInWorldSpace(Color color, float drawDur = 1.1f)
        {
#if UNITY_EDITOR
            for (int i = 0; i < ChildPositionsCount; i++)
            {
                DebugLogDrawCellIn(GetWorldPos(i), color, drawDur);
            }
#endif
        }

        public void DebugLogDrawCellIn(Vector3 worldPos, Color color, float drawDur = 1.1f)
        {
#if UNITY_EDITOR
            Vector3 drawSize = new Vector3(RootScale.x * 0.9f, RootScale.y * 0.1f, RootScale.z * 0.9f);
            Bounds b = new Bounds(worldPos, drawSize);
            FDebug.DrawBounds2D(b, color, b.center.y, 1f, drawDur);
#endif
        }

        public void DebugLogDrawLocalCellIn(Vector3Int localPos, Color color, float drawDur = 1.1f)
        {
#if UNITY_EDITOR
            DebugLogDrawCellIn(LocalToWorld(localPos), color, drawDur);
#endif
        }

        public void DebugLogDrawBoundings(Color color)
        {
#if UNITY_EDITOR
            if (Bounding.Count > 0)
            {
                for (int i = 0; i < Bounding.Count; i++)
                {
                    Bounds b = LocalToWorldBounds(Bounding[i]);
                    FDebug.DrawBounds2D(b, color, b.center.y, RootScale.x);
                }
            }
            else
            {
                Vector3 drawSize = new Vector3(RootScale.x * 0.9f, RootScale.y * 0.1f, RootScale.z * 0.9f);
                for (int i = 0; i < AllCells.Count; i++)
                {
                    Bounds b = new Bounds(GetWorldPos(i), drawSize);
                    FDebug.DrawBounds2D(b, color, b.center.y, 1f);
                }
            }
#endif
        }

        private Vector3 GetRandomFlatDirection()
        {
            int r = FGenerators.GetRandom(0, 4);
            if (r == 0) return Vector3.right;
            else if (r == 1) return Vector3.left;
            else if (r == 2) return Vector3.forward;
            return Vector3.back;
        }

        private readonly Vector3Int[] _randomFlatDirs = new Vector3Int[4];
        public Vector3Int[] GetRandomFlatDirections()
        {
            _randomFlatDirs[0] = Vector3Int.right;
            _randomFlatDirs[1] = Vector3Int.left;
            _randomFlatDirs[2] = new Vector3Int(0, 0, 1);
            _randomFlatDirs[3] = new Vector3Int(0, 0, -1);

            if (FGenerators.GetRandom(0f, 1f) < 0.5f)
                FGenerators.SwapElements(_randomFlatDirs, 0, FGenerators.GetRandom(0, 4));

            if (FGenerators.GetRandom(0f, 1f) < 0.5f)
                FGenerators.SwapElements(_randomFlatDirs, 1, FGenerators.GetRandom(0, 4));

            if (FGenerators.GetRandom(0f, 1f) < 0.5f)
                FGenerators.SwapElements(_randomFlatDirs, 2, FGenerators.GetRandom(0, 4));

            if (FGenerators.GetRandom(0f, 1f) < 0.5f)
                FGenerators.SwapElements(_randomFlatDirs, 3, FGenerators.GetRandom(0, 4));

            return _randomFlatDirs;
        }

        /// <summary> Not supporting rotation! Use GetTransponedBounding for rotation support </summary>
        public Bounds LocalToWorldBounds(Bounds value)
        {
            return new Bounds(Matrix.MultiplyPoint3x4(value.center), value.size);
        }

        /// <summary> Not supporting rotation! Use GetTransponedBounding for rotation support </summary>
        public Bounds WorldToLocalBounds(Bounds value)
        {
            return new Bounds(Matrix.inverse.MultiplyPoint3x4(value.center), value.size);
        }

        public void PushOutOfCollision(List<CheckerField3D> others)
        {
            for (int i = 0; i < others.Count; i++)
            {
                if (others[i] == this) continue;
                PushOutOfCollision(others[i]);
            }
        }

        public void PushOutOfCollision(List<Planning.FieldPlanner> others)
        {
            for (int i = 0; i < others.Count; i++)
            {
                if (others[i].LatestChecker == this) continue;
                PushOutOfCollision(others[i].LatestChecker);
            }
        }

    }
}