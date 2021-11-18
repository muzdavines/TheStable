using System;

namespace HardCodeLab.TutorialMaster
{
    public class ImageModuleSettingsValidator : BaseModuleSettingsValidator<ImageModuleSettings>
    {
        protected override string Prefix
        {
            get { return "Image Module"; }
        }

        public ImageModuleSettingsValidator(Type dataType) : base(dataType)
        {
        }

        protected override void OnValidate(ImageModuleSettings data)
        {
            base.OnValidate(data);

            if (data.SpriteContent == null)
                AddIssue("Sprite has not been assigned for the image", Prefix);
        }
    }
}