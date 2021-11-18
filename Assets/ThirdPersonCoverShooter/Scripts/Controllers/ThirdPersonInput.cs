using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Describes a key used to trigger a custom action, animation and message names.
    /// </summary>
    [Serializable]
    public struct CustomAction
    {
        /// <summary>
        /// Key to be pressed to active the trigger.
        /// </summary>
        [Tooltip("Key to be pressed to active the trigger.")]
        public KeyCode Key;

        /// <summary>
        /// Name of the animation trigger.
        /// </summary>
        [Tooltip("Name of the animation trigger.")]
        public string Name;

        /// <summary>
        /// Name of the message.
        /// </summary>
        [Tooltip("Name of the message.")]
        public string Message;
    }

    /// <summary>
    /// Takes player input and transform it to commands to ThirdPersonController.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(ThirdPersonController))]
    public class ThirdPersonInput : MonoBehaviour
    {
        /// <summary>
        /// Camera moved by this input component.
        /// </summary>
        public ThirdPersonCamera Camera
        {
            get
            {
                if (CameraOverride != null)
                    return CameraOverride;
                else
                {
                    if (CameraManager.Main != _cachedCameraOwner)
                    {
                        _cachedCameraOwner = CameraManager.Main;

                        if (_cachedCameraOwner == null)
                            _cachedCamera = null;
                        else
                            _cachedCamera = _cachedCameraOwner.GetComponent<ThirdPersonCamera>();
                    }

                    return _cachedCamera;
                }
            }
        }

        /// <summary>
        /// Is character running instead of walking when moving.
        /// </summary>
        [Tooltip("Is character running instead of walking when moving.")]
        public bool FastMovement = true;

        /// <summary>
        /// Is character slowing to to a walk when zooming in.
        /// </summary>
        [Tooltip("Is character slowing to to a walk when zooming in.")]
        public bool WalkWhenZooming = true;

        /// <summary>
        /// Camera to rotate around the player. If set to none it is taken from the main camera.
        /// </summary>
        [Tooltip("Camera to rotate around the player. If set to none it is taken from the main camera.")]
        public ThirdPersonCamera CameraOverride;

        /// <summary>
        /// Multiplier for horizontal camera rotation.
        /// </summary>
        [Tooltip("Multiplier for horizontal camera rotation.")]
        [Range(0, 10)]
        public float HorizontalRotateSpeed = 2.0f;

        /// <summary>
        /// Multiplier for vertical camera rotation.
        /// </summary>
        [Tooltip("Multiplier for vertical camera rotation.")]
        [Range(0, 10)]
        public float VerticalRotateSpeed = 1.0f;

        /// <summary>
        /// Multiplier to rotation speeds when zooming in. Speed is already adjusted by the FOV difference.
        /// </summary>
        [Tooltip("Multiplier to rotation speeds when zooming in. Speed is already adjusted by the FOV difference.")]
        [Range(0, 10)]
        public float ZoomRotateMultiplier = 1.0f;

        /// <summary>
        /// Is camera responding to mouse movement when the mouse cursor is unlocked.
        /// </summary>
        [Tooltip("Is camera responding to mouse movement when the mouse cursor is unlocked.")]
        public bool RotateWhenUnlocked = false;

        /// <summary>
        /// Maximum time in seconds to wait for a second tap to active rolling.
        /// </summary>
        [Tooltip("Maximum time in seconds to wait for a second tap to active rolling.")]
        public float DoubleTapDelay = 0.3f;

        /// <summary>
        /// Keys to be pressed to activate custom actions.
        /// </summary>
        [Tooltip("Keys to be pressed to activate custom actions.")]
        public CustomAction[] CustomActions;

        /// <summary>
        /// Input is ignored when a disabler is active.
        /// </summary>
        [Tooltip("Input is ignored when a disabler is active.")]
        public GameObject Disabler;

        private CharacterMotor _motor;
        private ThirdPersonController _controller;
        private CharacterInventory _inventory;

        private Camera _cachedCameraOwner;
        private ThirdPersonCamera _cachedCamera;

        private float _timeW;
        private float _timeA;
        private float _timeS;
        private float _timeD;

        private float _leftMoveIntensity = 1;
        private float _rightMoveIntensity = 1;
        private float _backMoveIntensity = 1;
        private float _frontMoveIntensity = 1;

        private void Awake()
        {
            _controller = GetComponent<ThirdPersonController>();
            _motor = GetComponent<CharacterMotor>();
            _inventory = GetComponent<CharacterInventory>();

            _controller.WaitForUpdateCall = true;
        }

        private void Update()
        {
            if (Disabler != null && Disabler.activeSelf)
                return;

            UpdateCamera();
            UpdateTarget();
            UpdateCustomActions();
            UpdateMovement();
            UpdateWeapons();
            UpdateReload();
            UpdateRolling();
            UpdateAttack();
            UpdateGrenade();
            UpdateCrouching();
            UpdateClimbing();
            UpdateCover();
            UpdateJumping();

            _controller.ManualUpdate();
        }

        protected virtual void UpdateCustomActions()
        {
            foreach (var action in CustomActions)
                if (Input.GetKeyDown(action.Key))
                {
                    if (action.Message != null && action.Message.Length > 0)
                        SendMessage(action.Message, SendMessageOptions.RequireReceiver);

                    if (action.Name != null && action.Name.Length > 0)
                        SendMessage("OnCustomAction", action.Name, SendMessageOptions.RequireReceiver);
                }
        }

        protected virtual void UpdateMovement()
        {
            var local = Input.GetAxis("Horizontal") * Vector3.right +
                        Input.GetAxis("Vertical") * Vector3.forward;

            var movement = new CharacterMovement();
            movement.Direction = getMovementDirection(local);

            if (WalkWhenZooming && _controller.ZoomInput)
            {
                movement.Magnitude = 0.5f;
                movement.IsSlowedDown = true;
            }
            else
            {
                if ((_motor.ActiveWeapon.Gun != null || _motor.ActiveWeapon.HasMelee) && FastMovement)
                {
                    if (Input.GetButton("Run") && !_motor.IsCrouching)
                        movement.Magnitude = 2.0f;
                    else
                        movement.Magnitude = 1.0f;
                }
                else
                {
                    if (Input.GetButton("Run"))
                        movement.Magnitude = 1.0f;
                    else
                        movement.Magnitude = 0.5f;
                }
            }

            _controller.MovementInput = movement;
        }

        protected virtual void UpdateClimbing()
        {
            if (Input.GetButtonDown("Climb"))
            {
                var direction = Input.GetAxis("Horizontal") * Vector3.right +
                                Input.GetAxis("Vertical") * Vector3.forward;

                if (direction.magnitude > float.Epsilon)
                {
                    direction = Quaternion.Euler(0, aimAngle, 0) * direction.normalized;

                    var cover = _motor.GetClimbableInDirection(direction);

                    if (cover != null)
                        _controller.InputClimbOrVault(cover);
                }
            }
        }

        protected virtual void UpdateCover()
        {
            if (Input.GetButtonDown("TakeCover"))
                _controller.InputTakeCover();
        }

        protected virtual void UpdateJumping()
        {
            if (Input.GetButtonDown("Jump"))
            {
                var direction = Input.GetAxis("Horizontal") * Vector3.right +
                                Input.GetAxis("Vertical") * Vector3.forward;

                if (direction.magnitude > float.Epsilon)
                    _controller.InputJump(Util.HorizontalAngle(direction) + aimAngle);
                else
                    _controller.InputJump();
            }
        }

        protected virtual void UpdateCrouching()
        {
            if (Input.GetButton("Crouch"))
                _controller.InputCrouch();
        }

        protected virtual void UpdateGrenade()
        {
            if (_motor.HasGrenadeInHand)
            {
                if (Input.GetButtonDown("Fire"))
                    _controller.InputThrowGrenade();

                if (Input.GetButtonDown("Cancel"))
                    _controller.InputCancelGrenade();
            }

            if (Input.GetButton("Grenade"))
                _controller.InputTakeGrenade();
        }

        protected virtual void UpdateAttack()
        {
            if (Input.GetButtonDown("Fire"))
                _controller.FireInput = true;

            if (Input.GetButtonUp("Fire"))
                _controller.FireInput = false;

            if (Input.GetButtonDown("Melee"))
                _controller.InputMelee();

            if (Input.GetButtonDown("Zoom"))
                _controller.ZoomInput = true;

            if (Input.GetButtonUp("Zoom"))
                _controller.ZoomInput = false;

            if (Input.GetButtonDown("Block"))
                _controller.BlockInput = true;

            if (Input.GetButtonUp("Block"))
                _controller.BlockInput = false;

            if (_controller.IsZooming)
            {
                if (Input.GetButtonDown("Scope"))
                    _controller.ScopeInput = !_controller.ScopeInput;
            }
            else
                _controller.ScopeInput = false;
        }

        protected virtual void UpdateRolling()
        {
            if (_timeW > 0) _timeW -= Time.deltaTime;
            if (_timeA > 0) _timeA -= Time.deltaTime;
            if (_timeS > 0) _timeS -= Time.deltaTime;
            if (_timeD > 0) _timeD -= Time.deltaTime;

            if (Input.GetButtonDown("RollForward"))
            {
                if (_timeW > float.Epsilon)
                {
                    var cover = _motor.GetClimbambleInDirection(aimAngle);

                    if (cover != null)
                        _controller.InputClimbOrVault(cover);
                    else
                        roll(Vector3.forward);
                }
                else
                    _timeW = DoubleTapDelay;
            }

            if (Input.GetButtonDown("RollLeft"))
            {
                if (_timeA > float.Epsilon)
                {
                    var cover = _motor.GetClimbambleInDirection(aimAngle - 90);

                    if (cover != null)
                        _controller.InputClimbOrVault(cover);
                    else
                        roll(-Vector3.right);
                }
                else
                    _timeA = DoubleTapDelay;
            }

            if (Input.GetButtonDown("RollBackward"))
            {
                if (_timeS > float.Epsilon)
                {
                    var cover = _motor.GetClimbambleInDirection(aimAngle + 180);

                    if (cover != null)
                        _controller.InputClimbOrVault(cover);
                    else
                        roll(-Vector3.forward);
                }
                else
                    _timeS = DoubleTapDelay;
            }

            if (Input.GetButtonDown("RollRight"))
            {
                if (_timeD > float.Epsilon)
                {
                    var cover = _motor.GetClimbambleInDirection(aimAngle + 90);

                    if (cover != null)
                        _controller.InputClimbOrVault(cover);
                    else
                        roll(Vector3.right);
                }
                else
                    _timeD = DoubleTapDelay;
            }
        }

        protected virtual void UpdateWeapons()
        {
            if (Input.GetKey(KeyCode.Alpha1)) { _motor.InputCancelGrenade(); inputWeapon(0); }
            if (Input.GetKey(KeyCode.Alpha2)) { _motor.InputCancelGrenade(); inputWeapon(1); }
            if (Input.GetKey(KeyCode.Alpha3)) { _motor.InputCancelGrenade(); inputWeapon(2); }
            if (Input.GetKey(KeyCode.Alpha4)) { _motor.InputCancelGrenade(); inputWeapon(3); }
            if (Input.GetKey(KeyCode.Alpha5)) { _motor.InputCancelGrenade(); inputWeapon(4); }
            if (Input.GetKey(KeyCode.Alpha6)) { _motor.InputCancelGrenade(); inputWeapon(5); }
            if (Input.GetKey(KeyCode.Alpha7)) { _motor.InputCancelGrenade(); inputWeapon(6); }
            if (Input.GetKey(KeyCode.Alpha8)) { _motor.InputCancelGrenade(); inputWeapon(7); }
            if (Input.GetKey(KeyCode.Alpha9)) { _motor.InputCancelGrenade(); inputWeapon(8); }
            if (Input.GetKey(KeyCode.Alpha0)) { _motor.InputCancelGrenade(); inputWeapon(9); }

            if (Input.mouseScrollDelta.y < 0)
            {
                if (currentWeapon == 0 && _inventory != null)
                    inputWeapon(_inventory.Weapons.Length);
                else
                    inputWeapon(currentWeapon - 1);
            }
            else if (Input.mouseScrollDelta.y > 0)
            {
                if (_inventory != null && currentWeapon == _inventory.Weapons.Length)
                    inputWeapon(0);
                else
                    inputWeapon(currentWeapon + 1);
            }
        }

        private int currentWeapon
        {
            get
            {
                if (_inventory == null || !_motor.IsEquipped)
                    return 0;

                for (int i = 0; i < _inventory.Weapons.Length; i++)
                    if (_inventory.Weapons[i].IsTheSame(ref _motor.Weapon))
                        return i + 1;

                return 0;
            }
        }

        private void inputWeapon(int index)
        {
            if (_inventory == null && index > 0)
                return;

            if (index <= 0 || (_inventory != null && index > _inventory.Weapons.Length))
                _controller.InputUnequip();
            else if (_inventory != null && index <= _inventory.Weapons.Length)
                _controller.InputEquip(_inventory.Weapons[index - 1]);
        }

        protected virtual void UpdateReload()
        {
            if (Input.GetButton("Reload"))
                _controller.InputReload();
        }

        protected virtual void UpdateTarget()
        {
            if (_controller == null)
                return;

            var camera = Camera;
            if (camera == null) return;

            var inaccurateTarget = camera.CalculateAimTarget(false);
            var accurateTarget = Util.GetClosestStaticHit(camera.transform.position, inaccurateTarget, 0);

            if (_motor.IsFiringFromCamera && _motor.ActiveWeapon.Gun != null)
            {
                var preciseTarget = camera.CalculateAimTarget(true);
                var preciseHit = Util.GetClosestStaticHit(camera.transform.position, preciseTarget, 0);

                _motor.ActiveWeapon.Gun.SetupRaycastThisFrame(camera.transform.position, preciseHit);
            }

            _controller.BodyTargetInput = inaccurateTarget;
            _controller.AimTargetInput = accurateTarget;
            _controller.GrenadeHorizontalAngleInput = Util.HorizontalAngle(camera.transform.forward);
            _controller.GrenadeVerticalAngleInput = Mathf.Asin(camera.transform.forward.y) * 180f / Mathf.PI;
        }

        protected virtual void UpdateCamera()
        {
            var camera = Camera;
            if (camera == null) return;

            if (Cursor.lockState == CursorLockMode.Locked || RotateWhenUnlocked)
            {
                var scale = 1.0f;

                if (_motor != null)
                {
                    var gun = _motor.ActiveWeapon.Gun;

                    if (_controller.IsScoping && gun != null)
                        scale = ZoomRotateMultiplier * (1.0f - gun.Zoom / camera.StateFOV);
                    else if (_controller.IsZooming)
                        scale = ZoomRotateMultiplier * (1.0f - camera.Zoom / camera.StateFOV);
                }

                camera.Horizontal += Input.GetAxis("Mouse X") * HorizontalRotateSpeed * scale;
                camera.Vertical -= Input.GetAxis("Mouse Y") * VerticalRotateSpeed * scale;
                camera.UpdatePosition();
            }

            camera.HorizontalRecoil = _motor.HorizontalRecoil;
            camera.VerticalRecoil = _motor.VerticalRecoil;
            camera.UpdatePosition();

            _motor.InputVerticalMeleeAngle(camera.Vertical);
        }

        private void roll(Vector3 local)
        {
            var direction = getMovementDirection(local);

            if (direction.sqrMagnitude > float.Epsilon)
                _controller.InputRoll(Util.HorizontalAngle(direction));
        }

        private Vector3 getMovementDirection(Vector3 local)
        {
            var forward = _controller.BodyTargetInput - transform.position;
            forward.y = 0;
            forward.Normalize();

            float angle;

            if (_motor.IsInCover)
            {
                angle = Util.HorizontalAngle(forward);

                if (_motor.Cover.IsLeft(angle, 45))
                    angle = Util.HorizontalAngle(_motor.Cover.Left);
                else if (_motor.Cover.IsRight(angle, 45))
                    angle = Util.HorizontalAngle(_motor.Cover.Right);
                else if (_motor.Cover.IsBack(angle, 45))
                    angle = Util.HorizontalAngle(-_motor.Cover.Forward);
                else
                    angle = Util.HorizontalAngle(_motor.Cover.Forward);

                forward = Util.HorizontalVector(angle);
            }
            else
                angle = Util.HorizontalAngle(forward);

            var right = Vector3.Cross(Vector3.up, forward);

            Util.Lerp(ref _leftMoveIntensity, _motor.IsFreeToMove(-right) ? 1.0f : 0.0f, 4);
            Util.Lerp(ref _rightMoveIntensity, _motor.IsFreeToMove(right) ? 1.0f : 0.0f, 4);
            Util.Lerp(ref _backMoveIntensity, _motor.IsFreeToMove(-forward) ? 1.0f : 0.0f, 4);
            Util.Lerp(ref _frontMoveIntensity, _motor.IsFreeToMove(forward) ? 1.0f : 0.0f, 4);

            if (local.x < -float.Epsilon) local.x *= _leftMoveIntensity;
            if (local.x > float.Epsilon) local.x *= _rightMoveIntensity;
            if (local.z < -float.Epsilon) local.z *= _backMoveIntensity;
            if (local.z > float.Epsilon) local.z *= _frontMoveIntensity;

            return Quaternion.Euler(0, angle, 0) * local;
        }

        private float aimAngle
        {
            get { return Util.HorizontalAngle(_controller.BodyTargetInput - transform.position); }
        }
    }
}