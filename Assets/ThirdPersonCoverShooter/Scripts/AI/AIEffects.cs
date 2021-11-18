using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Listens for events comign from AI components and instantiates effects.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    public class AIEffects : MonoBehaviour
    {
        /// <summary>
        /// Effect prefab to instantiate when the AI becomes alerted.
        /// </summary>
        [Tooltip("Effect prefab to instantiate when the AI becomes alerted.")]
        public GameObject Alert;

        /// <summary>
        /// Effect prefab to instantiate when the AI becomes scared.
        /// </summary>
        [Tooltip("Effect prefab to instantiate when the AI becomes scared.")]
        public GameObject Fear;

        /// <summary>
        /// Effect prefab to instantiate when the AI calls for cops.
        /// </summary>
        [Tooltip("Effect prefab to instantiate when the AI calls for cops.")]
        public GameObject CopCall;

        /// <summary>
        /// Effect prefab to instantiate when the AI calls for backup.
        /// </summary>
        [Tooltip("Effect prefab to instantiate when the AI calls for backup.")]
        public GameObject BackupCall;

        /// <summary>
        /// Effect prefab to instantiate when the AI decides to switch a cover.
        /// </summary>
        [Tooltip("Effect prefab to instantiate when the AI decides to switch a cover.")]
        public GameObject CoverSwitch;

        /// <summary>
        /// Effect prefab to instantiate when the AI begins an assault.
        /// </summary>
        [Tooltip("Effect prefab to instantiate when the AI begins an assault.")]
        public GameObject Assault;

        private CharacterMotor _motor;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
        }

        /// <summary>
        /// Instantiates the alert effect.
        /// </summary>
        public void OnAlerted()
        {
            if (_motor.IsAlive)
                instantiate(Alert, transform.position);
        }

        /// <summary>
        /// Instantiates the fear effect.
        /// </summary>
        public void OnScared()
        {
            if (_motor.IsAlive)
                instantiate(Fear, transform.position);
        }

        /// <summary>
        /// Instantiates the backup call effect.
        /// </summary>
        public void OnBackupCall()
        {
            if (_motor.IsAlive)
                instantiate(BackupCall, transform.position);
        }

        /// <summary>
        /// Instantiates the cop call effect.
        /// </summary>
        public void OnCopCall()
        {
            if (_motor.IsAlive)
                instantiate(CopCall, transform.position);
        }

        /// <summary>
        /// Instantiates the cover switch effect.
        /// </summary>
        public void OnCoverSwitch()
        {
            if (_motor.IsAlive)
                instantiate(CoverSwitch, transform.position);
        }

        /// <summary>
        /// Instantiates the assault effect.
        /// </summary>
        public void OnAssaultStart()
        {
            if (_motor.IsAlive)
                instantiate(Assault, transform.position);
        }

        /// <summary>
        /// Helper function to instantiate effect prefabs.
        /// </summary>
        private void instantiate(GameObject prefab, Vector3 position)
        {
            if (prefab == null)
                return;

            var obj = GameObject.Instantiate(prefab);
            obj.transform.SetParent(null);
            obj.transform.position = position;
            obj.SetActive(true);

            GameObject.Destroy(obj, 3);
        }
    }
}
