Texture2D<float4> Texture : register(t0, space2);
SamplerState Sampler : register(s0, space2);

struct Input
{
    float2 TexCoord : TEXCOORD0;
    float4 ColorBlend : TEXCOORD1;
    float4 ColorOverlay : TEXCOORD2;
};

float4 main(Input input) : SV_Target0
{
    float4 sampledColor = Texture.Sample(Sampler, input.TexCoord);

    if (sampledColor.a < 0.02)
    {
        discard;
    }
    if(input.ColorOverlay.a > 0.02){
        // return float4(1.0f, 1.0f, 1.0f, 1.0f);
        return input.ColorOverlay;
    }
    return sampledColor * input.ColorBlend;
}
