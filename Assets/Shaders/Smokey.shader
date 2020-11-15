Shader "Unlit/Smokey"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("color", Color) = (.34, .85, .92, 1) // color
		_Offset("Offset", Range(0,100)) = 0
	}
		SubShader
	{
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 worldSpacePos : TEXCOORD1;
			};

			float _Offset;
			float4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}


			float hash(float n)
			{
				return frac(sin(n) * 43758.5453);
			}

			float noise(float3 x)
			{
				// The noise function returns a value in the range -1.0f -> 1.0f
				x *= 1.5f;
				float3 p = floor(x);
				float3 f = frac(x);

				f = f * f * (3.0 - 2.0 * f);
				float n = p.x + p.y * 57.0 + 113.0 * p.z;

				return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
					lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
					lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
						lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float spd_hash = hash(_Offset * 123.0f);
				float4 thiscol = _Color;
				float4 randcol = float4(hash(_Offset), hash(_Offset + 2343.0f), hash(_Offset + 234.0f), 1.0f);
				thiscol = lerp(thiscol, randcol, 0.5f);
				spd_hash *= 0.5f;
				spd_hash + 0.5f;
				float4 col = 0.5f * thiscol * noise(float3(i.uv.x*16.0f + _Time.y* spd_hash + _Offset * 1234.0f,i.uv.y * 16.0f + _Time.y * spd_hash + _Offset*1234.0f, 0.0f));

				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
		ENDCG
	}
	}
}
