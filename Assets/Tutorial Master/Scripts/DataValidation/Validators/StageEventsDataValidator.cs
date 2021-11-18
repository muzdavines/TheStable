using System;

namespace HardCodeLab.TutorialMaster
{
    public class StageEventsDataValidator : DataValidator<StageEvents>
    {
        public StageEventsDataValidator(Type dataType) : base(dataType)
        {
        }

        protected override void OnValidate(StageEvents data)
        {
            const string prefix = "Events Settings";
            if (data.OnStageEnter == null)
                AddIssue("OnStageEnter is null.", prefix);

            if (data.OnStageExit == null)
                AddIssue("OnStageExit is null.", prefix);
        }
    }
}