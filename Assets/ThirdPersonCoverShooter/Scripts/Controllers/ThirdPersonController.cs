using UnityEngine;
using UnityEngine.UI;

namespace CoverShooter
{
    /// <summary>
    /// Takes player input (usually from ThirdPersonInput) and translates that to commands to CharacterMotor.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(Actor))]
    public class ThirdPersonController : MonoBehaviour
    {
        /// <summary>
        /// Is the character using zoom.
        /// </summary>
        public bool IsZooming
        {
            get
            {
                return _isZooming &&
                       _motor.IsAlive &&
                       _mode == Mode.Aim &&
                       (((!_motor.IsInCover || _motor.IsReloadingAndNotAiming) && _motor.IsInCameraAimableState) ||
                        (_motor.IsInCover && !_motor.IsReloadingAndNotAiming && _motor.IsInAimableState));
            }
        }

        /// <summary>
        /// Is the character using zoom.
        /// </summary>
        public bool IsScoping
        {
            get { return ScopeInput && IsZooming && CouldScope && _motor.IsWeaponScopeReady && _mode == Mode.Aim; }
        }

        /// <summary>
        /// Is the controller in melee mode right now.
        /// </summary>
        public bool IsMelee
        {
            get { return _mode == Mode.Melee; }
        }

        /// <summary>
        /// Can a scope be displayed right now.
        /// </summary>
        public bool CouldScope
        {
            get
            {
                var gun = _motor.ActiveWeapon.Gun;
                return gun != null && gun.Scope != null;
            }
        }

        /// <summary>
        /// Determines if the character takes cover automatically instead of waiting for player input.
        /// </summary>
        [Tooltip("Determines if the character takes cover automatically instead of waiting for player input.")]
        public bool AutoTakeCover = true;

        /// <summary>
        /// Time in seconds after a potential cover has been detected when the character automatically enters it.
        /// </summary>
        [Tooltip("Time in seconds after a potential cover has been detected when the character automatically enters it.")]
        public float CoverEnterDelay = 0.1f;

        /// <summary>
        /// Is the character always aiming in camera direction when not in cover.
        /// </summary>
        [Tooltip("Is the character always aiming in camera direction when not in cover.")]
        public bool AlwaysAim = false;

        /// <summary>
        /// Should the character aim when walking, if turned off it will only aim when zooming in.
        /// </summary>
        [Tooltip("Should the character aim when walking, if turned off it will only aim when zooming in.")]
        public bool AimWhenWalking = false;

        /// <summary>
        /// Should the character start crouching near closing in to a low cover.
        /// </summary>
        [Tooltip("Should the character start crouching near closing in to a low cover.")]
        public bool CrouchNearCovers = true;

        /// <summary>
        /// Will the character turn immediatelly when aiming.
        /// </summary>
        [Tooltip("Will the character turn immediatelly when aiming.")]
        public bool ImmediateTurns = true;

        /// <summary>
        /// Radius at which enemy actors are detected in melee mode.
        /// </summary>
        [Tooltip("Radius at which enemy actors are detected in melee mode.")]
        public float MeleeRadius = 4;

        /// <summary>
        /// Distance to maintain against melee targets.
        /// </summary>
        [Tooltip("Distance to maintain against melee targets.")]
        public float MinMeleeDistance = 1.5f;

        /// <summary>
        /// How long to continue aiming after no longer needed.
        /// </summary>
        [Tooltip("How long to continue aiming after no longer needed.")]
        public float AimSustain = 0.4f;

        /// <summary>
        /// Time in seconds to keep the gun down when starting to move.
        /// </summary>
        [Tooltip("Time in seconds to keep the gun down when starting to move.")]
        public float NoAimSustain = 0.14f;

        /// <summary>
        /// Cancel get hit animations upon player input.
        /// </summary>
        [Tooltip("Cancel get hit animations upon player input.")]
        public bool CancelHurt = true;

        /// <summary>
        /// Degrees to add when aiming a grenade vertically.
        /// </summary>
        [Tooltip("Degrees to add when aiming a grenade vertically.")]
        public float ThrowAngleOffset = 30;

