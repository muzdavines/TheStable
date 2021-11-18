// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable
// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "CoverShooter/ExplosionPreviewMobile"
{
	Properties
	{
		_Color("Color", Color) = (1, 0, 0, 1)    // (R, G, B, A)
		_Opacity("Opacity", Float) = 1
	}
	SubShader
	{
		Tags
		{ 
			"Queue" = "Transparent" 
			"RenderType" = "Transparent"
			"IgnoreProjector" = "True"
		}

		LOD 100
		ZWrite Off
		Blend One OneMinusSrcAlpha
		Cull Back
		Lighting Off 
		ZWrite Off 
		ZTest LEqual

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			float4 _Color;
			float _Opacity;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				float4 color = _Color;
				color.xyz *= color.w;
				color.w *= _Opacity;

				return color;
			}
			ENDCG
		}
	}
}
