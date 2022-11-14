
Texture2D Texture0 : register(t0);
Texture2D Texture1 : register(t1);
Texture2D AlphaMask : register(t2);

sampler TextureSampler : register(s0);

cbuffer Parameters : register(b0)
{
    int Texture0Stretch;
    int Texture1Stretch;
    int AlphaMaskStretch;
    int TextureIndex;
    
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
    float4 TexCoord : TEXCOORD0;
    float3 AlphaMaskCoord : TEXCOORD1;
};

float4 GetMainTextCoord(float4 position)
{
    float2 xy = position.xz / Texture0Stretch;
    float2 zw = position.xz / Texture1Stretch;
    
    return float4(xy, zw);
}

float3 GetAlphaTextCoord(float4 position, int textId)
{
    float2 xy = position.xz / AlphaMaskStretch;
    bool a = TextureIndex <= textId;
    
    return float3(xy, a);
}

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = mul(input.Position, WorldViewProjection);
    output.TexCoord = GetMainTextCoord(input.Position);
    output.AlphaMaskCoord = GetAlphaTextCoord(input.Position, input.TexIndex);

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color0 = Texture0.SampleLevel(TextureSampler, input.TexCoord.xy, 0);
    float4 color1 = Texture1.SampleLevel(TextureSampler, input.TexCoord.zw, 0);
    float4 alpha = AlphaMask.SampleLevel(TextureSampler, input.AlphaMaskCoord.xy, 0);
    
    float4 color = lerp(color0, color1, alpha.a);
    
    color = lerp(color, 0, 1 - input.AlphaMaskCoord.z);
    color.a = input.AlphaMaskCoord.z;
    
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