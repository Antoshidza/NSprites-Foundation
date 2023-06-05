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
                authoring.RenderSettings.Bake(this, authoring, authoring.NativeSpriteSize, NSpritesUtils.GetTextureST(authoring.Sprite));
                authoring.Sorting.Bake(this);
            }
        }

        [SerializeField] protected Sprite Sprite;
        [SerializeField] protected RegisterSpriteAuthoringModule RegisterSpriteData;
        [SerializeField] protected bool OverrideTextureFromSprite = true;
        
        [SerializeField] protected SpriteSettingsModule RenderSettings;
        [SerializeField] protected SortingAuthoringModule Sorting;

        public float2 ScaledSize => RenderSettings.Size * new float2(transform.lossyScale.x, transform.lossyScale.y);

        /// <summary> "Default" sprite size which would be a size of same sprite being placed on scene as a unity's built-in SpriteRenderer. </summary>
        public virtual float2 NativeSpriteSize => Sprite.GetSize();

        [ContextMenu("Set Native Size")]
        private void SetNativeSize() => RenderSettings.TrySetSize(NativeSpriteSize);
        
        internal void OnSpriteChanged() => SetNativeSize();

        protected virtual bool IsValid
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
