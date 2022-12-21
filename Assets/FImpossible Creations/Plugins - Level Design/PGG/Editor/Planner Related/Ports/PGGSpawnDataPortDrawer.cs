using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    [CustomPropertyDrawer(typeof(PGGSpawnPort))]
    public class PGGSpawnDataPortDrawer : NodePort_DrawerBase
    {
        PGGSpawnPort cllPrt = null; PGGSpawnPort PlannerPort { get { if (cllPrt == null) cllPrt = port as PGGSpawnPort; return cllPrt; } }

        protected override string InputTooltipText => "SpawnData " + base.InputTooltipText;
        protected override string OutputTooltipText => "SpawnData " + base.OutputTooltipText;

        void RefreshContentName()
        {
            if (displayContent == null) return;
            if (PlannerPort == null) return;

            if (PlannerPort.ContainsMultipleSpawns)
            {
                displayContent = new GUIContent("Multiple (" + PlannerPort.GetLocalSpawnsList.Count+")", displayContent.image, displayContent.tooltip);
            }
            else
            {
                if (PlannerPort.GetInputCellValue == null)
                {
                    if (!displayContent.text.EndsWith(")"))
                    {
                        displayContent = new GUIContent(displayContent.text + " (self)", displayContent.image, displayContent.tooltip);
                    }
                }
            }
        }

        protected override void DrawLabel(Rect fieldRect)
        {
            RefreshContentName();
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
