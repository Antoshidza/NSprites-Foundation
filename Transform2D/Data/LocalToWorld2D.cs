using Unity.Entities;
using Unity.Mathematics;
using quaternion = Unity.Mathematics.quaternion;
using static Unity.Mathematics.math;

namespace NSprites
{
    /// <summary>
    /// Only used for rendering, do not touch change
    /// </summary>
    public struct LocalToWorld2D : IComponentData
    {
        public float4x4 Value;

        public float2     Right    => float2(Value.c0.x, Value.c0.y);
        public float2     Up       => float2(Value.c1.x, Value.c1.y);
        public float2     Position => float2(Value.c3.x, Value.c3.y);
        public float2     Scale    => float2(length(Value.c0), length(Value.c1));
        public quaternion Rotation => new(orthonormalize(new float3x3(Value)));
    }
}
