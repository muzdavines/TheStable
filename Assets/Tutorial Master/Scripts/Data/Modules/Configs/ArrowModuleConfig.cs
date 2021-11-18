using System;

namespace HardCodeLab.TutorialMaster
{
    [Serializable]
    public class ArrowModuleConfig : ModuleConfig<ArrowModule, ArrowModuleSettings>
    {
        public ArrowModuleConfig(TutorialMasterManager manager, string parentStagePath) : base(manager)
        {
        }

        protected override ArrowModule GetModuleFromPool()
        {
            return ParentManager.ArrowModulePool.AllocateModule();
        }
    }
}