using System;
using System.Collections.Generic;
using UnityEngine;

namespace NSprites
{
    [CreateAssetMenu(fileName = "NewNSpriteAnimationSet", menuName = "NSprites/Animation Set")]
    public class SpriteAnimationSet : ScriptableObject
    {
        [Serializable]
        public struct NamedAnimation
        {
            public string name;
            public SpriteAnimation data;
        }

        [SerializeField] private NamedAnimation[] _animations;

        public IReadOnlyCollection<NamedAnimation> Animations => _animations;
        
        // returns true if all animation's sprites have the same texture as passed in
        public bool IsValid(Texture2D mainTexture)
        {
            var passed = true;
            
            foreach (var animation in _animations)
            {
                var texture = animation.data.SpriteSheet.texture;

                if (texture == null)
                {
                    Debug.LogException(new NSpritesException($"{nameof(SpriteAnimationSet)} {name}: all {nameof(_animations)}'s {nameof(SpriteAnimation.SpriteSheet)} must be not null"), this);
                    passed = false;
                }
                else if (animation.data.SpriteSheet.texture != mainTexture)
                {
                    Debug.LogException(new NSpritesException($"{nameof(SpriteAnimationSet)} {name}: all {nameof(_animations)}'s {nameof(SpriteAnimation.SpriteSheet)} must have same texture as passed \"{mainTexture.name}\", but animation \"{animation.name}\" have {nameof(SpriteAnimation.SpriteSheet)}'s texture \"{animation.data.SpriteSheet.name}\""), this);
                    passed = false;
                }
            }

            return passed;
        }
    }
}