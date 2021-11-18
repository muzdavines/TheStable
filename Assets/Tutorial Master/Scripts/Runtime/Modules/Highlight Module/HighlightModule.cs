using System;
using UnityEngine;

namespace HardCodeLab.TutorialMaster
{
    [AddComponentMenu("Tutorial Master/UGUI Highlight Module")]
    [HelpURL("https://support.hardcodelab.com/tutorial-master/2.0/highlighter-module")]
    public class HighlightModule : Module
    {
        private HighlightModuleSettings _currentHighlighterSettings;

        /// <summary>
        /// Deactivates the components if any.
        /// </summary>
        protected override void OnModuleDeactivated() { }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            CalculateSize();
            RecalculateModuleRect();
        }

        /// <inheritdoc />
        /// <summary>
        /// Initialize additional components if any.
        /// </summary>
        protected override void OnModuleActivated()
        {
            _currentHighlighterSettings = GetSettings<HighlightModuleSettings>();
            CalculateSize();
            RecalculateModuleRect();
        }

        private void CalculateSize()
        {
            if (_currentHighlighterSettings == null) return;

            switch (_currentHighlighterSettings.SizeType)
            {
                case SizeMode.BasedOnUITransform:

                    SetSize(_currentHighlighterSettings.UITransformReference, _currentHighlighterSettings.SizeOffset);

                    break;

                case SizeMode.CustomSize:

                    SetSize(_currentHighlighterSettings.CustomSize);

                    break;

                default:
                    throw TMLogger.LogException(new ArgumentOutOfRangeException());
            }
        }

        /// <summary>
        /// Sets the size of this Highlighter  Module.
        /// </summary>
        /// <param name="targetSize">Size of the target.</param>
        private void SetSize(Vector2 targetSize)
        {
            RectTransform.sizeDelta = targetSize;
            RecalculateModuleSize();
        }

        /// <summary>
        /// Sets the size of this Highlighter Module based on a UI Element.
        /// </summary>
        /// <param name="referenceUIElement">UI Element whose size will be used as a reference for this Highlighter Module.</param>
        /// <param name="sizeOffset">Additional amount that will be added to the overall size of this Highlighter Module.</param>
        private void SetSize(RectTransform referenceUIElement, Vector2 sizeOffset)
        {
            if (referenceUIElement == null) return;
            var referenceSize = RectTransformUtility.PixelAdjustRect(referenceUIElement, CurrentCanvas).size;
            RectTransform.sizeDelta = referenceSize + sizeOffset;
        }
    }
}