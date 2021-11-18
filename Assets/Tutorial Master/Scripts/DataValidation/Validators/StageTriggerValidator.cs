using System;

namespace HardCodeLab.TutorialMaster
{
    public class StageTriggerValidator : DataValidator<StageTrigger>
    {
        public StageTriggerValidator(Type dataType) : base(dataType)
        {
        }

        protected override void OnValidate(StageTrigger data)
        {
            const string prefix = "Trigger Settings";
            switch (data.Type)
            {
                case TriggerType.None:
                case TriggerType.AnyKeyPress:
                case TriggerType.KeyPress:
                    break;

                case TriggerType.Input:

                    if (string.IsNullOrEmpty(data.TriggerInput))
                        AddIssue("Trigger Input is empty", prefix);
                    break;

                case TriggerType.UnityEventInvoke:

                    if (data.EventTarget.Source == null)
                    {
                        AddIssue("UnityEvent source GameObject is null.", prefix);
                        break;
                    }
                    
                    if (data.EventTarget.Value == null)
                        AddIssue("UnityEvent Target couldn't be resolved.", prefix);

                    break;

                case TriggerType.UGUIButtonClick:
                    if (data.UIButtonTarget == null)
                        AddIssue("UGUI Button is not assigned.", prefix);
                    break;

                case TriggerType.Timer:

                    if (data.TriggerTimerAmount < 0)
                        AddIssue("Timer amount is negative.", prefix);
                    break;
            }

            if (data.Type == TriggerType.Timer)
                return;

            if (data.TriggerActivationDelay < 0)
                AddIssue("Activation Delay is negative.", prefix);
        }
    }
}