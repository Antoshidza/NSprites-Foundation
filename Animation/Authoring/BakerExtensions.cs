using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace NSprites.Authoring
{
    public static partial class BakerExtensions
    {
        public static void BakeAnimation<T>(this Baker<T> baker, in Entity entity, SpriteAnimationSet animationSet, int initialAnimationIndex = 0)
            where T : Component
        {
            if(baker == null)
            {
                Debug.LogError(new NSpritesException("Passed Baker is null"));
                return;
            }
            if (animationSet == null)
            {
                Debug.LogError(new NSpritesException("Passed AnimationSet is null"));
                return;
            }

            baker.DependsOn(animationSet);

            if (animationSet == null)
                return;

            if (initialAnimationIndex >= animationSet.Animations.Count || initialAnimationIndex < 0)
            {
                Debug.LogError(new NSpritesException($"Initial animation index {initialAnimationIndex} can't be less than 0 or great/equal to animation count {animationSet.Animations.Count}"));
                return;
            }
            
            #region create animation blob asset
            var blobBuilder = new BlobBuilder(Allocator.Temp); //can't use `using` keyword because there is extension which use this + ref
            ref var root = ref blobBuilder.ConstructRoot<BlobArray<SpriteAnimationBlobData>>();
            var animations = animationSet.Animations;
            var animationArray = blobBuilder.Allocate(ref root, animations.Count);

            var animIndex = 0;
            foreach (var anim in animations)
            {
                var animData = anim.data;
                var animationDuration = 0f;
                for (int i = 0; i < animData.FrameDurations.Length; i++)
                    animationDuration += animData.FrameDurations[i];

                animationArray[animIndex] = new SpriteAnimationBlobData
                {
                    ID = Animator.StringToHash(anim.name),
                    GridSize = animData.FrameCount,
                    UVAtlas = NSpritesUtils.GetTextureST(animData.SpriteSheet),
                    Scale2D = new float2(animData.SpriteSheet.bounds.size.x, animData.SpriteSheet.bounds.size.y),
                    AnimationDuration = animationDuration
                    // FrameDuration - allocate lately
                };

                var durations = blobBuilder.Allocate(ref animationArray[animIndex].FrameDurations, animData.FrameDurations.Length);
                for (int di = 0; di < durations.Length; di++)
                    durations[di] = animData.FrameDurations[di];

                animIndex++;
            }

            var blobAssetReference = blobBuilder.CreateBlobAssetReference<BlobArray<SpriteAnimationBlobData>>(Allocator.Persistent);
            baker.AddBlobAsset(ref blobAssetReference, out _);
            blobBuilder.Dispose();
            #endregion

            ref var initialAnim = ref blobAssetReference.Value[initialAnimationIndex];

            baker.AddComponent(entity, new AnimationSetLink { value = blobAssetReference });
            baker.AddComponent(entity, new AnimationIndex { value = initialAnimationIndex });
            baker.AddComponent(entity, new AnimationTimer { value = initialAnim.FrameDurations[0] });
            baker.AddComponent<FrameIndex>(entity);
            
            baker.AddComponent(entity, new MainTexSTInitial { value = initialAnim.UVAtlas });
        }
    }
}