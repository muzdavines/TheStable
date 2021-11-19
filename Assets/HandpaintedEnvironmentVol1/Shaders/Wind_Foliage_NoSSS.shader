// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "HandpaintedVol1/WindFoliage_NoSSS"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_AlbedoTint("AlbedoTint", Color) = (1,1,1,0)
		_MainTex("_MainTex", 2D) = "white" {}
		_AlbedoMask("AlbedoMask", 2D) = "white" {}
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
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Lambert keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform float _LeafSpeed;
		uniform float _LeafWindStrength;
		uniform half _WindMultiplier;
		uniform half _WindSpeed;
		uniform float _WindStrength;
		uniform float4 _AlbedoTint;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform sampler2D _AlbedoMask;
		uniform float4 _AlbedoMask_ST;
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

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2DNode14 = tex2D( _MainTex, uv_MainTex );
			float2 uv_AlbedoMask = i.uv_texcoord * _AlbedoMask_ST.xy + _AlbedoMask_ST.zw;
			float4 lerpResult88 = lerp( ( _AlbedoTint * tex2DNode14 ) , tex2DNode14 , ( 1.0 - tex2D( _AlbedoMask, uv_AlbedoMask ) ));
			o.Albedo = lerpResult88.rgb;
			o.Alpha = 1;
			clip( tex2DNode14.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
1080;727;2560;1301;1147.916;97.1411;1;True;True
Node;AmplifyShaderEditor.WorldPosInputsNode;29;-3597.799,1645.248;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleTimeNode;33;-3331.111,1890.989;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;32;-3526.014,1816.533;Float;False;Constant;_multiply;multiply;5;0;Create;True;0;0;False;0;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-3446.748,2009.335;Half;False;Property;_WindSpeed;Wind Speed;6;0;Create;True;0;0;False;0;0.3;0.204;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;30;-3291.792,1678.599;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-3117.697,1989.001;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-3142.888,1714.238;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-2863.399,1638.06;Float;False;Constant;_Float0;Float 0;5;0;Create;True;0;0;False;0;4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-2815.962,1904.858;Float;False;Constant;_Float1;Float 1;5;0;Create;True;0;0;False;0;15;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;38;-2988.775,1866.082;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-2633.742,1703.657;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-2813.859,2013.376;Float;False;Constant;_Float3;Float 3;5;0;Create;True;0;0;False;0;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;-2632.104,1818.655;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;44;-2426.245,1818.952;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;42;-2424.891,1702.459;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-2630,1939.083;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;46;-2244.449,1743.012;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;45;-2424.469,1939.831;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-2148.44,2041.463;Float;False;Constant;_Float14;Float 14;5;0;Create;True;0;0;False;0;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;49;-2756.563,1220.095;Float;False;Property;_LeafSpeed;Leaf Speed;4;0;Create;True;0;0;False;0;0.3;0.288;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-2863.593,779.8685;Float;False;Constant;_Multiplication;Multiplication;9;0;Create;True;0;0;False;0;0.3;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;52;-2665.417,964.1615;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;47;-2102.53,1913.241;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;50;-2820.649,599.3683;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PosVertexDataNode;56;-2082.02,1594.218;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-2423.593,1074.897;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-1920.969,1931.233;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;-2402.401,797.7274;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;60;-2192.748,665.8822;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;58;-2183.088,1035.761;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-1716.792,1830.338;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-1726.72,1971.712;Float;False;Property;_WindStrength;Wind Strength;7;0;Create;True;0;0;False;0;0.3;0.458;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;62;-1958.925,1098.76;Float;False;Constant;_LeafSpeedFrequency;Leaf Speed Frequency;7;0;Create;True;0;0;False;0;0.1;0.1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;-1547.793,1829.338;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;63;-1820.115,920.2443;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;64;-1411.466,998.4379;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;65;-1392.793,1782.338;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ObjectToWorldMatrixNode;66;-1442.793,1936.338;Float;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.CosOpNode;67;-1256.451,999.5695;Float;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;-1206.794,1822.338;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;68;-1306.028,827.4202;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-1007.135,929.0287;Float;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;70;-1051.794,1823.338;Float;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;76;-742.7896,1824.338;Float;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexColorNode;73;-1093.613,1252.996;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;72;-857.6126,1341.997;Half;False;Property;_WindMultiplier;Wind Multiplier;8;0;Create;True;0;0;False;0;2;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-939.699,1057.845;Float;False;Property;_LeafWindStrength;Leaf Wind Strength;5;0;Create;True;0;0;False;0;0.05;0.054;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;74;-820.2556,929.0294;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;-649.6129,1277.997;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;78;-587.6126,1818.997;Float;False;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexColorNode;77;-1254.429,645.9954;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;86;-384.3886,227.3543;Float;False;Property;_AlbedoTint;AlbedoTint;1;0;Create;True;0;0;False;0;1,1,1,0;0,0.7464175,0.8584906,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;80;-647.5307,938.9395;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;84;-471.4299,592.6669;Float;True;Property;_AlbedoMask;AlbedoMask;3;0;Create;True;0;0;False;0;None;609bb905051edda4ba86a5793b7b14bc;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;14;-470.0134,399.8959;Float;True;Property;_MainTex;_MainTex;2;0;Create;True;0;0;False;0;None;7ea45a7a8bc7a4549b19dd223b380d2a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;87;84.24405,378.9164;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-28.74219,1145.924;Float;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;81;-424.1593,838.674;Float;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;90;-11.79443,599.7419;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;88;442.5423,548.2275;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0.9215687,0.9215687,0.9215687,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;82;265.8755,832.5972;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;2;960.1729,548.5565;Float;False;True;2;Float;ASEMaterialInspector;0;0;Lambert;HandpaintedVol1/WindFoliage_NoSSS;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;2;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
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
WireConnection;47;0;46;0
WireConnection;47;1;45;0
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
WireConnection;61;0;59;0
WireConnection;61;1;57;0
WireConnection;63;0;60;0
WireConnection;63;1;58;0
WireConnection;64;0;63;0
WireConnection;64;1;62;0
WireConnection;65;0;61;0
WireConnection;65;2;61;0
WireConnection;67;0;64;0
WireConnection;69;0;65;0
WireConnection;69;1;66;0
WireConnection;71;0;68;2
WireConnection;71;1;67;0
WireConnection;70;0;69;0
WireConnection;76;0;70;0
WireConnection;76;2;70;2
WireConnection;74;1;71;0
WireConnection;79;0;73;1
WireConnection;79;1;72;0
WireConnection;78;3;76;0
WireConnection;80;0;74;0
WireConnection;80;1;75;0
WireConnection;87;0;86;0
WireConnection;87;1;14;0
WireConnection;83;0;79;0
WireConnection;83;1;78;0
WireConnection;81;0;77;2
WireConnection;81;1;80;0
WireConnection;90;0;84;0
WireConnection;88;0;87;0
WireConnection;88;1;14;0
WireConnection;88;2;90;0
WireConnection;82;0;81;0
WireConnection;82;1;83;0
WireConnection;2;0;88;0
WireConnection;2;10;14;4
WireConnection;2;11;82;0
ASEEND*/
//CHKSM=2B226FDF0434FA2463829092D8709DCA3ED29617