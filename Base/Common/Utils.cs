using Unity.Mathematics;
using UnityEngine;

namespace NSprites
{
    public static class Utils
    {
        public static float2 GetNativeSize(this Sprite source) => new float2(source.texture.width, source.texture.height) / source.pixelsPerUnit;
        public static float2 GetNativeSize(this Sprite source, in float2 uvAtlas) => source.GetNativeSize() * uvAtlas;
        public static float2 GetNativeSizeWithUVAtlas(this Sprite source) => source.GetNativeSize(((float4)NSpritesUtils.GetTextureST(source)).xy);
    }
}