using System;
using Unity.Entities;
using UnityEngine;

namespace NSprites
{
    /// <summary>
    /// Bakes sorting components for NSprites-Foundation sorting system
    /// </summary>
    [Serializable]
    public struct SortingAuthoringModule
    {
        [SerializeField] public bool StaticSorting;
        [SerializeField] public int SortingIndex;
        [SortingLayer] [SerializeField] public int SortingLayer;

        public void Bake<TAuthoring>(Baker<TAuthoring> baker)
            where TAuthoring : MonoBehaviour
        {
            baker.BakeSpriteSorting
            (
                baker.GetEntity(TransformUsageFlags.None),
                SortingIndex,
                UnityEngine.SortingLayer.GetLayerValueFromID(SortingLayer),
                StaticSorting
            );
        }
    }
}