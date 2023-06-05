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

        // TODO: utilize native size to use as default size multiplied by scale and size
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
                GetTilingAndOffsetByDrawMode(nativeSize),
                Pivot,
                Size * new float2(authoringScale.x, authoringScale.y),
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
        /// If <see cref="DrawModeType.Tiled"/> selected then it requires additional calculations and Native Size of sprite passed in.
        /// </summary>
        public float4 GetTilingAndOffsetByDrawMode(float2 nativeSpriteSize)
        {
            return DrawMode switch
            {
                // just return default user defined Tiling&Offset from inspector
                DrawModeType.Simple => TilingAndOffset,
                // while _size of sprite can be different it's UVs stay the same - in range [(0,0) ; (1,1)]
                // so in this case we want to get ration of size to sprite NativeSize (which should be "default" sprite size depending on it's import data) and then correct Tiling part in that ratio
                DrawModeType.Tiled => new(TilingAndOffset.xy * Size / nativeSpriteSize, TilingAndOffset.zw),
                
                _ => throw new ArgumentOutOfRangeException($"{GetType().Name}.{nameof(UVTilingAndOffset)} ({nameof(SpriteRendererAuthoring)}): can't handle draw mode {DrawMode}")
            };
        }
    }
}