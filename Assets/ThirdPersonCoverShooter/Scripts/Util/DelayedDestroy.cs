using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Destroys the game object after a certain amount of time. Counts time starting from OnEnable.
    /// </summary>
    public class DelayedDestroy : MonoBehaviour
    {
        /// <summary>
        /// Time to destroy in seconds.
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
                GameObject.Destroy(gameObject);
        }

        private void OnValidate()
        {
            Delay = Mathf.Max(0, Delay);
        }
    }
}