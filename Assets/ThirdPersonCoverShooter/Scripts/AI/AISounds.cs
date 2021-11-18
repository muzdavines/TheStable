using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Plays various sounds on AI events.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    public class AISounds : MonoBehaviour
    {
        /// <summary>
        /// Possible sounds to play when the AI becomes alerted.
        /// </summary>
        [Tooltip("Possible sounds to play when the AI becomes alerted.")]
        public AudioClip[] Alert;

        /// <summary>
        /// Possible sounds to play when the AI becomes scared.
        /// </summary>
        [Tooltip("Possible sounds to play when the AI becomes scared.")]
        public AudioClip[] Fear;

        /// <summary>
        /// Possible sounds to play when the AI calls for cops.
        /// </summary>
        [Tooltip("Possible sounds to play when the AI calls for cops.")]
        public AudioClip[] CopCall;

        /// <summary>
        /// Possible sounds to play when the AI calls for backup.
        /// </summary>
        [Tooltip("Possible sounds to play when the AI calls for backup.")]
        public AudioClip[] BackupCall;

        /// <summary>
        /// Possible sounds to play when the AI decides to switch a cover.
        /// </summary>
        [Tooltip("Possible sounds to play when the AI decides to switch a cover.")]
        public AudioClip[] CoverSwitch;

        /// <summary>
        /// Possible sounds to play when the AI begins an assault.
        /// </summary>
        [Tooltip("Possible sounds to play when the AI begins an assault.")]
        public AudioClip[] Assault;

        private CharacterMotor _motor;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
        }

        /// <summary>
        /// Plays the alert sound.
        /// </summary>
        public void OnAlerted()
        {
            if (_motor.IsAlive)
                playSound(Alert);
        }

        /// <summary>
        /// Plays the fear sound.
        /// </summary>
        public void OnScared()
        {
            if (_motor.IsAlive)
                playSound(Fear);
        }

        /// <summary>
        /// Plays the backup call sound.
        /// </summary>
        public void OnBackupCall()
        {
            if (_motor.IsAlive)
                playSound(BackupCall);
        }

        /// <summary>
        /// Plays the cop call sound.
        /// </summary>
        public void OnCopCall()
        {
            if (_motor.IsAlive)
                playSound(CopCall);
        }

        /// <summary>
        /// Plays the cover switch sound.
        /// </summary>
        public void OnCoverSwitch()
        {
            if (_motor.IsAlive)
                playSound(CoverSwitch);
        }

        /// <summary>
        /// Plays the assault sound.
        /// </summary>
        public void OnAssaultStart()
        {
            if (_motor.IsAlive)
                playSound(Assault);
        }

        private void playSound(AudioClip[] clips)
        {
            if (clips.Length == 0)
                return;

            var clip = clips[UnityEngine.Random.Range(0, clips.Length)];
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
}
