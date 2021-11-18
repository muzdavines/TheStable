using UnityEditor;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    /// <summary>
    /// Responsible for adding a section for Tutorial Master inside the Preferences menu.
    /// </summary>
    public class TutorialMasterPreferenceItem
    {
#if UNITY_2019_1_OR_NEWER

        [SettingsProvider]
        public static SettingsProvider RenderSettings()
        {
            var provider = new SettingsProvider("Project/Tutorial Master", SettingsScope.Project,
                new[] { "Tutorial Master", "Tutorial Maker", "Tutorial" })
            {
                label = "Tutorial Master 2", 
                guiHandler = (searchContext) =>
                {
                    TMLogger.LoggingLevel = (LoggingLevel)EditorGUILayout.EnumPopup("Logging Level", TMLogger.LoggingLevel);
                }
            };

            return provider;
        }

#else

        [PreferenceItem("Tutorial Master")]
        public static void OnPreferenceGUI()
        {
            TMLogger.LoggingLevel = (LoggingLevel)EditorGUILayout.EnumPopup("Logging Level", TMLogger.LoggingLevel);
        }

#endif
    }
}