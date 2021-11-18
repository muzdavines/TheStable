namespace HardCodeLab.TutorialMaster
{
    public static class ImageModuleExtensions
    {
        /// <summary>
        /// Specifies whether an Image Module should be enabled or disabled.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        /// <param name="isActive">If true, module will be enabled. If false, module will be disabled.</param>
        public static void SetImageModuleActive(this Stage stage, int index, bool isActive)
        {
            stage.GetImageModules()[index].Enabled = isActive;
        }

        /// <summary>
        /// Gets value that indicates whether an Image Module is enabled or disabled.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        public static bool GetImageModuleActive(this Stage stage, int index)
        {
            return stage.GetImageModules()[index].Enabled;
        }

        /// <summary>
        /// Gets Image Module settings.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        public static ImageModuleSettings GetImageModuleSettings(this Stage stage, int index)
        {
            return stage.GetImageModules()[index].Settings;
        }

        /// <summary>
        /// Gets an Image Module component.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        /// <returns>A MonoBehaviour Module component.</returns>
        public static ImageModule GetImageModuleComponent(this Stage stage, int index)
        {
            return stage.GetImageModules()[index].Module;
        }

        /// <summary>
        /// Forces Image Module to re-calculate its transform.
        /// Useful for updating module settings at runtime without having to re-enter Stage.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        public static void UpdateImageModulePosition(this Stage stage, int index)
        {
            stage.GetImageModules()[index].Module.ApplyModuleChanges();
        }
    }
}