using NSprites.Authoring;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace NSprites
{
    /// <summary>
    /// Advanced <see cref="SpriteRendererAuthoring"/> which also bakes animation data as blob asset and adds animation components.
    /// </summary>
    public class SpriteAnimationAuthoring : MonoBehaviour
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
                var frameSize = initialAnimData.SpriteSheet.GetSize() / initialAnimData.FrameCount;
                
                authoring.RegisterSpriteData.Bake(this, initialAnimData.SpriteSheet.texture);
                authoring.AnimationAuthoringModule.Bake(this);
                authoring.RenderSettings.Bake(this, authoring, frameSize, initialFrameUVAtlas);
                authoring.Sorting.Bake(this);
            }
        }
        
        [SerializeField] protected AnimationAuthoringModule AnimationAuthoringModule;
        [SerializeField] protected RegisterSpriteAuthoringModule RegisterSpriteData;
        [SerializeField] protected SpriteSettingsModule RenderSettings;
        [SerializeField] protected SortingAuthoringModule Sorting;

        private float2 FrameSize
        {
            get
            {
                var animationData = AnimationAuthoringModule.InitialAnimationData;
                return animationData.SpriteSheet.GetSize() / animationData.FrameCount;
            }
        }

        public float2 ScaledSize => RenderSettings.Size * FrameSize * new float2(transform.lossyScale.x, transform.lossyScale.y);

        protected virtual bool IsValid 
            => AnimationAuthoringModule.IsValid();
    }
}