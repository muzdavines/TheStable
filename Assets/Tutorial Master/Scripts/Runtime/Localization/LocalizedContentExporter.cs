using HardCodeLab.TutorialMaster.Localization;
using System.Collections.Generic;
using System.Linq;

namespace HardCodeLab.TutorialMaster
{
    public static class LocalizedContentExporter
    {
        //public static bool Export(ref TutorialMasterManager manager, string exportDirectory)
        //{
        //    using (var sw = new StreamWriter(exportDirectory, false, Encoding.UTF8))
        //    {
        //        var languages = manager.LocalizationData.Languages;
        //        var tutorials = manager.Tutorials;

        //        sw.Write(",");

        //        // populate first line with language along with their respected ID
        //        foreach (var language in languages)
        //        {
        //            sw.Write(string.Format("({0}) {1}", language.Id, language.Name).Sanitize() + ",");
        //        }

        //        sw.WriteLine();

        //        foreach (var tutorial in tutorials)
        //        {
        //            sw.WriteLine(string.Format("({0}) Tutorial - \"{1}\"", tutorial.Id, tutorial.Name).Sanitize());

        //            foreach (var stage in tutorial.Stages)
        //            {
        //                sw.WriteLine(string.Format("({0}) Stage - \"{1}\"", stage.Id, stage.Name).Sanitize());

        //                // TODO: get modules that contain localized content through reflection
        //                /*
        //                 *  Get the StageModules object of a currently iterated stage
        //                 *  Get all properties which are List<ModuleConfig> type
        //                 *  Iterate through List<ModuleConfig>
        //                 *  If the first ModuleConfig's ModuleSettings doesn't contain LocalizedString, then break the loop and move on
        //                 *  Otherwise, retrieve the content of a string and put it into the following format:
        //                 *              "(config_id) ModuleType [field-name],{content-in-language-a},{content-in-language-b}" (append to the csv)
        //                 *  Do same thing if there are other LocalizedString properties
        //                 */

        //                foreach (var popupModule in stage.Modules.Popups)
        //                {
        //                    sw.Write(string.Format("{{{0}}} PopupModule [PopupMessage]", popupModule.Id).Sanitize() + ",");
        //                    foreach (var lang in languages)
        //                    {
        //                        sw.Write(popupModule.Settings.PopupMessage.GetContent(lang.Id).Sanitize() + ",");
        //                    }

        //                    sw.WriteLine();
        //                    sw.Write(string.Format("{{{0}}} PopupModule [ButtonLabel]", popupModule.Id).Sanitize() + ",");
        //                    foreach (var lang in languages)
        //                    {
        //                        sw.Write(popupModule.Settings.ButtonLabel.GetContent(lang.Id).Sanitize() + ",");
        //                    }

        //                    sw.WriteLine();
        //                    sw.Write(string.Format("{{{0}}} PopupModule [PopupTitle]", popupModule.Id).Sanitize() + ",");
        //                    foreach (var lang in languages)
        //                    {
        //                        sw.Write(popupModule.Settings.PopupTitle.GetContent(lang.Id).Sanitize() + ",");
        //                    }

        //                    sw.WriteLine();
        //                }
        //            }
        //        }
        //    }

        //    return true;
        //}

        public static bool Export(ref List<Tutorial> tutorials, string exportDirectory)
        {
            var tutorialContainers = new List<LocalizedTutorialContainer>();

            foreach (var tutorial in tutorials)
            {
                var tutorialContainer = new LocalizedTutorialContainer();

                foreach (var stage in tutorial.Stages)
                {
                    var stageContainer = new LocalizedStageContainer();

                    foreach (var moduleConfig in stage.Modules.Popups)
                    {
                        stageContainer.PopupContainers = GetModuleContainers(moduleConfig.Settings, moduleConfig.Id);
                    }

                    foreach (var moduleConfig in stage.Modules.Arrows)
                    {
                        stageContainer.ArrowContainers = GetModuleContainers(moduleConfig.Settings, moduleConfig.Id);
                    }

                    foreach (var moduleConfig in stage.Modules.Images)
                    {
                        stageContainer.ImageContainers = GetModuleContainers(moduleConfig.Settings, moduleConfig.Id);
                    }

                    foreach (var moduleConfig in stage.Modules.Highlighters)
                    {
                        stageContainer.HighlighterContainers = GetModuleContainers(moduleConfig.Settings, moduleConfig.Id);
                    }

                    tutorialContainer.StageContainers.Add(stageContainer);
                }

                tutorialContainers.Add(tutorialContainer);
            }

            // this is where tutorialContainers is passed to CSVHelper (or other parsing library) to produce a CSV file

            return false;
        }

        private static List<LocalizedModuleContentContainer<TSettings>> GetModuleContainers<TSettings>(TSettings moduleSettings, string moduleId) where TSettings : ModuleSettings
        {
            var contentContainers = new List<LocalizedModuleContentContainer<TSettings>>();

            var localizedStringFields = typeof(TSettings).GetFields()
                .Where(info => info.FieldType.IsSubclassOf(typeof(LocalizedContent<string>))).ToArray();

            if (localizedStringFields.Length == 0) return null;

            foreach (var fieldInfo in localizedStringFields)
            {
                var localizedString = fieldInfo.GetValue(moduleSettings) as LocalizedString;
                if (localizedString == null) continue;

                var contentContainer = new LocalizedModuleContentContainer<TSettings>(moduleId, localizedString.Content);
                contentContainers.Add(contentContainer);
            }

            return contentContainers;
        }
    }
}