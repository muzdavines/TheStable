using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    public sealed class StageAudioEditor : Section
    {
        public StageAudioEditor(ref TutorialMasterEditor editor)
            : base(ref editor, "Audio", "Choose what sound effect to play and when during the Stage.")
        {
        }

        protected override void OnSectionGUI()
        {
            int selectedStageIndex = Mathf.Clamp(
                Editor.EditorData.SelectedStageIndex,
                0,
                Editor.SelectedTutorial.Stages.Count - 1);
            string basePropertyPath = string.Format(
                "Tutorials.Array.data[{0}].Stages.Array.data[{1}].Audio",
                Editor.EditorData.SelectedTutorialIndex,
                selectedStageIndex);

            StageAudio stageAudio = Editor.SelectedStage.Audio;

            EditorField.Field(
                Editor.serializedObject.FindProperty(basePropertyPath + ".Timing"),
                new GUIContent("Play Audio"));

            if (stageAudio.Timing != AudioTiming.Never)
            {
                EditorField.Field(
                    Editor.serializedObject.FindProperty(basePropertyPath + ".Source"),
                    new GUIContent("Audio Source"));

                EditorField.Field(
                    Editor.serializedObject.FindProperty(basePropertyPath + ".Clip"),
                    new GUIContent("Clip"));
            }
        }
    }
}