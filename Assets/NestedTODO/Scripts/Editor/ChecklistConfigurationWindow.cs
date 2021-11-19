using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace NestedTODO
{
    public class ChecklistConfigurationWindow : EditorWindow
    {
        int openMenu = 0;
        string[] toolbarStrings = { "General", "Category", "Pariority", "Tags", "Colors", "Export" };
        Vector2 scrollPosition;
        const int buttonWidth = 20;
        const int textWidth = 50;
        bool preview;
        bool doRepaint = false;

        public static void OpenWindow()
        {
            ChecklistConfigurationWindow window = (ChecklistConfigurationWindow)EditorWindow.GetWindow<ChecklistConfigurationWindow>();
#if UNITY_5_3_OR_NEWER
            window.titleContent.text = "Configuration";
#else
            window.title = "Configuration";
#endif
            window.minSize = new Vector2(300f, 430f);
        }

        void OnEnabled()
        {
            Undo.undoRedoPerformed += this.PerformUndo;
        }

        void OnDestroy()
        {
            Undo.undoRedoPerformed -= this.PerformUndo;
        }

        void PerformUndo()
        {
            this.Repaint();
            ChecklistWindow.Refresh();
        }

        void OnGUI()
        {
            if (ChecklistWindow.nestedList == null)
            {
                EditorGUILayout.HelpBox("There's no selected list.", MessageType.Warning);
                return;
            }

            openMenu = GUILayout.Toolbar(openMenu, toolbarStrings, EditorStyles.toolbarButton);

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            switch (openMenu)
            {
                case 0: ShowGeneralMenu(); break;
                case 4: ShowColorSchemesMenu(); break;
                case 5: ShowExportMenu(); break;
                default: ShowPropertyMenu(); break;
            }
            EditorGUILayout.EndVertical();

            DrawBottomToolbar();

            if (EditorGUI.EndChangeCheck())
            {
                ChecklistWindow.nestedList.SaveSerializedProperties();
                ChecklistWindow.Refresh();
            }
        }

        void DrawBottomToolbar()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            bool choice = false;
            if (GUILayout.Button("Set as Default", EditorStyles.toolbarButton))
            {
                GUI.FocusControl(null);
                int save = EditorUtility.DisplayDialogComplex("Set as Default", "Set the current configuration as the default configuration for new NestedLists?", "Set Current", "Cancel", "Set All");
                if (save == 0)
                {
                    DefaultConfiguration.SaveConfiguration(ChecklistWindow.nestedList.Configuration, openMenu);
                }
                else if(save == 2)
                {
                    DefaultConfiguration.SaveConfiguration(ChecklistWindow.nestedList.Configuration);
                }

            }
            if (GUILayout.Button("Reset Current", EditorStyles.toolbarButton))
            {
                GUI.FocusControl(null);
                choice = EditorUtility.DisplayDialog("Reset", "Reset current menu to Default Settings?", "Ok", "Cancel");
                if (choice)
                {
                    if (openMenu == 1)
                        ChecklistWindow.nestedList.ResetProperty("category");
                    if (openMenu == 2)
                        ChecklistWindow.nestedList.ResetProperty("priority");
                    if (openMenu == 3)
                        ChecklistWindow.nestedList.ResetProperty("tag");

                    ChecklistWindow.nestedList.Configuration.Reset(ChecklistWindow.nestedList.SerializedListObject, openMenu);
                    Repaint();
                }
            }
            if (GUILayout.Button("Reset All", EditorStyles.toolbarButton))
            {
                GUI.FocusControl(null);
                choice = EditorUtility.DisplayDialog("Reset", "Reset all menus to Default Settings?", "Ok", "Cancel");
                if (choice)
                {
                    ChecklistWindow.nestedList.ResetProperty("category");
                    ChecklistWindow.nestedList.ResetProperty("priority");
                    ChecklistWindow.nestedList.ResetProperty("tag");

                    ChecklistWindow.nestedList.Configuration.Reset(ChecklistWindow.nestedList.SerializedListObject);
                    Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void ShowGeneralMenu()
        {
            string tooltip;

            SerializedProperty config = ChecklistWindow.nestedList.SerializedListObject.FindProperty("listConfig");

            EditorGUILayout.LabelField("General Options", EditorStyles.boldLabel);
            
            tooltip = "Tasks will be automatically completed if their parent Tasks are marked as completed or if all Sub Tasks are marked as completed (the same applies when a Task is marked as uncompleted).";
            config.FindPropertyRelative("autoComplete").boolValue = EditorGUILayout.Toggle(new GUIContent("Auto Complete", tooltip), ChecklistWindow.nestedList.Configuration.AutoComplete);

            tooltip = "When a Task category changes, all Sub Tasks will be set to the same category.";
            config.FindPropertyRelative("autoCategory").boolValue = EditorGUILayout.Toggle(new GUIContent("Auto Categorize", tooltip), ChecklistWindow.nestedList.Configuration.AutoCategory);

            tooltip = "When a Task priority changes, all Sub Tasks will be set to the same priority.";
            config.FindPropertyRelative("autoPriority").boolValue = EditorGUILayout.Toggle(new GUIContent("Auto Prioritize", tooltip), ChecklistWindow.nestedList.Configuration.AutoPriority);

            tooltip = "When a Task tag changes, all Sub Tasks will be set to the same tag.";
            config.FindPropertyRelative("autoTag").boolValue = EditorGUILayout.Toggle(new GUIContent("Auto Tag", tooltip), ChecklistWindow.nestedList.Configuration.AutoTag);

            tooltip = "When a Sub Task is created, it will inherit its parent Category.";
            config.FindPropertyRelative("inheritCategory").boolValue = EditorGUILayout.Toggle(new GUIContent("Inherit Category", tooltip), ChecklistWindow.nestedList.Configuration.InheritCategory);

            tooltip = "When a Sub Task is created, it will inherit its parent Priority.";
            config.FindPropertyRelative("inheritPriority").boolValue = EditorGUILayout.Toggle(new GUIContent("Inherit Priority", tooltip), ChecklistWindow.nestedList.Configuration.InheritPriority);

            tooltip = "When a Sub Task is created, it will inherit its parent Tag.";
            config.FindPropertyRelative("inheritTag").boolValue = EditorGUILayout.Toggle(new GUIContent("Inherit Tag", tooltip), ChecklistWindow.nestedList.Configuration.InheritTag);

            tooltip = "When a Sub Task is created, it will inherit its parent LinkedFile.";
            config.FindPropertyRelative("inheritLinkedFile").boolValue = EditorGUILayout.Toggle(new GUIContent("Inherit LinkedFile", tooltip), ChecklistWindow.nestedList.Configuration.InheritLinkedFile);

            tooltip = "Open a confirmation window when deleting a Task that have one or more Sub Tasks branching out of it.";
            config.FindPropertyRelative("confirmParentDelete").boolValue = EditorGUILayout.Toggle(new GUIContent("Confirm Delete", tooltip), ChecklistWindow.nestedList.Configuration.ConfirmParentDelete);

            tooltip = "Newly created Tasks will be automatically set as the active task.";
            config.FindPropertyRelative("autoSelectNewTask").boolValue = EditorGUILayout.Toggle(new GUIContent("Auto Select New Task", tooltip), ChecklistWindow.nestedList.Configuration.AutoSelectNewTask);

            tooltip = "When a task is selected, the list will automatically scroll to make sure the selected task is visible";
            config.FindPropertyRelative("scrollToActiveTask").boolValue = EditorGUILayout.Toggle(new GUIContent("Scroll to Selected Task", tooltip), ChecklistWindow.nestedList.Configuration.ScrollToActiveTask);

            DrawGUIExtensions.DrawFixedVerticalSpace(10);

            EditorGUILayout.LabelField("Visibility Options", EditorStyles.boldLabel);

            tooltip = "Replace the main \"+\" Add Task button with options for adding Top Level Tasks and, if a Task is selected, buttons for adding Uncle Tasks, Sibling Tasks and Child Tasks.";
            config.FindPropertyRelative("showExtendedToolbar").boolValue = EditorGUILayout.Toggle(new GUIContent("Show Extended Toolbar", tooltip), ChecklistWindow.nestedList.Configuration.ShowExtendedToolbar);
            tooltip = "Parent Tasks will be hidden if they don't meet the selected view filter criteria.";
            config.FindPropertyRelative("hideParents").boolValue = EditorGUILayout.Toggle(new GUIContent("Hide Parents", tooltip), ChecklistWindow.nestedList.Configuration.HideParents);
            config.FindPropertyRelative("hideNotes").boolValue = EditorGUILayout.Toggle("Hide Notes Marks", ChecklistWindow.nestedList.Configuration.HideNotes);
            config.FindPropertyRelative("hideCompletedAt").boolValue = EditorGUILayout.Toggle("Hide Completed At Label", ChecklistWindow.nestedList.Configuration.HideCompletedAt);
            config.FindPropertyRelative("hidePriorityLevels").boolValue = EditorGUILayout.Toggle("Hide Priority Marks", ChecklistWindow.nestedList.Configuration.HidePriorityLevels);
            config.FindPropertyRelative("hideCategories").boolValue = EditorGUILayout.Toggle("Hide Category Popup", ChecklistWindow.nestedList.Configuration.HideCategories);
            config.FindPropertyRelative("hideTags").boolValue = EditorGUILayout.Toggle("Hide Tags Popup", ChecklistWindow.nestedList.Configuration.HideTags);
            config.FindPropertyRelative("hideProgressBars").boolValue = EditorGUILayout.Toggle("Hide Progress Bars", ChecklistWindow.nestedList.Configuration.HideProgressBars);
        }
        
        void ShowPropertyMenu()
        {
            if (GUILayout.Button("+", EditorStyles.toolbarButton))
            {
                switch (openMenu)
                {
                    case 1: ChecklistWindow.nestedList.AddProperty("categories", "New Category"); break;
                    case 2: ChecklistWindow.nestedList.AddProperty("priorities", "New Priority", Color.black); break;
                    case 3: ChecklistWindow.nestedList.AddProperty("tags", "New Tag"); break;
                }
            }

            if (openMenu == 2 && EditorGUIUtility.isProSkin)
                EditorGUILayout.HelpBox("Colors appear darker in Pro Skin", MessageType.Info);

            int count = 0;
            switch (openMenu)
            {
                case 1: count = ChecklistWindow.nestedList.Configuration.categories.Count; break;
                case 2: count = ChecklistWindow.nestedList.Configuration.priorities.Count; break;
                case 3: count = ChecklistWindow.nestedList.Configuration.tags.Count; break;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < count; i++)
            {
                //when a category, priorirty or tag is deleted, to prevent an out of bound error log
                //stop the property's list draw process, and do a repaint to update the count value
                if (doRepaint)
                    break;

                DrawProperty(i);
            }
            EditorGUILayout.EndScrollView();

            if (doRepaint)
            {
                doRepaint = false;
                Repaint();
            }
        }

        void DrawProperty(int index)
        {
            int count = 0;
            switch (openMenu)
            {
                case 1: count = ChecklistWindow.nestedList.Configuration.categories.Count; break;
                case 2: count = ChecklistWindow.nestedList.Configuration.priorities.Count; break;
                case 3: count = ChecklistWindow.nestedList.Configuration.tags.Count; break;
            }

            EditorGUILayout.BeginHorizontal();

            if (index == 0)
                GUI.enabled = false;
            if (GUILayout.Button(@"▲", EditorStyles.miniButtonLeft, GUILayout.Width(buttonWidth)))
            {
                switch (openMenu)
                {
                    case 1: ChecklistWindow.nestedList.SwapProperties("category", index, index - 1); break;
                    case 2: ChecklistWindow.nestedList.SwapProperties("priority", index, index - 1); break;
                    case 3: ChecklistWindow.nestedList.SwapProperties("tag", index, index - 1); break;
                }

                return;
            }
            GUI.enabled = true;

            if (index == count - 1)
                GUI.enabled = false;
            if (GUILayout.Button(@"▼", EditorStyles.miniButtonRight, GUILayout.Width(buttonWidth)))
            {
                switch (openMenu)
                {
                    case 1: ChecklistWindow.nestedList.SwapProperties("category", index, index + 1); break;
                    case 2: ChecklistWindow.nestedList.SwapProperties("priority", index, index + 1); break;
                    case 3: ChecklistWindow.nestedList.SwapProperties("tag", index, index + 1); break;
                }
                return;
            }
            GUI.enabled = true;

            string propertyPath = "";
            switch (openMenu)
            {
                case 1: propertyPath = "listConfig.categories"; break;
                case 2: propertyPath = "listConfig.priorities"; break;
                case 3: propertyPath = "listConfig.tags"; break;
            }

            SerializedProperty property = ChecklistWindow.nestedList.SerializedListObject.FindProperty(propertyPath).GetArrayElementAtIndex(index);

            switch (openMenu)
            {
                case 1:
                    property.stringValue = "Categories/" + EditorGUILayout.TextField(ChecklistWindow.nestedList.Configuration.categories[index].Replace("Categories/", ""), GUILayout.MinWidth(textWidth));
                    break;
                case 2:
                    property.FindPropertyRelative("priorityText").stringValue = "Priorities/" + EditorGUILayout.TextField(ChecklistWindow.nestedList.Configuration.priorities[index].priorityText.Replace("Priorities/", ""), GUILayout.MinWidth(textWidth));
                    property.FindPropertyRelative("priorityColor").colorValue = EditorGUILayout.ColorField(ChecklistWindow.nestedList.Configuration.priorities[index].priorityColor, GUILayout.MaxWidth(textWidth));
                    break;
                case 3:
                    property.stringValue = "Tags/" + EditorGUILayout.TextField(ChecklistWindow.nestedList.Configuration.tags[index].Replace("Tags/", ""), GUILayout.MinWidth(textWidth));
                    break;
            }

            if (index == 0)
                GUI.enabled = false;
            if (GUILayout.Button("x", GUILayout.Width(buttonWidth)))
            {
                doRepaint = true;
                switch (openMenu)
                {
                    case 1:
                        ChecklistWindow.nestedList.FixProperty("category", index);
                        ChecklistWindow.nestedList.DeleteProperty("categories", index);
                        break;
                    case 2:
                        ChecklistWindow.nestedList.FixProperty("priority", index);
                        ChecklistWindow.nestedList.DeleteProperty("priorities", index);
                        break;
                    case 3:
                        ChecklistWindow.nestedList.FixProperty("tag", index);
                        ChecklistWindow.nestedList.DeleteProperty("tags", index);
                        break;
                }
                return;
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        void ShowColorSchemesMenu()
        {
            Color normalColor = GUI.color;
            if (preview)
                GUI.backgroundColor = EditorGUIUtility.isProSkin ? Color.black : Color.gray;
            if (GUILayout.Button("Preview Colors", EditorStyles.toolbarButton)) { preview = !preview; GUI.FocusControl(null); }
            GUI.backgroundColor = normalColor;

            EditorGUILayout.Separator();

            var subArea = new GUIStyle(EditorStyles.boldLabel);
            subArea.fontSize = 12;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            SerializedProperty colors = ChecklistWindow.nestedList.SerializedListObject.FindProperty("listConfig.skinColors");
            colors.GetArrayElementAtIndex(6).colorValue = EditorGUILayout.ColorField(new GUIContent("Notes Mark", "Color of the note mark on the left of an Item title"), ChecklistWindow.nestedList.Configuration.skinColors[6]);
            if (preview)
            {
                var re = GUILayoutUtility.GetLastRect();
                re.Set(re.position.x + 75, re.position.y + 2, 12, 12);
                GUI.color = EditorGUIUtility.isProSkin ? Color.black : Color.white;
                EditorGUI.LabelField(re, "", GUI.skin.box);
                GUI.color = normalColor;
                int offset = 1;
                re.Set(re.position.x + offset, re.position.y + offset, re.size.x - offset * 2, re.size.y - offset * 2);
                EditorGUI.DrawRect(re, ChecklistWindow.nestedList.Configuration.skinColors[6]);
            }

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Line Colors", subArea);
            DrawColorField(ref colors, 0, "Highlight Color", "Highlight color for a selected Item", preview, 0, false);
            DrawColorField(ref colors, 1, "Odd Line Color", "Color for odd line Items", preview, 1, false);
            DrawColorField(ref colors, 2, "Even Line Color", "Color for even line Items", preview, 2, false);

            EditorGUILayout.LabelField("Text Colors", subArea);
            DrawColorField(ref colors, 3, "Uncompleted Color", "Text color for a non completed Item", preview, 1, true);
            DrawColorField(ref colors, 4, "Completed Color", "Text color for a completed Item", preview, 1, true);
            DrawColorField(ref colors, 5, "Highlight Color", "Text color for a selected Item", preview, 0, true);

            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();
        }

        void DrawColorField(ref SerializedProperty colors, int index, string text, string tooltip, bool previewLine, int lineColor, bool previewColor)
        {
            if (!previewLine)
            {
                colors.GetArrayElementAtIndex(index).colorValue = EditorGUILayout.ColorField(new GUIContent(text, tooltip), ChecklistWindow.nestedList.Configuration.skinColors[index]);
            }
            else
            {
                Color normalColor = GUI.backgroundColor;
                GUI.backgroundColor = EditorGUIUtility.isProSkin ? Color.black : Color.white;
                var ra = EditorGUILayout.BeginVertical(GUI.skin.box);
                GUI.backgroundColor = normalColor;
                int offset = 1;
                ra.Set(ra.position.x + offset, ra.position.y + offset, ra.size.x - offset * 2, ra.size.y - offset * 2);
                EditorGUI.DrawRect(ra, ChecklistWindow.nestedList.Configuration.skinColors[lineColor]);

                var s = EditorStyles.label;
                bool rich = s.richText;
                s.richText = true;

                if (previewColor)
                {
#if UNITY_5_3_OR_NEWER
                    colors.GetArrayElementAtIndex(index).colorValue = EditorGUILayout.ColorField(new GUIContent(string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(ChecklistWindow.nestedList.Configuration.skinColors[index]), text), tooltip), ChecklistWindow.nestedList.Configuration.skinColors[index]);
#else
                    Color tmpColor = ChecklistWindow.nestedList.Configuration.skinColors[index];
                    string hexColor = (Mathf.RoundToInt(tmpColor.r * 255)).ToString("x2") + (Mathf.RoundToInt(tmpColor.g * 255)).ToString("x2") + (Mathf.RoundToInt(tmpColor.b * 255)).ToString("x2") + (Mathf.RoundToInt(tmpColor.a * 255)).ToString("x2");
                    colors.GetArrayElementAtIndex(index).colorValue = EditorGUILayout.ColorField(new GUIContent(string.Format("<color=#{0}>{1}</color>", hexColor.ToUpper(), text), tooltip), ChecklistWindow.nestedList.Configuration.skinColors[index]);
#endif
                }
                else
                {
                    colors.GetArrayElementAtIndex(index).colorValue = EditorGUILayout.ColorField(new GUIContent(text, tooltip), ChecklistWindow.nestedList.Configuration.skinColors[index]);
                }

                s.richText = rich;
                EditorGUILayout.EndVertical();
            }
        }

        void ShowExportMenu()
        {
            string tooltip;

            SerializedProperty config = ChecklistWindow.nestedList.SerializedListObject.FindProperty("listConfig");

            tooltip = "The character that will be used to separate the columns on the CSV file, if no character is provided, the default ';' will be used.";
            string tmpString = EditorGUILayout.TextField(new GUIContent("CSV Separator", tooltip), ChecklistWindow.nestedList.Configuration.CsvSeparator, GUILayout.MinWidth(5)).Trim();
            if (tmpString.Length > 0)
                tmpString = tmpString.Substring(0, 1);
            else
            {
                tmpString = ";";
                Repaint();
                return;
            }
            config.FindPropertyRelative("csvSeparator").stringValue = tmpString;

            tooltip = "";
            config.FindPropertyRelative("exportCompletedAt").boolValue = EditorGUILayout.Toggle(new GUIContent("Export CompletedAt", tooltip), ChecklistWindow.nestedList.Configuration.ExportCompletedAt);

            tooltip = "";
            config.FindPropertyRelative("exportProgress").boolValue = EditorGUILayout.Toggle(new GUIContent("Export Progress", tooltip), ChecklistWindow.nestedList.Configuration.ExportProgress);

            tooltip = "";
            config.FindPropertyRelative("exportCategory").boolValue = EditorGUILayout.Toggle(new GUIContent("Export Categories", tooltip), ChecklistWindow.nestedList.Configuration.ExportCategory);

            tooltip = "";
            config.FindPropertyRelative("exportPriority").boolValue = EditorGUILayout.Toggle(new GUIContent("Export Priorities", tooltip), ChecklistWindow.nestedList.Configuration.ExportPriority);

            tooltip = "";
            config.FindPropertyRelative("exportTags").boolValue = EditorGUILayout.Toggle(new GUIContent("Export Tags", tooltip), ChecklistWindow.nestedList.Configuration.ExportTags);

            EditorGUILayout.HelpBox("The encoding property set which encoding method will be use to write the CSV file, this affects the characters available (if your lenguage uses accent marks and they are not written properly, try using UTF8.", MessageType.Info);

            string[] encodings = new string[] {"Default", "ASCII", "UTF8", "Unicode"};
            int[] codepages = new int[] { System.Text.Encoding.Default.CodePage, System.Text.Encoding.ASCII.CodePage, System.Text.Encoding.UTF8.CodePage, System.Text.Encoding.Unicode.CodePage };
            config.FindPropertyRelative("encodingCode").intValue = EditorGUILayout.IntPopup("Text Encoding", ChecklistWindow.nestedList.Configuration.EncodingCode, encodings, codepages);
        }

    }
}
