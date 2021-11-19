using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace NestedTODO
{
    public class DefaultConfiguration : ScriptableObject
    {
        /* Routes for opening the editors windows
         * to add hotkeys, add %(ctrl on windows, cmd on OS X) #(shift) and/or &(alt)
         * followed for the key you want to asign as a hotkey
         * e.g. "Window/NestedTODO/Checklist Window #&c"
         */
        public const string ChecklistWindowRoute = "Window/NestedTODO/Checklist Window";
        public const string AgileBoardWindowRoute = "Window/NestedTODO/Agile Board";

        [Header("General")]
        public bool defaultAutoComplete = true;
        public bool defaultAutoCategorize = true;
        public bool defaultAutoPrioritize = true;
        public bool defaultAutoTag = true;
        public bool defaultInheritCategory = true;
        public bool defaultInheritPriority = true;
        public bool defaultInheritTag = true;
        public bool defaultInheritLinkedFile = true;
        public bool defaultConfirmParentDelete = true;
        public bool defaultAutoSelectNewTask = true;
        public bool defaultScrollToActiveTask = true;

        [Header("Visibility")]
        public bool defaultShowExtendedToolbar = true;
        public bool defaultHideParents = false;
        public bool defaultHideNotes = false;
        public bool defaultCompletedAt = false;
        public bool defaultHideCategories = false;
        public bool defaultHidePriorityLevels = false;
        public bool defaultHideTags = false;
        public bool defaultHideProgressBars = false;

        [Header("Lists")]
        public List<string> DefaultCategories;
        public List<Priority> DefaultPriorities;
        public List<string> DefaultTags;

        [Header("Colors")]
        public Color[] DefaultPersonalSkinColors;
        public Color[] DefaultProfessionalSkinColors;

        [Header("Export")]
        public string defaultCsvSeparator = ",";
        public bool defaultExportCompletedAt = true;
        public bool defaultExportProgress = true;
        public bool defaultExportCategory = true;
        public bool defaultExportPriority = true;
        public bool defaultExportTag = true;
        public int defaultEncodingCode = System.Text.Encoding.Default.CodePage;

        void OnEnable()
        {
            if (DefaultCategories == null || DefaultCategories.Count == 0)
            {
                DefaultCategories = new List<string>();
                DefaultCategories.Add("Categories/None");
                DefaultCategories.Add("Categories/Design");
                DefaultCategories.Add("Categories/Scripting");
            }

            if (DefaultPriorities == null || DefaultPriorities.Count == 0)
            {
                DefaultPriorities = new List<Priority>();
                DefaultPriorities.Add(new Priority("Priorities/Normal", Color.green));
                DefaultPriorities.Add(new Priority("Priorities/High", Color.red));
                DefaultPriorities.Add(new Priority("Priorities/Low", Color.yellow));
            }

            if (DefaultTags == null || DefaultTags.Count == 0)
            {
                DefaultTags = new List<string>();
                DefaultTags.Add("Tags/Untagged");
            }

            if (DefaultPersonalSkinColors == null || DefaultPersonalSkinColors.Length != 7)
            {
                var skinColors = new Color[7];

                skinColors[0] = new Color(212f/255f,212f/255f,212f/255f,1);                     //highlight line color
                skinColors[1] = new Color(70f / 255f, 70f / 255f, 70f / 255f, 1);               //odd line color
                skinColors[2] = new Color(126f / 255f, 126f / 255f, 126f / 255f, 1);            //even line color
                skinColors[3] = new Color(1,1,1,1);                                             //uncompleted text color
                skinColors[4] = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1);            //completed text color
                skinColors[5] = new Color(0,0,0, 1);                                            //highlight text color
                skinColors[6] = new Color(1, 235f / 255f, 4f / 255f, 1);                        //note mark color

                DefaultPersonalSkinColors = skinColors;
            }

            if (DefaultProfessionalSkinColors == null || DefaultProfessionalSkinColors.Length != 7)
            {
                var skinColors = new Color[7];

                skinColors[0] = new Color(110f / 255f, 113f / 255f, 0, 172f / 255f);            //highlight line color
                skinColors[1] = new Color(16f / 255f, 16f / 255f, 16f / 255f, 123f / 255f);     //odd line color
                skinColors[2] = new Color(70f / 255f, 70f / 255f, 70f / 255f, 231f / 255f);     //even line color
                skinColors[3] = new Color(238f / 255f, 238f / 255f, 238f / 255f, 1);            //uncompleted text color
                skinColors[4] = new Color(119f / 255f, 119f / 255f, 119f / 255f, 1);            //completed text color
                skinColors[5] = new Color(1, 1, 1, 1);                                          //highlight text color
                skinColors[6] = new Color(1, 235f / 255f, 4f / 255f, 1);                        //note mark color

                DefaultProfessionalSkinColors = skinColors;
            }
        }

        public static DefaultConfiguration GetConfiguration()
        {
            var search = AssetDatabase.FindAssets("t:DefaultConfiguration");
            DefaultConfiguration config;
            if (search.Length == 0)
            {
                config = ScriptableObject.CreateInstance<DefaultConfiguration>();
                AssetDatabase.CreateAsset(config, ChecklistWindow.rootFolderPath + "Scripts/Editor/DefaultConfiguration.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                config = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(search[0]), typeof(DefaultConfiguration)) as DefaultConfiguration;
            }

            return config;
        }

        public static void SaveConfiguration(ListConfiguration configuration, int menu = -1)
        {
            DefaultConfiguration defaultConfig = GetConfiguration();
            SerializedObject serializedConfig = new SerializedObject(defaultConfig);

            switch (menu)
            {
                case 0:
                    serializedConfig.FindProperty("defaultAutoComplete").boolValue = configuration.AutoComplete;
                    serializedConfig.FindProperty("defaultAutoCategorize").boolValue = configuration.AutoCategory;
                    serializedConfig.FindProperty("defaultAutoPrioritize").boolValue = configuration.AutoPriority;
                    serializedConfig.FindProperty("defaultAutoTag").boolValue = configuration.AutoTag;
                    serializedConfig.FindProperty("defaultInheritCategory").boolValue = configuration.InheritCategory;
                    serializedConfig.FindProperty("defaultInheritPriority").boolValue = configuration.InheritPriority;
                    serializedConfig.FindProperty("defaultInheritTag").boolValue = configuration.InheritTag;
                    serializedConfig.FindProperty("defaultInheritLinkedFile").boolValue = configuration.InheritLinkedFile;

                    serializedConfig.FindProperty("defaultShowExtendedToolbar").boolValue = configuration.ShowExtendedToolbar;
                    serializedConfig.FindProperty("defaultAutoSelectNewTask").boolValue = configuration.AutoSelectNewTask;
                    serializedConfig.FindProperty("defaultScrollToActiveTask").boolValue = configuration.ScrollToActiveTask;
                    serializedConfig.FindProperty("defaultHideParents").boolValue = configuration.HideParents;
                    serializedConfig.FindProperty("defaultConfirmParentDelete").boolValue = configuration.ConfirmParentDelete;

                    serializedConfig.FindProperty("defaultHideNotes").boolValue = configuration.HideNotes;
                    serializedConfig.FindProperty("defaultHideCompletedAt").boolValue = configuration.HideCompletedAt;
                    serializedConfig.FindProperty("defaultHideCategories").boolValue = configuration.HideCategories;
                    serializedConfig.FindProperty("defaultHideProgressBars").boolValue = configuration.HideProgressBars;
                    serializedConfig.FindProperty("defaultHidePriorityLevels").boolValue = configuration.HidePriorityLevels;
                    serializedConfig.FindProperty("defaultHideTags").boolValue = configuration.HideTags;

                    serializedConfig.ApplyModifiedProperties();
                    break;
                case 1:
                    serializedConfig.FindProperty("DefaultCategories").arraySize = configuration.categories.Count;
                    for(int i = 0; i < configuration.categories.Count; i++)
                    {
                        serializedConfig.FindProperty("DefaultCategories").GetArrayElementAtIndex(i).stringValue = configuration.categories[i];
                    }

                    serializedConfig.ApplyModifiedProperties();
                    break;
                case 2:
                    var priorities = serializedConfig.FindProperty("DefaultPriorities");
                    priorities.arraySize = configuration.priorities.Count;
                    for (int i = 0; i < configuration.priorities.Count; i++)
                    {
                        priorities.GetArrayElementAtIndex(i).FindPropertyRelative("priorityText").stringValue = configuration.priorities[i].priorityText;
                        priorities.GetArrayElementAtIndex(i).FindPropertyRelative("priorityColor").colorValue = configuration.priorities[i].priorityColor;
                    }

                    serializedConfig.ApplyModifiedProperties();
                    break;
                case 3:
                    serializedConfig.FindProperty("DefaultTags").arraySize = configuration.tags.Count;
                    for (int i = 0; i < configuration.tags.Count; i++)
                    {
                        serializedConfig.FindProperty("DefaultTags").GetArrayElementAtIndex(i).stringValue = configuration.tags[i];
                    }

                    serializedConfig.ApplyModifiedProperties();
                    break;
                case 4:
                    string targetScheme = EditorGUIUtility.isProSkin ? "DefaultProfessionalSkinColors" : "DefaultPersonalSkinColors";
                    var colors = serializedConfig.FindProperty(targetScheme);
                    colors.arraySize = configuration.skinColors.Length;
                    for (int i = 0; i < configuration.skinColors.Length; i++)
                    {
                        colors.GetArrayElementAtIndex(i).colorValue = configuration.skinColors[i];
                    }

                    serializedConfig.ApplyModifiedProperties();
                    break;
                case 5:
                    serializedConfig.FindProperty("defaultCsvSeparator").stringValue = configuration.CsvSeparator;
                    serializedConfig.FindProperty("defaultExportCompletedAt").boolValue = configuration.ExportCompletedAt;
                    serializedConfig.FindProperty("defaultExportProgress").boolValue = configuration.ExportProgress;
                    serializedConfig.FindProperty("defaultExportCategory").boolValue = configuration.ExportCategory;
                    serializedConfig.FindProperty("defaultExportPriority").boolValue = configuration.ExportPriority;
                    serializedConfig.FindProperty("defaultExportTag").boolValue = configuration.ExportTags;
                    serializedConfig.FindProperty("defaultEncodingCode").intValue = configuration.EncodingCode;
                    break;
                default:
                    SaveConfiguration(configuration, 0);
                    SaveConfiguration(configuration, 1);
                    SaveConfiguration(configuration, 2);
                    SaveConfiguration(configuration, 3);
                    SaveConfiguration(configuration, 4);
                    SaveConfiguration(configuration, 5);
                    break;
            }
}

    }
}