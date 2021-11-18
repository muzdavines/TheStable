using UnityEngine;

namespace CoverShooter
{
    public enum CharacterSpeed
    {
        Walk,
        Run,
        Sprint
    }

    /// <summary>
    /// Takes touch screen input and translates that to Character Motor commands. Player input is generated using canvas objects containing components like Touch Movement or Touch Aiming. Those canvas components must be linked with the Mobile Controller.
    /// When throwing a grenade, displays a grenade preview using prefabs containing Path Preview and Explosion Preview components.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(Actor))]
    public class MobileController : MonoBehaviour
    {
        /// <summary>
        /// Is the character always aiming in camera direction when not in cover.
        /// </summary>
        [Tooltip("Is the character always aiming in camera direction when not in cover.")]
        public bool AlwaysAim = true;

        /// <summary>
        /// Should the controller automatically reload once the weapon magazine is empty.
        /// </summary>
        [Tooltip("Should the controller automatically reload once the weapon magazine is empty.")]
        public bool AutoReload = true;

        /// <summary>
        /// Maximum target angle difference for the character to aim precisely at an enemy.
        /// </summary>
        [Tooltip("Maximum target angle difference for the character to aim precisely at an enemy.")]
        public float AutoAimPrecision = 30;

        /// <summary>
        /// Maximum target distance for the character to aim precisely at an enemy.
        /// </summary>
        [Tooltip("Maximum target distance for the character to aim precisely at an enemy.")]
        public float AutoAimDistance = 20;

        /// <summary>
        /// Maximum ngle difference between the current and intended orientations of the character for it to fire.
        /// </summary>
        [Tooltip("Maximum ngle difference between the current and intended orientations of the character for it to fire.")]
        public float FirePrecision = 25;

        /// <summary>
        /// Is the character now crouching.
        /// </summary>
        [Tooltip("Is the character now crouching.")]
        public bool IsCrouching = false;

        /// <summary>
        /// How long to maintain the aim direction before aiming torwards the direction of running.
        /// </summary>
        [Tooltip("How long to maintain the aim direction before aiming torwards the direction of running.")]
        public float PostFireDelay = 0.5f;

        /// <summary>
        /// Movement speed.
        /// </summary>
        [Tooltip("Movement speed.")]
        public CharacterSpeed Speed = CharacterSpeed.Run;

        /// <summary>
        /// Input is ignored when a disabler is active.
        /// </summary>
        [Tooltip("Input is ignored when a disabler is active.")]
        public GameObject Disabler;

        /// <summary>
        /// Forward direction relative to the camera.
        /// </summary>
        [HideInInspector]
        public Vector3 Forward = Vector3.forward;

        /// <summary>
        /// Should the movement value be used.
        /// </summary>
        [HideInInspector]
        public bool HasMovement;

        /// <summary>
        /// Movement direction, valid only if HasMovement is set to true.
        /// </summary>
        [HideInInspector]
        public Vector2 Movement = Vector2.zero;

        /// <summary>
        /// Should the aim value be used.
        /// </summary>
        [HideInInspector]
        public bool HasAiming;

        /// <summary>
        /// Magnitude of the aiming. Used for grenade angle.
        /// </summary>
        [HideInInspector]
        public float Magnitude;

        /// <summary>
        /// Can attempt to fire (depending on the gun settings) when aiming.
        /// </summary>
        [HideInInspector]
        public bool IsAllowedToFire;

        /// <summary>
        /// Direction to aim to, valid only if IsAiming is set to true.
        /// </summary>
        [HideInInspector]
        public Vector2 Aiming = Vector2.zero;

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

        private bool _wasAiming;
        private bool _wasInCover;
        private bool _willFireOrThrow;
        private bool _hasJustFiredOrThrown;
        private float _coverTimer;
        private float _postAimWait = 0;

        private Vector3[] _grenadePath = new Vector3[128];
        private int _grenadePathLength;
        private bool _hasGrenadePath;
        private float _fireAngle;

        private GameObject _explosionPreview;
        private GameObject _pathPreview;

        private CharacterMotor _motor;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
        }

