using UnityEngine;
using UnityEngine.UI;

namespace MedievalKingdomUI.Scripts.Window
{
    public class AnimatedWindowController : WindowController
    {
        public GraphicRaycaster graphicRaycaster;
        public Animator animator;
        private GameObject _windowToOpen;
        private static readonly int RunAnimation = Animator.StringToHash("RunAnimation");

        public override void OpenWindow(GameObject window)
        {
            graphicRaycaster.enabled = true;
            _windowToOpen = window;
            animator.SetTrigger(RunAnimation);
        }

        public void Proceed()
        {
            base.OpenWindow(_windowToOpen);
            graphicRaycaster.enabled = false;
        }
    }
}