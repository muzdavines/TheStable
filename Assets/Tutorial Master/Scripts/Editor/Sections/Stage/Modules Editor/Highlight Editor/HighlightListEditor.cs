namespace HardCodeLab.TutorialMaster.EditorUI
{
    public sealed class HighlightListEditor : Section
    {
        public HighlightListEditor(ref TutorialMasterEditor editor)
            : base(ref editor, "Highlighters", canExpand: false)
        {
        }

        protected override void OnSectionGUI()
        {
            StageModules modules = Editor.SelectedStage.Modules;

            for (int i = 0; i < modules.Highlighters.Count; i++)
            {
                HighlightConfigEditor moduleEditor = new HighlightConfigEditor(ref Editor, i);
                moduleEditor.Render();
            }
        }
    }
}