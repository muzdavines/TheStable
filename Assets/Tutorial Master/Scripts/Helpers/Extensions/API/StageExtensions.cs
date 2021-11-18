using System.Collections.Generic;

namespace HardCodeLab.TutorialMaster
{
    public static class StageExtensions
    {
        /// <summary>
        /// Gets all Arrow Modules of this Stage.
        /// </summary>
        /// <param name="stage">Stage which modules will be retrieved.</param>
        /// <returns>Returns list of modules.</returns>
        public static List<ArrowModuleConfig> GetArrowModules(this Stage stage)
        {
            return stage.Modules.Arrows;
        }

        /// <summary>
        /// Gets all Highlighter Modules of this Stage.
        /// </summary>
        /// <param name="stage">Stage which modules will be retrieved.</param>
        /// <returns>Returns list of modules.</returns>
        public static List<HighlightModuleConfig> GetHighlighterModules(this Stage stage)
        {
            return stage.Modules.Highlighters;
        }

        /// <summary>
        /// Gets all Pop-Up Modules of this Stage.
        /// </summary>
        /// <param name="stage">Stage which modules will be retrieved.</param>
        /// <returns>Returns list of modules.</returns>
        public static List<PopupModuleConfig> GetPopupModules(this Stage stage)
        {
            return stage.Modules.Popups;
        }

        /// <summary>
        /// Gets all Pop-Up Modules of this Stage.
        /// </summary>
        /// <param name="stage">Stage which modules will be retrieved.</param>
        /// <returns>Returns list of modules.</returns>
        public static List<ImageModuleConfig> GetImageModules(this Stage stage)
        {
            return stage.Modules.Images;
        }
    }
}