using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Disables the target dialog when pressed.
    /// </summary>
    public class CloseButton : PressButton
    {
        /// <summary>
        /// Dialog object to deactivate when pressed.
        /// </summary>
        [Tooltip("Dialog object to deactivate when pressed.")]
        public GameObject Dialog;

        protected override void OnPress()
        {
            if (Dialog != null)
                Dialog.SetActive(false);
        }
    }
}
