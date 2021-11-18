using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoverShooter
{
    /// <summary>
    /// Reset the level on character's death.
    /// </summary>
    public class ResetOnDeath : MonoBehaviour, ICharacterHealthListener
    {
        /// <summary>
        /// Time in seconds to reset the level after character's death
        /// </summary>
        [Tooltip("Time in seconds to reset the level after character's death")]
        public float Delay = 3.0f;

        /// <summary>
        /// Starts a sequence to reset the level after waiting for Delay.
        /// </summary>
        public void OnDead()
        {
            StartCoroutine(delayedReset());
        }

        public void OnResurrect() { }

        private IEnumerator delayedReset()
        {
            yield return new WaitForSeconds(Delay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnValidate()
        {
            Delay = Mathf.Max(0, Delay);
        }
    }
}