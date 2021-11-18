using System.Collections;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Broadcasts alerts upon a hit. Used mostly on static level geometry.    
    /// </summary>
    public class HitAlerts : MonoBehaviour
    {
        /// <summary>
        /// Range of the alert generated.
        /// </summary>
        [Tooltip("Range of the alert generated.")]
        public float Range;

        /// <summary>
        /// Broadcasts an alert.
        /// </summary>
        public void OnHit(Hit hit)
        {
            Alerts.Broadcast(hit.Position, Range, true, Actors.Get(hit.Attacker), false);
        }
    }
}