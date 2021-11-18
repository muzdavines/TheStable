namespace HardCodeLab.TutorialMaster.EditorUI
{
    public sealed class TutorialEventsEditor : Section
    {
        public TutorialEventsEditor(ref TutorialMasterEditor editor)
            : base(ref editor, "Events", "Invoke custom actions for this Tutorial")
        {
        }

        protected override void OnSectionGUI()
        {
            string basePropertyPath = string.Format(
                "Tutorials.Array.data[{0}].Events",
                Editor.EditorData.SelectedTutorialIndex);

            var prop = Editor.serializedObject.FindProperty(basePropertyPath + ".OnTutorialStart");
            if (prop == null) return;

            EditorField.Field(Editor.serializedObject.FindProperty(basePropertyPath + ".OnTutorialEnter"));
            EditorField.Field(Editor.serializedObject.FindProperty(basePropertyPath + ".OnTutorialStart"));
            EditorField.Field(Editor.serializedObject.FindProperty(basePropertyPath + ".OnTutorialEnd"));
        }
    }
}