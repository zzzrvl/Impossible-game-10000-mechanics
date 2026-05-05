Shader "Custom/XRay"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay+100" "RenderPipeline" = "UniversalPipeline"}
        Pass
        {
ZTest Always

ZWrite Off

Blend SrcAlpha
OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
};

float4 _Color;

Varyings vert(Attributes input)
{
    Varyings output;
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    return output;
}

float4 frag(Varyings input) : SV_Target
{
    return _Color;
}
            ENDHLSL
        }
    }
}