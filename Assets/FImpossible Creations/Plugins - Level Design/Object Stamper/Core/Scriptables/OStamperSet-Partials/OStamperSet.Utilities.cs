using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class OStamperSet
    {
        [HideInInspector] public bool _editor_drawSettings = true;
        [HideInInspector] public bool _editor_drawPrefabs = true;
        [HideInInspector] public bool _editor_drawThumbs = true;

        public enum EOSPlacement { LayAlign, PlantAlign }
        public enum EOSRaystriction { None, AvoidAnyOtherStamper, AllowStackOnSelected, DisallowOnlyOnSelected }


        /// <summary>
        /// Refreshing reference bounds and all prefabs bounds
        /// </summary>
        public void RefreshBounds()
        {
            if (Prefabs == null) return;
            if (Prefabs.Count == 0) return;

            for (int i = 0; i < Prefabs.Count; i++)
            {
                if (Prefabs[i] == null) continue;
                Prefabs[i].RefreshBounds();
            }

            ReferenceBounds = new Bounds();
            for (int i = 0; i < Prefabs.Count; i++)
            {
                if (Prefabs[i] == null) continue;
                if (Prefabs[i].GameObject == null) continue;
                ReferenceBounds.Encapsulate(Prefabs[i].ReferenceBoundsFull);
            }
        }

        public Vector3 GetBoundedDirection(Vector3 localDirection)
        {
            return Vector3.Scale(ReferenceBounds.size, localDirection);
        }

        /// <summary>
        /// Use AngleStepForAxis and RandRotationAxis
        /// </summary>
        public static float GetAngleFor(float step, float randAxis, float random = 1f)
        {
            return Mathf.Round(randAxis * random) * step;
        }

        public float GetRandomRotation(float step)
        {
            int rotsAvailableP = Mathf.RoundToInt(RotationRanges.y / step);
            int rotsAvailableN = Mathf.RoundToInt(-RotationRanges.x / step);
            return FGenerators.GetRandom(-rotsAvailableN, rotsAvailableP);
        }

        public Vector3 R(Vector3 to)
        { return new Vector3(R(to.x), R(to.y), R(to.z)); }

        public float R(float to)
        { return FGenerators.GetRandom(-to, to); }

        public static Vector3 GetShiftedAxis(Vector3 target)
        { return new Vector3(target.y, target.z, target.x); }

        public static Vector3 GetCrossOnSingle(Vector3 axis)
        { return Vector3.Cross(axis, GetShiftedAxis(axis)); }
    }
}