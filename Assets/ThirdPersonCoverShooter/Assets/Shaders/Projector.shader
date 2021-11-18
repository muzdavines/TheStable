// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Projector/Multiply" 
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 0, 1)    // (R, G, B, A)
		_ShadowTex("Texture", 2D) = "gray" {}
	}
	Subshader
	{
		Tags { "Queue" = "Transparent" }
		Pass
		{
			ZWrite Off
			Fog { Color(1, 1, 1) }
			AlphaTest Greater 0
			ColorMask RGB
			Blend DstColor One
			Offset -1, -1

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
			};

			float4x4 unity_Projector;

			v2f vert(float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(vertex);
				o.uv = mul(unity_Projector, vertex);

				return o;
			}

			sampler2D _ShadowTex;
			float4 _Color;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 result = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uv));
				result *= _Color;
				result.rgb *= result.a;
				
				return result;
			}

			ENDCG
		}
	}
}