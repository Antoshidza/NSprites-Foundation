using System;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace NSprites
{
    [Serializable]
    public struct AnimationAuthoringModule
    {
        [SerializeField] public SpriteAnimationSet AnimationSet;
        [SerializeField] public int InitialAnimationIndex;

        public SpriteAnimation InitialAnimationData => AnimationSet.Animations.ElementAt(InitialAnimationIndex).data; 

        public bool IsValid(Texture2D texture)
        {
            if (AnimationSet == null)
            {
                Debug.LogWarning(new NSpritesException($"{nameof(AnimationSet)} is null"));
                return false;
            }

            if (InitialAnimationIndex >= AnimationSet.Animations.Count)
            {
                Debug.LogWarning(new NSpritesException($"{nameof(InitialAnimationIndex)} can't be greater than animations count. {nameof(InitialAnimationIndex)}: {InitialAnimationIndex}, animation count: {AnimationSet.Animations.Count}"));
                return false;
            }

            if (InitialAnimationIndex < 0)
            {
                Debug.LogWarning(new NSpritesException($"{nameof(InitialAnimationIndex)} can't be lower 0. Currently it is {InitialAnimationIndex}"));
                return false;
            }

            return AnimationSet.IsValid(texture);
        }
        
        public void Bake<TAuthoring>(Baker<TAuthoring> baker, in Entity entity)
            where TAuthoring : MonoBehaviour
            => baker.BakeAnimation(entity, AnimationSet, InitialAnimationIndex);
    }
}