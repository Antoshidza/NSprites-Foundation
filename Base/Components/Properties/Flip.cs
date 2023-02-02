using Unity.Entities;
using Unity.Mathematics;

namespace NSprites
{
    public struct Flip : IComponentData
    {
        public int2 Value;
    }
}