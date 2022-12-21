
using UnityEngine;

namespace FIMSpace.Graph
{
    public static partial class FGraphStyles
    {


        public static Texture2D TEX_freeInputShadowed { get { return _tex_freeInputSh != null ? _tex_freeInputSh : _tex_freeInputSh = Resources.Load<Texture2D>("ExtraUIPack/ESPR_InputV2.fw"); } }
        private static Texture2D _tex_freeInputSh;

        public static Texture2D TEX_Gradient1 { get { return _tex_grad1 != null ? _tex_grad1 : _tex_grad1 = Resources.Load<Texture2D>("ExtraUIPack/SPR_Grad1.fw"); } }
        private static Texture2D _tex_grad1;
        public static Texture2D TEX_Gradient2 { get { return _tex_grad2 != null ? _tex_grad2 : _tex_grad2 = Resources.Load<Texture2D>("ExtraUIPack/SPR_grad2.fw"); } }
        private static Texture2D _tex_grad2;
        public static Texture2D TEX_Gradient3 { get { return _tex_grad3 != null ? _tex_grad3 : _tex_grad3 = Resources.Load<Texture2D>("ExtraUIPack/SPR_grad3.fw"); } }
        private static Texture2D _tex_grad3;
        public static Texture2D TEX_Gradient4 { get { return _tex_grad4 != null ? _tex_grad4 : _tex_grad4 = Resources.Load<Texture2D>("ExtraUIPack/SPR_grad4.fw"); } }
        private static Texture2D _tex_grad4;



        public static Texture2D TEX_dragJack { get { return _tex_dragJack != null ? _tex_dragJack : _tex_dragJack = Resources.Load<Texture2D>("ExtraUIPack/ESPR_JackDrag"); } }
        private static Texture2D _tex_dragJack;

        public static Texture2D TEX_nodeBody { get { return _tex_nodeBody != null ? _tex_nodeBody : _tex_nodeBody = Resources.Load<Texture2D>("ExtraUIPack/ESPR_NodeBody.fw"); } }
        private static Texture2D _tex_nodeBody;
        public static Texture2D TEX_aNodeBody { get { return _tex_aNodeBody != null ? _tex_aNodeBody : _tex_aNodeBody = Resources.Load<Texture2D>("ExtraUIPack/ESPR_AudioNodeBody.fw"); } }
        private static Texture2D _tex_aNodeBody;

        public static Texture2D TEX_nodeHighlight { get { return _tex_nodeHighlight != null ? _tex_nodeHighlight : _tex_nodeHighlight = Resources.Load<Texture2D>("ExtraUIPack/ESPR_NodeHighlight.fw"); } }
        private static Texture2D _tex_nodeHighlight;

        public static Texture2D TEX_nodeSingleHighlight { get { return _tex_nodeSingleBodyHighlight != null ? _tex_nodeSingleBodyHighlight : _tex_nodeSingleBodyHighlight = Resources.Load<Texture2D>("ExtraUIPack/ESPR_AudioNodeSingleHighlight.fw"); } }
        private static Texture2D _tex_nodeSingleBodyHighlight;
        public static Texture2D TEX_nodeSingleV2Highlight { get { return _tex_nodeSingleV2BodyHighlight != null ? _tex_nodeSingleV2BodyHighlight : _tex_nodeSingleV2BodyHighlight = Resources.Load<Texture2D>("ExtraUIPack/ESPR_AudioNodeSingleV2Highlight.fw"); } }
        private static Texture2D _tex_nodeSingleV2BodyHighlight;

        public static Texture2D TEX_triggerHighlight { get { return _tex_triggerHighlight != null ? _tex_triggerHighlight : _tex_triggerHighlight = Resources.Load<Texture2D>("ExtraUIPack/ESPR_TriggerHeader.fw"); } }
        private static Texture2D _tex_triggerHighlight;

        public static Texture2D TEX_moduleTriggerHighlight { get { return _tex_moduleTriggerHighlight != null ? _tex_moduleTriggerHighlight : _tex_moduleTriggerHighlight = Resources.Load<Texture2D>("ExtraUIPack/ESPR_ModuleTrigger.fw"); } }

        private static Texture2D _tex_moduleTriggerHighlight;

