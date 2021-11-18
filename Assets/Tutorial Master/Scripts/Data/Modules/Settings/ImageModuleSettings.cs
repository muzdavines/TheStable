using System;
using UnityEngine;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Stores Image Module Settings
    /// </summary>
    /// <seealso cref="ModuleSettings" />
    [Serializable]
    [DataValidator(typeof(ImageModuleSettingsValidator))]
    public class ImageModuleSettings : ModuleSettings
    {
        /// <summary>
        /// The Image component of a given module
        /// </summary>
        public Sprite SpriteContent;
    }
}