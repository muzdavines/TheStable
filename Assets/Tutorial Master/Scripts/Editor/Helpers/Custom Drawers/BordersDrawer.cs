using UnityEditor;

using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI.Drawers
{
    /// <summary>
    /// Property drawer for rendering fields with Top, Bottom, Left and Right float fields.
    /// </summary>
    [CustomPropertyDrawer(typeof(Borders))]
    public sealed class BordersDrawer : PropertyDrawer
    {
        private const float VerticalDistanceBetweenFields = 4;

        private const float HorizontalDistanceBetweenFields = 2;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            EditorGUIUtility.labelWidth = 50;

            var topLeft = new Rect(
                position.x,
                position.y + 18,
                (position.width / 2) - HorizontalDistanceBetweenFields,
                16);
            var bottomLeft = new Rect(
                position.x,
                position.y + 32 + VerticalDistanceBetweenFields,
                (position.width / 2) - 2,
                16);

            var topRight = new Rect(
                position.x + (position.width / 2),
                position.y + 18,
                (position.width / 2) - HorizontalDistanceBetweenFields,
                16);
            var bottomRight = new Rect(
                position.x + (position.width / 2),
                position.y + 32 + VerticalDistanceBetweenFields,
                (position.width / 2) - 2,
                16);

            EditorGUI.PropertyField(topLeft, property.FindPropertyRelative("Top"));
            EditorGUI.PropertyField(bottomLeft, property.FindPropertyRelative("Bottom"));
            EditorGUI.PropertyField(topRight, property.FindPropertyRelative("Left"));
            EditorGUI.PropertyField(bottomRight, property.FindPropertyRelative("Right"));

            EditorGUIUtility.labelWidth = 0;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 55.0f;
        }
    }
}