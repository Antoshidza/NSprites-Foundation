using Unity.Entities;
using Unity.Mathematics;

namespace NSprites
{
    /// <summary>
    /// An optional transformation matrix used to implement non-affine
    /// transformation effects such as non-uniform scale.
    /// </summary>
    public struct PostTransformMatrix2D : IComponentData
    {
        /// <summary>
        /// The post-transform scale matrix
        /// </summary>
        public float4x4 Value;
    }
}
