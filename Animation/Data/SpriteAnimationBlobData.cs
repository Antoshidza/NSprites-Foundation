using Unity.Entities;
using Unity.Mathematics;

namespace NSprites
{
    public struct SpriteAnimationBlobData
    {
        public int ID;
        public float2 Scale2D; // TODO: remove or use
        public float4 UVAtlas;
        public int2 GridSize;
        public BlobArray<float> FrameDurations;
        public float AnimationDuration;
    }
}