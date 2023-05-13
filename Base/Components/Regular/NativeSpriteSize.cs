using Unity.Entities;
using Unity.Mathematics;

namespace NSprites
{
    public struct NativeSpriteSize : IComponentData
    {
        public float2 Value;
    }
}