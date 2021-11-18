namespace HardCodeLab.TutorialMaster.EditorUI
{
    public class FlyInEffectEditor : EffectEditor
    {
        public FlyInEffectEditor(TutorialMasterEditor editor, string baseSerializedPath)
            : base(editor, baseSerializedPath, "Fly In")
        {
        }

        protected override void AdditionalSettings()
        {
            EditorField.Field(Editor.serializedObject.FindProperty(BaseSerializedPath + ".FlyDirection"));
            EditorField.Field(Editor.serializedObject.FindProperty(BaseSerializedPath + ".FlyDistance"));
        }
    }
}