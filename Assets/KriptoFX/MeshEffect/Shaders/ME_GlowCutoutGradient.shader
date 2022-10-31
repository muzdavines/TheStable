Shader "KriptoFX/ME/GlowCutoutGradient" {
	Properties{
		[HDR]_TintColor("Tint Color", Color) = (0.5,0.5,0.5,1)
		_GradientStrength("Gradient Strength", Float) = 0.5
		_TimeScale("Time Scale", Vector) = (1,1,1,1)
		_MainTex("Noise Texture", 2D) = "white" {}
	_BorderScale("Border Scale (XY) Offset (Z)", Vector) = (0.5,0.05,1,1)
	}
		Category{

		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		SubShader{

		Pass{

			Blend One OneMinusSrcAlpha
			Cull Off
			Offset -1, -1
			ZWrite Off


		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_instancing
#pragma multi_compile_fog
#include "UnityCG.cginc"

	sampler2D _MainTex;

	UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_DEFINE_INSTANCED_PROP(float4, _TintColor)
		UNITY_DEFINE_INSTANCED_PROP(float4, _TimeScale)
		UNITY_DEFINE_INSTANCED_PROP(float4, _BorderScale)
		UNITY_DEFINE_INSTANCED_PROP(float, _GradientStrength)
	UNITY_INSTANCING_BUFFER_END(Props)

	//float4 _TintColor;
	//float4 _TimeScale;
	//float4 _BorderScale;
	//half _GradientStrength;

	struct appdata_t {
		float4 vertex : POSITION;
		half4 color : COLOR;
		float2 texcoord : TEXCOORD0;
		float3 normal : NORMAL;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f {
		float4 vertex : POSITION;
		half4 color : COLOR;
		float2 texcoord : TEXCOORD0;
		float4 worldPosScaled : TEXCOORD1;
		float3 normal : NORMAL;
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


		//v.vertex.xyz += v.normal / 100 * _BorderScale.z;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.color = 1;
		o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
		float3 worldPos = v.vertex * float3(length(unity_ObjectToWorld[0].xyz), length(unity_ObjectToWorld[1].xyz), length(unity_ObjectToWorld[2].xyz));
		o.worldPosScaled.x = worldPos.x *  _MainTex_ST.x;
		o.worldPosScaled.y = worldPos.y *  _MainTex_ST.y;
		o.worldPosScaled.z = worldPos.z *  _MainTex_ST.x;
		o.worldPosScaled.w = worldPos.z *  _MainTex_ST.y;
		o.normal = abs(v.normal);

		UNITY_TRANSFER_FOG(o, o.vertex);
		return o;
	}


	half tex2DTriplanar(sampler2D tex, float2 offset, float4 worldPos, float3 normal)
	{
		half3 texColor;
		texColor.x = tex2D(tex, worldPos.zy + offset);
		texColor.y = tex2D(tex, worldPos.xw + offset);
		texColor.z = tex2D(tex, worldPos.xy + offset);
		normal = normal / (normal.x + normal.y + normal.z);
		return dot(texColor, normal);
	}

	half4 frag(v2f i) : COLOR
	{
		UNITY_SETUP_INSTANCE_ID(i);
		//_Time.x = 0;
		float4 timeScale = UNITY_ACCESS_INSTANCED_PROP(Props, _TimeScale);
		float4 borderScale = UNITY_ACCESS_INSTANCED_PROP(Props, _BorderScale);
		half mask = tex2DTriplanar(_MainTex, _Time.x * timeScale.xy, i.worldPosScaled, i.normal);

		half tex = tex2DTriplanar(_MainTex, _Time.x * timeScale.zw + mask * borderScale.x, i.worldPosScaled, i.normal);
		half alphaMask = tex2DTriplanar(_MainTex, 0.3 + mask * borderScale.y, i.worldPosScaled, i.normal);

		float4 res;


		res = pow(UNITY_ACCESS_INSTANCED_PROP(Props, _TintColor), 2.2);
		res *= tex * mask;

		res = lerp(float4(0, 0, 0, 0), res, alphaMask.xxxx);


		res.rgb = pow(res.rgb, borderScale.w);
		//#ifndef UNITY_COLORSPACE_GAMMA
		//		res.rgb = pow(res.rgb, 0.75);
		//#endif

		half gray = dot(saturate(res.rgb + UNITY_ACCESS_INSTANCED_PROP(Props, _GradientStrength)), 0.33);
		//res.rgb = 1 - exp(-res.rgb);
		res =  float4(res.rgb, gray )* UNITY_ACCESS_INSTANCED_PROP(Props, _TintColor).a;

		res.rgb = clamp(res.rgb, 0, 10);

#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
		res.rgba *= i.fogCoord.x;
#endif
		return  res;
	}
		ENDCG
	}
	}

	}
}
