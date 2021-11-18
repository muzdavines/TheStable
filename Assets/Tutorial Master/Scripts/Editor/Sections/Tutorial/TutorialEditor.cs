using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    public sealed class TutorialEditor : Section
    {
        private readonly TutorialEventsEditor _tutorialEventsEditor;

        public TutorialEditor(ref TutorialMasterEditor editor)
            : base(ref editor, "Tutorial Settings")
        {
            _tutorialEventsEditor = new TutorialEventsEditor(ref Editor);

            ChildSections = new List<Section> { _tutorialEventsEditor };
        }

        protected override void OnSectionGUI()
        {
            Editor.serializedObject.Update();

            int selectedTutorialIndex = Mathf.Clamp(
                Editor.EditorData.SelectedTutorialIndex,
                0,
                Editor.TutorialManager.Tutorials.Count - 1);

            string tutorialInfo = string.Format(
                "Stages: {0}\nStatus: {1}",
                Editor.SelectedTutorial.Stages.Count,
                Editor.SelectedTutorial.IsPlaying ? "Currently Play" : "Not Play");

            EditorGUILayout.HelpBox(tutorialInfo, MessageType.None);

            EditorField.Field(
                Editor.serializedObject.FindProperty(
                    string.Format("Tutorials.Array.data[{0}].Name", selectedTutorialIndex)),
                new GUIContent("Name"));

            Editor.serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            _tutorialEventsEditor.RenderGUI();
        }
    }
}