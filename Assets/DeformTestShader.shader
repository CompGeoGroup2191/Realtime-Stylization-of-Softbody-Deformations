﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/DeformTestShader"
{
    Properties
    {
        // _Color ("Color", Color) = (1,1,1,1)
        // _Color2 ("Color2", Color) = (0.5, 0.2, 0.2, 1)
        _MainTex ("Main", 2D) = "white" {}
        _DamageTexture ("Damage", 2D) = "white" {}
        // _Glossiness ("Smoothness", Range(0,1)) = 0.5
        // _Metallic ("Metallic", Range(0,1)) = 0.0
        // _Damage("Damage", )
        // _DamageTexture("DamageTexture", 2D) = "white" {}
    }
    SubShader
    {
        // Tags { "RenderType"="Opaque" }
        // LOD 200

        Pass
        {
            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            // #pragma surface surf Standard fullforwardshadows vertex:vert

            // // Use shader model 3.0 target, to get nicer looking lighting
            // #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            struct AppData {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VertToFrag {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // sampler2D _MainTex;

            // struct Input
            // {
            //     float2 uv_MainTex;
            //     float damage;
            //     uint vid : SV_VertexID;
            // };

            // half _Glossiness;
            // half _Metallic;
            // fixed4 _Color;
            // fixed4 _Color2;
            // float _Damage[1000];


            VertToFrag vert(AppData ad) {
                VertToFrag ret;
                ret.vertex = UnityObjectToClipPos(ad.vertex);
                ret.uv = ad.uv;
                return ret;
            }
            sampler2D _MainTex;
            sampler2D _DamageTexture;
            fixed4 frag(VertToFrag vf) : SV_Target {
                return tex2D(_MainTex, vf.uv);
            }

            // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
            // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
            // #pragma instancing_options assumeuniformscaling
            UNITY_INSTANCING_BUFFER_START(Props)
                // put more per-instance properties here
            UNITY_INSTANCING_BUFFER_END(Props)

            // void vert (inout appdata_full v, out Input o) {
            //     UNITY_INITIALIZE_OUTPUT(Input,o);
            //     o.damage = _Damage[o.vid];
            // }

            // void surf (Input IN, inout SurfaceOutputStandard o)
            // {
            //     // Albedo comes from a texture tinted by color
            //     // fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color; //lerp(_Color, _Color2, 0.0);
            //     fixed4 c =  lerp(_Color, _Color2, IN.damage);
            //     o.Albedo = c.rgb;
            //     // o.Albedo *= IN.customColor;
            //     // Metallic and smoothness come from slider variables
            //     o.Metallic = _Metallic;
            //     o.Smoothness = _Glossiness;
            //     o.Alpha = c.a;
            // }
            ENDCG
        }
    }
    // FallBack "Diffuse"
}
