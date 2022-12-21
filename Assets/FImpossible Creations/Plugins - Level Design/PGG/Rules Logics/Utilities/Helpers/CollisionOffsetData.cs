using UnityEngine;

namespace FIMSpace.Generating.Rules.Helpers
{
    public struct CollisionOffsetData
    {
        public SpawnData parent;
        public Bounds bounds;
        public Bounds boundsWithSpawnOff;
        public Bounds prbounds;
        public Vector3 positionOffset;
        public Vector3 cellOffset;
        public Vector3 scale;
        public Quaternion rotation;
        public string name;

        public CollisionOffsetData(SpawnData spawn, Vector3? cellOffset = null, bool getAll = false)
        {
            parent = spawn;
            if (spawn.Prefab == null)
            {
                Debug.LogError("[Interior Generator] Null Prefab in modificator: " + spawn.OwnerMod);
            }


            name = spawn.Prefab.name;
            bounds = new Bounds(Vector3.zero, new Vector3(0.1f, 0.1f, 0.1f));

            if (spawn.PreviewMesh != null)
                bounds = spawn.PreviewMesh.bounds;

            Vector3 eul = spawn.RotationOffset + spawn.LocalRotationOffset;
            rotation = Quaternion.Euler(eul);

            Renderer filtr = spawn.Prefab.gameObject.GetComponentInChildren<Renderer>();
            if (filtr)
            {
                spawn.Prefab.transform.position = Vector3.zero;
                spawn.Prefab.transform.rotation = rotation;
                bounds = filtr.bounds;

                if (filtr.transform.parent != null)
                    bounds.center -= filtr.transform.TransformVector(filtr.transform.localPosition);

                spawn.Prefab.transform.rotation = Quaternion.identity;
            }

            Collider col = spawn.Prefab.GetComponentInChildren<Collider>();
            if (col)
            {
                // If collider bounds are bigger than renerer's
                if (col.bounds.size.magnitude > bounds.size.magnitude)
                {
                    bounds.size = col.bounds.size;
                }
            }

            // Get all renderers and colliders to generate bounding box
            if (getAll)
            {
                //FDebug.DrawBounds3D(bounds, Color.blue);

                foreach (Renderer r in spawn.Prefab.gameObject.GetComponentsInChildren<Renderer>())
                {
                    Bounds b = r.bounds;
                    //if (r.transform.parent != null) b.center -= r.transform.TransformVector(r.transform.localPosition);
                    bounds.Encapsulate(b);
                }

                foreach (Collider c in spawn.Prefab.gameObject.GetComponentsInChildren<Collider>())
                {
                    Bounds b = c.bounds;
                    //if (c.transform.parent != null) b.center -= c.transform.TransformVector(c.transform.localPosition);
                    bounds.Encapsulate(b);
                }

                Vector3 cnt = bounds.center;
                bounds = FEngineering.RotateBoundsByMatrix(bounds, rotation);
                bounds.center = cnt;
            }

            positionOffset = spawn.Offset + (Quaternion.Euler(spawn.RotationOffset)) * spawn.DirectionalOffset;
            
            scale = spawn.LocalScaleMul;
            bounds.size = Vector3.Scale(bounds.size, spawn.Prefab.transform.localScale);

            if (!getAll)
            {
                if (!filtr && !col)
                {
                    float angle = Quaternion.Angle(rotation, Quaternion.identity);
                    if (angle > 45 && angle < 135) bounds.size = new Vector3(bounds.size.z, bounds.size.y, bounds.size.x);
                    if (angle < 315 && angle > 225) bounds.size = new Vector3(bounds.size.z, bounds.size.y, bounds.size.x);
                }
            }

            if (cellOffset == null) this.cellOffset = Vector3.zero; else this.cellOffset = cellOffset.Value;

            prbounds = PRBounds(bounds, scale, positionOffset - this.cellOffset);
            boundsWithSpawnOff = new Bounds(bounds.center + spawn.GetFullOffset(true), bounds.size);
        }

        public static Bounds PRBounds(Bounds refB, Vector3 scale, Vector3 off)
        {
            return new Bounds(Vector3.Scale(refB.center, scale) + off, Vector3.Scale(refB.size, scale));
        }

