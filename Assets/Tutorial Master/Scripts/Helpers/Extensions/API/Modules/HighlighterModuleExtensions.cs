namespace HardCodeLab.TutorialMaster
{
    public static class HighlighterModuleExtensions
    {
        /// <summary>
        /// Specifies whether an Highlighter Module should be enabled or disabled.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        /// <param name="isActive">If true, module will be enabled. If false, module will be disabled.</param>
        public static void SetHighlighterModuleActive(this Stage stage, int index, bool isActive)
        {
            stage.GetHighlighterModules()[index].Enabled = isActive;
        }

        /// <summary>
        /// Gets value that indicates whether an Highlighter Module is enabled or disabled.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        public static bool GetHighlighterModuleActive(this Stage stage, int index)
        {
            return stage.GetHighlighterModules()[index].Enabled;
        }

        /// <summary>
        /// Gets Highlighter Module settings.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        public static HighlightModuleSettings GetHighlighterModuleSettings(this Stage stage, int index)
        {
            return stage.GetHighlighterModules()[index].Settings;
        }

        /// <summary>
        /// Gets an Highlighter Module component.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        /// <returns>A MonoBehaviour Module component.</returns>
        public static HighlightModule GetHighlighterModuleComponent(this Stage stage, int index)
        {
            return stage.GetHighlighterModules()[index].Module;
        }

        /// <summary>
        /// Forces Highlighter Module to re-calculate its transform.
        /// Useful for updating module settings at runtime without having to re-enter Stage.
        /// </summary>
        /// <param name="stage">Associated Stage.</param>
        /// <param name="index">Index of the module which will be affected.</param>
        public static void UpdateHighlighterModulePosition(this Stage stage, int index)
        {
            stage.GetHighlighterModules()[index].Module.ApplyModuleChanges();
        }
    }
}