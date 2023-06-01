using Unity.Mathematics;

namespace NSprites
{
    public readonly struct Bounds2D
    {
        private readonly float2 _position;
        private readonly float2 _extents;

        public float2 Min => _position - _extents;
        public float2 Max => _position + _extents;
        public float2 Size => _extents * 2f;

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

        private static bool Equals(Bounds2D lhs, Bounds2D rhs)
        {
            return math.all(lhs._position == rhs._position)
                   && math.all(lhs._extents == rhs._extents);
        }

        public static bool operator ==(Bounds2D lhs, Bounds2D rhs)
        {
            return Equals(lhs, rhs);
        }
        
        public static bool operator !=(Bounds2D lhs, Bounds2D rhs)
        {
            return !Equals(lhs, rhs);
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
        
        public static Bounds2D From(in LocalToWorld2D worldPosition, in Scale2D size, in Pivot pivot)
        {
            var  rotation = MathHelper.eulerZ(worldPosition.Rotation);
            var scale    = worldPosition.Scale * size.value;
            var position = worldPosition.Position - scale * pivot.value + scale * .5f;

            var sin = -math.sin(rotation);
            var cos = -math.cos(rotation);

            var adjustedScale = new float2(
                x: scale.x * cos + scale.y * sin,
                y: scale.x * sin + scale.y * cos
            );

            return new Bounds2D(position, adjustedScale);
        }
    }
}