using System;

namespace HardCodeLab.TutorialMaster
{
    public class StageValidator : DataValidator<Stage>
    {
        public StageValidator(Type dataType) : base(dataType)
        {
        }

        protected override void OnValidate(Stage data)
        {
            if (!data.DataValidatorEnabled)
                return;

            string prefix = string.Format("Stage \"{0}\"", data.Name);

            var audioValidator = DataValidatorResolver.Resolve<StageAudio>();
            var triggerValidator = DataValidatorResolver.Resolve<StageTrigger>();
            var eventsValidator = DataValidatorResolver.Resolve<StageEvents>();
            var modulesValidator = DataValidatorResolver.Resolve<StageModules>();

            if (!audioValidator.Validate(data.Audio))
                AddIssues(audioValidator.Issues, prefix);

            if (!triggerValidator.Validate(data.Trigger))
                AddIssues(triggerValidator.Issues, prefix);

            if (!eventsValidator.Validate(data.Events))
                AddIssues(eventsValidator.Issues, prefix);

            if (!modulesValidator.Validate(data.Modules))
                AddIssues(modulesValidator.Issues, prefix);
        }
    }
}