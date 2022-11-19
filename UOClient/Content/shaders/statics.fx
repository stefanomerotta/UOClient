
Texture2D Texture0 : register(t0);

sampler TextureSampler : register(s0) = sampler_state
{
    MinFilter = POINT;
    MagFilter = POINT;
};

cbuffer Parameters : register(b0)
{
    float2 TextureSize;
    float3x3 Rotation;
    float4x4 WorldViewProjection;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Bounds : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    float3 billboard = mul(float3(input.Bounds.xy, 0), Rotation);
    float4 translatedPosition = float4(input.Position.xyz + billboard, input.Position.w);
    
    output.Position = mul(translatedPosition, WorldViewProjection);
    output.TexCoord = input.Bounds.zw / TextureSize;

    return output;
}

float4 MainPS(VertexShaderOutput input) : SV_TARGET
{
    float4 color = Texture0.SampleLevel(TextureSampler, input.TexCoord.xy, 0);
    clip(color.a - 0.5);
    color.rgb *= color.a;
        
    return color;
}

float4 MainPS_Transparent(VertexShaderOutput input) : SV_TARGET
{
    float4 color = Texture0.SampleLevel(TextureSampler, input.TexCoord.xy, 0);
    //clip(1.5 - color.a);
    //color.rgb *= color.a;
    //color.r = 1;
    //color.a = 0.5;
    //color.a = 1;
    clip(0.5 - color.a);
    color.r = 1;
    
    return color;
}

technique Statics
{
    pass P0
    {
        VertexShader = compile vs_4_0 MainVS();
        PixelShader = compile ps_4_0 MainPS();
    }

    pass P1
    {
        VertexShader = compile vs_4_0 MainVS();
        PixelShader = compile ps_4_0 MainPS_Transparent();
    }
};