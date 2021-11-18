using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Describes an actor and its position.
    /// </summary>
    public struct CoverUser
    {
        /// <summary>
        /// Actor that has taken a cover.
        /// </summary>
        public Actor Actor;

        /// <summary>
        /// Position the actor is supposedly at.
        /// </summary>
        public Vector3 Position;
    }

    /// <summary>
    /// Types of cover climbing
    /// </summary>
    public enum CoverClimb
    {
        No,
        Climb,
        Vault
    }

    /// <summary>
    /// Walls usable for taking covers have to be marked by cover markers. A cover marker is any gameobject with Cover and Box Collider components attached. Markers can intersect, form a chain which is interpreted as one big cover by character motors.
    /// Cover orientation matters, the example scene contains markers with feet that mark facing directions.
    /// There are two kinds of covers, low and tall. The kind is determined from the height of a BoxCollider attached to the marker. The height threshold is different for every character and is defined by a Character Motor. The character will correctly change its stance when moving between tall and low covers.
    /// A corner of a tall cover with no adjacent tall covers nearby is treated as a corner characters can peek from. However, there can be unmarked walls and therefore character can attempt to take a peek in impossible situations. Such cases are handled by Open Left and Open Right properties inside the Cover component, setting a value to false marks that corner as unusable for peeking.
    /// Covers can be climbed or vaulted over. Upon request, character motor will calculate if a climb is possible and perform it at the correct height. If the wall is low and thin enough to be jumped over, the character will perform a vault.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class Cover : MonoBehaviour
    {
        /// <summary>
        /// Minimal distance from characters feet to the top of a cover for it to be considered tall.
        /// </summary>
        public const float TallThreshold = 1.2f;

        /// <summary>
        /// Returns all AI users of the cover.
        /// </summary>
        public IEnumerable<CoverUser> Users
        {
            get { return _userMap.Values; }
        }

        /// <summary>
        /// Y coordinate of the cover top in world space.
        /// </summary>
        public float Top
        {
            get
            {
                if (!Application.isPlaying || _collider == null)
                    return GetComponent<BoxCollider>().bounds.max.y;
                else
                    return _collider.bounds.max.y;
            }
        }

        /// <summary>
        /// Y coordinate of the cover bottom in world space.
        /// </summary>
        public float Bottom
        {
            get
            {
                if (!Application.isPlaying || _collider == null)
                    return GetComponent<BoxCollider>().bounds.min.y;
                else
                    return _collider.bounds.min.y;
            }
        }

        /// <summary>
        /// Direction pointing towards the wall.
        /// </summary>
        public Vector3 Forward
        {
            get { return transform.forward; }
        }

        /// <summary>
        /// Direction pointing right from a character along the wall.
        /// </summary>
        public Vector3 Right
        {
            get { return transform.right; }
        }

        /// <summary>
        /// Direction pointing left from a character along the wall.
        /// </summary>
        public Vector3 Left
        {
            get { return -transform.right; }
        }

        /// <summary>
        /// Orientation of the cover in degrees in world space.
        /// </summary>
        public float Angle
        {
            get { return transform.eulerAngles.y; }
        }

        /// <summary>
        /// Width of the cover.
        /// </summary>
        public float Width
        {
            get
            {
                checkOrientationAndSize();
                return _size.x;
            }
        }

        /// <summary>
        /// Height of the cover.
        /// </summary>
        public float Height
        {
            get
            {
                checkOrientationAndSize();
                return _size.y;
            }
        }

        /// <summary>
        /// Depth of the cover.
        /// </summary>
        public float Depth
        {
            get
            {
                checkOrientationAndSize();
                return _size.z;
            }
        }

        /// <summary>
        /// Cover to the left.
        /// </summary>
        public Cover LeftAdjacent
        {
            get { return _leftAdjacent; }
        }

        /// <summary>
        /// Cover to the right.
        /// </summary>
        public Cover RightAdjacent
        {
            get { return _rightAdjacent; }
        }

        /// <summary>
        /// Can the character use the left corner of the cover.
        /// </summary>
        [Tooltip("Can the character use the left corner of the cover.")]
        public bool OpenLeft = true;

        /// <summary>
        /// Can the character use the rgiht corner of the cover.
        /// </summary>
        [Tooltip("Can the character use the rgiht corner of the cover.")]
        public bool OpenRight = true;

        /// <summary>
        /// Maximum allowed distance to adjacent covers.
        /// </summary>
        [Tooltip("Maximum allowed distance to adjacent covers.")]
        public float AdjacentDistance = 1;

        private Vector3 _size;
        private Cover _leftAdjacent;
        private Cover _rightAdjacent;
        private BoxCollider _collider;
        private bool _hasLeftCorner;
        private bool _hasRightCorner;
        private bool _hasOrientationAndSize;
        private Vector3 _leftCorner;
        private Vector3 _rightCorner;
        private Quaternion _orientation;
        private Quaternion _negativeOrientation;

        private Dictionary<Actor, CoverUser> _userMap = new Dictionary<Actor, CoverUser>();

        /// <summary>
        /// Is the given cover adjacent to the left at the given position.
        /// </summary>
        public bool IsLeftAdjacent(Cover cover, Vector3 position)
        {
            return isAdjacent(cover, position, -120, 60);
        }

        /// <summary>
        /// Is the given cover adjacent to the right at the given position.
        /// </summary>
        public bool IsRightAdjacent(Cover cover, Vector3 position)
        {
            return isAdjacent(cover, position, -60, 120);
        }

        private void Start()
        {
            if (_collider == null)
                _collider = GetComponent<BoxCollider>();

            if (_leftAdjacent == null)
            {
                _leftAdjacent = findAdjacentTo(LeftCorner(Bottom), -120, 60, false);

                if (_leftAdjacent != null)
                    _leftAdjacent._rightAdjacent = this;
            }

            if (_rightAdjacent == null)
            {
                _rightAdjacent = findAdjacentTo(RightCorner(Bottom), -60, 120, true);
            
                if (_rightAdjacent != null)
                    _rightAdjacent._leftAdjacent = this;
            }
        }

        /// <summary>
        /// Checks if the cover is adjacent in the given position.
        /// </summary>
        private bool isAdjacent(Cover other, Vector3 position, float minAngle, float maxAngle)
        {
            var closest = other.ClosestPointTo(position, 0, 0);
            var distance = Vector3.Distance(position, closest);

            if (distance > AdjacentDistance)
                return false;

            var closestAngle = other.Angle;
            var deltaAngle = Mathf.DeltaAngle(Angle, closestAngle);

            return deltaAngle >= minAngle && deltaAngle <= maxAngle;
        }

        /// <summary>
        /// Find an adjacent cover.
        /// </summary>
        private Cover findAdjacentTo(Vector3 point, float minAngle, float maxAngle, bool useLeftCorner)
        {
            float closestDistance = 0f;
            Cover closestCover = null;

            foreach (var other in GameObject.FindObjectsOfType<Cover>())
                if (other != this)
                {
                    var closest = useLeftCorner ? other.LeftCorner(point.y) : other.RightCorner(point.y);
                    var distance = Vector3.Distance(point, closest);

                    if (distance > AdjacentDistance)
                        continue;

                    var closestAngle = other.Angle;
                    var deltaAngle = Mathf.DeltaAngle(Angle, closestAngle);

                    if (deltaAngle >= minAngle && deltaAngle <= maxAngle)
                        if (closestCover == null || distance < closestDistance)
                        {
                            closestCover = other;
                            closestDistance = distance;
                        }
                }

            return closestCover;
        }

        /// <summary>
        /// Returns the type of possible climb at the given position.
        /// </summary>
        public CoverClimb GetClimbAt(Vector3 position, float radius, float maxClimbHeight, float maxVaultHeight, float maxVaultDistance)
        {
            var left = LeftCorner(position.y);
            var right = RightCorner(position.y);
            var x = Vector3.Dot(Right, position - left) / Width;

            if ((x < 0 && _leftAdjacent == null) ||
                (x > 1 && _rightAdjacent == null))
                return CoverClimb.No;

            var space = radius / Width;

            if ((Height > maxClimbHeight && Height > maxVaultHeight) ||
                checkForward(x - space) ||
                checkForward(x) ||
                checkForward(x + space) ||
                checkUp(x - space, -0.2f, -0.2f) ||
                checkUp(x, -0.2f, -0.2f) ||
                checkUp(x + space, -0.2f, -0.2f) ||
                checkUp(x - space, 0.1f, 0.3f) ||
                checkUp(x, 0.1f, 0.3f) ||
                checkUp(x + space, 0.1f, 0.3f))
                return CoverClimb.No;
            else if (Height < maxVaultHeight &&
                     !checkDown(x - space, maxVaultDistance) &&
                     !checkDown(x, maxVaultDistance) &&
                     !checkDown(x + space, maxVaultDistance))
                return CoverClimb.Vault;
            else
                return CoverClimb.Climb;
        }

        /// <summary>
        /// Adds an AI as a user to the cover.
        /// </summary>
        public void RegisterUser(Actor actor, Vector3 position)
        {
            CoverUser user;
            user.Actor = actor;
            user.Position = position;

            _userMap[actor] = user;
        }

        /// <summary>
        /// Removes an AI from the cover users.
        /// </summary>
        public void UnregisterUser(Actor actor)
        {
            if (_userMap.ContainsKey(actor))
                _userMap.Remove(actor);
        }

        /// <summary>
        /// Returns whether the cover is considered for an observer standing at the same level as the cover.
        /// </summary>
        public bool IsTall
        {
            get
            {
                return (Top - Bottom) > TallThreshold;
            }
        }

        /// <summary>
        /// Return whether the cover is considered for an observer with given y coordinate.
        /// </summary>
        public bool CheckTall(float observer)
        {
            return (Top - observer) > TallThreshold;
        }

        /// <summary>
        /// Returns true if the given position is in front of the cover.
        /// </summary>
        /// <param name="isOld">Old positions use lesser thresholds.</param>
        public bool IsInFront(Vector3 observer, bool isOld)
        {
            var closest = ClosestPointTo(observer, 0, 0);
            var vector = (closest - observer).normalized;
            var dot = Vector3.Dot(vector, Forward);

            if (isOld)
                return dot >= 0.85f;
            else
                return dot >= 0.95f;
        }

        /// <summary>
        /// Returns the position of the left corner with the given height coordinate.
        /// </summary>
        public Vector3 LeftCorner(float y, float offset = 0)
        {
            var point = _leftCorner;

            if (!_hasLeftCorner)
            {
                _leftCorner = point = ClosestPointTo(transform.position + Left * 999, 0, 0);
                _hasLeftCorner = Application.isPlaying;
            }

            point += Left * offset;
            point.y = y;
            return point;
        }

        /// <summary>
        /// Returns the position of the right corner with the given height coordinate.
        /// </summary>
        public Vector3 RightCorner(float y, float offset = 0)
        {
            var point = _rightCorner;

            if (!_hasRightCorner)
            {
                _rightCorner = point = ClosestPointTo(transform.position + Right * 999, 0, 0);
                _hasRightCorner = Application.isPlaying;
            }

            point += Right * offset;
            point.y = y;
            return point;
        }

        /// <summary>
        /// Returns true if the given position is within the given distance of the left corner.
        /// </summary>
        public bool IsByLeftCorner(Vector3 position, float distance)
        {
            return Vector3.Distance(LeftCorner(position.y), position) <= distance;
        }

        /// <summary>
        /// Returns true if the given position is within the given distance of the right corner.
        /// </summary>
        public bool IsByRightCorner(Vector3 position, float distance)
        {
            return Vector3.Distance(RightCorner(position.y), position) <= distance;
        }

        /// <summary>
        /// Returns the cloest corner to a segment.
        /// </summary>
        public int ClosestCornerToSegment(Vector3 a, Vector3 b, float radius, out Vector3 position)
        {
            var left = LeftCorner(0, -radius);
            var right = RightCorner(0, -radius);

            var distLeft = Util.DistanceToSegment(left, a, b);
            var distRight = Util.DistanceToSegment(right, a, b);

            if (distLeft < distRight)
            {
                position = left;
                return -1;
            }
            else
            {
                position = right;
                return 1;
            }
        }

        /// <summary>
        /// Returns the closest corner useful for aiming.
        /// </summary>
        public int ClosestCornerTo(Vector3 point, float radius, out Vector3 position)
        {
            var left = LeftCorner(0, -radius);
            var right = RightCorner(0, -radius);

            var distLeft = Vector3.Distance(left, point);
            var distRight = Vector3.Distance(right, point);

            if (distLeft < distRight)
            {
                position = left;
                return -1;
            }
            else
            {
                position = right;
                return 1;
            }
        }

        private void checkOrientationAndSize()
        {
            if (Application.isPlaying && _collider == null)
                _collider = GetComponent<BoxCollider>();

            var collider = Application.isPlaying ? _collider : GetComponent<BoxCollider>();

            if (!_hasOrientationAndSize)
            {
                _orientation = Quaternion.Euler(0, -transform.eulerAngles.y, 0);
                _negativeOrientation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

                _size.x = 1.0f / transform.InverseTransformVector(_negativeOrientation * Vector3.right).magnitude;
                _size.y = collider.bounds.size.y;
                _size.z = 1.0f / transform.InverseTransformVector(_negativeOrientation * Vector3.forward).magnitude;

                _hasOrientationAndSize = Application.isPlaying;
            }
        }

        /// <summary>
        /// Returns a position on a cover closest to the given point.
        /// </summary>
        public Vector3 ClosestPointTo(Vector3 point, float sideRadius, float frontRadius)
        {
            checkOrientationAndSize();

            var hw = _size.x * 0.5f;
            var hd = _size.z * 0.5f;

            var local = _orientation * (point - transform.position);
            var left = new Vector3(-hw, local.y, -hd);
            var right = new Vector3(hw, local.y, -hd);
            var leftToRight = (right - left).normalized;
            left += leftToRight * sideRadius;
            right -= leftToRight * sideRadius;

            local = Util.FindClosestToPath(left, right, local);

            var result = _negativeOrientation * local + transform.position - Forward * frontRadius;

            return result;
        }

        /// <summary>
        /// Returns a position on a cover closest to the given point.
        /// </summary>
        public Vector3 ClosestPointTo(Vector3 point, float leftSideRadius, float rightSideRadius, float frontRadius)
        {
            checkOrientationAndSize();

            var hw = _size.x * 0.5f;
            var hd = _size.z * 0.5f;

            var local = _orientation * (point - transform.position);
            var left = new Vector3(-hw, local.y, -hd);
            var right = new Vector3(hw, local.y, -hd);
            var leftToRight = (right - left).normalized;
            left += leftToRight * leftSideRadius;
            right -= leftToRight * rightSideRadius;

            local = Util.FindClosestToPath(left, right, local);

            var result = _negativeOrientation * local + transform.position - Forward * frontRadius;

            return result;
        }

        /// <summary>
        /// Returns true if a given angle is pointing towards the front of the cover.
        /// </summary>
        /// <param name="angle">Degrees in world space.</param>
        /// <param name="field">Field of the front. 180 equals half a circle.</param>
        public bool IsFrontField(float angle, float field)
        {
            return IsFront(angle, (180 - field) / 2);
        }

        /// <summary>
        /// Returns true if a given angle is pointing towards the front of the cover.
        /// </summary>
        /// <param name="angle">Degrees in world space.</param>
        /// <param name="margin">Reduction in each side of the front arc of the cover.</param>
        public bool IsFront(float angle, float margin = 0)
        {
            float delta = Mathf.DeltaAngle(angle, Angle);

            return delta >= (-90 + margin) && delta <= (90 - margin);
        }

        /// <summary>
        /// Returns true if a given angle is pointing backwards from the cover.
        /// </summary>
        /// <param name="angle">Degrees in world space.</param>
        /// <param name="margin">Reduction in each side of the back arc of the cover.</param>
        public bool IsBack(float angle, float margin = 0)
        {
            float delta = Mathf.DeltaAngle(angle, Angle);

            return delta <= (-90 - margin) || delta >= (90 + margin);
        }

        /// <summary>
        /// Returns true if a given angle is pointing left of the cover.
        /// </summary>
        public bool IsLeft(float angle, float margin = 0)
        {
            float delta = Mathf.DeltaAngle(angle, Angle);

            return delta >= margin && delta <= (180 - margin);
        }

        /// <summary>
        /// Returns true if a given angle is pointing left of the cover.
        /// </summary>
        public bool IsLeftField(float angle, float field)
        {
            return IsLeft(angle, (180 - field) / 2);
        }

        /// <summary>
        /// Returns true if a given angle is pointing right of the cover.
        /// </summary>
        public bool IsRight(float angle, float margin = 0)
        {
            float delta = Mathf.DeltaAngle(angle, Angle);

            return delta >= (-180 + margin) && delta <= -margin;
        }

        /// <summary>
        /// Returns true if a given angle is pointing right of the cover.
        /// </summary>
        public bool IsRightField(float angle, float field)
        {
            return IsRight(angle, (180 - field) / 2);
        }

        private bool checkForward(float x)
        {
            return checkRay(LeftCorner(Top + 0.1f, -Width * x), Forward, 0.5f);
        }

        private bool checkDown(float x, float distance)
        {
            return checkRay(LeftCorner(Top + 0.1f, -Width * x) + Forward * distance, Vector3.down, 0.5f);
        }

        private bool checkUp(float x, float yoffset, float distance)
        {
            return checkRay(LeftCorner(Top + yoffset, -Width * x) + Forward * distance, Vector3.up, 2.0f);
        }

        private bool checkRay(Vector3 position, Vector3 direction, float distance)
        {
            return checkLine(position, position + direction * distance);
        }

        private bool checkLine(Vector3 position, Vector3 end)
        {
            if (Physics.Raycast(position, (end - position).normalized, Vector3.Distance(end, position), Layers.Geometry, QueryTriggerInteraction.Ignore))
                return true;

            return false;
        }
    }
}