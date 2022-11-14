
Texture2D Texture0 : register(t0);
Texture2D Normal : register(t1);

sampler TextureSampler : register(s0);

cbuffer Parameters : register(b0)
{
    int Texture0Stretch;
    int NormalStretch;
    int TextureIndex;
    
    float WaveHeight;
    float2 WindDirection;
    float WindForce;
    float Time;
    float2 Center;
    
    float4x4 WorldViewProjection;
};

struct VertexShaderInput
{
    float4 Position : SV_Position;
    uint TexIndex : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_Position;
    float4 TexCoord : TEXCOORD0;
    float AlphaMask : TEXCOORD1;
};

float2 GetText0Coord(float4 position)
{
    return position.xz / Texture0Stretch;
}

float2 GetText0CoordFollowCenter(float4 position)
{
    return ((Center - position.xz) / Texture0Stretch) - 0.5;
}

float2 GetNormalTextCoord(float4 position)
{
    float2 windDirection = normalize(WindDirection);
    float3 perpDirection = cross(float3(windDirection, 0), float3(0, 0, 1));
    
    float xDot = dot(position.xz, perpDirection.xy);
    float yDot = dot(position.xz, windDirection);
    
    yDot -= Time * WindForce;
    
    float normalX = xDot / NormalStretch;
    float normalY = yDot / NormalStretch;
    
    return float2(normalX, normalY);
}

float GetAlphaMask(int textId)
{
    return TextureIndex <= textId;
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = mul(input.Position, WorldViewProjection);
    output.TexCoord.xy = GetText0Coord(input.Position);
    output.TexCoord.zw = GetNormalTextCoord(input.Position);
    output.AlphaMask = GetAlphaMask(input.TexIndex);
    
    return output;
}

VertexShaderOutput MainVSFollowCenter(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = mul(input.Position, WorldViewProjection);
    output.TexCoord.xy = GetText0CoordFollowCenter(input.Position);
    output.TexCoord.zw = GetNormalTextCoord(input.Position);
    output.AlphaMask = GetAlphaMask(input.TexIndex);
    
    return output;
}

float4 MainPS(VertexShaderOutput input) : SV_TARGET
{
    float4 normal = Normal.SampleLevel(TextureSampler, input.TexCoord.zw, 0);
    float2 perturbation = WaveHeight * (normal.rg - 0.5f) * 2.0f;
    float2 perturbatedTexCoords = input.TexCoord.xy + perturbation;
    
    float4 color = Texture0.SampleLevel(TextureSampler, perturbatedTexCoords, 0);
    color = lerp(color, 0, 1 - input.AlphaMask);
    color.a = input.AlphaMask;
    
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

technique FollowCenter
{
    pass P0
    {
        VertexShader = compile vs_4_0 MainVSFollowCenter();
        PixelShader = compile ps_4_0 MainPS();
    }
};