
Texture2D Texture0 : register(t0);

sampler TextureSampler : register(s0) = sampler_state
{
    MinFilter = POINT;
    MagFilter = POINT;
};

cbuffer Parameters : register(b0)
{
    float2 TextureSize;
    
    float4x4 WorldViewProjection;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 TexCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = mul(input.Position, WorldViewProjection);
    output.TexCoord = float3(input.TexCoord.xy / TextureSize, input.TexCoord.z);

    return output;
}

float4 MainPS(VertexShaderOutput input) : SV_TARGET
{
    float4 color = Texture0.SampleLevel(TextureSampler, input.TexCoord.xy, 0);
    clip(color.a - 1);
    color.rgb *= color.a;
    //color.a = 0.5;
    
    return color;
}

technique Statics
{
    pass P0
    {
        VertexShader = compile vs_4_0 MainVS();
        PixelShader = compile ps_4_0 MainPS();
    }
};