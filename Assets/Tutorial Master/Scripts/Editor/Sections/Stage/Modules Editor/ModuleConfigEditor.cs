using UnityEditor;
using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    /// <summary>
    /// Used to render Editor for a module settings and module GameObject.
    /// </summary>
    public abstract class ModuleConfigEditor
    {
        /// <summary>
        /// The configuration index
        /// </summary>
        protected int ConfigIndex { get; private set; }

        /// <summary>
        /// Tutorial Master Editor which is associated with this Module Config Editor.
        /// </summary>
        protected TutorialMasterEditor Editor;

        private readonly FadeInEffectEditor _fadeInEffectEditor;
        private readonly FloatEffectEditor _floatEffectEditor;
        private readonly FlyInEffectEditor _flyInEffectEditor;
        private readonly ScalePulsingEffectEditor _scalePulsingEffectEditor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleConfigEditor"/> class.
        /// </summary>
        /// <param name="editor">Tutorial Master Editor to which this config editor is associated with.</param>
        /// <param name="index">The index.</param>
        protected ModuleConfigEditor(ref TutorialMasterEditor editor, int index)
        {
            Editor = editor;
            ConfigIndex = index;

            _fadeInEffectEditor = new FadeInEffectEditor(editor, BasePath + ".Settings.FadeInEffectSettings");
            _flyInEffectEditor = new FlyInEffectEditor(editor, BasePath + ".Settings.FlyInEffectSettings");
            _floatEffectEditor = new FloatEffectEditor(editor, BasePath + ".Settings.FloatEffectSettings");
            _scalePulsingEffectEditor = new ScalePulsingEffectEditor(editor, BasePath + ".Settings.ScalePulsingEffectSettings");
        }

        /// <summary>
        /// Base path which is used to located serialized property of this Module Config.
        /// </summary>
        /// <value>String representing the base path.</value>
        protected abstract string BasePath { get; }

        /// <summary>
        /// Renders the specified Editor.
        /// </summary>
        public void Render()
        {
            GUI.enabled = true;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            SettingsHeader();
            BaseSettingsBody();
            AdditionalSettingsBody();
            EffectsSettings();

            EditorGUILayout.EndVertical();

            GUI.enabled = true;
        }

        /// <summary>
        /// Render the additional settings body for this Module.
        /// </summary>
        protected virtual void AdditionalSettingsBody()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Module Properties", EditorStyles.boldLabel);
        }

        /// <summary>
        /// Renders the base settings for this module.
        /// </summary>
        private void BaseSettingsBody()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Base Properties", EditorStyles.boldLabel);

            string baseModuleSettingsPath = BasePath + ".Settings";

            EditorField.Field(Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".UpdateEveryFrame"));
            EditorField.Field(Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".TargetCanvas"));

            var positionModeProp = EditorField.Field(
                Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".PositionMode"));

            if (positionModeProp == null) return;

            switch (positionModeProp.enumValueIndex)
            {
                case (int)PositionMode.TransformBased:

                    var targetModeProp = EditorField.Field(
                        Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".TargetType"));

                    EditorField.Field(
                        targetModeProp.enumValueIndex == (int)TargetType.CanvasSpace
                            ? Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".UITarget")
                            : Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".TransformTarget"),
                        new GUIContent("Target"));

                    if (targetModeProp.enumValueIndex == (int)TargetType.CanvasSpace)
                    {
                        EditorField.Field(
                            Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".PlacementType"));
                    }

                    if (EditorField.Field(
                        Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".ConstrainToCanvas")).boolValue)
                    {
                        EditorField.Field(
                            Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".ConstrainPadding"),
                            new GUIContent("Padding"));
                    }

                    break;

                case (int)PositionMode.Manual:

                    EditorField.Field(
                        Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".CustomPosition"),
                        new GUIContent("Position"));

                    break;
            }

            if (positionModeProp.enumValueIndex != (int)PositionMode.Manual)
            {
                EditorField.Field(Editor.serializedObject.FindProperty(baseModuleSettingsPath + ".PositionOffset"));
            }
        }

        /// <summary>
        /// Deletes this Module Config.
        /// </summary>
        private void DeleteModule()
        {
            Editor.RegisterUndo("Deleting Module");
            Editor.DeleteProperty(Editor.serializedObject.FindProperty(BasePath));
            Editor.MarkEditorDirty();
        }

        /// <summary>
        /// Duplicates this Module Config.
        /// </summary>
        private void DuplicateModule()
        {
            Editor.RegisterUndo("Duplicating Module");
            Editor.DuplicateProperty(Editor.serializedObject.FindProperty(BasePath));
            Editor.MarkEditorDirty();
        }

        /// <summary>
        /// Renders the effect settings for this Module.
        /// </summary>
        private void EffectsSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);

            _fadeInEffectEditor.Render();
            var floatEnabled = _floatEffectEditor.Render();
            var flyEnabled = _flyInEffectEditor.Render();

            if (floatEnabled && flyEnabled)
            {
                EditorGUILayout.HelpBox(
                    "Position-based effects like Floating and Fly In won't work together. To avoid unpredictable results, please disable one of them.",
                    MessageType.Warning);
            }

            _scalePulsingEffectEditor.Render();
        }

        /// <summary>
        /// Renders the module editor header.
        /// </summary>
        /// <returns>Returns true, if the Module MonoBehaviour has been assigned.</returns>
        private void SettingsHeader()
        {
            var isEnabledProp = Editor.serializedObject.FindProperty(BasePath + ".Enabled");

            if (!isEnabledProp.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "This Module is disabled and won't be used for this Stage",
                    MessageType.Warning);
            }

            EditorGUILayout.BeginHorizontal();

            var indexLabelContent = new GUIContent(ConfigIndex + ")", "Index of this Module");
            var indexLabelStyle = EditorStyles.miniBoldLabel;
            var indexLabelWidth = indexLabelStyle.CalcSize(indexLabelContent).x;

            EditorGUILayout.LabelField(indexLabelContent, indexLabelStyle, GUILayout.Width(indexLabelWidth));

            bool overridePrefab = EditorField.Field(Editor.serializedObject.FindProperty(BasePath + ".OverridePrefab"))
                .boolValue;

            GUI.backgroundColor = Color.clear;

            var enabledIcon = EditorResources.Icons.GetToggle(isEnabledProp.boolValue, false);
            string enabledTooltip = (!isEnabledProp.boolValue ? "Enable" : "Disable") + " this Module";

            if (GUILayout.Button(
                new GUIContent(enabledIcon, enabledTooltip),
                EditorResources.Styles.ModuleButtonIconStyle,
                GUILayout.Width(16),
                GUILayout.Height(16)))
            {
                isEnabledProp.boolValue = !isEnabledProp.boolValue;
            }

            if (GUILayout.Button(
                new GUIContent(EditorResources.Icons.Duplicate, "Duplicate this Module"),
                EditorResources.Styles.ModuleButtonIconStyle,
                GUILayout.Width(16),
                GUILayout.Height(16)))
            {
                DuplicateModule();
            }

            if (GUILayout.Button(
                new GUIContent(EditorResources.Icons.TrashBin, "Delete this Module"),
                EditorResources.Styles.ModuleButtonIconStyle,
                GUILayout.Width(16),
                GUILayout.Height(16)))
            {
                DeleteModule();
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            if (!overridePrefab)
                return;

            EditorField.Field(Editor.serializedObject.FindProperty(BasePath + ".ModulePrefab"), new GUIContent("Prefab"));
        }
    }
}