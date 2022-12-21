#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{

    [System.Serializable]
    public class OSPrefabReference : PrefabReference
    {
        public float Propability = 1f;

        /// <summary> Local space bounds built out of all meshes found inside prefab </summary>
        public Bounds ReferenceBoundsFull;
        /// <summary> Local space bounds built out of first meshe found inside prefab </summary>
        public Bounds ReferenceBounds;
        /// <summary> If some bounds are detected as flat plane </summary>
        public bool FlatBounds = false;

#if UNITY_EDITOR
        protected override void DrawGUIWithPrefab(Color color, int previewSize = 72, string predicate = "", Action clickCallback = null, Action removeCallback = null, bool drawThumbnail = true, bool drawPrefabField = true)
        {
            base.DrawGUIWithPrefab(color, previewSize, predicate, clickCallback, removeCallback, drawThumbnail, drawPrefabField);

            if (Propability <= 0f) GUI.color = Color.gray;
            EditorGUILayout.BeginHorizontal(PrefabReference.opt2);
            EditorGUILayout.LabelField(new GUIContent("%", "Propability for choosed prefab"), EditorStyles.miniLabel, GUILayout.Width(12));
            Propability = GUILayout.HorizontalSlider(Propability, 0f, 1f);
            EditorGUILayout.EndHorizontal();
            if (Propability <= 0f) GUI.color = Color.white;
        }
#endif


        public override void OnPrefabChanges()
        {
            RefreshBounds();
        }


        public void RefreshBounds()
        {
            GameObject Prefab = GameObject;
            if (Prefab == null) { ReferenceBounds = new Bounds(); ReferenceBoundsFull = new Bounds(); return; }
            ReferenceBounds = GetBasicBounds(Prefab);
            ReferenceBoundsFull = BuildFullBounds(Prefab);

            if (HasFlat(ReferenceBounds.size) || HasFlat(ReferenceBoundsFull.size))
            {
                FlatBounds = true;
                //Debug.Log("Flat");
            }
            else
            {
                //Debug.Log("Not Flat");
                FlatBounds = false;
            }
        }

        private bool HasFlat(Vector3 scale)
        {
            float scaleRef = scale.magnitude * 0.075f;
            if (scaleRef < 0.035f) scaleRef = 0.075f;
            if (Mathf.Abs(scale.x) < scaleRef) return true;
            if (Mathf.Abs(scale.y) < scaleRef) return true;
            if (Mathf.Abs(scale.z) < scaleRef) return true;
            return false;
        }

        public Vector3 GetScaledBoundsExt(Vector3 scale)
        {
            return Vector3.Scale(ReferenceBounds.extents, scale);
        }

        public Vector3 GetScaledBoundsExt(Vector3 scale, Vector3 direction)
        {
            return Vector3.Scale(GetScaledBoundsExt(scale), direction);
        }

        public Vector3 GetRotatedBoundsDimension(Quaternion rotation, Vector3 axis)
        {
            return Matrix4x4.Rotate(rotation).MultiplyVector(axis);
        }

        public static Bounds BuildFullBounds(GameObject target)
        {
            Bounds b = new Bounds();
            if (target == null) return b;

            List<SkinnedMeshRenderer> rends = FTransformMethods.FindComponentsInAllChildren<SkinnedMeshRenderer>(target.transform);
            for (int r = 0; r < rends.Count; r++)
            {
                if (rends[r].sharedMesh == null) continue;
                b.Encapsulate(rends[r].sharedMesh.bounds);
            }

            List<MeshFilter> filters = FTransformMethods.FindComponentsInAllChildren<MeshFilter>(target.transform);
            for (int f = 0; f < filters.Count; f++)
            {
                if (filters[f].sharedMesh == null) continue;
                b.Encapsulate(filters[f].sharedMesh.bounds);
            }

            List<Collider> colliders = FTransformMethods.FindComponentsInAllChildren<Collider>(target.transform);
            for (int f = 0; f < colliders.Count; f++)
            {
                if (colliders[f] == null) continue;
                MeshCollider mc = colliders[f] as MeshCollider;
                if (mc) { if (mc.sharedMesh) b.Encapsulate(mc.sharedMesh.bounds); continue; }

                BoxCollider bx = colliders[f] as BoxCollider;
                if (bx) { b.Encapsulate(new Bounds(bx.center, bx.size)); }
            }

            return b;
        }

        public static Bounds GetBasicBounds(GameObject target)
        {
            Bounds b = new Bounds();
            if (target == null) return b;

            List<SkinnedMeshRenderer> rends = FTransformMethods.FindComponentsInAllChildren<SkinnedMeshRenderer>(target.transform);
            for (int r = 0; r < rends.Count; r++)
            { if (rends[r].sharedMesh == null) continue; return rends[r].sharedMesh.bounds; }

            List<MeshFilter> filters = FTransformMethods.FindComponentsInAllChildren<MeshFilter>(target.transform);
            for (int f = 0; f < filters.Count; f++)
            { if (filters[f].sharedMesh == null) continue; return (filters[f].sharedMesh.bounds); }

            List<Collider> colliders = FTransformMethods.FindComponentsInAllChildren<Collider>(target.transform);
            for (int f = 0; f < colliders.Count; f++)
            {
                if (colliders[f] == null) continue; MeshCollider mc = colliders[f] as MeshCollider; if (mc) { if (mc.sharedMesh) return (mc.sharedMesh.bounds); }
                BoxCollider bx = colliders[f] as BoxCollider; if (bx) { return (new Bounds(bx.center, bx.size)); }
            }

            return b;
        }

        public static Vector3[] GetBoundsCorners(Bounds bounds, bool offsetCenter = true)
        {
            float width = bounds.size.x;
            float height = bounds.size.y;

            Vector3 topRight = Vector3.zero, topLeft = Vector3.zero, bottomRight = Vector3.zero, bottomLeft = Vector3.zero;
            Vector3 topBRight, topBLeft, bottomBRight, bottomBLeft;

            topRight.x += width / 2;
            topRight.y += height / 2;

            topLeft.x -= width / 2;
            topLeft.y += height / 2;

            bottomRight.x += width / 2;
            bottomRight.y -= height / 2;

            bottomLeft.x -= width / 2;
            bottomLeft.y -= height / 2;

            topBRight = topRight;
            topBLeft = topLeft;
            bottomBRight = bottomRight;
            bottomBLeft = bottomLeft;

            Vector3[] corners = new Vector3[8] { topRight, topLeft, bottomLeft, bottomRight, topBRight, topBLeft, bottomBLeft, bottomBRight };

            for (int i = 0; i < 4; i++)
                corners[i].z += bounds.extents.z;

            for (int i = 4; i < 8; i++)
                corners[i].z -= bounds.extents.z;

            if (offsetCenter)
                for (int i = 0; i < 8; i++)
                    corners[i] += bounds.center;

            return corners;
        }


    }

}