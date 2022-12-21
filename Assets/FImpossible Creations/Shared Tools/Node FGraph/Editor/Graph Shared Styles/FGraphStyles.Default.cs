using UnityEngine;

namespace FIMSpace.Graph
{
    public static partial class FGraphStyles
    {
        public static DefaultGraphStyles DefaultStyles { get { return _defStyles != null ? _defStyles : _defStyles = new DefaultGraphStyles(); } }
        private static DefaultGraphStyles _defStyles = null;


        #region Utility Textures

        public static Texture2D TEX_Vignette { get { return _tex_vign != null ? _tex_vign : _tex_vign = Resources.Load<Texture2D>("SPR_FGridVignette.fw"); } }
        private static Texture2D _tex_vign;

        public static Texture2D TEX_overlayBox { get { return _tex_overlayBox != null ? _tex_overlayBox : _tex_overlayBox = Resources.Load<Texture2D>("ESPR_OverlayBox.fw"); } }
        private static Texture2D _tex_overlayBox;

        public static GUIStyle BGInBoxStyle { get { if (__inBoxStyle != null) return __inBoxStyle; __inBoxStyle = new GUIStyle(Label); Texture2D bg = Resources.Load<Texture2D>("FNInBox"); __inBoxStyle.normal.background = bg; __inBoxStyle.alignment = TextAnchor.MiddleCenter; __inBoxStyle.fontSize = 10; __inBoxStyle.border = new RectOffset(4, 4, 4, 4); __inBoxStyle.padding = new RectOffset(1, 1, 1, 1); __inBoxStyle.margin = new RectOffset(0, 0, 0, 0); return __inBoxStyle; } }
        private static GUIStyle __inBoxStyle = null;

        #endregion


        #region Ports Textures

        public static Texture2D TEX_PointerRight { get { return _tex_pointerRight != null ? _tex_pointerRight : _tex_pointerRight = Resources.Load<Texture2D>("ESPR_PointRight.fw"); } }
        private static Texture2D _tex_pointerRight;

        public static Texture2D TEX_freeInput { get { return _tex_freeInput != null ? _tex_freeInput : _tex_freeInput = Resources.Load<Texture2D>("ESPR_Input.fw"); } }
        private static Texture2D _tex_freeInput;
        public static Texture2D TEX_jackIn { get { return _tex_jackIn != null ? _tex_jackIn : _tex_jackIn = Resources.Load<Texture2D>("ESPR_InputConnected"); } }
        private static Texture2D _tex_jackIn;
        public static Texture2D TEX_outputConnected { get { return _tex_outputConnected != null ? _tex_outputConnected : _tex_outputConnected = Resources.Load<Texture2D>("ESPR_OutputConnected"); } }
        private static Texture2D _tex_outputConnected;
        public static Texture2D TEX_typeCircle { get { return _tex_typeCircle != null ? _tex_typeCircle : _tex_typeCircle = Resources.Load<Texture2D>("ESPR_InputId"); } }
        private static Texture2D _tex_typeCircle;


        #endregion


        #region Textures References


        public static Texture2D TEX_DefGradient { get { return _tex_dgrad != null ? _tex_dgrad : _tex_dgrad = Resources.Load<Texture2D>("FSPR_DefaultConn"); } }
        private static Texture2D _tex_dgrad;

        public static Texture2D TEX_DefNodeBody { get { return _tex_dnodeBody != null ? _tex_dnodeBody : _tex_dnodeBody = Resources.Load<Texture2D>("FSPR_DefaultBody"); } }
        private static Texture2D _tex_dnodeBody;
        public static Texture2D TEX_DefNodeHeader { get { return _tex_dnodeHeader != null ? _tex_dnodeHeader : _tex_dnodeHeader = Resources.Load<Texture2D>("FSPR_DeafultHeader"); } }
        private static Texture2D _tex_dnodeHeader;
        public static Texture2D TEX_DefNodeHighlight { get { return _tex_dnodeHighlight != null ? _tex_dnodeHighlight : _tex_dnodeHighlight = Resources.Load<Texture2D>("FSPR_DeafultHighlight"); } }
        private static Texture2D _tex_dnodeHighlight;

        public static Texture2D TEX_DefNodeConnectorInput { get { return _tex_dnodeInput != null ? _tex_dnodeInput : _tex_dnodeInput = Resources.Load<Texture2D>("FSPR_DefaultInput"); } }
        private static Texture2D _tex_dnodeInput;
        public static Texture2D TEX_DefNodeConnectorOutput { get { return _tex_dnodeOutput != null ? _tex_dnodeOutput : _tex_dnodeOutput = Resources.Load<Texture2D>("FSPR_DefaultOutput"); } }
        private static Texture2D _tex_dnodeOutput;

        #endregion


        public class DefaultGraphStyles
        {
            public GUIStyle nodeHeaderText, nodeHeader, nodeBody, bodyHighlight, overlayBox;

            public DefaultGraphStyles()
            {
                nodeHeaderText = new GUIStyle(BLabel);
                nodeHeaderText.fontSize = 16;
                nodeHeaderText.alignment = TextAnchor.UpperCenter;
                nodeHeaderText.padding = new RectOffset(8, 8, 6, 0);

                nodeHeader = new GUIStyle(nodeHeaderText);
                nodeHeader.normal.background = TEX_DefNodeHeader;

                nodeBody = new GUIStyle();
                nodeBody.normal.background = TEX_DefNodeBody;
                nodeBody.border = new RectOffset(16, 16, 16, 16);
                nodeBody.padding = new RectOffset(12, 12, 8, 8);

                bodyHighlight = new GUIStyle(nodeBody);
                bodyHighlight.normal.background = TEX_DefNodeHighlight;

                overlayBox = new GUIStyle(BLabel);
                overlayBox.normal.background = TEX_overlayBox;
                overlayBox.border = new RectOffset(6, 6, 6, 6);

            }
        }
    }
}