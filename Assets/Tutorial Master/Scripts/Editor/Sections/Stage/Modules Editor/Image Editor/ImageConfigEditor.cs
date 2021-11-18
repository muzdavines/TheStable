using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    public sealed class ImageConfigEditor : ModuleConfigEditor
    {
        public ImageConfigEditor(ref TutorialMasterEditor editor, int index)
            : base(ref editor, index)
        {
        }

        protected override void AdditionalSettingsBody()
        {
            base.AdditionalSettingsBody();

            string baseModuleSettingsPath = BasePath + ".Settings";

            EditorField.Field(
                Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".SpriteContent"),
                new GUIContent("Sprite"));
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
                    "Tutorials.Array.data[{0}].Stages.Array.data[{1}].Modules.Images.Array.data[{2}]",
                    Editor.EditorData.SelectedTutorialIndex,
                    selectedStageIndex,
                    ConfigIndex);
            }
        }
    }
}