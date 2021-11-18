using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Moves the object through the air and causes an explosion event after certain amount of time passes. By default causes damage to surrounding objects with CharacterHealth.
    /// </summary>
    public class Grenade : MonoBehaviour
    {
        /// <summary>
        /// Manages flight and explosion of a grenade. Flight and collisions are performed using raycasts. Grenades can bounce. Upon explosion a prefab is instantiated on grenades location. Grenade affects Character Health and Body Part Health objects in its explosion area. Damage is highest in the epicentre and decreases linearly to zero on the edge of the explosion area.
        /// </summary>
        public Actor Attacker
        {
            get { return _attacker; }
        }

        float launchTime = 0;

        /// <summary>
        /// Explosion prefab to instantiate on detonation.
        /// </summary>
        [Tooltip("Explosion prefab to instantiate on detonation.")]
        public GameObject Explosion;

        /// <summary>
        /// Distance from the explosion center where damage reaches 0.
        /// </summary>
        [Tooltip("Distance from the explosion center where damage reaches 0.")]
        public float ExplosionRadius = 4.5f;

        /// <summary>
        /// Damage done at the center of explosion.
        /// </summary>
        [Tooltip("Damage done at the center of explosion.")]
        public float CenterDamage = 150;

        /// <summary>
        /// Fraction of velocity retained when bouncing from a surface.
        /// </summary>
        [Tooltip("Fraction of velocity retained when bouncing from a surface.")]
        public float Bounciness = 0.3f;

        /// <summary>
        /// Explosion timer in seconds counted from activation.
        /// </summary>
        [Tooltip("Explosion timer in seconds counted from activation.")]
        public float Timer = 3;

        /// <summary>
        /// Camera shake duration when exploded.
        /// </summary>
        [Tooltip("Camera shake duration when exploded.")]
        public float ShakeDuration = 0.5f;

        /// <summary>
        /// Camera shake intensity when close to the camera.
        /// </summary>
        [Tooltip("Camera shake intensity when close to the camera.")]
        public float ShakeIntensity = 100;

        /// <summary>
        /// Prefab to instantiate to display grenade explosion preview.
        /// </summary>
        [Tooltip("Prefab to instantiate to display grenade explosion preview.")]
        public GameObject ExplosionPreview;

        /// <summary>
        /// Time in seconds since the activation to display the explosion preview.
        /// </summary>
        [Tooltip("Time in seconds since the activation to display the explosion preview.")]
        public float PreviewTime;

        private bool _hasExploded = false;

        private bool _isActivated = false;
        private bool _isActivating = false;
        private float _activateTimer = 0;
        private Actor _attacker;

        private float _timer = 0;

        private bool _isFlying;
        private Vector3 _velocity;
        private Vector3 _offset;
        private float _gravity;
        private Vector3 _target;
        private GameObject _preview;

        private void OnEnable()
        {
            launchTime = Mathf.Infinity;
            if (_isActivated)
                GrenadeList.Register(this);
        }

        private void OnDisable()
        {
            if (_preview != null)
            {
                GameObject.Destroy(_preview);
                _preview = null;
            }

            GrenadeList.Unregister(this);
        }

        /// <summary>
        /// Launched the grenade on flight with giver parameters.
        /// </summary>
        public void Fly(Vector3 origin, Vector3 velocity, float gravity)
        {
            _isFlying = true;
            launchTime = Time.time;
            print("Grenade Launch Time: " + launchTime + "  " + transform.GetInstanceID());
            _offset = transform.position - origin;

            if (_offset.magnitude > 0.5f)
                _offset = _offset.normalized * 0.5f;

            _velocity = velocity;
            _gravity = gravity;
        }

        /// <summary>
        /// Sets the grenade to explode on collision.
        /// </summary>
        public void Activate(Actor attacker, float delay = 0)
        {
            if (delay <= float.Epsilon)
            {
                _isActivated = true;
                _isActivating = false;
                _timer = Timer;
                GrenadeList.Register(this);
            }
            else
            {
                _isActivating = true;
                _activateTimer = delay;
            }

            _attacker = attacker;
        }

        private void Update()
        {
            transform.localScale = Vector3.one;
            if (_isActivating)
            {
                _activateTimer -= Time.deltaTime;

                if (_activateTimer < float.Epsilon)
                {
                    _isActivating = false;
                    _isActivated = true;
                    GrenadeList.Register(this);
                    _timer = Timer;
                }
            }

            if (_isActivated)
            {
                
                if (_timer < float.Epsilon)
                    explode();
                else
                {
                    _timer -= Time.deltaTime;

                    if (_timer < Timer - PreviewTime)
                    {
                        if (_preview == null && ExplosionPreview != null)
                        {
                            _preview = GameObject.Instantiate(ExplosionPreview);
                            _preview.transform.SetParent(null);
                            _preview.SetActive(true);
                        }

                        if (_preview != null)
                        {
                            _preview.transform.localScale = Vector3.one * ExplosionRadius * 2;
                            _preview.transform.position = transform.position;
                        }
                    }
                    else if (_preview != null)
                    {
                        GameObject.Destroy(_preview);
                        _preview = null;
                    }
                }
            }

            if (_isFlying)
            {
                var position = transform.position;

                var previousOffset = _offset;
                _offset = Util.Lerp(_offset, Vector3.zero, 2);
                position += _offset - previousOffset;

                GrenadePath.Step(ref position, ref _velocity, Time.deltaTime, _gravity, Bounciness);

                transform.position = position;
            }
        }

        private void explode()
        {
            if (_hasExploded || !_isActivated)
                return;

            ThirdPersonCamera.Shake(transform.position, ShakeIntensity, ShakeDuration);

            _hasExploded = true;

            if (Explosion != null)
            {
                var alert = Explosion.GetComponent<Alert>();

                if (alert != null)
                    alert.Generator = _attacker;

                var particle = GameObject.Instantiate(Explosion, transform.position, Quaternion.identity, null);
                particle.SetActive(true);
            }

            var colliders = Physics.OverlapSphere(transform.position, ExplosionRadius);
            var count = Physics.OverlapSphereNonAlloc(transform.position, ExplosionRadius, Util.Colliders);

            for (int i = 0; i < count; i++)
            {
                var collider = Util.Colliders[i];

                if (!collider.isTrigger)
                {
                    var part = collider.GetComponent<BodyPartHealth>();

                    if (part == null)
                    {
                        var closest = collider.transform.position;

                        if (collider.GetType() == typeof(MeshCollider))
                            if (((MeshCollider)collider).convex)
                                closest = collider.ClosestPoint(transform.position);

                        var vector = transform.position - closest;
                        var distance = vector.magnitude;

                        if (distance < ExplosionRadius)
                        {
                            Vector3 normal;

                            if (distance > float.Epsilon)
                                normal = vector / distance;
                            else
                                normal = (closest - collider.transform.position).normalized;

                            Apply(collider.gameObject, closest, normal, (1 - distance / ExplosionRadius));
                        }
                    }
                }
            }

            GameObject.Destroy(gameObject);
        }

        protected virtual void Apply(GameObject target, Vector3 position, Vector3 normal, float fraction)
        {
            var damage = CenterDamage * fraction;

            if (damage > float.Epsilon)
            {
                var hit = new Hit(position, normal, damage, null, target, HitType.Explosion, 0);
                target.SendMessage("OnHit", hit, SendMessageOptions.DontRequireReceiver);
            }
        }
        public bool explodeOnContact;
        public void OnTriggerEnter(Collider other) {
            print("Grenade: " + other.name + "  "+Time.time+ "  "+transform.GetInstanceID());
            if (!explodeOnContact || Time.time < launchTime + 1) {
                return;
            }
            _isActivated = true;
            explode();
        }
    }
}
