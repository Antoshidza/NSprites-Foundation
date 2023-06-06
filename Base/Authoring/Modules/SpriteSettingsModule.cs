using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace NSprites.Authoring
{
    [Serializable]
    public class SpriteSettingsModule
    {
        public enum DrawModeType
        {
            /// <summary> Sprite will be simply stretched to it's size. </summary>
            Simple,
            /// <summary> Sprite will be tiled depending in it's size and native size (default sprite size). </summary>
            Tiled
        }
        
        public float2 Pivot = new(.5f);
        public float2 Size = new(1f);
        [Tooltip("Prevents changing Size when Sprite changed")] public bool LockSize;
        public DrawModeType DrawMode;
        public float4 TilingAndOffset = new(1f, 1f, 0f, 0f);
        public bool2 Flip;

        /// <summary>
        /// Bakes sprite default (for NSprites-Foundation package) such as
        /// <list type="bullet">
        /// <item><see cref="UVAtlas"/> and <see cref="UVTilingAndOffset"/></item>
        /// <item><see cref="Scale2D"/></item>
        /// <item><see cref="Pivot"/></item>
        /// <item><see cref="Flip"/></item>
        /// </list>
        /// </summary>
        /// <param name="baker">baker bruh</param>
        /// <param name="authoring">authoring monobehaviour</param>
        /// <param name="nativeSize">The native size of a sprite being baked. Needs because sprite and it's params can come from arbitrary source, so need to be passed</param>
        /// <param name="uvAtlas">The same as <see cref="nativeSize"/> should be passed, because of external sprite</param>
        public void Bake<TAuthoring>(Baker<TAuthoring> baker, TAuthoring authoring, in float2 nativeSize, in float4 uvAtlas)
            where TAuthoring : Component
        {
            var authoringTransform = authoring.transform;
            var authoringScale = authoringTransform.lossyScale;
            
            baker.BakeSpriteRender
            (
                baker.GetEntity(TransformUsageFlags.None),
                authoring,
                uvAtlas,
                GetTilingAndOffsetByDrawMode(),
                Pivot,
                Size * nativeSize * new float2(authoringScale.x, authoringScale.y),
                flipX: Flip.x,
                flipY: Flip.y
            );
        }

        public void TrySetSize(in float2 value)
        {
            if (LockSize)
                Debug.LogWarning($"{nameof(SpriteSettingsModule)}: can't change size because {nameof(LockSize)} enabled");
            else
                Size = value;
        }

        /// <summary>
        /// Returns UV Tiling & Offset accounting selected <see cref="DrawMode"/>.
        /// </summary>
        public float4 GetTilingAndOffsetByDrawMode()
        {
            return DrawMode switch
            {
                // just return default user defined Tiling&Offset from inspector
                DrawModeType.Simple => TilingAndOffset,
                // while size of a sprite can be different it's UVs stay the same - in range [(0,0) ; (1,1)]
                // so in this case we want to get ratio of size to sprite NativeSize (which should be "default" sprite size depending on it's import data) and then correct Tiling part in that ratio
                DrawModeType.Tiled => new(TilingAndOffset.xy * Size, TilingAndOffset.zw),
                
                _ => throw new ArgumentOutOfRangeException($"{GetType().Name}.{nameof(UVTilingAndOffset)} ({nameof(SpriteRendererAuthoring)}): can't handle draw mode {DrawMode}")
            };
        }
    }
}