using System;

namespace HardCodeLab.TutorialMaster
{
    public abstract class BaseModuleSettingsValidator<TModuleSettings> : DataValidator<TModuleSettings>
        where TModuleSettings : ModuleSettings
    {
        protected abstract string Prefix { get; }

        protected BaseModuleSettingsValidator(Type dataType) 
            : base(dataType)
        {
        }

        protected override void OnValidate(TModuleSettings data)
        {
            if (!data.DataValidatorEnabled)
                return;

            if (data.TargetCanvas == null)
                AddIssue("Target Canvas is null", Prefix);

            if (data.PositionMode != PositionMode.TransformBased)
                return;

            switch (data.TargetType)
            {
                case TargetType.WorldSpace:
                    if (data.TransformTarget == null)
                        AddIssue("World Space Transform Target is null", Prefix);
                    break;

                case TargetType.CanvasSpace:
                    if (data.UITarget == null)
                        AddIssue("Canvas Space Transform Target is null", Prefix);
                    break;

                default:
                    AddIssue("Unexpected TargetType enum added", Prefix);
                    break;
            }
        }
    }
}