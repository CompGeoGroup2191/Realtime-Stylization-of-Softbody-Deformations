Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex ("Main", 2D) = "white" {}
        _DamageTex ("Dayum", 2D) = "notwhite" {}
        _factor ("factor", float) = 1.0
        _threshold ("threshold", float) = 0.1
        _OriginalVertecies ("OriginalVertecies", float4) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members oguv)
#pragma exclude_renderers d3d11
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 originalVertex : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                // float2 uvMain : TEXCOORD0;
                // float2 uvDamage : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 diff : TEXCOORD1;
                // float oguv : TEXCOORD2;
            };

            sampler2D _MainTex;
            sampler2D _DamageTex;
            float4 _MainTex_ST;
            float4 _DamageTex_ST;
            float _factor;
            float _threshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.diff = v.vertex;// - v.originalVertex;
                // o.diff = v.originalVertex;
                // o.uvMain = TRANSFORM_TEX(v.uv, _MainTex);
                // o.uvDamage = TRANSFORM_TEX(v.uv, _DamageTex);
                // o.oguv = v.uv.x
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float t;
                float distance = length(i.diff) / _factor;
                if (length(i.diff) > _threshold) {
                    t = 1.0;
                } else {
                    t = 0.0; // use the normals
                }

                fixed4 a = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));
                fixed4 b = tex2D(_DamageTex, TRANSFORM_TEX(i.uv, _DamageTex));
                fixed4 col = lerp(a, b, t);

                col = fixed4(i.diff, 1.0);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
