using Unity.Entities;

namespace NSprites
{
    public readonly partial struct AnimatorAspect : IAspect
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private readonly Entity _entity;
#endif
        private readonly RefRW<AnimationIndex> _animationIndex;
        private readonly RefRW<AnimationTimer> _animationTimer;
        private readonly RefRW<FrameIndex> _frameIndex;
        private readonly RefRO<AnimationSetLink> _animationSetLink;

        public void SetAnimation(int toAnimationIndex, in double worldTime)
        {
            // find animation by animation ID
            ref var animSet = ref _animationSetLink.ValueRO.value.Value;
            var setToAnimIndex = -1;
            for (int i = 0; i < animSet.Length; i++)
                if (animSet[i].ID == toAnimationIndex)
                {
                    setToAnimIndex = i;
                    break;
                }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (setToAnimIndex == -1)
                throw new NSpritesException($"{nameof(AnimatorAspect)}.{nameof(SetAnimation)}: incorrect {nameof(toAnimationIndex)} was passed. {_entity} has no animation with such ID ({toAnimationIndex}) was found");
#endif

            if (_animationIndex.ValueRO.value != setToAnimIndex)
            {
                ref var animData = ref animSet[setToAnimIndex];
                _animationIndex.ValueRW.value = setToAnimIndex;
                // here we want to set last frame and timer to 0 (equal to current time) to force animation system instantly switch
                // animation to 1st frame after we've modified it
                _frameIndex.ValueRW.value = animData.FrameDurations.Length - 1;
                _animationTimer.ValueRW.value = worldTime;
            }
        }

        public void SetToFrame(int frameIndex, in double worldTime)
        {
            ref var animData = ref _animationSetLink.ValueRO.value.Value[_animationIndex.ValueRO.value];
            _frameIndex.ValueRW.value = frameIndex;
            _animationTimer.ValueRW.value = worldTime + animData.FrameDurations[frameIndex];
        }

        public void ResetAnimation(in double worldTime) =>
            SetToFrame(0, worldTime);
    }
}