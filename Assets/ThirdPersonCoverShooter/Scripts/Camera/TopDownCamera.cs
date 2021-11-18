using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Strategical top down camera that follows characters and adjusts it's height based on the situation.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class TopDownCamera : CharacterCamera
    {
        /// <summary>
        /// Forward/up direction for the character.
        /// </summary>
        public Vector3 Forward { get { return new Vector3(-_offset.x, 0, -_offset.z).normalized; } }

        /// <summary>
        /// Right direction for the character.
        /// </summary>
        public Vector3 Right { get { return Vector3.Cross(Vector3.up, Forward); } }

        /// <summary>
        /// Target character motor.
        /// </summary>
        [Tooltip("Target character motor.")]
        public CharacterMotor Target;

        /// <summary>
        /// Camera offset from the target character when there are no enemies around.
        /// </summary>
        [Tooltip("Camera offset from the target character when there are no enemies around.")]
        public Vector3 CalmOffset = new Vector3(-3, 9, -3);

        /// <summary>
        /// Camera offset from the target character when there are enemies around.
        /// </summary>
        [Tooltip("Camera offset from the target character when there are enemies around.")]
        public Vector3 DangerOffset = new Vector3(-4, 14, -4);

        /// <summary>
        /// Speed to move between different offsets.
        /// </summary>
        [Tooltip("Speed to move between different offsets.")]
        public float OffsetSpeed = 2f;

        /// <summary>
        /// Time in seconds to go back to the calm offset after there are no more enemies around.
        /// </summary>
        [Tooltip("Time in seconds to go back to the calm offset after there are no more enemies around.")]
        public float ZoomDelay = 0.5f;

        /// <summary>
        /// Field of view.
        /// </summary>
        [Tooltip("Field of view.")]
        public float FOV = 45;

        /// <summary>
        /// Time in seconds it takes for the camera to move between targets.
        /// </summary>
        [Tooltip("Time in seconds it takes for the camera to move between targets.")]
        public float Switch = 0.5f;

        /// <summary>
        /// Min and max enemy distance. Enemies outside of the range are not considered when zooming.
        /// </summary>
        [Tooltip("Min and max enemy distance. Enemies outside of the range are not considered when zooming.")]
        public EnemyDistanceRange EnemyDistances = new EnemyDistanceRange(5, 10);

        /// <summary>
        /// Target offsets for each character direction. Targets are relative to the character's position.
        /// </summary>
        [Tooltip("Target offsets for each character direction. Targets are relative to the character's position.")]
        public MobileCameraTargetOffsets TargetOffsets = new MobileCameraTargetOffsets(new Vector3(1, 0, 1),
                                                                                       new Vector3(-1, 0, -1),
                                                                                       new Vector3(-1, 0, 1),
                                                                                       new Vector3(1, 0, -1));

        private Vector3 _motorPosition;
        private float _motorPivotIntensity = 1;
        private bool _wasInCover;
        private Vector3 _lookVector = new Vector3(0, 0, 1);

        private Vector3 _offset;
        private Vector3 _targetOffset;
        private float _offsetScale = 0;

        private float _zoom;

        private Camera _camera;

        private float _targetTravel;
        private CharacterMotor _lastTarget;
        private Vector3 _lastPosition;

        private float[] _zoomMultipliers = new float[] { 1, 1.5f, 2, 2.5f };
        private int _zoomIndex;

        private CharacterMotor _cachedTarget;
        private MonoBehaviour _targetComponent;

        private ICharacterController _deferredController;

        private bool _hasAskedForLateUpdate;

        public T TargetComponent<T>() where T : MonoBehaviour
        {
            if (_cachedTarget != Target || _targetComponent == null || !(_targetComponent is T))
            {
                _cachedTarget = Target;
                _targetComponent = Target != null ? Target.GetComponent<T>() : null;
            }

            return _targetComponent as T;
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _lastPosition = transform.position;
            _targetTravel = 1;
            _lastTarget = Target;
        }

        public void SetTargetImmediately(CharacterMotor value)
        {
            _lastPosition = transform.position;
            _targetTravel = 1;
            _lastTarget = value;
            Target = value;
        }

        private void updateZoom()
        {
            var scroll = 0f;

            if (Input.mousePresent)
                scroll = -Input.GetAxisRaw("Mouse ScrollWheel");
            else if (Input.touchSupported)
                scroll = Util.GetPinch();

            if (scroll < 0) _zoomIndex -= 1;
            else if (scroll > 0) _zoomIndex += 1;

            _zoomIndex = Mathf.Clamp(_zoomIndex, 0, _zoomMultipliers.Length - 1);
        }

        private void Update()
        {
            if (Target)
            {
                _hasAskedForLateUpdate = true;
                Target.AskForLateUpdate(this);
            }
            else
                _hasAskedForLateUpdate = false;
        }

        private void LateUpdate()
        {
            if (!_hasAskedForLateUpdate)
                UpdateForCharacterMotor();
        }

        /// <summary>
        /// Asks the camera to call UpdateAfterCamera on the given controller after the camera does it's update.
        /// </summary>
        public override void DeferUpdate(ICharacterController controller)
        {
            _deferredController = controller;
        }

        /// <summary>
        /// Update performed after the character motor does it's thing.
        /// </summary>
        public override void UpdateForCharacterMotor()
        {
            if (Target == null)
                return;

            if (Target != _lastTarget)
            {
                _lastPosition = transform.position;
                _targetTravel = 0;
                _lastTarget = Target;
            }

            updateZoom();

            if (Target.IsInCover && !Target.IsAiming)
                _lookVector = Target.Cover.Forward;
            else
                _lookVector = Target.transform.forward;

            Util.Lerp(ref _offset, perfectOffset * _zoomMultipliers[_zoomIndex], OffsetSpeed);
            Util.Lerp(ref _targetOffset, perfectTargetOffset, OffsetSpeed);

            var shouldGoUp = false;
            var actor = Target.GetComponent<Actor>();

            if (actor != null)
                foreach (var character in Characters.AllAlive)
                    if (character.Actor != null && character.Actor.Side != actor.Side)
                    {
                        var dist = (character.Object.transform.position - Target.transform.position).magnitude;

                        if (dist > EnemyDistances.Min && dist < EnemyDistances.Max)
                            shouldGoUp = true;
                    }

            if (shouldGoUp)
                _zoom = 1;
            else
                _zoom = Mathf.Clamp(_zoom - Time.deltaTime / ZoomDelay, 0, 1);

            if (_zoom > float.Epsilon)
                _offsetScale = Mathf.Clamp(_offsetScale + Time.deltaTime * OffsetSpeed, 0, 1);
            else
                _offsetScale = Mathf.Clamp(_offsetScale - Time.deltaTime * OffsetSpeed, 0, 1);

            var newPosition = Target.transform.position;

            Util.Lerp(ref _motorPivotIntensity, 1, 5);

            if (Target.IsInCover != _wasInCover)
                _motorPivotIntensity = 0;

            _motorPosition = Vector3.Lerp(_motorPosition, newPosition, _motorPivotIntensity);

            var t = _targetTravel;
            t = t * t * (3 - 2 * t);

            _targetTravel = Mathf.Clamp01(_targetTravel + Time.deltaTime / Switch);

            var nextCameraPosition = _motorPosition + _offset;
            var nextCameraTarget = _motorPosition + _targetOffset;

            var cameraPosition = Vector3.Lerp(_lastPosition, nextCameraPosition, t);
            var cameraTarget = cameraPosition + (nextCameraTarget - nextCameraPosition);

            transform.position = cameraPosition;
            transform.LookAt(cameraTarget);
            SendMessage("OnFadeTarget", new FadeTarget(Target.gameObject, cameraTarget), SendMessageOptions.DontRequireReceiver);

            _camera.fieldOfView = Util.Lerp(_camera.fieldOfView, FOV, 6);

            _wasInCover = Target.IsInCover;

            if (_deferredController != null)
            {
                var controller = _deferredController;
                _deferredController = null;
                controller.UpdateAfterCamera();
            }
        }

        private Vector3 perfectOffset
        {
            get
            {
                return Vector3.Lerp(CalmOffset, DangerOffset, _offsetScale);
            }
        }

        private Vector3 perfectTargetOffset
        {
            get
            {
                return TargetOffsets.Up * Mathf.Clamp01(Vector3.Dot(Forward, _lookVector)) +
                       TargetOffsets.Down * Mathf.Clamp01(Vector3.Dot(-Forward, _lookVector)) +
                       TargetOffsets.Right * Mathf.Clamp01(Vector3.Dot(Right, _lookVector)) +
                       TargetOffsets.Left * Mathf.Clamp01(Vector3.Dot(-Right, _lookVector));
            }
        }
    }
}