        private void Update()
        {
            if (Disabler != null && Disabler.activeSelf)
                return;

            updateGrenadeAimAndPreview();

            if (_motor.PotentialCover != null)
                _motor.InputTakeCover();

            if (AlwaysAim)
            {
                if (_motor.IsInCover)
                    _motor.InputAimWhenLeavingCover();
                else
                    _motor.InputAim();
            }

            var gun = _motor.EquippedWeapon.Gun;

            if (AutoReload && gun != null && gun.LoadedBulletsLeft <= 0)
                _motor.InputReload();

            var hasMovement = HasMovement && Movement.sqrMagnitude > float.Epsilon;
            var hasAiming = HasAiming && Aiming.sqrMagnitude > float.Epsilon;

            if (hasMovement)
            {
                var movement = new CharacterMovement();

                var right = Vector3.Cross(Vector3.up, Forward);
                movement.Direction = Movement.x * right + Movement.y * Forward;

                switch (Speed)
                {
                    case CharacterSpeed.Walk: movement.Magnitude = 0.4f; break;
                    case CharacterSpeed.Run: movement.Magnitude = 1.0f; break;
                    case CharacterSpeed.Sprint: movement.Magnitude = 2.0f; break;
                }

                if (_motor.IsInCover)
                    if (_coverTimer < 0.5f && (movement.Direction.magnitude < 0.1f || Vector3.Dot(movement.Direction, _motor.Cover.Forward) > -0.1f))
                        movement.Direction = (movement.Direction + _motor.Cover.Forward).normalized;

                _motor.InputMovement(movement);

                if (!hasAiming && !_willFireOrThrow && !_hasJustFiredOrThrown)
                {
                    var target = _motor.transform.position + movement.Direction * 100;
                    _motor.SetBodyTarget(target);
                    _motor.SetAimTarget(target);
                }
            }
            else if (_motor.IsInCover && _coverTimer < 0.5f)
            {
                var movement = new CharacterMovement();
                movement.Magnitude = 1.0f;
                movement.Direction = _motor.Cover.Forward;
                _motor.InputMovement(movement);
            }

            if (_motor.HasGrenadeInHand)
            {
                if (hasAiming)
                {
                    var right = Vector3.Cross(Vector3.up, Forward);
                    var direction = Aiming.x * right + Aiming.y * Forward;
                    var target = transform.position + direction * 100 + Vector3.up;

                    _motor.SetBodyTarget(target);
                    _motor.SetAimTarget(target);

                    _wasAiming = true;
                }
                else if (_wasAiming)
                {
                    _willFireOrThrow = IsAllowedToFire;
                    _wasAiming = false;
                }
            }
            else
            {
                if (hasAiming)
                {
                    findTarget();

                    var forward = _motor.AimTarget - _motor.transform.position;
                    forward.y = 0;
                    forward.Normalize();

                    if (Vector3.Dot(forward, _motor.transform.forward) > 0.25f || _motor.IsInCover)
                        if (IsAllowedToFire && !(Speed == CharacterSpeed.Sprint && HasMovement))
                        {
                            _motor.InputAim();
                            _wasAiming = true;
                        }

                    if (gun != null && gun.FireOnMobileAim && IsAllowedToFire)
                        _willFireOrThrow = true;
                }
                else if (_wasAiming)
                {
                    if (IsAllowedToFire)
                    {
                        findTarget();
                        _willFireOrThrow = true;
                    }

                    _wasAiming = false;
                }
            }

            if (_hasJustFiredOrThrown)
            {
                _postAimWait += Time.deltaTime;

                if (_postAimWait >= PostFireDelay)
                {
                    _hasJustFiredOrThrown = false;
                    _postAimWait = 0;
                }
            }

            if (_willFireOrThrow)
            {
                if (_motor.HasGrenadeInHand)
                {
                    if (_hasGrenadePath)
                        _motor.InputThrowGrenade(_grenadePath, _grenadePathLength, _motor.Grenade.Step);

                    _willFireOrThrow = false;
                    _hasJustFiredOrThrown = true;
                    _postAimWait = 0;
                }
                else
                {
                    var delta = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, _fireAngle));

                    if (delta < FirePrecision)
                    {
                        _motor.InputFire();

                        _willFireOrThrow = false;
                        _hasJustFiredOrThrown = true;
                        _postAimWait = 0;
                    }
                }
            }

            if (_motor.IsInCover)
            {
                if (_motor.IsInCover)
                {
                    if (!_wasInCover)
                    {
                        _wasInCover = true;
                        _coverTimer = 0;
                    }
                    else
                        _coverTimer += Time.deltaTime;
                }
                else
                    _wasInCover = false;
            }
            else if (_motor.PotentialCover != null)
                _motor.InputTakeCover();

            if (IsCrouching)
                _motor.InputCrouch();
        }

        private void findTarget()
        {
            var right = Vector3.Cross(Vector3.up, Forward);
            var direction = Aiming.x * right + Aiming.y * Forward;
            var target = transform.position + direction * 100 + Vector3.up;

            var maxDot = -1f;
            var deltaDot = Mathf.Cos(AutoAimPrecision * Mathf.Deg2Rad);

            foreach (var other in Characters.AllAlive)
                if (other.Object != gameObject && other.IsAnyInSight(0.01f))
                {
                    var vector = other.Object.transform.position - transform.position;
                    vector.y = 0;

                    var distance = vector.magnitude;

                    if (distance > float.Epsilon && distance < AutoAimDistance)
                    {
                        var dot = Vector3.Dot(direction, vector / distance);

                        if (dot > deltaDot && dot > maxDot)
                        {
                            maxDot = dot;

                            var otherMotor = other.Object.GetComponent<CharacterMotor>();
                            target = other.Object.transform.position + Vector3.up * otherMotor.CurrentHeight * 0.8f;
                        }
                    }
                }

            _fireAngle = Util.HorizontalAngle(target - transform.position);

            _motor.SetBodyTarget(target);
            _motor.SetAimTarget(target);
        }

        private void updateGrenadeAimAndPreview()
        {
            if ((HasAiming || _wasAiming) && IsAllowedToFire && _motor.IsAlive && _motor.IsReadyToThrowGrenade && _motor.CurrentGrenade != null)
            {
                GrenadeDescription desc;
                desc.Gravity = _motor.Grenade.Gravity;
                desc.Duration = _motor.PotentialGrenade.Timer;
                desc.Bounciness = _motor.PotentialGrenade.Bounciness;

                var angle = Magnitude * 45f;
                var velocity = _motor.Grenade.MaxVelocity;

                if (angle < 45)
                    velocity *= Mathf.Clamp01((angle + 15) / 45f);

                _grenadePathLength = GrenadePath.Calculate(GrenadePath.Origin(_motor, _motor.BodyAngle),
                                                           _motor.BodyAngle,
                                                           angle,
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
        }
    }
}
