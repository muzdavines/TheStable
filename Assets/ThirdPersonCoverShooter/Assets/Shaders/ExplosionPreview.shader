// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable
// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "CoverShooter/ExplosionPreview"
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
		Cull Front
		Lighting Off 
		ZWrite Off 
		ZTest Always

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
				float4 uv : TEXCOORD0;
				float3 ray : TEXCOORD1;
			};

			float4 _Color;
			float _Opacity;
			float3 _Center;
			float _Radius;

			sampler2D _CameraDepthTexture;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = ComputeScreenPos(o.vertex);
				o.ray = UnityObjectToViewPos(v.vertex) * float3(-1, -1, 1);

				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
				float2 uv = i.uv.xy / i.uv.w;

				float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, uv));
				depth = Linear01Depth(depth);
				float4 vpos = float4(i.ray * depth,1);
				float3 wpos = mul(unity_CameraToWorld, vpos).xyz;

				if (distance(wpos, _Center) > _Radius * 0.5f)
					discard;

				float4 color = _Color;
				color.xyz *= color.w;
				color.w *= _Opacity;

				return color;
			}
			ENDCG
		}
	}
}
