using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Denotes a zone that prompts the AI to be careful.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DangerZone : Zone<DangerZone>
    {
        private void OnTriggerEnter(Collider other)
        {
            other.SendMessage("OnEnterDanger", this, SendMessageOptions.DontRequireReceiver);
        }

        private void OnTriggerExit(Collider other)
        {
            other.SendMessage("OnLeaveDanger", this, SendMessageOptions.DontRequireReceiver);
        }
    }
}
