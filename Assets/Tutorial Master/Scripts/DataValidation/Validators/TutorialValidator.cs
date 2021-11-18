using System;

namespace HardCodeLab.TutorialMaster
{
    public class TutorialValidator : DataValidator<Tutorial>
    {
        public TutorialValidator(Type dataType) : base(dataType)
        {
        }

        protected override void OnValidate(Tutorial data)
        {
            if (!data.DataValidatorEnabled)
                return;

            var stageValidator = DataValidatorResolver.Resolve<Stage>();

            string prefix = string.Format("Tutorial \"{0}\"", data.Name);
            if (data.Stages == null)
            {
                AddIssue("Stages list have not been initialized!", prefix);
                return;
            }

            for (int i = 0; i < data.Stages.Count; i++)
            {
                var stage = data.Stages[i];
                if (!stageValidator.Validate(stage))
                    AddIssues(stageValidator.Issues, string.Format("{0}>{1}", prefix, i));
            }
        }
    }
}