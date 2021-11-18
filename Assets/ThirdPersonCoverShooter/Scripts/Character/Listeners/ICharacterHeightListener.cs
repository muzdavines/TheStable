using UnityEngine;

namespace CoverShooter
{
    public interface ICharacterHeightListener
    {
        /// <summary>
        /// Character changes their constant standing height.
        /// </summary>
        void OnStandingHeight(float value);

        /// <summary>
        /// Character has changed their current height (crouches or stands up).
        /// </summary>
        void OnCurrentHeight(float value);
    }
}