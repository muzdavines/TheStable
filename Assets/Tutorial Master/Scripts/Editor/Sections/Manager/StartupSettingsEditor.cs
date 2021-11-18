using UnityEditor;

using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    public sealed class StartupSettingsEditor : Section
    {
        public StartupSettingsEditor(ref TutorialMasterEditor editor)
            : base(
                ref editor,
                "Start Up Settings",
                "Section where you can set Tutorial Master to automatically start a specific Tutorial when the game starts.")
        {
        }

        /// <summary>
        /// Gets the tutorial names.
        /// </summary>
        /// <value>
        /// The tutorial names as well as their index numbering alongside.
        /// </value>
        private string[] TutorialNames
        {
            get
            {
                string[] tutorialNames = new string[Editor.TutorialManager.Tutorials.Count];

                for (int i = 0; i < Editor.TutorialManager.Tutorials.Count; i++)
                {
                    tutorialNames[i] = string.Format("{0} - {1}", i, Editor.TutorialManager.Tutorials[i].Name);
                }

                return tutorialNames;
            }
        }

        private int _startingTutorialIndex;

        protected override void OnSectionGUI()
        {
            GUI.enabled = (Editor.TutorialManager.Tutorials.Count > 0);

            EditorField.Field(Editor.serializedObject.FindProperty("PlayOnStart"));

            GUI.enabled = true;

            if (Editor.TutorialManager.Tutorials.Count > 0)
            {
                if (Editor.TutorialManager.PlayOnStart)
                {
                    _startingTutorialIndex = Editor.TutorialManager.StartingTutorialIndex;

                    GUI.changed = false;

                    _startingTutorialIndex = EditorGUILayout.Popup(
                        _startingTutorialIndex,
                        TutorialNames);

                    if (GUI.changed)
                    {
                        SetStartingTutorial(_startingTutorialIndex);
                    }
                }
            }
            else
            {
                Editor.TutorialManager.PlayOnStart = false;
                Editor.TutorialManager.StartingTutorialIndex = 0;
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
                        new GUIContent(string.Format("Set Starting Tutorial/{0} - \"{1}\"", i, tutorial.Name)),
                        Editor.TutorialManager.StartingTutorialIndex == i,
                        () => SetStartingTutorial(tutorialIndex));
                }

                menu.ShowAsContext();

                ev.Use();
            }
        }

        private void SetStartingTutorial(int index)
        {
            Editor.RegisterUndo(string.Format("Set Starting Tutorial - \"{0}\"", Editor.TutorialManager.Tutorials[index].Name));

            Editor.TutorialManager.StartingTutorialIndex = index;

            Editor.MarkEditorDirty();
        }
    }
}