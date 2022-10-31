// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "KriptoFX/ME/GlowCutout" {
	Properties{
		[HDR]_TintColor("Tint Color", Color) = (0.5,0.5,0.5,1)
		_TimeScale("Time Scale", Vector) = (1,1,1,1)
		_MainTex("Noise Texture", 2D) = "white" {}
	_BorderScale("Border Scale (XY) Offset (Z)", Vector) = (0.5,0.05,1,0)
	}

		Category{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZWrite Off
		Offset -1, -1

		SubShader{
		Pass{

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_instancing
#pragma multi_compile_fog

#include "UnityCG.cginc"

		sampler2D _MainTex;
	float4 _TintColor;
	float4 _TimeScale;
	float4 _BorderScale;

	struct appdata_t {
		float4 vertex : POSITION;
		fixed4 color : COLOR;
		float2 texcoord : TEXCOORD0;
		float3 normal : NORMAL;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f {
		float4 vertex : POSITION;
		fixed4 color : COLOR;
		float2 texcoord : TEXCOORD0;
		float3 normal : NORMAL;
		float3 worldPosScaled : TEXCOORD1;

		UNITY_FOG_COORDS(2)
		UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
	};

	float4 _MainTex_ST;

	v2f vert(appdata_t v)
	{
		v2f o;

		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_TRANSFER_INSTANCE_ID(v, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

		v.vertex.xyz += v.normal / 100 * _BorderScale.z;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.color = 1;
		o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
		float3 worldPos = v.vertex * float3(length(unity_ObjectToWorld[0].xyz), length(unity_ObjectToWorld[1].xyz), length(unity_ObjectToWorld[2].xyz));
		o.worldPosScaled = worldPos.xyz *  _MainTex_ST.x;
		o.normal = abs(v.normal);

		UNITY_TRANSFER_FOG(o, o.vertex);
		return o;
	}

	half2 tex2DTriplanar(sampler2D tex, float2 offset, float3 worldPos, float3 normal)
	{
		half2 yDiff = tex2D(tex, worldPos.xz + offset);
		half2 xDiff = tex2D(tex, worldPos.zy + offset);
		half2 zDiff = tex2D(tex, worldPos.xy + offset);
		normal = normal / (normal.x + normal.y + normal.z);
		return xDiff * normal.x + yDiff * normal.y + zDiff * normal.z;
	}

	half4 frag(v2f i) : COLOR
	{	 
		UNITY_SETUP_INSTANCE_ID(i);
		half2 mask = tex2DTriplanar(_MainTex, _Time.xx * _TimeScale.xy, i.worldPosScaled, i.normal);
		half2 tex = tex2DTriplanar(_MainTex, mask + _Time.xx * _TimeScale.zw, i.worldPosScaled, i.normal);
		float4 res = 0;
#if (!UNITY_COLORSPACE_GAMMA)
		//tex = pow(tex, 0.45);

#endif
		res.r = step(tex.r, _BorderScale.x);
		res.r -= step(tex.r, _BorderScale.x - _BorderScale.y);
		res.r *= tex.g;

		res = res.r * i.color * _TintColor;
		res.a = saturate(res.a);
		res =  res * _TintColor.a;

		UNITY_APPLY_FOG(i.fogCoord, res);

		return res;
	}
		ENDCG
	}
	}
	}

}