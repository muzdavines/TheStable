using UnityEditor;
using UnityEngine;

namespace FIMSpace.FEditor
{
    [CustomPropertyDrawer(typeof(PGG_SingleLineTwoProperties))]
    public class PGG_SingleLineTwoPropertiesDrawer : PropertyDrawer
    {
        public static GUIStyle HeaderStyle { get { if (_headerStyle == null) { _headerStyle = new GUIStyle(EditorStyles.helpBox); _headerStyle.fontStyle = FontStyle.Bold; _headerStyle.alignment = TextAnchor.MiddleCenter; _headerStyle.fontSize = 11; } return _headerStyle; } }
        private static GUIStyle _headerStyle;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            PGG_SingleLineTwoProperties att = (PGG_SingleLineTwoProperties)base.attribute;
            return EditorGUIUtility.singleLineHeight + att.UpPadding;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PGG_SingleLineTwoProperties att = (PGG_SingleLineTwoProperties)base.attribute;

            EditorGUI.BeginProperty(position, label, property);

            position.y += att.UpPadding;
            position.height -= att.UpPadding;

            Rect mainProp = position;
            mainProp.width -= position.width / 2 + att.AddSecondPropWidth;

            if (att.LabelWidth != 0) EditorGUIUtility.labelWidth = att.LabelWidth;
            EditorGUI.PrefixLabel(mainProp, label);
            EditorGUI.PropertyField(mainProp, property);

            Rect secProp = mainProp;
            secProp.x += position.width / 2 + att.MiddlePadding - att.AddSecondPropWidth;
            secProp.width -= att.MiddlePadding;
            secProp.width += att.AddSecondPropWidth* 2;

            SerializedProperty sp = property.serializedObject.FindProperty(att.PropName);

            if (sp != null)
            {
                if (att.SecLabelWidth != 0) EditorGUIUtility.labelWidth = att.SecLabelWidth;
                EditorGUI.PropertyField(secProp, sp);
            }
            else
                EditorGUI.LabelField(secProp, "No " + att.PropName + " variable!");

            EditorGUIUtility.labelWidth = 0;

            EditorGUI.EndProperty();
        }
    }

}

