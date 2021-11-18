using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    /// <summary>
    /// Creates an interactive section for rendering settings in an organized fashion
    /// </summary>
    public abstract class Section
    {
        /// <summary>
        /// Used to override the RenderGUI's canRender functionality.
        /// </summary>
        public bool ShowSection = true;

        protected Rect BodySectionArea;

        /// <summary>
        /// If true, this Section can be expanded and collapsed with a mouse click.
        /// If it's false, it will be permanently rendered in expanded state.
        /// </summary>
        protected bool CanExpand;

        /// <summary>
        /// Sub-sections of this Section.
        /// </summary>
        protected List<Section> ChildSections;

        /// <summary>
        /// The parent editor of this section
        /// </summary>
        protected TutorialMasterEditor Editor;

        /// <summary>
        /// Gets the editor data of the TutorialMasterEditor Inspector.
        /// </summary>
        protected EditorData Data
        {
            get
            {
                return EditorData.Data;
            }
        }

        /// <summary>
        /// Section area of the Header.
        /// </summary>
        protected Rect HeaderSectionArea;

        /// <summary>
        /// Title of the section
        /// </summary>
        protected string Title;

        /// <summary>
        /// The tooltip of the Section
        /// </summary>
        protected string Tooltip;

        /// <summary>
        /// Used to animate transition between closed and expanded state of this section
        /// </summary>
        private readonly AnimBool _animExpanded;

        private Color _defaultBackgroundColor;

        /// <summary>
        /// Initializes the Section
        /// </summary>
        /// <param name="editor">Reference to the TutorialMasterEditor Object</param>
        /// <param name="title">Title of the section which be rendered on top</param>
        /// <param name="tooltipContent">Tooltip of a section which will show up if user keeps a mouse over the header section.</param>
        /// <param name="canExpand">If set to false, you won't be able to collapse this section and it'll always remain expanded.</param>
        protected Section(ref TutorialMasterEditor editor, string title, string tooltipContent = "", bool canExpand = true)
        {
            ChildSections = new List<Section>();

            Title = title;
            Tooltip = tooltipContent;
            CanExpand = canExpand;
            IsExpanded = !canExpand || GetSectionToggle();

            _animExpanded = new AnimBool(IsExpanded, editor.Repaint)
            {
                speed = 3f
            };

            Editor = editor;
        }

        /// <summary>
        /// Returns <c>true</c> if section is expanded and <c>false</c> if section is collapsed
        /// </summary>
        public bool IsExpanded
        {
            get { return GetSectionToggle(); }
            set
            {
                SetSectionToggle(value);
            }
        }

        /// <summary>
        /// Returns true if user can edit the contents of a section
        /// </summary>
        protected bool CanEdit
        {
            get
            {
                return !EditorApplication.isPlaying;
            }
        }

        /// <summary>
        /// Gets the section toggle key which is uniquely identified based on the class name of this section. Used to keep track on section's state.
        /// </summary>
        /// <value>
        /// The section toggle key.
        /// </value>
        protected string SectionToggleKey
        {
            get
            {
                string className = GetType().Name.ToLower();
                string key = className.StartsWith("tm_")
                    ? className.Substring("tm_".Length)
                    : className;

                key = string.Concat("toggle_", key);

                return key;
            }
        }

        /// <summary>
        /// Renders this section.
        /// </summary>
        /// <param name="box">An optional HCL Box which is used to render boxes.</param>
        /// <param name="boldLabel">If true, the title label will be bold (bold by default).</param>
        /// <param name="canRender">If true, this section will not be shown (rendered by default).</param>
        /// <param name="addSpace">If true, a gap will be added at after the body when the section is expanded.</param>
        /// <param name="brief">Unless empty, is rendered when section is not folded.</param>
        /// <param name="indentContent">If set to true, all the content inside will be indented</param>
        public void RenderGUI(HCLBox box = null,
            bool boldLabel = true,
            bool canRender = true,
            bool addSpace = false,
            string brief = "",
            bool indentContent = false)
        {
            _defaultBackgroundColor = GUI.backgroundColor;

            Editor.serializedObject.Update();

            if (!canRender || !ShowSection) return;
            BodySectionArea = box != null
                ? EditorGUILayout.BeginVertical(box.Render(HCLBox.BORDERSTYLE.ALL))
                : EditorGUILayout.BeginVertical();

            var buttonContent =
                CanExpand
                    ? IsExpanded
                        ? new GUIContent(Title, EditorStyles.foldout.onNormal.background, Tooltip)
                        : new GUIContent(Title, EditorStyles.foldout.normal.background, Tooltip)
                    : new GUIContent(Title, Tooltip);

            HeaderSectionArea = EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField(buttonContent, EditorResources.Styles.GetSectionTitleLabelStyle(boldLabel));

            if (!IsExpanded && brief != "")
            {
                EditorGUILayout.LabelField(brief, EditorResources.Styles.SectionBriefLabelStyle);
            }

            EditorGUILayout.EndVertical();

            if (CanExpand)
            {
                if (HeaderSectionArea.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        Event.current.Use();

                        if (Event.current.button == 0)
                        {
                            ToggleExpansion();
                        }
                    }
                }
            }

            if (indentContent) 
                EditorGUI.indentLevel++;

            if (!EditorApplication.isPlaying)
            {
                if (EditorGUILayout.BeginFadeGroup(_animExpanded.faded))
                {
                    OnSectionGUI();
                    if (addSpace) 
                        EditorGUILayout.Space();
                }

                EditorGUILayout.EndFadeGroup();
            }
            else
            {
                if (IsExpanded)
                {
                    OnSectionGUI();
                    if (addSpace) 
                        EditorGUILayout.Space();
                }
            }

            if (indentContent) EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();

            OnContextMenu();

            Editor.serializedObject.ApplyModifiedProperties();

            GUI.backgroundColor = _defaultBackgroundColor;
        }

        /// <summary>
        /// Toggles the expansion/collapsing of child sections of this section.
        /// </summary>
        /// <param name="state">If true, child sections will be expanded. If false, they'll collapse.</param>
        public void ToggleChildren(bool state)
        {
            SetExpansion(state);
            if (ChildSections.Count == 0) return;
            ChildSections.ForEach(x => x.ToggleChildren(state));
        }

        /// <summary>
        /// Used to produce context menu items. Called after section has been rendered.
        /// </summary>
        protected virtual void OnContextMenu() { }

        /// <summary>
        /// Renders the body of the section GUI
        /// </summary>
        protected abstract void OnSectionGUI();

        /// <summary>
        /// Gets the current state of the section whether it's been expanded or collapsed. Will always return true if CanExpand has been set to false.
        /// </summary>
        /// <returns>Boolean representing the state of this section.</returns>
        private bool GetSectionToggle()
        {
            if (!CanExpand) return true;
            if (!EditorData.Data.SectionToggles.ContainsKey(SectionToggleKey))
            {
                EditorData.Data.SectionToggles.Add(SectionToggleKey, true);
            }

            return EditorData.Data.SectionToggles[SectionToggleKey];
        }

        /// <summary>
        /// Sets the section to either expand or collapse.
        /// </summary>
        /// <param name="state">If true, this section will expand. If false, it'll collapse.</param>
        private void SetExpansion(bool state)
        {
            IsExpanded = state;
            _animExpanded.target = IsExpanded;
        }

        /// <summary>
        /// Sets the section toggle for this section. The call is ignored if CanExpand has been set to false.
        /// </summary>
        /// <param name="value">If true, it means this section has been expanded.</param>
        private void SetSectionToggle(bool value)
        {
            if (!CanExpand) return;
            EditorData.Data.SectionToggles[SectionToggleKey] = value;
        }

        /// <summary>
        /// Toggles the expansion of this section.
        /// </summary>
        private void ToggleExpansion()
        {
            IsExpanded = !IsExpanded;
            _animExpanded.target = IsExpanded;
        }
    }
}