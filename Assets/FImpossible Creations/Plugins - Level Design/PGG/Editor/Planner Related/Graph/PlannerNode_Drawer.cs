using FIMSpace.FEditor;
using FIMSpace.Generating.Planner.Nodes;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    public class PlannerNode_Drawer : FGraph_NodeBase_Drawer
    {
        PGGPlanner_NodeBase get;
        public PlannerNode_Drawer(FGraph_NodeBase inst) : base(inst)
        {
            get = inst as PGGPlanner_NodeBase;
        }


        public override bool SelectNodeInspector { get { return get.DrawInspector; } }

        public override EConnectorsWireMode ConnectorsWiresMode => EConnectorsWireMode.Up_Down;

        protected override void DrawNodeFrameHeader(Rect bodyRect)
        {
            Rect nodeHeader = new Rect(bodyRect);
            nodeHeader.width -= 10;
            nodeHeader.height = 32;
            nodeHeader.position += new Vector2(6, 8);

            GUI.backgroundColor = get.GetNodeColor();
            GUI.Box(nodeHeader, new GUIContent(baseGet.GetDisplayName(), baseGet.GetNodeIcon, baseGet.GetNodeTooltipDescription), GetFrameHeaderStyle);
            GUI.backgroundColor = Color.white;
        }

        protected override void DrawNodeFrameBody(Rect bodyRect)
        {
            GUI.backgroundColor = new Color(0.45f, 0.45f, 0.45f, 0.8f);
            base.DrawNodeFrameBody(bodyRect);
            GUI.backgroundColor = Color.white;
        }

        public override Rect GetBaseConnectorInputClickAreaRect()
        {
            Rect r = new Rect(_E_LatestInputRect);
            r.size = new Vector2(r.size.x * 1.5f, r.size.y * 1.3f);
            r.position -= new Vector2(14, 10);
            return r;
        }

        public override Rect GetBaseConnectorOutputClickAreaRect()
        {
            Rect r = new Rect(_E_LatestOutputRect);
            r.size = new Vector2(r.size.x * 1.5f, r.size.y * 1.5f);
            r.position -= new Vector2(14, 4);
            return r;
        }

        public override Rect GetBaseConnectorOutputClickAreaRect(int multipleImplementedID)
        {
            Rect r = new Rect(GetBaseConnectorOutputRect(baseGet._E_LatestRect, multipleImplementedID));
            r.size = new Vector2(r.size.x * 1.5f, r.size.y * 1.5f);
            r.position -= new Vector2(14, 4);
            return r;
        }


        public override Vector2 GetInputConnectorPinPosition(int multipleConnectorsIDHelper)
        {
            Vector2 pin = _E_LatestInputRect.center;
            pin.y -= 2;
            return pin;
        }


        public override Vector2 GetOutputConnectorPinPosition(int multipleConnectorsIDHelper)
        {
            Vector2 pin;
            if (multipleConnectorsIDHelper >= 0)
            {
                pin = GetBaseConnectorOutputClickAreaRect(multipleConnectorsIDHelper).center;
                pin.y += 2;
            }
            else
            {
                pin = _E_LatestOutputRect.center;
                pin.y += 2;
            }

            return pin;
        }


        public override Rect GetBaseConnectorInputRect(Rect referenceRect)
        {
            Rect r = new Rect(referenceRect);
            r.size = new Vector2(52, 20);
            //r.position += new Vector2(40, 2);
            //r.position += new Vector2(10, 2);
            //r.position += new Vector2(26 + referenceRect.width / 2f - r.size.x / 2f, 2);
            r.position += new Vector2(referenceRect.width / 2f - r.size.x / 2f, 2);
            _E_LatestInputRect = r;
            return r;
        }

        public override Rect GetBaseConnectorOutputRect(Rect referenceRect)
        {
            Rect r = new Rect(referenceRect);
            r.size = new Vector2(52, 20);
            r.position += new Vector2(NodeSize.x / 2 - r.size.x / 2f, _E_BodyRect.height - 18);
            //r.position += new Vector2(NodeSize.x - r.size.x / 2f - 64, _E_BodyRect.height - 18);
            //r.position += new Vector2(NodeSize.x - r.size.x / 2f - 34, _E_BodyRect.height - 18);
            _E_LatestOutputRect = r;

            return r;
        }

        public override Rect GetBaseConnectorOutputRect(Rect referenceRect, int multipleImplementedID)
        {
            Rect baseR = GetBaseConnectorOutputRect(referenceRect);

            if (get)
            {
                float indx = (get.OutputConnectorsCount-1f) / (float)2f;
                float spreadWidth = (_E_BodyRect.width * 0.7f) / get.OutputConnectorsCount;

                Vector2 pos = baseR.position;
                pos.x = baseR.position.x + (-indx + multipleImplementedID ) * spreadWidth;

                baseR.position = pos;

                return baseR;
            }

            if (multipleImplementedID == 0)
            {
                baseR.position -= new Vector2(30, 0);
                return baseR;
            }
            else
            {
                baseR.position += new Vector2(30, 0);
                return baseR;
            }
        }


        public override Rect GetDragRect()
        {
            Rect r = new Rect(_E_BodyRect);
            r.position += new Vector2(11, 18);
            r.width -= 22;
            r.height = 50;

            return r;
        }

        public override bool InteractionAreaContainsCursor(Vector2 inGraphMousePos)
        {
            Rect r = GetDragRect();
            r.height = 28;
            if (r.Contains(inGraphMousePos)) return false;
            return base.InteractionAreaContainsCursor(inGraphMousePos);
        }

        public override Rect GetFrameBodyHighlightRect(Rect referenceRect)
        {
            Rect r = base.GetFrameBodyHighlightRect(referenceRect);
            r.position += new Vector2(2, 11);
            r.size -= new Vector2(3, 9);
            return r;
        }

        protected override GUIStyle GetFrameBodyStyle => PlannerGraphWindow.Styles.nodeBody;
        public override GUIStyle GetFrameBodyHighlightStyle => PlannerGraphWindow.Styles.bodyHighlight;
        protected override GUIStyle GetFrameHeaderStyle => PlannerGraphWindow.Styles.nodeHeader;


        protected override Texture GetLeftConnectorSprite => PlannerGraphWindow.PlannerGraphStyles.TEX_nodeInput;
        protected override Texture GetRightConnectorSprite => PlannerGraphWindow.PlannerGraphStyles.TEX_nodeOutput;

        public override Rect GetGuiBodyRect()
        {
            Rect r = base.GetGuiBodyRect();
            r.position += new Vector2(4, 6);
            r.size -= new Vector2(4, 4);
            return r;
        }


        protected override void DrawNodeFrameInputsAndOutputConnector(Rect referenceRect)
        {
            if (baseGet.DrawOutputConnector)
            {
                if (baseGet.OutputConnectorsCount > 1)
                {

                    for (int b = 0; b < baseGet.OutputConnectorsCount; b++)
                    {
                        Rect outputRect = GetBaseConnectorOutputRect(referenceRect, b);

                        Rect labelRect = new Rect(outputRect);
                        labelRect.position += new Vector2(0, -15);
                        GUI.Label(labelRect, baseGet.GetOutputHelperText(b), EditorStyles.centeredGreyMiniLabel);

                        GUI.DrawTexture(outputRect, GetRightConnectorSprite, ScaleMode.ScaleToFit);
                    }

                    //Rect outputRect = GetBaseConnectorOutputRect(referenceRect, 0);

                    //Rect labelRect = new Rect(outputRect);
                    //labelRect.position += new Vector2(0, -15);
                    //GUI.Label(labelRect, baseGet.GetOutputHelperText(0), EditorStyles.centeredGreyMiniLabel);

                    //GUI.DrawTexture(outputRect, GetRightConnectorSprite, ScaleMode.ScaleToFit);

                    //outputRect = GetBaseConnectorOutputRect(referenceRect, 1);

                    //labelRect = new Rect(outputRect);
                    //labelRect.position += new Vector2(0, -15);
                    //GUI.Label(labelRect, baseGet.GetOutputHelperText(1), EditorStyles.centeredGreyMiniLabel);

                    //GUI.DrawTexture(outputRect, GetRightConnectorSprite, ScaleMode.ScaleToFit);
                }
                else
                {
                    if (baseGet.AllowedOutputConnectionIndex > -1)
                    {
                        Rect outputRect = GetBaseConnectorOutputRect(referenceRect);
                        Rect labelRect = new Rect(outputRect);
                        labelRect.width += 60;
                        labelRect.position += new Vector2(-30, -15);
                        GUI.Label(labelRect, baseGet.GetOutputHelperText(baseGet.AllowedOutputConnectionIndex), EditorStyles.centeredGreyMiniLabel);
                        GUI.DrawTexture(outputRect, GetRightConnectorSprite, ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        Rect outputRect = GetBaseConnectorOutputRect(referenceRect);
                        GUI.DrawTexture(outputRect, GetRightConnectorSprite, ScaleMode.ScaleToFit);
                    }
                }
            }

            if (baseGet.DrawInputConnector)
            {
                Rect inputRect = GetBaseConnectorInputRect(referenceRect);
                GUI.DrawTexture(inputRect, GetLeftConnectorSprite, ScaleMode.ScaleToFit);
            }
        }

    }
}