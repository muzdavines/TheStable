using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    public sealed class StageEventsEditor : Section
    {
        public StageEventsEditor(ref TutorialMasterEditor editor)
            : base(ref editor, "Events", "Invoke custom actions for this Stage.")
        {
        }

        protected override void OnSectionGUI()
        {
            int selectedStageIndex = Mathf.Clamp(
                Editor.EditorData.SelectedStageIndex,
                0,
                Editor.SelectedTutorial.Stages.Count - 1);
            string basePropertyPath = string.Format(
                "Tutorials.Array.data[{0}].Stages.Array.data[{1}].Events",
                Editor.EditorData.SelectedTutorialIndex,
                selectedStageIndex);

            EditorField.Field(Editor.serializedObject.FindProperty(basePropertyPath + ".OnStageEnter"));
            EditorField.Field(Editor.serializedObject.FindProperty(basePropertyPath + ".OnStagePlay"));
            EditorField.Field(Editor.serializedObject.FindProperty(basePropertyPath + ".OnStageExit"));
        }
    }
}