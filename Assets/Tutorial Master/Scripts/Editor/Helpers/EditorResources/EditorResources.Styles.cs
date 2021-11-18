using UnityEditor;

using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    /// <summary>
    /// Used to load required editor resources such as icons.
    /// </summary>
    public static partial class EditorResources
    {
        /// <summary>
        /// Responsible for loading all Editor Styles
        /// </summary>
        public static class Styles
        {
            private static GUIStyle _listItem;

            private static GUIStyle _normalListItem;

            private static GUIStyle _normalListIconItem;

            private static GUIStyle _selectedListItem;

            private static GUIStyle _selectedListIconItem;

            private static GUIStyle _sectionBriefLabelStyle;

            private static GUIStyle _sectionTitleLabelStyle;

            private static GUIStyle _boldSectionTitleLabelStyle;

            private static GUIStyle _moduleButtonIconStyle;

            private static Texture2D _listItemBackground;

            private static GUIStyle _dropdownStyle;

            private static readonly Color NormalTextColor = new Color(0.89F, 0.89F, 0.89F);

            /// <summary>
            /// Gets the dropdown style.
            /// </summary>
            /// <value>
            /// The dropdown style.
            /// </value>
            public static GUIStyle DropdownStyle
            {
                get
                {
                    if (_dropdownStyle == null)
                    {
                        _dropdownStyle = new GUIStyle(EditorStyles.popup);
                        _dropdownStyle.active = _dropdownStyle.normal;
                    }

                    return _dropdownStyle;
                }
            }

            /// <summary>
            /// List Item Container
            /// </summary>
            /// <value>
            /// GUIStyle for this style
            /// </value>
            public static GUIStyle ListItemContainer
            {
                get
                {
                    if (_listItem == null)
                    {
                        _listItem = new GUIStyle(GUI.skin.box)
                        {
                            padding = new RectOffset(0, 0, 0, 0),
                            margin = new RectOffset(0, 0, 0, 0)
                        };
                    }

                    return _listItem;
                }
            }

            /// <summary>
            /// Label style for Section GUI Brief
            /// </summary>
            /// <value>
            /// GUIStyle for this style
            /// </value>
            public static GUIStyle SectionBriefLabelStyle
            {
                get
                {
                    if (_sectionBriefLabelStyle == null)
                    {
                        _sectionBriefLabelStyle =
                            new GUIStyle(EditorStyles.miniLabel) { margin = new RectOffset(0, 0, 0, 0) };
                    }

                    return _sectionBriefLabelStyle;
                }
            }

            /// <summary>
            /// List Item Background
            /// </summary>
            /// <value>
            /// Texture representing a background of the list ItemPrefab
            /// </value>
            private static Texture2D ListItemBackground
            {
                get
                {
                    if (_listItemBackground == null)
                    {
                        _listItemBackground = (Texture2D)AssetDatabase.LoadAssetAtPath(
                            TMEditorUtils.DirectoryPath + "/Skins/Items/tm_item_selected.png",
                            typeof(Texture2D));
                    }

                    return _listItemBackground;
                }
            }

            /// <summary>
            /// Normal List Item Icon
            /// </summary>
            /// <value>
            /// GUIStyle for this style
            /// </value>
            private static GUIStyle NormalListIconItem
            {
                get
                {
                    if (_normalListIconItem == null)
                    {
                        _normalListIconItem = new GUIStyle(GUI.skin.button)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            margin = new RectOffset(0, 0, 0, 0),
                            padding = new RectOffset(5, 5, 5, 5),
                            active =
                            {
                                background = ListItemBackground
                            },
                            normal =
                            {
                                background = ListItemBackground,
                                textColor = Color.black
                            }
                        };

                        if (EditorGUIUtility.isProSkin)
                        {
                            _normalListIconItem.normal.textColor = NormalTextColor;
                            _normalListIconItem.active.textColor = NormalTextColor;
                        }
                    }

                    return _normalListIconItem;
                }
            }

            /// <summary>
            /// Normal List Item
            /// </summary>
            /// <value>
            /// GUIStyle for this style
            /// </value>
            private static GUIStyle NormalListItem
            {
                get
                {
                    if (_normalListItem == null)
                    {
                        _normalListItem = new GUIStyle(GUI.skin.button)
                        {
                            alignment = TextAnchor.MiddleLeft,
                            margin = new RectOffset(0, 0, 0, 0),
                            active =
                            {
                                background = ListItemBackground,
                                textColor = Color.black
                            },
                            normal =
                            {
                                background = ListItemBackground,
                                textColor = Color.black
                            }
                        };

                        if (EditorGUIUtility.isProSkin)
                        {
                            _normalListItem.normal.textColor = NormalTextColor;
                            _normalListItem.active.textColor = NormalTextColor;
                        }
                    }

                    return _normalListItem;
                }
            }

            /// <summary>
            /// Selected List Item Icon
            /// </summary>
            /// <value>
            /// GUIStyle for this style
            /// </value>
            private static GUIStyle SelectedListIconItem
            {
                get
                {
                    if (_selectedListIconItem == null)
                    {
                        _selectedListIconItem = new GUIStyle(GUI.skin.button)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            margin = new RectOffset(0, 0, 0, 0),
                            padding = new RectOffset(5, 5, 5, 5),
                            active =
                            {
                                background = ListItemBackground
                            },
                            normal =
                            {
                                background = ListItemBackground,
                                textColor = Color.white
                            }
                        };
                    }

                    return _selectedListIconItem;
                }
            }

            /// <summary>
            /// Selected List Item
            /// </summary>
            /// <value>
            /// GUIStyle for this style
            /// </value>
            private static GUIStyle SelectedListItem
            {
                get
                {
                    if (_selectedListItem == null)
                    {
                        _selectedListItem = new GUIStyle(GUI.skin.button)
                        {
                            alignment = TextAnchor.MiddleLeft,
                            margin = new RectOffset(0, 0, 0, 0),
                            active =
                            {
                                background = ListItemBackground,
                                textColor = Color.white
                            },
                            normal =
                            {
                                background = ListItemBackground,
                                textColor = Color.white
                            }
                        };
                    }

                    return _selectedListItem;
                }
            }

            /// <summary>
            /// Label style for Section GUI Title
            /// </summary>
            /// <value>
            /// GUIStyle for this style
            /// </value>
            private static GUIStyle SectionTitleLabelStyle
            {
                get
                {
                    if (_sectionTitleLabelStyle == null)
                    {
                        _sectionTitleLabelStyle =
                            new GUIStyle(EditorStyles.label) { margin = new RectOffset(0, 0, 0, 0) };
                    }

                    return _sectionTitleLabelStyle;
                }
            }

            /// <summary>
            /// Bold Label style for Section GUI Title
            /// </summary>
            /// <value>
            /// GUIStyle for this style
            /// </value>
            private static GUIStyle BoldSectionTitleLabelStyle
            {
                get
                {
                    if (_boldSectionTitleLabelStyle == null)
                    {
                        _boldSectionTitleLabelStyle =
                            new GUIStyle(EditorStyles.boldLabel) { margin = new RectOffset(0, 0, 0, 0) };
                    }

                    return _boldSectionTitleLabelStyle;
                }
            }

            /// <summary>
            /// Icon Button style for Module Config
            /// </summary>
            /// <value>
            /// GUIStyle for this style
            /// </value>
            public static GUIStyle ModuleButtonIconStyle
            {
                get
                {
                    if (_moduleButtonIconStyle == null)
                    {
                        _moduleButtonIconStyle =
                            new GUIStyle(GUI.skin.button)
                            {
                                alignment = TextAnchor.MiddleCenter,
                                margin = new RectOffset(5, 0, 1, 0),
                                padding = new RectOffset(0, 0, 0, 0)
                            };
                    }

                    return _moduleButtonIconStyle;
                }
            }

            /// <summary>
            /// Gets the section title label style.
            /// </summary>
            /// <param name="boldStyle">If true, bold style will be returned</param>
            /// <returns>Evaluated GUIStyle</returns>
            public static GUIStyle GetSectionTitleLabelStyle(bool boldStyle)
            {
                return boldStyle ? BoldSectionTitleLabelStyle : SectionTitleLabelStyle;
            }

            /// <summary>
            /// Gets the list icon ItemPrefab style.
            /// </summary>
            /// <param name="useSelectedStyle">If true, selected list icon ItemPrefab style will be used.</param>
            /// <returns>Evaluated GUIStyle</returns>
            public static GUIStyle GetListIconItemStyle(bool useSelectedStyle)
            {
                return useSelectedStyle ? SelectedListIconItem : NormalListIconItem;
            }

            /// <summary>
            /// Gets the list ItemPrefab style.
            /// </summary>
            /// <param name="useSelectedStyle">If trye, selected list ItemPrefab will be used.</param>
            /// <returns>Evaluated GUIStyle</returns>
            public static GUIStyle GetListItemStyle(bool useSelectedStyle)
            {
                return useSelectedStyle ? SelectedListItem : NormalListItem;
            }
        }
    }
}