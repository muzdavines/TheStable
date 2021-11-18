using UnityEngine;
using UnityEngine.UI;

namespace HardCodeLab.TutorialMaster
{
    [RequireComponent(typeof(RawImage))]
    [AddComponentMenu("Tutorial Master/UGUI Image Module")]
    public class RawImageModule : Module
    {
        public RawImage ImageContainer;

        protected override void OnModuleDeactivated()
        {
        }

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
            if (ImageContainer == null) return;
            if (image == null) return;

            ImageContainer.texture = image.texture;
        }
    }
}