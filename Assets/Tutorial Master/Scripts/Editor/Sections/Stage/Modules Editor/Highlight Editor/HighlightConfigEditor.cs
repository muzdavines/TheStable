using System;

using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    public sealed class HighlightConfigEditor : ModuleConfigEditor
    {
        public HighlightConfigEditor(ref TutorialMasterEditor editor, int index)
            : base(ref editor, index)
        {
        }

        /// <summary>
        /// Render the additional settings body for this module
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        protected override void AdditionalSettingsBody()
        {
            base.AdditionalSettingsBody();

            if (ConfigIndex >= Editor.SelectedStage.Modules.Highlighters.Count) return;

            HighlightModuleConfig moduleConfig = Editor.SelectedStage.Modules.Highlighters[ConfigIndex];

            string baseModuleSettingsPath = BasePath + ".Settings";

            EditorField.Field(Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".SizeType"));

            switch (moduleConfig.Settings.SizeType)
            {
                case SizeMode.BasedOnUITransform:

                    EditorField.Field(
                        Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".UITransformReference"),
                        new GUIContent("UI Transform"));
                    EditorField.Field(Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".SizeOffset"));

                    break;

                case SizeMode.CustomSize:

                    EditorField.Field(Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".CustomSize"));

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
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
                    "Tutorials.Array.data[{0}].Stages.Array.data[{1}].Modules.Highlighters.Array.data[{2}]",
                    Editor.EditorData.SelectedTutorialIndex,
                    selectedStageIndex,
                    ConfigIndex);
            }
        }
    }
}