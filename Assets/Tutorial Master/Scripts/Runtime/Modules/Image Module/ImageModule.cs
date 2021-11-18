using UnityEngine;
using UnityEngine.UI;

namespace HardCodeLab.TutorialMaster
{
    [AddComponentMenu("Tutorial Master/UGUI Image Module")]
    [HelpURL("https://support.hardcodelab.com/tutorial-master/2.0/image-module")]
    public class ImageModule : Module
    {
        public Image ImageContainer;
        public RawImage RawImageContainer;

        protected override void OnModuleDeactivated() { }

        /// <inheritdoc />
        /// <summary>
        /// Activates the functionalities of this Module
        /// </summary>
        protected override void OnModuleActivated()
        {
            SetImage(GetSettings<ImageModuleSettings>().SpriteContent);
        }

        /// <summary>
        /// Sets the image for this Popup Module unless it's missing an appropriate component.
        /// Image won't be changed if it's null.
        /// </summary>
        /// <param name="image">An image which will be set.</param>
        protected void SetImage(Sprite image)
        {
            if (image == null) return;
            if (ImageContainer == null && RawImageContainer == null)
            {
                TMLogger.LogInfo(string.Format("Missing Image Component for Image Module (\"{0}\")!", gameObject.name), CallerManager);
                return;
            }

            if (ImageContainer != null)
            {
                ImageContainer.sprite = image;
            }
            else
            {
                RawImageContainer.texture = image.texture;
            }
        }
    }
}