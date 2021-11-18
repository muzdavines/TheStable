using HardCodeLab.TutorialMaster.Localization;
using System;
using UnityEngine;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Stores Popup Module Settings
    /// </summary>
    /// <seealso cref="ModuleSettings" />
    [Serializable]
    [DataValidator(typeof(PopupModuleSettingsValidator))]
    public class PopupModuleSettings : ModuleSettings
    {
        /// <summary>
        /// An button click which will be invoked when Popup button is clicked.
        /// </summary>
        public ActionType ButtonClickEvent = ActionType.PlayNextStage;

        /// <summary>
        /// Label of the button of the Pop-Up
        /// </summary>
        public LocalizedString ButtonLabel;

        /// <summary>
        /// The popup image component
        /// </summary>
        public Sprite PopupImage;

        /// <summary>
        /// The popup message
        /// </summary>
        public LocalizedString PopupMessage;

        /// <summary>
        /// The title of the popup box.
        /// </summary>
        public LocalizedString PopupTitle;

        /// <summary>
        /// If true, the Pop-Up button will be visible
        /// </summary>
        public bool ShowButton = false;
    }
}