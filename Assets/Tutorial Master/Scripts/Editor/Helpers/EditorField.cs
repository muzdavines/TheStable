using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    public static class EditorField
    {
        /// <summary>
        /// Renders a property field
        /// </summary>
        /// <param name="property">The property which will be rendered.</param>
        /// <param name="content">The content for this property. If it's null, tooltip will be tried to be fetched from the Database.</param>
        public static SerializedProperty Field(SerializedProperty property, GUIContent content = null)
        {
            if (property == null)
            {
                EditorGUILayout.LabelField("Unable to find property!");
                return null;
            }

            string labelName = content == null ? property.displayName : content.text;
            string tooltip = content == null ? "" : content.tooltip;

            EditorGUILayout.PropertyField(property, new GUIContent(labelName, tooltip), true);
            GUI.color = Color.white;

            return property;
        }

        /// <summary>
        /// Returns the unique path. Used to distinguish fields from one another
        /// </summary>
        /// <param name="originalPath">The original property path.</param>
        /// <returns></returns>
        private static string GetUniquePath(string originalPath)
        {
            string path = Regex.Replace(originalPath, @"\[(.*?)\]", "");
            path = path.Replace("Array.data.", "");

            return path;
        }
    }
}