using FIMSpace.Generating;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    //[CustomPropertyDrawer(typeof(IntPort))] !!! Needed to be added on derived classes
    public partial class NodePort_DrawerBase : PropertyDrawer
    {

        // Overridables -----------------------------------
        protected virtual string InputTooltipText { get { return "Input Port " + port.PortID; } }
        protected virtual string OutputTooltipText { get { return "Output Port " + port.PortID; } }



        // Drawing Value Overridables ----------------------------

        protected virtual void DrawLabel(Rect fieldRect)
        {
            EditorGUI.LabelField(fieldRect, displayContent);
        }

        protected virtual void DrawValueFieldNoEditable(Rect fieldRect)
        {
            object val = port.GetPortValue;
            if (FGenerators.CheckIfExist_NOTNULL(val))
                EditorGUI.LabelField(fieldRect, val.ToString(), FGraphStyles.BGInBoxStyle);
            else
                EditorGUI.LabelField(fieldRect, "Can't read value" , FGraphStyles.BGInBoxStyle);
        }

        protected virtual void DrawValueWithLabelField(Rect labelRect, Rect fieldRect, Rect bothRect)
        {
            float preW = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = labelRect.width;
            DrawLabel(labelRect);
            DrawValueField(fieldRect);
            EditorGUIUtility.labelWidth = preW;
        }

        protected virtual void DrawValueField(Rect fieldRect)
        {
            //intPort.Value = EditorGUI.IntField(fieldRect, GUIContent.none, intPort.Value);
        }

    }

}
