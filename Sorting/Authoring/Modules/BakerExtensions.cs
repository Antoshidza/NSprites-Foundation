using Unity.Entities;
using UnityEngine;

namespace NSprites
{
    public static partial class BakerExtensions
    {
        /// <summary>
        /// Bakes sorting components for NSprites-Foundation sorting system
        /// </summary>
        public static void BakeSpriteSorting<TAuthoring>(this Baker<TAuthoring> baker, in Entity entity, int sortingIndex, int sortingLayer, bool staticSorting = false)
            where TAuthoring : MonoBehaviour
        {
            if (baker == null)
            {
                Debug.LogError(new NSpritesException($"Passed baker is null"));
                return;
            }

            baker.AddComponent<VisualSortingTag>(entity);
            baker.AddComponent<SortingValue>(entity);
            baker.AddComponent(entity, new SortingIndex { value = sortingIndex });
            baker.AddSharedComponent(entity, new SortingLayer { index = sortingLayer });
            if (staticSorting)
                baker.AddComponent<SortingStaticTag>(entity);
        }
    }
}