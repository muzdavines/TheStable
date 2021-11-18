using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Utility functions.
    /// </summary>
    public sealed class Util
    {
        public static Mesh Clone(Mesh mesh, int[] triangles)
        {
            Mesh copy = new Mesh();
            copy.Clear();
            copy.vertices = mesh.vertices;
            copy.triangles = triangles;
            copy.uv = mesh.uv;
            copy.uv2 = mesh.uv2;
            copy.uv3 = mesh.uv3;
            copy.uv4 = mesh.uv4;
            copy.colors = mesh.colors;
            copy.subMeshCount = 1;
            copy.normals = mesh.normals;
            
            return copy;
        }

        public static Collider[] Colliders = new Collider[256];
        public static RaycastHit[] Hits = new RaycastHit[256];

        // Cache
        private static List<DarkZone> _dark = new List<DarkZone>();
        private static List<LightZone> _light = new List<LightZone>();
        private static List<GrassZone> _grass = new List<GrassZone>();

        public static float GetViewDistance(Vector3 position, float viewDistance, bool isAlerted)
        {
            var count = Physics.OverlapSphereNonAlloc(position, 1, Colliders, Layers.Zones, QueryTriggerInteraction.Collide);

            _dark.Clear();
            _light.Clear();
            _grass.Clear();

            for (int i = 0; i < count; i++)
            {
                {
                    var zone = DarkZone.Get(Colliders[i].gameObject);
                    if (zone != null) _dark.Add(zone);
                }
                {
                    var zone = LightZone.Get(Colliders[i].gameObject);
                    if (zone != null) _light.Add(zone);
                }
                {
                    var zone = GrassZone.Get(Colliders[i].gameObject);
                    if (zone != null) _grass.Add(zone);
                }
            }

            return GetViewDistance(viewDistance, _dark, _light, _grass, isAlerted);
        }

        public static float GetViewDistance(float viewDistance, List<DarkZone> dark, List<LightZone> light, List<GrassZone> grass, bool isAlerted)
        {
            var value = viewDistance;

            if (dark != null)
                for (int i = 0; i < dark.Count; i++)
                {
                    var zone = dark[i];
                    var newValue = 0f;

                    switch (zone.Type)
                    {
                        case VisibilityType.constant: newValue = isAlerted ? zone.AlertValue : zone.DefaultValue; break;
                        case VisibilityType.multiplier: newValue = viewDistance * (isAlerted ? zone.AlertValue : zone.DefaultValue); break;
                    }

                    if (newValue < value)
                        value = newValue;
                }

            if (light != null)
                for (int i = 0; i < light.Count; i++)
                {
                    var zone = light[i];
                    var newValue = 0f;

                    switch (zone.Type)
                    {
                        case VisibilityType.constant: newValue = zone.Value; break;
                        case VisibilityType.multiplier: newValue = viewDistance * zone.Value; break;
                    }

                    if (newValue > value)
                        value = newValue;
                }

            if (grass != null)
                for (int i = 0; i < grass.Count; i++)
                {
                    var zone = grass[i];
                    var newValue = 0f;

                    switch (zone.Type)
                    {
                        case VisibilityType.constant: newValue = isAlerted ? zone.AlertValue : zone.DefaultValue; break;
                        case VisibilityType.multiplier: newValue = viewDistance * (isAlerted ? zone.AlertValue : zone.DefaultValue); break;
                    }

                    if (newValue < value)
                        value = newValue;
                }

            return value;
        }

        public static T[] GetInterfaces<T>(GameObject gameObject) where T : class
        {
            var components = gameObject.GetComponents(typeof(MonoBehaviour));
            var count = 0;

            for (int i = 0; i < components.Length; i++)
                if (components[i] is T)
                    count++;

            var array = new T[count];

            count = 0;

            for (int i = 0; i < components.Length; i++)
                if (components[i] is T)
                    array[count++] = (T)(object)components[i];

            return array;
        }

        public static bool IsFree(GameObject gameObject, Vector3 origin, Vector3 direction, float distance, bool coverMeansFree, bool actorMeansFree)
        {
            var count = Physics.RaycastNonAlloc(origin,
                                                direction,
                                                Hits,
                                                distance);

            var isFree = true;

            for (int i = 0; i < count; i++)
            {
                if (coverMeansFree && Hits[i].collider.isTrigger && Hits[i].collider.GetComponent<Cover>() != null)
                    return true;

                if (actorMeansFree && !Hits[i].collider.isTrigger && Actors.Get(Hits[i].collider.gameObject) != null)
                    return true;

                if (!Hits[i].collider.isTrigger && !InHiearchyOf(Hits[i].collider.gameObject, gameObject))
                    isFree = false;
            }

            return isFree;
        }

        public static bool GetClosestCover(Vector3 position, float radius, ref Cover resultCover, ref Vector3 resultPosition)
        {
            var minDistance = 0f;
            var count = Physics.OverlapSphereNonAlloc(position, radius, Colliders, Layers.Cover, QueryTriggerInteraction.Collide);

            resultCover = null;

            for (int i = 0; i < count; i++)
            {
                var cover = CoverSearch.GetCover(Util.Colliders[i].gameObject);

                if (cover != null)
                {
                    var point = cover.ClosestPointTo(position, 0.3f, 0.3f);
                    var distance = Vector3.Distance(position, point);

                    if (distance < minDistance || resultCover == null)
                    {
                        resultCover = cover;
                        resultPosition = point;
                        minDistance = distance;
                    }
                }
            }

            return resultCover != null;
        }

        public static RaycastHit GetClosestRaycastHit(Vector3 origin, Vector3 target, float minDistance, GameObject ignore = null)
        {
            var vector = (target - origin).normalized;
            var maxDistance = Vector3.Distance(origin, target);

            RaycastHit closestHit = new RaycastHit();

            var count = Physics.RaycastNonAlloc(origin, vector, Hits);

            for (int i = 0; i < count; i++)
            {
                var hit = Hits[i];

                if (hit.collider.gameObject != ignore && !hit.collider.isTrigger)
                    if (hit.distance > minDistance && hit.distance < maxDistance)
                    {
                        maxDistance = hit.distance;
                        closestHit = hit;
                    }
            }

            return closestHit;
        }

        public static Vector3 GetClosestNonActorHit(Vector3 origin, Vector3 target, float minDistance, GameObject ignore = null)
        {
            var vector = (target - origin).normalized;
            var maxDistance = Vector3.Distance(origin, target);
            var closestHit = target;
            var count = Physics.RaycastNonAlloc(origin, vector, Hits);

            for (int i = 0; i < count; i++)
            {
                var hit = Hits[i];

                if (hit.collider.gameObject != ignore && !hit.collider.isTrigger)
                    if (hit.distance > minDistance && hit.distance < maxDistance)
                    if (Actors.Get(hit.collider.gameObject) == null)
                    {
                        maxDistance = hit.distance;
                        closestHit = hit.point;
                    }
            }

            return closestHit;
        }

        public static Vector3 GetClosestHitIgnoreSide(Vector3 origin, Vector3 target, float minDistance, int side, out Vector3 normal)
        {
            var vector = (target - origin).normalized;
            var maxDistance = Vector3.Distance(origin, target);
            var closestHit = target;
            var count = Physics.RaycastNonAlloc(origin, vector, Hits);

            normal = Vector3.zero;

            for (int i = 0; i < count; i++)
            {
                var hit = Hits[i];

                if (!hit.collider.isTrigger)
                    if (hit.distance > minDistance && hit.distance < maxDistance)
                    {
                        var actor = Actors.Get(hit.collider.gameObject);

                        if (actor == null || actor.Side != side)
                        {
                            maxDistance = hit.distance;
                            closestHit = hit.point;
                            normal = hit.normal;
                        }
                    }
            }

            return closestHit;
        }

        public static Vector3 GetClosestStaticHit(Vector3 origin, Vector3 target, float minDistance, GameObject ignore = null)
        {
            var vector = (target - origin).normalized;
            var maxDistance = Vector3.Distance(origin, target);
            var closestHit = target;
            var count = Physics.RaycastNonAlloc(origin, vector, Hits, maxDistance, Layers.Geometry, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                var hit = Hits[i];

                if (hit.collider.gameObject != ignore && !hit.collider.isTrigger)
                    if (hit.distance > minDistance && hit.distance < maxDistance)
                    {
                        maxDistance = hit.distance;
                        closestHit = hit.point;
                    }
            }

            return closestHit;
        }

        public static Vector3 GetClosestHit(Vector3 origin, Vector3 target, float minDistance, GameObject ignore = null)
        {
            var vector = (target - origin).normalized;
            var maxDistance = Vector3.Distance(origin, target);
            var closestHit = target;
            var count = Physics.RaycastNonAlloc(origin, vector, Hits);

            for (int i = 0; i < count; i++)
            {
                var hit = Hits[i];

                if (hit.collider.gameObject != ignore && !hit.collider.isTrigger)
                    if (hit.distance > minDistance && hit.distance < maxDistance)
                    {
                        maxDistance = hit.distance;
                        closestHit = hit.point;
                    }
            }

            return closestHit;
        }

        public static float RandomUnobstructedAngle(GameObject gameObject, Vector3 position, float maxObstruction, int attempts = 10)
        {
            return RandomUnobstructedAngle(gameObject, position, 0, 360, maxObstruction, attempts);
        }

        public static float RandomUnobstructedAngle(GameObject gameObject, Vector3 position, float current, float angleRange, float maxObstruction, int attempts = 10)
        {
            int attemptsLeft = attempts;

            while (attemptsLeft > 0)
            {
                float angle;

                if (attemptsLeft > attempts / 2)
                    angle = Random.Range(current - angleRange / 2f, current + angleRange / 2f);
                else
                    angle = Random.Range(0f, 360f);

                var vector = HorizontalVector(angle);

                if (IsFree(gameObject, position, vector, maxObstruction, false, true))
                    return angle;

                attemptsLeft--;
            }

            return Random.Range(0f, 360f);
        }

        // source: https://docs.unity3d.com/Manual/PlatformDependentCompilation.html
        public static float GetPinch()
        {
            if (Input.touchCount == 2)
            {
                // Store both touches.
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                // Find the position in the previous frame of each touch.
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                // Find the magnitude of the vector (the distance) between the touches in each frame.
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                // Find the difference in the distances between each frame.
                return touchDeltaMag - prevTouchDeltaMag;
            }

            return 0;
        }

        private const float Multiplier = 2;

        public static float Lerp(float Value, float Target, float rate)
        {
            return Value + (Target - Value) * Mathf.Clamp01(1 - Mathf.Exp(-rate * Multiplier * Time.deltaTime));
        }

        public static float LerpAngle(float Value, float Target, float rate)
        {
            return Mathf.LerpAngle(Value, Target, 1 - Mathf.Exp(-rate * Multiplier * Time.deltaTime));
        }

        public static Vector3 Lerp(Vector3 Value, Vector3 Target, float rate)
        {
            return Value + (Target - Value) * Mathf.Clamp01(1 - Mathf.Exp(-rate * Multiplier * Time.deltaTime));
        }

        public static Vector3 Slerp(Vector3 Value, Vector3 Target, float rate)
        {
            return Vector3.Slerp(Value, Target, Mathf.Clamp01(1 - Mathf.Exp(-rate * Multiplier * Time.deltaTime)));
        }

        public static Quaternion Lerp(Quaternion Value, Quaternion Target, float rate)
        {
            return Quaternion.Slerp(Value, Target, 1 - Mathf.Exp(-rate * Multiplier * Time.deltaTime));
        }

        public static void Lerp(ref float Value, float Target, float rate)
        {
            Value = Value + (Target - Value) * Mathf.Clamp01(1 - Mathf.Exp(-rate * Multiplier * Time.deltaTime));
        }

        public static void LerpAngle(ref float Value, float Target, float rate)
        {
            Value = Mathf.LerpAngle(Value, Target, 1 - Mathf.Exp(-rate * Multiplier * Time.deltaTime));
        }

        public static void Lerp(ref Vector3 Value, Vector3 Target, float rate)
        {
            Value = Value + (Target - Value) * Mathf.Clamp01(1 - Mathf.Exp(-rate * Multiplier * Time.deltaTime));
        }

        public static void Slerp(ref Vector3 Value, Vector3 Target, float rate)
        {
            Value = Vector3.Slerp(Value, Target, Mathf.Clamp01(1 - Mathf.Exp(-rate * Multiplier * Time.deltaTime)));
        }

        public static void Lerp(ref Quaternion Value, Quaternion Target, float rate)
        {
            Value = Quaternion.Slerp(Value, Target, 1 - Mathf.Exp(-rate * Multiplier * Time.deltaTime));
        }

        public static void Move(ref Vector3 Value, Vector3 Target, float speed)
        {
            var delta = Target - Value;
            var distance = delta.magnitude;

            if (distance <= float.Epsilon)
                return;

            if (distance < speed)
                Value = Target;
            else
                Value += delta * speed / distance;
        }

        public static void Move(ref float Value, float Target, float speed)
        {
            if (Target > Value)
            {
                if (Value + speed > Target)
                    Value = Target;
                else if (speed > 0)
                    Value += speed;
            }
            else
            {
                if (Value - speed < Target)
                    Value = Target;
                else if (speed > 0)
                    Value -= speed;
            }
        }

        public static void MoveAngle(ref float Value, float Target, float speed)
        {
            var delta = Mathf.DeltaAngle(Value, Target);

            if (delta > 0)
            {
                if (Value + speed > Target)
                    Value = Target;
                else if (speed > 0)
                    Value += speed;
            }
            else
            {
                if (Value - speed < Target)
                    Value = Target;
                else if (speed > 0)
                    Value -= speed;
            }
        }

        /// <summary>
        /// Is the given target object is inside the parent hierarchy.
        /// </summary>
        public static bool InHiearchyOf(GameObject target, GameObject parent)
        {
            var obj = target;

            if (target.transform.root == parent.transform)
                return true;

            while (obj != null)
            {
                if (obj == parent)
                    return true;

                if (obj.transform.parent != null)
                    obj = obj.transform.parent.gameObject;
                else
                    obj = null;
            }

            return false;
        }

        /// <summary>
        /// Delta of a point on AB line closest to the given point.
        /// </summary>
        public static float FindDeltaPath(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 ap = point - a;
            Vector3 ab = b - a;
            float ab2 = ab.x * ab.x + +ab.z * ab.z;
            float ap_ab = ap.x * ab.x + ap.z * ab.z;
            float t = ap_ab / ab2;

            return t;
        }

        /// <summary>
        /// Position of a point on AB line closest to the given .point.
        /// </summary>
        public static Vector3 FindClosestToPath(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 ap = point - a;
            Vector3 ab = b - a;
            float ab2 = ab.x * ab.x + +ab.z * ab.z;
            float ap_ab = ap.x * ab.x + ap.z * ab.z;
            float t = ap_ab / ab2;

            return a + ab * Mathf.Clamp01(t);
        }

        /// <summary>
        /// Calculates a vector given the horizontal angle.
        /// </summary>
        public static Vector3 HorizontalVector(float angle)
        {
            angle *= Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
        }

        /// <summary>
        /// Calculates a vector given the horizontal angle.
        /// </summary>
        public static Vector3 Vector(float horizontal, float vertical)
        {
            horizontal *= Mathf.Deg2Rad;
            vertical *= Mathf.Deg2Rad;

            return new Vector3(Mathf.Sin(horizontal), -Mathf.Sin(vertical), Mathf.Cos(horizontal));
        }

        /// <summary>
        /// Calculates horizontal angle of the given vector.
        /// </summary>
        public static float HorizontalAngle(Vector3 vector)
        {
            var v = new Vector2(vector.z, vector.x);

            if (v.sqrMagnitude > 0.01f)
                v.Normalize();

            var sign = (v.y < 0) ? -1.0f : 1.0f;
            return Vector2.Angle(Vector2.right, v) * sign;
        }

        /// <summary>
        /// Calculates vertical angle in degrees.
        /// </summary>
        public static float VerticalAngle(Vector3 vector)
        {
            var horizontal = vector;
            horizontal.y = 0;

            var sign = (vector.y > 0) ? -1.0f : 1.0f;
            return Vector2.Angle(Vector2.right, new Vector2(horizontal.magnitude, vector.y)) * sign;
        }

        /// <summary>
        /// Calculates vertical angle in degrees.
        /// </summary>
        public static float VerticalAngle(float height, float distance)
        {
            var sign = (height > 0) ? -1.0f : 1.0f;
            return Vector2.Angle(Vector2.right, new Vector2(distance, height)) * sign;
        }

        /// <summary>
        /// An utility function to calculate a distance between a point and a segment.
        /// </summary>
        public static float DistanceToSegment(Vector3 point, Vector3 p0, Vector3 p1)
        {
            var lengthSqr = (p1 - p0).sqrMagnitude;
            if (lengthSqr <= float.Epsilon) return Vector3.Distance(point, p0);

            var t = Mathf.Clamp01(((point.x - p0.x) * (p1.x - p0.x) +
                                   (point.y - p0.y) * (p1.y - p0.y) +
                                   (point.z - p0.z) * (p1.z - p0.z)) / lengthSqr);

            return Vector3.Distance(point, p0 + (p1 - p0) * t);
        }

        /// <summary>
        /// An utility function to calculate a vector between a point and a segment.
        /// </summary>
        public static Vector3 VectorToSegment(Vector3 point, Vector3 p0, Vector3 p1)
        {
            var lengthSqr = (p1 - p0).sqrMagnitude;
            if (lengthSqr <= float.Epsilon) return p0 - point;

            var t = Mathf.Clamp01(((point.x - p0.x) * (p1.x - p0.x) +
                                   (point.y - p0.y) * (p1.y - p0.y) +
                                   (point.z - p0.z) * (p1.z - p0.z)) / lengthSqr);

            return p0 + (p1 - p0) * t - point;
        }
    }
}