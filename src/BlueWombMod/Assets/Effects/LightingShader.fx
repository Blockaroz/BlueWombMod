sampler uImage0 : register(s0);

float4 uColor;

matrix uTransformMatrix;

struct VertexShaderInput
{
    float2 Coord : TEXCOORD0;
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float2 Coord : TEXCOORD0;
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

VertexShaderOutput vertex(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    output.Color = input.Color;
    output.Coord = input.Coord;
    output.Position = mul(input.Position, uTransformMatrix);
    return output;
}

float4 main(in VertexShaderOutput input) : COLOR0
{
    return input.Color * uColor * tex2D(uImage0, input.Coord);
}

#ifdef FX
technique Technique1
{
    pass CurseFieldPass
    {
        VertexShader = compile vs_3_0 vertex();
        PixelShader = compile ps_3_0 main();
    }
}
#endif // FX
