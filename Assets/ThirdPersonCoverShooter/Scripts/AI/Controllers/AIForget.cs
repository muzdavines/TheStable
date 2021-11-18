using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Resets the AI if no update of the enemy has occured for long enough.
    /// </summary>
    public class AIForget : AIBase
    {
        /// <summary>
        /// Time in seconds it takes for the AI to forget about the enemy. Time is measured from the moment the last information about the enemy was received.
        /// </summary>
        [Tooltip("Time in seconds it takes for the AI to forget about the enemy. Time is measured from the moment the last information about the enemy was received.")]
        public float Duration = 30;

        private bool _isActive;
        private float _time;

        private void OnThreat(Actor value)
        {
            _isActive = true;
            _time = 0;
        }

        private void OnThreatPosition(Vector3 value)
        {
            _isActive = true;
            _time = 0;
        }

        private void OnSearch()
        {
            _isActive = true;
            _time = 0;
        }

        private void Update()
        {
            if (_isActive)
            {
                if (_time >= Duration - float.Epsilon)
                {
                    _isActive = false;
                    Message("ToForget");
                }
                else
                    _time += Time.deltaTime;
            }
        }
    }
}
