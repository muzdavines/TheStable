namespace HardCodeLab.TutorialMaster.EditorUI
{
    public sealed class ImageListEditor : Section
    {
        public ImageListEditor(ref TutorialMasterEditor editor)
            : base(ref editor, "Images", canExpand: false)
        {
        }

        protected override void OnSectionGUI()
        {
            StageModules modules = Editor.SelectedStage.Modules;

            for (int i = 0; i < modules.Images.Count; i++)
            {
                ImageConfigEditor moduleEditor = new ImageConfigEditor(ref Editor, i);
                moduleEditor.Render();
            }
        }
    }
}