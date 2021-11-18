using UnityEditor;
using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    public sealed class TutorialListEditor : Section
    {
        public OnTutorialSelectedDelegate TutorialSelected;

        private const float ItemHeight = 25;

        private const float ListBoxHeight = 150;

        public TutorialListEditor(ref TutorialMasterEditor editor)
            : base(ref editor, "Tutorials List")
        {
        }

        public delegate void OnTutorialSelectedDelegate(int index);

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
                return Editor.TutorialManager.Tutorials.Count > MaxItems;
            }
        }

        public void OnTutorialSelected(int index)
        {
            if (TutorialSelected != null)
            {
                TutorialSelected.Invoke(index);
            }
        }

        protected override void OnContextMenu()
        {
            if (IsExpanded) return;

            Event ev = Event.current;

            if (HeaderSectionArea.Contains(ev.mousePosition) && ev.type == EventType.ContextClick)
            {
                GenericMenu menu = new GenericMenu();

                for (int i = 0; i < Editor.TutorialManager.Tutorials.Count; i++)
                {
                    int tutorialIndex = i;
                    var tutorial = Editor.TutorialManager.Tutorials[tutorialIndex];

                    menu.AddItem(
                        new GUIContent(string.Format("Select/{0} - \"{1}\"", i, tutorial.Name)),
                        Editor.EditorData.SelectedTutorialIndex == i,
                        () => SelectTutorial(tutorialIndex));
                }

                menu.AddItem(new GUIContent("Create Tutorial"), false, CreateTutorial);

                menu.ShowAsContext();

                ev.Use();
            }
        }

        protected override void OnSectionGUI()
        {
            if (GUILayout.Button("Create Tutorial", GUILayout.Height(25))) 
                CreateTutorial();

            if (Editor.TutorialManager.Tutorials.Count > 0)
            {
                EditorGUILayout.Space();

                HCLBox box = new HCLBox(TMEditorUtils.DirectoryPath + "/Skins/Items/HCLBox/");
                EditorGUILayout.BeginVertical(
                    box.Render(HCLBox.BORDERSTYLE.NO_SIDES, EditorResources.Styles.ListItemContainer),
                    GUILayout.Height(ListBoxHeight));

                if (ShowScrollView)
                {
                    Editor.EditorData.ScrollViewTutorialsList =
                        EditorGUILayout.BeginScrollView(Editor.EditorData.ScrollViewTutorialsList);
                }

                for (int i = 0; i < Editor.TutorialManager.Tutorials.Count; i++)
                {
                    TutorialItem(i);
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
                    "It feels a bit lonely here. Click \"Create Tutorial\" to start making your first tutorial!",
                    MessageType.Warning);
            }
        }

        /// <summary>
        /// Creates a new Tutorial and selects it
        /// </summary>
        private void CreateTutorial()
        {
            Editor.RegisterUndo("Create Tutorial");

            Editor.TutorialManager.Tutorials.Add(new Tutorial());
            SelectTutorial(Editor.TutorialManager.Tutorials.Count - 1);

            Editor.MarkEditorDirty();
        }

        /// <summary>
        /// Deletes a Tutorial
        /// </summary>
        /// <param name="index">Index of Tutorial which will be deleted</param>
        private void DeleteTutorial(int index)
        {
            if (index < 0 || index >= Editor.TutorialManager.Tutorials.Count)
                return;

            string tutorialName = Editor.TutorialManager.Tutorials[index].Name;

            Editor.RegisterUndo("Delete Tutorial - " + tutorialName);

            Editor.TutorialManager.Tutorials.RemoveAt(index);

            int nTutorials = Editor.TutorialManager.Tutorials.Count;

            if (Editor.EditorData.SelectedTutorialIndex == nTutorials)
            {
                SelectTutorial(Editor.EditorData.SelectedTutorialIndex - 1);
            }

            if (nTutorials > 0)
            {
                if (Editor.TutorialManager.StartingTutorialIndex > nTutorials - 1)
                {
                    Editor.TutorialManager.StartingTutorialIndex = nTutorials - 1;
                }
            }
            else
            {
                SelectTutorial(Editor.EditorData.SelectedTutorialIndex - 1);
            }

            Editor.MarkEditorDirty();
        }

        /// <summary>
        /// Selects a tutorial
        /// </summary>
        /// <param name="index">Index of the tutorial which should be selected. Automatically clamped if out of range.</param>
        private void SelectTutorial(int index)
        {
            if (index < 0 || index >= Editor.TutorialManager.Tutorials.Count)
                return;

            Editor.RegisterUndo(string.Format("Select Tutorial - \"{0}\"", 
                Editor.TutorialManager.Tutorials[index].Name));

            GUI.FocusControl(null);
            Editor.EditorData.SelectedTutorialIndex = Mathf.Clamp(index, 0, 
                Editor.TutorialManager.Tutorials.Count - 1);
            OnTutorialSelected(index);
        }

        private void TutorialItem(int index)
        {
            if (index >= Editor.TutorialManager.Tutorials.Count)
                return;

            Tutorial tutorial = Editor.TutorialManager.Tutorials[index];
            bool isSelected = index == Editor.EditorData.SelectedTutorialIndex;

            GUI.backgroundColor = isSelected ? new Color(62, 255, 231) : Color.clear;

            EditorGUILayout.BeginHorizontal();

            GUIContent labelContent;
            string labelName = tutorial.Name;

            float maxWidth = Screen.width - 27;

            //int maxCharLimit = (int)(maxWidth / 7.75f);

            //if (labelName.Length >= maxCharLimit)
            //{
            //    int maxLength = Math.Min((tutorial.Name).Length, maxCharLimit);
            //    labelName = (tutorial.Name).Substring(0, maxLength);

            //    labelName = labelName.Substring(0, maxLength) + "...";
            //}

            if (tutorial.IsPlaying && EditorApplication.isPlaying)
            {
                labelContent = new GUIContent(" " + labelName, EditorResources.Icons.Play, tutorial.Name);
            }
            else
            {
                labelContent = new GUIContent(labelName, tutorial.Name);
            }

            if (GUILayout.Button(
                labelContent,
                EditorResources.Styles.GetListItemStyle(isSelected),
                GUILayout.MaxWidth(maxWidth),
                GUILayout.Height(ItemHeight)))
            {
                if (Event.current.button == 0)
                {
                    SelectTutorial(index);
                }
            }

            if (GUILayout.Button(
                new GUIContent(EditorResources.Icons.Delete, "Delete this Tutorial"),
                EditorResources.Styles.GetListIconItemStyle(isSelected),
                GUILayout.Width(25),
                GUILayout.Height(ItemHeight)))
            {
                if (Event.current.button == 0) DeleteTutorial(index);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}