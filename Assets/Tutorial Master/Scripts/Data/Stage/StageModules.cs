using HardCodeLab.TutorialMaster.Localization;
using System;
using System.Collections.Generic;

namespace HardCodeLab.TutorialMaster
{
    [Serializable]
    [DataValidator(typeof(StageModulesValidator))]
    public class StageModules
    {
        /// <summary>
        /// All Arrow Module Configs for this Stage.
        /// </summary>
        [LocalizableField] public List<ArrowModuleConfig> Arrows;

        /// <summary>
        /// All Highlighter Module Configs for this Stage.
        /// </summary>
        [LocalizableField] public List<HighlightModuleConfig> Highlighters;

        /// <summary>
        /// All Image Module Configs for this Stage.
        /// </summary>
        [LocalizableField] public List<ImageModuleConfig> Images;

        /// <summary>
        /// All Pop-Up Module Configs for this Stage.
        /// </summary>
        [LocalizableField] public List<PopupModuleConfig> Popups;

        /// <summary>
        /// Initializes a new instance of the <see cref="StageModules"/> class.
        /// </summary>
        public StageModules()
        {
            Arrows = new List<ArrowModuleConfig>();
            Images = new List<ImageModuleConfig>();
            Highlighters = new List<HighlightModuleConfig>();
            Popups = new List<PopupModuleConfig>();
        }

        /// <summary>
        /// Returns the total number of modules
        /// </summary>
        public int TotalCount
        {
            get { return Arrows.Count + Images.Count + Highlighters.Count + Popups.Count; }
        }

        /// <summary>
        /// Activates all modules for this Stage.
        /// </summary>
        /// <param name="languageKey">The language key.</param>
        public void Activate(string languageKey = "")
        { 
            ActivateModule<ArrowModuleConfig, ArrowModule, ArrowModuleSettings>(Arrows, languageKey);
            ActivateModule<HighlightModuleConfig, HighlightModule, HighlightModuleSettings>(Highlighters, languageKey);
            ActivateModule<ImageModuleConfig, ImageModule, ImageModuleSettings>(Images, languageKey);
            ActivateModule<PopupModuleConfig, PopupModule, PopupModuleSettings>(Popups, languageKey);
        }

        /// <summary>
        /// Disables all modules for this Stage.
        /// </summary>
        public void Disable()
        {
            DisableModule<ArrowModuleConfig, ArrowModule, ArrowModuleSettings>(Arrows);
            DisableModule<HighlightModuleConfig, HighlightModule, HighlightModuleSettings>(Highlighters);
            DisableModule<ImageModuleConfig, ImageModule, ImageModuleSettings>(Images);
            DisableModule<PopupModuleConfig, PopupModule, PopupModuleSettings>(Popups);
        }

        /// <summary>
        /// Updates the language for all modules.
        /// </summary>
        /// <param name="languageKey">Unique key of a language which will be set for all modules.</param>
        public void SetLanguage(string languageKey)
        {
            SetModuleLanguage<ArrowModuleConfig, ArrowModule, ArrowModuleSettings>(Arrows, languageKey);
            SetModuleLanguage<HighlightModuleConfig, HighlightModule, HighlightModuleSettings>(Highlighters, languageKey);
            SetModuleLanguage<ImageModuleConfig, ImageModule, ImageModuleSettings>(Images, languageKey);
            SetModuleLanguage<PopupModuleConfig, PopupModule, PopupModuleSettings>(Popups, languageKey);
        }

        private void ActivateModule<TConfig, TModule, TSettings>(IEnumerable<TConfig> moduleConfigs, string languageKey)
                            where TConfig : ModuleConfig<TModule, TSettings>
            where TModule : Module
            where TSettings : ModuleSettings
        {
            foreach (var config in moduleConfigs)
            {
                if (!config.Enabled)
                    continue;
                
                config.Activate(languageKey);
            }
        }

        private void DisableModule<TConfig, TModule, TSettings>(IEnumerable<TConfig> moduleConfigs)
            where TConfig : ModuleConfig<TModule, TSettings>
            where TModule : Module
            where TSettings : ModuleSettings
        {
            foreach (var config in moduleConfigs)
            {
                if (!config.Enabled)
                    continue;

                config.Deactivate();
            }
        }

        private void SetModuleLanguage<TConfig, TModule, TSettings>(IEnumerable<TConfig> moduleConfigs, string languageKey)
            where TConfig : ModuleConfig<TModule, TSettings>
            where TModule : Module
            where TSettings : ModuleSettings
        {
            foreach (var config in moduleConfigs)
            {
                if (!config.Enabled)
                    continue;

                config.SetLanguage(languageKey);
            }
        }
    }
}