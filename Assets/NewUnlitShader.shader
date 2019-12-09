Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex ("Main", 2D) = "white" {}
        _DamageTex ("Dayum", 2D) = "notwhite" {}
        _factor ("factor", float) = 1.0
        _threshold ("threshold", float) = 0.1
        _power ("power", float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 gles
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members oguv)
#pragma exclude_renderers d3d11
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma target 3.5

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float2 originalVertexXY : TEXCOORD1;
                float2 originalVertexZ : TEXCOORD2;
                float2 originalNormalXY : TEXCOORD3;
                float2 originalNormalZ : TEXCOORD4;
                uint id : SV_VertexID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 deformedNorm : TEXCOORD1;
                float3 originalNorm : TEXCOORD2;
                float3 dist : TEXCOORD3;
            };

            sampler2D _MainTex;
            sampler2D _DamageTex;
            float4 _MainTex_ST;
            float4 _DamageTex_ST;
            float _factor;
            float _threshold;
            float _power;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                float3 original;
                original.xy = v.originalVertexXY.xy;
                original.z = v.originalVertexZ.x;
                o.dist = original - v.vertex;
                float3 ogNormal;
                ogNormal.xy = v.originalNormalXY.xy;
                ogNormal.z = v.originalNormalZ.x;
                o.deformedNorm = v.normal;// - ogNormal;
                o.originalNorm = ogNormal;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 a = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));
                fixed4 b = tex2D(_DamageTex, TRANSFORM_TEX(i.uv, _DamageTex));
                fixed4 col;
                if (length(cross(i.originalNorm, i.deformedNorm)) != 0.0) {
                    col = lerp(a, b, saturate(length(cross(i.originalNorm, i.deformedNorm))));
                } else {
                    col = lerp(a, b, saturate(length(i.dist) * _factor));
                }
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
