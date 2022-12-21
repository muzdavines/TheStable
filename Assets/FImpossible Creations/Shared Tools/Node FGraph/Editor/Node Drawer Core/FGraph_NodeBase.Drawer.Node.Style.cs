#if UNITY_EDITOR

// If UNITY_EDITOR for being able to define drawer in the main assembly,
// if there are just few gui elements to change, it's just more comfortable
// when creating new nodes and managing smaller ones

using UnityEngine;

namespace FIMSpace.Graph
{
    public partial class FGraph_NodeBase_Drawer
    {
        protected virtual GUIStyle GetFrameBodyStyle { get { return FGraphStyles.DefaultStyles.nodeBody; } }
        protected virtual GUIStyle GetFrameHeaderStyle { get { return FGraphStyles.DefaultStyles.nodeHeader; } }
        public virtual GUIStyle GetFrameBodyHighlightStyle { get { return FGraphStyles.DefaultStyles.bodyHighlight; } }
        protected virtual Texture GetLeftConnectorSprite { get { return FGraphStyles.TEX_DefNodeConnectorInput; } }
        protected virtual Texture GetRightConnectorSprite { get { return FGraphStyles.TEX_DefNodeConnectorOutput; } }
        public virtual EConnectorsWireMode ConnectorsWiresMode { get { return EConnectorsWireMode.Left_Right; } }


        #region Node Frame Display

        public virtual Rect DrawFullNode(Rect graphAreaRect)
        {
            Rect nodeRect = new Rect(NodePosition, NodeSize);

            ClampNodeFrameRectInWindow(ref nodeRect, graphAreaRect);

            Rect initRect = new Rect(nodeRect);

            Rect bodyRect = new Rect(nodeRect);
            bodyRect.position += Vector2.right * 9f;
            bodyRect.width -= 18;
            _E_BodyRect = bodyRect;

            DrawNodeFrameBody(bodyRect);
            DrawNodeFrameHeader(bodyRect);

            if (_E_SelectedNode == this) DrawNodeFrameSelectionHighlight(bodyRect);
            DrawNodeFrameInputsAndOutputConnector(initRect);

            return nodeRect;
        }



        protected virtual void DrawNodeFrameBody(Rect bodyRect)
        {
            GUI.Box(bodyRect, GUIContent.none, GetFrameBodyStyle);
        }


        protected virtual void DrawNodeFrameHeader(Rect bodyRect)
        {
            Rect nodeHeader = new Rect(bodyRect);
            nodeHeader.height = 32;
            nodeHeader.width -= 10;
            nodeHeader.position += Vector2.right * 5;

            GUI.backgroundColor = baseGet.GetNodeColor();
            GUI.Box(nodeHeader, new GUIContent(baseGet.GetDisplayName(), baseGet.GetNodeIcon, baseGet.GetNodeTooltipDescription), GetFrameHeaderStyle);
            GUI.backgroundColor = Color.white;

            _E_HeaderRect = nodeHeader;
        }


        protected virtual void DrawNodeFrameSelectionHighlight(Rect referenceRect)
        {
            GUI.color = new Color(1f, 0.75f, 0f, 0.7f);
            Rect highlightRect = GetFrameBodyHighlightRect(referenceRect);
            highlightRect.size += new Vector2(2, 8);
            highlightRect.position -= new Vector2(1, 12);
            GUI.Box(highlightRect, GUIContent.none, GetFrameBodyHighlightStyle);
            GUI.color = Color.white;
        }


        static GUIStyle _emptyGUIStyle = null;
        protected static GUIStyle emptyGUIStyle
        {
            get { if (_emptyGUIStyle == null) {_emptyGUIStyle = new GUIStyle(); _emptyGUIStyle.alignment = TextAnchor.MiddleCenter; } return _emptyGUIStyle; }
        }

        protected virtual void DrawNodeFrameInputsAndOutputConnector(Rect referenceRect)
        {
            if (baseGet.DrawOutputConnector)
            {
                Rect outputRect = GetBaseConnectorOutputRect(referenceRect);
                GUI.Label(outputRect, new GUIContent( GetRightConnectorSprite, "Output signal - it will send signal forward, to run action of connected node"), emptyGUIStyle);
                //GUI.DrawTexture(outputRect, GetRightConnectorSprite, ScaleMode.ScaleToFit);
            }

            if (baseGet.DrawInputConnector)
            {
                Rect inputRect = GetBaseConnectorInputRect(referenceRect);
                GUI.Label(inputRect, new GUIContent(GetLeftConnectorSprite, "Input for signal - other node must send signal to this input connector port, to run actions of this node"), emptyGUIStyle);
                //GUI.DrawTexture(inputRect, GetLeftConnectorSprite, ScaleMode.ScaleToFit);
            }
        }

        #endregion


    }

}
#endif
