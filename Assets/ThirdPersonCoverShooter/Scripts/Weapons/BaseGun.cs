using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Guns raycasts bullets, manage magazine and recoil.
    /// For player characters bullets originate at camera in order for player to be able to fire on targets they can see, even if there is a small obstacle in front of the gun. The fire origin is set by a camera. Since AI do not have Third Person Camera attached their bullets are fired starting from the Aim marker, which usually is at the end of the gun.
    /// Each weapon has two marker objects. Aim defines point of origin for AI bullets and is also used when rotating character’s arms till the marker points towards the target. Left Hand object marks the position for character’s left hand. Naming of left and right hands refers to the general case as character’s handedness can be swapped.
    /// The intended position of the left hand might differ in some animations, to handle that there are left hand marker overwrites you can use to set up IK for the left hand for some specific situations. Empty values are not used as overwrites.
    /// Currently there are two kinds of weapons, pistols and rifles. The type defines character animations when using the weapon.
    /// </summary>
    public abstract class BaseGun : BaseWeapon {
        #region Properties

        /// <summary>
        /// Point of creation for bullets in world space.
        /// </summary>
        public Vector3 Origin { get { return Aim == null ? transform.position : Aim.transform.position; } }

        /// <summary>
        /// Direction of bullets when created at the origin.
        /// </summary>
        public Vector3 Direction { get { return Aim == null ? transform.forward : Aim.transform.forward; } }

        /// <summary>
        /// Whether the gun can be used to perform a melee hit right now.
        /// </summary>
        public bool CanHit {
            get { return _hitWait <= 0; }
        }

        /// <summary>
        /// Returns true if the gun fired during the last update.
        /// </summary>
        public bool HasJustFired {
            get { return _hasJustFired; }
        }

        /// <summary>
        /// Returns true if the gun is allowed to fire.
        /// </summary>
        public bool IsAllowed {
            get { return _isAllowed; }
        }

        /// <summary>
        /// Renderer attached to the object.
        /// </summary>
        public Renderer Renderer {
            get { return _renderer; }
        }

        /// <summary>
        /// Origin to cast bullets from.
        /// </summary>
        public Vector3 RaycastOrigin {
            get { return _isUsingCustomRaycast ? _customRaycastOrigin : Origin; }
        }

        /// <summary>
        /// Target position at which bullets are fired at.
        /// </summary>
        public Vector3 RaycastTarget {
            get { return _isUsingCustomRaycast ? _customRaycastTarget : (Origin + Direction * Distance); }
        }

        /// <summary>
        /// Are raycast settings setup manually by some other component.
        /// </summary>
        public bool HasRaycastSetup {
            get { return _isUsingCustomRaycast; }
        }

        /// <summary>
        /// Can the gun be loaded with more bullets.
        /// </summary>
        public abstract bool CanLoad { get; }

        /// <summary>
        /// Number of bullets left in the gun.
        /// </summary>
        public abstract int LoadedBulletsLeft { get; }

        /// <summary>
        /// Is the gun fully loaded with bullets.
        /// </summary>
        public abstract bool IsFullyLoaded { get; }

        /// <summary>
        /// Load percentage for the ammo ui.
        /// </summary>
        public abstract float LoadPercentage { get; }

        #endregion

        #region Public fields


        /// <summary>
        /// Name of the gun to be display on the HUD.
        /// </summary>
        [Tooltip("Name of the gun to be display on the HUD.")]
        public string Name = "Gun";

        /// <summary>
        /// How many degrees should the camera FOV be reduced when using scope on the gun.
        /// </summary>
        [Tooltip("How many degrees should the camera FOV be reduced when using scope on the gun.")]
        public float Zoom = 30;

        /// <summary>
        /// Sprite that's displayed when zooming in.
        /// </summary>
        [Tooltip("Sprite that's displayed when zooming in.")]
        public Sprite Scope;

        /// <summary>
        /// Rate of fire in shots per second.
        /// </summary>
        [Tooltip("Rate of fire in shots per second.")]
        [Range(0, 1000)]
        public float Rate = 7;

        /// <summary>
        /// Bullets fired per single shot.
        /// </summary>
        [Tooltip("Bullets fired per single shot.")]
        public int BulletsPerShot = 1;

        /// <summary>
        /// If firing multiple bullets per shot, should only a single bullet be removed from the inventory.
        /// </summary>
        [Tooltip("If firing multiple bullets per shot, should only a single bullet be removed from the inventory.")]
        public bool ConsumeSingleBulletPerShot = true;

        /// <summary>
        /// Maximum distance of a bullet hit. Objects further than this value are ignored.
        /// </summary>
        [Tooltip("Maximum distance of a bullet hit. Objects further than this value are ignored.")]
        public float Distance = 50;


        /// <summary>
        /// Maximum degrees of error the gun can make when firing.
        /// </summary>
        [Tooltip("Maximum degrees of error the gun can make when firing.")]
        public float Error = 0;

        /// <summary>
        /// Should the gun be reloaded automatically when the magazine is empty.
        /// </summary>
        [Tooltip("Should the gun be reloaded automatically when the magazine is empty.")]
        public bool AutoReload = false;

        /// <summary>
        /// Is the gun reloaded with whole magazines or bullet by bullet.
        /// </summary>
        [Tooltip("Is the gun reloaded with whole magazines or bullet by bullet.")]
        public bool ReloadWithMagazines = true;

        /// <summary>
        /// If reloading bullet by bullet, can the gun be fired during reload.
        /// </summary>
        [Tooltip("If reloading bullet by bullet, can the gun be fired during reload.")]
        public bool CanInterruptBulletLoad = true;

        /// <summary>
        /// After a new magazine or the last bullet is loaded, should the gun be pumped.
        /// </summary>
        [Tooltip("After a new magazine or the last bullet is loaded, should the gun be pumped.")]
        public bool PumpAfterFinalLoad = false;

        /// <summary>
        /// Should the gun be pumped after each bullet load.
        /// </summary>
        [Tooltip("Should the gun be pumped after each bullet load.")]
        public bool PumpAfterBulletLoad = false;

        /// <summary>
        /// Should the gun be pumped after firing a shot.
        /// </summary>
        [Tooltip("Should the gun be pumped after firing a shot.")]
        public bool PumpAfterFire = false;

        /// <summary>
        /// Will the character fire by just aiming the mobile controller.
        /// </summary>
        [Tooltip("Will the character fire by just aiming the mobile controller.")]
        public bool FireOnMobileAim = true;

        /// <summary>
        /// Should the laser be visible only when zooming.
        /// </summary>
        [Tooltip("Should the laser be visible only when zooming.")]
        public bool LaserOnlyOnZoom = true;

        /// <summary>
        /// Should a debug ray be displayed.
        /// </summary>
        [Tooltip("Should a debug ray be displayed.")]
        public bool DebugAim = false;

        /// <summary>
        /// Link to the object that controls the aiming direction.
        /// </summary>
        [Tooltip("Link to the object that controls the aiming direction.")]
        public GameObject Aim;

        /// <summary>
        /// Object to be instantiated as a bullet.
        /// </summary>
        [Tooltip("Object to be instantiated as a bullet.")]
        public GameObject Bullet;

        /// <summary>
        /// Link to the object that controls the position of character's left hand relative to the weapon.
        /// </summary>
        [Tooltip("Link to the object that controls the position of character's left hand relative to the weapon.")]
        public Transform LeftHandDefault;

        /// <summary>
        /// Time in seconds between hits that the target character will respond to with hurt animations.
        /// </summary>
        [Tooltip("Time in seconds between hits that the target character will respond to with hurt animations.")]
        public float DamageResponseWaitTime = 1;

        /// <summary>
        /// Should the gun's crosshair be used instead of the one set in the Crosshair component.
        /// </summary>
        [Tooltip("Should the gun's crosshair be used instead of the one set in the Crosshair component.")]
        public bool UseCustomCrosshair;

        /// <summary>
        /// Custom crosshair settings to override the ones set in the Crosshair component. Used only if UseCustomCrosshair is enabled.
        /// </summary>
        [Tooltip("Custom crosshair settings to override the ones set in the Crosshair component. Used only if UseCustomCrosshair is enabled.")]
        public CrosshairSettings CustomCrosshair = CrosshairSettings.Default();

        /// <summary>
        /// Settings that manage gun's recoil behaviour.
        /// </summary>
        [Tooltip("Settings that manage gun's recoil behaviour.")]
        public GunRecoilSettings Recoil = GunRecoilSettings.Default();

        /// <summary>
        /// Links to objects that overwrite the value in LeftHand based on the gameplay situation.
        /// </summary>
        [Tooltip("Links to objects that overwrite the value in LeftHand based on the gameplay situation.")]
        public HandOverwrite LeftHandOverwrite;

        /// <summary>
        /// Force the pistol to use this laser instead of finding one on its own.
        /// </summary>
        [Tooltip("Force the pistol to use this laser instead of finding one on its own.")]
        public Laser LaserOverwrite;

        /// <summary>
        /// Owning object with a CharacterMotor component.
        /// </summary>
        [HideInInspector]
        public CharacterMotor Character;

        #endregion

        #region Actions

        /// <summary>
        /// Event executed at the beginning of a magazine load animation.
        /// </summary>
        public Action MagazineLoadStarted;

        /// <summary>
        /// Event executed after the gun has been fully loaded.
        /// </summary>
        public Action FullyLoaded;

        /// <summary>
        /// Event executed at the beginning of a bullet load animation.
        /// </summary>
        public Action BulletLoadStarted;

        /// <summary>
        /// Event executed after a bullet has been loaded.
        /// </summary>
        public Action BulletLoaded;

        /// <summary>
        /// Event executed at the start of a pump animation.
        /// </summary>
        public Action PumpStarted;

        /// <summary>
        /// Event executed at the end of a pump animation.
        /// </summary>
        public Action Pumped;

        /// <summary>
        /// Event executed after a successful fire.
        /// </summary>
        public Action Fired;

        /// <summary>
        /// Event executed every time there is an attempt to fire with no bullets in the gun.
        /// </summary>
        public Action EmptyFire;

        /// <summary>
        /// Event executed after a series of bullet fires as started.
        /// </summary>
        public Action FireStarted;

        /// <summary>
        /// Event executed after a series of bullet fires has stopped.
        /// </summary>
        public Action FireStopped;

        /// <summary>
        /// Event executed after a bullet hit something.
        /// </summary>
        public Action<Hit> SuccessfulyHit;

        #endregion

        #region Private fields

        private Renderer _renderer;

        private bool _hasJustFired;

        private bool _isUsingCustomRaycast;
        private Vector3 _customRaycastOrigin;
        private Vector3 _customRaycastTarget;

        private float _fireWait = 0;
        private bool _isGoingToFire;
        private bool _isFiringOnNextUpdate;
        private bool _isAllowed;
        private bool _wasAllowedAndFiring;

        private float _hitWait = 0;

        private RaycastHit[] _hits = new RaycastHit[16];

        private Laser _laser;

        private bool _isIgnoringSelf = true;
        private bool _hasFireCondition;
        private int _fireConditionSide;

        private float _additionalError = 0;
        private float _errorMultiplier = 1;

        private bool _hasUpdatedThisFrame;
        private bool _hasManuallyUpdated;

        private IGunListener[] _listeners;

        #endregion

        #region Commands

        /// <summary>
        /// Command to use the weapon.
        /// </summary>
        public void ToUse() {
            print(transform.root.name + " TO USE");
            TryFireNow();
        }

        #endregion

        #region Events

        /// <summary>
        /// Notified of a magazine load start by the CharacterMotor.
        /// </summary>
        public virtual void OnMagazineLoadStart() {
            if (MagazineLoadStarted != null)
                MagazineLoadStarted();
        }

        /// <summary>
        /// Notified of a magazine load start by the CharacterMotor.
        /// </summary>
        public virtual void OnBulletLoadStart() {
            if (BulletLoadStarted != null)
                BulletLoadStarted();
        }

        /// <summary>
        /// Notified of a magazine load start by the CharacterMotor.
        /// </summary>
        public virtual void OnPumpStart() {
            if (PumpStarted != null)
                PumpStarted();
        }

        /// <summary>
        /// Notified of a magazine load start by the CharacterMotor.
        /// </summary>
        public virtual void OnPumped() {
            if (Pumped != null)
                Pumped();
        }

        #endregion

        #region Behaviour

        /// <summary>
        /// Get the LineRenderer if there is one.
        /// </summary>
        protected virtual void Start() {
            _laser = LaserOverwrite;

            if (_laser == null)
                _laser = GetComponent<Laser>();

            if (_laser == null)
                _laser = GetComponentInChildren<Laser>();

            if (_laser == null && transform.parent != null) {
                _laser = transform.parent.GetComponentInChildren<Laser>();

                if (_laser != null && _laser.GetComponent<BaseGun>() != null)
                    _laser = null;
            }
        }

        private void OnValidate() {
            Distance = Mathf.Max(0, Distance);
        }

        /// <summary>
        /// Finds the renderer.
        /// </summary>
        protected virtual void Awake() {
            _renderer = GetComponent<Renderer>();
            _listeners = Util.GetInterfaces<IGunListener>(gameObject);
        }

        private void LateUpdate() {
            if (!_hasManuallyUpdated) {
                _hasUpdatedThisFrame = true;
                Frame();
            }
            else {
                _hasUpdatedThisFrame = false;
                _hasManuallyUpdated = false;
            }
        }

        protected IGunListener[] Listeners { get { return _listeners; } }
        private int _currentMoveIndex;
        public int currentMoveIndex { get { return _currentMoveIndex; } set { _currentMoveIndex = value;  myCharacter.currentMoveIndex = value; } }
        public float nextFire;
        protected virtual void Frame() {
            _hasJustFired = false;

            if (_isGoingToFire)
                _isFiringOnNextUpdate = true;

            if (_hitWait >= 0)
                _hitWait -= Time.deltaTime;

            if (DebugAim) {
                Debug.DrawLine(Origin, Origin + (RaycastTarget - Origin).normalized * Distance, Color.red);

                if (_isUsingCustomRaycast)
                    Debug.DrawLine(_customRaycastOrigin, _customRaycastTarget, Color.green);
            }

            // Notify character if the trigger is pressed. Used to make faces.
            {
                var isAllowedAndFiring = _isGoingToFire && _isAllowed;

                if (Character != null) {
                    if (isAllowedAndFiring && !_wasAllowedAndFiring) {
                        Character.NotifyStartGunFire();
                        if (FireStarted != null) FireStarted.Invoke();
                    }

                    if (!isAllowedAndFiring && _wasAllowedAndFiring) {
                        Character.NotifyStopGunFire();
                        if (FireStopped != null) FireStopped.Invoke();
                    }
                }

                _wasAllowedAndFiring = isAllowedAndFiring;
            }

            _fireWait -= Time.deltaTime;

            // Check if the trigger is pressed.
            print(transform.root.name + " is trying to Fire; IsFiringNextUpdate: " + _isFiringOnNextUpdate + "// Is Allowed: " + _isAllowed);
            if (currentMove == null) { currentMove = myCharacter.activeMoves[currentMoveIndex]; }
            print("Time: " + Time.time + "   Next Fire: " + nextFire);
            if (_isFiringOnNextUpdate && _isAllowed && Time.time >= nextFire) {
                // Time in seconds between bullets.
                var fireDelay = 1.0f / Rate;
                
                
                print("Time :" + Time.time + " Fire Wait: " + _fireWait);
                // Fire all bullets in this frame.
                
                    var hasFired = false;

                    for (int i = 0; i < BulletsPerShot; i++) {
                        if (LoadedBulletsLeft <= 0)
                            break;

                        if (fire(!ConsumeSingleBulletPerShot)) {
                            nextFire = Time.time + currentMove.cooldown;
                            hasFired = true;
                        }
                    }

                    if (hasFired && ConsumeSingleBulletPerShot)
                        Consume();

                    if (hasFired) {
                        for (int i = 0; i < _listeners.Length; i++)
                            _listeners[i].OnFire(0);

                        if (Fired != null) Fired.Invoke();

                        if (Character != null) {
                            if (PumpAfterFire)
                                Character.InputPump(0.1f);

                            Character.InputRecoil(Recoil.Vertical, Recoil.Horizontal);
                            ThirdPersonCamera.Shake(Character, Recoil.ShakeIntensity, Recoil.ShakeTime);
                        }
                    }
                    else {
                        for (int i = 0; i < _listeners.Length; i++)
                            _listeners[i].OnEmptyFire();

                        if (EmptyFire != null) EmptyFire.Invoke();
                    }

                
                    _fireWait += fireDelay;
                    _isGoingToFire = false;
                
            }

            _isFiringOnNextUpdate = false;
            _isUsingCustomRaycast = false;

            // Clamp fire delay timer.
            if (_fireWait < 0) _fireWait = 0;

            if (_laser != null && Character != null)
                _laser.Alpha = Character.IsZooming ? 1 : 0;

            adjustLaser();

            _additionalError = 0;
            _errorMultiplier = 1;
        }

        #endregion

        #region Notify methods

        public void NotifyRechamber() {
            for (int i = 0; i < _listeners.Length; i++)
                _listeners[i].OnRechamber();
        }

        public void NotifyEject() {
            for (int i = 0; i < _listeners.Length; i++)
                _listeners[i].OnEject();
        }

        public void NotifyPumpStart() {
            OnPumpStart();

            for (int i = 0; i < _listeners.Length; i++)
                _listeners[i].OnPumpStart();
        }

        public void NotifyPump() {
            for (int i = 0; i < _listeners.Length; i++)
                _listeners[i].OnPump();
        }

        public void NotifyMagazineLoadStart() {
            OnMagazineLoadStart();

            for (int i = 0; i < _listeners.Length; i++)
                _listeners[i].OnMagazineLoadStart();
        }

        public void NotifyBulletLoadStart() {
            OnBulletLoadStart();

            for (int i = 0; i < _listeners.Length; i++)
                _listeners[i].OnBulletLoadStart();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Sets the gun to ignore hitting it's owner.
        /// </summary>
        public void IgnoreSelf(bool value = true) {
            _isIgnoringSelf = value;
        }

        /// <summary>
        /// Sets the gun to not fire if aiming at a friend.
        /// </summary>
        public void SetFireCondition(int side) {
            _hasFireCondition = true;
            _fireConditionSide = side;
        }

        /// <summary>
        /// Sets the gun to fire in any condition.
        /// </summary>
        public void CancelFireCondition() {
            _hasFireCondition = false;
        }

        /// <summary>
        /// Returns a game object the gun is currently aiming at.
        /// </summary>
        public GameObject FindCurrentAimedTarget() {
            var hit = Raycast();

            if (hit.collider != null)
                return hit.collider.gameObject;
            else
                return null;
        }

        /// <summary>
        /// Returns a game object with CharacterHealth the gun is currently aiming at.
        /// </summary>
        public GameObject FindCurrentAimedHealthTarget() {
            return getHealthTarget(FindCurrentAimedTarget());
        }

        /// <summary>
        /// Reload whole magazine.
        /// </summary>
        public abstract bool LoadMagazine();

        /// <summary>
        /// Add a single bullet to the magazine.
        /// </summary>
        public abstract bool LoadBullet();

        /// <summary>
        /// Finds an object and a hit position a bullet would hit if fired.
        /// </summary>
        public RaycastHit Raycast() {
            bool isFriend;
            return Raycast(RaycastOrigin, (RaycastTarget - RaycastOrigin).normalized, out isFriend, false);
        }

        /// <summary>
        /// Finds an object and a hit position a bullet would hit if fired. Checks if it is a friend.
        /// </summary>
        public RaycastHit Raycast(Vector3 origin, Vector3 direction, out bool isFriend, bool friendCheck) {
            RaycastHit closestHit = new RaycastHit();
            float closestDistance = Distance * 10;

            var minDistance = 0f;

            if (_isUsingCustomRaycast)
                minDistance = Vector3.Distance(Origin, RaycastOrigin);

            if (minDistance > 0.5f)
                minDistance -= 0.5f;

            isFriend = false;
            var count = Physics.RaycastNonAlloc(origin, direction, _hits, Distance);

            for (int i = 0; i < count; i++) {
                var hit = _hits[i];

                if (Character != null && Util.InHiearchyOf(hit.collider.gameObject, Character.gameObject))
                    continue;

                if (hit.distance < closestDistance && hit.distance > minDistance) {
                    var isOk = true;
                    var isShield = false;

                    if (hit.collider.isTrigger) {
                        if (BodyPartHealth.Contains(hit.collider.gameObject))
                            isOk = true;
                        else {
                            var shield = BulletShield.Get(hit.collider.gameObject);

                            if (shield != null) {
                                if (Vector3.Dot(shield.transform.forward, hit.normal) >= -0.2f) {
                                    isOk = true;
                                    isShield = true;
                                }
                                else
                                    isOk = false;
                            }
                            else
                                isOk = false;
                        }
                    }
                    else {
                        var health = CharacterHealth.Get(hit.collider.gameObject);

                        if (health != null)
                            isOk = health.IsRegisteringHits;
                    }

                    if (isOk) {
                        if (!isShield && (_isIgnoringSelf || _hasFireCondition) && friendCheck) {
                            var root = getHealthTarget(hit.collider.gameObject);

                            if (root != null) {
                                if (_isIgnoringSelf && Character != null && root == Character.gameObject)
                                    isFriend = true;
                                else if (_hasFireCondition) {
                                    var actor = Actors.Get(root);

                                    if (actor != null)
                                        isFriend = actor.Side == _fireConditionSide;
                                    else
                                        isFriend = false;
                                }
                                else
                                    isFriend = false;
                            }
                            else
                                isFriend = false;
                        }

                        closestHit = hit;
                        closestDistance = hit.distance;
                    }
                }
            }

            return closestHit;
        }

        /// <summary>
        /// Sets the gun to try firing during the next update.
        /// Gun fires only when both fire mode is on and the gun is allowed to fire.
        /// </summary>
        public void TryFireNow() {
            print("Inside TryFireNow");
            _isFiringOnNextUpdate = true;
        }

        /// <summary>
        /// Sets the fire mode on. It stays on until CancelFire() is called or the gun has fired.
        /// Gun fires only when both fire mode is on and the gun is allowed to fire.
        /// </summary>
        public void FireWhenReady() {
            print("Fire When Ready: " + transform.root.name);
            _isGoingToFire = true;
        }

        /// <summary>
        /// Sets the fire mode off.
        /// </summary>
        public void CancelFire() {
            _isGoingToFire = false;
        }

        /// <summary>
        /// Sets whether the gun is allowed to fire. Manipulated when changing weapons or a reload animation is playing.
        /// </summary>
        /// <param name="value"></param>
        public void Allow(bool value) {
            //print("Allow: " + value);
            _isAllowed = value;
        }

        /// <summary>
        /// Sets the position from which bullets are spawned. The game usually sets it as the camera position.
        /// </summary>
        public void SetupRaycastThisFrame(Vector3 origin, Vector3 target) {
            _isUsingCustomRaycast = true;
            _customRaycastOrigin = origin;
            _customRaycastTarget = target;
        }

        /// <summary>
        /// Sets the aim error in degrees for the next frame. Errors are stacked.
        /// </summary>
        public void AddErrorThisFrame(float degrees) {
            _additionalError += degrees;
        }

        /// <summary>
        /// Sets the base error (Error property) multiplier for this frame.
        /// </summary>
        public void SetBaseErrorMultiplierThisFrame(float multiplier) {
            _errorMultiplier = multiplier;
        }

        /// <summary>
        /// Call the update method manually. Performed by the CharacterMotor, in order to fire the weapon after the weapon has performed it's IK.
        /// </summary>
        public void UpdateManually() {
            _hasManuallyUpdated = true;

            if (!_hasUpdatedThisFrame)
                Frame();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Consume a single bullet.
        /// </summary>
        protected abstract void Consume();

        private void adjustLaser() {
            // Adjust the laser.
            if (_laser != null) {
                var origin = Origin;
                var direction = Direction;

                bool isFriend;
                var hit = Raycast(origin, direction, out isFriend, false);

                if (hit.collider == null)
                    _laser.Setup(origin, origin + direction * Distance);
                else
                    _laser.Setup(origin, hit.point);
            }
        }

        /// <summary>
        /// Finds best CharacterHealth gameobject.
        /// </summary>
        private GameObject getHealthTarget(GameObject target) {
            while (target != null) {
                var health = CharacterHealth.Get(target);

                if (health != null) {
                    if (health.Health <= float.Epsilon)
                        target = null;

                    break;
                }

                var parent = target.transform.parent;

                if (parent != null)
                    target = parent.gameObject;
                else
                    target = null;
            }

            return target;
        }

        /// <summary>
        /// Calculates direction to target from origin.
        /// </summary>
        private Vector3 calculateRaycastDirection() {
            var direction = (RaycastTarget - RaycastOrigin).normalized;

            var error = (_additionalError + Error * _errorMultiplier) * 0.5f;
            var x = UnityEngine.Random.Range(-error, error);
            var y = UnityEngine.Random.Range(-error, error);

            var up = Vector3.up;
            if (direction.y > 0.99f || direction.y < -0.99f)
                up = Vector3.forward;

            var right = Vector3.Cross(up, direction);

            if (error > 0.1f && x * x + y * y >= error * error) {
                var magnitude = Mathf.Sqrt(x * x + y * y) * error;
                x /= magnitude;
                y /= magnitude;

                x *= UnityEngine.Random.Range(-1f, 1f);
                y *= UnityEngine.Random.Range(-1f, 1f);
            }

            return Quaternion.AngleAxis(x, right) * Quaternion.AngleAxis(y, up) * direction;
        }

        /// <summary>
        /// Cast a single bullet using raycasting.
        /// </summary>
        private bool fire(bool consume) {

            Allow(false);
            
            Character.GetComponent<Animator>().SetTrigger("FireShot");
            return true;
        }
        DebugRayHelper debugRayHelper;
        public void AnimFireTrigger() {

            bool isFriend;
            var direction = calculateRaycastDirection();
            var hit = Raycast(RaycastOrigin, direction, out isFriend, true);
            if (debugRayHelper == null) {
                debugRayHelper = gameObject.AddComponent<DebugRayHelper>();
            }
            debugRayHelper.SetRay(RaycastOrigin, direction);
            if (Character != null)
                Character.KeepAiming();

            if (!isFriend) {
                var end = hit.point;

                
                    Consume();

                if (hit.collider == null)
                    end = RaycastOrigin + Distance * direction;

                HitType type;

                switch (Type) {
                    case WeaponType.Pistol: type = HitType.Pistol; break;
                    case WeaponType.Rifle: type = HitType.Rifle; break;
                    case WeaponType.Shotgun: type = HitType.Shotgun; break;
                    case WeaponType.Sniper: type = HitType.Sniper; break;

                    default:
                        type = HitType.Pistol;
                        Debug.Assert(false, "Invalid gun type");
                        break;
                }

                var hitStruct = new Hit(hit.point, -direction, 0, Character.gameObject, hit.collider == null ? null : hit.collider.gameObject, type, DamageResponseWaitTime);

                if (Bullet != null) {
                    var bullet = GameObject.Instantiate(Bullet);
                    bullet.transform.position = Origin;
                    bullet.transform.LookAt(end);

                    var projectile = bullet.GetComponent<Projectile>();
                    var vector = end - Origin;

                    var trail = bullet.GetComponent<TrailRenderer>();
                    if (trail == null) trail = bullet.GetComponentInChildren<TrailRenderer>();

                    if (trail != null)
                        trail.Clear();

                    if (projectile != null) {
                        projectile.Distance = vector.magnitude;
                        projectile.Direction = vector.normalized;

                        if (hit.collider != null) {
                            projectile.Target = hit.collider.gameObject;
                            projectile.Hit = hitStruct;
                        }

                    }
                    else if (hit.collider != null)
                        hit.collider.SendMessage("OnHit", hitStruct, SendMessageOptions.DontRequireReceiver);

                    bullet.SetActive(true);
                }
                else if (hit.collider != null) {
                    print("Hit: " + hit.collider.transform.name);
                    var hitHealth = hit.collider.GetComponentInChildren<CharacterHealth>();
                    if (hitHealth == null) {
                        print("Ranged Hit has no Character Health Component: " + hit.collider.transform.name);
                    } else {
                        currentMove = Character.GetComponent<MissionCharacter>().activeMoves[currentMoveIndex];
                        var normal = (Character.transform.position - hit.transform.position).normalized;
                        var thisHitStruct = new Hit(hit.collider.ClosestPointOnBounds(transform.position), normal, 0, Character.gameObject, hit.transform.gameObject, type, DamageResponseWaitTime, currentMove);
                        print("Ranged Hit success! " + currentMove.staminaDamage);
                        hitHealth.GetComponent<CharacterMotor>().OnHit(thisHitStruct);
                        //hitHealth.SendMessage("OnHit", thisHitStruct, SendMessageOptions.DontRequireReceiver);
                    }

                }


                _hasJustFired = true;
                return;
            }
            else {
                _hasJustFired = true;
                return;
            }
        }
        public void AnimEndTrigger() {
            Allow(true);
            currentMove = GetNextMove();
            myCharacter.currentMoveIndex = currentMoveIndex;
            print("Changing Move to: " + currentMove.moveType.ToString());
        }
        public Move GetNextMove() {
            currentMoveIndex++;
            if (currentMoveIndex >= myCharacter.activeMoves.Length) {
                currentMoveIndex = 0;
                return myCharacter.activeMoves[currentMoveIndex];
            }
            if (myCharacter.activeMoves[currentMoveIndex] == null) {
                return GetNextMove();
            }
            return myCharacter.activeMoves[currentMoveIndex];
        }
        #endregion
    }
}