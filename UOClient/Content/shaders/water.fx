#include "Macros.fxh"

Texture2D Texture0 : register(t0);
Texture2D Normal : register(t1);

sampler TextureSampler : register(s0);

BEGIN_CONSTANTS

    int Texture0Stretch;
    int NormalStretch;
    int TextureIndex;
    
    float WaveHeight;
    float2 WindDirection;
    float WindForce;
    float Time;
    float2 Center;
    
    float4 DiffuseColor _vs(c0) _ps(c1) _cb(c0);
    float3 EmissiveColor _vs(c1) _ps(c2) _cb(c1);
    float3 SpecularColor _vs(c2) _ps(c3) _cb(c2);
    float SpecularPower _vs(c3) _ps(c4) _cb(c2.w);

    float3 DirLight0Direction _vs(c4) _ps(c5) _cb(c3);
    float3 DirLight0DiffuseColor _vs(c5) _ps(c6) _cb(c4);
    float3 DirLight0SpecularColor _vs(c6) _ps(c7) _cb(c5);

    float3 DirLight1Direction _vs(c7) _ps(c8) _cb(c6);
    float3 DirLight1DiffuseColor _vs(c8) _ps(c9) _cb(c7);
    float3 DirLight1SpecularColor _vs(c9) _ps(c10) _cb(c8);

    float3 DirLight2Direction _vs(c10) _ps(c11) _cb(c9);
    float3 DirLight2DiffuseColor _vs(c11) _ps(c12) _cb(c10);
    float3 DirLight2SpecularColor _vs(c12) _ps(c13) _cb(c11);

    float3 EyePosition _vs(c13) _ps(c14) _cb(c12);

    float3 FogColor _ps(c0) _cb(c13);
    float4 FogVector _vs(c14) _cb(c14);

    float4x4 World _vs(c19) _cb(c15);
    float3x3 WorldInverseTranspose _vs(c23) _cb(c19);

MATRIX_CONSTANTS

    float4x4 WorldViewProj _vs(c15) _cb(c0);

END_CONSTANTS


#include "structures.fxh"
#include "common.fxh"
#include "lighting.fxh"

struct VSInputTxArray
{
    float4 Position : SV_Position;
    uint TexIndex : TEXCOORD0;
};

struct VSInputNmTxVcArray
{
    float4 Position : SV_Position;
    float3 Normal : NORMAL;
    uint TexIndex : TEXCOORD0;
    float4 Color : COLOR;
};

struct VSInputNmTxArray
{
    float4 Position : SV_Position;
    float3 Normal : NORMAL;
    uint TexIndex : TEXCOORD0;
};

struct VSInputTxVcArray
{
    float4 Position : SV_Position;
    uint TexIndex : TEXCOORD0;
    float4 Color : COLOR;
};

struct VSOutputTxArray
{
    float4 PositionPS : SV_Position;
    float4 Diffuse : COLOR0;
    float4 Specular : COLOR1;
    float4 TexCoord : TEXCOORD0;
    float AlphaMask : TEXCOORD1;
};

struct VSOutputTxArrayNoFog
{
    float4 PositionPS : SV_Position;
    float4 Diffuse : COLOR0;
    float4 TexCoord : TEXCOORD0;
    float AlphaMask : TEXCOORD1;
};

struct VSOutputPixelLightingTxArray
{
    float4 PositionPS : SV_Position;
    float4 TexCoord : TEXCOORD0;
    float AlphaMask : TEXCOORD1;
    float4 PositionWS : TEXCOORD2;
    float3 NormalWS : TEXCOORD3;
    float4 Diffuse : COLOR0;
};

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

float2 GetText0CoordFollowCenter(float4 position)
{
    return ((Center - position.xz) / Texture0Stretch) - 0.5;
}

float2 GetText0Coord(float4 position)
{
    return position.xz / Texture0Stretch;
}

float GetAlphaMask(int textId)
{
    return TextureIndex <= textId;
}

// Vertex shader: basic.
VSOutput VSBasic(VSInput vin)
{
    VSOutput vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    return vout;
}


// Vertex shader: no fog.
VSOutputNoFog VSBasicNoFog(VSInput vin)
{
    VSOutputNoFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsNoFog;
    
    return vout;
}


