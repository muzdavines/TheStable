using UnityEditor;
using UnityEngine;

namespace FIMSpace.FEditor
{
    [CustomPropertyDrawer(typeof(PGG_SingleLineSwitch))]
    public class PGG_SingleLineSwitchDrawer : PropertyDrawer
    {
        public static GUIStyle HeaderStyle { get { if (_headerStyle == null) { _headerStyle = new GUIStyle(EditorStyles.helpBox); _headerStyle.fontStyle = FontStyle.Bold; _headerStyle.alignment = TextAnchor.MiddleCenter; _headerStyle.fontSize = 11; } return _headerStyle; } }
        private static GUIStyle _headerStyle;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            PGG_SingleLineSwitch att = (PGG_SingleLineSwitch)base.attribute;
            return EditorGUIUtility.singleLineHeight + att.UpPadding;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PGG_SingleLineSwitch att = (PGG_SingleLineSwitch)base.attribute;

            EditorGUI.BeginProperty(position, label, property);

            position.y += att.UpPadding;
            position.height -= att.UpPadding;

            Rect mainProp = position;
            mainProp.width -= att.Width;
            
            if ( att.LabelWidth != 0 ) EditorGUIUtility.labelWidth = att.LabelWidth;
            EditorGUI.PrefixLabel(mainProp, label);
            EditorGUI.PropertyField(mainProp, property);
            EditorGUIUtility.labelWidth = 0;

            SerializedProperty sp = property.serializedObject.FindProperty(att.PropName);

            Rect secProp = position;
            secProp.x += position.width - att.Width;
            secProp.width = att.Width;

            if (sp != null)
            {
                EditorGUIUtility.labelWidth = 5;
                EditorGUI.PropertyField(secProp, sp, new GUIContent(" ", att.PropTooltip));
                EditorGUIUtility.labelWidth = 0;
            }
            else
                EditorGUI.LabelField(secProp, "No " + att.PropName + " variable!");

            if ( att.LabelWidth != 0 ) EditorGUIUtility.labelWidth = 0;

            EditorGUI.EndProperty();
        }
    }

}

