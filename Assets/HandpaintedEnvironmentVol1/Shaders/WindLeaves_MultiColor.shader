// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "HandpaintedVol1/WindFoliageMulticolor"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_SubsuraceDistortion("Subsurace Distortion", Range( 0 , 1)) = 0.5
		_SubsurfaceColor("Subsurface Color", Color) = (1,1,1,0)
		_AlbedoTint("AlbedoTint", Color) = (0,0,0,0)
		_MainTex("_MainTex", 2D) = "white" {}
		_SSSIntensity("SSS Intensity", Range( 0 , 100)) = 0
		_AlbedoMask("AlbedoMask", 2D) = "white" {}
		_SSSScale("SSS Scale", Range( 0 , 0.1)) = 0
		_LeafSpeed("Leaf Speed", Range( 0 , 1)) = 0.3
		_LeafWindStrength("Leaf Wind Strength", Range( 0 , 0.5)) = 0.05
		_WindSpeed("Wind Speed", Range( 0 , 1)) = 0.3
		_WindStrength("Wind Strength", Range( 0 , 1)) = 0.3
		_WindMultiplier("Wind Multiplier", Float) = 2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" }
		Cull Off
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float3 worldNormal;
			float3 viewDir;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform float _LeafSpeed;
		uniform float _LeafWindStrength;
		uniform half _WindMultiplier;
		uniform half _WindSpeed;
		uniform float _WindStrength;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float4 _AlbedoTint;
		uniform sampler2D _AlbedoMask;
		uniform float4 _AlbedoMask_ST;
		uniform float4 _SubsurfaceColor;
		uniform float _SubsuraceDistortion;
		uniform float _SSSIntensity;
		uniform float _SSSScale;
		uniform float _Cutoff = 0.5;


		float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
		{
			original -= center;
			float C = cos( angle );
			float S = sin( angle );
			float t = 1 - C;
			float m00 = t * u.x * u.x + C;
			float m01 = t * u.x * u.y - S * u.z;
			float m02 = t * u.x * u.z + S * u.y;
			float m10 = t * u.x * u.y + S * u.z;
			float m11 = t * u.y * u.y + C;
			float m12 = t * u.y * u.z - S * u.x;
			float m20 = t * u.x * u.z - S * u.y;
			float m21 = t * u.y * u.z + S * u.x;
			float m22 = t * u.z * u.z + C;
			float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
			return mul( finalMatrix, original ) + center;
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 appendResult74 = (float2(0.0 , ( v.texcoord.xy.y * cos( ( ( ase_vertex3Pos + ( ( ase_worldPos * 0.3 ) + ( _Time.y * _LeafSpeed ) ) ) / 0.1 ) ) ).x));
			float temp_output_38_0 = ( ( ( ase_worldPos.x + ase_worldPos.z ) * 0.01 ) + ( _Time.y * _WindSpeed ) );
			float temp_output_61_0 = ( ( ase_vertex3Pos.y * ( ( ( sin( ( temp_output_38_0 * 4.0 ) ) + sin( ( temp_output_38_0 * 15.0 ) ) ) - cos( ( temp_output_38_0 * 5.0 ) ) ) * 0.1 ) ) * _WindStrength );
			float4 appendResult65 = (float4(temp_output_61_0 , 0.0 , temp_output_61_0 , 0.0));
			float4 break70 = mul( appendResult65, unity_ObjectToWorld );
			float3 appendResult76 = (float3(break70.x , 0.0 , break70.z));
			float3 rotatedValue78 = RotateAroundAxis( float3( 0,0,0 ), appendResult76, float3( 0,0,0 ), 0.0 );
			v.vertex.xyz += ( float3( ( v.color.g * ( appendResult74 * _LeafWindStrength ) ) ,  0.0 ) + ( ( v.color.r * _WindMultiplier ) * rotatedValue78 ) );
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2DNode14 = tex2D( _MainTex, uv_MainTex );
			SurfaceOutputStandard s15 = (SurfaceOutputStandard ) 0;
			float2 uv_AlbedoMask = i.uv_texcoord * _AlbedoMask_ST.xy + _AlbedoMask_ST.zw;
			float4 lerpResult88 = lerp( ( _AlbedoTint * tex2DNode14 ) , tex2DNode14 , ( 1.0 - tex2D( _AlbedoMask, uv_AlbedoMask ) ));
			s15.Albedo = lerpResult88.rgb;
			float3 ase_worldNormal = i.worldNormal;
			s15.Normal = ase_worldNormal;
			s15.Emission = float3( 0,0,0 );
			s15.Metallic = 0.0;
			s15.Smoothness = 0.0;
			s15.Occlusion = 1.0;

			data.light = gi.light;

			UnityGI gi15 = gi;
			#ifdef UNITY_PASS_FORWARDBASE
			Unity_GlossyEnvironmentData g15 = UnityGlossyEnvironmentSetup( s15.Smoothness, data.worldViewDir, s15.Normal, float3(0,0,0));
			gi15 = UnityGlobalIllumination( data, s15.Occlusion, s15.Normal, g15 );
			#endif

			float3 surfResult15 = LightingStandard ( s15, viewDir, gi15 ).rgb;
			surfResult15 += s15.Emission;

			#ifdef UNITY_PASS_FORWARDADD//15
			surfResult15 -= s15.Emission;
			#endif//15
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 objToWorldDir22 = mul( unity_ObjectToWorld, float4( ase_vertex3Pos, 0 ) ).xyz;
			float3 normalizeResult23 = normalize( objToWorldDir22 );
			float dotResult6 = dot( i.viewDir , -( ase_worldlightDir + ( normalizeResult23 * _SubsuraceDistortion ) ) );
			float dotResult27 = dot( pow( dotResult6 , _SSSIntensity ) , _SSSScale );
			float4 blendOpSrc18 = float4( surfResult15 , 0.0 );
			float4 blendOpDest18 = ( float4( surfResult15 , 0.0 ) + ( _SubsurfaceColor * saturate( dotResult27 ) * distance( ase_vertex3Pos , float3( 0,0,0 ) ) ) );
			float4 lerpBlendMode18 = lerp(blendOpDest18,( blendOpSrc18 * blendOpDest18 ),0.6);
			c.rgb = ( saturate( lerpBlendMode18 )).rgb;
			c.a = 1;
			clip( tex2DNode14.a - _Cutoff );
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.viewDir = worldViewDir;
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
				o.Alpha = LightingStandardCustomLighting( o, worldViewDir, gi ).a;
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
1080;727;2560;1301;3198.284;1356.798;1.6;True;True
Node;AmplifyShaderEditor.WorldPosInputsNode;29;-3597.799,1645.248;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleTimeNode;33;-3331.111,1890.989;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;32;-3526.014,1816.533;Float;False;Constant;_multiply;multiply;5;0;Create;True;0;0;False;0;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-3446.748,2009.335;Half;False;Property;_WindSpeed;Wind Speed;10;0;Create;True;0;0;False;0;0.3;0.046;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;30;-3291.792,1678.599;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-3117.697,1989.001;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-3142.888,1714.238;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-2863.399,1638.06;Float;False;Constant;_Float0;Float 0;5;0;Create;True;0;0;False;0;4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-2815.962,1904.858;Float;False;Constant;_Float1;Float 1;5;0;Create;True;0;0;False;0;15;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;38;-2988.775,1866.082;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-2633.742,1703.657;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-2813.859,2013.376;Float;False;Constant;_Float3;Float 3;5;0;Create;True;0;0;False;0;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;-2632.104,1818.655;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;20;-3639.438,278.6364;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinOpNode;44;-2426.245,1818.952;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;42;-2424.891,1702.459;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-2630,1939.083;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;46;-2244.449,1743.012;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;45;-2424.469,1939.831;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TransformDirectionNode;22;-3445.038,122.0041;Float;False;Object;World;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleTimeNode;52;-2665.417,964.1615;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;49;-2756.563,1220.095;Float;False;Property;_LeafSpeed;Leaf Speed;8;0;Create;True;0;0;False;0;0.3;0.573;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-2863.593,779.8685;Float;False;Constant;_Multiplication;Multiplication;9;0;Create;True;0;0;False;0;0.3;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-2148.44,2041.463;Float;False;Constant;_Float14;Float 14;5;0;Create;True;0;0;False;0;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;50;-2820.649,599.3683;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;9;-3202.41,220.916;Float;False;Property;_SubsuraceDistortion;Subsurace Distortion;1;0;Create;True;0;0;False;0;0.5;0.244;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;23;-3199.337,123.3041;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;47;-2102.53,1913.241;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-2780.434,124.4221;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;4;-3229.394,-63.59536;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PosVertexDataNode;56;-2082.02,1594.218;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-2423.593,1074.897;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-1920.969,1931.233;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;-2402.401,797.7274;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;58;-2183.088,1035.761;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;60;-2192.748,665.8822;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;57;-1726.72,1971.712;Float;False;Property;_WindStrength;Wind Strength;11;0;Create;True;0;0;False;0;0.3;0.006;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-1716.792,1830.338;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;11;-2590.955,-62.09594;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;3;-2802.931,-269.0933;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;-1547.793,1829.338;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;62;-1958.925,1098.76;Float;False;Constant;_LeafSpeedFrequency;Leaf Speed Frequency;7;0;Create;True;0;0;False;0;0.1;0.1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;5;-2443.955,-59.09594;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;63;-1820.115,920.2443;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;6;-2310.931,-260.0933;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;64;-1411.466,998.4379;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;65;-1392.793,1782.338;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ObjectToWorldMatrixNode;66;-1442.793,1936.338;Float;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-2449.208,22.67554;Float;False;Property;_SSSIntensity;SSS Intensity;5;0;Create;True;0;0;False;0;0;16;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;14;-2347.077,-869.2488;Float;True;Property;_MainTex;_MainTex;4;0;Create;True;0;0;False;0;None;05e6cf3d522d89e439057a0f29ebaa7b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;86;-2329.869,-514.7296;Float;False;Property;_AlbedoTint;AlbedoTint;3;0;Create;True;0;0;False;0;0,0,0,0;0.003921586,0.6509804,0.03601455,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CosOpNode;67;-1256.451,999.5695;Float;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-2447.208,113.6757;Float;False;Property;_SSSScale;SSS Scale;7;0;Create;True;0;0;False;0;0;0.0099;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;84;-2449.558,-1239.863;Float;True;Property;_AlbedoMask;AlbedoMask;6;0;Create;True;0;0;False;0;None;0e294e419b1b0fc4dad893b91e29d5de;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;92;-1856.462,-575.4709;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;-1206.794,1822.338;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PowerNode;25;-2131.209,-120.3244;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;68;-1306.028,827.4202;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;90;-2088.923,-1230.788;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;87;-1563.134,-510.4091;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;70;-1051.794,1823.338;Float;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.WireNode;91;-1738.784,-809.0139;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;27;-1938.209,-122.3244;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-1007.135,929.0287;Float;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexColorNode;73;-1093.613,1252.996;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DistanceOpNode;24;-2180.381,287.3999;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-942.1476,1057.845;Float;False;Property;_LeafWindStrength;Leaf Wind Strength;9;0;Create;True;0;0;False;0;0.05;0.077;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;74;-820.2556,929.0294;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;76;-742.7896,1824.338;Float;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;7;-2137.932,-257.0933;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;88;-1229.534,-501.809;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0.9215687,0.9215687,0.9215687,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-857.6126,1341.997;Half;False;Property;_WindMultiplier;Wind Multiplier;12;0;Create;True;0;0;False;0;2;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;12;-1813.795,-267.7603;Float;False;Property;_SubsurfaceColor;Subsurface Color;2;0;Create;True;0;0;False;0;1,1,1,0;0.07355816,0.8207547,0.1097858,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;-649.6129,1277.997;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;80;-647.5307,938.9395;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VertexColorNode;77;-1032.117,567.5809;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-1275.326,-255.7801;Float;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CustomStandardSurface;15;-888.5924,-501.9348;Float;False;Metallic;Tangent;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;78;-587.6126,1818.997;Float;False;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-476.2006,-277.912;Float;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-28.74219,1145.924;Float;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;81;-424.1593,838.674;Float;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;82;261.9756,832.5972;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BlendOpsNode;18;-43.1843,-302.2059;Float;False;Multiply;True;3;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0.6;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;2;810.8989,-521.0718;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;HandpaintedVol1/WindFoliageMulticolor;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;2;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;30;0;29;1
WireConnection;30;1;29;3
WireConnection;35;0;33;0
WireConnection;35;1;31;0
WireConnection;34;0;30;0
WireConnection;34;1;32;0
WireConnection;38;0;34;0
WireConnection;38;1;35;0
WireConnection;40;0;38;0
WireConnection;40;1;37;0
WireConnection;39;0;38;0
WireConnection;39;1;36;0
WireConnection;44;0;39;0
WireConnection;42;0;40;0
WireConnection;43;0;38;0
WireConnection;43;1;41;0
WireConnection;46;0;42;0
WireConnection;46;1;44;0
WireConnection;45;0;43;0
WireConnection;22;0;20;0
WireConnection;23;0;22;0
WireConnection;47;0;46;0
WireConnection;47;1;45;0
WireConnection;10;0;23;0
WireConnection;10;1;9;0
WireConnection;55;0;52;0
WireConnection;55;1;49;0
WireConnection;53;0;47;0
WireConnection;53;1;48;0
WireConnection;54;0;50;0
WireConnection;54;1;51;0
WireConnection;58;0;54;0
WireConnection;58;1;55;0
WireConnection;59;0;56;2
WireConnection;59;1;53;0
WireConnection;11;0;4;0
WireConnection;11;1;10;0
WireConnection;61;0;59;0
WireConnection;61;1;57;0
WireConnection;5;0;11;0
WireConnection;63;0;60;0
WireConnection;63;1;58;0
WireConnection;6;0;3;0
WireConnection;6;1;5;0
WireConnection;64;0;63;0
WireConnection;64;1;62;0
WireConnection;65;0;61;0
WireConnection;65;2;61;0
WireConnection;67;0;64;0
WireConnection;92;0;14;0
WireConnection;69;0;65;0
WireConnection;69;1;66;0
WireConnection;25;0;6;0
WireConnection;25;1;26;0
WireConnection;90;0;84;0
WireConnection;87;0;86;0
WireConnection;87;1;92;0
WireConnection;70;0;69;0
WireConnection;91;0;14;0
WireConnection;27;0;25;0
WireConnection;27;1;28;0
WireConnection;71;0;68;2
WireConnection;71;1;67;0
WireConnection;24;0;20;0
WireConnection;74;1;71;0
WireConnection;76;0;70;0
WireConnection;76;2;70;2
WireConnection;7;0;27;0
WireConnection;88;0;87;0
WireConnection;88;1;91;0
WireConnection;88;2;90;0
WireConnection;79;0;73;1
WireConnection;79;1;72;0
WireConnection;80;0;74;0
WireConnection;80;1;75;0
WireConnection;13;0;12;0
WireConnection;13;1;7;0
WireConnection;13;2;24;0
WireConnection;15;0;88;0
WireConnection;78;3;76;0
WireConnection;16;0;15;0
WireConnection;16;1;13;0
WireConnection;83;0;79;0
WireConnection;83;1;78;0
WireConnection;81;0;77;2
WireConnection;81;1;80;0
WireConnection;82;0;81;0
WireConnection;82;1;83;0
WireConnection;18;0;15;0
WireConnection;18;1;16;0
WireConnection;2;10;14;4
WireConnection;2;13;18;0
WireConnection;2;11;82;0
ASEEND*/
//CHKSM=8F6E88BFA13441D7BAB5259777AD9C4A104E023F