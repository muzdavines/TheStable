// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Unlit alpha-blended shader with tint color.
// - no lighting
// - no lightmap support
Shader "Energy Bar Toolkit/Unlit/Transparent Tint Depth Based" {
    Properties {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    }

    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off
        Fog { Mode Off }
        ZWrite On
        Cull Off
        ColorMaterial AmbientAndDiffuse

        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            struct appdata {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv: TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos (v.vertex);
                o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
                o.color = v.color;
                return o;
            }

            half4 frag (v2f i) : COLOR {
                half4 texcol = tex2D (_MainTex, i.uv);
                return texcol * i.color;
            }

            ENDCG
        }
    }

    SubShader {
        Tags { "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off
        Fog { Mode Off }
        ZWrite On
        Cull Off
        ColorMaterial AmbientAndDiffuse

        Pass {
            SetTexture [_MainTex] {
            	combine texture * primary
            }
        }
    }
}
