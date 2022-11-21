
Texture2D Texture0 : register(t0);

static const float3 TileTranslation = float3(0.5f, 0, 1.5f);

sampler TextureSampler : register(s0) = sampler_state
{
    MinFilter = POINT;
    MagFilter = POINT;
};

cbuffer Parameters : register(b0)
{
    float2 TextureSize;
    float2 Planes;
    float3x3 Rotation;
    float4x4 WorldViewProjection;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Bounds : TEXCOORD0;
    float DepthOffset : TEXCOORD1;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 TexCoord : TEXCOORD0;
};

struct PixelShaderOutput
{
    float4 Color : SV_TARGET;
    float Depth : SV_DEPTH;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    float3 billboard = mul(float3(input.Bounds.xy, 0), Rotation);
    float4 translatedPosition = float4(input.Position.xyz + TileTranslation + billboard, input.Position.w);
    
    float depth = mul(float4(input.Position.xyz + TileTranslation + float3(0, input.DepthOffset, 0), input.Position.w), WorldViewProjection).z;
    
    output.Position = mul(translatedPosition, WorldViewProjection);
    output.TexCoord = float3(input.Bounds.zw / TextureSize, depth);

    return output;
}

PixelShaderOutput MainPS(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput) 0;
    
    float4 color = Texture0.SampleLevel(TextureSampler, input.TexCoord.xy, 0);
    clip(color.a - 0.0001);
    
    output.Color = color;
    output.Depth = input.TexCoord.z;
    
    return output;
}

PixelShaderOutput MainPS_Transparent(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput) 0;
    
    float4 color = Texture0.SampleLevel(TextureSampler, input.TexCoord.xy, 0);
    clip(0.5 - color.a);
    
    color.rgb *= color.a * 2;
    output.Color = color;
    
    return output;
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