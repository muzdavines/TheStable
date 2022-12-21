
using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Generating
{
    [CustomPropertyDrawer(typeof(FieldSetup))]
    public class FieldSetup_PropertyDrawer : PropertyDrawer
    {
        //bool triedGet = false;
        FieldSetup gettedSetup = null;
        int checkDelay = 0;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            Rect noButtonRect = new Rect(position);
            noButtonRect.width -= 24;
            EditorGUI.PropertyField(noButtonRect, property, GUIContent.none);

            Rect buttonRect = new Rect(noButtonRect.position, new Vector2(20, 19));
            buttonRect.x += noButtonRect.width;

            if (property != null)
                if (property.serializedObject != null)
                {
                    checkDelay -= 1;
                    if (checkDelay <= 0) gettedSetup = property.GetValue<FieldSetup>();

                    if (gettedSetup)
                    {
                        if (GUI.Button(buttonRect, "→"))
                        {
                            FieldDesignWindow.OpenFieldSetupFileInWindow(gettedSetup);

                            if (FieldDesignWindow.Get)
                            {
                                FieldDesignWindow.Get.AutoRefreshPreview = false;
                            }
                        }
                    }
                    else
                    {
                        checkDelay = 10;
                    }
                }

            EditorGUI.EndProperty();
        }
    }
}