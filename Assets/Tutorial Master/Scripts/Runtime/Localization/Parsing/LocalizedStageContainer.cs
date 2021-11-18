using System.Collections.Generic;

namespace HardCodeLab.TutorialMaster.Localization
{
    public class LocalizedStageContainer
    {
        public List<LocalizedModuleContentContainer<ArrowModuleSettings>> ArrowContainers;
        public List<LocalizedModuleContentContainer<HighlightModuleSettings>> HighlighterContainers;
        public List<LocalizedModuleContentContainer<ImageModuleSettings>> ImageContainers;
        public List<LocalizedModuleContentContainer<PopupModuleSettings>> PopupContainers;

        public LocalizedStageContainer()
        {
            PopupContainers = new List<LocalizedModuleContentContainer<PopupModuleSettings>>();
            ArrowContainers = new List<LocalizedModuleContentContainer<ArrowModuleSettings>>();
            ImageContainers = new List<LocalizedModuleContentContainer<ImageModuleSettings>>();
            HighlighterContainers = new List<LocalizedModuleContentContainer<HighlightModuleSettings>>();
        }
    }
}