        /// <summary>
        /// How high can the player throw the grenade.
        /// </summary>
        [Tooltip("How high can the player throw the grenade.")]
        public float MaxThrowAngle = 45;

        /// <summary>
        /// Time in seconds to wait after landing before AlwaysAim activates again.
        /// </summary>
        [Tooltip("Time in seconds to wait after landing before AlwaysAim activates again.")]
        public float PostLandAimDelay = 0.4f;

        /// <summary>
        /// Time in seconds to wait before lifting an arm when running with a pistol.
        /// </summary>
        [Tooltip("Time in seconds to wait before lifting an arm when running with a pistol.")]
        public float ArmLiftDelay = 1.5f;

        /// <summary>
        /// Prefab to instantiate to display grenade explosion preview.
        /// </summary>
        [Tooltip("Prefab to instantiate to display grenade explosion preview.")]
        public GameObject ExplosionPreview;

        /// <summary>
        /// Prefab to instantiate to display grenade path preview.
        /// </summary>
        [Tooltip("Prefab to instantiate to display grenade path preview.")]
        public GameObject PathPreview;

        /// <summary>
        /// Scope object and component that's enabled and maintained when using scope.
        /// </summary>
        [Tooltip("Scope object and component that's enabled and maintained when using scope.")]
        public Image Scope;

        /// <summary>
        /// Sets the controller to start or stop firing.
        /// </summary>
        [HideInInspector]
        public bool FireInput;

        /// <summary>
        /// Sets the controller to start and stop zooming.
        /// </summary>
        [HideInInspector]
        public bool ZoomInput;

        /// <summary>
        /// Sets the controller to start and stop using scope.
        /// </summary>
        [HideInInspector]
        public bool ScopeInput;

        /// <summary>
        /// Sets the controller to block melee attacks.
        /// </summary>
        [HideInInspector]
        public bool BlockInput;

        /// <summary>
        /// Sets the position the controller is rotated at.
        /// </summary>
        [HideInInspector]
        public Vector3 BodyTargetInput;

        /// <summary>
        /// Sets the position the controller is aiming at.
        /// </summary>
        [HideInInspector]
        public Vector3 AimTargetInput;

        /// <summary>
        /// Sets the horizontal angle for aiming a grenade.
        /// </summary>
        [HideInInspector]
        public float GrenadeHorizontalAngleInput;

        /// <summary>
        /// Sets the vertical angle for aiming a grenade.
        /// </summary>
        [HideInInspector]
        public float GrenadeVerticalAngleInput;

        /// <summary>
        /// Sets the movement for the controller.
        /// </summary>
        [HideInInspector]
        public CharacterMovement MovementInput;

        /// <summary>
        /// Will the Update function be called manually by some other component (most likely ThirdPersonInput).
        /// </summary>
        [HideInInspector]
        public bool WaitForUpdateCall;

        public enum Mode
        {
            Default,
            Cover,
            Sprint,
            Aim,
            Grenade,
            Melee
        }

        private Actor _actor;
        private CharacterMotor _motor;

        private Mode _mode;

        private bool _wantsToThrowGrenade;
        private bool _wantsToAim;
        private bool _wantsToHit;
        private bool _wantsToRoll;
        private bool _wantsToReload;
        private bool _wantsToUnequip;
        private WeaponDescription _wantsToEquip;
        private bool _wantsToTakeGrenade;
        private bool _wantsToCancelGrenade;
        private bool _wantsToCrouch;
        private bool _wantsToTakeCover;
        private bool _wantsToJump;
        private Cover _wantsToClimbOrVault;

        private bool _isZooming;
        private bool _isScoping;

        private float _rollAngle;
        private float _jumpAngle;
        private bool _hasJumpAngle;

        private float _coverDelayWait;
        private float _aimSustain;
        private float _noAimSustain;
        private float _postSprintNoAutoAim;
        private float _armLiftTimer;
        private float _armLiftRetain;
        private float _postLandWait;

        private GameObject _explosionPreview;
        private GameObject _pathPreview;

        private Vector3[] _grenadePath = new Vector3[128];
        private int _grenadePathLength;
        private bool _hasGrenadePath;

        private Actor _meleeTarget;

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            _motor = GetComponent<CharacterMotor>();
        }

