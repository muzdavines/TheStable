using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoverShooter
{
    /// <summary>
    /// When pressed toggles associated characters crouch state.
    /// </summary>
    public class CrouchTouch : Text, IPointerDownHandler
    {
        /// <summary>
        /// Link to the mobile controller that will be set to crouch.
        /// </summary>
        [Tooltip("Link to the mobile controller that will be set to crouch.")]
        public MobileController Controller;

        private RectTransform _rect;

        protected override void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            if (Controller == null || CameraManager.Main == null)
                return;

            var viewport = CameraManager.Main.WorldToViewportPoint(Controller.transform.position);
            _rect.localPosition = new Vector3(0, 0, _rect.localPosition.z);
            _rect.anchorMin = viewport;
            _rect.anchorMax = viewport;
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (Controller != null)
                Controller.IsCrouching = !Controller.IsCrouching;
        }
    }
}