namespace HardCodeLab.TutorialMaster
{
    public static class PopupModuleExtensions
    {
        /// <summary>
        /// Specifies whether an Popup Module should be enabled or disabled.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        /// <param name="isActive">If true, module will be enabled. If false, module will be disabled.</param>
        public static void SetPopupModuleActive(this Stage stage, int index, bool isActive)
        {
            stage.GetPopupModules()[index].Enabled = isActive;
        }

        /// <summary>
        /// Gets value that indicates whether an Popup Module is enabled or disabled.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        public static bool GetPopupModuleActive(this Stage stage, int index)
        {
            return stage.GetPopupModules()[index].Enabled;
        }

        /// <summary>
        /// Gets Popup Module settings.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        public static PopupModuleSettings GetPopupModuleSettings(this Stage stage, int index)
        {
            return stage.GetPopupModules()[index].Settings;
        }

        /// <summary>
        /// Gets an Popup Module component.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        /// <returns>A MonoBehaviour Module component.</returns>
        public static PopupModule GetPopupModuleComponent(this Stage stage, int index)
        {
            return stage.GetPopupModules()[index].Module;
        }

        /// <summary>
        /// Forces Popup Module to re-calculate its transform.
        /// Useful for updating module settings at runtime without having to re-enter Stage.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        public static void UpdatePopupModulePosition(this Stage stage, int index)
        {
            stage.GetPopupModules()[index].Module.ApplyModuleChanges();
        }
    }
}