        public void InputTakeGrenade()
        {
            _wantsToTakeGrenade = true;
        }

        public void InputCancelGrenade()
        {
            _wantsToCancelGrenade = true;
        }

        public void InputTakeCover()
        {
            _wantsToTakeCover = true;
        }

        public void InputReload()
        {
            _wantsToReload = true;
        }

        public void InputJump()
        {
            _wantsToJump = true;
            _hasJumpAngle = false;
        }

        public void InputJump(float angle)
        {
            _wantsToJump = true;
            _hasJumpAngle = true;
            _jumpAngle = angle;
        }

        public void InputUnequip()
        {
            _wantsToUnequip = true;
        }

        public void InputEquip(WeaponDescription weapon)
        {
            _wantsToEquip = weapon;
        }

        public void InputCrouch()
        {
            _wantsToCrouch = true;
        }

        /// <summary>
        /// Set the grenade throw mode.
        /// </summary>
        public void InputThrowGrenade()
        {
            _wantsToThrowGrenade = true;
        }

        /// <summary>
        /// Make the character aim in the following frame.
        /// </summary>
        public void InputAim()
        {
            _wantsToAim = true;
        }

        /// <summary>
        /// Tell the controller to manipulate the motor and do a hit.
        /// </summary>
        public void InputMelee()
        {
            _wantsToHit = true;
        }

        /// <summary>
        /// Tell the controller to roll 
        /// </summary>
        public void InputRoll(float angle)
        {
            _wantsToRoll = true;
            _rollAngle = angle;
        }

        public void InputClimbOrVault(Cover cover)
        {
            _wantsToClimbOrVault = cover;
        }

        private void OnEnable()
        {
            if (_motor != null && AlwaysAim && !_motor.ActiveWeapon.IsNull)
                _motor.InputImmediateAim();
        }

        private void LateUpdate()
        {
            if (!WaitForUpdateCall)
                ManualUpdate();
        }

