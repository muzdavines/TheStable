using System;
using System.Collections.Generic;
using System.Linq;

namespace HardCodeLab.TutorialMaster
{
    [Serializable]
    public class PopupPool : ModulePool<PopupModule>
    {
        protected override int CalculateMaxPoolSize(ref List<Tutorial> tutorials)
        {
            int targetPoolSize = 0;

            foreach (var tutorial in tutorials)
            {
                foreach (var stage in tutorial.Stages)
                {
                    int nModules = stage.Modules.Popups.Count(x => x.OverridePrefab == false);
                    if (nModules > targetPoolSize)
                    {
                        targetPoolSize = nModules;
                    }
                }
            }

            return targetPoolSize;
        }
    }
}