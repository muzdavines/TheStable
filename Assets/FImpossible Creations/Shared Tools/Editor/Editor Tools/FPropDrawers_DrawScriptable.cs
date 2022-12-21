using UnityEditor;
using UnityEngine;

namespace FIMSpace.FEditor
{
    [CustomPropertyDrawer(typeof(FPropDrawers_DrawScriptableAttribute))]
    public class FPropDrawers_DrawScriptableDrawer : PropertyDrawer
    {
        bool showProperty = false;
        float DrawerHeight = 0;
        string button = "►";

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var e = Editor.CreateEditor(property.objectReferenceValue);
            var indent = EditorGUI.indentLevel;
            Rect temp = new Rect(position.x, position.y, 20, 16);

            if (GUI.Button(temp, button))
                if (showProperty)
                {
                    showProperty = false;
                    button = "►";
                }
                else
                {
                    showProperty = true;
                    button = "▼";
                }

            DrawerHeight = 0;
            position.height = 16;
            position.x += 20;
            position.width -= 20;
            EditorGUI.PropertyField(position, property);
            position.width += 20;
            position.x -= 20;
            position.y += 20;

            if (!showProperty) return;
            if (e != null)
            {
                position.x += 20;
                position.width -= 40;

                var so = e.serializedObject;
                so.Update();

                var prop = so.GetIterator();
                prop.NextVisible(true);

                int depthChilden = 0;
                bool showChilden = false;

                while (prop.NextVisible(true))
                {
                    if (prop.depth == 0) { showChilden = false; depthChilden = 0; }

                    if (showChilden && prop.depth > depthChilden)
                    {
                        continue;
                    }

                    position.height = 16;
                    EditorGUI.indentLevel = indent + prop.depth;
                    if (EditorGUI.PropertyField(position, prop))
                    {
                        showChilden = false;
                    }
                    else
                    {
                        showChilden = true;
                        depthChilden = prop.depth;
                    }

                    position.y += 20;
                    SetDrawerHeight(20);
                }

                if (GUI.changed)
                {
                    so.ApplyModifiedProperties();
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = base.GetPropertyHeight(property, label);
            height += DrawerHeight;
            return height;
        }

        void SetDrawerHeight(float height)
        {
            DrawerHeight += height;
        }
    }

    public class FPropDrawers_DrawScriptableAttribute : PropertyAttribute
    {

    }
}