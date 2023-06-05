using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Entities;
using UnityEngine;

namespace NSprites.Authoring
{
    /// <summary>
    /// Register <see cref="SpriteRenderData"/> by creating new <see cref="Material"/> with overrided <see cref="Texture2D"/> from <see cref="Sprite"/>
    /// </summary>
    [Serializable]
    public struct RegisterSpriteAuthoringModule
    {
        [SerializeField] public SpriteRenderData SpriteRenderData;

        private static readonly Dictionary<Texture, Material> OverridedMaterials = new();
        private static readonly int MainTexPropertyID = Shader.PropertyToID("_MainTex");

        public bool IsValid(out string message)
        {
            if (SpriteRenderData.Material == null)
            {
                message = $"{nameof(SpriteRenderData.Material)} is null";
                return false;
            }

            if (SpriteRenderData.PropertiesSet == null)
            {
                message = $"{nameof(SpriteRenderData.PropertiesSet)} is null";
                return false;
            }

            message = string.Empty;
            return true;
        }

        public void Bake<TAuthoring>(Baker<TAuthoring> baker, Texture2D overrideTexture = null)
            where TAuthoring : Component =>
            baker.BakeSpriteBase(GetRenderData(overrideTexture));
        
        private SpriteRenderData GetRenderData([CanBeNull] Texture overrideTexture = null)
        {
            if (overrideTexture == null)
                return SpriteRenderData;
            
            if (SpriteRenderData.Material == null)
            {
                Debug.LogException(new NSpritesException($"{nameof(SpriteRenderData.Material)} is null"));
                return default;
            }

            return overrideTexture == null
                ? SpriteRenderData
                // create new instance with overrided material
                : new()
                {
                    Material = GetOrCreateOverridedMaterial(overrideTexture),
                    PropertiesSet = SpriteRenderData.PropertiesSet
                };
        }

        private Material GetOrCreateOverridedMaterial(Texture texture)
        {
            if (!OverridedMaterials.TryGetValue(texture, out var material))
                material = CreateOverridedMaterial(texture);
#if UNITY_EDITOR // for SubScene + domain reload
            else if (material == null)
            {
                _ = OverridedMaterials.Remove(texture);
                material = CreateOverridedMaterial(texture);
            }
#endif
            return material;
        }
        
        private Material CreateOverridedMaterial(Texture texture)
        {
            var material = new Material(SpriteRenderData.Material);
            material.SetTexture(MainTexPropertyID, texture);
            OverridedMaterials.Add(texture, material);
            return material;
        }
    }
}