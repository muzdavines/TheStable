namespace HardCodeLab.TutorialMaster.EditorUI
{
    public sealed class PopupListEditor : Section
    {
        public PopupListEditor(ref TutorialMasterEditor editor)
            : base(ref editor, "Popups", canExpand: false)
        {
        }

        protected override void OnSectionGUI()
        {
            StageModules modules = Editor.SelectedStage.Modules;

            for (int i = 0; i < modules.Popups.Count; i++)
            {
                PopupConfigEditor moduleEditor = new PopupConfigEditor(ref Editor, i);
                moduleEditor.Render();
            }
        }
    }
}