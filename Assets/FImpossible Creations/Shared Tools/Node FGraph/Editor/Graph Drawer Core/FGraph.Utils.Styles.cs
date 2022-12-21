using UnityEngine;

namespace FIMSpace.Graph
{
    public abstract partial class FGraphDrawerBase
    {
        public Texture2D Tex_Net = null;

        public virtual void RefreshNetTextureReference()
        {
            if (Tex_Net is null) Tex_Net = Resources.Load<Texture2D>("SPR_APPGridNet");
            if (Tex_Net is null)
            {
                Tex_Net = new Texture2D(1, 1);
                Tex_Net.SetPixel(0, 0, new Color(0.05f, 0.05f, 0.08f, 1f));
                Tex_Net.Apply();
            }
        }


        public static GUIStyle WindowBGStyle
        {
            get
            {
                if (_wbgStyle == null)
                {
                    _wbgStyle = new GUIStyle();
                    _wbgStyle.normal.background = Tex_BG;
                }

                return _wbgStyle;
            }
        }
        private static GUIStyle _wbgStyle = null;

        public static GUIStyle WindowLightBGStyle
        {
            get
            {
                if (_wbglStyle == null)
                {
                    _wbglStyle = new GUIStyle();
                    _wbglStyle.normal.background = Tex_BG_L;
                }

                return _wbglStyle;
            }
        }
        private static GUIStyle _wbglStyle = null;
        public static Texture2D Tex_BG
        {
            get
            {
                if (_tex_bg == null)
                {
                    _tex_bg = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    _tex_bg.SetPixel(0, 0, new Color32(16, 16, 19, 255));
                    //30,30,34
                    //16,16,19
                    _tex_bg.Apply();
                }

                return _tex_bg;
            }
        }
        private static Texture2D _tex_bg = null;
        public static Texture2D Tex_BG_L
        {
            get
            {
                if (_tex_bgl == null)
                {
                    _tex_bgl = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    _tex_bgl.SetPixel(0, 0, new Color32(30, 30, 34, 255));
                    _tex_bgl.Apply();
                }

                return _tex_bgl;
            }
        }

        private static Texture2D _tex_bgl = null;


    }
}