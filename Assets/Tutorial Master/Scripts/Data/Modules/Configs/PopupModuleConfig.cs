using System;

namespace HardCodeLab.TutorialMaster
{
    [Serializable]
    public class PopupModuleConfig : ModuleConfig<PopupModule, PopupModuleSettings>
    {
        public PopupModuleConfig(TutorialMasterManager manager) : base(manager)
        {
        }

        protected override PopupModule GetModuleFromPool()
        {
            return ParentManager.PopupModulePool.AllocateModule();
        }
    }
}