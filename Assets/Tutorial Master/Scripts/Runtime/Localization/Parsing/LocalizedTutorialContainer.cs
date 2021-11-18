using System.Collections.Generic;

namespace HardCodeLab.TutorialMaster.Localization
{
    public class LocalizedTutorialContainer
    {
        public List<LocalizedStageContainer> StageContainers;

        public LocalizedTutorialContainer()
        {
            StageContainers = new List<LocalizedStageContainer>();
        }
    }
}