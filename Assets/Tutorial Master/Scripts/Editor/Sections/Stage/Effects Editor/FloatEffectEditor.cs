using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    public class FloatEffectEditor : EffectEditor
    {
        public FloatEffectEditor(TutorialMasterEditor editor, string baseSerializedPath)
            : base(editor, baseSerializedPath, "Floating", false)
        {
        }

        protected override void AdditionalSettings()
        {
            EditorField.Field(Editor.serializedObject.FindProperty(BaseSerializedPath + ".Direction"));

            if (EditorField.Field(Editor.serializedObject.FindProperty(BaseSerializedPath + ".FloatPattern"))
                    .enumValueIndex == (int)WaveType.Custom)
            {
                EditorField.Field(
                    Editor.serializedObject.FindProperty(BaseSerializedPath + ".CustomPattern"),
                    new GUIContent(""));
            }

            EditorField.Field(Editor.serializedObject.FindProperty(BaseSerializedPath + ".FloatRange"));
        }
    }
}