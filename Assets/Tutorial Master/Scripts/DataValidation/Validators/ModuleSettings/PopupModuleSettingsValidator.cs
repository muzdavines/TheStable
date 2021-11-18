using System;

namespace HardCodeLab.TutorialMaster
{
    public class PopupModuleSettingsValidator : BaseModuleSettingsValidator<PopupModuleSettings>
    {
        protected override string Prefix
        {
            get { return "Pop-Up Module"; }
        }

        public PopupModuleSettingsValidator(Type dataType) : base(dataType)
        {
        }
    }
}