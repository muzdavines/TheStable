using System;

namespace HardCodeLab.TutorialMaster
{
    public class TutorialMasterManagerValidator : DataValidator<TutorialMasterManager>
    {
        public TutorialMasterManagerValidator(Type dataType) : base(dataType)
        {
        }

        protected override void OnValidate(TutorialMasterManager data)
        {
            string prefix = data.name;
            if (data.Tutorials == null)
            {
                AddIssue("Tutorials list has not been initialized!", prefix);
                return;
            }

            var tutorialValidator = DataValidatorResolver.Resolve<Tutorial>();

            for (var i = 0; i < data.Tutorials.Count; i++)
            {
                var tutorial = data.Tutorials[i];

                if (!tutorialValidator.Validate(tutorial))
                    AddIssues(tutorialValidator.Issues, string.Format("{0}>{1}", prefix, i));
            }
        }
    }
}