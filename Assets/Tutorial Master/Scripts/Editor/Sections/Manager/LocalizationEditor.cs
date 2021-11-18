using HardCodeLab.TutorialMaster.Localization;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    
    public sealed class LocalizationEditor : Section
    {
        private readonly GUIStyle _radioButtonDisabledStyle;
        private readonly GUIStyle _radioButtonEnabledStyle;
        private string _newLanguageName = "";

        public LocalizationEditor(ref TutorialMasterEditor editor) : base(ref editor, "Localization Settings", "Manage languages that you want your tutorials to be localized in")
        {
            _radioButtonEnabledStyle = new GUIStyle(EditorStyles.radioButton)
            {
                margin = new RectOffset(0, 0, 1, 0)
            };
            _radioButtonEnabledStyle.normal = _radioButtonEnabledStyle.onNormal;

            _radioButtonDisabledStyle = new GUIStyle(EditorStyles.radioButton)
            {
                margin = new RectOffset(0, 0, 1, 0)
            };
        }

        public delegate void LanguageEdited(string languageKey);

        public event LanguageEdited LanguageAdd;

        public event LanguageEdited LanguageRemove;

        /// <summary>
        /// Renders a dropdown list of all created languages.
        /// </summary>
        /// <returns>Index of a selected Language.</returns>
        public static int LanguagesDropdownList(ref TutorialMasterEditor editor, string label = "Language")
        {
            List<Language> languages = editor.TutorialManager.LocalizationData.Languages;

            string defaultLanguageKey = editor.TutorialManager.LocalizationData.LanguageId;

            var languageNames = languages.Select(x => x.Id.Equals(defaultLanguageKey) ? x.Name + " <DEFAULT>" : x.Name).ToArray();

            editor.EditorData.SelectedLanguageIndex = EditorGUILayout.Popup(label, editor.EditorData.SelectedLanguageIndex, languageNames);
            editor.EditorData.SelectedLanguageIndex = Mathf.Clamp(editor.EditorData.SelectedLanguageIndex, 0, languages.Count - 1);

            return editor.EditorData.SelectedLanguageIndex;
        }

        protected override void OnSectionGUI()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUIUtility.labelWidth = 50;

            Editor.serializedObject.Update();

            GUI.SetNextControlName("NewLanguageNameTextField");
            _newLanguageName = EditorGUILayout.TextField(new GUIContent("Name", "Name of the new Language you want to add"), _newLanguageName);
            EditorGUIUtility.labelWidth = 0;

            if (Event.current.keyCode == KeyCode.Return)
            {
                if (GUI.GetNameOfFocusedControl().Equals("NewLanguageNameTextField"))
                {
                    AddLanguage(_newLanguageName);
                    GUI.FocusControl("NewLanguageNameTextField");
                }
            }

            if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
            {
                AddLanguage(_newLanguageName);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Languages");

            List<Language> languages = Editor.TutorialManager.LocalizationData.Languages;

            for (int i = 0; i < languages.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                RenderRadioButton(i);

                SerializedProperty languageProperty = Editor.serializedObject.FindProperty(string.Format("LocalizationData.Languages.Array.data[{0}].Name", i));

                if (languageProperty != null)
                {
                    EditorField.Field(languageProperty, new GUIContent());
                }

                GUI.enabled = (languages.Count > 1);

                if (GUILayout.Button(new GUIContent("-", "TrashBin Language"), EditorStyles.miniButton, GUILayout.Width(16), GUILayout.Height(16)))
                {
                    RemoveLanguage(i);
                }

                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();
            }

            //RenderImportExportToolbar();

            EditorGUILayout.Space();

            RenderEventSettings();

            EditorGUILayout.EndVertical();
        }

        private void RenderEventSettings()
        {
            EditorField.Field(Editor.serializedObject.FindProperty("OnLanguageChanged"));
        }

        /// <summary>
        /// Adds a new language
        /// </summary>
        /// <param name="newLanguageName"></param>
        private void AddLanguage(string newLanguageName)
        {
            List<Language> languages = Editor.TutorialManager.LocalizationData.Languages;

            if (!CanEdit)
                return;

            string tempLangName = _newLanguageName.Trim();
            if (string.IsNullOrEmpty(tempLangName))
                return;

            bool exists = languages.Any(lang => lang.Name.Equals(tempLangName));

            if (exists)
                return;

            Editor.RegisterUndo("Adding Language - " + _newLanguageName);
            Language newLanguage = new Language(newLanguageName);
            languages.Add(newLanguage);
            _newLanguageName = "";
            GUI.FocusControl(null);

            OnLanguageAdded(newLanguage.Id);

            Editor.MarkEditorDirty();
        }

        /// <summary>
        /// Deletes unused localized content
        /// </summary>
        /// <param name="languageKey">Id of a language which is going to be removed.</param>
        private void DisposeUnusedContent(string languageKey)
        {
            foreach (Tutorial tutorial in Editor.TutorialManager.Tutorials)
            {
                foreach (Stage stage in tutorial.Stages)
                {
                    foreach (PopupModuleConfig popup in stage.Modules.Popups)
                    {
                        popup.Settings.PopupMessage.RemoveContent(languageKey);
                        popup.Settings.ButtonLabel.RemoveContent(languageKey);
                    }
                }
            }
        }

        private void OnLanguageAdded(string languageKey)
        {
            LanguageEdited handler = LanguageAdd;
            if (handler != null) handler(languageKey);
        }

        private void OnLanguageRemoved(string languageKey)
        {
            LanguageEdited handler = LanguageRemove;
            if (handler != null) handler(languageKey);
        }

        /// <summary>
        /// Removes a language
        /// </summary>
        /// <param name="languageId"></param>
        private void RemoveLanguage(int languageId)
        {
            LocalizationData localizationData = Editor.TutorialManager.LocalizationData;
            Editor.RegisterUndo("Removing Language - " + _newLanguageName);
            string languageKey = Editor.TutorialManager.LocalizationData.Languages[languageId].Id;
            Editor.TutorialManager.LocalizationData.Languages.RemoveAt(languageId);

            localizationData.CurrentLanguageIndex = Mathf.Clamp(localizationData.CurrentLanguageIndex, 0, localizationData.Languages.Count - 1);
            Editor.EditorData.SelectedLanguageIndex = Mathf.Clamp(Editor.EditorData.SelectedLanguageIndex, 0, localizationData.Languages.Count - 1);

            DisposeUnusedContent(languageKey);
            OnLanguageRemoved(languageKey);

            Editor.MarkEditorDirty();
        }

        private void RenderImportExportToolbar()
        {
            var manager = Editor.TutorialManager;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("Export Content...", "Exports all Localized Content into a .csv file. Modules' contents that support localized content will be exported."),
                EditorStyles.miniButtonLeft))
            {
                //string fileName = string.Format("Localized Tutorial Content ({0}) - {1:yyyy-MM-dd_hh-mm-ss-tt}", PlayerSettings.productName, DateTime.Now);
                string exportDirectory = "test dir";
                //exportDirectory = EditorUtility.SaveFilePanel("Select Directory", EditorPrefs.GetString("tm_localization_lastExportLocation"), fileName, "csv");

                if (exportDirectory.Length != 0)
                {
                    if (LocalizedContentExporter.Export(ref manager.Tutorials, exportDirectory))
                    {
                        /*
                        EditorUtility.DisplayDialog("Export Finished",
                            string.Format("Localized Content has been saved to\n\n{0}", exportDirectory), "OK");
                        */
                    }

                    EditorPrefs.SetString("tm_localization_lastExportLocation", exportDirectory);
                }
            }

            if (GUILayout.Button(new GUIContent("Import Content...", "Import all Localized Content from a .csv file. Modules that support localized content will be affected."),
                EditorStyles.miniButtonRight))
            {
                string importDirectory = EditorUtility.OpenFilePanel("Select File", EditorPrefs.GetString("tm_localization_lastImportLocation"), "csv");

                if (importDirectory.Length != 0)
                {
                    LocalizedContentImporter.Import(ref manager, importDirectory);
                    Editor.MarkEditorDirty();

                    EditorPrefs.SetString("tm_localization_lastImportLocation", importDirectory);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Renders a radio button for a specified Language.
        /// </summary>
        /// <param name="index">Language index.</param>
        private void RenderRadioButton(int index)
        {
            LocalizationData localizationData = Editor.TutorialManager.LocalizationData;

            GUIStyle radioButtonStyle = localizationData.CurrentLanguageIndex == index
                ? _radioButtonEnabledStyle
                : _radioButtonDisabledStyle;

            if (GUILayout.Button(new GUIContent("", "Set a default language."), radioButtonStyle, GUILayout.ExpandWidth(false)))
            {
                if (EditorApplication.isPlaying)
                {
                    Editor.TutorialManager.SetLanguage(index);
                }
                else
                {
                    localizationData.CurrentLanguageIndex = index;
                }
            }
        }
    }
}