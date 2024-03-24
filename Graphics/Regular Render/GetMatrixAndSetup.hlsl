#if defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) || defined(UNITY_STEREO_INSTANCING_ENABLED)
StructuredBuffer<int> _propertyPointers;
StructuredBuffer<float4> _uvTilingAndOffsetBuffer;
StructuredBuffer<float4> _uvAtlasBuffer;
StructuredBuffer<float> _sortingValueBuffer;
StructuredBuffer<float4x4> _positionBuffer;
StructuredBuffer<float2> _pivotBuffer;
StructuredBuffer<float2> _heightWidthBuffer;
StructuredBuffer<int2> _flipBuffer;
#endif

float4x4 offset_matrix(const float2 input, const float2 scale)
{
    return float4x4(
        scale.x,0,0,scale.x * -input.x,
        0,scale.y,0,scale.y * -input.y,
        0,0,1,0,
        0,0,0,1
    );
}

void setup()
{
#if defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) || defined(UNITY_STEREO_INSTANCING_ENABLED)
    int propertyIndex = _propertyPointers[unity_InstanceID];
    float4x4 transform = _positionBuffer[propertyIndex];
    float2 pivot = _pivotBuffer[propertyIndex];
    float2 scale = _heightWidthBuffer[propertyIndex];
    unity_ObjectToWorld = mul(transform, offset_matrix(pivot, scale));
#endif
}

void PropertyPointer_float(in float instanceID, out float index)
{
#if defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) || defined(UNITY_STEREO_INSTANCING_ENABLED)
    index = _propertyPointers[(uint)instanceID];
#else
    index = 0;
#endif
}

void UV_float(in float index, out float4 uv)
{
    #if defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) || defined(UNITY_STEREO_INSTANCING_ENABLED)
    uv = _uvAtlasBuffer[(uint)index];
    #else
    uv = (float4)0;
    #endif
}

void InstancingSetup_float(in float3 IN, out float3 OUT)
{
    OUT = IN;
}