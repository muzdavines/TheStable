using UnityEditor;
using UnityEngine;

namespace FIMSpace.FEditor
{
    [CustomPropertyDrawer(typeof(PGG_SingleLineSelector))]
    public class PGG_SingleLineSelectorDrawer : PropertyDrawer
    {
        public static GUIStyle HeaderStyle { get { if (_headerStyle == null) { _headerStyle = new GUIStyle(EditorStyles.helpBox); _headerStyle.fontStyle = FontStyle.Bold; _headerStyle.alignment = TextAnchor.MiddleCenter; _headerStyle.fontSize = 11; } return _headerStyle; } }
        private static GUIStyle _headerStyle;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            PGG_SingleLineSelector att = (PGG_SingleLineSelector)base.attribute;
            return EditorGUIUtility.singleLineHeight + att.UpPadding;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PGG_SingleLineSelector att = (PGG_SingleLineSelector)base.attribute;

            EditorGUI.BeginProperty(position, label, property);

            position.y += att.UpPadding;
            position.height -= att.UpPadding;

            Rect mainProp = position;
            mainProp.width -= att.Width;

            if (att.LabelWidth != 0) EditorGUIUtility.labelWidth = att.LabelWidth;
            EditorGUI.PrefixLabel(mainProp, label);
            EditorGUI.PropertyField(mainProp, property);
            EditorGUIUtility.labelWidth = 0;

            Rect secProp = position;
            secProp.x += position.width - att.Width;
            secProp.width = att.Width;

            if (property.intValue < att.PropNames.Length)
            {
                SerializedProperty sp = property.serializedObject.FindProperty(att.PropNames[property.intValue]);

                if (sp == null)
                {
                    // Try find in serialized's child
                    int dotIndex = property.propertyPath.IndexOf(".");
                    if (dotIndex > 1)
                    {
                        string parentProp = property.propertyPath.Substring(0, dotIndex);
                        sp = property.serializedObject.FindProperty(parentProp);
                        if (sp != null) sp = sp.FindPropertyRelative(att.PropNames[property.intValue]);
                    }
                }

                if (sp != null)
                {
                    EditorGUIUtility.labelWidth = 5;
                    EditorGUI.PropertyField(secProp, sp, new GUIContent(" ", att.PropTooltip));
                    EditorGUIUtility.labelWidth = 0;
                }
                else
                {
                    string log = "No property " + att.PropNames[property.intValue] + " variable found!";
                    EditorGUI.LabelField(secProp, new GUIContent(log, log));
                }
            }
            else
            {
                EditorGUI.LabelField(secProp, "Enum index too high! " + property.intValue + " / " + att.PropNames.Length);
            }

            if (att.LabelWidth != 0) EditorGUIUtility.labelWidth = 0;

            EditorGUI.EndProperty();
        }
    }

}

