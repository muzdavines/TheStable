using System;

namespace HardCodeLab.TutorialMaster
{
    public class StageAudioValidator : DataValidator<StageAudio>
    {
        public StageAudioValidator(Type dataType) : base(dataType)
        {
        }

        protected override void OnValidate(StageAudio data)
        {
            const string prefix = "Audio Settings";
            if (data.Timing == AudioTiming.Never)
                return;

            if (data.Source == null)
                AddIssue("AudioSource is not assigned.", prefix);
        }
    }
}