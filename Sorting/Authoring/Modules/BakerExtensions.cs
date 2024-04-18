using System;
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

            if (sortingLayer < 0)
            {
                Debug.LogError(new ArgumentException($"Sorting layer index can't be less then zero, passed: {sortingLayer}"));
                return;
            }
            
            if(sortingLayer >= SpriteSortingSystem.LayerCount)
            {
                Debug.LogError(new ArgumentException(
                    $"Sorting layer index can't be greater or equal to {nameof(SpriteSortingSystem.LayerCount)} defined in {nameof(SpriteSortingSystem)}, passed: {sortingLayer}"));
                return;
            }

            if (sortingIndex < 0)
            {
                Debug.LogError(new ArgumentException($"Sorting index can't be less then zero, passed: {sortingIndex}"));
                return;
            }
            
            if(sortingIndex >= SpriteSortingSystem.SortingIndexCount)
            {
                Debug.LogError(new ArgumentException(
                    $"Sorting index can't be greater or equal to {nameof(SpriteSortingSystem.SortingIndexCount)} defined in {nameof(SpriteSortingSystem)}, passed: {sortingIndex}"));
                return;
            }

            baker.AddComponent(entity, new SortingData(sortingLayer, sortingIndex));
        }
    }
}