using System;

namespace HardCodeLab.TutorialMaster
{
    public class StageModulesValidator : DataValidator<StageModules>
    {
        public StageModulesValidator(Type dataType) : base(dataType)
        {
        }

        protected override void OnValidate(StageModules data)
        {
            var arrowSettingsValidator = DataValidatorResolver.Resolve<ArrowModuleSettings>();
            var highlighterSettingsValidator = DataValidatorResolver.Resolve<HighlightModuleSettings>();
            var imageSettingsValidator = DataValidatorResolver.Resolve<ImageModuleSettings>();
            var popupSettingsValidator = DataValidatorResolver.Resolve<PopupModuleSettings>();

            for (var i = 0; i < data.Arrows.Count; i++)
            {
                var settings = data.Arrows[i].Settings;
                if (!arrowSettingsValidator.Validate(settings))
                {
                    AddIssues(arrowSettingsValidator.Issues, i.ToString());
                }
            }

            for (var i = 0; i < data.Highlighters.Count; i++)
            {
                var settings = data.Highlighters[i].Settings;
                if (!highlighterSettingsValidator.Validate(settings))
                {
                    AddIssues(highlighterSettingsValidator.Issues, i.ToString());
                }
            }

            for (var i = 0; i < data.Images.Count; i++)
            {
                var settings = data.Images[i].Settings;
                if (!imageSettingsValidator.Validate(settings))
                {
                    AddIssues(imageSettingsValidator.Issues, i.ToString());
                }
            }

            for (var i = 0; i < data.Popups.Count; i++)
            {
                var settings = data.Popups[i].Settings;
                if (!popupSettingsValidator.Validate(settings))
                {
                    AddIssues(popupSettingsValidator.Issues, i.ToString());
                }
            }
        }
    }
}