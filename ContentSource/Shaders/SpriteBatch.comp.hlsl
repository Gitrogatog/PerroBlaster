struct SpriteComputeData
{
    float3 position;
    float rotation;
    float2 scale;
    float4 colorBlend;
    float4 colorOverlay;
    float2 uv0;
    float2 uv1;
    float2 uv2;
    float2 uv3;
};

struct SpriteVertex
{
    float4 position;
    float2 texcoord;
    float4 colorBlend;
    float4 colorOverlay;
};

StructuredBuffer<SpriteComputeData> ComputeBuffer : register(t0, space0);
RWStructuredBuffer<SpriteVertex> VertexBuffer : register(u0, space1);

[numthreads(64, 1, 1)]
void main(uint3 GlobalInvocationID : SV_DispatchThreadID)
{
    uint n = GlobalInvocationID.x;

    SpriteComputeData currentSpriteData = ComputeBuffer[n];

    float4x4 Scale = float4x4(
        float4(currentSpriteData.scale.x, 0.0f, 0.0f, 0.0f),
        float4(0.0f, currentSpriteData.scale.y, 0.0f, 0.0f),
        float4(0.0f, 0.0f, 1.0f, 0.0f),
        float4(0.0f, 0.0f, 0.0f, 1.0f)
    );

    float c = cos(currentSpriteData.rotation);
    float s = sin(currentSpriteData.rotation);

    float4x4 Rotation = float4x4(
        float4(   c,    s, 0.0f, 0.0f),
        float4(  -s,    c, 0.0f, 0.0f),
        float4(0.0f, 0.0f, 1.0f, 0.0f),
        float4(0.0f, 0.0f, 0.0f, 1.0f)
    );

    float halfScaleX = currentSpriteData.scale.x * 0.5f;
    float halfScaleY = currentSpriteData.scale.y * 0.5f;

    float4x4 Translation = float4x4(
        float4(1.0f, 0.0f, 0.0f, 0.0f),
        float4(0.0f, 1.0f, 0.0f, 0.0f),
        float4(0.0f, 0.0f, 1.0f, 0.0f),
        float4(currentSpriteData.position.x + halfScaleX, currentSpriteData.position.y + halfScaleY, currentSpriteData.position.z, 1.0f)
    );
    float4x4 BackTranslation = float4x4(
        float4(1.0f, 0.0f, 0.0f, 0.0f),
        float4(0.0f, 1.0f, 0.0f, 0.0f),
        float4(0.0f, 0.0f, 1.0f, 0.0f),
        float4(-halfScaleX, -halfScaleY, currentSpriteData.position.z, 1.0f)
    );

    // float4x4 Model = mul(Scale, mul(Translation(mul(Translation, mul(Rotation, NegativeTranslation)))));
    float4x4 Model = mul(Scale, mul(BackTranslation, mul(Rotation, Translation)));

    float4 topLeft = float4(0.0f, 0.0f, 0.0f, 1.0f);
    float4 topRight = float4(1.0f, 0.0f, 0.0f, 1.0f);
    float4 bottomLeft = float4(0.0f, 1.0f, 0.0f, 1.0f);
    float4 bottomRight = float4(1.0f, 1.0f, 0.0f, 1.0f);

    VertexBuffer[n * 4u]    .position = mul(topLeft, Model);
    VertexBuffer[n * 4u + 1].position = mul(topRight, Model);
    VertexBuffer[n * 4u + 2].position = mul(bottomLeft, Model);
    VertexBuffer[n * 4u + 3].position = mul(bottomRight, Model);

    VertexBuffer[n * 4u]    .texcoord = currentSpriteData.uv0;
    VertexBuffer[n * 4u + 1].texcoord = currentSpriteData.uv1;
    VertexBuffer[n * 4u + 2].texcoord = currentSpriteData.uv2;
    VertexBuffer[n * 4u + 3].texcoord = currentSpriteData.uv3;

    VertexBuffer[n * 4u]    .colorBlend = currentSpriteData.colorBlend;
    VertexBuffer[n * 4u + 1].colorBlend = currentSpriteData.colorBlend;
    VertexBuffer[n * 4u + 2].colorBlend = currentSpriteData.colorBlend;
    VertexBuffer[n * 4u + 3].colorBlend = currentSpriteData.colorBlend;

    VertexBuffer[n * 4u]    .colorOverlay = currentSpriteData.colorOverlay;
    VertexBuffer[n * 4u + 1].colorOverlay = currentSpriteData.colorOverlay;
    VertexBuffer[n * 4u + 2].colorOverlay = currentSpriteData.colorOverlay;
    VertexBuffer[n * 4u + 3].colorOverlay = currentSpriteData.colorOverlay;
}
