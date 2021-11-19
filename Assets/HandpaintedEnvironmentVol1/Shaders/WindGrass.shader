// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "HandpaintedVol1/WindGrass"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.05
		_AlbedoTint("AlbedoTint", Color) = (0,0,0,0)
		_MainTex("_MainTex", 2D) = "white" {}
		_AlbedoMask("AlbedoMask", 2D) = "white" {}
		_WorldPositionSpeed("World Position Speed", Range( 0 , 5)) = 2.5
		_Frequency("Frequency", Range( 0 , 1)) = 0.25
		_XZWave("XZWave", Range( 0 , 2)) = 0.25
		_YWave("YWave", Range( 0 , 3)) = 0.25
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

		uniform float _XZWave;
		uniform float _WorldPositionSpeed;
		uniform float _Frequency;
		uniform float _YWave;
		uniform float4 _AlbedoTint;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform sampler2D _AlbedoMask;
		uniform float4 _AlbedoMask_ST;
		uniform float _Cutoff = 0.05;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float temp_output_101_0 = sin( ( ( ase_worldPos.x + _Time.y + ase_worldPos.z ) * _WorldPositionSpeed ) );
			float lerpResult109 = lerp( cos( ( ( _Time.y + v.texcoord.xy.y ) / _Frequency ) ) , 0.0 , ( 1.0 - v.texcoord.xy.y ));
			float temp_output_112_0 = ( _XZWave * ( temp_output_101_0 * lerpResult109 ) );
			float clampResult95 = clamp( ( 1.0 - v.texcoord.xy.y ) , 0.0 , 1.0 );
			float lerpResult114 = lerp( temp_output_101_0 , 0.0 , clampResult95);
			float3 appendResult113 = (float3(temp_output_112_0 , ( _YWave * lerpResult114 ) , temp_output_112_0));
			v.vertex.xyz += ( v.color.r * appendResult113 * v.color.b );
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
933;798;2560;1301;1014.052;950.6387;1;True;True
Node;AmplifyShaderEditor.TextureCoordinatesNode;104;-3121.484,693.2877;Float;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TimeNode;98;-3030.043,309.0691;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;107;-2420.785,574.9875;Float;False;Property;_Frequency;Frequency;10;0;Create;True;0;0;False;0;0.25;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;96;-3045.17,98.02378;Float;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;105;-2670.385,476.1871;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;106;-2281.681,474.887;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;99;-2676.043,340.0691;Float;False;Property;_WorldPositionSpeed;World Position Speed;9;0;Create;True;0;0;False;0;2.5;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;93;-3045.415,-235.3493;Float;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;97;-2611.696,122.9202;Float;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;94;-2735.415,-191.3492;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;108;-2032.085,474.887;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;102;-2677.884,747.7872;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;100;-2268.043,119.6871;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;95;-2506.415,-195.3492;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;109;-1774.686,607.4869;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;101;-2029.044,121.0691;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;114;-1576.781,-98.84321;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;-1337.408,582.2603;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;111;-1329.401,478.841;Float;False;Property;_XZWave;XZWave;11;0;Create;True;0;0;False;0;0.25;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;116;-1539.47,145.7744;Float;False;Property;_YWave;YWave;12;0;Create;True;0;0;False;0;0.25;0;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;84;-868.4485,-375.0466;Float;True;Property;_AlbedoMask;AlbedoMask;3;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;115;-1122.099,-86.40511;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;86;-861.106,-847.9666;Float;False;Property;_AlbedoTint;AlbedoTint;1;0;Create;True;0;0;False;0;0,0,0,0;0.3537734,0.6513632,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;14;-944.7433,-675.5905;Float;True;Property;_MainTex;_MainTex;2;0;Create;True;0;0;False;0;None;83aa432230935474cb4b81b96f9f8642;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;112;-841.5466,560.3802;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;113;-399.3002,528.5939;Float;True;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexColorNode;117;-683.98,-9.46991;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;87;-4.469604,-763.8334;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;90;-523.917,-370.7468;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-2633.258,2582.894;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;60;-2196.006,1309.693;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;49;-2759.821,1863.906;Float;False;Property;_LeafSpeed;Leaf Speed;4;0;Create;True;0;0;False;0;0.3;0.285;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;42;-2428.149,2346.27;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-2866.851,1423.679;Float;False;Constant;_Multiplication;Multiplication;9;0;Create;True;0;0;False;0;0.3;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;-2405.659,1441.538;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;38;-2992.033,2509.893;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;88;361.1266,-518.4271;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0.9215687,0.9215687,0.9215687,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CosOpNode;67;-1259.709,1643.38;Float;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-2637,2347.468;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;52;-2668.675,1607.972;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;74;-823.5135,1572.84;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;46;-2247.707,2386.823;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-2819.22,2548.669;Float;False;Constant;_Float1;Float 1;5;0;Create;True;0;0;False;0;15;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;76;-746.0475,2468.149;Float;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexColorNode;77;-1035.375,1211.391;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;65;-1396.051,2426.149;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-2866.657,2281.871;Float;False;Constant;_Float0;Float 0;5;0;Create;True;0;0;False;0;4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-32.00011,1789.734;Float;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-3146.146,2358.049;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;78;-590.8705,2462.808;Float;False;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;80;-650.7886,1582.75;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;-1210.052,2466.149;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.VertexColorNode;73;-1096.871,1896.806;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ObjectToWorldMatrixNode;66;-1446.051,2580.149;Float;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;58;-2186.346,1679.572;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-2151.698,2685.274;Float;False;Constant;_Float14;Float 14;5;0;Create;True;0;0;False;0;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;44;-2429.503,2462.763;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-860.8705,1985.807;Half;False;Property;_WindMultiplier;Wind Multiplier;8;0;Create;True;0;0;False;0;2;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;50;-2823.907,1243.179;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-1924.227,2575.044;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;45;-2427.727,2583.642;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;63;-1823.373,1564.055;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;-2635.362,2462.466;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;29;-3601.057,2289.059;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;32;-3529.272,2460.344;Float;False;Constant;_multiply;multiply;5;0;Create;True;0;0;False;0;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-1010.393,1572.839;Float;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;-652.8708,1921.807;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-945.4055,1701.656;Float;False;Property;_LeafWindStrength;Leaf Wind Strength;5;0;Create;True;0;0;False;0;0.05;0.111;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;82;258.7177,1476.408;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;118;-88.97998,19.53009;Float;True;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;62;-1962.183,1742.571;Float;False;Constant;_LeafSpeedFrequency;Leaf Speed Frequency;7;0;Create;True;0;0;False;0;0.1;0.1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;56;-2085.278,2238.029;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;-1551.051,2473.149;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;68;-1309.286,1471.231;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;30;-3295.05,2322.41;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-3120.955,2632.812;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;70;-1055.052,2467.149;Float;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;31;-3450.006,2653.146;Half;False;Property;_WindSpeed;Wind Speed;6;0;Create;True;0;0;False;0;0.3;0.435;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;81;-427.4172,1482.484;Float;True;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;47;-2105.788,2557.052;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-1729.978,2615.523;Float;False;Property;_WindStrength;Wind Strength;7;0;Create;True;0;0;False;0;0.3;0.007;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-2426.851,1718.708;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;33;-3334.369,2534.8;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;64;-1414.724,1642.249;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-2817.117,2657.187;Float;False;Constant;_Float3;Float 3;5;0;Create;True;0;0;False;0;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-1720.05,2474.149;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;2;810.8989,-521.0718;Float;False;True;2;Float;ASEMaterialInspector;0;0;Lambert;HandpaintedVol1/WindGrass;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.05;True;True;0;True;TransparentCutout;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;2;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;105;0;98;2
WireConnection;105;1;104;2
WireConnection;106;0;105;0
WireConnection;106;1;107;0
WireConnection;97;0;96;1
WireConnection;97;1;98;2
WireConnection;97;2;96;3
WireConnection;94;0;93;2
WireConnection;108;0;106;0
WireConnection;102;0;104;2
WireConnection;100;0;97;0
WireConnection;100;1;99;0
WireConnection;95;0;94;0
WireConnection;109;0;108;0
WireConnection;109;2;102;0
WireConnection;101;0;100;0
WireConnection;114;0;101;0
WireConnection;114;2;95;0
WireConnection;110;0;101;0
WireConnection;110;1;109;0
WireConnection;115;0;116;0
WireConnection;115;1;114;0
WireConnection;112;0;111;0
WireConnection;112;1;110;0
WireConnection;113;0;112;0
WireConnection;113;1;115;0
WireConnection;113;2;112;0
WireConnection;87;0;86;0
WireConnection;87;1;14;0
WireConnection;90;0;84;0
WireConnection;43;0;38;0
WireConnection;43;1;41;0
WireConnection;42;0;40;0
WireConnection;54;0;50;0
WireConnection;54;1;51;0
WireConnection;38;0;34;0
WireConnection;38;1;35;0
WireConnection;88;0;87;0
WireConnection;88;1;14;0
WireConnection;88;2;90;0
WireConnection;67;0;64;0
WireConnection;40;0;38;0
WireConnection;40;1;37;0
WireConnection;74;1;71;0
WireConnection;46;0;42;0
WireConnection;46;1;44;0
WireConnection;76;0;70;0
WireConnection;76;2;70;2
WireConnection;65;0;61;0
WireConnection;65;2;61;0
WireConnection;83;0;79;0
WireConnection;83;1;78;0
WireConnection;34;0;30;0
WireConnection;34;1;32;0
WireConnection;78;3;76;0
WireConnection;80;0;74;0
WireConnection;80;1;75;0
WireConnection;69;0;65;0
WireConnection;69;1;66;0
WireConnection;58;0;54;0
WireConnection;58;1;55;0
WireConnection;44;0;39;0
WireConnection;53;0;47;0
WireConnection;53;1;48;0
WireConnection;45;0;43;0
WireConnection;63;0;60;0
WireConnection;63;1;58;0
WireConnection;39;0;38;0
WireConnection;39;1;36;0
WireConnection;71;0;68;2
WireConnection;71;1;67;0
WireConnection;79;0;73;1
WireConnection;79;1;72;0
WireConnection;82;0;81;0
WireConnection;82;1;83;0
WireConnection;118;0;117;1
WireConnection;118;1;113;0
WireConnection;118;2;117;3
WireConnection;61;0;59;0
WireConnection;61;1;57;0
WireConnection;30;0;29;1
WireConnection;30;1;29;3
WireConnection;35;0;33;0
WireConnection;35;1;31;0
WireConnection;70;0;69;0
WireConnection;81;0;77;2
WireConnection;81;1;80;0
WireConnection;47;0;46;0
WireConnection;47;1;45;0
WireConnection;55;0;52;0
WireConnection;55;1;49;0
WireConnection;64;0;63;0
WireConnection;64;1;62;0
WireConnection;59;0;56;2
WireConnection;59;1;53;0
WireConnection;2;0;88;0
WireConnection;2;10;14;4
WireConnection;2;11;118;0
ASEEND*/
//CHKSM=7A8CE20D637832341B938CAAC7ECB6B214A492C3