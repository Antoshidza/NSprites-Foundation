using Unity.Mathematics;

namespace NSprites
{
    public readonly struct Bounds2D
    {
        private readonly float2 _position;
        private readonly float2 _extents;

        private float2 Min => _position - _extents;
        private float2 Max => _position + _extents;

        public Bounds2D(in float2 position, in float2 size)
        {
            _position = position;
            _extents = size / 2f;
        }

        public Bounds2D(in float2x2 rect)
        {
            _position = math.lerp(rect.c0, rect.c1, .5f);
            _extents = math.abs(rect.c1 - rect.c0) / 2f;
        }

        public bool Intersects(in Bounds2D bounds)
        {
            var max = Max;
            var min = Min;
            var anotherMax = bounds.Max;
            var anotherMin = bounds.Min;

            return min.x <= anotherMax.x && max.x >= anotherMin.x &&
                   min.y <= anotherMax.y && max.y >= anotherMin.y;
        }
    }
}