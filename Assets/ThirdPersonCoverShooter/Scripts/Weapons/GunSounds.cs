using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Generates randomised sounds on various gun events.
    /// </summary>
    public class GunSounds : MonoBehaviour, IGunListener
    {
        /// <summary>
        /// Sound to play when ejecting a magazine.
        /// </summary>
        [Tooltip("Sound to play when ejecting a magazine.")]
        public AudioClip[] Eject;

        /// <summary>
        /// Sound to play when a magazine is put inside the gun.
        /// </summary>
        [Tooltip("Sound to play when a magazine is put inside the gun.")]
        public AudioClip[] Rechamber;

        /// <summary>
        /// Possible sounds to play when pumping a shotgun.
        /// </summary>
        [Tooltip("Possible sounds to when pumping a shotgun.")]
        public AudioClip[] Pump;

        /// <summary>
        /// Possible sounds to play on each bullet fire.
        /// </summary>
        [Tooltip("Possible sounds to play on each bullet fire.")]
        public AudioClip[] Fire;

        /// <summary>
        /// Possible sounds to play on each fire attempt on empty magazine.
        /// </summary>
        [Tooltip("Possible sounds to play on each fire attempt on empty magazine.")]
        public AudioClip[] EmptyFire;

        /// <summary>
        /// Play pump sound.
        /// </summary>
        public void OnPump()
        {
            if (Pump.Length > 0)
                play(Pump[UnityEngine.Random.Range(0, Pump.Length)]);
        }

        /// <summary>
        /// Play magazine eject sound.
        /// </summary>
        public void OnEject()
        {
            if (Eject.Length > 0)
                play(Eject[UnityEngine.Random.Range(0, Eject.Length)]);
        }

        /// <summary>
        /// Play magazine load sound.
        /// </summary>
        public void OnRechamber()
        {
            if (Rechamber.Length > 0)
                play(Rechamber[UnityEngine.Random.Range(0, Rechamber.Length)]);
        }

        /// <summary>
        /// Play fire sound delayed by the given amount of time in seconds.
        /// </summary>
        /// <param name="delay">Time to delay the creation of sound.</param>
        public void OnFire(float delay)
        {
            StartCoroutine(play(delay, Fire));
        }

        /// <summary>
        /// Play empty magazine sounds.
        /// </summary>
        public void OnEmptyFire()
        {
            if (EmptyFire.Length > 0)
                play(EmptyFire[UnityEngine.Random.Range(0, EmptyFire.Length)]);
        }

        public void OnFullyLoaded() { }
        public void OnBulletLoad() { }
        public void OnPumpStart() { }
        public void OnMagazineLoadStart() { }
        public void OnBulletLoadStart() { }

        private void play(AudioClip clip)
        {
            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, transform.position);
        }

        private void play(AudioClip[] clips)
        {
            if (clips.Length > 0)
                play(clips[UnityEngine.Random.Range(0, clips.Length)]);
        }

        private IEnumerator play(float delay, AudioClip[] clips)
        {
            yield return new WaitForSeconds(delay);
            play(clips);
        }
    }
}