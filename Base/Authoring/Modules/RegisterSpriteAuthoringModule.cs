using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace NSprites.Modules
{
    /// <summary>
    /// Register <see cref="SpriteRenderData"/> by creating new <see cref="Material"/> with overrided <see cref="Texture2D"/> from <see cref="Sprite"/>
    /// </summary>
    [Serializable]
    public struct RegisterSpriteAuthoringModule
    {
        [SerializeField] public Sprite Sprite;
        [SerializeField] public SpriteRenderData SpriteRenderData;
        [SerializeField] public bool PreventOverrideTextureFromSprite;

        private static readonly Dictionary<Texture, Material> OverridedMaterials = new();
        private static readonly int MainTexPropertyID = Shader.PropertyToID("_MainTex");
        
        private SpriteRenderData HandledSpriteRenderData
        {
            get
            {
                if (SpriteRenderData.Material == null)
                {
                    Debug.LogException(new NSpritesException($"{nameof(SpriteRenderData.Material)} is null"));
                    return default;
                }

                if (!PreventOverrideTextureFromSprite && Sprite != null)
                    // create new instance with overrided material
                    return new()
                    {
                        Material = GetOrCreateOverridedMaterial(Sprite.texture),
                        PropertiesSet = SpriteRenderData.PropertiesSet
                    };
                return SpriteRenderData;
            }
        }
        
        public bool IsValid(out string message)
        {
            if (Sprite == null)
            {
                message = $"{nameof(Sprite)} is null";
                return false;
            }

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

        public void Bake<TAuthoring>(Baker<TAuthoring> baker)
            where TAuthoring : Component =>
            baker.BakeSpriteBase(HandledSpriteRenderData);

        private Material GetOrCreateOverridedMaterial(Texture texture)
        {
            if (!OverridedMaterials.TryGetValue(texture, out var material))
                material = CreateOverridedMaterial(texture);
#if UNITY_EDITOR //for SubScene + domain reload
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
            material.SetTexture(MainTexPropertyID, Sprite.texture);
            OverridedMaterials.Add(texture, material);
            return material;
        }
    }
}