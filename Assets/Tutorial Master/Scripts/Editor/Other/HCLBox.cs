using UnityEditor;
using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    /// <summary>
    /// Special Box which allows to quickly render it with different border styles
    /// </summary>
    public class HCLBox
    {
        /// <summary>
        /// Background box has 4 sides, each side has borders.
        /// A normal box would have all 4 borders, however for Editor I have different boxes with variable number of borders.
        /// As a result, a unique variable naming is required to avoid confusion.
        ///
        /// The naming pattern is in the clockwise order: TOP, RIGHT, BOTTOM, LEFT
        ///
        /// EXAMPLES:
        ///     box_o_o_o_o --> means that this is a box with no borders
        ///     box_i_i_i_i --> means that this box has borders on all 4 sides
        ///
        ///     box_i_i_o_i --> means that this box has no border at the bottom
        ///     box_o_i_i_i --> means that this box has no border at the top
        ///
        /// "i" means that there is a border on a specified side
        /// "o" means that there is no border on a specified side
        ///
        /// There are limited combinations of possible boxes because not that many are needed
        /// </summary>
        /// <summary>
        /// The base GUIStyle of a default box. Used to transfer margins and padding values to the new box
        /// </summary>
        private readonly GUIStyle _baseBox;

        /// <summary>
        /// Texture of a Box with borders on all sides for Dark Skin
        /// </summary>
        private readonly Texture2D dark_box_i_i_i_i;

        /// <summary>
        /// Texture of a Box with a missing border at the bottom for Dark Skin
        /// </summary>
        private readonly Texture2D dark_box_i_i_o_i;

        /// <summary>
        /// Texture of a Box with a missing border on its Sides for Dark Skin
        /// </summary>
        private readonly Texture2D dark_box_i_o_i_o;

        /// <summary>
        /// Texture of a Box with a missing border on top for Dark Skin
        /// </summary>
        private readonly Texture2D dark_box_o_i_i_i;

        /// <summary>
        /// Texture of a Box with a missing border on Top and Bottom for Dark Skin
        /// </summary>
        private readonly Texture2D dark_box_o_i_o_i;

        /// <summary>
        /// Texture of a Box without any borders for Dark Skin
        /// </summary>
        private readonly Texture2D dark_box_o_o_o_o;

        /// <summary>
        /// Texture of a Box with borders on all sides
        /// </summary>
        private readonly Texture2D light_box_i_i_i_i;

        /// <summary>
        /// Texture of a Box with a missing border at the bottom
        /// </summary>
        private readonly Texture2D light_box_i_i_o_i;

        /// <summary>
        /// Texture of a Box with a missing border on its Sides
        /// </summary>
        private readonly Texture2D light_box_i_o_i_o;

        /// <summary>
        /// Texture of a Box with a missing border on top
        /// </summary>
        private readonly Texture2D light_box_o_i_i_i;

        /// <summary>
        /// Texture of a Box with a missing border on Top and Bottom
        /// </summary>
        private readonly Texture2D light_box_o_i_o_i;

        /// <summary>
        /// Texture of a Box without any borders
        /// </summary>
        private readonly Texture2D light_box_o_o_o_o;

        /// <summary>
        /// Initializes the box instance. Note: to render a box, Render() must be called separately
        /// </summary>
        /// <param name="textureDir">The root directory of where box textures reside. Note that the directory must end with "/" in order to work</param>
        public HCLBox(string textureDir)
        {
            _baseBox = new GUIStyle(GUI.skin.box);

            light_box_i_i_i_i = (Texture2D)AssetDatabase.LoadAssetAtPath(
                textureDir + "light_box_i_i_i_i.png",
                typeof(Texture2D));
            light_box_o_o_o_o = (Texture2D)AssetDatabase.LoadAssetAtPath(
                textureDir + "light_box_o_o_o_o.png",
                typeof(Texture2D));
            light_box_o_i_i_i = (Texture2D)AssetDatabase.LoadAssetAtPath(
                textureDir + "light_box_o_i_i_i.png",
                typeof(Texture2D));
            light_box_o_i_o_i = (Texture2D)AssetDatabase.LoadAssetAtPath(
                textureDir + "light_box_o_i_o_i.png",
                typeof(Texture2D));
            light_box_i_i_o_i = (Texture2D)AssetDatabase.LoadAssetAtPath(
                textureDir + "light_box_i_i_o_i.png",
                typeof(Texture2D));
            light_box_i_o_i_o = (Texture2D)AssetDatabase.LoadAssetAtPath(
                textureDir + "light_box_i_o_i_o.png",
                typeof(Texture2D));

            dark_box_i_i_i_i = (Texture2D)AssetDatabase.LoadAssetAtPath(
                textureDir + "dark_box_i_i_i_i.png",
                typeof(Texture2D));
            dark_box_o_o_o_o = (Texture2D)AssetDatabase.LoadAssetAtPath(
                textureDir + "dark_box_o_o_o_o.png",
                typeof(Texture2D));
            dark_box_o_i_i_i = (Texture2D)AssetDatabase.LoadAssetAtPath(
                textureDir + "dark_box_o_i_i_i.png",
                typeof(Texture2D));
            dark_box_o_i_o_i = (Texture2D)AssetDatabase.LoadAssetAtPath(
                textureDir + "dark_box_o_i_o_i.png",
                typeof(Texture2D));
            dark_box_i_i_o_i = (Texture2D)AssetDatabase.LoadAssetAtPath(
                textureDir + "dark_box_i_i_o_i.png",
                typeof(Texture2D));
            dark_box_i_o_i_o = (Texture2D)AssetDatabase.LoadAssetAtPath(
                textureDir + "dark_box_i_o_i_o.png",
                typeof(Texture2D));
        }

        /// <summary>
        /// Enum used to specify which Border Style should the Box be rendered with
        /// </summary>
        public enum BORDERSTYLE
        {
            /// <summary>
            /// The box will be rendered without any borders
            /// </summary>
            NONE,

            /// <summary>
            /// The box will be rendered with borders
            /// </summary>
            ALL,

            /// <summary>
            /// The box will be rendered with a missing border at the bottom
            /// </summary>
            NO_BOTTOM,

            /// <summary>
            /// The box will be rendered with a missing border at the top
            /// </summary>
            NO_TOP,

            /// <summary>
            /// The box will be rendered with a missing border at the top and bottom
            /// </summary>
            NO_BOTTOM_TOP,

            /// <summary>
            /// The box will be rendered with a missing border on both sides.
            /// </summary>
            NO_SIDES
        }

        /// <summary>
        /// Renders the Box
        /// </summary>
        /// <param name="borderStyle">Border style this box is going to have</param>
        /// <returns></returns>
        public GUIStyle Render(BORDERSTYLE borderStyle)
        {
            GUIStyle box = _baseBox;

            if (!EditorGUIUtility.isProSkin)
            {
                switch (borderStyle)
                {
                    case BORDERSTYLE.NONE:
                        box.normal.background = light_box_o_o_o_o;
                        break;

                    case BORDERSTYLE.ALL:
                        box.normal.background = light_box_i_i_i_i;
                        break;

                    case BORDERSTYLE.NO_BOTTOM:
                        box.normal.background = light_box_i_i_o_i;
                        break;

                    case BORDERSTYLE.NO_TOP:
                        box.normal.background = light_box_o_i_i_i;
                        break;

                    case BORDERSTYLE.NO_BOTTOM_TOP:
                        box.normal.background = light_box_o_i_o_i;
                        break;

                    case BORDERSTYLE.NO_SIDES:
                        box.normal.background = light_box_i_o_i_o;
                        break;

                    default:
                        return box;
                }
            }
            else
            {
                switch (borderStyle)
                {
                    case BORDERSTYLE.NONE:
                        box.normal.background = dark_box_o_o_o_o;
                        break;

                    case BORDERSTYLE.ALL:
                        box.normal.background = dark_box_i_i_i_i;
                        break;

                    case BORDERSTYLE.NO_BOTTOM:
                        box.normal.background = dark_box_i_i_o_i;
                        break;

                    case BORDERSTYLE.NO_TOP:
                        box.normal.background = dark_box_o_i_i_i;
                        break;

                    case BORDERSTYLE.NO_BOTTOM_TOP:
                        box.normal.background = dark_box_o_i_o_i;
                        break;

                    case BORDERSTYLE.NO_SIDES:
                        box.normal.background = dark_box_i_o_i_o;
                        break;

                    default:
                        return box;
                }
            }

            return box;
        }

        /// <summary>
        /// Renders the Box with a custom GUIStyle box
        /// </summary>
        /// <param name="borderStyle">Border style this box is going to have</param>
        /// <param name="_box">GUIStyle of a box.</param>
        /// <returns></returns>
        public GUIStyle Render(BORDERSTYLE borderStyle, GUIStyle _box)
        {
            GUIStyle box = _box;

            if (!EditorGUIUtility.isProSkin)
            {
                switch (borderStyle)
                {
                    case BORDERSTYLE.NONE:
                        box.normal.background = light_box_o_o_o_o;
                        break;

                    case BORDERSTYLE.ALL:
                        box.normal.background = light_box_i_i_i_i;
                        break;

                    case BORDERSTYLE.NO_BOTTOM:
                        box.normal.background = light_box_i_i_o_i;
                        break;

                    case BORDERSTYLE.NO_TOP:
                        box.normal.background = light_box_o_i_i_i;
                        break;

                    case BORDERSTYLE.NO_BOTTOM_TOP:
                        box.normal.background = light_box_o_i_o_i;
                        break;

                    case BORDERSTYLE.NO_SIDES:
                        box.normal.background = light_box_i_o_i_o;
                        break;

                    default:
                        return box;
                }
            }
            else
            {
                switch (borderStyle)
                {
                    case BORDERSTYLE.NONE:
                        box.normal.background = dark_box_o_o_o_o;
                        break;

                    case BORDERSTYLE.ALL:
                        box.normal.background = dark_box_i_i_i_i;
                        break;

                    case BORDERSTYLE.NO_BOTTOM:
                        box.normal.background = dark_box_i_i_o_i;
                        break;

                    case BORDERSTYLE.NO_TOP:
                        box.normal.background = dark_box_o_i_i_i;
                        break;

                    case BORDERSTYLE.NO_BOTTOM_TOP:
                        box.normal.background = dark_box_o_i_o_i;
                        break;

                    case BORDERSTYLE.NO_SIDES:
                        box.normal.background = dark_box_i_o_i_o;
                        break;

                    default:
                        return box;
                }
            }

            return box;
        }
    }
}