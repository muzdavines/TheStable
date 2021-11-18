using UnityEditor;
using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    /// <inheritdoc />
    /// <summary>
    /// Used to render an editor GUI for managing all Modules.
    /// </summary>
    /// <seealso cref="T:HardCodeLab.TutorialMaster.EditorUI.Section" />
    public sealed class StageModulesEditor : Section
    {
        private const int ArrowTabIndex = 0;
        private const int PopupTabIndex = 1;
        private const int HighlighterTabIndex = 2;
        private const int ImageTabIndex = 3;

        private readonly GenericMenu _addModuleMenu;

        private readonly ArrowListEditor _arrowList;
        private readonly HighlightListEditor _highlightList;
        private readonly ImageListEditor _imageList;
        private readonly PopupListEditor _popupList;

        public StageModulesEditor(ref TutorialMasterEditor editor)
            : base(ref editor, "Modules", "Module make great guiders for teacher users the ropes of the UI.")
        {
            _arrowList = new ArrowListEditor(ref editor);
            _popupList = new PopupListEditor(ref editor);
            _imageList = new ImageListEditor(ref editor);
            _highlightList = new HighlightListEditor(ref editor);

            _addModuleMenu = new GenericMenu();
            _addModuleMenu.AddItem(new GUIContent("Arrow Module"), false, AddArrow);
            _addModuleMenu.AddItem(new GUIContent("Highlight Module"), false, AddHighlight);
            _addModuleMenu.AddItem(new GUIContent("Image Module"), false, AddImage);
            _addModuleMenu.AddItem(new GUIContent("Pop-Up Module"), false, AddPopup);
        }

        protected override void OnContextMenu()
        {
            var ev = Event.current;

            if (HeaderSectionArea.Contains(ev.mousePosition) && ev.type == EventType.ContextClick)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Expand All"), false, () => ToggleChildren(true));
                menu.AddItem(new GUIContent("Collapse All"), false, () => ToggleChildren(false));

                menu.ShowAsContext();

                ev.Use();
            }
        }

        protected override void OnSectionGUI()
        {
            if (GUILayout.Button("Add..."))
            {
                _addModuleMenu.ShowAsContext();
            }

            Data.ShowAllModules = EditorGUILayout.ToggleLeft(new GUIContent("Show all Modules",
                "If true, shows all modules at the same time (can be performance heavy)."),
                Data.ShowAllModules);

            if (!Data.ShowAllModules)
            {
                var rect = GUILayoutUtility.GetLastRect();

                string[] modules =
                {
                    string.Format("Arrows ({0})", Editor.SelectedStage.Modules.Arrows.Count),
                    string.Format("Pop-Ups ({0})", Editor.SelectedStage.Modules.Popups.Count),
                    string.Format("Highlighters ({0})", Editor.SelectedStage.Modules.Highlighters.Count),
                    string.Format("Images ({0})", Editor.SelectedStage.Modules.Images.Count),
                };

                Data.SelectedModuleTabIndex = GUILayout.Toolbar(Data.SelectedModuleTabIndex, modules,
                    GUILayout.MinWidth(rect.width));
            }

            if (Editor.SelectedStage.Modules.TotalCount > 0)
            {
                EditorGUILayout.Space();

                RenderModuleEditor(_arrowList, ArrowTabIndex, Editor.SelectedStage.Modules.Arrows.Count);
                RenderModuleEditor(_popupList, PopupTabIndex, Editor.SelectedStage.Modules.Popups.Count);
                RenderModuleEditor(_highlightList, HighlighterTabIndex, Editor.SelectedStage.Modules.Highlighters.Count);
                RenderModuleEditor(_imageList, ImageTabIndex, Editor.SelectedStage.Modules.Images.Count);
            }

            // Message box to tell that there are no modules in this Stage
            if (Data.ShowAllModules)
            {
                if (Editor.SelectedStage.Modules.TotalCount == 0)
                {
                    EditorGUILayout.HelpBox("You haven't added any Modules for this Stage.", MessageType.Warning);
                }
            }
            else
            {
                string currentModuleName = "";
                int currentModuleCount = 0;

                switch (Data.SelectedModuleTabIndex)
                {
                    case ArrowTabIndex:
                        currentModuleName = "Arrow";
                        currentModuleCount = Editor.SelectedStage.Modules.Arrows.Count;
                        break;

                    case HighlighterTabIndex:
                        currentModuleName = "Highlighter";
                        currentModuleCount = Editor.SelectedStage.Modules.Highlighters.Count;
                        break;

                    case ImageTabIndex:
                        currentModuleName = "Image";
                        currentModuleCount = Editor.SelectedStage.Modules.Images.Count;
                        break;

                    case PopupTabIndex:
                        currentModuleName = "Pop-Up";
                        currentModuleCount = Editor.SelectedStage.Modules.Popups.Count;
                        break;
                }

                if (currentModuleCount == 0)
                {
                    EditorGUILayout.HelpBox(
                        string.Format("You haven't added any {0} Modules for this Stage.", currentModuleName), 
                        MessageType.Warning);
                }
            }
        }

        /// <summary>
        /// Renders the module editor.
        /// </summary>
        /// <param name="editor">The editor that will be rendered.</param>
        /// <param name="moduleIndex">Index of the module.</param>
        /// <param name="moduleCount">The module count.</param>
        private void RenderModuleEditor(Section editor, int moduleIndex, int moduleCount)
        {
            bool showEditor = moduleCount > 0 
                              && (Data.ShowAllModules || Data.SelectedModuleTabIndex == moduleIndex);

            editor.RenderGUI(canRender: showEditor, addSpace: true);
        }

        /// <summary>
        /// Sets the Module Tab unless the tab option is disabled.
        /// </summary>
        /// <param name="tabIndex">Index of the module tab.</param>
        private void SetModuleTab(int tabIndex)
        {
            if (Data.ShowAllModules)
                return;

            Data.SelectedModuleTabIndex = tabIndex;
        }

        /// <summary>
        /// Adds the Arrow Module.
        /// </summary>
        private void AddArrow()
        {
            Editor.RegisterUndo("Adding Arrow Module");
            Editor.SelectedStage.Modules.Arrows.Add(
                new ArrowModuleConfig(Editor.TutorialManager, Editor.SelectedStage.Id));
            Editor.MarkEditorDirty();

            SetModuleTab(ArrowTabIndex);
        }

        /// <summary>
        /// Adds the Highlight Module.
        /// </summary>
        private void AddHighlight()
        {
            Editor.RegisterUndo("Adding Highlight Module");
            Editor.SelectedStage.Modules.Highlighters.Add(
                new HighlightModuleConfig(Editor.TutorialManager, Editor.SelectedStage.Id));
            Editor.MarkEditorDirty();

            SetModuleTab(HighlighterTabIndex);
        }

        /// <summary>
        /// Adds the Image Module.
        /// </summary>
        private void AddImage()
        {
            Editor.RegisterUndo("Adding Image Module");
            Editor.SelectedStage.Modules.Images.Add(
                new ImageModuleConfig(Editor.TutorialManager, Editor.SelectedStage.Id));
            Editor.MarkEditorDirty();

            SetModuleTab(ImageTabIndex);
        }

        /// <summary>
        /// Adds the Pop-Up Module.
        /// </summary>
        private void AddPopup()
        {
            Editor.RegisterUndo("Adding Pop-Up Module");
            Editor.SelectedStage.Modules.Popups.Add(
                new PopupModuleConfig(Editor.TutorialManager));
            Editor.MarkEditorDirty();

            SetModuleTab(PopupTabIndex);
        }
    }
}