sampler uImage0 : register(s0);

static const int lightSize = 8;

float3 uLights[lightSize * lightSize];

float4 main(float4 sampleColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    int i = floor(uv.x * (lightSize - 1));
    int j = floor(uv.y * (lightSize - 1)) * lightSize;
    float3 row = lerp(uLights[i + j], uLights[i + 1 + j], uv.x * (lightSize - 1) - i);
    float3 rowDown = lerp(uLights[i + j + lightSize], uLights[i + j + 1 + lightSize], uv.x * (lightSize - 1) - i);
    float3 color = lerp(row, rowDown, uv.y * (lightSize - 1) - j / lightSize);

    return float4(color, 1) * tex2D(uImage0, uv) * sampleColor;
}

#ifdef FX
technique Technique1
{
    pass CurseFieldPass
    {
        PixelShader = compile ps_3_0 main();
    }
}
#endif // FX
