using UnityEngine;
using UnityEngine.UI;

namespace CoverShooter
{
    /// <summary>
    /// On touch makes the character pick up a grenade.
    /// </summary>
    [ExecuteInEditMode]
    public class GrenadeBar : PressButton
    {
        /// <summary>
        /// Motor which will be told to use grenades when pressed.
        /// </summary>
        [Tooltip("Motor which will be assigned to use grenades when pressed.")]
        public CharacterMotor Motor;

        /// <summary>
        /// Determines if the bar is hidden when there is no target.
        /// </summary>
        [Tooltip("Determines if the ammo bar is hidden when there is no target.")]
        public bool HideWhenNone = false;

        /// <summary>
        /// Link to the object that draws the background of the bar.
        /// </summary>
        [Tooltip("Link to the object that draws the background of the ammo bar.")]
        public RectTransform BackgroundRect;

        /// <summary>
        /// Link to the icon object.
        /// </summary>
        [Tooltip("Link to the icon object.")]
        public RectTransform Icon;

        /// <summary>
        /// Link to the object that will be used to display a gun's name.
        /// </summary>
        [Tooltip("Link to the object that will be used to display a gun's name.")]
        public Text Name;

        /// <summary>
        /// Color to use on elements when grenades are selected.
        /// </summary>
        [Tooltip("Color to use on elements when the gun is selected.")]
        public Color ActiveColor = Color.white;

        /// <summary>
        /// Color to use on elements when grenades are not selected.
        /// </summary>
        [Tooltip("Color to use on elements when the gun is not selected.")]
        public Color InactiveColor = new Color(1, 1, 1, 0.6f);

        protected override void OnPress()
        {
            if (Motor != null && !Motor.HasGrenadeInHand)
                Motor.InputTakeGrenade();
        }

        private void LateUpdate()
        {
            if (Application.isPlaying)
            {
                var isVisible = !HideWhenNone || Motor != null;
                updateElement(BackgroundRect, isVisible);
                updateElement(Icon, isVisible);
                updateElement(Name, isVisible);
            }
        }

        private void updateElement(RectTransform obj, bool isVisible)
        {
            if (obj == null)
                return;

            if (obj.gameObject.activeSelf != isVisible)
                obj.gameObject.SetActive(isVisible);

            if (isVisible)
            {
                var image = obj.GetComponent<Image>();

                if (Motor != null)
                    image.color = Motor.HasGrenadeInHand ? ActiveColor : InactiveColor;
            }
        }

        private void updateElement(Text obj, bool isVisible)
        {
            if (obj == null)
                return;

            if (obj.gameObject.activeSelf != isVisible)
                obj.gameObject.SetActive(isVisible);
        }
    }
}