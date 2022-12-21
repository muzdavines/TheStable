using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    [CustomPropertyDrawer(typeof(PGGCellPort))]
    public class PGGCellPortDrawer : NodePort_DrawerBase
    {
        PGGCellPort cllPrt = null; PGGCellPort PlannerPort { get { if (cllPrt == null) cllPrt = port as PGGCellPort; return cllPrt; } }

        protected override string InputTooltipText => "Cell " + base.InputTooltipText;
        protected override string OutputTooltipText => "Cell " + base.OutputTooltipText;

        protected override void DrawLabel(Rect fieldRect)
        {
            base.DrawLabel(fieldRect);
        }

        protected override void DrawValueFieldNoEditable(Rect fieldRect)
        { }

        protected override void DrawValueField(Rect fieldRect)
        { }

        protected override void DrawValueWithLabelField(Rect labelRect, Rect fieldRect, Rect bothRect)
        {
            base.DrawLabel(bothRect);
        }

    }

}
