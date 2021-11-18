using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Manages the camera object by setting an appropriate orientation depending on the target object's state. For camera to work you have to link it to the target object that has a Character Motor attached.
    /// The camera component also maintains and draws a crosshair. It hides the crosshair if the character is unarmed or unable to fire at a wall because is too close. The visibility of crosshair also can be turned off manually by setting Is Crosshair Enabled value to false when your game needs so.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ThirdPersonCamera : BaseCamera
    {
        /// <summary>
        /// Field of view as defined by the current camera state.
        /// </summary>
        public float StateFOV
        {
            get { return _stateFOV; }
        }

        /// <summary>
        /// Current state name.
        /// </summary>
        public string State
        {
            get { return _stateName; }
        }

        /// <summary>
        /// Alpha transparency of a crosshair that's drawn for this camera. Camera itself doesn't draw it.
        /// </summary>
        public float CrosshairAlpha
        {
            get { return _crosshairAlpha; }
        }

        /// <summary>
        /// Current maximum shake angle offset. Used for crosshair size.
        /// </summary>
        public float ShakeOffset
        {
            get
            {
                var x = Mathf.Abs(_shakeTarget.x);
                var y = Mathf.Abs(_shakeTarget.y);

                if (x > y)
                    return x;
                else
                    return y;
            }
        }

        /// <summary>
        /// Is the camera adjusting itself so there are no colliders between it and the target.
        /// </summary>
        [Tooltip("Is the camera adjusting itself so there are no colliders between it and the target.")]
        public bool AvoidObstacles = true;

        /// <summary>
        /// Should the AvoidObstacles be ignored when the character is zooming in.
        /// </summary>
        [Tooltip("Should the AvoidObstacles be ignored when the character is zooming in.")]
        public bool IgnoreObstaclesWhenZooming = true;

        /// <summary>
        /// Reduction in field of view when zooming in without a scope.
        /// </summary>
        [Tooltip("Reduction in field of view when zooming in without a scope.")]
        public float Zoom = 10;

        /// <summary>
        /// Does camera shake affect aiming.
        /// </summary>
        [Tooltip("Does camera shake affect aiming.")]
        public bool ShakingAffectsAim = true;

        /// <summary>
        /// Should the camera ask for smoother rotation animations when zooming in.
        /// </summary>
        [Tooltip("Should the camera ask for smoother rotation animations when zooming in.")]
        public bool AskForSmoothRotations = true;

        /// <summary>
        /// Camera settings for all gameplay situations.
        /// </summary>
        [Tooltip("Camera settings for all gameplay situations.")]
        public CameraStates States = CameraStates.GetDefault();

        /// <summary>
        /// Horizontal orientation of the camera in degrees.
        /// </summary>
        [HideInInspector]
        public float Horizontal;

        /// <summary>
        /// Vertical orientation of the camera in degrees.
        /// </summary>
        [HideInInspector]
        public float Vertical;

        /// <summary>
        /// Vertical recoil offset in degrees.
        /// </summary>
        [HideInInspector]
        public float VerticalRecoil;

        /// <summary>
        /// Horizontal recoil offset in degrees.
        /// </summary>
        [HideInInspector]
        public float HorizontalRecoil;

        private Vector3 _pivot;
        private Vector3 _offset;
        private float _horizontalDifference;
        private float _verticalDifference;

        private Vector3 _orientation;
        private float _crosshairAlpha;

        private Vector3 _motorPosition;
        private Quaternion _motorRotation;
        private Vector3 _motorOffset;
        private float _motorOffsetIntensity;

        private Vector3 _obstacleFix;

        private float _lastTargetDistance;

        private Camera _camera;

        private ThirdPersonController _controller;
        private CharacterMotor _cachedMotor;

        private float _stateFOV;

        private bool _hasAskedForLateUpdate;

        private bool _wasScoped;
        private bool _wasAiming;

        private CameraObjectFader _fader;

        private Vector3 _currentPivot;
        private Vector3 _currentOffset;

        private bool _isByCorner;
        private float _cornerDelay;

        private bool _isMirrored;

        private bool _couldCornerAim;

        private string _stateName;

        private float _shakeTime;
        private float _shakeDuration;
        private float _shakeIntensity;
        private Vector3 _shake;
        private Vector3 _shakeTarget;

        /// <summary>
        /// Shakes the main camera due to an explosion at a position.
        /// </summary>
        public static void Shake(Vector3 position, float intensity, float time)
        {
            var main = Camera.main;
            if (main == null) return;

            var camera = main.GetComponent<ThirdPersonCamera>();
            if (camera == null) return;

            var distance = Vector3.Distance(camera.transform.position, position);

            if (distance < 0.1f)
                distance = 0.1f;

            distance *= 10;

            intensity *= 100f / (distance * distance);

            if (intensity > 0.01f)
                camera.Shake(intensity, time);
        }

        /// <summary>
        /// Shakes the main camera in certain intensity for a certain time. Only if it is the ThirdPersonCamera with the same character motor.
        /// </summary>
        public static void Shake(CharacterMotor motor, float intensity, float time)
        {
            var main = Camera.main;
            if (main == null) return;

            var camera = main.GetComponent<ThirdPersonCamera>();
            if (camera == null) return;

            if (camera.Target == motor)
                camera.Shake(intensity, time);
        }

        /// <summary>
        /// Shakes the camera in certain intensity for a certain time.
        /// </summary>
        public void Shake(float intensity, float time)
        {
            _shakeTime = time;
            _shakeDuration = time;
            _shakeIntensity = intensity;
            _shakeTarget = Vector3.zero;
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _offset = States.Default.Offset;

            // Required for the explosion preview.
            _camera.depthTextureMode = DepthTextureMode.Depth;
            _stateFOV = States.Default.FOV;

            _fader = GetComponent<CameraObjectFader>();
        }

        /// <summary>
        /// Convenient way to get the controller.
        /// </summary>
        private ThirdPersonController getCurrentController()
        {
            if (_cachedMotor != Target)
            {
                _cachedMotor = Target;

                if (_cachedMotor == null)
                    _controller = null;
                else
                    _controller = _cachedMotor.GetComponent<ThirdPersonController>();
            }

            return _controller;
        }

        private void calculatePositionAndTarget(float horizontal, float vertical, Vector3 pivot, Vector3 offset, out Vector3 position, out Vector3 target)
        {
            var rotation = Quaternion.Euler(vertical, horizontal, 0);
            var transformPivot = _motorRotation * pivot + _motorPosition;

            position = transformPivot + rotation * offset;
            target = _motorPosition + pivot + rotation * Vector3.forward * 10;
        }

        /// <summary>
        /// Calculates target position to be aimed at in the world.
        /// </summary>
        public Vector3 CalculateAimTarget(bool includeRecoil)
        {
            var horizontal = Horizontal + (includeRecoil ? HorizontalRecoil : 0);
            var vertical = Vertical - (includeRecoil ? VerticalRecoil : 0);

            Vector3 cameraPosition, cameraTarget;
            calculatePositionAndTarget(horizontal, vertical, _currentPivot, _currentOffset, out cameraPosition, out cameraTarget);

            if (ShakingAffectsAim)
            {
                var vector = cameraTarget - cameraPosition;

                cameraTarget += Quaternion.AngleAxis(_shake.x, transform.right) * Quaternion.AngleAxis(_shake.y, transform.up) * vector;
            }

            return cameraTarget;
        }

        /// <summary>
        /// Update camera position and orientation based on the latest known information.
        /// </summary>
        public void UpdatePosition()
        {
            Vector3 cameraPosition, cameraTarget;
            calculatePositionAndTarget(Horizontal + HorizontalRecoil, Vertical - VerticalRecoil, _currentPivot, _currentOffset, out cameraPosition, out cameraTarget);

            var controller = Target.GetComponent<ThirdPersonController>();
            var gun = Target.ActiveWeapon.Gun;

            if (gun != null && gun.Scope != null && (Target.IsScoping || (controller != null && controller.IsScoping)))
            {
                Target.InputLayer(Layers.Scope);
                _wasScoped = true;
                _obstacleFix = Util.Lerp(_obstacleFix, Vector3.zero, 20);
            }
            else
            {
                _wasScoped = false;

                var forward = (cameraTarget - cameraPosition).normalized;

                var colliderFixTarget = Vector3.zero;

                if (AvoidObstacles && (!IgnoreObstaclesWhenZooming || !controller.IsZooming))
                    colliderFixTarget = checkObstacles(cameraPosition, _motorPosition + Target.StandingHeight * Vector3.up, 0.1f);

                _obstacleFix = Util.Lerp(_obstacleFix, colliderFixTarget, 6);
            }

            cameraPosition += _obstacleFix;

            transform.position = cameraPosition;
            transform.LookAt(cameraTarget);

            var magnitude = Vector3.Distance(cameraPosition, cameraTarget);

            transform.localRotation *= Quaternion.Euler(_shake);
            cameraTarget = transform.position + transform.forward * magnitude;
        }

        private void LateUpdate()
        {
            if (!_hasAskedForLateUpdate)
                UpdateForCharacterMotor();
        }

        protected override void Update()
        {
            base.Update();

            if (_shakeTime > float.Epsilon)
            {
                var t = _shakeTime / _shakeDuration;
                t *= t;

                Util.Lerp(ref _shake, _shakeTarget, 20);

                if (Vector3.Distance(_shake, _shakeTarget) < 1)
                {
                    _shakeTarget = Random.onUnitSphere * t * _shakeIntensity * 5;
                    _shakeTarget.z = 0;
                }

                _shakeTime -= Time.deltaTime;
            }
            else
                Util.Lerp(ref _shake, Vector3.zero, 10);

            if (Target != null)
            {
                var controller = getCurrentController();

                if (Target.IsInTallCover)
                    _isMirrored = Target.IsStandingLeftInCover;
                else if (!controller.IsZooming)
                {
                    if (_isMirrored && Target.WantsToMaintainMirror)
                    {
                        // Keep it as it is
                    }
                    else
                        _isMirrored = false;
                }

                if (_isMirrored)
                    Target.InputMirror();
            }

            if (Target)
            {
                _hasAskedForLateUpdate = true;
                Target.AskForLateUpdate(this);
            }
            else
                _hasAskedForLateUpdate = false;
        }

        /// <summary>
        /// Update performed after the character motor does it's thing.
        /// </summary>
        public override void UpdateForCharacterMotor()
        {
            _hasAskedForLateUpdate = false;

            if (Target == null)
                return;

            if ((Target.IsStandingLeftInCover && Target.IsByAnOpenLeftCorner) || (!Target.IsStandingLeftInCover && Target.IsByAnOpenRightCorner))
            {
                Util.Lerp(ref _motorOffsetIntensity, 1, 6);
                _motorOffset = Vector3.Lerp(_motorOffset, Target.ClosestCoverPosition - Target.transform.position, _motorOffsetIntensity);
            }
            else
            {
                _motorOffsetIntensity = 0;
                Util.Lerp(ref _motorOffset, Vector3.zero, 6);
            }

            _motorPosition = Target.transform.position + _motorOffset;

            if (Target.IsVaulting)
                _motorPosition.y = Target.VaultPosition;

            _motorRotation = Target.transform.rotation;

            UpdatePosition();

            if (_fader != null)
            {
                var plane = new Plane(_motorPosition, _motorPosition + transform.up, _motorPosition + transform.right);
                var ray = new Ray(transform.position, transform.forward);

                float enter;
                Vector3 hit;

                if (plane.Raycast(ray, out enter))
                    hit = transform.position + ray.direction * enter;
                else
                {
                    hit = Util.GetClosestHit(transform.position, transform.forward * 100, 0.1f, Target.gameObject);
                    hit -= transform.forward * 0.2f;
                }

                _fader.SetFadeTarget(new FadeTarget(Target.gameObject, hit));
            }

            var lookPosition = transform.position + transform.forward * 1000;
            var closestHit = Util.GetClosestHit(transform.position, lookPosition, Vector3.Distance(transform.position, Target.Top), Target.gameObject);

            _lastTargetDistance = Vector3.Distance(transform.position, closestHit);

            float alphaTarget = 0;
            CameraState state;

            var controller = getCurrentController();

            var couldCornerAim = false;

            if ((_couldCornerAim || !Target.IsMoving) &&
                Target.IsInCover &&
                ((Target.IsInTallCover && Target.CoverSettings.CanUseTallCorners) || (Target.IsInLowCover && Target.CoverSettings.CanUseLowCorners)) &&
                Target.Cover.IsFrontField(Horizontal + _horizontalDifference, 180) &&
                ((Target.IsNearLeftCorner && Target.IsStandingLeftInCover && Target.Cover.OpenLeft) || 
                 (Target.IsNearRightCorner && !Target.IsStandingLeftInCover && Target.Cover.OpenRight)))
            {
                couldCornerAim = true;
            }

            if (couldCornerAim)
            {
                const float max = 0.2f;

                if (!Target.IsWalkingInCover)
                    _cornerDelay = max;

                if (_cornerDelay < max)
                    _cornerDelay += Time.deltaTime;
                else
                    _isByCorner = true;
            }
            else if (_cornerDelay > float.Epsilon)
                _cornerDelay -= Time.deltaTime;
            else
                _isByCorner = false;

            _couldCornerAim = couldCornerAim;

            if (!Target.IsAlive)
            {
                _stateName = "Dead";
                state = States.Dead;
                alphaTarget = 0;
            }
            else if (controller != null && controller.IsMelee)
            {
                alphaTarget = 0;
                state = States.Melee;
                _stateName = "Melee";
            }
            else if (controller != null && controller.IsZooming)
            {
                alphaTarget = 1;

                if (couldCornerAim)
                {
                    if (Target.IsInTallCover)
                    {
                        _stateName = "TallCornerZoom";
                        state = States.TallCornerZoom;
                    }
                    else
                    {
                        _stateName = "LowCornerZoom";
                        state = States.LowCornerZoom;
                    }
                }
                else
                    state = States.Zoom;
            }
            else if (Target.IsClimbingOrVaulting)
            {
                _stateName = "Climb";
                state = States.Climb;
            }
            else if (Target.IsInCover)
            {
                if (couldCornerAim && _isByCorner)
                {
                    alphaTarget = 1;
                    _stateName = "Corner";
                    state = States.Corner;
                }
                else if (Target.IsInTallCover)
                {
                    if (Target.Cover.IsBack(Horizontal + _horizontalDifference, 0))
                    {
                        alphaTarget = 1;
                        _stateName = "TallCoverBack";
                        state = States.TallCoverBack;
                    }
                    else
                    {
                        _stateName = "TallCover";
                        state = States.TallCover;
                    }
                }
                else if (Target.HasGrenadeInHand)
                {
                    _stateName = "LowCoverGrenade";
                    state = States.LowCoverGrenade;
                }
                else
                {
                    alphaTarget = 1;
                    _stateName = "LowCover";
                    state = States.LowCover;
                }
            }
            else if (Target.IsCrouching)
            {
                if (Target.IsAiming)
                    alphaTarget = 1;

                _stateName = "Crouch";
                state = States.Crouch;
            }
            else if (Target.WouldAim)
            {
                if (Target.IsAiming)
                    alphaTarget = 1;

                _stateName = "Aim";
                state = States.Aim;
            }
            else
            {
                alphaTarget = 0;
                _stateName = "Default";
                state = States.Default;
            }

            var clamped = Mathf.Clamp(Vertical + _verticalDifference, -state.MaxAngle, -state.MinAngle);
            _verticalDifference = clamped - Vertical;

            var fov = state.FOV;
            _stateFOV = state.FOV;

            if (controller != null && Target != null && Target.ActiveWeapon.Gun != null)
            {
                if (controller.IsScoping)
                    fov -= Target.ActiveWeapon.Gun.Zoom;
                else if (controller.IsZooming)
                    fov -= Zoom;
            }

            var isScoped = controller != null && controller.IsScoping;
            var lerp = 6.0f;

            if (isScoped || (Target != null && Target.IsChangingWeapon))
                alphaTarget = 0;

            if (isScoped || _wasScoped)
            {
                _camera.fieldOfView = Util.Lerp(_camera.fieldOfView, fov, lerp * 3);
                lerp = 1.0f;
            }
            else
                _camera.fieldOfView = Util.Lerp(_camera.fieldOfView, fov, lerp);

            _wasScoped = isScoped;

            var stateConstantPivot = state.ConstantPivot;
            var statePivot = state.Pivot;
            var stateOffset = state.Offset;
            var stateOrientation = state.Orientation;

            if (_isMirrored)
            {
                stateConstantPivot.x *= -1;

                if (statePivot == Pivot.leftShoulder)
                    statePivot = Pivot.rightShoulder;
                else if (statePivot == Pivot.rightShoulder)
                    statePivot = Pivot.leftShoulder;

                stateOffset.x *= -1;
                stateOrientation.y *= -1;
            }

            _camera.fieldOfView = Util.Lerp(_camera.fieldOfView, fov, 6);
            Util.Lerp(ref _crosshairAlpha, alphaTarget, lerp);
            Util.Lerp(ref _orientation, stateOrientation, lerp);

            Vector3 newPivot;
            Vector3 newOffset = stateOffset;

            if (statePivot == Pivot.constant)
                newPivot = stateConstantPivot;
            else
            {
                if (Target.IsCrouching || (Target.IsInLowCover && !Target.IsAimingThroughCoverPlane))
                    newPivot = Target.ShoulderSettings.Crouching;
                else
                    newPivot = Target.ShoulderSettings.Standing;

                if (statePivot == Pivot.leftShoulder)
                    newPivot.x *= -1;
            }

            if (_wasAiming || Target.IsAiming)
            {
                Vector3 previousPosition, previousTarget;
                Vector3 nextPosition, nextTarget;

                calculatePositionAndTarget(Horizontal + _horizontalDifference, Vertical + _verticalDifference, _pivot, _offset, out previousPosition, out previousTarget);
                calculatePositionAndTarget(Horizontal + _horizontalDifference, Vertical + _verticalDifference, newPivot, newOffset, out nextPosition, out nextTarget);

                Vector3 target;
                RaycastHit hit;
                if (Physics.Raycast(previousPosition, (previousTarget - previousPosition).normalized, out hit) && Vector3.Distance(previousPosition, hit.point) > 2)
                    target = hit.point;
                else
                    target = previousTarget;

                var current = (nextTarget - nextPosition).normalized;
                var previous = (target - nextPosition).normalized;

                var horizontalShift = Util.HorizontalAngle(previous) - Util.HorizontalAngle(current);
                _horizontalDifference += horizontalShift;

                var verticalShift = Mathf.Asin(current.y) * Mathf.Rad2Deg - Mathf.Asin(previous.y) * Mathf.Rad2Deg;
                _verticalDifference += verticalShift;
            }

            _wasAiming = Target.IsAiming;
            _pivot = newPivot;
            _offset = newOffset;

            Util.Lerp(ref _currentPivot, _pivot, 6);
            Util.Lerp(ref _currentOffset, _offset, 6);

            var currentHorizontalDifferene = Util.LerpAngle(0, _horizontalDifference, 6);
            var currentVerticalDifference = Util.LerpAngle(0, _verticalDifference, 6);

            Horizontal += currentHorizontalDifferene;
            _horizontalDifference -= currentHorizontalDifferene;

            Vertical += currentVerticalDifference;
            _verticalDifference -= currentVerticalDifference;

            if (controller != null && controller.IsZooming && AskForSmoothRotations)
                Target.InputSmoothRotation();

            Target.InputPreciseHands();
        }

        private Vector3 checkObstacles(Vector3 camera, Vector3 target, float radius)
        {
            var startOffset = 0;

            var centerDistance = Vector3.Distance(camera, target);
            var forward = (target - camera).normalized;

            var maxFix = 0f;

            if (startOffset < centerDistance)
            {
                var origin = target - forward * startOffset;
                var max = Vector3.Distance(camera, target) + radius;

                if (max < float.Epsilon)
                    max = float.Epsilon;

                var up = (forward.y > -0.99f || forward.y < 0.99f) ? Vector3.up : Vector3.forward;
                var right = Vector3.Cross(up, forward);

                // SphereCast is buggy and returns garbage, use multiple raycasts as a hack
                raycast(origin, forward, max, ref maxFix);
                raycast(origin + right * radius, forward, max, ref maxFix);
                raycast(origin - right * radius, forward, max, ref maxFix);
                raycast(origin + up * radius, forward, max, ref maxFix);
                raycast(origin - up * radius, forward, max, ref maxFix);
            }

            return maxFix * forward;
        }

        private void raycast(Vector3 origin, Vector3 forward, float distance, ref float maxFix)
        {
            var ray = new Ray(origin, -forward);
            var count = Physics.RaycastNonAlloc(ray, Util.Hits, distance, Layers.Geometry, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                var hit = Util.Hits[i];

                if (hit.collider == null)
                    continue;

                if (!Util.InHiearchyOf(hit.collider.gameObject, Target.gameObject))
                {
                    var fix = Mathf.Clamp(distance - hit.distance, 0, distance);

                    if (fix > maxFix)
                        maxFix = fix;
                }
            }
        }
    }
}