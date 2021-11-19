using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace NestedTODO
{
    [System.Serializable]
    public class ListConfiguration
    {
        //behavior config
        [SerializeField, HideInInspector]
        private bool autoComplete;
        public bool AutoComplete { get { return autoComplete; } set { autoComplete = value; } }
        [SerializeField, HideInInspector]
        private bool autoCategory;
        public bool AutoCategory { get { return autoCategory; } set { autoCategory = value; } }
        [SerializeField, HideInInspector]
        private bool autoPriority;
        public bool AutoPriority { get { return autoPriority; } set { autoPriority = value; } }
        [SerializeField, HideInInspector]
        private bool autoTag;
        public bool AutoTag { get { return autoTag; } set { autoTag = value; } }
        [SerializeField, HideInInspector]
        private bool inheritPriority;
        public bool InheritPriority { get { return inheritPriority; } set { inheritPriority = value; } }
        [SerializeField, HideInInspector]
        private bool inheritCategory;
        public bool InheritCategory { get { return inheritCategory; } set { inheritCategory = value; } }
        [SerializeField, HideInInspector]
        private bool inheritTag;
        public bool InheritTag { get { return inheritTag; } set { inheritTag = value; } }
        [SerializeField, HideInInspector]
        private bool inheritLinkedFile;
        public bool InheritLinkedFile { get { return inheritLinkedFile; } set { inheritLinkedFile = value; } }
        [SerializeField, HideInInspector]
        private bool confirmParentDelete;
        public bool ConfirmParentDelete { get { return confirmParentDelete; } set { confirmParentDelete = value; } }
        [SerializeField, HideInInspector]
        private bool autoSelectNewTask;
        public bool AutoSelectNewTask { get { return autoSelectNewTask; } set { autoSelectNewTask = value; } }

        //visibility config
        [SerializeField, HideInInspector]
        private bool showExtendedToolbar;
        public bool ShowExtendedToolbar { get { return showExtendedToolbar; } set { showExtendedToolbar = value; } }
        [SerializeField, HideInInspector]
        private bool scrollToActiveTask;
        public bool ScrollToActiveTask { get { return scrollToActiveTask; } set { scrollToActiveTask = value; } }
        [SerializeField, HideInInspector]
        private bool hideParents;
        public bool HideParents { get { return hideParents; } set { hideParents = value; } }
        [SerializeField, HideInInspector]
        private bool hideNotes;
        public bool HideNotes { get { return hideNotes; } set { hideNotes = value; } }
        [SerializeField, HideInInspector]
        private bool hideCompletedAt;
        public bool HideCompletedAt { get { return hideCompletedAt; } set { hideCompletedAt = value; } }
        [SerializeField, HideInInspector]
        private bool hidePriorityLevels;
        public bool HidePriorityLevels { get { return hidePriorityLevels; } set { hidePriorityLevels = value; } }
        [SerializeField, HideInInspector]
        private bool hideCategories;
        public bool HideCategories { get { return hideCategories; } set { hideCategories = value; } }
        [SerializeField, HideInInspector]
        private bool hideTags;
        public bool HideTags { get { return hideTags; } set { hideTags = value; } }
        [SerializeField, HideInInspector]
        private bool hideProgressBars;
        public bool HideProgressBars { get { return hideProgressBars; } set { hideProgressBars = value; } }

        [HideInInspector]
        public List<string> categories;
        
        [HideInInspector]
        public List<Priority> priorities;

        [HideInInspector]
        public List<string> tags;

        [HideInInspector]
        public Color[] skinColors;
        public enum SkinColors
        {
            HIGHLIGHT,
            LINE_ODD,
            LINE_EVEN,
            TEXT_UNCOMPLETED,
            TEXT_COMPLETED,
            TEXT_HIGHLIGHT,
            NOTEMARK
        }

        [SerializeField, HideInInspector]
        private string csvSeparator = ";";
        public string CsvSeparator { get { if (!string.IsNullOrEmpty(csvSeparator)) return csvSeparator; else return ";"; } set { csvSeparator = value; } }
        [SerializeField, HideInInspector]
        private bool exportCompletedAt = true;
        public bool ExportCompletedAt { get { return exportCompletedAt; } set { exportCompletedAt = value; } }
        [SerializeField, HideInInspector]
        private bool exportProgress = true;
        public bool ExportProgress { get { return exportProgress; } set { exportProgress = value; } }
        [SerializeField, HideInInspector]
        private bool exportCategory = true;
        public bool ExportCategory { get { return exportCategory; } set { exportCategory = value; } }
        [SerializeField, HideInInspector]
        private bool exportPriority = true;
        public bool ExportPriority { get { return exportPriority; } set { exportPriority = value; } }
        [SerializeField, HideInInspector]
        private bool exportTags = true;
        public bool ExportTags { get { return exportTags; } set { exportTags = value; } }
        [SerializeField, HideInInspector]
        private int encodingCode = System.Text.Encoding.Default.CodePage;
        public int EncodingCode { get { if (encodingCode != 0) return encodingCode; else return System.Text.Encoding.Default.CodePage; } set { encodingCode = value; } }

        public ListConfiguration()
        {
            categories = new List<string>();
            priorities = new List<Priority>();
            tags = new List<string>();
            skinColors = new Color[7];
        }

        public void Reset(SerializedObject serializedObject, int option = -1)
        {
            var defaultConfig = DefaultConfiguration.GetConfiguration();
            var config = serializedObject.FindProperty("listConfig");
            switch (option)
            {
                case 0:
                    config.FindPropertyRelative("autoComplete").boolValue = defaultConfig.defaultAutoComplete;
                    config.FindPropertyRelative("autoCategory").boolValue = defaultConfig.defaultAutoCategorize;
                    config.FindPropertyRelative("autoPriority").boolValue = defaultConfig.defaultAutoPrioritize;
                    config.FindPropertyRelative("autoTag").boolValue = defaultConfig.defaultAutoTag;
                    config.FindPropertyRelative("inheritCategory").boolValue = defaultConfig.defaultInheritCategory;
                    config.FindPropertyRelative("inheritPriority").boolValue = defaultConfig.defaultInheritPriority;
                    config.FindPropertyRelative("inheritTag").boolValue = defaultConfig.defaultInheritTag;
                    config.FindPropertyRelative("inheritLinkedFile").boolValue = defaultConfig.defaultInheritLinkedFile;

                    config.FindPropertyRelative("showExtendedToolbar").boolValue = defaultConfig.defaultShowExtendedToolbar;
                    config.FindPropertyRelative("autoSelectNewTask").boolValue = defaultConfig.defaultAutoSelectNewTask;
                    config.FindPropertyRelative("scrollToActiveTask").boolValue = defaultConfig.defaultScrollToActiveTask;
                    config.FindPropertyRelative("hideParents").boolValue = defaultConfig.defaultHideParents;
                    config.FindPropertyRelative("confirmParentDelete").boolValue = defaultConfig.defaultConfirmParentDelete;

                    config.FindPropertyRelative("hideNotes").boolValue = defaultConfig.defaultHideNotes;
                    config.FindPropertyRelative("hideCompletedAt").boolValue = defaultConfig.defaultCompletedAt;
                    config.FindPropertyRelative("hideCategories").boolValue = defaultConfig.defaultHideCategories;
                    config.FindPropertyRelative("hideProgressBars").boolValue = defaultConfig.defaultHideProgressBars;
                    config.FindPropertyRelative("hideTags").boolValue = defaultConfig.defaultHideTags;
                    config.FindPropertyRelative("hidePriorityLevels").boolValue = defaultConfig.defaultHidePriorityLevels;
                    break;
                case 1:
                    var tmpCategories = defaultConfig.DefaultCategories;
                    var categories = config.FindPropertyRelative("categories");
                    categories.arraySize = tmpCategories.Count;
                    for(int i = 0; i < tmpCategories.Count; i++)
                    {
                        categories.GetArrayElementAtIndex(i).stringValue = tmpCategories[i];
                    }
                    break;
                case 2:
                    var tmpPriorities = defaultConfig.DefaultPriorities;
                    var priorities = config.FindPropertyRelative("priorities");
                    priorities.arraySize = tmpPriorities.Count;
                    for (int i = 0; i < tmpPriorities.Count; i++)
                    {
                        priorities.GetArrayElementAtIndex(i).FindPropertyRelative("priorityText").stringValue = tmpPriorities[i].priorityText;
                        priorities.GetArrayElementAtIndex(i).FindPropertyRelative("priorityColor").colorValue = tmpPriorities[i].priorityColor;
                    }
                    break;
                case 3:
                    var tmpTags = defaultConfig.DefaultTags;
                    var tags = config.FindPropertyRelative("tags");
                    tags.arraySize = tmpTags.Count;
                    for (int i = 0; i < tmpTags.Count; i++)
                    {
                        tags.GetArrayElementAtIndex(i).stringValue = tmpTags[i];
                    }
                    break;
                case 4:
                    var tmpColors = EditorGUIUtility.isProSkin ? defaultConfig.DefaultProfessionalSkinColors : defaultConfig.DefaultPersonalSkinColors;
                    var colors = config.FindPropertyRelative("skinColors");
                    colors.arraySize = tmpColors.Length;
                    for(int i = 0; i < tmpColors.Length; i++)
                    {
                        colors.GetArrayElementAtIndex(i).colorValue = tmpColors[i];
                    }
                    break;
                case 5:
                    config.FindPropertyRelative("csvSeparator").stringValue = defaultConfig.defaultCsvSeparator;
                    config.FindPropertyRelative("exportCompletedAt").boolValue = defaultConfig.defaultExportCompletedAt;
                    config.FindPropertyRelative("exportProgress").boolValue = defaultConfig.defaultExportProgress;
                    config.FindPropertyRelative("exportCategory").boolValue = defaultConfig.defaultExportCategory;
                    config.FindPropertyRelative("exportPriority").boolValue = defaultConfig.defaultExportPriority;
                    config.FindPropertyRelative("exportTags").boolValue = defaultConfig.defaultExportTag;
                    config.FindPropertyRelative("encodingCode").intValue = defaultConfig.defaultEncodingCode;
                    break;
                default:
                    Reset(serializedObject, 0);
                    Reset(serializedObject, 1);
                    Reset(serializedObject, 2);
                    Reset(serializedObject, 3);
                    Reset(serializedObject, 4);
                    Reset(serializedObject, 5);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
        
        public string[] getPriorities()
        {
            string[] priorities = new string[this.priorities.Count];
            for(int i = 0; i < priorities.Length; i++)
            {
                priorities[i] = this.priorities[i].priorityText;
            }

            return priorities;
        }

        public Color getColor(int index)
        {
            return skinColors[index];
        }

        public void SwapCategories(int A, int B)
        {
            string tmp = categories[A];
            categories[A] = categories[B];
            categories[B] = tmp;
        }

        public void SwapPriorities(int A, int B)
        {
            Priority tmp = priorities[A];
            priorities[A] = priorities[B];
            priorities[B] = tmp;
        }

        public void SwapTags(int A, int B)
        {
            string tmp = tags[A];
            tags[A] = tags[B];
            tags[B] = tmp;
        }
    }

    [System.Serializable]
    public class Priority
    {
        public string priorityText;
        public Color priorityColor;

        public Priority(string s, Color c)
        {
            this.priorityText = s;
            this.priorityColor = c;
        }
    }
}
