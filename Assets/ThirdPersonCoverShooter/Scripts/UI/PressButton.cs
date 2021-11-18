using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoverShooter
{
    /// <summary>
    /// Describes button graphics for each possible UI state.
    /// </summary>
    [Serializable]
    public struct PressButtonSettings
    {
        /// <summary>
        /// Image component that is manipulated.
        /// </summary>
        [Tooltip("Image component that is manipulated.")]
        public Image Target;

        /// <summary>
        /// Image color when the button is neither pressed nor highlighted.
        /// </summary>
        [Tooltip("Image color when the button is neither pressed nor highlighted.")]
        public Color NormalColor;

        /// <summary>
        /// Image color when the cursor is held above the button.
        /// </summary>
        [Tooltip("Image color when the cursor is held above the button.")]
        public Color HighlightColor;

        /// <summary>
        /// Image color when the button is being pressed.
        /// </summary>
        [Tooltip("Image color when the button is being pressed.")]
        public Color PressColor;

        /// <summary>
        /// Image sprite when the cursor is held above the button.
        /// </summary>
        [Tooltip("Image sprite when the cursor is held above the button.")]
        public Sprite HighlightSprite;

        /// <summary>
        /// Image sprite when the button is pressed.
        /// </summary>
        [Tooltip("Image sprite when the button is pressed.")]
        public Sprite PressSprite;

        public static PressButtonSettings Default()
        {
            PressButtonSettings settings;
            settings.Target = null;
            settings.NormalColor = Color.white;
            settings.HighlightColor = new Color(127, 127, 127, 255);
            settings.PressColor = new Color(63, 63, 63, 255);
            settings.HighlightSprite = null;
            settings.PressSprite = null;

            return settings;
        }
    }

    /// <summary>
    /// Registers pointer events and adjusts Image based on the UI state. Calls various events.
    /// </summary>
    public class PressButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
    {
        public PressButtonSettings Button = PressButtonSettings.Default();

        private Sprite _savedNormalSprite;
        private bool _isNormalSpriteRightNow = true;

        private bool _isPressed;
        private bool _isHighlighted;

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            _changeGraphic(Button.PressColor, Button.PressSprite);
            OnPress();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;

            if (_isHighlighted)
                _changeGraphic(Button.HighlightColor, Button.HighlightSprite);
            else
                _changeGraphicBack();

            OnRelease();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHighlighted = true;

            if (!_isPressed)
                _changeGraphic(Button.HighlightColor, Button.HighlightSprite);

            OnEnter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHighlighted = false;

            if (!_isPressed)
                _changeGraphicBack();

            OnExit();
        }

        private void OnDisable()
        {
            _isPressed = false;
            _isHighlighted = false;
            _changeGraphicBack();
        }

        protected virtual void OnPress()
        {
        }

        protected virtual void OnRelease()
        {
        }

        protected virtual void OnEnter()
        {
        }

        protected virtual void OnExit()
        {
        }

        private void _changeGraphic(Color color, Sprite sprite)
        {
            if (Button.Target == null)
                return;

            if (sprite != null)
            {
                if (_isNormalSpriteRightNow)
                {
                    _isNormalSpriteRightNow = false;
                    _savedNormalSprite = Button.Target.sprite;
                }

                Button.Target.sprite = sprite;
            }

            Button.Target.color = color;
        }

        private void _changeGraphicBack()
        {
            if (Button.Target == null)
                return;

            if (!_isNormalSpriteRightNow)
            {
                _isNormalSpriteRightNow = true;
                Button.Target.sprite = _savedNormalSprite;
            }

            Button.Target.color = Button.NormalColor;
        }
    }
}
