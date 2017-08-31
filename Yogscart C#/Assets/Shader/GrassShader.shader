// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "RobosCoolShaders/Grass"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}
		_Length("Length", float) = 1.0
		_Width("Width", float) = 1.0
		_Gravity("Gravity", float) = 1.0
		_Steps("Steps", int) = 1
		_NoiseIntensity("NoiseIntensity", float) = 1.0
		_DirectionIntensity("DirectionIntensity", float) = 1.0
		_LengthIntensity("LengthIntensity", float) = 1.0
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

	Pass
	{
	
		Cull Off 

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma geometry geom

		#include "UnityCG.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float4 vertex : SV_POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
			float3 worldPosition : TEXCOORD1;
		};

		sampler2D _MainTex;
		float4 _MainTex_ST;

		// Base properties
		float _Length;
		float _Width;
		float _Gravity;
		int _Steps;
		// Randomization properties
		sampler2D _Noise;
		float _NoiseIntensity;
		float _DirectionIntensity;
		float _LengthIntensity;

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			o.normal = v.normal;
			o.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
			return o;
		}

		[maxvertexcount(80)]
		void geom(triangle v2f input[3], inout TriangleStream<v2f> OutputStream)
		{
			float4 P1 = input[0].vertex;
			float4 P2 = input[1].vertex;
			float4 P3 = input[2].vertex;

			float4 N1 = float4(input[0].normal,1);
			float4 N2 = float4(input[1].normal, 1);
			float4 N3 = float4(input[2].normal, 1);

			float3 W1 = input[0].worldPosition;
			float3 W2 = input[1].worldPosition;
			float3 W3 = input[2].worldPosition;

			//Calculate Average Position
			float4 P = (P1 + P2 + P3) / 3.0f;
			//Calculate Average World Position
			float3 W = (W1 + W2 + W3) / 3.0f;
			//Calculate Average Normal
			float4 N = (N1 + N2 + N3) / 3.0f;
			//Calculate Lateral Direction of Grass
			float4 T = float4(normalize((P2 - P1).xyz), 0.0f);

			// Sample textures
			float3 noise = tex2Dlod(_Noise, float4(W * _NoiseIntensity,1)).xyz;

			// Modulate strand length
			float l = _Length + noise.r * _LengthIntensity;

			// Modulate grow direction
			float3 noiseNormal = (noise * 2 - 1) * _DirectionIntensity; // Convert the noise sample to a vector and scale it with the intensity.
			N = normalize(float4((N + noiseNormal).xyz, 0)); // Add the noise normal and normalize. Make sure the fourth component is null to avoid issues.

			//Itteriate for number of steps
			for (int i = 0; i < _Steps; i++)
			{
				float t0 = (float)i / _Steps;
				float t1 = (float)(i + 1) / _Steps;

				// Make our normal bend down with gravity.
				// The further we are on the strand, and the longer it is, the more it bends.
				// We then normalize this new direction, and scale it by the length at the current iteration of the loop.
				float4 p0 = normalize(N - (float4(0, _Length * t0, 0, 0) * _Gravity * t0)) * (_Length * t0);
				float4 p1 = normalize(N - (float4(0, _Length * t1, 0, 0) * _Gravity * t1)) * (_Length * t1);

				// Interpolate the width, and scale the lateral direction vector with it
				float4 w0 = T * lerp(_Width, 0, t0);
				float4 w1 = T * lerp(_Width, 0, t1);

				v2f test = (v2f)0;
				test.normal = N.xyz;

				float4 e0 = P + (p0 - w0);
				test.vertex = e0;
				OutputStream.Append(test);

				e0 = P + (p0 + w0);
				test.vertex = e0;
				OutputStream.Append(test);

				e0 = P + (p1 - w1);
				test.vertex = e0;
				OutputStream.Append(test);

				e0 = P + (p1 + w1);
				test.vertex = e0;
				OutputStream.Append(test);

				//v2f test = (v2f)0;
				//float3 normal = normalize(cross(input[1].worldPosition.xyz - input[0].worldPosition.xyz, input[2].worldPosition.xyz - input[0].worldPosition.xyz));
				//for (int i = 0; i < 3; i++)
				//{
				//	test.normal = normal;
				//	test.vertex = input[i].vertex;
				//	test.uv = input[i].uv;
				//	OutputStream.Append(test);
				//}
			}
		}

		fixed4 frag(v2f i) : SV_Target
		{
			// sample the texture
			fixed4 col = tex2D(_MainTex, i.uv);

		float3 lightDir = float3(1, 1, 0);
		float ndotl = dot(i.normal, normalize(lightDir));

		return col * ndotl;
		}
			ENDCG
		}
	}
}
