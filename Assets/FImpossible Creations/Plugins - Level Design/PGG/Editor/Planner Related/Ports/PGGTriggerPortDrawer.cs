using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    [CustomPropertyDrawer(typeof(PGGTriggerPort))]
    public class PGGTriggerPort_Drawer : NodePort_DrawerBase
    {
        PGGTriggerPort plPrt = null; PGGTriggerPort PlannerPort { get { if (plPrt == null) plPrt = port as PGGTriggerPort; return plPrt; } }

        protected override string InputTooltipText => "Execution Trigger Port for the Node";
        protected override string OutputTooltipText => "Execution Trigger for connected node";

        public override void DisplayPortGUI(bool setRectsRefs)
        {
            if (PlannerPort != null) PlannerPort.IsSendingSignals = true;
            base.DisplayPortGUI(setRectsRefs);
        }

        protected override void DrawValueFieldNoEditable(Rect fieldRect)
        {
        }

        protected override void DrawValueField(Rect fieldRect)
        { 
        }

        protected override void DrawValueWithLabelField(Rect labelRect, Rect fieldRect, Rect bothRect)
        {
            base.DrawLabel(labelRect);
        }

    }
}
