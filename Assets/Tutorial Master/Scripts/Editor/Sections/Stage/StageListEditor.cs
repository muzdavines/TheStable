using UnityEditor;

using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    public sealed class StageListEditor : Section
    {
        private const float ItemHeight = 25;

        private const float ListBoxHeight = 150;

        public StageListEditor(ref TutorialMasterEditor editor)
            : base(ref editor, "Stages List", "Lists all Stages for a currently inspected Tutorial")
        {
        }

        private int MaxItems
        {
            get
            {
                return (int)(ListBoxHeight / ItemHeight);
            }
        }

        private bool ShowScrollView
        {
            get
            {
                return Editor.SelectedTutorial.Stages.Count > MaxItems;
            }
        }

        protected override void OnContextMenu()
        {
            if (IsExpanded) return;

            Event ev = Event.current;

            if (HeaderSectionArea.Contains(ev.mousePosition) && ev.type == EventType.ContextClick)
            {
                GenericMenu menu = new GenericMenu();

                for (int i = 0; i < Editor.SelectedTutorial.Stages.Count; i++)
                {
                    int stageIndex = i;
                    var stage = Editor.SelectedTutorial.Stages[stageIndex];

                    menu.AddItem(
                        new GUIContent(string.Format("Select/{0} - \"{1}\"", i, stage.Name)),
                        Editor.EditorData.SelectedStageIndex == i,
                        () => SelectStage(stageIndex));
                }

                menu.AddItem(new GUIContent("New Stage"), false, AddStage);

                menu.ShowAsContext();
                ev.Use();
            }
        }

        protected override void OnSectionGUI()
        {
            if (GUILayout.Button("New Stage", GUILayout.Height(25))) 
                AddStage();

            EditorGUILayout.Space();

            if (Editor.SelectedTutorial.Stages.Count > 0)
            {
                HCLBox box = new HCLBox(TMEditorUtils.DirectoryPath + "/Skins/Items/HCLBox/");
                EditorGUILayout.BeginVertical(
                    box.Render(HCLBox.BORDERSTYLE.NO_SIDES, EditorResources.Styles.ListItemContainer),
                    GUILayout.Height(ListBoxHeight));

                if (ShowScrollView)
                {
                    Editor.EditorData.ScrollViewStagesList =
                        EditorGUILayout.BeginScrollView(Editor.EditorData.ScrollViewStagesList);
                }

                for (int i = 0; i < Editor.SelectedTutorial.Stages.Count; i++)
                {
                    StageItem(i);
                }

                if (ShowScrollView)
                {
                    EditorGUILayout.EndScrollView();
                }

                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "You haven't added any Stages for this Tutorial. Click \"New Stage\" to get started!",
                    MessageType.Warning);
            }
        }

        /// <summary>
        /// Adds a new Stage.
        /// </summary>
        private void AddStage()
        {
            Editor.RegisterUndo("Add Stage");

            Stage newStage = new Stage(Editor.SelectedTutorial.Id, "Stage " + Editor.SelectedTutorial.Stages.Count);

            int stageCount = Editor.SelectedTutorial.Stages.Count;
            int insertIndex = stageCount;

            if (stageCount > 1)
            {
                insertIndex = Mathf.Clamp(Editor.EditorData.SelectedStageIndex + 1, 0, stageCount);
                Editor.SelectedTutorial.Stages.Insert(insertIndex, newStage);
            }
            else
            {
                Editor.SelectedTutorial.Stages.Add(newStage);
            }

            SelectStage(insertIndex);

            Editor.MarkEditorDirty();
        }

        /// <summary>
        /// Deletes a Stage.
        /// </summary>
        /// <param name="index">The index of a Stage that will be deleted.</param>
        private void DeleteStage(int index)
        {
            Editor.RegisterUndo(string.Format("Delete Stage - \"{0}\"", Editor.SelectedTutorial.Stages[index].Name));

            Editor.SelectedTutorial.Stages.RemoveAt(index);

            int stageCount = Editor.SelectedTutorial.Stages.Count;

            if (Editor.EditorData.SelectedStageIndex == stageCount)
            {
                SelectStage(Editor.EditorData.SelectedStageIndex - 1);
            }

            if (stageCount == 0)
            {
                SelectStage(Editor.EditorData.SelectedStageIndex - 1);
            }

            Editor.MarkEditorDirty();
        }

        /// <summary>
        /// Duplicates a Stage
        /// </summary>
        /// <param name="index">The index of a stage that will be duplicated.</param>
        private void DuplicateStage(int index)
        {
            Editor.RegisterUndo(string.Format("Duplicate Stage - \"{0}\"", Editor.SelectedTutorial.Stages[index].Name));

            string targetStagePath = string.Format(
                "Tutorials.Array.data[{0}].Stages.Array.data[{1}]",
                Editor.EditorData.SelectedTutorialIndex,
                Mathf.Clamp(index, 0, Editor.SelectedTutorial.Stages.Count - 1));

            Editor.DuplicateProperty(Editor.serializedObject.FindProperty(targetStagePath));

            int newIndex = Mathf.Clamp(index + 1, 0, Editor.SelectedTutorial.Stages.Count - 1);
            string newStagePath = string.Format(
                "Tutorials.Array.data[{0}].Stages.Array.data[{1}]",
                Editor.EditorData.SelectedTutorialIndex,
                newIndex);

            Editor.serializedObject.FindProperty(newStagePath + ".Name").stringValue += " (Copy)";

            SelectStage(newIndex);
            Editor.MarkEditorDirty();
        }

        /// <summary>
        /// Moves the stage to a different tutorial
        /// </summary>
        /// <param name="stageIndex">Index of the stage which will be moved.</param>
        /// <param name="tutorialIndex">Index of the new tutorial where stage will be moved to.</param>
        private void MoveStage(int stageIndex, int tutorialIndex)
        {
            var targetStage = Editor.SelectedTutorial.Stages[stageIndex];
            var newTutorialDestination = Editor.TutorialManager.Tutorials[tutorialIndex];

            Editor.RegisterUndo(
                string.Format("Move Stage - \"{0}\" from \"{1}\"", targetStage.Name, Editor.SelectedTutorial.Name));

            newTutorialDestination.Stages.Add(targetStage);
            targetStage.SetParentTutorial(newTutorialDestination);
            Editor.SelectedTutorial.Stages.RemoveAt(stageIndex);

            GUI.FocusControl(null);
            Editor.EditorData.SelectedTutorialIndex = Mathf.Clamp(
                tutorialIndex,
                0,
                Editor.TutorialManager.Tutorials.Count - 1);
            SelectStage(Editor.SelectedTutorial.Stages.Count - 1);

            Editor.MarkEditorDirty();
        }

        /// <summary>
        /// Selects a Stage to focus on.
        /// </summary>
        /// <param name="index">The index of a Stage.</param>
        private void SelectStage(int index)
        {
            GUI.FocusControl(null);
            Editor.EditorData.SelectedStageIndex = Mathf.Clamp(index, 0, Editor.SelectedTutorial.Stages.Count - 1);
        }

        /// <summary>
        /// Shifts the stage down the list
        /// </summary>
        /// <param name="index">The index of a Stage.</param>
        private void ShiftStageDown(int index)
        {
            if (index >= Editor.SelectedTutorial.Stages.Count - 1) return;

            Editor.RegisterUndo("Shift Stage Down");

            Stage oldStage = Editor.SelectedTutorial.Stages[index + 1];
            Editor.SelectedTutorial.Stages[index + 1] = Editor.SelectedTutorial.Stages[index];
            Editor.SelectedTutorial.Stages[index] = oldStage;

            if (Editor.EditorData.SelectedStageIndex == index)
            {
                SelectStage(index + 1);
            }

            Editor.MarkEditorDirty();
        }

        /// <summary>
        /// Shifts the stage up the list
        /// </summary>
        /// <param name="index">The index of a Stage.</param>
        private void ShiftStageUp(int index)
        {
            if (index <= 0) return;

            Editor.RegisterUndo("Shift Stage Up");

            Stage oldStage = Editor.SelectedTutorial.Stages[index - 1];
            Editor.SelectedTutorial.Stages[index - 1] = Editor.SelectedTutorial.Stages[index];
            Editor.SelectedTutorial.Stages[index] = oldStage;

            if (Editor.EditorData.SelectedStageIndex == index)
            {
                SelectStage(index - 1);
            }

            Editor.MarkEditorDirty();
        }

        private void StageItem(int index)
        {
            Event _event = Event.current;

            Stage stage = Editor.SelectedTutorial.Stages[index];
            bool isSelected = index == Editor.EditorData.SelectedStageIndex;

            GUI.backgroundColor = isSelected ? new Color(62, 255, 231) : Color.clear;

            float maxWidth = Screen.width - 52;

            GUIContent labelContent;
            string labelName = " " + stage.Name;
            int maxCharLimit = (int)(maxWidth / 7.75f);

            if (labelName.Length >= maxCharLimit)
            {
                int maxLength = Mathf.Min(stage.Name.Length, maxCharLimit);
                labelName = stage.Name.Substring(0, maxLength);

                labelName = labelName.Substring(0, maxLength) + "...";
            }

            if (stage.IsPlaying && EditorApplication.isPlaying)
            {
                labelContent = new GUIContent(" " + labelName, EditorResources.Icons.Play, stage.Name);
            }
            else
            {
                labelContent = new GUIContent(stage.Name, stage.Name);
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(
                new GUIContent(
                    EditorResources.Icons.GetToggle(stage.IsEnabled, isSelected),
                    "Toggle the state of this Stage."),
                EditorResources.Styles.GetListItemStyle(isSelected),
                GUILayout.Width(25),
                GUILayout.Height(ItemHeight)))
            {
                if (_event.button == 0)
                {
                    ToggleStage(index);
                }
            }

            if (GUILayout.Button(
                labelContent,
                EditorResources.Styles.GetListItemStyle(isSelected),
                GUILayout.MaxWidth(maxWidth),
                GUILayout.Height(ItemHeight)))
            {
                SelectStage(index);

                if (_event.button == 1)
                {
                    StageItemContextMenu(index);
                }
            }

            if (GUILayout.Button(
                new GUIContent(EditorResources.Icons.Duplicate, "Duplicate this Stage"),
                EditorResources.Styles.GetListIconItemStyle(isSelected),
                GUILayout.Width(25),
                GUILayout.Height(ItemHeight)))
            {
                if (_event.button == 0)
                {
                    DuplicateStage(index);
                }
            }

            if (GUILayout.Button(
                new GUIContent(EditorResources.Icons.Delete, "Delete this Stage"),
                EditorResources.Styles.GetListIconItemStyle(isSelected),
                GUILayout.Width(25),
                GUILayout.Height(ItemHeight)))
            {
                if (_event.button == 0)
                {
                    DeleteStage(index);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void StageItemContextMenu(int index)
        {
            var menu = new GenericMenu();

            if (Editor.SelectedTutorial.Stages.Count > 1)
            {
                if (index > 0)
                {
                    menu.AddItem(new GUIContent("Shift/Up"), false, () => ShiftStageUp(index));
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Shift/Up"));
                }

                if (index < Editor.SelectedTutorial.Stages.Count - 1)
                {
                    menu.AddItem(new GUIContent("Shift/Down"), false, () => ShiftStageDown(index));
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Shift/Down"));
                }
            }

            if (Editor.TutorialManager.Tutorials.Count > 1)
            {
                for (int i = 0; i < Editor.TutorialManager.Tutorials.Count; i++)
                {
                    if (Editor.EditorData.SelectedTutorialIndex == i) continue;
                    int tutorialIndex = i;

                    menu.AddItem(
                        new GUIContent(
                            string.Format(
                                "Move To/{0} - \"{1}\"",
                                tutorialIndex,
                                Editor.TutorialManager.Tutorials[i].Name)),
                        false,
                        () => MoveStage(index, tutorialIndex));
                }
            }

            menu.ShowAsContext();
        }

        /// <summary>
        /// Toggles the Stage.
        /// </summary>
        /// <param name="index">The index of a Stage.</param>
        private void ToggleStage(int index)
        {
            Editor.RegisterUndo(string.Format("Toggle Stage - \"{0}\"", Editor.SelectedTutorial.Stages[index].Name));

            Editor.SelectedTutorial.Stages[index].IsEnabled = !Editor.SelectedTutorial.Stages[index].IsEnabled;

            Editor.MarkEditorDirty();
        }
    }
}