Shader "Fimpossible/PGG/Utils/Tile Preview Wireframe 2"
{
	Properties
	{
		_WireVal("Wireframe width", Range(0., 0.5)) = 0.05
		_FrontColor("Front color", color) = (1., 1., 1., 1.)
		_BackColor("Back color", color) = (1., 1., 1., 1.)
	}

	SubShader
	{
		Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }

	Pass
	{
		Cull Back
		CGPROGRAM

		#pragma vertex vert
		#pragma fragment frag
		#pragma geometry geom

			#include "UnityCG.cginc"

			struct vTriangl 
			{
				float4 worldPos : SV_POSITION;
				fixed4 color : COLOR;
			};

			struct vStream 
			{
				float4 pos : SV_POSITION;
				float3 texCrd : TEXCOORD0;
				fixed4 color : COLOR;
			};

			vTriangl vert(appdata_full v) 
			{
				vTriangl o;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.color = v.color;
				return o;
			}

			[maxvertexcount(3)]
			void geom(triangle vTriangl IN[3], inout TriangleStream<vStream> triStream) 
			{
				float3 offst = float3(0., 0., 0.);

				float edgA = length(IN[0].worldPos - IN[1].worldPos);
				float edgB = length(IN[1].worldPos - IN[2].worldPos);
				float edgC = length(IN[2].worldPos - IN[0].worldPos);

				if (edgA > edgB && edgA > edgC)
					offst.y = 1.;
				else if (edgB > edgC && edgB > edgA)
					offst.x = 1.;
				else
					offst.z = 1.;

				vStream o;
				o.pos = mul(UNITY_MATRIX_VP, IN[0].worldPos);
				o.texCrd = float3(1., 0., 0.) + offst;
				o.color = IN[0].color;
				triStream.Append(o);

				o.pos = mul(UNITY_MATRIX_VP, IN[1].worldPos);
				o.texCrd = float3(0., 0., 1.) + offst;
				o.color = IN[1].color;
				triStream.Append(o);

				o.pos = mul(UNITY_MATRIX_VP, IN[2].worldPos);
				o.texCrd = float3(0., 1., 0.) + offst;
				o.color = IN[2].color;
				triStream.Append(o);
			}

			float _WireVal;
			fixed4 _FrontColor;

			fixed4 frag(vStream i) : SV_Target
			{
				if (!any(bool3(i.texCrd.x <= _WireVal, i.texCrd.y <= _WireVal, i.texCrd.z <= _WireVal)))
				 discard;

				return _FrontColor * i.color;
			}

			ENDCG
		}
	
	
	
	


		Pass
			{
				Cull Front
				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#pragma geometry geom

			#include "UnityCG.cginc"


			struct vTriangl { float4 worldPos : SV_POSITION; };


			struct vStream
			{
				float4 pos : SV_POSITION;
				float3 texCrd : TEXCOORD0;
			};


			vTriangl vert(appdata_base v)
			{
				vTriangl o;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);

				return o;
			}


			[maxvertexcount(3)]
			void geom(triangle vTriangl IN[3], inout TriangleStream<vStream> triStream)
			{
				float3 offst = float3(0., 0., 0.);

				float edgA = length(IN[0].worldPos - IN[1].worldPos);
				float edgB = length(IN[1].worldPos - IN[2].worldPos);
				float edgC = length(IN[2].worldPos - IN[0].worldPos);

				if (edgA > edgB && edgA > edgC)
					offst.y = 1.;
				else if (edgB > edgC && edgB > edgA)
					offst.x = 1.;
				else
					offst.z = 1.;

				vStream o;
				o.pos = mul(UNITY_MATRIX_VP, IN[0].worldPos);
				o.texCrd = float3(1., 0., 0.) + offst;
				triStream.Append(o);

				o.pos = mul(UNITY_MATRIX_VP, IN[1].worldPos);
				o.texCrd = float3(0., 0., 1.) + offst;
				triStream.Append(o);

				o.pos = mul(UNITY_MATRIX_VP, IN[2].worldPos);
				o.texCrd = float3(0., 1., 0.) + offst;
				triStream.Append(o);
			}

			float _WireVal;
			fixed4 _BackColor;

			fixed4 frag(vStream i) : SV_Target
			{
				if (!any(bool3(i.texCrd.x < _WireVal, i.texCrd.y < _WireVal, i.texCrd.z < _WireVal)))
					discard;

				return _BackColor;
			}

			ENDCG
			}
	
	
	
	}




}
