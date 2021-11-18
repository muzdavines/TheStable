using System.Collections;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Plays sounds upon a hit. Used mostly on static level geometry.    
    /// </summary>
    public class HitSounds : MonoBehaviour
    {
        /// <summary>
        /// Possible sounds to play on bullet impact.
        /// </summary>
        [Tooltip("Possible sounds to play on bullet impact.")]
        public AudioClip[] Bullet;

        /// <summary>
        /// Possible sounds to play on melee impact.
        /// </summary>
        [Tooltip("Possible sounds to play on melee impact.")]
        public AudioClip[] Melee;

        /// <summary>
        /// Plays a sound.
        /// </summary>
        public void OnHit(Hit hit)
        {
            var clips = hit.IsMelee ? Melee : Bullet;

            if (clips == null || clips.Length == 0)
                return;

            var clip = clips[UnityEngine.Random.Range(0, clips.Length)];

            AudioSource.PlayClipAtPoint(clip, hit.Position);
        }
    }
}