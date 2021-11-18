using UnityEditor;
using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    public sealed class PopupConfigEditor : ModuleConfigEditor
    {
        public PopupConfigEditor(ref TutorialMasterEditor editor, int index)
            : base(ref editor, index)
        {
        }

        protected override void AdditionalSettingsBody()
        {
            base.AdditionalSettingsBody();
            GUI.changed = false;

            if (ConfigIndex >= Editor.SelectedStage.Modules.Popups.Count) return;
            PopupModuleConfig moduleConfig = Editor.SelectedStage.Modules.Popups[ConfigIndex];

            EditorStyles.textField.wordWrap = true;

            string languageKey = Editor.TutorialManager.LocalizationData
                .Languages[Editor.EditorData.SelectedLanguageIndex].Id;
            string baseModuleSettingsPath = BasePath + ".Settings";

            EditorField.Field(Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".PopupImage"));

            EditorGUILayout.Space();

            Editor.EditorData.SelectedLanguageIndex = LocalizationEditor.LanguagesDropdownList(ref Editor);

            moduleConfig.Settings.PopupTitle.SetContent(
                languageKey,
                EditorGUILayout.TextField(
                    new GUIContent("Title"),
                    moduleConfig.Settings.PopupTitle.GetContent(languageKey)));


            EditorGUILayout.PrefixLabel("Message");
            moduleConfig.Settings.PopupMessage.SetContent(
                languageKey,
                EditorGUILayout.TextArea(moduleConfig.Settings.PopupMessage.GetContent(languageKey)));


            EditorField.Field(Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".ShowButton"));
            if (moduleConfig.Settings.ShowButton)
            {
                moduleConfig.Settings.ButtonLabel.SetContent(
                    languageKey,
                    EditorGUILayout.TextField(
                        "Button Label",
                        moduleConfig.Settings.ButtonLabel.GetContent(languageKey)));
                EditorField.Field(
                    Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".ButtonClickEvent"),
                    new GUIContent("Button Click Event"));
            }

            if (GUI.changed)
                Editor.MarkEditorDirty();
        }

        protected override string BasePath
        {
            get
            {
                int selectedStageIndex = Mathf.Clamp(
                    Editor.EditorData.SelectedStageIndex,
                    0,
                    Editor.SelectedTutorial.Stages.Count - 1);

                return string.Format(
                    "Tutorials.Array.data[{0}].Stages.Array.data[{1}].Modules.Popups.Array.data[{2}]",
                    Editor.EditorData.SelectedTutorialIndex,
                    selectedStageIndex,
                    ConfigIndex);
            }
        }
    }
}