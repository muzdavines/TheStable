using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Causes player characters to hide their weapon upon entering the area.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PlayerDisarmTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var motor = other.GetComponent<CharacterMotor>();
            if (motor == null) return;

            motor.IsEquipped = false;
        }
    }
}
