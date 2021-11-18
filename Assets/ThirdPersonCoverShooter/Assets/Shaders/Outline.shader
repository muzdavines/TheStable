Shader "CoverShooter/Outline" 
{
	Properties
	{
		_Color("Color", Color) = (0, 0, 0, 1)
		_Width("Width", Range(0.0, 2)) = 0.5
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	struct appdata 
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2f 
	{
		float4 pos : POSITION;
		float4 color : COLOR;
	};

	uniform float _Width;
	uniform float4 _Color;

	v2f vert(appdata v) 
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);

		float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
		float2 offset = TransformViewToProjection(norm.xy);

		if (length(offset) > 0.001f)
			offset = normalize(offset);

		o.pos.xy += offset * _Width;
		o.color = _Color;
		return o;
	}

	ENDCG

	SubShader
	{
		Tags { "Queue" = "Transparent" }

		Pass
		{
			Name "BASE"
			Cull Back
			Blend Zero One

			// uncomment this to hide inner details:
			Offset -8, -8

			SetTexture[_OutlineColor]
			{
				ConstantColor(0,0,0,0)
				Combine constant
			}
		}

		Pass
		{
			Name "OUTLINE"
			Tags { "LightMode" = "Always" }
			Cull Front
			Blend One OneMinusDstColor

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			half4 frag(v2f i) : COLOR
			{
				return i.color;
			}

			ENDCG
		}
	}

	Fallback "Diffuse"
}