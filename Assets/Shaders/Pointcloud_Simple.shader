Shader "DX11/Pointcloud_Simple"
{
	SubShader
	{
		Pass
	{

		CGPROGRAM
		#pragma target 5.0

		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"
		#include "ParticleSystemCommon.cginc"
		
		// struct ParticleStruct
		// {
		// 	float3 pos;
		// 	float3 vel;
		// 	float3 cd;
		// };

		//The buffer containing the points we want to draw.
		//StructuredBuffer<float3> buf_Points;
		StructuredBuffer<ParticleStruct> dataBuffer;

		//A simple input struct for our pixel shader step containing a position.
		struct ps_input 
		{
			float4 pos : SV_POSITION;
			float4 cd : COLOR;
		};

		//Our vertex function simply fetches a point from the buffer corresponding to the vertex index
		//which we transform with the view-projection matrix before passing to the pixel program.
		ps_input vert(uint id : SV_VertexID)
		{
			ps_input o;
			float3 worldPos = dataBuffer[id].pos;
			o.pos = mul(UNITY_MATRIX_VP, float4(worldPos,1.0f));
			o.cd = float4(dataBuffer[id].cd, 1);
			return o;
		}

		//Pixel function returns a solid color for each point.
		float4 frag(ps_input i) : COLOR
		{
			return i.cd;
		}

			ENDCG

		}
		}

		Fallback Off
}