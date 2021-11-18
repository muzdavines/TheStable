using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Generates alerts for the AI to pick up on various gun events.
    /// </summary>
    [RequireComponent(typeof(BaseGun))]
    public class GunAlerts : MonoBehaviour, IGunListener
    {
        /// <summary>
        /// Distance at which fire can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which fire can be heard. Alert is not generated if value is zero or negative.")]
        public float Fire = 20;

        /// <summary>
        /// Distance at which a failed fire attempt can be heard.
        /// </summary>
        [Tooltip("Distance at which a failed fire attempt can be heard.")]
        public float EmptyFire = 20;

        /// <summary>
        /// Distance at which reloads can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which reloads can be heard. Alert is not generated if value is zero or negative.")]
        public float Reload = 10;

        private BaseGun _gun;
        private Actor _actor;
        private CharacterMotor _cachedMotor;

        private void Awake()
        {
            _gun = GetComponent<BaseGun>();
        }

        /// <summary>
        /// Broadcast pump alert.
        /// </summary>
        public void OnPump()
        {
            if (Reload <= float.Epsilon)
                return;

            checkActor();
            Alerts.Broadcast(transform.position, Reload, true, _actor, true);
        }

        /// <summary>
        /// Broadcast rechamber alert.
        /// </summary>
        public void OnRechamber()
        {
            if (Reload <= float.Epsilon)
                return;

            checkActor();
            Alerts.Broadcast(transform.position, Reload, true, _actor, true);
        }

        /// <summary>
        /// Broadcast eject event./
        /// </summary>
        public void OnEject()
        {
            if (Reload <= float.Epsilon)
                return;

            checkActor();
            Alerts.Broadcast(transform.position, Reload, true, _actor, true);
        }

        /// <summary>
        /// Generates a land alert.
        /// </summary>
        public void OnFire(float delay)
        {
            if (Fire <= float.Epsilon)
                return;

            checkActor();
            Alerts.Broadcast(transform.position, Fire, true, _actor, true);
        }
        
        /// <summary>
        /// Broadcast empty fire alert.
        /// </summary>
        public void OnEmptyFire()
        {
            if (EmptyFire <= float.Epsilon)
                return;

            checkActor();
            Alerts.Broadcast(transform.position, EmptyFire, true, _actor, true);
        }

        /// <summary>
        /// Generates a hurt alert.
        /// </summary>
        public void OnMagazineLoadStart()
        {
            if (Reload <= float.Epsilon)
                return;

            checkActor();
            Alerts.Broadcast(transform.position, Reload, true, _actor, true);
        }

        public void OnFullyLoaded() { }
        public void OnBulletLoad() { }
        public void OnPumpStart() { }
        public void OnBulletLoadStart() { }

        private void checkActor()
        {
            if (_gun.Character != _cachedMotor)
            {
                _cachedMotor = _gun.Character;
                _actor = _cachedMotor.GetComponent<Actor>();
            }
        }
    }
}
