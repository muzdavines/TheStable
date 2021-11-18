using System;

namespace HardCodeLab.TutorialMaster
{
    [Serializable]
    public class ImageModuleConfig : ModuleConfig<ImageModule, ImageModuleSettings>
    {
        public ImageModuleConfig(TutorialMasterManager manager, string parentStagePath) : base(manager)
        {
        }

        protected override ImageModule GetModuleFromPool()
        {
            return ParentManager.ImageModulePool.AllocateModule();
        }
    }
}