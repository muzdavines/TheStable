using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    [CustomPropertyDrawer(typeof(PGGModCellPort))]
    public class PGGModCellPortDrawer : NodePort_DrawerBase
    {
        PGGModCellPort cllPrt = null; PGGModCellPort PlannerPort { get { if (cllPrt == null) cllPrt = port as PGGModCellPort; return cllPrt; } }

        protected override string InputTooltipText => "Mod Grid Cell " + base.InputTooltipText;
        protected override string OutputTooltipText => "Mod Grid Cell " + base.OutputTooltipText;
        int wdthBoost = 0;

        void RefreshContentName()
        {
            if (displayContent == null) return;
            if (PlannerPort == null) return;
            wdthBoost = 0;

            if (cllPrt.GetInputCellValue == null)
            {
                if (PlannerPort.ContainsMultipleCells)
                {
                    displayContent.text = baseContent.text + " (" + PlannerPort.CellsList.Count + ")";
                    wdthBoost = 22;
                    return;
                }

                if (!displayContent.text.EndsWith(")"))
                {
                    if (PlannerPort.ForcedNull)
                        displayContent.text = baseContent.text + " (null)";
                    else
                        displayContent.text = baseContent.text + " (self)";
                }
            }

            if (PlannerPort.IsInput)
            {
                if (PlannerPort.LimitInConnectionsCount != 1)
                    if (cllPrt.ConnectedWithMultipleCells)
                    {
                        displayContent.text = baseContent.text + " (" + cllPrt.CountAllCellsInAllConnections + ")";
                        wdthBoost = 22;
                    }
            }
        }


        protected override void DrawLabel(Rect fieldRect)
        {
            RefreshContentName();
            if (wdthBoost > 0) fieldRect.width += wdthBoost;
            base.DrawLabel(fieldRect);
        }

        protected override void DrawValueFieldNoEditable(Rect fieldRect)
        { }

        protected override void DrawValueField(Rect fieldRect)
        { }

        protected override void DrawValueWithLabelField(Rect labelRect, Rect fieldRect, Rect bothRect)
        {
            RefreshContentName();
            base.DrawLabel(bothRect);
        }

    }

}
