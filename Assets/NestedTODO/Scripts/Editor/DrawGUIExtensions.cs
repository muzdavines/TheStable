using UnityEngine;
using UnityEditor;
using System.Collections;

namespace NestedTODO
{
    public class DrawGUIExtensions : MonoBehaviour
    {
        public static Rect StartHorizontalColored(Color c, GUIStyle style = null)
        {
            var rect = new Rect();
            var guiColor = GUI.color;
            GUI.color = c;
            if (style == null)
                rect = EditorGUILayout.BeginHorizontal();
            else
                rect = EditorGUILayout.BeginHorizontal(style);
            GUI.color = guiColor;

            return rect;
        }
        
        public static void DrawFixedHorizontalSpace(float w)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(w));
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawExpandableHorizontalSpace()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(1), GUILayout.MaxWidth(1000), GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawFixedVerticalSpace(float h)
        {
            EditorGUILayout.BeginVertical(GUILayout.Height(h));
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        public static void DrawExpandableVerticalSpace()
        {
            EditorGUILayout.BeginVertical(GUILayout.MinHeight(1), GUILayout.MaxHeight(1000), GUILayout.ExpandHeight(true));
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }
        
        public static void DebugLastRect()
        {
            EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), new Color(1, 1, 1, .25f));
        }

        public static void DebugLastRect(Color c)
        {
            EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), c);
        }

        public static void DebugRect(Rect r)
        {
            EditorGUI.DrawRect(r, new Color(1, 1, 1, .25f));
        }

        public static void DebugRect(Rect r, Color c)
        {
            EditorGUI.DrawRect(r, c);
        }
    }
}