// Vertex shader: vertex color.
VSOutput VSBasicVc(VSInputVc vin)
{
    VSOutput vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: vertex color, no fog.
VSOutputNoFog VSBasicVcNoFog(VSInputVc vin)
{
    VSOutputNoFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsNoFog;
    
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: texture.
VSOutputTxArray VSBasicTx(VSInputTxArray vin)
{
    VSOutputTxArray vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.TexCoord.xy = GetText0Coord(vin.Position);
    vout.TexCoord.zw = GetNormalTextCoord(vin.Position);
    vout.AlphaMask = GetAlphaMask(vin.TexIndex);

    return vout;
}


// Vertex shader: texture, no fog.
VSOutputTxArrayNoFog VSBasicTxNoFog(VSInputTxArray vin)
{
    VSOutputTxArrayNoFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsNoFog;
    
    vout.TexCoord.xy = GetText0Coord(vin.Position);
    vout.TexCoord.zw = GetNormalTextCoord(vin.Position);
    vout.AlphaMask = GetAlphaMask(vin.TexIndex);
    
    return vout;
}


// Vertex shader: texture, no fog, follow center.
VSOutputTxArrayNoFog VSBasicTxNoFogFollowCenter(VSInputTxArray vin)
{
    VSOutputTxArrayNoFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsNoFog;
    
    vout.TexCoord.xy = GetText0CoordFollowCenter(vin.Position);
    vout.TexCoord.zw = GetNormalTextCoord(vin.Position);
    vout.AlphaMask = GetAlphaMask(vin.TexIndex);
    
    return vout;
}

// Vertex shader: texture + vertex color.
VSOutputTxArray VSBasicTxVc(VSInputTxVcArray vin)
{
    VSOutputTxArray vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.TexCoord.xy = GetText0Coord(vin.Position);
    vout.TexCoord.zw = GetNormalTextCoord(vin.Position);
    vout.AlphaMask = GetAlphaMask(vin.TexIndex);
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: texture + vertex color, no fog.
VSOutputTxArrayNoFog VSBasicTxVcNoFog(VSInputTxVcArray vin)
{
    VSOutputTxArrayNoFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsNoFog;
    
    vout.TexCoord.xy = GetText0Coord(vin.Position);
    vout.TexCoord.zw = GetNormalTextCoord(vin.Position);
    vout.AlphaMask = GetAlphaMask(vin.TexIndex);
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: vertex lighting.
VSOutput VSBasicVertexLighting(VSInputNm vin)
{
    VSOutput vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 3);
    SetCommonVSOutputParams;
    
    return vout;
}


// Vertex shader: vertex lighting + vertex color.
VSOutput VSBasicVertexLightingVc(VSInputNmVc vin)
{
    VSOutput vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 3);
    SetCommonVSOutputParams;
    
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: vertex lighting + texture.
VSOutputTxArray VSBasicVertexLightingTx(VSInputNmTxArray vin)
{
    VSOutputTxArray vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 3);
    SetCommonVSOutputParams;
    
    vout.TexCoord.xy = GetText0Coord(vin.Position);
    vout.TexCoord.zw = GetNormalTextCoord(vin.Position);
    vout.AlphaMask = GetAlphaMask(vin.TexIndex);
    
    return vout;
}


// Vertex shader: vertex lighting + texture + vertex color.
VSOutputTxArray VSBasicVertexLightingTxVc(VSInputNmTxVcArray vin)
{
    VSOutputTxArray vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 3);
    SetCommonVSOutputParams;
    
    vout.TexCoord.xy = GetText0Coord(vin.Position);
    vout.TexCoord.zw = GetNormalTextCoord(vin.Position);
    vout.AlphaMask = GetAlphaMask(vin.TexIndex);
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: one light.
VSOutput VSBasicOneLight(VSInputNm vin)
{
    VSOutput vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 1);
    SetCommonVSOutputParams;
    
    return vout;
}


// Vertex shader: one light + vertex color.
VSOutput VSBasicOneLightVc(VSInputNmVc vin)
{
    VSOutput vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 1);
    SetCommonVSOutputParams;
    
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: one light + texture.
VSOutputTxArray VSBasicOneLightTx(VSInputNmTxArray vin)
{
    VSOutputTxArray vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 1);
    SetCommonVSOutputParams;
    
    vout.TexCoord.xy = GetText0Coord(vin.Position);
    vout.TexCoord.zw = GetNormalTextCoord(vin.Position);
    vout.AlphaMask = GetAlphaMask(vin.TexIndex);
    
    return vout;
}


