
Texture2DArray Texture0 : register(t0);

sampler TextureSampler : register(s0);

cbuffer Parameters : register(b0)
{   
    float4x4 WorldViewProjection;
};

struct VertexShaderInput
{
    float4 Position : SV_POSITION;
    uint TexIndex : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 TexCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = mul(input.Position, WorldViewProjection);
    output.TexCoord = float3(input.Position.xz, input.TexIndex);

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = Texture0.SampleLevel(TextureSampler, input.TexCoord, 0);
    
    return color;
}

technique Main
{
    pass P0
    {
        VertexShader = compile vs_4_0 MainVS();
        PixelShader = compile ps_4_0 MainPS();
    }
};