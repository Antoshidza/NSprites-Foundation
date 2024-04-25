using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace NSprites
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public partial struct SpriteSortingSystem : ISystem
    {
        private static readonly int SortingGlobalData = Shader.PropertyToID("_sortingGlobalData");

        public const int LayerCount = 4;
        public const int SortingIndexCount = 5;

        private const float PerLayerOffset = 1f / LayerCount;
        private const float PerSortingIndexOffset = PerLayerOffset / SortingIndexCount;

        public void OnCreate(ref SystemState state) 
            => Shader.SetGlobalVector(SortingGlobalData, new Vector4(PerLayerOffset, PerSortingIndexOffset, default, default));
    }
}

