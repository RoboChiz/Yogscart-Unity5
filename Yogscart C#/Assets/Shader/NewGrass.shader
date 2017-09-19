Shader "RobosCoolShaders/GrassV2"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Length("Length", float) = 1.0
		_Width("Width", float) = 1.0
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
			float4 N = normalize((N1 + N2 + N3) / 3.0f);
			//Calculate Lateral Direction of Grass
			float4 T = float4(normalize((P2 - P1).xyz), 0.0f);

			//Create a temp vertex to hold data
			v2f temp = (v2f)0;
			temp.normal = N.xyz;

			float4 e0 = P;
			temp.vertex = e0;
			temp.uv = float2(0, 0);
			OutputStream.Append(temp);

			e0 = P + float4(1,0,0,0);
			temp.vertex = e0;
			temp.uv = float2(1, 0);
			OutputStream.Append(temp);

			e0 = P + (N * _length);
			temp.vertex = e0;
			temp.uv = float2(0, 1);
			OutputStream.Append(temp);

			e0 = P + float4(1, 0, 0, 0) + (N * _Length);
			temp.vertex = e0;
			temp.uv = float2(1, 1);
			OutputStream.Append(temp);

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
