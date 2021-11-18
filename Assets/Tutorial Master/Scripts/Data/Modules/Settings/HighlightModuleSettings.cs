using System;
using UnityEngine;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Stores Highlight Module Settings
    /// </summary>
    /// <seealso cref="ModuleSettings" />
    [Serializable]
    [DataValidator(typeof(HighlighterModuleSettingsValidator))]
    public class HighlightModuleSettings : ModuleSettings
    {
        /// <summary>
        /// The custom size for this highlight module
        /// </summary>
        public Vector2 CustomSize;

        /// <summary>
        /// Determines an additional size for Highlight Module.
        /// </summary>
        public Vector2 SizeOffset;

        /// <summary>
        /// The size type of this highlight module
        /// </summary>
        public SizeMode SizeType = SizeMode.BasedOnUITransform;

        /// <summary>
        /// The UI transform rect whose size will be used for sizing this highlighter
        /// </summary>
        public RectTransform UITransformReference;
    }
}