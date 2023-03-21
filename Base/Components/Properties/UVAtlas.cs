using Unity.Entities;
using Unity.Mathematics;

namespace NSprites
{
    /// <summary>
    /// Supposed to use as texture ST to locate actual texture on atlas if used (if not use default value). In shader every float2 UV would be multiplied to value.xy and offsetted with value.zw
    /// like UV * value.xy + value.zw
    /// </summary>
    public struct UVAtlas : IComponentData
    {
        public float4 value;

        public static UVAtlas Default => new() { value = new float4(1f, 1f, 0f, 0f) };
    }
}