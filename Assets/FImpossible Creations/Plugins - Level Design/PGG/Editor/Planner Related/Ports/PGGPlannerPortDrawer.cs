using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    [CustomPropertyDrawer(typeof(PGGPlannerPort))]
    public class PGGPlannerPort_Drawer : NodePort_DrawerBase
    {
        PGGPlannerPort plPrt = null; PGGPlannerPort PlannerPort { get { if (plPrt == null) plPrt = port as PGGPlannerPort; return plPrt; } }

        protected override string InputTooltipText => "Field Planner " + base.InputTooltipText + "\n(Using self planner if not connected)";
        protected override string OutputTooltipText => "Field Planner " + base.OutputTooltipText + "\n(Using self planner if not connected)";
        private GUIContent labelDispl = new GUIContent();

        protected override void DrawLabel(Rect fieldRect)
        {
            string prefix = "";
            if (PlannerPort.DisplayVariableName) prefix = displayContent.text + " ";
            string infoTxt = "(self)";
            if (plPrt.JustCheckerContainer) infoTxt = "(shape)";

            if (port.PortState() == EPortPinState.Connected)
            {

                if (PlannerPort != null)
                {
                    PGGPlannerPort conn = null;

                    if (PlannerPort.BaseConnection != null)
                        if (PlannerPort.BaseConnection.PortReference != null)
                            if (PlannerPort.BaseConnection.PortReference is PGGPlannerPort)
                                conn = PlannerPort.BaseConnection.PortReference as PGGPlannerPort;

                    int plannerId = PlannerPort.UniquePlannerID;
                    int duplId = PlannerPort.DuplicatePlannerID;

                    bool mulDraw = false;
                    if (conn != null)
                    {
                        if (conn.ContainsMultiplePlanners)
                        {
                            labelDispl.text = prefix + "Multiple (" + conn.PlannersList.Count + ")";
                            mulDraw = true;
                        }
                    }

                    if (!mulDraw)
                    {
                        if (!PlannerPort.HasShape)
                        {
                            if (PlannerPort.ContainsMultiplePlanners && plPrt.PlannersList.Count > 1)
                            {
                                //if (plPrt.PlannersList.Count == 1)
                                //    labelDispl.text = prefix + "| " + plPrt.PlannersList[0].ArrayNameString;
                                //else
                                    labelDispl.text = prefix + "| Multiple (" + plPrt.PlannersList.Count + ")";
                            }
                            else
                            {
                                if (duplId >= 0)
                                    labelDispl.text = prefix + "[" + plannerId + ", " + duplId + "]";
                                else
                                {
                                    if (plannerId < 0)
                                    {
                                        labelDispl.text = prefix + (port.IsOutput ? "" : infoTxt);
                                    }
                                    else
                                        labelDispl.text = prefix + "[" + plannerId + "]";
                                }
                            }
                        }
                        else
                        {
                            labelDispl.text = prefix + "(SHAPE)";
                        }
                    }

                    labelDispl.tooltip = displayContent.tooltip;

                    displayContent.text = labelDispl.text;
                    fieldRect.width = DisplayContentWidth() + 10;

                    SetLabelWidth();
                    EditorGUI.LabelField(fieldRect, labelDispl);
                    RestoreLabelWidth();
                }
                else
                {
                    SetLabelWidth();
                    base.DrawLabel(fieldRect);
                    RestoreLabelWidth();
                }
            }
            else
            {
                SetLabelWidth();

                if (PlannerPort.HasShape)
                    EditorGUI.LabelField(fieldRect, prefix + "(SHAPE)");
                else
                    EditorGUI.LabelField(fieldRect, prefix + (port.IsOutput ? "" : infoTxt));

                RestoreLabelWidth();
            }
        }

        protected override void DrawValueField(Rect fieldRect)
        {
            PlannerPort.UniquePlannerID = EditorGUI.IntField(fieldRect, GUIContent.none, PlannerPort.UniquePlannerID);
        }

        protected override void DrawValueFieldNoEditable(Rect fieldRect)
        {
        }


        protected override void DrawValueWithLabelField(Rect labelRect, Rect fieldRect, Rect bothRect)
        {
            if (port.PortState() == EPortPinState.Connected)
            {
                SetLabelWidth();
                PlannerPort.UniquePlannerID = EditorGUI.IntField(bothRect, displayContent, PlannerPort.UniquePlannerID);
                RestoreLabelWidth();
            }
            else
            {
                string prefix = "";
                if (PlannerPort.DisplayVariableName) prefix = displayContent.text + " ";

                if (PlannerPort.ContainsMultiplePlanners)
                {
                    labelDispl.text = prefix + "Multiple (" + plPrt.PlannersList.Count + ")";
                }
                else
                {
                    if (PlannerPort.HasShape == false)
                    {
                        if (PlannerPort.UniquePlannerID < 0)
                        {
                            string infoTxt = "(self)";
                            if (plPrt.JustCheckerContainer) infoTxt = "(shape)";
                            displayContent.text = prefix + (port.IsOutput ? "" : infoTxt);
                        }
                        else
                            displayContent.text = prefix + "[" + PlannerPort.UniquePlannerID + "]";
                    }
                    else
                    {
                        displayContent.text = prefix + "(SHAPE)";
                    }
                }

                SetLabelWidth();
                PlannerPort.UniquePlannerID = EditorGUI.IntField(bothRect, displayContent, PlannerPort.UniquePlannerID);
                RestoreLabelWidth();
            }
        }

    }

}
