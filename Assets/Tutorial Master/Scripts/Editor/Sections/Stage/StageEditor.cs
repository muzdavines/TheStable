using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    public sealed class StageEditor : Section
    {
        private readonly StageAudioEditor _stageAudioEditor;

        private readonly StageModulesEditor _stageModulesEditor;

        private readonly StageEventsEditor _stageEventsEditor;

        private readonly StageTriggerEditor _stageTriggerEditor;

        public StageEditor(ref TutorialMasterEditor editor)
            : base(ref editor, "Stage Settings", "Edit settings of this Stage")
        {
            _stageTriggerEditor = new StageTriggerEditor(ref editor);
            _stageAudioEditor = new StageAudioEditor(ref editor);
            _stageEventsEditor = new StageEventsEditor(ref editor);
            _stageModulesEditor = new StageModulesEditor(ref editor);

            ChildSections = new List<Section> { _stageTriggerEditor, _stageAudioEditor, _stageEventsEditor, _stageModulesEditor };
        }

        protected override void OnContextMenu()
        {
            Event ev = Event.current;

            if (HeaderSectionArea.Contains(ev.mousePosition) && ev.type == EventType.ContextClick)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Expand All"), false, () => ToggleChildren(true));
                menu.AddItem(new GUIContent("Collapse All"), false, () => ToggleChildren(false));

                menu.ShowAsContext();

                ev.Use();
            }
        }

        protected override void OnSectionGUI()
        {
            Editor.serializedObject.Update();

            int selectedStageIndex = Mathf.Clamp(
                Editor.EditorData.SelectedStageIndex,
                0,
                Editor.SelectedTutorial.Stages.Count - 1);
            string selectedStageBasePropertyPath = string.Format(
                "Tutorials.Array.data[{0}].Stages.Array.data[{1}]",
                Editor.EditorData.SelectedTutorialIndex,
                selectedStageIndex);

            EditorField.Field(Editor.serializedObject.FindProperty(selectedStageBasePropertyPath + ".Name"));

            Editor.serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            _stageTriggerEditor.RenderGUI(addSpace: true);
            _stageEventsEditor.RenderGUI(addSpace: true);
            _stageAudioEditor.RenderGUI(addSpace: true);
            _stageModulesEditor.RenderGUI(
                addSpace: true,
                brief: string.Format("     Total: {0}", Editor.SelectedStage.Modules.TotalCount));
        }
    }
}