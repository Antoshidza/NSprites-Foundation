using Unity.Mathematics;
using UnityEngine;

namespace NSprites
{
    public static class Utils
    {
        public static float2 GetSize(this Sprite sprite) => new(sprite.bounds.size.x, sprite.bounds.size.y);
    }
}