namespace HardCodeLab.TutorialMaster.EditorUI
{
    public class ScalePulsingEffectEditor : EffectEditor
    {
        public ScalePulsingEffectEditor(TutorialMasterEditor editor, string baseSerializedPath)
            : base(editor, baseSerializedPath, "Scale Pulsing", false)
        {
        }

        protected override void AdditionalSettings()
        {
            EditorField.Field(Editor.serializedObject.FindProperty(BaseSerializedPath + ".WidthRange"));
            EditorField.Field(Editor.serializedObject.FindProperty(BaseSerializedPath + ".HeightRange"));
        }
    }
}