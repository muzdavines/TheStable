using System.Collections.Generic;

namespace HardCodeLab.TutorialMaster.Localization
{
    /// <summary>
    /// Stores moduleId of the module and the localized content
    /// </summary>
    public class LocalizedModuleContentContainer<TSettings>
        where TSettings : ModuleSettings
    {
        public LocalizedModuleContentContainer(string moduleId, Dictionary<string, string> localizedContent)
        {
            ModuleId = moduleId;
            LocalizedContent = localizedContent;
        }

        public Dictionary<string, string> LocalizedContent { get; private set; }
        public string ModuleId { get; private set; }
    }
}