using System.Collections;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Spawns sound instances on various character events. Sounds are randomly picked from lists.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    public class CharacterSounds : MonoBehaviour, ICharacterPhysicsListener, ICharacterHealthListener
    {
        /// <summary>
        /// Possible sounds to play on each footstep.
        /// </summary>
        [Tooltip("Possible sounds to play on each footstep.")]
        public AudioClip[] Footstep;

        /// <summary>
        /// Possible sounds to play when the character dies.
        /// </summary>
        [Tooltip("Possible sounds to play when the character dies.")]
        public AudioClip[] Death;

        /// <summary>
        /// Possible sounds to play at the beginning of a jump.
        /// </summary>
        [Tooltip("Possible sounds to play at the beginning of a jump.")]
        public AudioClip[] Jump;

        /// <summary>
        /// Possible sounds to play when the character lands.
        /// </summary>
        [Tooltip("Possible sounds to play when the character lands.")]
        public AudioClip[] Land;

        /// <summary>
        /// Possible sounds to play when the character blocks a melee attack.
        /// </summary>
        [Tooltip("Possible sounds to play when the character blocks a melee attack.")]
        public AudioClip[] Block;

        /// <summary>
        /// Possible sounds to play when the character is hurt.
        /// </summary>
        [Tooltip("Possible sounds to play when the character is hurt.")]
        public AudioClip[] Hurt;

        /// <summary>
        /// Possible sounds to play when the character is hit.
        /// </summary>
        [Tooltip("Possible sounds to play when the character is hit.")]
        public AudioClip[] Hit;

        /// <summary>
        /// Possible sounds to play when the character is dealt a lot of damage by a hit.
        /// </summary>
        [Tooltip("Possible sounds to play when the character is dealt a lot of damage by a hit.")]
        public AudioClip[] BigHit;

        /// <summary>
        /// Damage that has to be dealt to play big hit sound.
        /// </summary>
        [Tooltip("Damage that has to be dealt to play big hit sound.")]
        public float BigDamageThreshold = 50;

        private CharacterMotor _motor;
        private float _hurtSoundTimer;
        private float _fallSoundTimer;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
        }

        private void LateUpdate()
        {
            if (_hurtSoundTimer > -float.Epsilon)
                _hurtSoundTimer -= Time.deltaTime;

            if (_fallSoundTimer > -float.Epsilon)
                _fallSoundTimer -= Time.deltaTime;
        }

        public void OnLand()
        {
            if (_fallSoundTimer <= 0 && _motor.IsAlive)
            {
                _fallSoundTimer = 0.4f;
                playSound(Land, transform.position);
            }
        }

        public void OnFootstep(Vector3 position)
        {
            if (_motor.IsAlive)
                playSound(Footstep, position);
        }

        public void OnDead()
        {
            playSound(Death, transform.position, 0.3f);
        }

        public void OnResurrect()
        {
        }

        public void OnBlock(Hit hit)
        {
            playSound(Block, hit.Position);
        }

        public void OnJump()
        {
            if (_motor.IsAlive)
                playSound(Jump, transform.position);
        }

        public void OnTakenHit(Hit hit)
        {
            if (_hurtSoundTimer < float.Epsilon && _motor.IsAlive)
            {
                _hurtSoundTimer = 0.5f;
                playSound(Hurt, hit.Position, 0.1f);
            }

            if (BigHit != null && BigHit.Length > 0 && hit.Damage >= BigDamageThreshold)
                playSound(BigHit, hit.Position);
            else
                playSound(Hit, hit.Position);
        }

        private void playSound(AudioClip[] clips, Vector3 position,  float delay = 0f)
        {
            if (clips.Length == 0)
                return;

            var clip = clips[UnityEngine.Random.Range(0, clips.Length)];

            if (delay < float.Epsilon)
                AudioSource.PlayClipAtPoint(clip, position);
            else
                StartCoroutine(delayedClip(clip, position, delay));
        }

        private IEnumerator delayedClip(AudioClip clip, Vector3 position, float delay)
        {
            yield return new WaitForSeconds(delay);
            AudioSource.PlayClipAtPoint(clip, position);
        }
    }
}