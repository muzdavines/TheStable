using UnityEditor;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    /// <summary>
    /// Allows user to edit settings of effects.
    /// </summary>
    public abstract class EffectEditor
    {
        /// <summary>
        /// The serialized path which is used to locate effect settings
        /// </summary>
        protected string BaseSerializedPath;

        /// <summary>
        /// How this effect will show up in the editor
        /// </summary>
        protected string DisplayName;

        /// <summary>
        /// Tutorial Master Editor which is associated with this Effect Editor.
        /// </summary>
        protected TutorialMasterEditor Editor;

        /// <summary>
        /// If true, the CanInteract setting will show up in the editor.
        /// </summary>
        private readonly bool _showInteractOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="EffectEditor"/> class.
        /// </summary>
        /// <param name="editor">The editor.</param>
        /// <param name="baseSerializedPath">The serialized path.</param>
        /// <param name="displayName">Display name for an this Editor</param>
        /// <param name="showInteractOption">Whether or not should CanInteract option be displayed</param>
        protected EffectEditor(
            TutorialMasterEditor editor,
            string baseSerializedPath,
            string displayName,
            bool showInteractOption = true)
        {
            Editor = editor;
            BaseSerializedPath = baseSerializedPath;
            DisplayName = displayName;
            _showInteractOption = showInteractOption;
        }

        /// <summary>
        /// Renders the specified Editor.
        /// </summary>
        public bool Render()
        {
            bool effectEnabled = SettingsHeader();
            if (effectEnabled)
            {
                EditorGUI.indentLevel++;

                BaseSettings();
                AdditionalSettings();

                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
            }

            return effectEnabled;
        }

        /// <summary>
        /// Render the additional settings body for this module
        /// </summary>
        protected abstract void AdditionalSettings();

        /// <summary>
        /// Renders the base settings for this effect.
        /// </summary>
        private void BaseSettings()
        {
            EditorField.Field(Editor.serializedObject.FindProperty(BaseSerializedPath + ".Speed"));

            if (_showInteractOption)
            {
                EditorField.Field(Editor.serializedObject.FindProperty(BaseSerializedPath + ".CanInteract"));
            }
        }

        private bool SettingsHeader()
        {
            var prop = Editor.serializedObject.FindProperty(BaseSerializedPath + ".IsEnabled");
            if (prop == null) return false;

            prop.boolValue = EditorGUILayout.ToggleLeft(DisplayName, prop.boolValue);

            return prop.boolValue;
        }
    }
}