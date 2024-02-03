using Unity.Entities;
using Unity.Mathematics;

namespace NSprites
{
    public struct SpriteAnimationBlobData
    {
        public int ID;
        public float4 UVAtlas;
        public int2 GridSize;
        public int2 FrameRange;
        public BlobArray<float> FrameDurations;
        public float AnimationDuration;

        public int FrameOffset => FrameRange.x;
        public int FrameCount => FrameRange.y;
    }
}