        public static Texture2D TEX_outTriggerArrow { get { return _tex_outTriggerArrow != null ? _tex_outTriggerArrow : _tex_outTriggerArrow = Resources.Load<Texture2D>("ExtraUIPack/ESPR_TriggerOut.fw"); } }

        private static Texture2D _tex_outTriggerArrow;

        public static Texture2D TEX_moduleHighlight { get { return _tex_moduleHighlight != null ? _tex_moduleHighlight : _tex_moduleHighlight = Resources.Load<Texture2D>("ExtraUIPack/ESPR_ModuleHighlight.fw"); } }
        private static Texture2D _tex_moduleHighlight;

        public static Texture2D TEX_commentArea { get { return _tex_commentArea != null ? _tex_commentArea : _tex_commentArea = Resources.Load<Texture2D>("ExtraUIPack/ESPR_Comment.fw"); } }
        private static Texture2D _tex_commentArea;

        public static Texture2D TEX_nodeHeader { get { return _tex_nodeHeader != null ? _tex_nodeHeader : _tex_nodeHeader = Resources.Load<Texture2D>("ExtraUIPack/ESPR_NodeHeader.fw"); } }
        private static Texture2D _tex_nodeHeader;

        public static Texture2D TEX_audioNodeHeader { get { return _tex_audioNodeHeader != null ? _tex_audioNodeHeader : _tex_audioNodeHeader = Resources.Load<Texture2D>("ExtraUIPack/ESPR_AudioNodeHeader.fw"); } }
        private static Texture2D _tex_audioNodeHeader;
        public static Texture2D TEX_audioNodeHeaderL { get { return _tex_audioNodeHeaderL != null ? _tex_audioNodeHeaderL : _tex_audioNodeHeaderL = Resources.Load<Texture2D>("ExtraUIPack/ESPR_AudioNodeHeaderL.fw"); } }
        private static Texture2D _tex_audioNodeHeaderL;

        public static Texture2D TEX_moduleHeader { get { return _tex_moduleHeader != null ? _tex_moduleHeader : _tex_moduleHeader = Resources.Load<Texture2D>("ExtraUIPack/ESPR_ModuleHeader.fw"); } }
        private static Texture2D _tex_moduleHeader;

        public static Texture2D TEX_nodeSingle { get { return _tex_nodeSingle != null ? _tex_nodeSingle : _tex_nodeSingle = Resources.Load<Texture2D>("ExtraUIPack/ESPR_AudioNodeSingle.fw"); } }
        private static Texture2D _tex_nodeSingle;
        public static Texture2D TEX_nodeSingleV2 { get { return _tex_nodeSingleV2 != null ? _tex_nodeSingleV2 : _tex_nodeSingleV2 = Resources.Load<Texture2D>("ExtraUIPack/ESPR_AudioNodeSingleV2.fw"); } }
        private static Texture2D _tex_nodeSingleV2;
        public static Texture2D TEX_nodeSingleV3 { get { return _tex_nodeSingleV3 != null ? _tex_nodeSingleV3 : _tex_nodeSingleV3 = Resources.Load<Texture2D>("ExtraUIPack/ESPR_AudioNodeSingleV3.fw"); } }
        private static Texture2D _tex_nodeSingleV3;
        public static Texture2D TEX_nodeSingleV3Frame { get { return _tex_nodeSingleV3frame != null ? _tex_nodeSingleV3frame : _tex_nodeSingleV3frame = Resources.Load<Texture2D>("ExtraUIPack/ESPR_AudioNodeSingleV3Frame.fw"); } }
        private static Texture2D _tex_nodeSingleV3frame;
        public static Texture2D TEX_nodeSingleV3Body { get { return _tex_nodeSingleV3body != null ? _tex_nodeSingleV3body : _tex_nodeSingleV3body = Resources.Load<Texture2D>("ExtraUIPack/ESPR_AudioNodeSingleV3Body.fw"); } }
        private static Texture2D _tex_nodeSingleV3body;
        public static Texture2D TEX_nodeSingleV3Header { get { return _tex_nodeSingleV3header != null ? _tex_nodeSingleV3header : _tex_nodeSingleV3header = Resources.Load<Texture2D>("ExtraUIPack/ESPR_AudioNodeHeaderV3.fw"); } }
        private static Texture2D _tex_nodeSingleV3header;

