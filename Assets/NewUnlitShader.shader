Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex ("Main", 2D) = "white" {}
        _DamageTex ("Dayum", 2D) = "notwhite" {}
        _factor ("factor", float) = 1.0
        _threshold ("threshold", float) = 0.1
        _power ("power", float) = 2.0
        // _OriginalVertecies ("OriginalVertecies", Vector) = (1, 1, 1, 1)
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
                // float2 uvMain : TEXCOORD0;
                // float2 uvDamage : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                // float3 diff : TEXCOORD1;
                // float3 diffNorm : TEXCOORD2;
                float3 deformedNorm : TEXCOORD1;
                float3 originalNorm : TEXCOORD2;
                float3 dist : TEXCOORD3;
                // float oguv : TEXCOORD2;
            };

            sampler2D _MainTex;
            sampler2D _DamageTex;
            float4 _MainTex_ST;
            float4 _DamageTex_ST;
            float _factor;
            float _threshold;
            float _power;
            // float4 ogPoints[10000];
            // float4 _OriginalVertecies;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                // o.diff = v.vertex;// - v.originalVertex;
                float3 original;
                original.xy = v.originalVertexXY.xy;
                // o.diff.y = v.originalVertexXY.y;
                original.z = v.originalVertexZ.x;
                o.dist = original - v.vertex;
                // o.diff = v.vertex - original;
                float3 ogNormal;
                ogNormal.xy = v.originalNormalXY.xy;
                ogNormal.z = v.originalNormalZ.x;
                o.deformedNorm = v.normal;// - ogNormal;
                o.originalNorm = ogNormal;
                // o.diffNorm = ogNormal;
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
                // float distance = length(i.diff) / _factor;
                
                fixed4 a = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));
                fixed4 b = tex2D(_DamageTex, TRANSFORM_TEX(i.uv, _DamageTex));
                fixed4 col;
                if (length(cross(i.originalNorm, i.deformedNorm)) != 0.0) {
                    col = lerp(a, b, saturate(length(cross(i.originalNorm, i.deformedNorm))));
                    // col = lerp(a, b, saturate(length(i.dist)));
                    // col = b;
                } else {
                    // col = lerp(a, b, length(i.originalNorm - i.deformedNorm) / _factor);
                    col = lerp(a, b, saturate(length(i.dist) * _factor));
                }

                // if (distance > _threshold) {
                //     float t = saturate(pow(distance, _power));
                //     col = lerp(a, b, t);
                // } else {
                //     col = a; // use the normals
                // }
                // col = lerp(a, b, abs(i.diffNorm.x) +abs(i.diffNorm.y) +abs(i.diffNorm.z));
                // // col = fixed4(i.diff, 1.0);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
