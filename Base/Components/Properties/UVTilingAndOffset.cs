using Unity.Entities;
using Unity.Mathematics;

namespace NSprites
{
    /// <summary>
    /// Supposed to use as texture ST to perform tiling and offset which will be repeted inside texture located with <see cref="UVAtlas"/>.
    /// In shader every float2 UV would be multiplied to value.xy and offsetted with value.zw like UV * value.xy + value.zw.
    /// </summary>
    public struct UVTilingAndOffset : IComponentData
    {
        public float4 value;

        public static UVTilingAndOffset Default => new() { value = new float4(1f, 1f, 0f, 0f) };
    }
}
