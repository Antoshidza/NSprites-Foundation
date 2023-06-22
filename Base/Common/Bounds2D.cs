using System;
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
        
        public bool Equals(Bounds2D other)
        {
            return _position.Equals(other._position) && _extents.Equals(other._extents);
        }

        public override bool Equals(object obj)
        {
            return obj is Bounds2D other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_position, _extents);
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
        
        public static Bounds2D From(in LocalToWorld2D ltw, in Scale2D size, in Pivot pivot)
        {
            var rotation = MathHelper.euler(ltw.Rotation).z;
            var scale = ltw.Scale * size.value;
            var localCenter= -scale * pivot.value + scale * .5f;

            var sin = math.sin(rotation);
            var cos = math.cos(rotation);

            static float2 RotateScale2D(in float sin, in float cos, in float2 v)
            {
                var abssin = math.abs(sin);
                var abscos = math.abs(cos);
                return new float2(v.x * abscos + v.y * abssin, v.x * abssin + v.y * abscos);
            }
            static float2 RotateFloat2(in float sin, in float cos, in float2 v)
                => new(v.x * cos - v.y * sin, v.x * sin + v.y * cos);

            var adjustedScale = RotateScale2D(sin, cos, scale);
            var position = ltw.Position + RotateFloat2(sin, cos, localCenter);
    
            return new Bounds2D(position, adjustedScale);
        }
    }
}