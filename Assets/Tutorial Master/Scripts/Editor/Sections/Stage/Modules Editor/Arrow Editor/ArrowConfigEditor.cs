using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    public sealed class ArrowConfigEditor : ModuleConfigEditor
    {
        public ArrowConfigEditor(ref TutorialMasterEditor editor, int index)
            : base(ref editor, index)
        {
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
                    "Tutorials.Array.data[{0}].Stages.Array.data[{1}].Modules.Arrows.Array.data[{2}]",
                    Editor.EditorData.SelectedTutorialIndex,
                    selectedStageIndex,
                    ConfigIndex);
            }
        }

        protected override void AdditionalSettingsBody()
        {
            base.AdditionalSettingsBody();

            if (ConfigIndex >= Editor.SelectedStage.Modules.Arrows.Count) return;

            string baseModuleSettingsPath = BasePath + ".Settings";

            var pointDirectionProp = Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".PointDirection");

            EditorField.Field(pointDirectionProp);

            if (pointDirectionProp.enumValueIndex == (int)PointDirection.LookAtTransform)
            {
                EditorField.Field(
                    Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".PointTarget"),
                    new GUIContent("Point Target"));
            }
        }
    }
}