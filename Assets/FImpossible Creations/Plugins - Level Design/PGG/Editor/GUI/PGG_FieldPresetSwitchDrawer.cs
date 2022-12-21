using FIMSpace.Generating;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FEditor
{
    [CustomPropertyDrawer(typeof(PGG_FieldPresetSwitch))]
    public class PGG_FieldPresetSwitchDrawer : PropertyDrawer
    {
        public static GUIStyle HeaderStyle { get { if (_headerStyle == null) { _headerStyle = new GUIStyle(EditorStyles.helpBox); _headerStyle.fontStyle = FontStyle.Bold; _headerStyle.alignment = TextAnchor.MiddleCenter; _headerStyle.fontSize = 11; } return _headerStyle; } }
        protected static GUIStyle _headerStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PGG_FieldPresetSwitch att = (PGG_FieldPresetSwitch)base.attribute;

            EditorGUI.BeginProperty(position, label, property);

            int switchWidth = 38;

            Rect mainProp = position;
            mainProp.width -= switchWidth+6;

            if ( att.LabelWidth != 0 ) EditorGUIUtility.labelWidth = att.LabelWidth;

            DrawProperty(mainProp, property);
            EditorGUIUtility.labelWidth = 0;

            SerializedProperty sp = property.serializedObject.FindProperty(att.PropName);

            Rect secProp = position;
            secProp.x += position.width - switchWidth;
            secProp.width = switchWidth;

            if (sp != null)
            {
                EditorGUIUtility.labelWidth = 18;
                sp.boolValue = GUI.Toggle(secProp, sp.boolValue, new GUIContent(PGGInspectorUtilities._Tex_FieldIcon, att.PropTooltip));
                EditorGUIUtility.labelWidth = 0;
            }
            else
                base.OnGUI(secProp, property, label);

            if (att.LabelWidth != 0) EditorGUIUtility.labelWidth = 0;

            EditorGUI.EndProperty();
        }

        protected virtual void DrawProperty(Rect position, SerializedProperty property)
        {
            EditorGUI.PropertyField(position, property);
        }
    }

    [CustomPropertyDrawer(typeof(PGG_FieldPresetSwitchRange))]
    public class PGG_FieldPresetSwitchRangeDrawer : PGG_FieldPresetSwitchDrawer
    {
        protected override void DrawProperty(Rect position, SerializedProperty property)
        {
            PGG_FieldPresetSwitchRange att = (PGG_FieldPresetSwitchRange)base.attribute;
            EditorGUI.Slider(position, property, att.From, att.To);
        }
    }

}

