using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace NSprites
{
    /// <summary>
    /// Advanced <see cref="SpriteRendererAuthoring"/> which also bakes animation data as blob asset and adds animation components.
    /// </summary>
    public class SpriteAnimationAuthoring : SpriteRendererAuthoring
    {
        private class Baker : Baker<SpriteAnimationAuthoring>
        {
            public override void Bake(SpriteAnimationAuthoring authoring)
            {
                if(!authoring.IsValid)
                    return;
                
                var initialAnimData = authoring.AnimationAuthoringModule.InitialAnimationData;
                var initialSheetUVAtlas = (float4)NSpritesUtils.GetTextureST(initialAnimData.SpriteSheet);
                var initialFrameUVAtlas = new float4(new float2(initialSheetUVAtlas.xy / initialAnimData.FrameCount), initialSheetUVAtlas.zw);
                
                authoring.RegisterSpriteData.Bake(this, authoring.OverrideTextureFromSprite ? initialAnimData.SpriteSheet.texture : null);
                authoring.AnimationAuthoringModule.Bake(this);

                authoring.RenderSettings.Bake(this, authoring, authoring.NativeSpriteSize, initialFrameUVAtlas);
                authoring.Sorting.Bake(this);
            }
        }
        
        [Space]
        [SerializeField] public AnimationAuthoringModule AnimationAuthoringModule;
        
        protected float2 FrameSize
        {
            get
            {
                var animationData = AnimationAuthoringModule.InitialAnimationData;
                return animationData.SpriteSheet.GetSize() / animationData.FrameCount;
            }
        }

        public override float2 NativeSpriteSize => FrameSize;

        protected override bool IsValid 
            => base.IsValid && AnimationAuthoringModule.IsValid();

        [ContextMenu("Set Native Size")]
        public void SetNativeSize() => RenderSettings.Size = FrameSize;
    }
}