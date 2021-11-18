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
        /// Responsible for loading all Editor Icons
        /// </summary>
        public static class Icons
        {
            private static Texture2D _warning;

            private static Texture2D _duplicate;

            private static Texture2D _trashBin;

            private static Texture2D _delete;

            private static Texture2D _play;

            private static Texture2D _selectedToggleEnabled;

            private static Texture2D _selectedToggleDisabled;

            private static Texture2D _normalToggleEnabled;

            private static Texture2D _normalToggleDisabled;

            /// <summary>
            /// "Warning" icon
            /// </summary>
            /// <value>
            /// The Texture2D representing an icon
            /// </value>
            public static Texture2D Warning
            {
                get
                {
                    if (_warning == null)
                    {
                        _warning = (Texture2D)AssetDatabase.LoadAssetAtPath(
                            TMEditorUtils.DirectoryPath + "/Skins/Icons/tm_icon_warning.png",
                            typeof(Texture2D));
                    }

                    return _warning;
                }
            }

            /// <summary>
            /// "Duplicate" icon
            /// </summary>
            /// <value>
            /// The Texture2D representing an icon
            /// </value>
            public static Texture2D Duplicate
            {
                get
                {
                    if (_duplicate == null)
                    {
                        _duplicate = (Texture2D)AssetDatabase.LoadAssetAtPath(
                            TMEditorUtils.DirectoryPath + "/Skins/Icons/tm_icon_duplicate.png",
                            typeof(Texture2D));
                    }

                    return _duplicate;
                }
            }

            /// <summary>
            /// "Trash bin" icon
            /// </summary>
            /// <value>
            /// The Texture2D representing an icon
            /// </value>
            public static Texture2D TrashBin
            {
                get
                {
                    if (_trashBin == null)
                    {
                        _trashBin = (Texture2D)AssetDatabase.LoadAssetAtPath(
                            TMEditorUtils.DirectoryPath + "/Skins/Icons/tm_icon_trash.png",
                            typeof(Texture2D));
                    }

                    return _trashBin;
                }
            }

            /// <summary>
            /// "Delete" icon
            /// </summary>
            /// <value>
            /// The Texture2D representing an icon
            /// </value>
            public static Texture2D Delete
            {
                get
                {
                    if (_delete == null)
                    {
                        _delete = (Texture2D)AssetDatabase.LoadAssetAtPath(
                            TMEditorUtils.DirectoryPath + "/Skins/Icons/tm_icon_delete.png",
                            typeof(Texture2D));
                    }

                    return _delete;
                }
            }

            /// <summary>
            /// "Play" icon
            /// </summary>
            /// <value>
            /// The Texture2D representing an icon
            /// </value>
            public static Texture2D Play
            {
                get
                {
                    if (_play == null)
                    {
                        _play = (Texture2D)AssetDatabase.LoadAssetAtPath(
                            TMEditorUtils.DirectoryPath + "/Skins/Icons/tm_icon_playing.png",
                            typeof(Texture2D));
                    }

                    return _play;
                }
            }

            private static Texture2D SelectedToggleEnabled
            {
                get
                {
                    if (_selectedToggleEnabled == null)
                    {
                        _selectedToggleEnabled = (Texture2D)AssetDatabase.LoadAssetAtPath(
                            TMEditorUtils.DirectoryPath + "/Skins/Icons/tm_icon_stage_enabled_selected.png",
                            typeof(Texture2D));
                    }

                    return _selectedToggleEnabled;
                }
            }

            private static Texture2D SelectedToggleDisabled
            {
                get
                {
                    if (_selectedToggleDisabled == null)
                    {
                        _selectedToggleDisabled = (Texture2D)AssetDatabase.LoadAssetAtPath(
                            TMEditorUtils.DirectoryPath + "/Skins/Icons/tm_icon_stage_disabled_selected.png",
                            typeof(Texture2D));
                    }

                    return _selectedToggleDisabled;
                }
            }

            private static Texture2D NormalToggleEnabled
            {
                get
                {
                    if (_normalToggleEnabled == null)
                    {
                        _normalToggleEnabled = (Texture2D)AssetDatabase.LoadAssetAtPath(
                            TMEditorUtils.DirectoryPath + "/Skins/Icons/tm_icon_stage_enabled.png",
                            typeof(Texture2D));
                    }

                    return _normalToggleEnabled;
                }
            }

            private static Texture2D NormalToggleDisabled
            {
                get
                {
                    if (_normalToggleDisabled == null)
                    {
                        _normalToggleDisabled = (Texture2D)AssetDatabase.LoadAssetAtPath(
                            TMEditorUtils.DirectoryPath + "/Skins/Icons/tm_icon_stage_disabled.png",
                            typeof(Texture2D));
                    }

                    return _normalToggleDisabled;
                }
            }

            /// <summary>
            /// Gets an appropriate toggle texture.
            /// </summary>
            /// <param name="isEnabled">If true, only enabled toggle will be returned.</param>
            /// <param name="useSelectedStyle">If true, selected style will be returned.</param>
            /// <returns>Evaluated texture.</returns>
            public static Texture2D GetToggle(bool isEnabled, bool useSelectedStyle)
            {
                if (isEnabled)
                {
                    return useSelectedStyle ? SelectedToggleEnabled : NormalToggleEnabled;
                }
                else
                {
                    return useSelectedStyle ? SelectedToggleDisabled : NormalToggleDisabled;
                }
            }
        }
    }
}