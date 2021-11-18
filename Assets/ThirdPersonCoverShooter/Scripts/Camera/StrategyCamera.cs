using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Simple camera that looks and follows at characters from above.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class StrategyCamera : BaseCamera
    {
        /// <summary>
        /// Should the camera always stay on top of the target.
        /// </summary>
        [Tooltip("Should the camera always stay on top of the target.")]
        public bool FollowTarget = false;

        /// <summary>
        /// Field of view.
        /// </summary>
        [Tooltip("Field of view.")]
        public float FOV = 45;

        /// <summary>
        /// Speed at which the camera moves.
        /// </summary>
        [Tooltip("Speed at which the camera moves.")]
        public float Speed = 10;

        /// <summary>
        /// Speed at which the camera accelerates.
        /// </summary>
        [Tooltip("Speed at which the camera accelerates.")]
        public float Acceleration = 10;

        /// <summary>
        /// Time in seconds it takes for the camera to transition between targets.
        /// </summary>
        [Tooltip("Time in seconds it takes for the camera to transition between targets.")]
        public float TargetTransitionDuration = 1;

        /// <summary>
        /// Min height the camera can reach.
        /// </summary>
        [Tooltip("Min height the camera can reach.")]
        public float MinHeight = 5;

        /// <summary>
        /// Max height the camera can reach.
        /// </summary>
        [Tooltip("Max height the camera can reach.")]
        public float MaxHeight = 15;

        public float HeightSpeed = 1;

        private CharacterMotor _lastTarget;
        private float _targetTravel;
        private Vector3 _travelStart;

        private Camera _camera;
        private Vector3 _velocity;
        private float _heightOffset;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        protected override void Update()
        {
            base.Update();

            _camera.fieldOfView = Util.Lerp(_camera.fieldOfView, FOV, 6);

            var forwardVector = transform.forward;
            forwardVector.y = 0;

            if (forwardVector.magnitude < float.Epsilon)
                forwardVector = Vector3.forward;
            else
                forwardVector.Normalize();

            var rightVector = Vector3.Cross(Vector3.up, forwardVector);

            var forwardSpeed = Vector3.Dot(_velocity, forwardVector);
            var rightSpeed = Vector3.Dot(_velocity, rightVector);

            var forwardTarget = 0f;
            var rightTarget = 0f;

            if (_lastTarget != Target)
            {
                _lastTarget = Target;
                _travelStart = transform.position;
                _targetTravel = 0;
            }

            if (FollowTarget && Target != null)
            {
                var targetPosition = Target.transform.position;
                targetPosition.y += 2;

                var diff = transform.position.y - Target.transform.position.y;
                targetPosition.y += diff;
                targetPosition -= forwardVector * diff;

                if (_targetTravel < 1 - float.Epsilon)
                {
                    var t = _targetTravel;
                    t = t * t * (3 - 2 * t);

                    targetPosition = _travelStart + (targetPosition - _travelStart) * t;
                    _targetTravel = Mathf.Clamp01(_targetTravel + Time.deltaTime / TargetTransitionDuration);
                }

                var move = (targetPosition - transform.position) / Time.deltaTime;

                forwardSpeed = Vector3.Dot(move, forwardVector);
                rightSpeed = Vector3.Dot(move, rightVector);
            }
            else
            {
                _targetTravel = 0;
                _travelStart = transform.position;

                if (Input.GetKey(KeyCode.W)) forwardTarget = 1;
                if (Input.GetKey(KeyCode.S)) forwardTarget = -1;
                if (Input.GetKey(KeyCode.D)) rightTarget = 1;
                if (Input.GetKey(KeyCode.A)) rightTarget = -1;

                Util.Lerp(ref forwardSpeed, forwardTarget * Speed, Acceleration);
                Util.Lerp(ref rightSpeed, rightTarget * Speed, Acceleration);
            }

            _velocity = forwardVector * forwardSpeed + rightVector * rightSpeed;
            transform.position += _velocity * Time.deltaTime;

            _heightOffset = Mathf.Clamp(transform.position.y + _heightOffset - Input.mouseScrollDelta.y * HeightSpeed * 0.5f, MinHeight, MaxHeight) - transform.position.y;

            {
                var move = Mathf.Clamp(Time.deltaTime * 10, 0, Mathf.Abs(_heightOffset)) * Mathf.Sign(_heightOffset);

                transform.position += Vector3.up * move;
                transform.position -= forwardVector * move;
                _heightOffset -= move;
            }
        }
    }
}