        public bool OffsetOn(CollisionOffsetData other, ref SpawnData spawn, float amount, FieldCell cell, float sides, float limit = 2f)
        {

            if (other.prbounds.Intersects(prbounds))
            {
                //sides *= FGenerators.GetRandom(0f, 2f) > 1f ? -1f : 1f;
                //sides = 1f;
                #region Debugging

                //Vector3 cOff = new Vector3(cell.FlatPos.x, 0, cell.FlatPos.y) * 2f;
                //if (cell.FlatPos == new Vector2Int(0, 1))
                //{
                //    Bounds offs = prbounds;
                //    offs.center += cOff;
                //    PlanHelper.DebugBounds3D(offs, Color.green * 0.7f);

                //    offs = other.prbounds;
                //    offs.center += cOff;
                //    PlanHelper.DebugBounds3D(offs, Color.red * 0.8f);
                //}

                #endregion

                Vector3 toCenter = (-other.positionOffset).normalized;
                float startRotY = toCenter == Vector3.zero ? 0f : Quaternion.LookRotation(toCenter).eulerAngles.y;

                float toEdgeFactor = Mathf.Abs(other.prbounds.min.x) + Mathf.Abs(other.prbounds.max.x) + Mathf.Abs(other.prbounds.min.z) + Mathf.Abs(other.prbounds.max.z) + Mathf.Abs(other.prbounds.center.x) + Mathf.Abs(other.prbounds.center.z) + Mathf.Abs(other.positionOffset.x) + Mathf.Abs(other.positionOffset.z);

                float distMul = 2f;
                if (toEdgeFactor > 3f) distMul = Mathf.Lerp(distMul, 0.85f, toEdgeFactor - 3f);

                Vector3 origin = Quaternion.Euler(0, startRotY + 89f * sides, 0f) * Vector3.forward * distMul;
                origin.y = other.prbounds.center.y;

                Vector3 castDir = -origin;
                castDir.y = 0f;
                castDir.Normalize();

                if (toEdgeFactor >= 3f)
                {
                    castDir = Vector3.Lerp(castDir, other.positionOffset.normalized, toEdgeFactor / 2f - 2.5f);
                    origin = Vector3.Lerp(origin, -other.positionOffset, toEdgeFactor / 2f - 2.5f);
                    //castDir = Vector3.Lerp(castDir, (origin - other.prbounds.ClosestPoint(origin)).normalized, toEdgeFactor - 3.5f);
                }

                origin += other.positionOffset;

                #region Debugging

                //if (cell.FlatPos == new Vector2Int(0, 1))
                //{
                //    UnityEngine.Debug.Log("Edg " + toEdgeFactor + " " + other.name);
                //    Debug.DrawRay(origin + cOff, castDir, Color.white, 1.1f);
                //    Debug.DrawRay(origin + cOff, Vector3.up, Color.white * 0.7f, 1.1f);
                //    //UnityEngine.Debug.Log(" " + other.prbounds.min.x + " " + other.prbounds.max.x + " z " + " " + other.prbounds.min.z + " " + other.prbounds.max.z);
                //}

                #endregion

                float distance;
                Ray r = new Ray(origin, castDir);
                if (other.prbounds.IntersectRay(r, out distance))
                {
                    Vector3 hitPoint = r.origin + r.direction * distance;

                    #region Debugging
                    //if (cell.FlatPos == new Vector2Int(0, 1))
                    //{
                    //    UnityEngine.Debug.Log("Intersect with " + other.name + " factor = " + toEdgeFactor);
                    //    Debug.DrawRay(hitPoint + cOff, Vector3.down, Color.magenta, 1.1f);
                    //}
                    #endregion

                    Vector3 desiredPos = hitPoint - r.direction * amount * (prbounds.extents.x + prbounds.extents.z) / 2f;

                    if (Mathf.Abs(desiredPos.x) > limit || Mathf.Abs(desiredPos.z) > limit)
                    {
                        return false;
                    }

                    desiredPos.y = positionOffset.y;
                    spawn.DirectionalOffset = Vector3.zero;
                    spawn.Offset = desiredPos;

                    desiredPos.y = prbounds.center.y;
                    prbounds.center = desiredPos;
                }

            }

            return true;
        }
    }
}