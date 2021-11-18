using System;
using UnityEngine;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Responsible for playing an Audio clip from specified Audio Source.
    /// </summary>
    [Serializable]
    [DataValidator(typeof(StageAudioValidator))]
    public class StageAudio
    {
        /// <summary>
        /// Clip which will be played.
        /// </summary>
        public AudioClip Clip;

        /// <summary>
        /// The audio source from which clip will be played.
        /// </summary>
        public AudioSource Source;

        /// <summary>
        /// Specifies when audio clip will be played
        /// </summary>
        public AudioTiming Timing;

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        public void PlayClip()
        {
            if (NullChecker.IsNull(Source, "Unable to play an audio clip. There's no Audio Source assigned."))
                return;

            if (NullChecker.IsNull(Clip, "Unable to play an audio clip. There's no Audio Clip assigned."))
                return;

            Source.PlayOneShot(Clip);
        }
    }
}