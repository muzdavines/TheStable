using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Base dialog component. Enables background object when activated.
    /// </summary>
    public class BaseDialog : MonoBehaviour
    {
        /// <summary>
        /// Background to activate when the dialog is active.
        /// </summary>
        [Tooltip("Background to activate when the dialog is open.")]
        public GameObject Background;

        private void OnEnable()
        {
            if (Background != null)
                Background.SetActive(true);
        }

        private void OnDisable()
        {
            if (Background != null)
                Background.SetActive(false);
        }
    }
}
