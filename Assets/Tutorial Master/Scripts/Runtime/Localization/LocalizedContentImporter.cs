using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HardCodeLab.TutorialMaster
{
    public static class LocalizedContentImporter
    {
        public static void Import(ref TutorialMasterManager manager, string importDirectory)
        {
            using (var reader = new StreamReader(new FileStream(importDirectory, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8))
            {
                int cTutorialIndex = -1;
                int cStageIndex = -1;

                var languageRegex = new Regex("\\((.*?)\\)");
                var tutorialRegex = new Regex("\\((.*?)\\) Tutorial -");
                var stageRegex = new Regex("\\((.*?)\\) Stage -");
                var moduleRegex = new Regex("\\((.*?)\\) PopupModule \\[(.*?)\\]");

                string languagesString = reader.ReadLine().Trim(',');

                string[] languageKeys = languagesString.Split(',').Select(x => languageRegex.Match(x.Clean()).Groups[1].Value).ToArray();

                int[] languageIndexes = new int[languageKeys.Length];

                for (int i = 0; i < languageKeys.Length; i++)
                {
                    languageIndexes[i] = FindLanguageIndex(ref manager, languageKeys[i]);
                }

                while (!reader.EndOfStream)
                {
                    string currentLine = reader.ReadLine();

                    // skip this line if it begins with #
                    if (currentLine.StartsWith("#")) continue;

                    // check if this line is setting new tutorial
                    var match = tutorialRegex.Match(currentLine);

                    if (match.Success)
                    {
                        string tutorialKey = match.Groups[1].Value;
                        cTutorialIndex = FindTutorialIndex(ref manager, tutorialKey);

                        continue;
                    }

                    // check if this line is setting new stage
                    match = stageRegex.Match(currentLine);

                    if (match.Success)
                    {
                        string stageKey = match.Groups[1].Value;
                        cStageIndex = FindStageIndex(ref manager, cTutorialIndex, stageKey);

                        continue;
                    }

                    // check if tutorial and stage indexes are valid
                    if (cTutorialIndex != -1 && cStageIndex != -1)
                    {
                        match = moduleRegex.Match(currentLine);

                        if (match.Success)
                        {
                            var localizedContent = currentLine.Replace(match.Value, "").Trim(',').Split(',');
                            var popupConfigIndex = FindModuleConfigIndex(ref manager, cTutorialIndex, cStageIndex,
                                match.Groups[1].Value);

                            if (popupConfigIndex >= 0)
                            {
                                var popupSettings = manager.Tutorials[cTutorialIndex].Stages[cStageIndex].Modules.Popups[popupConfigIndex].Settings;

                                if (match.Groups[2].Value.Equals("PopupMessage"))
                                {
                                    for (int i = 0; i < localizedContent.Length; i++)
                                    {
                                        popupSettings.PopupMessage.SetContent(languageKeys[i], localizedContent[i]);
                                    }
                                }
                                else if (match.Groups[2].Value.Equals("ButtonLabel"))
                                {
                                    for (int i = 0; i < localizedContent.Length; i++)
                                    {
                                        popupSettings.ButtonLabel.SetContent(languageKeys[i], localizedContent[i]);
                                    }
                                }
                                else if (match.Groups[2].Value.Equals("PopupTitle"))
                                {
                                    for (int i = 0; i < localizedContent.Length; i++)
                                    {
                                        popupSettings.PopupTitle.SetContent(languageKeys[i], localizedContent[i]);
                                    }
                                }

                                manager.Tutorials[cTutorialIndex].Stages[cStageIndex].Modules.Popups[popupConfigIndex].Settings = popupSettings;
                            }
                        }
                    }
                }
            }

            /*
             * Take first line and split them into an array of strings, retrieving a key of each language)
             * Begin going through each lines of the file (ignore the first one)
             * If found a line that contains "Tutorial", then try finding the tutorial index based on an stageId
             * [If not found, set current tutorial index to -1 and ignore lines until the next "Tutorial"]
             * If found, update the current tutorial index
             * If found a line that contains "Stage", then try finding the stage index based on an stageId
             * [If not found, set current stage index to -1 and ignore lines until the next "Stage" or "Tutorial"]
             * If found, update the current stage index
             * Check what's the type of Module is there. If it's a PopupModule, then try finding a popupmodule config index based on stageId
             * [If not found, ignore the line]
             * If found, split the current line into an array by , (ignore strings inside of quotation marks).
             * Check which field does the content belong to. Once found, iterate through each language content and allocate localized content to it. Also clean the localized content before doing so
             */
        }

        /// <summary>
        /// Cleans the specified string from CSV rulesets
        /// </summary>
        /// <param name="str">The string which will be cleaned.</param>
        /// <returns>Cleaned string</returns>
        private static string Clean(this string str)
        {
            //TODO: Make this work!

            // if string is surrounded by quotes that means it needs to be cleaned up before being used
            bool toClean = str[0].Equals('"') && str[str.Length - 1].Equals('"');

            /*
             *  Detect if there are double quotes
             *  [If no, then output normal text]
             *  Get rid of double quotes (so substring first and last characters off string)
             *  Locate a quotation mark
             *  Get rid of outer quotation marks
             *  Combine everything back and return a new string
             */

            if (toClean)
            {
                // get rid of quotes
                //string unquotedText = str.Substring(1, str.Length - 2);
                //string[] words = unquotedText.Split(' ');
            }

            return str;
        }

        private static int FindLanguageIndex(ref TutorialMasterManager manager, string languageKey)
        {
            for (int i = 0; i < manager.LocalizationData.Languages.Count; i++)
            {
                if (manager.LocalizationData.Languages[i].Id.Equals(languageKey))
                {
                    return i;
                }
            }

            return -1;
        }

        private static int FindModuleConfigIndex(ref TutorialMasterManager manager, int tutorialIndex, int stageIndex, string moduleConfigKey)
        {
            if (tutorialIndex >= 0 && stageIndex >= 0)
            {
                var moduleConfigs = manager.Tutorials[tutorialIndex].Stages[stageIndex].Modules.Popups;

                for (int i = 0; i < moduleConfigs.Count; i++)
                {
                    if (moduleConfigs[i].Id.Equals(moduleConfigKey))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private static int FindStageIndex(ref TutorialMasterManager manager, int tutorialIndex, string stageId)
        {
            if (tutorialIndex >= 0)
            {
                for (int i = 0; i < manager.Tutorials[tutorialIndex].Stages.Count; i++)
                {
                    if (manager.Tutorials[tutorialIndex].Stages[i].Id.Equals(stageId))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private static int FindTutorialIndex(ref TutorialMasterManager manager, string tutorialId)
        {
            for (int i = 0; i < manager.Tutorials.Count; i++)
            {
                if (manager.Tutorials[i].Id.Equals(tutorialId))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}