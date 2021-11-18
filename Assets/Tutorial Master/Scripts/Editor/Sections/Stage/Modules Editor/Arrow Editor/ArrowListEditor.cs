namespace HardCodeLab.TutorialMaster.EditorUI
{
    public sealed class ArrowListEditor : Section
    {
        public ArrowListEditor(ref TutorialMasterEditor editor)
            : base(ref editor, "Arrows", canExpand: false)
        {
        }

        protected override void OnSectionGUI()
        {
            StageModules modules = Editor.SelectedStage.Modules;

            for (int i = 0; i < modules.Arrows.Count; i++)
            {
                ArrowConfigEditor moduleEditor = new ArrowConfigEditor(ref Editor, i);
                moduleEditor.Render();
            }
        }
    }
}