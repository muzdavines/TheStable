using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Generates alerts on various character events.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    public class AIAlerts : MonoBehaviour
    {
        /// <summary>
        /// Distance at which the alerted AI can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which the alerted AI can be heard. Alert is not generated if value is zero or negative.")]
        public float Alert = 10;

        /// <summary>
        /// Distance at which the scared AI can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which the scared AI can be heard. Alert is not generated if value is zero or negative.")]
        public float Fear = 10;

        /// <summary>
        /// Distance at which the AI calling for cops can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which the AI calling for cops can be heard. Alert is not generated if value is zero or negative.")]
        public float CopCall = 10;

        /// <summary>
        /// Distance at which the AI calling for backup can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which the AI calling for backup can be heard. Alert is not generated if value is zero or negative.")]
        public float BackupCall = 10;

        /// <summary>
        /// Distance at which the AI announcing a cover switch can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which the AI announcing a cover switch can be heard. Alert is not generated if value is zero or negative.")]
        public float CoverSwitch = 10;

        /// <summary>
        /// Distance at which the AI announcing an assault can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which the AI announcing an assault can be heard. Alert is not generated if value is zero or negative.")]
        public float Assault = 10;

        private Actor _actor;

        private void Awake()
        {
            _actor = GetComponent<Actor>();
        }

        /// <summary>
        /// Generates an alert alert.
        /// </summary>
        public void OnAlerted()
        {
            if (_actor.IsAlive && Alert > float.Epsilon)
                Alerts.Broadcast(transform.position, Alert, false, _actor, true);

        }

        /// <summary>
        /// Generates a fear alert.
        /// </summary>
        public void OnScared()
        {
            if (_actor.IsAlive && Fear > float.Epsilon)
                Alerts.Broadcast(transform.position, Fear, false, _actor, true);
        }

        /// <summary>
        /// Generates a backup call alert.
        /// </summary>
        public void OnBackupCall()
        {
            if (_actor.IsAlive && BackupCall > float.Epsilon)
                Alerts.Broadcast(transform.position, BackupCall, false, _actor, true);
        }

        /// <summary>
        /// Generates a cop call alert.
        /// </summary>
        public void OnCopCall()
        {
            if (_actor.IsAlive && CopCall > float.Epsilon)
                Alerts.Broadcast(transform.position, CopCall, false, _actor, true);
        }

        /// <summary>
        /// Generates a cover switch alert.
        /// </summary>
        public void OnCoverSwitch()
        {
            if (_actor.IsAlive && CoverSwitch > float.Epsilon)
                Alerts.Broadcast(transform.position, CoverSwitch, false, _actor, true);
        }

        /// <summary>
        /// Generates an assault alert.
        /// </summary>
        public void OnAssaultStart()
        {
            if (_actor.IsAlive && Assault > float.Epsilon)
                Alerts.Broadcast(transform.position, Assault, false, _actor, true);
        }
    }
}
