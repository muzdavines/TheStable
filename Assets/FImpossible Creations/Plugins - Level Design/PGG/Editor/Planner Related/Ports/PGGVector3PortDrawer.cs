using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    [CustomPropertyDrawer(typeof(PGGVector3Port))]
    public class Vector3Port_Drawer : NodePort_DrawerBase
    {
        PGGVector3Port v3P = null; PGGVector3Port V3Port { get { if (v3P == null) v3P = port as PGGVector3Port; return v3P; } }

        protected override string InputTooltipText => "Vector3 " + base.InputTooltipText;
        protected override string OutputTooltipText => "Vector3 " + base.OutputTooltipText;

        protected override void DrawValueField(Rect fieldRect)
        {
            V3Port.Value = EditorGUI.Vector3Field(fieldRect, GUIContent.none, V3Port.Value);
        }
        protected override void DrawValueWithLabelField(Rect labelRect, Rect fieldRect, Rect bothRect)
        {
            V3Port.Value = EditorGUI.Vector3Field(bothRect, displayContent, V3Port.Value);
        }

    }

}
