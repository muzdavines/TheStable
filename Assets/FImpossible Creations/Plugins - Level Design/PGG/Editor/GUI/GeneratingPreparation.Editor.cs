using UnityEditor;
using UnityEngine;
using FIMSpace.FEditor;

namespace FIMSpace.Generating
{
    [CustomPropertyDrawer(typeof(GeneratingPreparation))]
    public class GeneratingPreparationDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 1;
        }

        GeneratingPreparation Get = null;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int preI = EditorGUI.indentLevel;

            if (EditorGUI.indentLevel > 1) EditorGUI.indentLevel = 1;


            #region Getting reference to variable

            if (Get == null)
            {
                GeneratingPreparation helper = null;
                var targetObject = property.serializedObject.targetObject;
                var targetObjectClassType = targetObject.GetType();
                var field = targetObjectClassType.GetField(property.propertyPath);
                if (field != null) helper = (GeneratingPreparation)field.GetValue(targetObject);
                Get = helper;
            }

            if (Get == null)
            {
                EditorGUILayout.HelpBox("Can't find preparation settings! It shouldn't happen!", MessageType.Warning);
                return;
            }

            #endregion


            EditorGUI.BeginProperty(position, label, property);

            GeneratingPreparation.DrawPreparation(Get, property);

            EditorGUI.EndProperty();
            EditorGUI.indentLevel = preI;
        }

    }
}