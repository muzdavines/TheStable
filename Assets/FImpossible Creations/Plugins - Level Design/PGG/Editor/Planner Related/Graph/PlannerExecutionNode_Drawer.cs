using FIMSpace.FEditor;
using FIMSpace.Generating.Planner.Nodes;
using UnityEngine;

namespace FIMSpace.Graph
{
    public class PlannerExecutionNode_Drawer : PlannerNode_Drawer
    {
        PGGPlanner_NodeBase eget;

        public PlannerExecutionNode_Drawer(FGraph_NodeBase inst) : base(inst)
        {
            eget = inst as PGGPlanner_ExecutionNode;
        }


        protected override void DrawNodeFrameHeader(Rect bodyRect)
        {
            Rect nodeHeader = new Rect(bodyRect);
            nodeHeader.height -= 6;
            GUI.backgroundColor = eget.GetNodeColor();
            GUI.Box(nodeHeader, new GUIContent(eget.GetDisplayName()), GetFrameHeaderStyle);
            GUI.backgroundColor = Color.white;
        }

        protected override void DrawNodeFrameBody(Rect bodyRect)
        {
            Color bCol = eget.GetNodeColor() * 0.7f;
            bCol.a = 1f;
            GUI.backgroundColor = bCol;
            //bodyRect.position -= new Vector2(0, 7);
            //bodyRect.height += 6;

            bodyRect.position += new Vector2(11, 10);
            bodyRect.size -= new Vector2(22, 26);

            GUI.Box(bodyRect, GUIContent.none, FGUI_Resources.HeaderBoxStyleH);
            GUI.backgroundColor = Color.white;
        }

        public override Rect GetBaseConnectorOutputClickAreaRect()
        {
            Rect r = base.GetBaseConnectorOutputClickAreaRect();
            r.size = new Vector2(42, 68);
            r.position += new Vector2(20, 2);
            return r;
        }


        public override Rect GetFrameBodyHighlightRect(Rect referenceRect)
        {
            Rect r = base.GetFrameBodyHighlightRect(referenceRect);
            r.width -= 5;
            r.height -= 12;
            r.position += new Vector2(3, 4);
            return r;
        }

        public override Rect GetFrameBodyHighlightMultiSelectedRect(Rect referenceRect)
        {
            Rect r = base.GetFrameBodyHighlightMultiSelectedRect(referenceRect);
            r.position -= new Vector2(0, 4);
            r.height += 4;
            return r;
        }



        public override Rect GetDragRect()
        {
            return new Rect(NodePosition + new Vector2(0,10), NodeSize - new Vector2(0,12));
        }

        public override Vector2 GetOutputConnectorPinPosition(int multipleConnectorsIDHelper)
        {
            return base.GetOutputConnectorPinPosition(multipleConnectorsIDHelper) + new Vector2(0, 0);
        }


        public override bool InteractionAreaContainsCursor(Vector2 inGraphMousePos)
        {
            if (GetBaseConnectorOutputClickAreaRect().Contains(inGraphMousePos)) return true;
            return false;
            //return base.InteractionAreaContainsCursor(inGraphMousePos);
        }


        protected override void DrawNodeFrameInputsAndOutputConnector(Rect referenceRect)
        {
            if (baseGet.DrawOutputConnector)
            {
                if (eget.OutputConnections.Count == 1) GUI.color = new Color(1f, 1f, 1f, 0.5f);
                Rect outputRect = GetBaseConnectorOutputRect(referenceRect);
                GUI.DrawTexture(outputRect, GetRightConnectorSprite, ScaleMode.ScaleToFit);
                if (eget.OutputConnections.Count == 1) GUI.color = Color.white;
            }

            if (baseGet.DrawInputConnector)
            {
                Rect inputRect = GetBaseConnectorInputRect(referenceRect);
                GUI.DrawTexture(inputRect, GetLeftConnectorSprite, ScaleMode.ScaleToFit);
            }
        }

        public override Vector2 GetInputConnectorPinPosition(int multipleConnectorsIDHelper)
        {
            Vector2 r = base.GetInputConnectorPinPosition(multipleConnectorsIDHelper);
            r.y -= 6;
            return r;
        }

        public override Rect GetBaseConnectorInputClickAreaRect()
        {
            Rect r = base.GetBaseConnectorInputClickAreaRect();
            //r.size = new Vector2(40, 16);
            r.position -= new Vector2(0, 8);
            return r;
        }

        public override Rect GetBaseConnectorInputRect(Rect referenceRect)
        {
            Rect r = base.GetBaseConnectorInputRect(referenceRect);
            r.position -= new Vector2(0, 6);
            return r;
        }

        protected override GUIStyle GetFrameHeaderStyle => PlannerGraphWindow.Styles.nodeExBody;
        public override GUIStyle GetFrameBodyHighlightStyle => PlannerGraphWindow.Styles.nodeExBodyHighlight;
        //protected override GUIStyle GetFrameBodyStyle => FGraphStyles.DefaultStyles.nodeBody;

    }
}