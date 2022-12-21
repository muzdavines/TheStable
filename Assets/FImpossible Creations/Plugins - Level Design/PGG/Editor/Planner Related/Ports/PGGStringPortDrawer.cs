using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    [CustomPropertyDrawer(typeof(PGGStringPort))]
    public class PGGStringPort_Drawer : NodePort_DrawerBase
    {
        PGGStringPort plPrt = null; PGGStringPort PlannerPort { get { if (plPrt == null) plPrt = port as PGGStringPort; return plPrt; } }

        protected override string InputTooltipText => "String (Text) " + base.InputTooltipText;
        protected override string OutputTooltipText => "String (Text) " + base.OutputTooltipText;

        protected override void DrawValueField(Rect fieldRect)
        {
            PlannerPort.StringVal = EditorGUI.TextField(fieldRect, GUIContent.none, PlannerPort.StringVal);
        }

        protected override void DrawValueWithLabelField(Rect labelRect, Rect fieldRect, Rect bothRect)
        {
            PlannerPort.StringVal = EditorGUI.TextField(bothRect, displayContent, PlannerPort.StringVal);
        }

    }

}
