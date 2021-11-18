using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    /// <inheritdoc />
    /// <summary>
    /// Responsible for drawing custom GUI for components of type <seealso cref="T:HardCodeLab.TutorialMaster.TutorialMasterManager" />.
    /// </summary>
    /// <seealso cref="T:UnityEditor.Editor" />
    [CustomEditor(typeof(TutorialMasterManager))]
    public sealed class TutorialMasterEditor : Editor
    {
        private static HCLBox _box;

        private static HCLBox Box
        {
            get
            {
                if (_box == null)
                    _box = new HCLBox(TMEditorUtils.DirectoryPath + "/Skins/Items/HCLBox/");

                return _box;
            }
        }

        private DebugControllerEditor _debugControllerEditor;
        private TutorialMasterEditor _editor;
        private LocalizationEditor _localizationSettings;
        private PoolingEditor _poolingEditor;
        private StageListEditor _stageList;
        private StageEditor _stageSettings;
        private StartupSettingsEditor _startupSettings;
        private TutorialListEditor _tutorialList;
        private TutorialEditor _tutorialSettings;

        /// <summary>
        /// Gets the Tutorial Manager Component this Editor is associated with.
        /// </summary>
        public TutorialMasterManager TutorialManager
        {
            get
            {
                return (TutorialMasterManager)target;
            }
        }

        /// <summary>
        /// Gets the editor data of this Editor.
        /// </summary>
        public EditorData EditorData
        {
            get
            {
                return EditorData.Data;
            }
        }

        /// <summary>
        /// Gets a currently selected stage for this Editor.
        /// </summary>
        public Stage SelectedStage
        {
            get
            {
                if (SelectedTutorial == null) return null;
                if (SelectedTutorial.Stages.Count == 0) return null;
                return SelectedTutorial.Stages[Mathf.Clamp(
                    EditorData.SelectedStageIndex,
                    0,
                    SelectedTutorial.Stages.Count - 1)];
            }
        }
        /// <summary>
        /// Gets a currently selected tutorial for this Editor.
        /// </summary>
        public Tutorial SelectedTutorial
        {
            get
            {
                return TutorialManager.Tutorials.Count == 0
                           ? null
                           : TutorialManager.Tutorials[Mathf.Clamp(
                               EditorData.SelectedTutorialIndex,
                               0,
                               TutorialManager.Tutorials.Count - 1)];
            }
        }

        /// <summary>
        /// Gets the first tutorial that will be executed.
        /// </summary>
        public Tutorial StartingTutorial
        {
            get
            {
                if (TutorialManager.PlayOnStart == false) return null;
                if (TutorialManager.Tutorials.Count == 0) return null;
                int tutIndex = Mathf.Clamp(
                    TutorialManager.StartingTutorialIndex,
                    0,
                    TutorialManager.Tutorials.Count - 1);
                return tutIndex >= TutorialManager.Tutorials.Count ? null : TutorialManager.Tutorials[tutIndex];
            }
        }

        /// <summary>
        /// Duplicates the property.
        /// </summary>
        /// <param name="property">The property that will be duplicated.</param>
        public void DuplicateProperty(SerializedProperty property)
        {
            property.DuplicateCommand();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Deletes the property.
        /// </summary>
        /// <param name="property">The property that will be deleted.</param>
        public void DeleteProperty(SerializedProperty property)
        {
            property.DeleteCommand();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public void MarkEditorDirty()
        {
            EditorSceneManager.MarkAllScenesDirty();
        }

        public override void OnInspectorGUI()
        {
            InitSections();
            RenderSections();
        }

        /// <summary>
        /// Registers the undo.
        /// </summary>
        /// <param name="actionName">Name of the action. This will show up in the undo history.</param>
        public void RegisterUndo(string actionName)
        {
            Undo.RecordObject(TutorialManager, actionName);
        }

        /// <summary>
        /// Initializes the sections of the GUI.
        /// </summary>
        private void InitSections()
        {
            if (_debugControllerEditor == null)
                _debugControllerEditor = new DebugControllerEditor(ref _editor);

            if (_localizationSettings == null)
                _localizationSettings = new LocalizationEditor(ref _editor);

            if (_startupSettings == null)
                _startupSettings = new StartupSettingsEditor(ref _editor);

            if (_poolingEditor == null)
                _poolingEditor = new PoolingEditor(ref _editor);

            if (_tutorialList == null)
                _tutorialList = new TutorialListEditor(ref _editor);

            if (_tutorialSettings == null)
                _tutorialSettings = new TutorialEditor(ref _editor);

            if (_stageList == null)
                _stageList = new StageListEditor(ref _editor);

            if (_stageSettings == null)
                _stageSettings = new StageEditor(ref _editor);
        }

        /// <summary>
        /// Called when this Editor is being focused on.
        /// </summary>
        private void OnEnable()
        {
            _editor = this;
        }

        /// <summary>
        /// Renders all the sections of the Tutorial Editor component.
        /// </summary>
        private void RenderSections()
        {
            Repaint();
            _debugControllerEditor.RenderGUI(Box, canRender: EditorApplication.isPlaying);
            _localizationSettings.RenderGUI(
                Box,
                brief: TutorialManager.LocalizationData != null
                    ? string.Format("Selected Language: \"{0}\"", TutorialManager.LocalizationData.Languages[TutorialManager.LocalizationData.CurrentLanguageIndex].Name)
                    : ""
            );
            _poolingEditor.RenderGUI(Box);
            _startupSettings.RenderGUI(
                Box,
                brief: StartingTutorial != null
                   ? string.Format("Starting Tutorial: \"{0}\"", StartingTutorial.Name)
                   : "");
            _tutorialList.RenderGUI(
                Box,
                brief: SelectedTutorial != null
                   ? string.Format("Selected Tutorial: \"{0}\"", SelectedTutorial.Name)
                   : "");
            _tutorialSettings.RenderGUI(Box, canRender: SelectedTutorial != null);
            _stageList.RenderGUI(
                Box,
                canRender: SelectedTutorial != null,
                brief: string.Format(
                    "Selected Stage: {0}",
                    SelectedStage != null ? string.Format("\"{0}\"", SelectedStage.Name) : "None"));

            _stageSettings.RenderGUI(Box, canRender: SelectedTutorial != null && SelectedStage != null);
        }
    }
}