// Vertex shader: one light + texture + vertex color.
VSOutputTxArray VSBasicOneLightTxVc(VSInputNmTxVcArray vin)
{
    VSOutputTxArray vout;
    
    CommonVSOutput cout = ComputeCommonVSOutputWithLighting(vin.Position, vin.Normal, 1);
    SetCommonVSOutputParams;
    
    vout.TexCoord.xy = GetText0Coord(vin.Position);
    vout.TexCoord.zw = GetNormalTextCoord(vin.Position);
    vout.AlphaMask = GetAlphaMask(vin.TexIndex);
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: pixel lighting.
VSOutputPixelLighting VSBasicPixelLighting(VSInputNm vin)
{
    VSOutputPixelLighting vout;
    
    CommonVSOutputPixelLighting cout = ComputeCommonVSOutputPixelLighting(vin.Position, vin.Normal);
    SetCommonVSOutputParamsPixelLighting;

    vout.Diffuse = float4(1, 1, 1, DiffuseColor.a);
    
    return vout;
}


// Vertex shader: pixel lighting + vertex color.
VSOutputPixelLighting VSBasicPixelLightingVc(VSInputNmVc vin)
{
    VSOutputPixelLighting vout;
    
    CommonVSOutputPixelLighting cout = ComputeCommonVSOutputPixelLighting(vin.Position, vin.Normal);
    SetCommonVSOutputParamsPixelLighting;
    
    vout.Diffuse.rgb = vin.Color.rgb;
    vout.Diffuse.a = vin.Color.a * DiffuseColor.a;
    
    return vout;
}


// Vertex shader: pixel lighting + texture.
VSOutputPixelLightingTxArray VSBasicPixelLightingTx(VSInputNmTxArray vin)
{
    VSOutputPixelLightingTxArray vout;
    
    CommonVSOutputPixelLighting cout = ComputeCommonVSOutputPixelLighting(vin.Position, vin.Normal);
    SetCommonVSOutputParamsPixelLighting;
    
    vout.Diffuse = float4(1, 1, 1, DiffuseColor.a);
    vout.TexCoord.xy = GetText0Coord(vin.Position);
    vout.TexCoord.zw = GetNormalTextCoord(vin.Position);
    vout.AlphaMask = GetAlphaMask(vin.TexIndex);

    return vout;
}


// Vertex shader: pixel lighting + texture + vertex color.
VSOutputPixelLightingTxArray VSBasicPixelLightingTxVc(VSInputNmTxVcArray vin)
{
    VSOutputPixelLightingTxArray vout;
    
    CommonVSOutputPixelLighting cout = ComputeCommonVSOutputPixelLighting(vin.Position, vin.Normal);
    SetCommonVSOutputParamsPixelLighting;
    
    vout.Diffuse.rgb = vin.Color.rgb;
    vout.Diffuse.a = vin.Color.a * DiffuseColor.a;
    vout.TexCoord.xy = GetText0Coord(vin.Position);
    vout.TexCoord.zw = GetNormalTextCoord(vin.Position);
    vout.AlphaMask = GetAlphaMask(vin.TexIndex);
    
    return vout;
}


// Pixel shader: basic.
float4 PSBasic(VSOutput pin) : SV_Target0
{
    float4 color = pin.Diffuse;
    
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: no fog.
float4 PSBasicNoFog(VSOutputNoFog pin) : SV_Target0
{
    return pin.Diffuse;
}


// Pixel shader: texture.
float4 PSBasicTx(VSOutputTxArray pin) : SV_Target0
{
    float4 color = Texture0.SampleLevel(TextureSampler, pin.TexCoord.xy, 0) * pin.Diffuse;
    
    ApplyFog(color, pin.Specular.w);
    
    return color;
}

// Pixel shader: texture, no fog.
float4 PSBasicTxNoFog(VSOutputTxArrayNoFog pin) : SV_Target0
{
    float4 normal = Normal.SampleLevel(TextureSampler, pin.TexCoord.zw, 0);
    float2 perturbation = WaveHeight * (normal.rg - 0.5f) * 2.0f;
    float2 perturbatedTexCoords = pin.TexCoord.xy + perturbation;
    
    float4 color = Texture0.SampleLevel(TextureSampler, perturbatedTexCoords, 0);
    color = lerp(color, 0, 1 - pin.AlphaMask);
    color.a = pin.AlphaMask;
    
    return color * pin.Diffuse;
}


// Pixel shader: vertex lighting.
float4 PSBasicVertexLighting(VSOutput pin) : SV_Target0
{
    float4 color = pin.Diffuse;

    AddSpecular(color, pin.Specular.rgb);
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: vertex lighting, no fog.
float4 PSBasicVertexLightingNoFog(VSOutput pin) : SV_Target0
{
    float4 color = pin.Diffuse;
    
    AddSpecular(color, pin.Specular.rgb);
    
    return color;
}


// Pixel shader: vertex lighting + texture.
float4 PSBasicVertexLightingTx(VSOutputTxArray pin) : SV_Target0
{
    float4 color = Texture0.SampleLevel(TextureSampler, pin.TexCoord.xy, 0) * pin.Diffuse;
    
    AddSpecular(color, pin.Specular.rgb);
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: vertex lighting + texture, no fog.
float4 PSBasicVertexLightingTxNoFog(VSOutputTxArray pin) : SV_Target0
{
    float4 color = Texture0.SampleLevel(TextureSampler, pin.TexCoord.xy, 0) * pin.Diffuse;
    
    AddSpecular(color, pin.Specular.rgb);
    
    return color;
}


// Pixel shader: pixel lighting.
float4 PSBasicPixelLighting(VSOutputPixelLighting pin) : SV_Target0
{
    float4 color = pin.Diffuse;

    float3 eyeVector = normalize(EyePosition - pin.PositionWS.xyz);
    float3 worldNormal = normalize(pin.NormalWS);
    
    ColorPair lightResult = ComputeLights(eyeVector, worldNormal, 3);

    color.rgb *= lightResult.Diffuse;
    
    AddSpecular(color, lightResult.Specular);
    ApplyFog(color, pin.PositionWS.w);
    
    return color;
}


// Pixel shader: pixel lighting + texture.
float4 PSBasicPixelLightingTx(VSOutputPixelLightingTxArray pin) : SV_Target0
{
    float4 color = Texture0.SampleLevel(TextureSampler, pin.TexCoord.xy, 0) * pin.Diffuse;
    
    float3 eyeVector = normalize(EyePosition - pin.PositionWS.xyz);
    float3 worldNormal = normalize(pin.NormalWS);
    
    ColorPair lightResult = ComputeLights(eyeVector, worldNormal, 3);
    
    color.rgb *= lightResult.Diffuse;

    AddSpecular(color, lightResult.Specular);
    ApplyFog(color, pin.PositionWS.w);
    
    return color;
}


// NOTE: The order of the techniques here are
// defined to match the indexing in BasicEffect.cs.

TECHNIQUE(BasicEffect, VSBasic, PSBasic);
TECHNIQUE(BasicEffect_NoFog, VSBasicNoFog, PSBasicNoFog);
TECHNIQUE(BasicEffect_VertexColor, VSBasicVc, PSBasic);
TECHNIQUE(BasicEffect_VertexColor_NoFog, VSBasicVcNoFog, PSBasicNoFog);
TECHNIQUE(BasicEffect_Texture, VSBasicTx, PSBasicTx);
TECHNIQUE(BasicEffect_Texture_NoFog, VSBasicTxNoFog, PSBasicTxNoFog);
TECHNIQUE(BasicEffect_Texture_VertexColor, VSBasicTxVc, PSBasicTx);
TECHNIQUE(BasicEffect_Texture_VertexColor_NoFog, VSBasicTxVcNoFog, PSBasicTxNoFog);

TECHNIQUE(BasicEffect_VertexLighting, VSBasicVertexLighting, PSBasicVertexLighting);
TECHNIQUE(BasicEffect_VertexLighting_NoFog, VSBasicVertexLighting, PSBasicVertexLightingNoFog);
TECHNIQUE(BasicEffect_VertexLighting_VertexColor, VSBasicVertexLightingVc, PSBasicVertexLighting);
TECHNIQUE(BasicEffect_VertexLighting_VertexColor_NoFog, VSBasicVertexLightingVc, PSBasicVertexLightingNoFog);
TECHNIQUE(BasicEffect_VertexLighting_Texture, VSBasicVertexLightingTx, PSBasicVertexLightingTx);
TECHNIQUE(BasicEffect_VertexLighting_Texture_NoFog, VSBasicVertexLightingTx, PSBasicVertexLightingTxNoFog);
TECHNIQUE(BasicEffect_VertexLighting_Texture_VertexColor, VSBasicVertexLightingTxVc, PSBasicVertexLightingTx);
TECHNIQUE(BasicEffect_VertexLighting_Texture_VertexColor_NoFog, VSBasicVertexLightingTxVc, PSBasicVertexLightingTxNoFog);

TECHNIQUE(BasicEffect_OneLight, VSBasicOneLight, PSBasicVertexLighting);
TECHNIQUE(BasicEffect_OneLight_NoFog, VSBasicOneLight, PSBasicVertexLightingNoFog);
TECHNIQUE(BasicEffect_OneLight_VertexColor, VSBasicOneLightVc, PSBasicVertexLighting);
TECHNIQUE(BasicEffect_OneLight_VertexColor_NoFog, VSBasicOneLightVc, PSBasicVertexLightingNoFog);
TECHNIQUE(BasicEffect_OneLight_Texture, VSBasicOneLightTx, PSBasicVertexLightingTx);
TECHNIQUE(BasicEffect_OneLight_Texture_NoFog, VSBasicOneLightTx, PSBasicVertexLightingTxNoFog);
TECHNIQUE(BasicEffect_OneLight_Texture_VertexColor, VSBasicOneLightTxVc, PSBasicVertexLightingTx);
TECHNIQUE(BasicEffect_OneLight_Texture_VertexColor_NoFog, VSBasicOneLightTxVc, PSBasicVertexLightingTxNoFog);

TECHNIQUE(BasicEffect_PixelLighting, VSBasicPixelLighting, PSBasicPixelLighting);
TECHNIQUE(BasicEffect_PixelLighting_NoFog, VSBasicPixelLighting, PSBasicPixelLighting);
TECHNIQUE(BasicEffect_PixelLighting_VertexColor, VSBasicPixelLightingVc, PSBasicPixelLighting);
TECHNIQUE(BasicEffect_PixelLighting_VertexColor_NoFog, VSBasicPixelLightingVc, PSBasicPixelLighting);
TECHNIQUE(BasicEffect_PixelLighting_Texture, VSBasicPixelLightingTx, PSBasicPixelLightingTx);
TECHNIQUE(BasicEffect_PixelLighting_Texture_NoFog, VSBasicPixelLightingTx, PSBasicPixelLightingTx);
TECHNIQUE(BasicEffect_PixelLighting_Texture_VertexColor, VSBasicPixelLightingTxVc, PSBasicPixelLightingTx);
TECHNIQUE(BasicEffect_PixelLighting_Texture_VertexColor_NoFog, VSBasicPixelLightingTxVc, PSBasicPixelLightingTx);

TECHNIQUE(BasicEffect_FollowCenter, VSBasic, PSBasic);
TECHNIQUE(BasicEffect_FollowCenter_NoFog, VSBasicNoFog, PSBasicNoFog);
TECHNIQUE(BasicEffect_FollowCenter_VertexColor, VSBasicVc, PSBasic);
TECHNIQUE(BasicEffect_FollowCenter_VertexColor_NoFog, VSBasicVcNoFog, PSBasicNoFog);
TECHNIQUE(BasicEffect_FollowCenter_Texture, VSBasicTx, PSBasicTx);
TECHNIQUE(BasicEffect_FollowCenter_Texture_NoFog, VSBasicTxNoFogFollowCenter, PSBasicTxNoFog);
TECHNIQUE(BasicEffect_FollowCenter_Texture_VertexColor, VSBasicTxVc, PSBasicTx);
TECHNIQUE(BasicEffect_FollowCenter_Texture_VertexColor_NoFog, VSBasicTxVcNoFog, PSBasicTxNoFog);