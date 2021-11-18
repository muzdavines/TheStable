using UnityEngine;
using UnityEngine.Profiling;

namespace CoverShooter
{
    /// <summary>
    /// A grenade description passed to functions inside GrenadePath.
    /// </summary>
    public struct GrenadeDescription
    {
        /// <summary>
        /// Gravity applied to the greande.
        /// </summary>
        public float Gravity;
        
        /// <summary>
        /// Time in seconds for the grenade to explode once thrown.
        /// </summary>
        public float Duration;
        
        /// <summary>
        /// Grenade bounciness on the ground.
        /// </summary>
        public float Bounciness;
    }

    /// <summary>
    /// Contains functions used for grenade path calculation.
    /// </summary>
    public static class GrenadePath
    {
        /// <summary>
        /// Calculates point of grenade path origin for the given character.
        /// </summary>
        public static Vector3 Origin(CharacterMotor motor, float lookAngle)
        {
            var origin = Vector3.zero;

            if (motor.IsInLowCover)
                origin = motor.Grenade.CrouchOrigin;
            else
                origin = motor.Grenade.StandingOrigin;

            if (motor.IsThrowingLeft)
                origin.x *= -1;

            float angle = lookAngle;

            origin = Quaternion.AngleAxis(angle, Vector3.up) * origin;

            return origin + motor.transform.position;
        }

        /// <summary>
        /// Moves the grenade by a single step (one game update cycle)
        /// </summary>
        public static void Step(ref Vector3 position, ref Vector3 velocity, float step, float gravity, float bounciness)
        {
            var v = velocity.magnitude;
            var vector = velocity / v;

            RaycastHit hit;
            if (Physics.Raycast(position - vector * 0.1f, vector, out hit, v * step + 0.1f, Layers.Geometry, QueryTriggerInteraction.Ignore))
            {
                position = hit.point;
                velocity = Vector3.Reflect(velocity, hit.normal) * bounciness;
            }
            else
                position += velocity * step;

            velocity -= Vector3.up * gravity * step;
        }

        /// <summary>
        /// Calculates grenade position for every frame and stores it inside the buffer. Returns number of positions calculated.
        /// </summary>
        public static int Calculate(Vector3 start, float horizontalAngle, float angleInDegrees, float velocity, GrenadeDescription desc, Vector3[] buffer, float step = 0.05f)
        {
            if (buffer.Length == 0)
                return 0;

            buffer[0] = start;
            var count = 1;

            var horizontal = Util.HorizontalVector(horizontalAngle);
            var distance = horizontal.magnitude;
            var angle = angleInDegrees * Mathf.Deg2Rad;

            var currentVelocity = (horizontal.normalized * Mathf.Cos(angle) + Vector3.up * Mathf.Sin(angle)) * velocity;
            var position = start;
            var time = 0f;

            Profiler.BeginSample("Grenade path");

            while (count < buffer.Length && time < desc.Duration && currentVelocity.magnitude > 0.1f)
            {
                Step(ref position, ref currentVelocity, step, desc.Gravity, desc.Bounciness);

                time += step;
                buffer[count++] = position;
            }

            Profiler.EndSample();

            return count;
        }

        /// <summary>
        /// Calculates required throw velocity in order for the grenade to reach the target.
        /// </summary>
        public static Vector3 GetVelocity(Vector3 start, Vector3 target, float maxVelocity, float gravity)
        {
            var horizontal = new Vector3(target.x, 0, target.z) - new Vector3(start.x, 0, start.z);
            var height = target.y - start.y;
            var distance = horizontal.magnitude;

            float angle = Mathf.Deg2Rad * 45;
            float velocity = maxVelocity;

            if (getAngle(height, distance, velocity, -gravity, ref angle))
                if (angle > Mathf.Deg2Rad * 45)
                {
                    angle = Mathf.Deg2Rad * 45;
                    velocity = getVelocity(height, distance, -gravity, angle);
                }

            return (horizontal.normalized * Mathf.Cos(angle) + Vector3.up * Mathf.Sin(angle)) * velocity;
        }

        /// <summary>
        /// Calculates grenade position for every frame and stores it inside the buffer. Returns number of positions calculated.
        /// </summary>
        public static int Calculate(Vector3 start, Vector3 target, float maxVelocity, GrenadeDescription desc, Vector3[] buffer, float step = 0.05f)
        {
            if (buffer.Length == 0)
                return 0;

            buffer[0] = start;
            var count = 1;

            var currentVelocity = GetVelocity(start, target, maxVelocity, desc.Gravity);
            var position = start;
            var time = 0f;

            Profiler.BeginSample("Grenade path");

            while (count < buffer.Length && time < desc.Duration && currentVelocity.magnitude > 0.1f)
            {
                Step(ref position, ref currentVelocity, step, desc.Gravity, desc.Bounciness);

                time += step;
                buffer[count++] = position;
            }

            Profiler.EndSample();

            return count;
        }

        /// <summary>
        /// Calculates grenade position for every frame and stores it inside the buffer. Returns number of positions calculated.
        /// </summary>
        public static int Calculate(Vector3 start, Vector3 target, Vector3[] buffer, float step = 0.05f)
        {
            if (buffer.Length == 0)
                return 0;

            buffer[0] = start;
            var count = 1;

            var horizontal = new Vector3(target.x, 0, target.z) - new Vector3(start.x, 0, start.z);
            var height = target.y - start.y;
            var distance = horizontal.magnitude;

            var gravity = 10;
            var angle = Mathf.Deg2Rad * 45;
            var velocity = getVelocity(height, distance, -gravity, angle);
            var currentVelocity = (horizontal.normalized * Mathf.Cos(angle) + Vector3.up * Mathf.Sin(angle)) * velocity;
            var position = start;
            var time = 0f;

            Profiler.BeginSample("Grenade path");

            while (count < buffer.Length && Vector3.Distance(buffer[count], target) > 0.1f)
            {
                position += currentVelocity * step;
                currentVelocity -= Vector3.up * gravity * step;

                time += step;
                buffer[count++] = position;
            }

            Profiler.EndSample();

            return count;
        }

        private static float getVelocity(float height, float distance, float gravity, float angle)
        {
            var tan = Mathf.Tan(angle);
            var top = Mathf.Sqrt(distance) * Mathf.Sqrt(Mathf.Abs(gravity)) * Mathf.Sqrt(tan * tan + 1);
            var bottom = Mathf.Sqrt(-((2 * height) / distance - 2 * tan));

            return top / bottom;
        }

        private static bool getAngle(float height, float distance, float velocity, float gravity, ref float angle)
        {
            var sqrt = velocity * velocity * velocity * velocity - (gravity * (gravity * distance * distance - 2 * height * velocity * velocity));

            if (sqrt > 0)
            {
                sqrt = Mathf.Sqrt(sqrt);
                angle = Mathf.Atan((velocity * velocity + sqrt) / (-gravity * distance));
                return true;
            }
            else
                return false;
        }
        public static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;

            return p;
        }
    }
}
