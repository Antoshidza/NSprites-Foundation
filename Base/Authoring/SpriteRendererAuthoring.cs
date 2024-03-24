using NSprites.Authoring;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace NSprites
{
    /// <summary>
    /// Adds basic render components such as <see cref="UVAtlas"/>, <see cref="UVTilingAndOffset"/>, <see cref="Scale2D"/>, <see cref="Pivot"/>.
    /// Optionally adds sorting components, removes built-in 3D transforms and adds 2D transforms.
    /// </summary>
    public class SpriteRendererAuthoring : MonoBehaviour
    {
        private class Baker : Baker<SpriteRendererAuthoring>
        {
            public override void Bake(SpriteRendererAuthoring authoring)
            {
                if (!authoring.IsValid)
                    return;

                DependsOn(authoring);

                authoring.RegisterSpriteData.Bake(this, authoring.OverrideTextureFromSprite ? authoring.Sprite.texture : null);
                var uvAtlas = (float4)NSpritesUtils.GetTextureST(authoring.Sprite);
                authoring.RenderSettings.Bake(this, authoring, authoring.Sprite.GetNativeSize(uvAtlas.xy), uvAtlas);
                authoring.Sorting.Bake(this);
            }
        }

        [SerializeField] public Sprite Sprite;
        [SerializeField] public RegisterSpriteAuthoringModule RegisterSpriteData;
        [SerializeField] public bool OverrideTextureFromSprite = true;
        
        [SerializeField] public SpriteSettingsAuthoringModule RenderSettings;
        [SerializeField] public SortingAuthoringModule Sorting;

        private bool IsValid
        {
            get
            {
                if (!RegisterSpriteData.IsValid(out var message))
                {
                    Debug.LogWarning(new NSpritesException(message), gameObject);
                    return false;
                }
                
                // Settings just have struct values and there is nothing to validate

                if (Sprite == null)
                {
                    Debug.LogWarning(new NSpritesException($"{GetType().Name}: {nameof(Sprite)} is null"), gameObject);
                    return false;
                }

                return true;
            }
        }
    }
}
