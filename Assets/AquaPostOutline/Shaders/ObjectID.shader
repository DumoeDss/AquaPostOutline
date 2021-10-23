Shader "Unlit/ObjectID"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag


	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 vcolor : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 vcolor : TEXCOORD0;
            };



            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.vcolor = v.vcolor;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = i.vcolor;
                return col;
            }
            ENDHLSL
        }
    }
}