using UnityEngine;
using UnityEngine.UI;

namespace CoverShooter
{
    /// <summary>
    /// Manages the display of a character's stamina.
    /// </summary>
    [ExecuteInEditMode]
    public class StaminaBar : MonoBehaviour
    {
        /// <summary>
        /// Object whose stamina is displayed on the stamina bar.
        /// </summary>
        [Tooltip("Object whose stamina is displayed on the stamina bar.")]
        public GameObject Target;

        /// <summary>
        /// Current value of the stamina bar.
        /// </summary>
        [Range(0, 1)]
        [Tooltip("Current value of the stamina bar.")]
        public float Value = 1.0f;

        /// <summary>
        /// Determines if the stamina bar is hidden when the target has no stamina.
        /// </summary>
        [Tooltip("Determines if the stamina bar is hidden when the target has no stamina.")]
        public bool HideWhenDead = true;

        /// <summary>
        /// Determines if the stamina bar is hidden when there is no target.
        /// </summary>
        [Tooltip("Determines if the stamina bar is hidden when there is no target.")]
        public bool HideWhenNone = false;

        /// <summary>
        /// Link to the object that draws the background of the stamina bar.
        /// </summary>
        [Tooltip("Link to the object that draws the background of the stamina bar.")]
        public RectTransform BackgroundRect;

        /// <summary>
        /// Link to the object that draws the stamina bar.
        /// </summary>
        [Tooltip("Link to the object that draws the stamina bar.")]
        public RectTransform FillRect;

        /// <summary>
        /// Link to the object that will be used to display a character's name.
        /// </summary>
        [Tooltip("Link to the object that will be used to display a character's name.")]
        public Text Name;

        private GameObject _cachedTarget;
        private CharacterStamina _cachedCharacterStamina;
        private CharacterName _cachedCharacterName;

        private void LateUpdate()
        {
            if (Target != _cachedTarget)
            {
                _cachedTarget = Target;

                if (Target != null)
                {
                    _cachedCharacterStamina = Target.GetComponent<CharacterStamina>();
                    _cachedCharacterName = Target.GetComponent<CharacterName>();
                }
                else
                {
                    _cachedCharacterStamina = null;
                    _cachedCharacterName = null;
                }
            }

            if (_cachedCharacterStamina != null)
                Value = _cachedCharacterStamina.Stamina / _cachedCharacterStamina.MaxStamina;

            var isVisible = true;

            if (Application.isPlaying)
            {
                isVisible = (!HideWhenDead || Value > float.Epsilon) && (!HideWhenNone || Target != null);

                if (FillRect != null) FillRect.gameObject.SetActive(isVisible);
                if (BackgroundRect != null) BackgroundRect.gameObject.SetActive(isVisible);
                if (Name != null) Name.gameObject.SetActive(isVisible);
            }

            if (isVisible)
            {
                if (Name != null)
                {
                    if (_cachedCharacterName == null)
                    {
                        if (Target != null)
                            Name.text = Target.name;
                    }
                    else
                        Name.text = _cachedCharacterName.Name;
                }

                if (FillRect != null)
                    FillRect.anchorMax = new Vector2(Value, 1);
            }
        }
    }
}