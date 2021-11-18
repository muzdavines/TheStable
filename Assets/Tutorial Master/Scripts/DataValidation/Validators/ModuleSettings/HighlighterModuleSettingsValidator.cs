using System;

namespace HardCodeLab.TutorialMaster
{
    public class HighlighterModuleSettingsValidator : BaseModuleSettingsValidator<HighlightModuleSettings>
    {
        protected override string Prefix
        {
            get { return "Highlighter Module"; }
        }

        public HighlighterModuleSettingsValidator(Type dataType) : base(dataType)
        {
        }

        protected override void OnValidate(HighlightModuleSettings data)
        {
            base.OnValidate(data);

            switch (data.SizeType)
            {
                case SizeMode.BasedOnUITransform:

                    if (data.UITransformReference == null)
                        AddIssue("UI Transform reference has not been assigned.", Prefix);
                    break;
            }
        }
    }
}