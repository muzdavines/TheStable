using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using HardCodeLab.TutorialMaster.Localization;

namespace HardCodeLab.TutorialMaster
{
    [HelpURL("https://support.hardcodelab.com/tutorial-master/2.0/popup-module")]
    public abstract class PopupModule : Module
    {
        /// <summary>
        /// The Button component which is responsible for Pop-Up's confirmation button
        /// </summary>
        public Button ConfirmationButton;

        /// <summary>
        /// An Image component which is responsible for Pop-Up's image.
        /// </summary>
        public Image PopupImage;

        /// <summary>
        /// The OnClick event for this Pop-Up's button component
        /// </summary>
        protected UnityAction AdditionalButtonClickEvent;

        /// <summary>
        /// Current localized content for Pop-Up's button label
        /// </summary>
        protected LocalizedString CurrentButtonLabel;

        /// <summary>
        /// Current localized content for Pop-Up's message body
        /// </summary>
        protected LocalizedString CurrentMessageBody;

        /// <summary>
        /// Current localized content for Pop-Up's title.
        /// </summary>
        protected LocalizedString CurrentTitleContent;

        /// <summary>
        /// Sets the button click event for this Pop-Up Module
        /// </summary>
        /// <param name="eventType">Type of OnClick event this button will have.</param>
        /// <param name="nextStageAction">Action which will take user to the next stage of the tutorial.</param>
        /// <param name="stopTutorialAction">Action which will stop the currently active tutorial.</param>
        public void AssignButtonClickEvent(ActionType eventType, Action nextStageAction, Action stopTutorialAction)
        {
            if (NullChecker.IsNull(ConfirmationButton, "Unable to assign click events! Button Component has not been assigned.", CallerManager))
                return;

            RemoveButtonClickEvent();

            switch (eventType)
            {
                case ActionType.PlayNextStage:

                    AdditionalButtonClickEvent = () => nextStageAction();

                    break;

                case ActionType.StopTutorial:

                    AdditionalButtonClickEvent = () => stopTutorialAction();

                    break;
            }

            if (AdditionalButtonClickEvent == null) 
                return;

            ConfirmationButton.onClick.AddListener(AdditionalButtonClickEvent);
        }

        /// <inheritdoc />
        /// <summary>
        /// Updates this Module's localized content to a new language.
        /// </summary>
        /// <param name="languageKey">Unique key of a language which will be set for this Module.</param>
        public override void SetLanguage(string languageKey)
        {
            base.SetLanguage(languageKey);

            if (CurrentTitleContent != null) 
                SetTitle(CurrentTitleContent.GetContent(CurrentLanguageKey));

            if (CurrentMessageBody != null) 
                SetMessage(CurrentMessageBody.GetContent(CurrentLanguageKey));

            if (CurrentButtonLabel != null) 
                SetButtonLabel(CurrentButtonLabel.GetContent(CurrentLanguageKey));
        }

        /// <inheritdoc />
        /// <summary>
        /// Called after this Module is deactivated.
        /// </summary>
        protected override void OnModuleDeactivated()
        {
            RemoveButtonClickEvent();
        }

        /// <inheritdoc />
        /// <summary>
        /// Called after this Module is activated.
        /// </summary>
        protected override void OnModuleActivated()
        {
            var settings = GetSettings<PopupModuleSettings>();

            CurrentTitleContent = settings.PopupTitle;
            CurrentMessageBody = settings.PopupMessage;
            CurrentButtonLabel = settings.ButtonLabel;

            SetImage(settings.PopupImage);

            ToggleButtonVisibility(settings.ShowButton);

            if (settings.ShowButton)
            {
                SetButtonLabel(settings.ButtonLabel.GetContent(CurrentLanguageKey));
            }
        }

        /// <summary>
        /// Removes the button click event which it added previously.
        /// </summary>
        protected void RemoveButtonClickEvent()
        {
            if (ConfirmationButton == null || AdditionalButtonClickEvent == null)
                return;

            ConfirmationButton.onClick.RemoveListener(AdditionalButtonClickEvent);
            AdditionalButtonClickEvent = null;
        }

        /// <summary>
        /// Sets the button label for this Popup Module unless it's missing an appropriate component.
        /// Text won't be changed if it's empty.
        /// </summary>
        /// <param name="text">The text which will be set.</param>
        protected abstract void SetButtonLabel(string text);

        /// <summary>
        /// Sets the image for this Popup Module unless it's missing an appropriate component.
        /// Image won't be changed if it's null.
        /// </summary>
        /// <param name="image">An image which will be set.</param>
        protected void SetImage(Sprite image)
        {
            if (image == null)
                return;

            if (NullChecker.IsNull(PopupImage, "Unable to set an image. Image Component has not been assigned.", CallerManager))
                return;

            PopupImage.sprite = image;
        }

        /// <summary>
        /// Sets the message for this Popup Module unless it's missing an appropriate component.
        /// Text won't be changed if it's empty.
        /// </summary>
        /// <param name="text">The text which will be set.</param>
        protected abstract void SetMessage(string text);

        /// <summary>
        /// Sets the title for this Popup Module unless it's missing an appropriate component
        /// </summary>
        /// <param name="text">The text which will be set.</param>
        protected abstract void SetTitle(string text);

        /// <summary>
        /// Toggles the visibility of a button for this Popup Module unless it's missing an appropriate component
        /// </summary>
        /// <param name="state">If set to <c>true</c> button is disabled.</param>
        protected void ToggleButtonVisibility(bool state)
        {
            if (ConfirmationButton == null) 
                return;

            ConfirmationButton.gameObject.SetActive(state);
        }
    }
}