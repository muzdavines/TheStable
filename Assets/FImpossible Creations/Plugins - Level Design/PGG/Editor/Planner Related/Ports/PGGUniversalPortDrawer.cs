using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    [CustomPropertyDrawer(typeof(PGGUniversalPort))]
    public class PGGUniversalPort_Drawer : NodePort_DrawerBase
    {
        PGGUniversalPort plPrt = null; PGGUniversalPort PlannerPort { get { if (plPrt == null) plPrt = port as PGGUniversalPort; return plPrt; } }

        protected override string InputTooltipText => "Universal port - pin here any port, but beware for type errors";
        protected override string OutputTooltipText => "Universal port - pin here any port, but beware for type errors";

        protected override void DrawValueField(Rect fieldRect)
        {
            if (PlannerPort.Variable.ValueType == Generating.FieldVariable.EVarType.Number)
            {
                PlannerPort.Variable.Float = EditorGUI.FloatField(fieldRect, GUIContent.none, PlannerPort.Variable.Float);
            }
            else if (PlannerPort.Variable.ValueType == Generating.FieldVariable.EVarType.Vector2)
            {
                PlannerPort.Variable.SetValue(EditorGUI.Vector2Field(fieldRect, GUIContent.none, PlannerPort.Variable.GetVector2Value()));
            }
            else if (PlannerPort.Variable.ValueType == Generating.FieldVariable.EVarType.Vector3)
            {
                PlannerPort.Variable.SetValue(EditorGUI.Vector3Field(fieldRect, GUIContent.none, PlannerPort.Variable.GetVector3Value()));
            }
            else if (PlannerPort.Variable.ValueType == Generating.FieldVariable.EVarType.String)
            {
                PlannerPort.Variable.SetValue(EditorGUI.TextField(fieldRect, GUIContent.none, PlannerPort.Variable.GetStringValue()));
            }
        }

        protected override void DrawValueWithLabelField(Rect labelRect, Rect fieldRect, Rect bothRect)
        {
            if (PlannerPort.Variable.ValueType == Generating.FieldVariable.EVarType.Number)
            {
                PlannerPort.Variable.Float = EditorGUI.FloatField(bothRect, displayContent, PlannerPort.Variable.Float);
            }
            else if (PlannerPort.Variable.ValueType == Generating.FieldVariable.EVarType.Vector2)
            {
                PlannerPort.Variable.SetValue(EditorGUI.Vector2Field(bothRect, displayContent, PlannerPort.Variable.GetVector2Value()));
            }
            else if (PlannerPort.Variable.ValueType == Generating.FieldVariable.EVarType.Vector3)
            {
                PlannerPort.Variable.SetValue(EditorGUI.Vector3Field(bothRect, displayContent, PlannerPort.Variable.GetVector3Value()));
            }
            else
            {
                DrawLabel(labelRect);
            }
        }

    }
}
