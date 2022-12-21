using UnityEngine;
using UnityEditor;
using FIMSpace.Generating;

namespace FIMSpace.Graph
{

    [CustomPropertyDrawer(typeof(IntPort))]
    public class IntPort_Drawer : NodePort_DrawerBase
    {
        // Utility Get Port Instance -------------
        IntPort intP = null; IntPort intPort { get { if (intP == null) intP = port as IntPort; return intP; } }

        protected override string InputTooltipText => "Int " + base.InputTooltipText;
        protected override string OutputTooltipText => "Int " + base.OutputTooltipText;


        // Int draw ----------------------

        protected override void DrawLabel(Rect fieldRect)
        {
            base.DrawLabel(fieldRect);
        }

        protected override void DrawValueField(Rect fieldRect)
        {
            intPort.Value = EditorGUI.IntField(fieldRect, GUIContent.none, intPort.Value);
        }

        protected override void DrawValueFieldNoEditable(Rect fieldRect)
        {
            base.DrawValueFieldNoEditable(fieldRect);
        }

        protected override void DrawValueWithLabelField(Rect labelRect, Rect fieldRect, Rect bothRect)
        {
            intPort.Value = EditorGUI.IntField(bothRect, displayContent, intPort.Value);
        }

    }


    [CustomPropertyDrawer(typeof(FloatPort))]
    public class FloatPort_Drawer : NodePort_DrawerBase
    {
        FloatPort floatP = null; FloatPort floatPort { get { if (floatP == null) floatP = port as FloatPort; return floatP; } }

        protected override string InputTooltipText => "Float " + base.InputTooltipText;
        protected override string OutputTooltipText => "Float " + base.OutputTooltipText;

        protected override void DrawValueField(Rect fieldRect)
        {
            floatPort.Value = EditorGUI.FloatField(fieldRect, GUIContent.none, floatPort.Value);
        }

        protected override void DrawValueFieldNoEditable(Rect fieldRect)
        {
            object val = port.GetPortValue;
            if (FGenerators.CheckIfExist_NOTNULL(val))
            {
                if (val is float)
                {
                    float fVal = (float)val;
                    EditorGUI.LabelField(fieldRect, System.Math.Round(fVal, 1).ToString(), FGraphStyles.BGInBoxStyle);
                }
                else
                    EditorGUI.LabelField(fieldRect, val.ToString(), FGraphStyles.BGInBoxStyle);
            }
            else
                EditorGUI.LabelField(fieldRect, "Can't read value", FGraphStyles.BGInBoxStyle);
        }

        protected override void DrawValueWithLabelField(Rect labelRect, Rect fieldRect, Rect bothRect)
        {
            floatPort.Value = EditorGUI.FloatField(bothRect, displayContent, floatPort.Value);
        }

    }


    [CustomPropertyDrawer(typeof(BoolPort))]
    public class BoolPort_Drawer : NodePort_DrawerBase
    {
        BoolPort boolP = null; BoolPort boolPort { get { if (boolP == null) boolP = port as BoolPort; return boolP; } }

        protected override string InputTooltipText => "Bool " + base.InputTooltipText;
        protected override string OutputTooltipText => "Bool " + base.OutputTooltipText;

        protected override void DrawValueField(Rect fieldRect)
        {
            boolPort.Value = EditorGUI.Toggle(fieldRect, GUIContent.none, boolPort.Value);
        }

        protected override void DrawValueWithLabelField(Rect labelRect, Rect fieldRect, Rect bothRect)
        {
            boolPort.Value = EditorGUI.Toggle(bothRect, displayContent, boolPort.Value);
        }

    }

}