        public bool ManualUpdate()
        {
            if (_mode != Mode.Grenade)
                destroyGrenadePreview();

            checkLanded();

            switch (_mode)
            {
                //////////////////////////////////////////////////////////////////////////////////////////////////////
                case Mode.Default:
                    {
                        //// TRANSITIONS //////////////////////

                        if (_motor.HasGrenadeInHand)
                            return transitionAndPerform(Mode.Grenade);

                        if (updateIsAiming())
                            return transitionAndPerform(Mode.Aim);

                        if (isSprinting())
                            return transitionAndPerform(Mode.Sprint);

                        if (updateIsMelee())
                            return transitionAndPerform(Mode.Melee);

                        if (_motor.IsInCover)
                            return transitionAndPerform(Mode.Cover);

                        //// LOGIC //////////////////////

                        checkArmLift();
                        checkReload();
                        checkUnzoom();
                        checkEquipment();
                        checkJump();
                        checkRoll();
                        checkClimbOrVault();
                        checkMovement(1);
                        checkCover();
                        checkGrenade();
                        checkCrouch();
                        checkMelee();

                    } break;

                //////////////////////////////////////////////////////////////////////////////////////////////////////
                case Mode.Sprint:
                    {
                        //// TRANSITIONS //////////////////////

                        if (_motor.HasGrenadeInHand)
                            return transitionAndPerform(Mode.Grenade);

                        if (updateIsAiming())
                            return transitionAndPerform(Mode.Aim);

                        if (!isSprinting())
                            return transitionAndPerform(Mode.Default);

                        //// LOGIC //////////////////////

                        lowerArms();

                        checkReload();
                        checkUnzoom();
                        checkEquipment();
                        checkJump();
                        checkRoll();
                        checkClimbOrVault();
                        checkMovement(2);
                        checkCover();
                        checkGrenade();
                        checkCrouch();
                    }
                    break;

                case Mode.Cover:
                    {
                        //// TRANSITIONS //////////////////////

                        if (_motor.HasGrenadeInHand)
                            return transitionAndPerform(Mode.Grenade);

                        if (updateIsAiming())
                            return transitionAndPerform(Mode.Aim);

                        if (isSprinting())
                            return transitionAndPerform(Mode.Sprint);

                        if (updateIsMelee())
                            return transitionAndPerform(Mode.Melee);

                        if (!_motor.IsInCover)
                            return transitionAndPerform(Mode.Default);

                        //// LOGIC //////////////////////

                        lowerArms();

                        checkMelee();
                        checkReload();
                        checkUnzoom();
                        checkEquipment();
                        checkJump();
                        checkRoll();
                        checkClimbOrVault();
                        checkMovement(1);
                        checkGrenade();
                        checkCrouch();
                    } break;

                //////////////////////////////////////////////////////////////////////////////////////////////////////
                case Mode.Aim:
                    {
                        //// TRANSITIONS //////////////////////

                        if (!updateIsAiming())
                            return transitionAndPerform(Mode.Default);

                        //// LOGIC //////////////////////

                        _noAimSustain = 0;

                        if (AimWhenWalking && MovementInput.IsMoving)
                            _aimSustain = AimSustain;
                        else
                            _aimSustain -= Time.deltaTime;

                        checkZoom();
                        checkEquipment();
                        checkJump();
                        checkRoll();
                        checkClimbOrVault();
                        checkMovement(1);
                        checkCover();
                        checkGrenade();
                        checkCrouch();
                        checkReload();
                        checkMelee();

                        if (CancelHurt)
                            _motor.CancelAndPreventGetHit(0.3f);

                        liftArms();

                        _motor.SetBodyTarget(BodyTargetInput);
                        _motor.SetAimTarget(AimTargetInput);

                        if (ImmediateTurns)
                            _motor.InputPossibleImmediateTurn();

                        if (_motor.IsInCover)
                            _motor.InputAimWhenLeavingCover();

                        _motor.InputAim();

                        if (_motor.IsWeaponReady && FireInput)
                        {
                            _aimSustain = AimSustain;

                            var gun = _motor.ActiveWeapon.Gun;

                            if (gun != null && gun.LoadedBulletsLeft <= 0)
                                _motor.InputReload();
                            else
                                _motor.InputFire();
                        }

                        if (_motor.IsWeaponScopeReady && ZoomInput)
                        {
                            _aimSustain = AimSustain;

                            _motor.InputZoom();

                            if (_isScoping)
                                _motor.InputScope();
                        }
                    }
                    break;

                //////////////////////////////////////////////////////////////////////////////////////////////////////
                case Mode.Grenade:
                    {
                        if (!_motor.HasGrenadeInHand)
                            return transitionAndPerform(Mode.Default);

                        if (_motor.IsReadyToThrowGrenade && _motor.CurrentGrenade != null)
                        {
                            GrenadeDescription desc;
                            desc.Gravity = _motor.Grenade.Gravity;
                            desc.Duration = _motor.PotentialGrenade.Timer;
                            desc.Bounciness = _motor.PotentialGrenade.Bounciness;

                            var verticalAngle = Mathf.Min(GrenadeVerticalAngleInput + ThrowAngleOffset, MaxThrowAngle);

                            var velocity = _motor.Grenade.MaxVelocity;

                            if (verticalAngle < 45)
                                velocity *= Mathf.Clamp01((verticalAngle + 15) / 45f);

                            _grenadePathLength = GrenadePath.Calculate(GrenadePath.Origin(_motor, GrenadeHorizontalAngleInput),
                                                                       GrenadeHorizontalAngleInput,
                                                                       verticalAngle,
                                                                       velocity,
                                                                       desc,
                                                                       _grenadePath,
                                                                       _motor.Grenade.Step);
                            _hasGrenadePath = true;

                            if (_explosionPreview == null && ExplosionPreview != null)
                            {
                                _explosionPreview = GameObject.Instantiate(ExplosionPreview);
                                _explosionPreview.transform.SetParent(null);
                                _explosionPreview.SetActive(true);
                            }

                            if (_explosionPreview != null)
                            {
                                _explosionPreview.transform.localScale = Vector3.one * _motor.PotentialGrenade.ExplosionRadius * 2;
                                _explosionPreview.transform.position = _grenadePath[_grenadePathLength - 1];
                            }

                            if (_pathPreview == null && PathPreview != null)
                            {
                                _pathPreview = GameObject.Instantiate(PathPreview);
                                _pathPreview.transform.SetParent(null);
                                _pathPreview.SetActive(true);
                            }

                            if (_pathPreview != null)
                            {
                                _pathPreview.transform.position = _grenadePath[0];

                                var path = _pathPreview.GetComponent<PathPreview>();

                                if (path != null)
                                {
                                    path.Points = _grenadePath;
                                    path.PointCount = _grenadePathLength;
                                }
                            }
                        }
                        else
                            destroyGrenadePreview();

                        if (_hasGrenadePath && _wantsToThrowGrenade)
                        {
                            if (ImmediateTurns)
                                _motor.InputPossibleImmediateTurn();

                            _motor.SetBodyTarget(BodyTargetInput);
                            _motor.SetAimTarget(BodyTargetInput);
                            _motor.InputThrowGrenade(_grenadePath, _grenadePathLength, _motor.Grenade.Step);
                        }

                        if (_wantsToCancelGrenade)
                            _motor.InputCancelGrenade();

                        lowerArms();

                        checkZoom();
                        checkJump();
                        checkRoll();
                        checkClimbOrVault();
                        checkMovement(2);
                        checkCover();
                        checkCrouch();
                    }
                    break;

                //////////////////////////////////////////////////////////////////////////////////////////////////////
                case Mode.Melee:
                    {
                        //// TRANSITIONS //////////////////////

                        if (_motor.HasGrenadeInHand)
                            return transitionAndPerform(Mode.Grenade);

                        if (isSprinting())
                            return transitionAndPerform(Mode.Sprint);

                        if (!updateIsMelee())
                            return transitionAndPerform(Mode.Default);

                        //// LOGIC //////////////////////

                        checkUnzoom();
                        checkEquipment();
                        checkJump();
                        checkRoll();
                        checkClimbOrVault();
                        checkMovement(2);
                        checkGrenade();
                        checkCrouch();
                        checkMelee();

                        if (_meleeTarget != null)
                        {
                            _motor.SetBodyTarget(_meleeTarget.transform.position);
                            _motor.SetAimTarget(_meleeTarget.transform.position + Vector3.up * 1.5f);
                            _motor.InputMeleeTarget(_meleeTarget.transform.position);

                            if (!MovementInput.IsMoving)
                            {
                                var vector = _meleeTarget.transform.position - _motor.transform.position;
                                var distance = vector.magnitude;

                                if (distance < MinMeleeDistance)
                                {
                                    var other = Characters.Get(_meleeTarget.gameObject);

                                    if (other.Motor == null || !other.Motor.IsPerformingMelee)
                                    {
                                        const float minMovementDuration = 0.2f;

                                        if (distance < MinMeleeDistance * 0.35f)
                                            _motor.InputMovement(new CharacterMovement(-vector / distance, 1, minMovementDuration));
                                        else
                                            _motor.InputMovement(new CharacterMovement(-vector / distance, 0.5f, minMovementDuration));
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            // RESET

            _wantsToThrowGrenade = false;
            _wantsToAim = false;
            _wantsToHit = false;
            _wantsToRoll = false;
            _wantsToReload = false;
            _wantsToUnequip = false;
            _wantsToTakeGrenade = false;
            _wantsToCancelGrenade = false;
            _wantsToCrouch = false;
            _wantsToTakeCover = false;
            _wantsToJump = false;
            _wantsToClimbOrVault = null;

            if (!_wantsToEquip.IsNull)
                _wantsToEquip = new WeaponDescription();

            // RETURN

            return true;
        }

        /// UPDATE AND TRANSITION /////////////////////////////////////////////

        private bool transitionAndPerform(Mode mode)
        {
            _mode = mode;
            return ManualUpdate();
        }

        private bool isSprinting()
        {
            if (!MovementInput.IsMoving ||
                MovementInput.Magnitude <= 1.1f ||
                _motor.IsCrouching ||
                _motor.IsClimbingOrVaulting ||
                _motor.IsRolling ||
                !_motor.IsGrounded ||
                _motor.IsInCover ||
                _motor.HasGrenadeInHand)
                return false;
            else if ((FireInput & _motor.IsWeaponReady) ||
                     (ZoomInput && _motor.IsWeaponScopeReady))
                return false;
            else
                return true;
        }

        private bool updateIsAiming()
        {
            if (_motor.HasGrenadeInHand)
                return false;

            if (_postLandWait > float.Epsilon)
                return false;

            if (_wantsToAim && _motor.IsWeaponReady)
                return true;

            if (!_motor.IsGrounded)
                return false;

            var weapon = _motor.EquippedWeapon;

            if (weapon.Gun == null)
                return false;

            if (!_motor.IsInCover)
            {
                if (AlwaysAim)
                    return !isSprinting();
                else if (AimWhenWalking && MovementInput.IsMoving)
                {
                    if (isSprinting())
                        return false;
                    else
                    {
                        if (_noAimSustain < NoAimSustain)
                            _noAimSustain += Time.deltaTime;
                        else
                            return true;
                    }
                }
                else
                    _noAimSustain = 0;
            }

            if ((FireInput & _motor.IsWeaponReady) ||
                (ZoomInput && _motor.IsWeaponScopeReady))
                return true;

            if (_motor.IsInCover)
                return false;
            else
                return _aimSustain > float.Epsilon;
        }

        private bool updateIsMelee()
        {
            _meleeTarget = null;

            var weapon = _motor.EquippedWeapon;

            if (!weapon.HasMelee || weapon.Gun != null)
                return false;

            var minDist = 0f;

            for (int i = 0; i < AIUtil.FindActors(_motor.transform.position, MeleeRadius, _actor); i++)
            {
                var actor = AIUtil.Actors[i];

                if (actor.Side == _actor.Side)
                    continue;

                var dist = Vector3.Distance(_motor.transform.position, actor.transform.position);

                if (_meleeTarget == null || dist < minDist)
                {
                    _meleeTarget = actor;
                    minDist = dist;
                }
            }

            return _meleeTarget != null;
        }

        /// CHECKS /////////////////////////////////////////////

        private void checkLanded()
        {
            if (_motor.IsGrounded)
            {
                if (_postLandWait >= 0)
                    _postLandWait -= Time.deltaTime;
            }
            else
                _postLandWait = PostLandAimDelay;
        }

        private void destroyGrenadePreview()
        {
            if (_explosionPreview != null)
            {
                GameObject.Destroy(_explosionPreview);
                _explosionPreview = null;
            }

            if (_pathPreview != null)
            {
                GameObject.Destroy(_pathPreview);
                _pathPreview = null;
            }

            _hasGrenadePath = false;
        }

        private void lowerArms()
        {
            _armLiftTimer = 0;
            _armLiftRetain = 0;
        }

        private void liftArms()
        {
            _armLiftTimer = ArmLiftDelay;
            _armLiftRetain = 0;
        }

        private void checkArmLift()
        {
            if (_motor.IsInProcess && !_motor.CanMoveInProcess)
                lowerArms();
            else if (!_motor.IsGrounded)
                lowerArms();
            else
            {
                if ((MovementInput.IsSlowedDown || MovementInput.Magnitude > 0.6f) && MovementInput.Magnitude < 1.1f && MovementInput.IsMoving && !_motor.IsClimbingOrVaulting)
                {
                    _armLiftTimer += Time.deltaTime;
                    _armLiftRetain = 0.1f;
                }
                else
                {
                    if (_armLiftRetain > float.Epsilon)
                        _armLiftRetain -= Time.deltaTime;
                    else
                        _armLiftTimer = Mathf.Clamp01(_armLiftTimer - Time.deltaTime);
                }
            }

            if (_armLiftTimer >= ArmLiftDelay - float.Epsilon)
                _motor.InputArmLift();
        }

        private void checkZoom()
        {
            if (ZoomInput && !_isZooming)
            {
                _isZooming = true;
                _motor.NotifyZoom();
            }
            else if (!ZoomInput && _isZooming)
            {
                _isZooming = false;
                _motor.NotifyUnzoom();
            }

            if (ScopeInput && !_isScoping)
            {
                _isScoping = true;
                _motor.NotifyScope();
            }
            else if (!ScopeInput && _isScoping)
            {
                _isScoping = false;
                _motor.NotifyUnscope();
            }

            if (Scope != null)
            {
                if (Scope.gameObject.activeSelf != IsScoping)
                {
                    Scope.gameObject.SetActive(IsScoping);

                    var gun = _motor.ActiveWeapon.Gun;

                    if (Scope.gameObject.activeSelf && gun != null)
                        Scope.sprite = gun.Scope;
                }
            }
        }

        private void checkMelee()
        {
            var weapon = _motor.EquippedWeapon;

            if (!weapon.HasMelee)
                return;

            Vector3 target;

            if (_meleeTarget != null)
                target = _meleeTarget.transform.position;
            else
                target = AimTargetInput;

            if (_wantsToHit)
                _motor.InputMelee(target);
            else if (FireInput && weapon.Gun == null)
                _motor.InputMelee(target);
            else if (BlockInput && weapon.Gun == null && !_motor.IsPerformingMelee)
                _motor.InputBlock();
        }

        private void checkUnzoom()
        {
            if (_isZooming)
            {
                _motor.NotifyUnzoom();
                _isZooming = false;
            }

            if (_isScoping)
            {
                _motor.NotifyUnscope();
                _isScoping = false;
            }

            if (Scope != null)
                if (Scope.gameObject.activeSelf != IsScoping)
                    Scope.gameObject.SetActive(IsScoping);
        }

        private void checkEquipment()
        {
            if (!_wantsToEquip.IsNull)
            {
                _motor.CancelAndPreventGetHit(0.3f);
                _motor.IsEquipped = true;
                _motor.Weapon = _wantsToEquip;
            }
            else if (_wantsToUnequip)
                _motor.IsEquipped = false;
        }

        private void checkJump()
        {
            if (!_wantsToJump)
                return;

            if (_hasJumpAngle)
                _motor.InputJump(_jumpAngle, 1);
            else
                _motor.InputJump(_motor.transform.eulerAngles.y, 0);
        }

        private void checkRoll()
        {
            if (!_wantsToRoll)
                return;

            if (CancelHurt)
                _motor.CancelAndPreventGetHit(0.3f);

            var direction = Util.Vector(_rollAngle, 0);

            _motor.SetBodyTarget(_motor.transform.position + direction * 100);
            _motor.InputRoll(_rollAngle);
        }

        private void checkClimbOrVault()
        {
            if (_wantsToClimbOrVault == null)
                return;

            if (CancelHurt)
                _motor.CancelAndPreventGetHit(0.3f);

            _motor.InputClimbOrVault(_wantsToClimbOrVault);
        }

        private void checkMovement(float max)
        {
            var movement = MovementInput;

            if (movement.Magnitude > max)
                movement.Magnitude = max;

            _motor.InputMovement(movement);

            if (movement.IsMoving)
            {
                if (CancelHurt)
                    _motor.CancelAndPreventGetHit(0.1f);

                _motor.SetBodyTarget(BodyTargetInput);
                _motor.SetAimTarget(AimTargetInput);
            }
        }

        private void checkReload()
        {
            if (!_wantsToReload)
                return;

            var gun = _motor.ActiveWeapon.Gun;

            if (gun != null)
                _motor.InputReload();
        }

        private void checkCover()
        {
            if (AutoTakeCover)
                _motor.InputImmediateCoverSearch();

            if (_wantsToTakeCover)
                _motor.InputTakeCover();

            if ((AutoTakeCover && _coverDelayWait >= CoverEnterDelay) && _motor.PotentialCover != null)
                _motor.InputTakeCover();

            if (AutoTakeCover && CrouchNearCovers)
                _motor.InputCrouchNearCover();

            if (_motor.PotentialCover != null)
                _coverDelayWait += Time.deltaTime;
            else
                _coverDelayWait = 0;
        }

        private void checkGrenade()
        {
            if (_wantsToTakeGrenade)
            {
                if (CancelHurt)
                    _motor.CancelAndPreventGetHit(0.3f);

                _motor.InputTakeGrenade();
            }
        }

        private void checkCrouch()
        {
            if (!_wantsToCrouch)
                return;

            if (!_motor.IsCrouching)
                _motor.CancelAndPreventGetHit(0.3f);

            _motor.InputCrouch();
        }
    }
}