        public static Texture2D TEX_moduleBody { get { return _tex_moduleBody != null ? _tex_moduleBody : _tex_moduleBody = Resources.Load<Texture2D>("ExtraUIPack/ESPR_ModuleBody.fw"); } }
        private static Texture2D _tex_moduleBody;


        public static Texture2D TEX_moduleAddButton { get { return _tex_moduleAddButton != null ? _tex_moduleAddButton : _tex_moduleAddButton = Resources.Load<Texture2D>("ExtraUIPack/ESPR_AddModule.fw"); } }
        private static Texture2D _tex_moduleAddButton;



        public static Texture2D TEX_moduleRemoveButtonNormal { get { return _tex_mrbn != null ? _tex_mrbn : _tex_mrbn = Resources.Load<Texture2D>("ExtraUIPack/ESPR_SmallButtonNormal.fw"); } }
        private static Texture2D _tex_mrbn;
        public static Texture2D TEX_moduleRemoveButtonHover { get { return _tex_mrbh != null ? _tex_mrbh : _tex_mrbh = Resources.Load<Texture2D>("ExtraUIPack/ESPR_SmallButtonHover.fw"); } }
        private static Texture2D _tex_mrbh;
        public static Texture2D TEX_moduleRemoveButtonPress { get { return _tex_mrbp != null ? _tex_mrbp : _tex_mrbp = Resources.Load<Texture2D>("ExtraUIPack/ESPR_SmallButtonPress.fw"); } }
        private static Texture2D _tex_mrbp;


        private static readonly Color defaultTextColor = new Color(0.85f, 0.85f, 0.85f, 0.85f);

        // Styles
        public static Styles GStyles { get { return _styles != null ? _styles : _styles = new Styles(); } }
        private static Styles _styles = null;
       
#if UNITY_EDITOR
        public static GUIStyle Label { get { return UnityEditor.EditorStyles.label; } }
        public static GUIStyle BLabel { get { return UnityEditor.EditorStyles.boldLabel; } }
#else
        public static GUIStyle Label { get { GUIStyle g = new GUIStyle(/**/) { alignment = TextAnchor.UpperLeft }; g.normal.textColor = Color.white; return g; } }
        public static GUIStyle BLabel { get { return new GUIStyle(Label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperRight }; } }
#endif


        public class Styles
        {
            public GUIStyle nodeHeader, nodeHeaderFrame, nodeHeaderFrameL, nodeBodySingle, nodeBodySingleV2, nodeBodySingleV3,
                            nodeBodySingleV3Frame, nodeBodySingleV3Body, nodeBodySingleV3Header,
                            nodeBodySingleHighlight, nodeBodySingleHighlightV2, aNodeHeader;

            public GUIStyle nodeBodyText, nodeBodyTextConnectorsOffset;

            public GUIStyle nodeBody, audioNodeBody, nodeHighlight;

            public GUIStyle moduleButton, moduleRemoveButton, moduleBody, moduleHighlight,
                            moduleHeader, moduleHeaderTextToLeft;

            public GUIStyle audioLabel, commentBody, commentArea, middleHeader, valueInfo;

            public GUIStyle inputPort, tooltip, icon;

