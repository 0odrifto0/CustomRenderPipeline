Shader "CustomSRP/BlinnPhongLit"
{
    Properties
    {
        _MainColor ("MainColor", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap ("Bump", 2D) = "Bump" {}
        _SpecularFactor ("Specular Factor", float) = 32
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}
        LOD 200

        Pass
        {
     		Tags { "LightMode" = "ForwardBase" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#include "Assets/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float3 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 viewDir : TEXCOORD1;
                float3 lightDir : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _MainColor;
            sampler2D _NormalMap;
            float4 _NormalMap_ST;
            float _SpecularFactor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float3x3 objectToTangent = CreateTangentToWorld(v.normal, v.tangent, v.tangent.w);
                
                float3 objectCameraPos = TransformWorldToObject(_WorldSpaceCameraPos);
                float3 objectViewDir = normalize(objectCameraPos - v.vertex);
                float3 objectLightDir = TransformWorldToObjectDir(normalize(_MainLightPosition.xyz));
                o.viewDir = mul(objectToTangent, objectViewDir);
                o.lightDir = mul(objectToTangent, objectLightDir);
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(UnpackNormal(tex2D(_NormalMap, i.uv)).xyz * 2);

                half4 albedo = tex2D(_MainTex, i.uv) * _MainColor;

                float3 lightDir = i.lightDir;
                // diffuse
                float diffuse = max(dot(lightDir, normal), 0);

                // specular
                float3 viewDir = i.viewDir;
                float3 halfDir = normalize(lightDir + viewDir);
                float specular = pow(max(dot(halfDir, normal), 0), _SpecularFactor);
                
                half4 finalColor = albedo + diffuse * albedo + specular*_MainLightColor;
                return finalColor;
            }

            ENDHLSL
        }
    }
}
