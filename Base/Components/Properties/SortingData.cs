using Unity.Entities;
using Unity.Mathematics;

namespace NSprites
{
    public struct SortingData : IComponentData
    {
        public int2 Value;

        public int Layer => Value.x;
        public int SortingIndex => Value.y;
        
        public SortingData(in int layer, in int sortingIndex) 
            => Value = new int2(layer, sortingIndex);
    }
}