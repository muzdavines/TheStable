using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Generates randomised sounds on various melee events.
    /// </summary>
    public class MeleeSounds : MonoBehaviour, IMeleeListener
    {
        /// <summary>
        /// Sounds to be played when attacking.
        /// </summary>
        [Tooltip("Sounds to be played when attacking.")]
        public AudioClip[] Attack;

        /// <summary>
        /// Sounds to be played upon a successful hit.
        /// </summary>
        [Tooltip("Sounds to be played upon a successful hit.")]
        public AudioClip[] Hit;

        /// <summary>
        /// Sounds to be played upon a specific melee moment defined by the animation.
        /// </summary>
        [Tooltip("Sounds to be played upon a specific melee moment defined by the animation.")]
        public AudioClip[] Moment;

        /// <summary>
        /// Play melee attack sound.
        /// </summary>
        public void OnMeleeAttack()
        {
            if (Attack.Length > 0)
                play(Attack[Random.Range(0, Attack.Length)]);
        }

        /// <summary>
        /// Play melee hit sound.
        /// </summary>
        public void OnMeleeHit()
        {
            if (Hit.Length > 0)
                play(Hit[Random.Range(0, Hit.Length)]);
        }

        /// <summary>
        /// Play melee moment sound.
        /// </summary>
        public void OnMeleeMoment()
        {
            if (Moment.Length > 0)
                play(Moment[Random.Range(0, Moment.Length)]);
        }

        private void play(AudioClip clip)
        {
            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
}