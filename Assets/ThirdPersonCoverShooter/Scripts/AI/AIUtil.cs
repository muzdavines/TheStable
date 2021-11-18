using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    /// <summary>
    /// A number of utility functions related to the AI.
    /// </summary>
    public static class AIUtil
    {
        /// <summary>
        /// Actor array filled by some of the methods.
        /// </summary>
        public static Actor[] Actors = new Actor[128];

        private static Collider[] _colliders = new Collider[512];

        /// <summary>
        /// Finds all actors near the given position in a given radius. Includes actors that are dead. Returns number of them and fills the Actors array with the results.
        /// </summary>
        public static int FindActorsIncludingDead(Vector3 position, float radius, Actor ignore = null)
        {
            return FindActors(position, radius, false, ignore);
        }

        /// <summary>
        /// Finds all actors near the given position in a given radius. Returns number of them and fills the Actors array with the results.
        /// </summary>
        public static int FindActors(Vector3 position, float radius, Actor ignore = null)
        {
            return FindActors(position, radius, true, ignore);
        }

        /// <summary>
        /// Finds all actors near the given position in a given radius. Returns number of them and fills the Actors array with the results.
        /// </summary>
        public static int FindActors(Vector3 position, float radius, bool ignoreDead, Actor ignore = null)
        {
            int count = 0;
            var physicsCount = Physics.OverlapSphereNonAlloc(position, radius, _colliders, Layers.Character);

            for (int i = 0; i < physicsCount; i++)
            {
                if (ignore != null && _colliders[i].gameObject == ignore.gameObject)
                    continue;

                var actor = CoverShooter.Actors.Get(_colliders[i].gameObject);

                if (actor != null && (!ignoreDead || actor.IsAlive))
                {
                    if (count < Actors.Length)
                        Actors[count++] = actor;
                    else
                        return count;
                }
            }

            return count;
        }

        /// <summary>
        /// Finds a closest actor to the given position in a given radius. Can include dead actors.
        /// </summary>
        public static Actor FindClosestActorIncludingDead(Vector3 position, float radius, Actor ignore = null)
        {
            return FindClosestActor(position, radius, false, ignore);
        }

        /// <summary>
        /// Finds a closest actor to the given position in a given radius.
        /// </summary>
        public static Actor FindClosestActor(Vector3 position, float radius, Actor ignore = null)
        {
            return FindClosestActor(position, radius, true, ignore);
        }

        /// <summary>
        /// Finds a closest actor to the given position in a given radius.
        /// </summary>
        public static Actor FindClosestActor(Vector3 position, float radius, bool ignoreDead, Actor ignore = null)
        {
            Actor result = null;
            float minDist = 0;

            var physicsCount = Physics.OverlapSphereNonAlloc(position, radius, _colliders, Layers.Character);

            for (int i = 0; i < physicsCount; i++)
            {
                if (ignore != null && _colliders[i].gameObject == ignore.gameObject)
                    continue;

                var actor = _colliders[i].GetComponent<Actor>();

                if (actor != null && (!ignoreDead || actor.IsAlive))
                {
                    var dist = Vector3.Distance(actor.transform.position, position);

                    if (result == null || dist < minDist)
                    {
                        result = actor;
                        minDist = dist;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if a ray cannot be traced on a navmesh without hiting anything.
        /// </summary>
        public static bool IsNavigationBlocked(Vector3 origin, Vector3 target)
        {
            NavMeshHit hit;
            return NavMesh.Raycast(origin, target, out hit, NavMesh.AllAreas);
        }

        /// <summary>
        /// Modifies the position to the closed on a nav mesh. Returns true if any were found.
        /// </summary>
        public static bool GetClosestStandablePosition(ref Vector3 position)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(position, out hit, 3, NavMesh.AllAreas))
            {
                position = hit.position;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Returns true if the given position is on a nav mesh.
        /// </summary>
        public static bool IsPositionOnNavMesh(Vector3 position)
        {
            NavMeshHit hit;
            return NavMesh.SamplePosition(position, out hit, 0.2f, NavMesh.AllAreas);
        }

        /// <summary>
        /// Calculates a path from the source to target.
        /// </summary>
        public static void Path(ref NavMeshPath path, Vector3 source, Vector3 target)
        {
            if (path == null)
                path = new NavMeshPath();

            GetClosestStandablePosition(ref source);
            GetClosestStandablePosition(ref target);

            NavMesh.CalculatePath(source, target, NavMesh.AllAreas, path);
        }

        /// <summary>
        /// Returns true if a given position is in sight.
        /// </summary>
        public static bool IsInSight(Actor actor, Vector3 target, float maxDistance, float fieldOfView, float obstacleObstructionDistance = 1)
        {
            var motorTop = actor.StandingTopPosition;
            var vector = target - motorTop;

            if (vector.magnitude > maxDistance)
                return false;

            vector.y = 0;

            var angle = Mathf.Abs(Mathf.DeltaAngle(0, Mathf.Acos(Vector3.Dot(vector.normalized, actor.HeadDirection)) * Mathf.Rad2Deg));
            if (angle > fieldOfView * 0.5f)
                return false;

            return vector.magnitude < obstacleObstructionDistance || !IsObstructed(motorTop, target);
        }

        /// <summary>
        /// Returns true if there is no unobstructed line between the given origin and the target.
        /// </summary>        
        public static bool IsObstructed(Vector3 origin, Vector3 target)
        {
            RaycastHit hit;
            if (Physics.Raycast(origin, (target - origin).normalized, out hit, Vector3.Distance(origin, target), Layers.Geometry, QueryTriggerInteraction.Ignore))
            {
                if (Vector3.Distance(hit.point, target) < 0.5f)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Returns true if the given position on the cover protects the character from the enemy.
        /// </summary>
        public static bool IsGoodAngle(float maxTallAngle, float maxLowAngle, Cover cover, Vector3 a, Vector3 b, bool isTall)
        {
            var dot = Vector3.Dot((b - a).normalized, cover.Forward);

            if (isTall)
            {
                if (Mathf.DeltaAngle(0, Mathf.Acos(dot) * Mathf.Rad2Deg) > maxTallAngle)
                    return false;
            }
            else
            {
                if (Mathf.DeltaAngle(0, Mathf.Acos(dot) * Mathf.Rad2Deg) > maxLowAngle)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the given position is already taken by a friend that's close enough to communicate.
        /// </summary>
        public static bool IsCoverPositionFree(Cover cover, Vector3 position, float threshold, Actor newcomer)
        {
            if (!IsJustThisCoverPositionFree(cover, position, threshold, newcomer))
                return false;

            if (cover.LeftAdjacent != null && !IsJustThisCoverPositionFree(cover.LeftAdjacent, position, threshold, newcomer))
                return false;

            if (cover.RightAdjacent != null && !IsJustThisCoverPositionFree(cover.RightAdjacent, position, threshold, newcomer))
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if the given position is free for taking.
        /// </summary>
        public static bool IsJustThisCoverPositionFree(Cover cover, Vector3 position, float threshold, Actor newcomer)
        {
            foreach (var user in cover.Users)
                if (user.Actor != newcomer && Vector3.Distance(user.Position, position) <= threshold)
                    return false;

            return true;
        }
    }
}
