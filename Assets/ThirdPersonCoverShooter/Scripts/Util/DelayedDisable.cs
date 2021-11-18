using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Disables the game object after a certain amount of time. Counts time starting from OnEnable.
    /// </summary>
    public class DelayedDisable : MonoBehaviour
    {
        /// <summary>
        /// Time to disable in seconds.
        /// </summary>
        [Tooltip("Time to disable in seconds.")]
        public float Delay = 1.0f;

        private float _timer = 0;

        private void OnEnable()
        {
            _timer = 0;
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            if (_timer >= Delay)
                gameObject.SetActive(false);
        }

        private void OnValidate()
        {
            Delay = Mathf.Max(0, Delay);
        }
    }
}