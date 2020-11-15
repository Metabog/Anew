Shader "Unlit/Grid"
{
    Properties
    {
        _ParticleTex ("ParticleTex", 2D) = "white" {}
		_QuantizedGrid("QuantizedGrid", 2D) = "white" {}
		_VectorField("VectorField", 2D) = "white" {}
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
            };

            sampler2D _ParticleTex;
			sampler2D _QuantizedGrid;
			sampler2D _VectorField;

            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			float lines(float2 p)
			{
				const float scale = 128.0f;
				const float sampling = 128.0f;
				const float2 R = float2(4096.0f, 4096.0f);

				//float2 uv = (p - R / 2.) / R.y;
				float2 arrow_read_pos = frac(p* scale) *2. - 1;

				float2 n = tex2D(_VectorField, floor(p*scale)/scale);

				float l = length(n),	
					x = (n.x * arrow_read_pos.y - n.y * arrow_read_pos.x) / l,
					w = dot(n, arrow_read_pos) / l;         // option: arrow points to vector dir

				float ff = 0.0f;
				ff = smoothstep(scale * 3. / R.y, 0., abs(x) + .05 * w); // display vector line
				ff *= smoothstep(3. / R.y, 0., abs(w) - l); // optional: show vector length
				
				return ff;
			}

			float3 hsv2rgb(in float3 c)
			{
				float3 rgb = clamp(abs(fmod(c.x * 6.0 + float3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);

				return c.z * lerp(float3(1.0,1.0f,1.0f), rgb, c.y);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				const float grid_res = 32.0f;

				// sample the texture
				float4 col = 0.0f;
				fixed4 particles = tex2D(_ParticleTex, i.uv);
				fixed4 quant = tex2D(_QuantizedGrid, i.uv);
				fixed4 vec_field = tex2D(_VectorField, i.uv);
				float grid_val = 0.1f  * smoothstep(0.9, 1.0, max(frac(i.uv.x*grid_res), frac(i.uv.y*grid_res)));

				col = float4(grid_val, grid_val, grid_val, 0.1f);
				if(length(particles)>0.0f)
					col = particles;

				//col += quant;
				//col += vec_field*0.2f;
				//float fviz = fieldviz(i.uv*10.0);
				//col += float4(fviz,fviz,fviz,fviz);

				float a = lines(i.uv);
				float hsvang = atan2(vec_field.x, vec_field.y);
				hsvang += 1.0f;
				hsvang *= 0.09f;
				float3 vec_color = hsv2rgb(float3(hsvang, 0.6f, 0.7f));

				col += a * float4(vec_color, a);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