            public Styles()
            {
                GUIStyle baseStyle = new GUIStyle("Label");
                baseStyle.fixedHeight = 24;

                inputPort = new GUIStyle(baseStyle);
                inputPort.alignment = TextAnchor.UpperLeft;
                inputPort.padding.left = 10;
                inputPort.fontSize = 12;


                nodeHeader = new GUIStyle();
                nodeHeader.fixedHeight = 44;
                nodeHeader.padding = new RectOffset(0, 4, 0, 10);

                nodeHeader.alignment = TextAnchor.MiddleCenter;
                nodeHeader.fontSize = 14;
                nodeHeader.fontStyle = FontStyle.Bold;

                nodeHeader.normal.textColor = new Color(.85f, .85f, .85f, 1f);


                aNodeHeader = new GUIStyle(nodeHeader);
                aNodeHeader.padding = new RectOffset(0, 18, 0, 10);

                middleHeader = new GUIStyle(Label);
                middleHeader.alignment = TextAnchor.MiddleCenter;

                nodeBodyText = new GUIStyle();
                nodeBodyText.border = new RectOffset(32, 32, 32, 32);
                nodeBodyText.padding = new RectOffset(24, 24, 8, 14);
                nodeBodyText.margin = new RectOffset(4, 4, 1, 0);
                nodeBodyText.fontSize = 13;

                nodeBodyTextConnectorsOffset = new GUIStyle(nodeBodyText);
                nodeBodyTextConnectorsOffset.border = new RectOffset(32, 42, 32, 32);

                nodeBody = new GUIStyle(nodeBodyText);
                nodeBody.normal.background = FGraphStyles.TEX_nodeBody;

                nodeBodySingle = new GUIStyle(nodeBody);
                nodeBodySingle.border = new RectOffset(16, 16, 16, 16);
                //nodeBodySingle.padding = new RectOffset(24, 24, 8, 14);
                nodeBodySingle.alignment = TextAnchor.MiddleCenter;
                nodeBodySingle.fontSize = 14;
                nodeBodySingle.fontStyle = FontStyle.Bold;
                nodeBodySingle.normal.textColor = new Color(.85f, .85f, .85f, 1f);

                nodeBodySingle.normal.background = FGraphStyles.TEX_nodeSingle;
                //NodeEditorResources.styles.tooltip = tooltip;

                nodeBodySingleV2 = new GUIStyle(nodeBodySingle);
                nodeBodySingleV2.normal.background = FGraphStyles.TEX_nodeSingleV2;


                audioNodeBody = new GUIStyle(nodeBody);
                audioNodeBody.normal.background = FGraphStyles.TEX_aNodeBody;

                commentBody = new GUIStyle(nodeBody);
                commentBody.padding = new RectOffset(24, 24, 24, 12);

                commentArea = new GUIStyle(commentBody);
                commentArea.normal.background = FGraphStyles.TEX_commentArea;

                moduleHeader = new GUIStyle(nodeHeader);
                moduleHeader.padding = new RectOffset(12, 12, 4, 4);
                moduleHeader.fontSize = 13;
                moduleHeader.alignment = TextAnchor.MiddleCenter;
                moduleHeader.fixedHeight = 32;
                moduleHeader.normal.background = FGraphStyles.TEX_moduleHeader;

                moduleHeaderTextToLeft = new GUIStyle(moduleHeader);
                moduleHeaderTextToLeft.alignment = TextAnchor.MiddleLeft;

                moduleBody = new GUIStyle(nodeBody);
                moduleBody.normal.background = FGraphStyles.TEX_moduleBody;
                moduleBody.padding = new RectOffset(38, 26, 0, 14);
                moduleBody.border = new RectOffset(32, 32, 32, 32);

                nodeHighlight = new GUIStyle();
                nodeHighlight.normal.background = FGraphStyles.TEX_nodeHighlight;
                nodeHighlight.border = new RectOffset(32, 32, 32, 32);

                nodeBodySingleHighlight = new GUIStyle();
                nodeBodySingleHighlight.normal.background = FGraphStyles.TEX_nodeSingleHighlight;
                nodeBodySingleHighlight.border = nodeBodySingle.border;
                nodeBodySingleHighlight.padding = nodeBodySingle.padding;

                nodeBodySingleHighlightV2 = new GUIStyle(nodeBodySingleHighlight);
                nodeBodySingleHighlightV2.normal.background = FGraphStyles.TEX_nodeSingleV2Highlight;

                moduleHighlight = new GUIStyle();
                moduleHighlight.normal.background = FGraphStyles.TEX_moduleHighlight;
                moduleHighlight.border = new RectOffset(32, 32, 32, 32);

                icon = new GUIStyle();
                icon.fixedWidth = 24;
                icon.fixedHeight = 24;

                tooltip = new GUIStyle("helpBox");
                tooltip.alignment = TextAnchor.MiddleCenter;

#if UNITY_EDITOR
                moduleButton = new GUIStyle(UnityEditor.EditorStyles.toolbarButton);
#else
                moduleButton = new GUIStyle();
#endif
                moduleButton.normal.textColor = new Color(1f, 1f, 1f, 0.6f);
                moduleButton.alignment = TextAnchor.MiddleCenter;
                moduleButton.fontStyle = FontStyle.Bold;
                moduleButton.fontSize = 16;
                moduleButton.fixedHeight = 36;
                moduleButton.normal.background = TEX_moduleAddButton;
                moduleButton.active.background = TEX_moduleAddButton;
                moduleButton.focused.background = TEX_moduleAddButton;

                moduleRemoveButton = new GUIStyle(Label);
                moduleRemoveButton.normal.background = TEX_moduleRemoveButtonNormal;
                moduleRemoveButton.active.background = TEX_moduleRemoveButtonPress;
                moduleRemoveButton.hover.background = TEX_moduleRemoveButtonHover;

                audioLabel = new GUIStyle(Label);

                audioLabel.normal.textColor = defaultTextColor;
                audioLabel.focused.textColor = new Color(1f, 0.6f, 0f, 1f);
                audioLabel.active.textColor = new Color(1f, 0.8f, .2f, 1f);
                audioLabel.fontStyle = FontStyle.Bold;
                audioLabel.alignment = TextAnchor.MiddleLeft;
                audioLabel.padding = new RectOffset(5, 5, 0, 0);
                audioLabel.fontSize = 13;

                valueInfo = new GUIStyle(GUI.skin.box);
                valueInfo.border = new RectOffset(-1, -1, -1, -1);
                valueInfo.fontSize = BLabel.fontSize;
                valueInfo.fontStyle = BLabel.fontStyle;
                valueInfo.margin = new RectOffset(0, 0, 3, 0);
                valueInfo.padding = new RectOffset(2, 2, 1, 1);
                valueInfo.alignment = TextAnchor.UpperCenter;
                valueInfo.fixedHeight = 15;
                Color[] solidColor = new Color[1]; solidColor[0] = new Color(1f, 1f, 1f, 0.2f);
                Texture2D bg = new Texture2D(1, 1); bg.SetPixels(solidColor); bg.Apply();
                valueInfo.normal.background = bg;

                nodeHeaderFrame = new GUIStyle(nodeHeader);
                nodeHeaderFrame.normal.background = TEX_audioNodeHeader;
                nodeHeaderFrame.padding = new RectOffset(0, 0, 1, 3);
                nodeHeaderFrame.alignment = TextAnchor.MiddleCenter;

                nodeHeaderFrameL = new GUIStyle(nodeHeaderFrame);
                nodeHeaderFrameL.normal.background = TEX_audioNodeHeaderL;

                nodeBodySingleV3Frame = new GUIStyle(nodeBody);
                int brder = 18;
                nodeBodySingleV3Frame.border = new RectOffset(brder, brder, brder, brder);
                nodeBodySingleHighlightV2.border = new RectOffset(brder, brder, brder, brder);

                nodeBodySingleV3Frame.normal.background = FGraphStyles.TEX_nodeSingleV3Frame;
                nodeBodySingleV3Body = new GUIStyle(nodeBody);
                //nodeBodySingleV3Body.padding = new RectOffset(32, 32, 8, 14);
                nodeBodySingleV3Body.border = new RectOffset(brder, brder, brder, brder);
                nodeBodySingleV3Body.normal.background = FGraphStyles.TEX_nodeSingleV3Body;

                nodeBodySingleV3Header = new GUIStyle(nodeHeader);
                nodeBodySingleV3Header.fixedHeight = 38;
                nodeBodySingleV3Header.border = new RectOffset(brder, brder, brder, 8);
                //nodeBodySingleV3Header.padding = new RectOffset(0, 0, 0, 30);
                //nodeBodySingleV3Header.margin = new RectOffset(0, 0, 0, 30);
                //nodeBodySingleV3Header.contentOffset = new Vector2(0, 10);
                //nodeBodySingleV3Header.alignment = TextAnchor.UpperCenter;
                nodeBodySingleV3Header.normal.background = FGraphStyles.TEX_nodeSingleV3Header;

                //inputEmpty = new GUIStyle();
                //inputEmpty.normal.background = FGraphStyles.TEX_freeInput;
                //inputFilled = new GUIStyle();
                //inputFilled.normal.background = FGraphStyles.TEX_outputConnected;

                //inputRight = new GUIStyle();
                //inputRight.normal.background = FGraphStyles.TEX_inputRight;
                //inputLeft = new GUIStyle();
                //inputLeft.normal.background = FGraphStyles.TEX_inputLeft;
            }
        }
    }
}