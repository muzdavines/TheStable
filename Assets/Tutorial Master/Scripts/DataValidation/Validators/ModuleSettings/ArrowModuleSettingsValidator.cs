using System;

namespace HardCodeLab.TutorialMaster
{
    public class ArrowModuleSettingsValidator : BaseModuleSettingsValidator<ArrowModuleSettings>
    {
        protected override string Prefix
        {
            get { return "Arrow Module"; }
        }

        public ArrowModuleSettingsValidator(Type dataType) : base(dataType)
        {
        }

        protected override void OnValidate(ArrowModuleSettings data)
        {
            base.OnValidate(data);

            if (data.PointDirection != PointDirection.LookAtTransform)
                return;

            if (data.PointTarget == null)
                AddIssue("Point Target is not specified", Prefix);
        }
    }
}