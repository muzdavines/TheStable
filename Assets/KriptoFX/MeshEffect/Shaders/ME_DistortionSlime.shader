Shader "KriptoFX/ME/DistortionSlime" {
	Properties{
			_TintColor("Main Color", Color) = (1,1,1,1)
			_MainTex("Base (RGB) Emission Tex (A)", 2D) = "white" {}
			_CutOut("CutOut (A)", 2D) = "white" {}
			_BumpMap("Normalmap", 2D) = "bump" {}
			_BumpAmt("Distortion", Float) = 10
	}
		Category{

			Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
						Blend SrcAlpha OneMinusSrcAlpha
						ZWrite Off
						Offset -1,-1
						Cull Off
						

			SubShader {
				GrabPass {
					"_GrabTexture"
				}
				Pass {
					CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					#pragma multi_compile_instancing
					#pragma multi_compile_fog

					#include "UnityCG.cginc"

					sampler2D _MainTex;
					sampler2D _BumpMap;
					sampler2D _CutOut;
					samplerCUBE _Cube;

					float _BumpAmt;
					UNITY_DECLARE_SCREENSPACE_TEXTURE(_GrabTexture);
					float4 _GrabTexture_TexelSize;

					float4 _TintColor;
					float _FPOW;
					float _R0;

					struct appdata_t {
						float4 vertex : POSITION;
						float3 normal : NORMAL;
						half4 color : COLOR;
						float2 texcoord : TEXCOORD0;
						UNITY_VERTEX_INPUT_INSTANCE_ID
					};

					struct v2f {
						half4 vertex : POSITION;
						half2 uv_MainTex: TEXCOORD0;
						half2 uv_BumpMap : TEXCOORD1;
						half2 uv_CutOut : TEXCOORD2;
						half4 grab : TEXCOORD3;
						half4 color : COLOR;
						UNITY_FOG_COORDS(4)
						UNITY_VERTEX_INPUT_INSTANCE_ID
						UNITY_VERTEX_OUTPUT_STEREO
					};

					float4 _MainTex_ST;
					float4 _BumpMap_ST;
					float4 _CutOut_ST;

					v2f vert(appdata_full v)
					{
						v2f o;
						UNITY_SETUP_INSTANCE_ID(v);
						UNITY_TRANSFER_INSTANCE_ID(v, o);
						UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

						o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
						o.uv_BumpMap = TRANSFORM_TEX(v.texcoord, _BumpMap);
						o.uv_CutOut = TRANSFORM_TEX(v.texcoord, _CutOut);

						o.vertex = UnityObjectToClipPos(v.vertex);
						
						o.grab = ComputeGrabScreenPos(o.vertex);
						o.color = v.color;

						UNITY_TRANSFER_FOG(o, o.vertex);
						return o;
					}

					half4 frag(v2f i) : COLOR
					{
						UNITY_SETUP_INSTANCE_ID(i);

						half4 tex = tex2D(_MainTex, i.uv_MainTex);
						half4 c = tex * _TintColor;
						half4 cut = tex2D(_CutOut, i.uv_CutOut);

						half3 normal = UnpackNormal(tex2D(_BumpMap, i.uv_BumpMap));

						half2 offset = normal.rg * _BumpAmt * 0.01 * i.color.a;
						i.grab.xy = offset * i.grab.z + i.grab.xy;
						half4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture, i.grab.xy / i.grab.w);

						half gray = col.r * 0.3 + col.g * 0.59 + col.b * 0.11;
						half3 emission = col.rgb*_TintColor.rgb;

						half4 res = half4(emission, cut.a * _TintColor.a * i.color.r * i.color.a);

						UNITY_APPLY_FOG(i.fogCoord, res);

						return res;
					}
					ENDCG
				}
			}
			}
}