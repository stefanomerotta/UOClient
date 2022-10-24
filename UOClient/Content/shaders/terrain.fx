#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#include "macros.fxh"

DECLARE_TEXTURE(Texture0, 0);
DECLARE_TEXTURE(Texture1, 1);
DECLARE_TEXTURE(Texture2, 2);

float4 DiffuseColor = 1111;

matrix WorldViewProjection;



struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 Texture0 : TEXCOORD0;
    float2 Texture1 : TEXCOORD1;
    float2 Texture2 : TEXCOORD2;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 Texture0 : TEXCOORD0;
    float2 Texture1 : TEXCOORD1;
    float2 Texture2 : TEXCOORD2;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = mul(input.Position, WorldViewProjection);
    output.Texture0 = input.Texture0;
    output.Texture1 = input.Texture1;
    output.Texture2 = input.Texture2;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = SAMPLE_TEXTURE(Texture0, input.Texture0);
    float4 overlay = SAMPLE_TEXTURE(Texture1, input.Texture1);
    float4 alpha = SAMPLE_TEXTURE(Texture2, input.Texture2);
    
    return lerp(color, overlay, alpha.a);
    
    return color;
}

TECHNIQUE(DualTextureWithAlpha, MainVS, MainPS);