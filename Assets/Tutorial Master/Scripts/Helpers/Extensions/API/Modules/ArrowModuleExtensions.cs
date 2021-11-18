namespace HardCodeLab.TutorialMaster
{
    public static class ArrowModuleExtensions
    {
        /// <summary>
        /// Specifies whether an Arrow Module should be enabled or disabled.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        /// <param name="isActive">If true, module will be enabled. If false, module will be disabled.</param>
        public static void SetArrowModuleActive(this Stage stage, int index, bool isActive)
        {
            stage.GetArrowModules()[index].Enabled = isActive;
        }

        /// <summary>
        /// Gets value that indicates whether an Arrow Module is enabled or disabled.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        public static bool GetArrowModuleActive(this Stage stage, int index)
        {
            return stage.GetArrowModules()[index].Enabled;
        }

        /// <summary>
        /// Gets Arrow Module settings.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        public static ArrowModuleSettings GetArrowModuleSettings(this Stage stage, int index)
        {
            return stage.GetArrowModules()[index].Settings;
        }

        /// <summary>
        /// Gets an Arrow Module component.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        /// <returns>A MonoBehaviour Module component.</returns>
        public static ArrowModule GetArrowModuleComponent(this Stage stage, int index)
        {
            return stage.GetArrowModules()[index].Module;
        }

        /// <summary>
        /// Forces Arrow Module to re-calculate its transform.
        /// Useful for updating module settings at runtime without having to re-enter Stage.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        public static void UpdateArrowModulePosition(this Stage stage, int index)
        {
            stage.GetArrowModules()[index].Module.ApplyModuleChanges();
        }
    }
}