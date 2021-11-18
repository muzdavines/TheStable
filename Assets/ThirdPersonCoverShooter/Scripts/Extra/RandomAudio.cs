using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Picks a random audio sample from the supplied list and sets Audio Source to play it.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class RandomAudio : MonoBehaviour
    {
        /// <summary>
        /// Array of possible audio clips to be played.
        /// </summary>
        [Tooltip("Array of possible audio clips to be played.")]
        public AudioClip[] Clips;

        /// <summary>
        /// Should the clip selection happen during the object's awakening.
        /// </summary>
        [Tooltip("Should the clip selection happen during the object's awakening.")]
        public bool PlayOnAwake = true;

        /// <summary>
        /// Plays a random clip if set so.
        /// </summary>
        private void Awake()
        {
            if (PlayOnAwake)
                Play();
        }

        /// <summary>
        /// Sets up an AudioSource to play a random clip.
        /// </summary>
        public void Play()
        {
            if (Clips.Length == 0)
                return;

            var source = GetComponent<AudioSource>();
            if (source == null) return;
            
            source.clip = Clips[Random.Range(0, Clips.Length)];
            source.Play();
        }
    }
}
